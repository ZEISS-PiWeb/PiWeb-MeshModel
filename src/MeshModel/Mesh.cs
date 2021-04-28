#region copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss Industrielle Messtechnik GmbH        */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2021                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

namespace Zeiss.PiWeb.MeshModel
{
	#region usings

	using System;
	using System.IO;
	using System.Linq;

	#endregion

	/// <summary>
	/// Describes a triangulated mesh.
	/// </summary>
	public sealed class Mesh : IMesh
	{
		#region members

		private static readonly Version FileVersion21 = new Version( 2, 1 );
		private static readonly Version FileVersion22 = new Version( 2, 2 );
		private static readonly Version FileVersion33 = new Version( 3, 3 );

		private static readonly MeshIndices EmptyMeshIndices = MeshIndices.Create( Array.Empty<int>() );

		private readonly MeshIndices _TriangleIndices;
		private Rect3F? _Bounds;

		#endregion

		#region constructors

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
		public Mesh( int index, Vector3F[] positions, Vector3F[] normals, int[] triangleIndices, Vector2F[] textureCoordinates = null, Color? color = null, Color[] colors = null, string[] layer = null, string name = null )
			: this( index, positions, normals, MeshIndices.Create( triangleIndices ), textureCoordinates, color, colors, layer, name )
		{ }

		private Mesh( int index, Vector3F[] positions, Vector3F[] normals, MeshIndices triangleIndices, Vector2F[] textureCoordinates = null, Color? color = null, Color[] colors = null, string[] layer = null, string name = null )
		{
			Index = index;
			Positions = positions ?? Array.Empty<Vector3F>();
			_TriangleIndices = triangleIndices ?? EmptyMeshIndices;

			Normals = normals ?? Array.Empty<Vector3F>();
			TextureCoordinates = textureCoordinates ?? Array.Empty<Vector2F>();

			Normals = ValidateAndRegenerateNormals( Positions, Normals, _TriangleIndices.GetIndizes() );

			Colors = colors;
			Color = color;
			Layer = layer;
			Name = name;
		}

		#endregion

		#region properties

		/// <summary>
		/// Gets the number of triangles in this mesh.
		/// </summary>
		public int TriangleCount => TriangleIndicesCount / 3;

		#endregion

		#region methods

		/// <summary>
		/// Gets the triangle with the provided index.
		/// </summary>
		/// <param name="index">Index of the wanted triangle.</param>
		/// <returns>The wanted triangle's vertices.</returns>
		public Triangle GetTriangle( int index )
		{
			if( index > TriangleCount )
				throw new ArgumentException( $"{nameof( index )} {index} is greater than maximum index {TriangleCount}." );

			return new Triangle( index, this );
		}

		/// <summary>
		/// Generates the normals from the given positions and triangle information if needed.
		/// </summary>
		private static Vector3F[] ValidateAndRegenerateNormals( Vector3F[] positions, Vector3F[] normals, int[] triangleIndices )
		{
			if( normals.Length == positions.Length )
				return normals;

			var epsilon = (float)Math.Sqrt( 1.0000000116861E-07 );

			normals = new Vector3F[ positions.Length ];
			for( var i = 0; i < triangleIndices.Length; i += 3 )
			{
				var i0 = triangleIndices[ i ];
				var i1 = triangleIndices[ i + 1 ];
				var i2 = triangleIndices[ i + 2 ];

				var p0 = positions[ i0 ];
				var p1 = positions[ i1 ];
				var p2 = positions[ i2 ];

				var direction = Vector3F.CrossProduct( p1 - p0, p2 - p1 );
				var norm2 = direction.Length;

				if( norm2 > epsilon )
				{
					direction /= norm2;

					// Add direction vector to each vertex of the triangle.
					normals[ i0 ] += direction;
					normals[ i1 ] += direction;
					normals[ i2 ] += direction;
				}
			}

			// Normalize vertex normals.
			for( var i = 0; i < normals.Length; ++i )
			{
				normals[ i ].Normalize();
			}

			return normals;
		}

		internal static Mesh Read( BinaryReader binaryReader, int index, Version fileVersion )
		{
			var name = fileVersion >= FileVersion21 ? binaryReader.ReadString() : "";
			var color = binaryReader.ReadBoolean() ? binaryReader.ReadArgbColor() : default( Color? );

			var positions = binaryReader.ReadConditionalVector3FArray( fileVersion );
			var normals = binaryReader.ReadConditionalVector3FArray( fileVersion );
			var indices = binaryReader.ReadBoolean()
				? binaryReader.ReadIndices( positions?.Length ?? 0 )
				: null;

			var layer = binaryReader.ReadConditionalStringArray();

			var textureCoordinates = fileVersion >= FileVersion22 && binaryReader.ReadBoolean()
				? binaryReader.ReadArray( Vector2FIo.Instance )
				: null;
			var colors = fileVersion >= FileVersion33 && binaryReader.ReadBoolean()
				? binaryReader.ReadArray( ColorIo.Instance )
				: null;

			return new Mesh( index, positions, normals, indices, textureCoordinates, color, colors, layer, name );
		}

		internal void Write( BinaryWriter binaryWriter )
		{
			binaryWriter.Write( Name ?? "" );
			binaryWriter.WriteConditionalColor( Color );
			binaryWriter.WriteConditionalArray( Vector3FIo.Instance, Positions );
			binaryWriter.WriteConditionalArray( Vector3FIo.Instance, Normals );

			WriteTriangleIndizes( binaryWriter );

			binaryWriter.WriteConditionalStrings( Layer );
			binaryWriter.WriteConditionalArray( Vector2FIo.Instance, TextureCoordinates );
			binaryWriter.WriteConditionalArray( ColorIo.Instance, Colors );
		}

		private void WriteTriangleIndizes( BinaryWriter binaryWriter )
		{
			if( TriangleIndicesCount > 0 )
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
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		public override string ToString()
		{
			return $"Mesh [{Name}] ({Positions.Length} points, {TriangleIndicesCount} triangle indices)";
		}

		#endregion

		#region interface IMesh

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
		public Vector3F[] Normals { get; }

		/// <summary>
		/// Gets the position data array.
		/// </summary>
		public Vector3F[] Positions { get; }

		/// <summary>
		/// Gets the texture data array.
		/// </summary>
		public Vector2F[] TextureCoordinates { get; }

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
		public Color[] Colors { get; }

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
				for( var i = 0; i < Positions.Length; i++ )
				{
					bounds.Union( Positions[ i ] );
				}

				_Bounds = bounds;

				return _Bounds.Value;
			}
		}

		/// <summary>
		/// Gets the triangles' indices of the CAD model. The triangle indices point to the
		/// vertices' index. Therefore triangle indices are sorted like this:
		///
		/// { [0]triangle1VertexAIndex, [1]triangle1VertexBIndex, [2]triangle1VertexCIndex, [3]triangle2VertexAIndex, ...  }
		///
		/// To get the x component of the second triangle's first vertex, it's index has to be
		/// multiplied by 3:
		///
		/// positions: { [0] vertex0X, [1] vertex0Y, [2] vertex0Z, [3] vertex1X, [4] vertex1Y, ... }
		///
		/// Example:
		///
		/// triangle2VertexAIndex == 2
		/// therefore
		/// positions[2*3 + 0] == triangle2VertexA.X
		/// positions[2*3 + 1] == triangle2VertexA.Y
		/// </summary>
		public int[] GetTriangleIndices()
		{
			return _TriangleIndices?.GetIndizes() ?? Array.Empty<int>();
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

		#endregion

		#region class ByteMeshIndices

		internal class ByteMeshIndices : MeshIndices
		{
			#region members

			private readonly byte[] _Indices;

			#endregion

			#region constructors

			internal ByteMeshIndices( byte[] indices )
				: base( indices.Length )
			{
				_Indices = indices;
			}

			#endregion

			#region properties

			public override int this[ int index ] => _Indices[ index ];

			#endregion

			#region methods

			public override int[] GetIndizes()
			{
				return _Indices.Select( i => (int)i ).ToArray();
			}

			#endregion
		}

		#endregion

		#region class IntegerMeshIndices

		internal class IntegerMeshIndices : MeshIndices
		{
			#region members

			private readonly int[] _Indices;

			#endregion

			#region constructors

			internal IntegerMeshIndices( int[] indices )
				: base( indices.Length )
			{
				_Indices = indices;
			}

			#endregion

			#region properties

			public override int this[ int index ] => _Indices[ index ];

			#endregion

			#region methods

			public override int[] GetIndizes()
			{
				return _Indices;
			}

			#endregion
		}

		#endregion

		#region class MeshIndices

		internal abstract class MeshIndices
		{
			#region constructors

			internal MeshIndices( int count )
			{
				Count = count;
			}

			#endregion

			#region properties

			public int Count { get; }

			public abstract int this[ int index ] { get; }

			#endregion

			#region methods

			public static MeshIndices Create( int[] indices )
			{
				if( indices.Length == 0 )
					return new ByteMeshIndices( Array.Empty<byte>() );

				var maxValue = indices.Max();
				if( maxValue <= byte.MaxValue )
					return new ByteMeshIndices( indices.Select( i => (byte)i ).ToArray() );

				if( maxValue <= short.MaxValue )
					return new ShortMeshIndices( indices.Select( i => (short)i ).ToArray() );

				return new IntegerMeshIndices( indices );
			}

			public abstract int[] GetIndizes();

			#endregion
		}

		#endregion

		#region class ShortMeshIndices

		internal class ShortMeshIndices : MeshIndices
		{
			#region members

			private readonly short[] _Indices;

			#endregion

			#region constructors

			internal ShortMeshIndices( short[] indices )
				: base( indices.Length )
			{
				_Indices = indices;
			}

			#endregion

			#region properties

			public override int this[ int index ] => _Indices[ index ];

			#endregion

			#region methods

			public override int[] GetIndizes()
			{
				return _Indices.Select( i => (int)i ).ToArray();
			}

			#endregion
		}

		#endregion

		#region struct Triangle

		public readonly struct Triangle
		{
			private readonly Mesh _Mesh;
			private readonly int _Index;

			public Triangle( int index, Mesh mesh )
			{
				_Mesh = mesh;
				_Index = index;
			}

			public Vector3F VertexA => _Mesh.Positions[ _Mesh._TriangleIndices[ _Index * 3 + 0 ] ];
			public Vector3F VertexB => _Mesh.Positions[ _Mesh._TriangleIndices[ _Index * 3 + 1 ] ];
			public Vector3F VertexC => _Mesh.Positions[ _Mesh._TriangleIndices[ _Index * 3 + 2 ] ];

			public Vector3F NormalA => _Mesh.Normals[ _Mesh._TriangleIndices[ _Index * 3 + 0 ] ];
			public Vector3F NormalB => _Mesh.Normals[ _Mesh._TriangleIndices[ _Index * 3 + 1 ] ];
			public Vector3F NormalC => _Mesh.Normals[ _Mesh._TriangleIndices[ _Index * 3 + 2 ] ];

			public Vector3F MeanNormal => ( NormalA + NormalB + NormalC ) / 3f;
		}

		#endregion
	}
}