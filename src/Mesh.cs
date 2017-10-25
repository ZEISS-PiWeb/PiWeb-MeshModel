#region Copyright
/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss IMT (IZfM Dresden)                   */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2010                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */
#endregion

namespace Zeiss.IMT.PiWeb.MeshModel
{
	#region using

	using System;
	using System.IO;
	using System.Linq;

	#endregion

	/// <summary>
	/// Describes a triangulated mesh.
	/// </summary>
	public sealed class Mesh
	{
		#region constants

		private static readonly Version FileVersion10 = new Version( 1, 0, 0, 0 );
		private static readonly Version FileVersion21 = new Version( 2, 1 );
		private static readonly Version FileVersion22 = new Version( 2, 2 );
		private static readonly Version FileVersion33 = new Version( 3, 3 );

		private static readonly float[] EmptyFloatArray = new float[0];
		private static readonly MeshIndices EmptyMeshIndices = MeshIndices.Create( new int[0] );

		#endregion

		#region members

		private readonly MeshIndices _TriangleIndices;
		private Rect3F? _Bounds;

		#endregion

		#region constructor


		/// <summary>
		/// Initializes a new instance of the <see cref="Mesh"/> class.
		/// </summary>
		/// <param name="index">The index of the mesh in the <see cref="MeshModelPart"/>..</param>
		/// <param name="positions">The point data array.</param>
		/// <param name="normals">The normal data array.</param>
		/// <param name="triangleIndices">The index array.</param>
		/// <param name="textureCoordinates">The texture coordinate array.</param>
		/// <param name="color">The default color.</param>
		/// <param name="colors">The color data array.</param>
		/// <param name="layer">The layers to which this mesh belongs.</param>
		/// <param name="name">The name.</param>
		public Mesh( int index, float[] positions, float[] normals, int[] triangleIndices, float[] textureCoordinates = null, Color? color = null, float[] colors = null, string[] layer = null, string name = null )
			: this( index, positions, normals, MeshIndices.Create( triangleIndices ), textureCoordinates, color, colors, layer, name )
		{ }
		
		private Mesh( int index, float[] positions, float[] normals, MeshIndices triangleIndices, float[] textureCoordinates = null, Color? color = null, float[] colors = null, string[] layer = null, string name = null )
		{
			Index = index;
			Positions = positions ?? EmptyFloatArray;
			_TriangleIndices = triangleIndices ?? EmptyMeshIndices;

			Normals = normals ?? EmptyFloatArray;
			TextureCoordinates = textureCoordinates ?? EmptyFloatArray;

			Normals = ValidateAndRegenerateNormals( Positions, Normals, _TriangleIndices.GetIndizes().ToArray() );
			
			Colors = colors;
			Color = color;
			Layer = layer;
			Name = name;
		}

		#endregion

		#region properties

		/// <summary>
		/// Gets the name of the triangle mesh.
		/// </summary>
		public string Name { get; }


		/// <summary>
		/// Gets a value indicating whether this instance is empty (Has no points).
		/// </summary>
		public bool IsEmpty => _TriangleIndices.Count == 0;


		/// <summary>
		/// Gets the index of the mesh in the <see cref="MeshModelPart"/>.
		/// </summary>
		public int Index { get; }

		/// <summary>
		/// Gets the normal data array.
		/// </summary>
		public float[] Normals { get; }

		/// <summary>
		///Gets the position data array.
		/// </summary>
		public float[] Positions { get; }

		/// <summary>
		/// Gets the texture data array.
		/// </summary>
		public float[] TextureCoordinates { get; }

		/// <summary>
		/// Gets the number of triangle indices
		/// </summary>
		public int TriangleIndicesCount => _TriangleIndices?.Count ?? 0;

		/// <summary>
		/// Gets the default color of the mesh.
		/// </summary>
		public Color? Color { get; }

		/// <summary>
		/// Gets the color data array.
		/// </summary>
		public float[] Colors { get; }

		/// <summary>
		/// Gets the layers to which this mesh belongs
		/// </summary>
		public string[] Layer { get; }

		/// <summary>
		/// Gets the bounding box of this triangle mesh.
		/// </summary>
		public Rect3F Bounds
		{
			get
			{
				if( _Bounds.HasValue ) return _Bounds.Value;

				var bounds = Rect3F.Empty;
				for( var i = 0; i < Positions.Length; i += 3 )
				{
					bounds.Union( new Point3F( Positions[ i ], Positions[ i + 1 ], Positions[ i + 2 ] ) );
				}
				_Bounds = bounds;

				return _Bounds.Value;
			}
		}

		#endregion

		#region methods

		/// <summary>
		/// Gibt die Dreiecksindizes des CAD-Modells zurück.
		/// </summary>
		public int[] GetTriangleIndices()
		{
			return _TriangleIndices?.GetIndizes() ?? new int[0];
		}

		/// <summary>
		/// Gets the index of the triangle.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <returns></returns>
		public int GetTriangleIndex( int index )
		{
			return _TriangleIndices[ index ];
		}

		/// <summary>
		/// Creates a mesh based on the current mesh, but with the specified default color.
		/// </summary>
		/// <param name="color">The default color.</param>
		/// <returns></returns>
		public Mesh MeshWithColor( Color color )
		{
			return new Mesh( Index, Positions, Normals, _TriangleIndices, TextureCoordinates, color, Colors, Layer, Name );
		}

		/// <summary>
		/// Creates a mesh based on the current mesh, but with a default color.
		/// </summary>
		/// <returns></returns>
		public Mesh MeshWithoutColor()
		{
			return new Mesh( Index, Positions, Normals, _TriangleIndices, TextureCoordinates, null, Colors, Layer, Name );
		}

		/// <summary>
		/// Creates a mesh based on the current mesh, but without color data.
		/// </summary>
		/// <returns></returns>
		public Mesh MeshWithoutColors()
		{
			return new Mesh( Index, Positions, Normals, _TriangleIndices, TextureCoordinates, Color, null, Layer, Name );
		}

		/// <summary>
		/// Generates the normals from the given positions and triangle information if needed.
		/// </summary>
		private static float[] ValidateAndRegenerateNormals( float[] positions, float[] normals, int[] indices )
		{
			if( normals.Length == positions.Length )
				return normals;

			if( positions.Length % 3 != 0 )
				throw new ArgumentException( "The length of the points array has to be a multiple of 3." );

			var epsilon = (float)Math.Sqrt( 1.0000000116861E-07 );

			normals = new float[ positions.Length ];
			for( var i = 0; i < indices.Length; i += 3 )
			{
				var i0 = indices[ i ];
				var i1 = indices[ i + 1 ];
				var i2 = indices[ i + 2 ];

				var p0 = new Point3F( positions[ i0 * 3 ], positions[ i0 * 3 + 1 ], positions[ i0 * 3 + 2 ] );
				var p1 = new Point3F( positions[ i1 * 3 ], positions[ i1 * 3 + 1 ], positions[ i1 * 3 + 2 ] );
				var p2 = new Point3F( positions[ i2 * 3 ], positions[ i2 * 3 + 1 ], positions[ i2 * 3 + 2 ] );

				var direction = Vector3F.CrossProduct( p1 - p0, p2 - p1 );
				var norm2 = direction.Length;

				if( norm2 > epsilon )
				{
					direction /= norm2;

					normals[ i0 * 3 + 0 ] += direction.X;
					normals[ i0 * 3 + 1 ] += direction.Y;
					normals[ i0 * 3 + 2 ] += direction.Z;

					normals[ i1 * 3 + 0 ] += direction.X;
					normals[ i1 * 3 + 1 ] += direction.Y;
					normals[ i1 * 3 + 2 ] += direction.Z;

					normals[ i2 * 3 + 0 ] += direction.X;
					normals[ i2 * 3 + 1 ] += direction.Y;
					normals[ i2 * 3 + 2 ] += direction.Z;
				}
			}

			for( var i = 0; i < normals.Length; i += 3 )
			{
				var norm2 = new Vector3F( normals[ i ], normals[ i + 1 ], normals[ i + 2 ] ).Length;
				if( norm2 > epsilon )
				{
					normals[ i ] /= norm2;
					normals[ i + 1 ] /= norm2;
					normals[ i + 2 ] /= norm2;
				}
			}
			return normals;
		}

		internal static Mesh Read( BinaryReader binaryReader, int index, Version fileVersion )
		{
			float[] positions = null;
			float[] normals = null;
			float[] textureCoordinates = null;
			float[] colors = null;

			MeshIndices indices = null;
			Color? color = null;
			string[] layer = null;
			var name = string.Empty;

			if( fileVersion >= FileVersion21 )
				name = binaryReader.ReadString();

			if( binaryReader.ReadBoolean() )
				color = binaryReader.ReadArgbColor();

			if( binaryReader.ReadBoolean() )
			{
				positions = fileVersion == FileVersion10
					? binaryReader.ReadDoubleArray( 3 )
					: binaryReader.ReadFloatArray( 3 );
			}

			if( binaryReader.ReadBoolean() )
			{
				normals = fileVersion == FileVersion10
					? binaryReader.ReadDoubleArray( 3 )
					: binaryReader.ReadFloatArray( 3 );
			}

			if( binaryReader.ReadBoolean() )
				indices = binaryReader.ReadIndices( positions?.Length ?? 0 );

			if( binaryReader.ReadBoolean() )
			{
				layer = new string[ binaryReader.ReadInt32()];
				for( var l = 0; l < layer.Length; l++ )
				{
					layer[ l ] = binaryReader.ReadString();
				}
			}
			if( fileVersion >= FileVersion22 && binaryReader.ReadBoolean() )
				textureCoordinates = binaryReader.ReadFloatArray( 2 );

			if( fileVersion >= FileVersion33 && binaryReader.ReadBoolean() )
				colors = binaryReader.ReadFloatArray( 4 );

			return new Mesh( index, positions, normals, indices, textureCoordinates, color, colors, layer, name );
		}

		internal void Write( BinaryWriter binaryWriter )
		{
			binaryWriter.Write( Name ?? "" );
			if( Color.HasValue )
			{
				binaryWriter.Write( true );
				binaryWriter.WriteArgbColor( Color.Value );
			}
			else
			{
				binaryWriter.Write( false );
			}

			if (Positions != null && Positions.Length > 0)
			{
				binaryWriter.Write(true);
				binaryWriter.WriteFloatArray(Positions, 3);
			}
			else
			{
				binaryWriter.Write(false);
			}

			if (Normals != null && Normals.Length > 0)
			{
				binaryWriter.Write(true);
				binaryWriter.WriteFloatArray(Normals, 3);
			}
			else
			{
				binaryWriter.Write(false);
			}

			if ( TriangleIndicesCount > 0 )
			{
				binaryWriter.Write( true );
				binaryWriter.Write( TriangleIndicesCount );
				foreach( var j in GetTriangleIndices() )
				{
					binaryWriter.Write( j );
				}
			}
			else
			{
				binaryWriter.Write( false );
			}
			if( Layer != null && Layer.Length > 0 )
			{
				binaryWriter.Write( true );
				binaryWriter.Write( Layer.Length );
				foreach( var l in Layer )
				{
					binaryWriter.Write( l );
				}
			}
			else
			{
				binaryWriter.Write( false );
			}
			
			if (TextureCoordinates != null && TextureCoordinates.Length > 0)
			{
				binaryWriter.Write(true);
				binaryWriter.WriteFloatArray(TextureCoordinates, 2);
			}
			else
			{
				binaryWriter.Write(false);
			}

			if (Colors != null && Colors.Length > 0)
			{
				binaryWriter.Write(true);
				binaryWriter.WriteFloatArray(Colors, 4);
			}
			else
			{
				binaryWriter.Write(false);
			}
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		public override string ToString()
		{
			return $"Mesh [{Name}] ({Positions.Length} points, {TriangleIndicesCount} triangle indices)";
		}

		#endregion

		#region class MeshIndices

		internal abstract class MeshIndices
		{
			internal MeshIndices( int count )
			{
				Count = count;
			}

			public int Count { get; }

			public static MeshIndices Create( int[] indices )
			{
				if( indices.Length == 0 )
					return new ByteMeshIndices( new byte[] { } );

				var maxValue = indices.Max();
				if( maxValue <= byte.MaxValue )
					return new ByteMeshIndices( indices.Select( i => ( byte ) i ).ToArray() );
				
				if (maxValue <= short.MaxValue )
					return new ShortMeshIndices( indices.Select( i => ( short ) i ).ToArray() );

				return new IntegerMeshIndices( indices );
			}

			public abstract int this[ int index ] { get; }

			public abstract int[] GetIndizes();
		}

		internal class ByteMeshIndices : MeshIndices
		{
			private readonly byte[] _Indices;

			internal ByteMeshIndices( byte[] indices )
				: base( indices.Length )
			{
				_Indices = indices;
			}

			public override int this[ int index ] => _Indices[ index ];

			public override int[] GetIndizes()
			{
				return _Indices.Select( i => (int)i ).ToArray();
			}
		}

		internal class ShortMeshIndices : MeshIndices
		{
			private readonly short[] _Indices;

			internal ShortMeshIndices( short[] indices )
				: base( indices.Length )
			{
				_Indices = indices;
			}

			public override int this[ int index ] => _Indices[ index ];

			public override int[] GetIndizes()
			{
				return _Indices.Select( i => (int)i ).ToArray();
			}
		}

		internal class IntegerMeshIndices : MeshIndices
		{
			private readonly int[] _Indices;

			internal IntegerMeshIndices( int[] indices )
				: base( indices.Length )
			{
				_Indices = indices;
			}

			public override int this[ int index ] => _Indices[ index ];

			public override int[] GetIndizes()
			{
				return _Indices;
			}
		}

		#endregion
	}
}