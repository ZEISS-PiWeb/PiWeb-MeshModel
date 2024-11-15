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
	using Zeiss.PiWeb.ColorScale;

	#endregion

	/// <summary>
	/// Describes an edge in the CAD model, that is usually used to outline surfaces.
	/// </summary>
	public sealed class Edge
	{
		#region members

		private static readonly Version FileVersion21 = new Version( 2, 1 );
		private static readonly Version FileVersion32 = new Version( 3, 2 );

		private Rect3F? _Bounds;

		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Edge"/> class.
		/// </summary>
		/// <param name="points">The points.</param>
		/// <param name="color">The color.</param>
		/// <param name="layer">The layer.</param>
		/// <param name="name">The name.</param>
		public Edge( Vector3F[] points, Color? color = null, string[] layer = null, string name = null )
		{
			Points = points ?? Array.Empty<Vector3F>();
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
		public Vector3F[] Points { get; }

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
				for( var i = 0; i < Points.Length; i++ )
				{
					_Bounds = Rect3F.Union( _Bounds.Value, Points[ i ] );
				}

				return _Bounds.Value;
			}
		}

		#endregion

		#region methods

		internal static Edge Read( BinaryReader binaryReader, Version fileVersion )
		{
			var name = fileVersion >= FileVersion21 ? binaryReader.ReadString() : "";
			var color = binaryReader.ReadBoolean() ? binaryReader.ReadArgbColor() : default( Color? );

			var positions = binaryReader.ReadConditionalVector3FArray( fileVersion );
			var layer = binaryReader.ReadConditionalStringArray();

			if( fileVersion < FileVersion32 )
				positions = MeshModelHelper.RemoveDuplicatePositions( positions );

			return new Edge( positions, color, layer, name );
		}

		internal void Write( BinaryWriter binaryWriter )
		{
			binaryWriter.Write( Name ?? "" );
			binaryWriter.WriteConditionalColor( Color );
			binaryWriter.WriteConditionalArray( Vector3FIo.Instance, Points );
			binaryWriter.WriteConditionalStrings( Layer );
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