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
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Text;
	using System.Xml;

	#endregion

	/// <summary>
	/// Describes the meta data of a <see cref="MeshModelPart"/> or <see cref="MeshModel"/>.
	/// </summary>
	public sealed class MeshModelMetadata
	{
		#region members

		private static readonly XmlReaderSettings XmlReaderSettings = new XmlReaderSettings
		{
			IgnoreComments = true,
			IgnoreWhitespace = true,
			IgnoreProcessingInstructions = true,
			CloseInput = false,
			NameTable = new NameTable()
		};

		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshModelMetadata" /> class.
		/// </summary>
		/// <param name="fileVersion">The file version.</param>
		/// <param name="format">The source format.</param>
		/// <param name="name">The name.</param>
		/// <param name="layers">The layers to which this model or part belongs.</param>
		public MeshModelMetadata( Version fileVersion = null, string format = "", string name = "", string[] layers = null )
		{
			FileVersion = fileVersion ?? MeshModel.MeshModelFileVersion;
			SourceFormat = format;
			Name = name;
			Layer = layers ?? Array.Empty<string>();
		}

		#endregion

		#region properties

		/// <summary>
		/// Gets or sets the file version.
		/// </summary>
		public Version FileVersion { get; set; }

		/// <summary>
		/// Gets or sets the format from which the model was initially created.
		/// </summary>
		public string SourceFormat { get; set; }

		/// <summary>
		/// Gets or sets the unique identifier. It is used to detect changed models on the PiWeb server.
		/// </summary>
		public Guid Guid { get; set; } = Guid.NewGuid();

		/// <summary>
		/// Gets or sets the triangulation hash. Models with the same triangulation hash are defined to have the same triangle mesh.
		/// </summary>
		public Guid TriangulationHash { get; internal set; }

		/// <summary>
		/// Gets or sets the name of the model.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets the number of parts in the model.
		/// </summary>
		public int PartCount { get; private set; } = 1;

		/// <summary>
		/// Gets or sets the layers that exist in the model.
		/// </summary>
		public string[] Layer { get; set; } = Array.Empty<string>();

		/// <summary>
		/// Gets the names of the source models, in case this is a combined model
		/// </summary>
		public string[] SourceModels { get; private set; } = Array.Empty<string>();

		/// <summary>
		/// Gets the mesh value entries that exist in the associated <see cref="MeshModelPart"/> or <see cref="MeshModel"/>.
		/// </summary>
		/// <value>
		/// The mesh value entries.
		/// </value>
		public MeshValueEntry[] MeshValueEntries { get; internal set; } = Array.Empty<MeshValueEntry>();

		#endregion

		#region methods

		/// <summary>
		/// Combines the specified <paramref name="metadatas"/> and sets the name of the combined <see cref="MeshModelMetadata"/>.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="metadatas">The metadatas.</param>
		/// <returns></returns>
		public static MeshModelMetadata CreateCombined( string name, params MeshModelMetadata[] metadatas )
		{
			var format = metadatas[ 0 ].SourceFormat;
			var layer = new HashSet<string>();
			var sourceModels = new List<string>();

			for( var i = 0; i < metadatas.Length; i += 1 )
			{
				var metadata = metadatas[ i ];

				if( format != metadata.SourceFormat )
					format = "";

				layer.UnionWith( metadata.Layer );

				sourceModels.AddRange( metadata.SourceModels );
				if( !string.IsNullOrEmpty( metadata.Name ) && !sourceModels.Contains( metadata.Name ) )
					sourceModels.Add( metadata.Name );

				if( string.IsNullOrEmpty( name ) && !string.IsNullOrEmpty( metadata.Name ) )
					name = metadata.Name;
			}

			return new MeshModelMetadata { Name = name, SourceFormat = format, Layer = layer.ToArray(), PartCount = metadatas.Length, SourceModels = sourceModels.ToArray() };
		}


		/// <summary>
		/// Extracts the <see cref="MeshModelMetadata"/> from the specified <paramref name="stream"/>.
		/// </summary>
		/// <remarks>
		/// The stream must contain a meshmodel file which has been created with the <see cref="MeshModel.Serialize(Stream)"/> method.
		/// </remarks>
		public static MeshModelMetadata ExtractFrom( Stream stream, string subFolder = "" )
		{
			if( stream == null )
				throw new ArgumentNullException( nameof( stream ) );

			var seekableStream = stream.CanSeek ? stream : new MemoryStream( MeshModelHelper.StreamToArray( stream ) );

			using var zipFile = new ZipArchive( seekableStream, ZipArchiveMode.Read, true, Encoding.UTF8 );
			return ExtractFrom( zipFile, subFolder );
		}

		/// <summary>
		/// Extracts the <see cref="MeshModelMetadata"/> from the specified archive.
		/// </summary>
		/// <remarks>
		/// The stream must contain a meshmodel file which has been created with the <see cref="MeshModel.Serialize(Stream)"/> method.
		/// </remarks>
		public static MeshModelMetadata ExtractFrom( ZipArchive archive, string subFolder = "" )
		{
			if( archive == null )
				throw new ArgumentNullException( nameof( archive ) );

			using var entryStream = archive.GetEntry( Path.Combine( subFolder, "Metadata.xml" ) )?.Open();
			if( entryStream == null )
				throw new InvalidOperationException( "Unable to find metadata entry 'Metadata.xml' in zip archive." );

			var dataEntries = GetSubfolderEntries( archive, subFolder )
				.Where( entry => entry.Name != "PreviewImage.png" );

			return ReadFrom( entryStream, new ZipEntriesGuidGenerator( dataEntries ) );
		}

		/// <summary>
		/// Extracts a hash for comparison from the <see cref="MeshModelMetadata"/> within the specified <paramref name="stream"/>.
		/// </summary>
		/// <remarks>
		/// The stream must contain a meshmodel file which has been created with the <see cref="MeshModel.Serialize(Stream)"/> method.
		/// </remarks>
		/// <param name="stream">The stream containing the meshmodel file.</param>
		/// <param name="subFolder">The folder to read within the meshmodel file.</param>
		public static Guid ExtractComparisonHash( Stream stream, string subFolder = "" )
		{
			if( stream == null )
				throw new ArgumentNullException( nameof( stream ) );

			using var archive = new ZipArchive( stream.CanSeek ? stream : new MemoryStream( MeshModelHelper.StreamToArray( stream ) ), ZipArchiveMode.Read, true, Encoding.UTF8 );
			var dataEntries = GetSubfolderEntries( archive, subFolder )
				.Where( entry => entry.Name != "PreviewImage.png" );

			return ZipEntriesGuidGenerator.GenerateGuidStatic( dataEntries );
		}

		private static IEnumerable<ZipArchiveEntry> GetSubfolderEntries( ZipArchive archive, string subFolder )
		{
			var dataEntries = string.IsNullOrEmpty( subFolder )
				? archive.Entries
				: archive.Entries.Where( e => string.Equals( Path.GetDirectoryName( e.FullName ), subFolder, StringComparison.OrdinalIgnoreCase ) );
			return dataEntries;
		}

		/// <summary>
		/// Serializes the data of this instance and writes it into the specified <paramref name="stream" />.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="upgradeVersionNumber">if set to <c>true</c>, the version number is adjusted to match the current version.</param>
		public void WriteTo( Stream stream, bool upgradeVersionNumber )
		{
			if( upgradeVersionNumber )
				FileVersion = MeshModel.MeshModelFileVersion;

			var settings = new XmlWriterSettings
			{
				Indent = false,
				Encoding = Encoding.UTF8,
				CloseOutput = false
			};

			using var writer = XmlWriter.Create( stream, settings );
			writer.WriteStartDocument( true );
			writer.WriteStartElement( "MeshModelMetadata" );

			writer.WriteElementString( "FileVersion", FileVersion.ToString() );
			writer.WriteElementString( "SourceFormat", SourceFormat );
			writer.WriteElementString( "Guid", Guid.ToString( "N", CultureInfo.InvariantCulture ) );
			writer.WriteElementString( "TriangulationHash", TriangulationHash.ToString( "N", CultureInfo.InvariantCulture ) );
			writer.WriteElementString( "Name", Name );
			writer.WriteElementString( "PartCount", PartCount.ToString( CultureInfo.InvariantCulture ) );

			WriteLayers( writer );
			WriteSourceModels( writer );

			if( MeshValueEntries != null && MeshValueEntries.Length > 0 )
				WriteMeshValueEntries( writer );

			writer.WriteEndElement();
			writer.WriteEndDocument();
		}

		private void WriteSourceModels( XmlWriter writer )
		{
			if( SourceModels == null || SourceModels.Length == 0 ) return;

			writer.WriteStartElement( "SourceModels" );
			foreach( var layer in SourceModels )
			{
				writer.WriteElementString( "Model", layer );
			}

			writer.WriteEndElement();
		}

		private void WriteLayers( XmlWriter writer )
		{
			if( Layer == null || Layer.Length == 0 ) return;

			writer.WriteStartElement( "Layer" );
			foreach( var layer in Layer )
			{
				writer.WriteElementString( "string", layer );
			}

			writer.WriteEndElement();
		}

		private void WriteMeshValueEntries( XmlWriter writer )
		{
			writer.WriteStartElement( "MeshValueEntries" );
			foreach( var entry in MeshValueEntries )
			{
				writer.WriteStartElement( "MeshValueEntry" );
				entry.Write( writer );
				writer.WriteEndElement();
			}

			writer.WriteEndElement();
		}

		/// <summary>
		/// Initializes this instance with the serialized metadata from the specified <paramref name="stream"/>.
		/// </summary>
		public static MeshModelMetadata ReadFrom( Stream stream )
		{
			return ReadFrom( stream, new RandomGuidGenerator() );
		}

		private static MeshModelMetadata ReadFrom( Stream stream, IGuidGenerator fallbackGuidGenerator )
		{
			// Needed for later property validation
			var hasGuid = false;
			var hasName = false;
			var hasPartCount = false;

			using var reader = XmlReader.Create( stream, XmlReaderSettings );

			var result = new MeshModelMetadata();
			reader.MoveToElement();

			while( reader.Read() )
			{
				switch( reader.Name )
				{
					case "Guid":
						result.Guid = Guid.ParseExact( reader.ReadString(), "N" );
						hasGuid = true;
						break;
					case "TriangulationHash":
						result.TriangulationHash = Guid.ParseExact( reader.ReadString(), "N" );
						break;
					case "Name":
						result.Name = reader.ReadString();
						hasName = true;
						break;
					case "PartCount":
						result.PartCount = int.Parse( reader.ReadString(), CultureInfo.InvariantCulture );
						hasPartCount = true;
						break;
					case "FileVersion":
						result.FileVersion = new Version( reader.ReadString() );
						break;
					case "SourceFormat":
						result.SourceFormat = reader.ReadString();
						break;
					case "Layer":
						result.Layer = ReadLayers( reader );
						break;
					case "SourceModels":
						result.SourceModels = ReadSourceModels( reader );
						break;
					case "MeshValueEntries":
						result.MeshValueEntries = ReadMeshValueEntries( reader );
						break;
				}
			}

			// validation for mendatory properties since version 5.1.0.0 and missing Guid fallback for older versions
			if( result.FileVersion != null && result.FileVersion >= new Version( 5, 1, 0, 0 ) )
			{
				ValidateMetadata( hasGuid, hasName, hasPartCount );
			}
			else if( !hasGuid )
			{
				result.Guid = fallbackGuidGenerator.CreateGuid();
			}

			return result;
		}

		private static void ValidateMetadata( bool hasGuid, bool hasName, bool hasPartCount )
		{
			if( !hasGuid )
				throw new InvalidOperationException( MeshModelHelper.FormatResource<MeshModel>( "InvalidFormatMissingProperty_ErrorText", "Guid" ) );

			if( !hasName )
				throw new InvalidOperationException( MeshModelHelper.FormatResource<MeshModel>( "InvalidFormatMissingProperty_ErrorText", "Name" ) );

			if( !hasPartCount )
				throw new InvalidOperationException( MeshModelHelper.FormatResource<MeshModel>( "InvalidFormatMissingProperty_ErrorText", "PartCount" ) );
		}

		private static MeshValueEntry[] ReadMeshValueEntries( XmlReader reader )
		{
			var entries = new List<MeshValueEntry>();
			while( reader.Read() && reader.NodeType != XmlNodeType.EndElement )
			{
				if( reader.Name == "MeshValueEntry" )
					entries.Add( MeshValueEntry.Read( reader ) );
			}

			return entries.ToArray();
		}

		private static string[] ReadSourceModels( XmlReader reader )
		{
			var sourceModels = new List<string>();
			while( reader.Read() && reader.NodeType != XmlNodeType.EndElement )
			{
				sourceModels.Add( reader.ReadString() );
			}

			return sourceModels.ToArray();
		}

		private static string[] ReadLayers( XmlReader reader )
		{
			var layer = new List<string>();
			while( reader.Read() && reader.NodeType != XmlNodeType.EndElement )
			{
				layer.Add( reader.ReadString() );
			}

			return layer.ToArray();
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		public override string ToString()
		{
			return $"{Name} (version {FileVersion}, format {SourceFormat})";
		}

		#endregion
	}
}