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
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Text;

	#endregion

	/// <summary>
	/// Describes a CAD model that is composed of one or more child models of type <see cref="MeshModelPart"/>.
	/// </summary>
	public sealed class MeshModel
	{
		#region constants

		/// <summary>
		/// The default file extension of MeshModels with a leading point.
		/// </summary>
		public const string FileExtension = ".meshModel";

		#endregion

		#region members

		/// <summary>
		/// The current MeshModel file format version.
		/// </summary>
		public static readonly Version MeshModelFileVersion = new Version( 5, 1, 0, 0 );

		private readonly List<MeshModelPart> _Parts;

		private Rect3F? _Bounds;
		private byte[] _Thumbnail;

		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshModel" /> class from a set of triangle meshes.
		/// </summary>
		/// <param name="metaData">The meta data.</param>
		/// <param name="meshes">The triangle meshes.</param>
		public MeshModel( MeshModelMetadata metaData, IEnumerable<Mesh> meshes )
			: this( metaData, meshes, Array.Empty<Edge>() )
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshModel" /> class from a set of triangle meshes and edges.
		/// </summary>
		/// <param name="metaData">The meta data.</param>
		/// <param name="meshes">The triangle meshes.</param>
		/// <param name="edges">The edges.</param>
		public MeshModel( MeshModelMetadata metaData, IEnumerable<Mesh> meshes, IEnumerable<Edge> edges )
			: this( new MeshModelPart( metaData, meshes, edges ) )
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshModel" /> class from a set of <see cref="MeshModelPart" />s.
		/// </summary>
		/// <param name="metaData">The meta data.</param>
		/// <param name="parts">The parts.</param>
		public MeshModel( MeshModelMetadata metaData, IEnumerable<MeshModelPart> parts )
		{
			_Parts = new List<MeshModelPart>( parts );
			Metadata = metaData;

			if( metaData.PartCount != _Parts.Count )
				throw new ArgumentException( "Part count in meta data is different from actual part count.", nameof( metaData ) );
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshModel" /> class from a set of <see cref="MeshModelPart" />s.
		/// </summary>
		/// <param name="metadata">The meta data.</param>
		/// <param name="thumbnail">The thumbnail image.</param>
		/// <param name="parts">The parts.</param>
		public MeshModel( MeshModelMetadata metadata, byte[] thumbnail, IEnumerable<MeshModelPart> parts )
			: this( metadata, parts )
		{
			Thumbnail = thumbnail;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshModel" /> class from a single <see cref="MeshModelPart" />.
		/// </summary>
		/// <param name="part">The part.</param>
		private MeshModel( MeshModelPart part )
			: this( part.Metadata, part.Thumbnail, new List<MeshModelPart> { part } )
		{ }

		#endregion

		#region properties

		/// <summary>
		/// Returns the child models, of which this instance is composed.
		/// </summary>
		public IReadOnlyList<MeshModelPart> Parts => _Parts;

		/// <summary>
		/// Gets this <see cref="MeshModel"/>'s metadata.
		/// </summary>
		public MeshModelMetadata Metadata { get; }

		/// <summary>
		/// Gets the bounding box of this <see cref="MeshModel"/>.
		/// </summary>
		public Rect3F Bounds
		{
			get
			{
				if( _Bounds.HasValue )
					return _Bounds.Value;

				_Bounds = Rect3F.Empty;
				foreach( var p in Parts )
					_Bounds = Rect3F.Union( _Bounds.Value, p.Bounds );

				return _Bounds.Value;
			}
		}

		/// <summary>
		/// Gets or sets a byte array containing the thumbnail of this <see cref="MeshModel" />.
		/// </summary>
		public byte[] Thumbnail
		{
			get => _Thumbnail;
			set
			{
				_Thumbnail = value;
				if( _Thumbnail != null && _Thumbnail.Length == 0 )
					_Thumbnail = null;

				if( _Parts.Count == 1 )
				{
					_Parts[ 0 ] = _Parts[ 0 ].CreateModelWithThumbnail( value );
				}
			}
		}

		#endregion

		#region methods

		/// <summary>
		/// Converts this model to a <see cref="MeshModelPart"/>.
		/// </summary>
		/// <param name="meshValueLists">The mesh value lists.</param>
		/// <returns>A new MeshModelPart with all information of this MeshModel.</returns>
		public MeshModelPart ToMeshModelPart( IEnumerable<MeshValueList> meshValueLists = null )
		{
			var meshes = Parts.SelectMany( p => p.Meshes );
			var edges = Parts.SelectMany( p => p.Edges );

			return new MeshModelPart( Metadata, meshes, edges, meshValueLists, Thumbnail );
		}

		/// <summary>
		/// Creates a new <see cref="MeshModel"/> from a single <see cref="MeshModelPart"/>.
		/// </summary>
		/// <param name="part">The part.</param>
		/// <returns>A new <see cref="MeshModel"/>.</returns>
		public static MeshModel FromMeshModelPart( MeshModelPart part )
		{
			return new MeshModel( part );
		}

		/// <summary>
		/// Creates a combined <see cref="MeshModel" /> from the specified <paramref name="models" />.
		/// </summary>
		/// <param name="models">The models.</param>
		/// <returns>A new <see cref="MeshModel"/>.</returns>
		public static MeshModel CreateCombined( params MeshModel[] models )
		{
			return CreateCombined( (string)null, null, models );
		}

		/// <summary>
		/// Creates a combined <see cref="MeshModel" /> from the specified <paramref name="models" />.
		/// </summary>
		/// <param name="metadata">The metadata for the combined model.</param>
		/// <param name="thumbnail">The thumbnail for the combined model.</param>
		/// <param name="models">The models.</param>
		/// <returns>A new <see cref="MeshModel"/>.</returns>
		public static MeshModel CreateCombined( MeshModelMetadata metadata, byte[] thumbnail, params MeshModel[] models )
		{
			return new MeshModel( metadata, thumbnail, models.SelectMany( m => m.Parts ).ToArray() );
		}

		/// <summary>
		/// Creates a combined <see cref="MeshModel" /> from the specified <paramref name="models" />.
		/// </summary>
		/// <param name="name">The name for the combined model.</param>
		/// <param name="thumbnail">The thumbnail for the combined model.</param>
		/// <param name="models">The models.</param>
		/// <returns>A new <see cref="MeshModel"/>.</returns>
		public static MeshModel CreateCombined( string name, byte[] thumbnail, params MeshModel[] models )
		{
			return CreateCombined( name, thumbnail, models.SelectMany( m => m.Parts ).ToArray() );
		}

		/// <summary>
		/// Creates a combined <see cref="MeshModel" /> from the specified <paramref name="parts" />.
		/// </summary>
		/// <param name="name">The name for the combined model.</param>
		/// <param name="thumbnail">The thumbnail for the combined model.</param>
		/// <param name="parts">The parts.</param>
		/// <returns>A new <see cref="MeshModel"/>.</returns>
		public static MeshModel CreateCombined( string name, byte[] thumbnail, params MeshModelPart[] parts )
		{
			if( parts.Length == 0 )
				return new MeshModel( new MeshModelMetadata(), new List<MeshModelPart>() );

			if( parts.Length == 1 )
				return new MeshModel( parts[ 0 ] );

			for( var i = 0; i < parts.Length; i += 1 )
			{
				var part = parts[ i ];

				if( thumbnail == null && part.HasThumbnail )
					thumbnail = part.Thumbnail;
			}

			return new MeshModel( MeshModelMetadata.CreateCombined( name, parts.Select( p => p.Metadata ).ToArray() ), thumbnail, parts );
		}

		/// <summary>
		/// Serializes this <see cref="MeshModel"/> and returns it as a byte array.
		/// </summary>
		/// <returns>A byte array containing the serialized <see cref="MeshModel"/>.</returns>
		public byte[] Serialize()
		{
			var stream = new MemoryStream();

			Serialize( stream );

			return stream.ToArray();
		}

		/// <summary>
		/// Serializes this <see cref="MeshModel"/> and writes the result into the specified <paramref name="stream"/>.
		/// </summary>
		public void Serialize( Stream stream )
		{
			using var zipOutput = new ZipArchive( stream, ZipArchiveMode.Create, true, Encoding.UTF8 );

			// Write meta data.
			if( Parts.Count != 1 )
			{
				var entry = zipOutput.CreateNormalizedEntry( "Metadata.xml", CompressionLevel.Optimal );
				using( var entryStream = entry.Open() )
				{
					Metadata.WriteTo( entryStream, true );
				}

				// Preview image.
				if( _Thumbnail != null )
				{
					entry = zipOutput.CreateNormalizedEntry( "PreviewImage.png", CompressionLevel.NoCompression );
					using var entryStream = entry.Open();
					entryStream.Write( _Thumbnail, 0, _Thumbnail.Length );
				}

				// Write parts.
				for( var i = 0; i < Parts.Count; i += 1 )
				{
					Parts[ i ].Serialize( zipOutput, i.ToString() );
				}
			}
			else
			{
				Parts[ 0 ].Serialize( zipOutput );
			}
		}

		/// <summary>
		/// Extracts the <see cref="MeshModelFormatData"/> from the specified <paramref name="zipFile"/>
		/// and checks whether the version is known and supported.
		/// </summary>
		/// <param name="zipFile">Source zip file.</param>
		private static MeshModelFormatData ExtractAndCheckFormatData( ZipArchive zipFile )
		{
			var formatData = MeshModelFormatData.ExtractFrom( zipFile );

			var fileVersion = formatData.FileVersion;
			if( fileVersion == null )
				throw new InvalidOperationException( MeshModelHelper.GetResource<MeshModel>( "OldFileVersionError_Text" ) );
			if( fileVersion.Major > MeshModelFileVersion.Major )
				throw new InvalidOperationException( MeshModelHelper.FormatResource<MeshModel>( "FileVersionError_Text", fileVersion, MeshModelFileVersion ) );

			return formatData;
		}

		/// <summary>
		/// Deserializes a <see cref="MeshModel"/> from the specified <paramref name="stream"/>
		/// </summary>
		/// <returns>The deserialized <see cref="MeshModel"/> instance.</returns>
		public static MeshModel Deserialize( Stream stream )
		{
			if( !stream.CanSeek )
				stream = new MemoryStream( MeshModelHelper.StreamToArray( stream ) );

			using var zipFile = new ZipArchive( stream, ZipArchiveMode.Read, true, Encoding.UTF8 );
			var formatData = ExtractAndCheckFormatData( zipFile );

			// Load model data.
			return formatData.Type == MeshModelType.Part
				? DeserializeSinglePart( zipFile )
				: DeserializeComposite( zipFile );
		}

		/// <summary>
		/// Deserializes the values of a <see cref="MeshModel"/> from the specified <paramref name="stream"/>
		/// </summary>
		/// <returns>The deserialized <see cref="MeshModel"/> instance.</returns>
		public static MeshModel DeserializeValues( MeshModel baseModel, Stream stream )
		{
			if( !stream.CanSeek )
				stream = new MemoryStream( MeshModelHelper.StreamToArray( stream ) );

			using var zipFile = new ZipArchive( stream, ZipArchiveMode.Read, true, Encoding.UTF8 );
			var formatData = ExtractAndCheckFormatData( zipFile );

			// Load model data.
			return formatData.Type == MeshModelType.Part
				? DeserializeSinglePartValues( baseModel, zipFile )
				: DeserializeCompositeValues( baseModel, zipFile );
		}

		private static MeshModel DeserializeSinglePartValues( MeshModel baseModel, ZipArchive zipFile )
		{
			return new MeshModel( MeshModelPart.DeserializeValues( baseModel.Parts.First(), zipFile ) );
		}

		private static MeshModel DeserializeSinglePart( ZipArchive zipFile )
		{
			return new MeshModel( MeshModelPart.Deserialize( zipFile ) );
		}

		private static MeshModel DeserializeComposite( ZipArchive zipFile )
		{
			var metadata = MeshModelMetadata.ExtractFrom( zipFile );
			var partCount = metadata.PartCount;

			// Read preview image.
			byte[] thumbnail = null;

			if( partCount != 1 )
			{
				var entry = zipFile.GetEntry( "PreviewImage.png" );
				if( entry != null )
				{
					using var entryStream = entry.Open();
					thumbnail = MeshModelHelper.StreamToArray( entryStream );
				}
			}

			// Read parts.
			var parts = new List<MeshModelPart>( partCount );
			for( var i = 0; i < partCount; i += 1 )
			{
				parts.Add( MeshModelPart.Deserialize( zipFile, i.ToString() ) );
			}

			return new MeshModel( metadata, thumbnail, parts.ToArray() );
		}

		private static MeshModel DeserializeCompositeValues( MeshModel baseModel, ZipArchive zipFile )
		{
			var metadata = MeshModelMetadata.ExtractFrom( zipFile );
			var partCount = metadata.PartCount;

			// Read parts.
			var parts = new List<MeshModelPart>( partCount );
			for( var i = 0; i < partCount; i += 1 )
			{
				parts.Add( MeshModelPart.DeserializeValues( baseModel.Parts[ i ], zipFile, i.ToString() ) );
			}

			return new MeshModel( metadata, baseModel.Thumbnail, parts.ToArray() );
		}

		/// <summary>
		/// Determines whether the specified <paramref name="layer"/> is currently enabled.
		/// </summary>
		/// <param name="layer">The layer.</param>
		public bool IsLayerEnabled( string layer )
		{
			return Parts.Any( p => p.IsLayerEnabled( layer ) );
		}

		/// <summary>
		/// Determines whether all of the specified <paramref name="layers"/> are currently enabled.
		/// </summary>
		/// <param name="layers">The layers.</param>
		public bool AreLayersEnabled( string[] layers )
		{
			if( layers == null || layers.Length == 0 )
				return IsLayerEnabled( "" );

			return layers.All( IsLayerEnabled );
		}

		/// <summary>
		/// Enables the specified <paramref name="layer"/>.
		/// </summary>
		/// <param name="layer">The layer.</param>
		public void EnableLayer( string layer )
		{
			foreach( var part in Parts )
			{
				part.EnableLayer( layer );
			}
		}

		/// <summary>
		/// Disables the specified <paramref name="layer"/>.
		/// </summary>
		/// <param name="layer">The layer.</param>
		public void DisableLayer( string layer )
		{
			foreach( var part in Parts )
			{
				part.DisableLayer( layer );
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"CAD {Parts.Count} '{Metadata.Name}'";
		}

		#endregion
	}
}