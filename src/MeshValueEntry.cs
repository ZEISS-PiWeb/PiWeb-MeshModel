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

	using System.Xml;

	#endregion

	/// <summary>
	/// Describes the type or content of one or more <see cref="MeshValue"/> datasets.
	/// </summary>
	public class MeshValueEntry
	{
		#region constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshValueEntry"/> class.
		/// </summary>
		/// <param name="dataType">Type of the data.</param>
		/// <param name="filename">The filename which is used during serialization.</param>
		/// <param name="colorScale">The color scale.</param>
		public MeshValueEntry( string dataType, string filename, ColorScale colorScale = null )
		{
			DataType = dataType.ToLowerInvariant();
			Filename = filename;
			ColorScale = colorScale;
		}

		#endregion

		#region properties

		/// <summary>
		/// Gets the color scale.
		/// </summary>
		public ColorScale ColorScale { get; }
		
		/// <summary>
		/// Gets the type of the data.
		/// </summary>
		/// <value>
		/// Use the <see cref="WellKnownDataTypes"/> to explore the known data types and to avoid typos.
		/// </value>
		public string DataType { get; }

		/// <summary>
		/// Gets a value indicating whether this entry marks deviations.
		/// </summary>
		public bool IsDeviation => string.Equals( WellKnownDataTypes.Deviation, DataType, System.StringComparison.OrdinalIgnoreCase );

		/// <summary>
		/// The filename within the meshmodel archive from which this entry was loaded, and to which it's serialized.
		/// </summary>
		public string Filename { get; }

		#endregion

		#region methods

		internal static MeshValueEntry Read( XmlReader reader )
		{
			var dataType = reader.GetAttribute( "DataType" );
			var filename = reader.GetAttribute( "Filename" );

			ColorScale colorScale = null;

			var colorScaleReader = reader.ReadSubtree();
			while( colorScaleReader.ReadToFollowing("ColorScale") && colorScaleReader.NodeType == XmlNodeType.Element )
			{
				colorScale = ColorScale.Read(colorScaleReader);
			}
			return new MeshValueEntry( dataType, filename, colorScale );
		}

		internal void Write( XmlWriter writer )
		{
			writer.WriteAttributeString( "DataType", DataType );
			writer.WriteAttributeString( "Filename", Filename );

			if( ColorScale != null )
			{
				writer.WriteStartElement( "ColorScale" );
				ColorScale.Write( writer );
				writer.WriteEndElement();
			}
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		public override string ToString()
		{
			return $"{DataType}: {Filename}";
		}

		#endregion
	}
}