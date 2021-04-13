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
	using System.Globalization;
	using System.IO;
	using System.IO.Compression;
	using System.Text;
	using System.Xml;

	#endregion

	/// <summary>
	/// Describes the format of a <see cref="MeshModelPart"/> or <see cref="MeshModel"/>.
	/// </summary>
	public sealed class MeshModelFormatData
	{
		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshModelFormatData" /> class.
		/// </summary>
		public MeshModelFormatData( Version fileVersion, MeshModelType type )
		{
			FileVersion = fileVersion;
			Type = type;
		}

		#endregion

		#region properties

		/// <summary>
		/// Gets the file version.
		/// </summary>
		public Version FileVersion { get; }

		/// <summary>
		/// Gets the file version.
		/// </summary>
		public MeshModelType Type { get; }

		#endregion

		#region methods

		/// <summary>
		/// Extracts the <see cref="MeshModelFormatData"/> from the specified <paramref name="stream"/>.
		/// </summary>
		/// <remarks>
		/// The stream must contain a meshmodel file which has been created with the <see cref="MeshModel.Serialize(Stream)"/> method.
		/// </remarks>
		public static MeshModelFormatData ExtractFrom( Stream stream )
		{
			if( stream == null )
				throw new ArgumentNullException( nameof( stream ) );

			using var zipFile = new ZipArchive( stream.CanSeek ? stream : new MemoryStream( MeshModelHelper.StreamToArray( stream ) ), ZipArchiveMode.Read, true, Encoding.UTF8 );
			return ExtractFrom( zipFile );
		}

		/// <summary>
		/// Initializes this instance with the serialized metadata from the specified archive.
		/// </summary>
		public static MeshModelFormatData ExtractFrom( ZipArchive archive )
		{
			var entry = archive.GetEntry( "Metadata.xml" );
			if( entry == null )
				throw new InvalidOperationException( MeshModelHelper.FormatResource<MeshModel>( "InvalidFormatMissingManifest_ErrorText", "Metadata.xml" ) );

			using var entryStream = entry.Open();
			return ReadFrom( entryStream );
		}

		/// <summary>
		/// Initializes this instance with the serialized metadata from the specified <paramref name="stream"/>.
		/// </summary>
		public static MeshModelFormatData ReadFrom( Stream stream )
		{
			var settings = new XmlReaderSettings
			{
				IgnoreComments = true,
				IgnoreWhitespace = true,
				IgnoreProcessingInstructions = true,
				CloseInput = false,
				NameTable = new NameTable()
			};
			using var reader = XmlReader.Create( stream, settings );

			Version fileVersion = null;
			var type = MeshModelType.Part;

			reader.MoveToElement();
			while( reader.Read() )
			{
				switch( reader.Name )
				{
					case "PartCount":
						var partCount = int.Parse( reader.ReadString(), CultureInfo.InvariantCulture );
						type = partCount == 1 ? MeshModelType.Part : MeshModelType.Composite;
						break;

					case "FileVersion":
						fileVersion = new Version( reader.ReadString() );
						break;
				}
			}

			return new MeshModelFormatData( fileVersion, type );
		}

		#endregion
	}
}