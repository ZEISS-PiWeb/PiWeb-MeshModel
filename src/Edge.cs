#region Copyright
/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss IMT (IZfM Dresden)                   */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2010                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */
#endregion

namespace Zeiss.IMT.PiWeb.Meshmodels
{
	#region using

	using System;
	using System.IO;

	#endregion

	/// <summary>
	/// Describes an edge in the CAD model, that is usually used to outline surfaces.
	/// </summary>
	public sealed class Edge
	{
		#region constants

		private static readonly Version FileVersion10 = new Version( 1, 0, 0, 0 );
		private static readonly Version FileVersion21 = new Version( 2, 1 );
		private static readonly Version FileVersion32 = new Version( 3, 2 );

		private static readonly float[] EmptyFloatArray = new float[ 0 ];

		#endregion

		#region members

		private Rect3F? _Bounds;

		#endregion

		#region constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="Edge"/> class.
		/// </summary>
		/// <param name="points">The points.</param>
		/// <param name="color">The color.</param>
		/// <param name="layer">The layer.</param>
		/// <param name="name">The name.</param>
		public Edge( float[] points, Color? color = null, string[] layer = null, string name = null )
		{
			Points = points ?? EmptyFloatArray;
			Color = color;
			Layer = layer;
			Name = name;
		}

		#endregion

		#region properties

		/// <summary>
		/// Gets the name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets a value indicating whether this instance is empty (Has no points).
		/// </summary>
		public bool IsEmpty => Points.Length == 0;
		
		/// <summary>
		/// Gets the points of the edge.
		/// </summary>
		public float[] Points { get; }
		
		/// <summary>
		/// Gets the color of the edge.
		/// </summary>
		public Color? Color { get; }

		/// <summary>
		/// Gets the layers to which this edge belongs.
		/// </summary>
		public string[] Layer { get; }

		/// <summary>
		/// Gets the bounding box of the edge.
		/// </summary>
		public Rect3F Bounds
		{
			get
			{
				if( _Bounds.HasValue ) return _Bounds.Value;

				_Bounds = Rect3F.Empty;
				for( var i = 0; i < Points.Length; i += 3 )
				{
					_Bounds = Rect3F.Union( _Bounds.Value, new Point3F( Points[ i ], Points[ i + 1 ], Points[ i + 2 ] ) );
				}
				return _Bounds.Value;
			}
		}

		#endregion

		#region methods

		internal static Edge Read( BinaryReader binaryReader, Version fileVersion )
		{
			float[] positions = null;
			string[] layer = null;
			Color? color = null;
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

				if( fileVersion < FileVersion32 )
					positions = MeshModelHelper.RemoveDuplicatePositions( positions );
			}
			if( binaryReader.ReadBoolean() )
			{
				layer = new string[binaryReader.ReadInt32()];
				for( var l = 0; l < layer.Length; l++ )
				{
					layer[ l ] = binaryReader.ReadString();
				}
			}
			return new Edge( positions, color, layer, name );
		}

		internal void Write( BinaryWriter binaryWriter )
		{
			binaryWriter.Write( Name ?? "" );
			if (Color.HasValue)
			{
				binaryWriter.Write(true);
				binaryWriter.WriteArgbColor(Color.Value);
			}
			else
			{
				binaryWriter.Write(false);
			}

			if (Points != null && Points.Length > 0)
			{
				binaryWriter.Write(true);
				binaryWriter.WriteFloatArray(Points, 3);
			}
			else
			{
				binaryWriter.Write(false);
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
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		public override string ToString()
		{
			return $"Edge [{Name}] {Points.Length} points";
		}

		#endregion
	}
}