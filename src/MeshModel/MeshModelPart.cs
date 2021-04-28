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
	using System.Buffers;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Text;

	#endregion

	/// <summary>
	/// Describes a CAD model that is composed of multiple triangle meshes and discrete edges.
	/// </summary>
	public sealed class MeshModelPart
	{
		#region members

		private readonly HashSet<string> _DisabledLayers = new HashSet<string>();
		private Rect3F? _Bounds;

		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshModelPart"/> class.
		/// </summary>
		/// <param name="metaData">The meta data.</param>
		/// <param name="meshes">The meshes.</param>
		/// <param name="edges">The edges.</param>
		/// <param name="meshValueLists">A list of data values for this model (deviations, minima, maxima, mean values, etc.).</param>
		/// <param name="thumbnail">The thumbnail.</param>
		public MeshModelPart( MeshModelMetadata metaData, IEnumerable<Mesh> meshes, IEnumerable<Edge> edges = null, IEnumerable<MeshValueList> meshValueLists = null, byte[] thumbnail = null )
			: this( metaData, SortMeshes( meshes ), SortEdges( edges ), meshValueLists?.ToArray() ?? Array.Empty<MeshValueList>(), thumbnail, false )
		{ }

		private MeshModelPart( MeshModelMetadata metaData, Mesh[] meshes, Edge[] edges, MeshValueList[] meshValueLists, byte[] thumbnail, bool updateTriangulationHash )
		{
			if( metaData.PartCount > 1 )
				throw new ArgumentException( $"A MeshModelPart may have only 1 part.", nameof( metaData ) );

			Meshes = meshes;
			Edges = edges;
			Metadata = metaData;

			if( updateTriangulationHash || Metadata.TriangulationHash == Guid.Empty )
				UpdateTriangulationHash();
			MeshValues = meshValueLists;

			CheckMeshValueIntegrity();

			Metadata.MeshValueEntries = MeshValues.Select( m => m.Entry ).ToArray();
			Thumbnail = thumbnail != null && thumbnail.Length > 0 ? thumbnail : null;
		}

		#endregion

		#region properties

		/// <summary>
		/// Gets the triangulated meshes.
		/// </summary>
		public Mesh[] Meshes { get; }

		/// <summary>
		/// Gets a list of data values for this model (deviations, minima, maxima, mean values, etc.)
		/// </summary>
		public MeshValueList[] MeshValues { get; }

		/// <summary>
		/// Gets the edges.
		/// </summary>
		public Edge[] Edges { get; }

		/// <summary>
		/// Gets the metadata.
		/// </summary>
		public MeshModelMetadata Metadata { get; }

		/// <summary>
		/// Gets the bounding box of this <see cref="MeshModelPart"/>.
		/// </summary>
		public Rect3F Bounds
		{
			get
			{
				if( _Bounds.HasValue ) return _Bounds.Value;

				var bounds = Rect3F.Empty;
				foreach( var m in Meshes )
					bounds.Union( m.Bounds );

				foreach( var e in Edges )
					bounds.Union( e.Bounds );
				_Bounds = bounds;

				return _Bounds.Value;
			}
		}


		/// <summary>
		/// Gets a value indicating whether this instance has thumbnail.
		/// </summary>
		public bool HasThumbnail => Thumbnail != null;

		/// <summary>
		/// Gets or sets the thumbnail of this <see cref="MeshModelPart"/>.
		/// </summary>
		/// <value>
		/// A byte array containing image data.
		/// </value>
		public byte[] Thumbnail { get; }

		/// <summary>
		/// Gets a value indicating whether this instance has mesh values.
		/// </summary>
		public bool HasMeshValueLists => ( MeshValues?.Length ?? 0 ) > 0;

		#endregion

		#region methods

		private static Mesh[] SortMeshes( IEnumerable<Mesh> meshes )
		{
			return ( meshes ?? Array.Empty<Mesh>() )
				.Where( m => m.Positions != null && m.Positions.Length > 0 )
				.OrderBy( m => ColorOrdering( m.Color ) )
				.ToArray();
		}

		private static Edge[] SortEdges( IEnumerable<Edge> edges )
		{
			return ( edges ?? Array.Empty<Edge>() )
				.Where( e => e.Points != null && e.Points.Length > 0 )
				.OrderBy( e => ColorOrdering( e.Color ) )
				.ToArray();
		}

		/// <summary>
		/// Create a clone of this mpdel with the new thumbnail <paramref name="thumbnail"/>.
		/// </summary>
		public MeshModelPart CreateModelWithThumbnail( byte[] thumbnail )
		{
			if( ( thumbnail ?? Array.Empty<byte>() ).SequenceEqual( Thumbnail ?? Array.Empty<byte>() ) )
				return this;

			return new MeshModelPart( Metadata, Meshes, Edges, MeshValues, thumbnail, false );
		}

		private void CheckMeshValueIntegrity()
		{
			if( MeshValues == null || MeshValues.Length == 0 || Meshes == null || Meshes.Length == 0 )
				return;

			foreach( var meshValue in MeshValues )
			{
				if( meshValue.MeshValues.Length != Meshes.Length )
					throw new ArgumentException( $"When specifiying a MeshValueList, every MeshValue must be exactly as long as the {nameof( Meshes )}" );
			}
		}

		/// <summary>
		/// Creates the specified <paramref name="subFolder"/> in the specified <paramref name="zipFile"/> and writes the serialized data into it.
		/// If the <paramref name="subFolder"/> is null or empty, the data will be serialized into the root directory of the zip archive .
		/// </summary>
		public void Serialize( ZipArchive zipFile, string subFolder = "" )
		{
			Metadata.MeshValueEntries = MeshValues.Select( v => v.Entry ).ToArray();

			using( var entryStream = zipFile.CreateNormalizedEntry( Path.Combine( subFolder, "Metadata.xml" ), CompressionLevel.Optimal ).Open() )
			{
				Metadata.WriteTo( entryStream, true );
			}

			// Vorschaubild
			if( HasThumbnail )
			{
				using var entryStream = zipFile.CreateNormalizedEntry( Path.Combine( subFolder, "PreviewImage.png" ), CompressionLevel.NoCompression ).Open();
				entryStream.Write( Thumbnail, 0, Thumbnail.Length );
			}

			// Triangulierungsdaten schreiben
			using( var entryStream = zipFile.CreateNormalizedEntry( Path.Combine( subFolder, "Meshes.dat" ), CompressionLevel.Fastest ).Open() )
			{
				using var binaryWriter = new BinaryWriter( entryStream, Encoding.UTF8, true );
				binaryWriter.Write( Meshes.Length );
				foreach( var mesh in Meshes )
				{
					mesh.Write( binaryWriter );
				}
			}

			// Edgedaten schreiben
			using( var entryStream = zipFile.CreateNormalizedEntry( Path.Combine( subFolder, "Edges.dat" ), CompressionLevel.Fastest ).Open() )
			{
				using var binaryWriter = new BinaryWriter( entryStream, Encoding.UTF8, true );
				binaryWriter.Write( Edges.Length );
				foreach( var edge in Edges )
				{
					edge.Write( binaryWriter );
				}
			}

			// Datenwerte schreiben
			foreach( var entry in MeshValues )
			{
				using var entryStream = zipFile.CreateNormalizedEntry( Path.Combine( subFolder, entry.Entry.Filename ), CompressionLevel.Fastest ).Open();
				using var binaryWriter = new BinaryWriter( entryStream, Encoding.UTF8, true );
				entry.Write( binaryWriter );
			}
		}

		/// <summary>
		/// Reads the data from the specified <paramref name="subFolder"/> in the specified <paramref name="zipFile"/> and deserializes the data found in it.
		/// If no <paramref name="subFolder"/> is specified, the method deserializes the data in the <paramref name="zipFile"/>s root directory.
		/// </summary>
		public static MeshModelPart Deserialize( ZipArchive zipFile, string subFolder = "" )
		{
			var fileVersion10 = new Version( 1, 0, 0, 0 );

			var metadata = MeshModelMetadata.ExtractFrom( zipFile, subFolder );

			// Versionschecks
			var fileVersion = metadata.FileVersion;
			if( fileVersion < fileVersion10 )
				throw new InvalidOperationException( MeshModelHelper.GetResource<MeshModel>( "OldFileVersionError_Text" ) );

			if( fileVersion.Major > MeshModel.MeshModelFileVersion.Major )
				throw new InvalidOperationException( MeshModelHelper.FormatResource<MeshModel>( "FileVersionError_Text", fileVersion, MeshModel.MeshModelFileVersion ) );

			var thumbnail = ReadThumbnail( zipFile, subFolder );
			var meshes = ReadMeshes( zipFile, subFolder, fileVersion );
			var edges = ReadEdges( zipFile, subFolder, fileVersion );
			var meshValueList = ReadMeshValueLists( zipFile, metadata, subFolder, fileVersion );

			return new MeshModelPart( metadata, meshes, edges, meshValueList, thumbnail, false );
		}

		/// <summary>
		/// Reads the values from the specified <paramref name="subFolder"/> in the specified <paramref name="zipFile"/> and deserializes the data found in it.
		/// If no <paramref name="subFolder"/> is specified, the method deserializes the data in the <paramref name="zipFile"/>s root directory.
		/// </summary>
		public static MeshModelPart DeserializeValues( MeshModelPart baseModelPart, ZipArchive zipFile, string subFolder = "" )
		{
			var fileVersion10 = new Version( 1, 0, 0, 0 );

			var metadata = MeshModelMetadata.ExtractFrom( zipFile, subFolder );

			if( metadata.TriangulationHash != baseModelPart.Metadata.TriangulationHash )
				throw new InvalidOperationException( MeshModelHelper.GetResource<MeshModel>( "TriangulationMismatch_ErrorText" ) );

			// Versionschecks
			var fileVersion = metadata.FileVersion;
			if( fileVersion < fileVersion10 )
				throw new InvalidOperationException( MeshModelHelper.GetResource<MeshModel>( "OldFileVersionError_Text" ) );

			if( fileVersion.Major > MeshModel.MeshModelFileVersion.Major )
				throw new InvalidOperationException( MeshModelHelper.FormatResource<MeshModel>( "FileVersionError_Text", fileVersion, MeshModel.MeshModelFileVersion ) );

			var meshValueList = ReadMeshValueLists( zipFile, metadata, subFolder, fileVersion );

			metadata.TriangulationHash = baseModelPart.Metadata.TriangulationHash;

			return new MeshModelPart( metadata, baseModelPart.Meshes, baseModelPart.Edges, meshValueList, baseModelPart.Thumbnail, false );
		}

		private static byte[] ReadThumbnail( ZipArchive zipFile, string subFolder )
		{
			var thumbnailEntry = zipFile.GetEntry( Path.Combine( subFolder, "PreviewImage.png" ) );
			if( thumbnailEntry == null ) return null;

			using var entryStream = thumbnailEntry.Open();
			return MeshModelHelper.StreamToArray( entryStream );
		}

		private static MeshValueList[] ReadMeshValueLists( ZipArchive zipFile, MeshModelMetadata metadata, string subFolder, Version fileVersion )
		{
			// Datenwerte lesen
			var meshValueList = new List<MeshValueList>( metadata.MeshValueEntries?.Length ?? 0 );
			foreach( var entry in metadata.MeshValueEntries ?? Array.Empty<MeshValueEntry>() )
			{
				using var binaryReader = new BinaryReader( zipFile.GetEntry( Path.Combine( subFolder, entry.Filename ) ).Open() );
				meshValueList.Add( MeshValueList.Read( binaryReader, entry, fileVersion ) );
			}

			return meshValueList.ToArray();
		}

		private static Edge[] ReadEdges( ZipArchive zipFile, string subFolder, Version fileVersion )
		{
			var entry = zipFile.GetEntry( Path.Combine( subFolder, "Edges.dat" ) );
			if( entry == null ) return Array.Empty<Edge>();

			using var binaryReader = new BinaryReader( entry.Open() );
			var edgeCount = binaryReader.ReadInt32();

			var edges = new List<Edge>( edgeCount );
			for( var i = 0; i < edgeCount; i++ )
			{
				edges.Add( Edge.Read( binaryReader, fileVersion ) );
			}

			// we don't try to create a single-point edge
			return edges.Where( e => e.Points?.Length > 1 ).ToArray();
		}

		private static Mesh[] ReadMeshes( ZipArchive zipFile, string subFolder, Version fileVersion )
		{
			var entry = zipFile.GetEntry( Path.Combine( subFolder, "Meshes.dat" ) );
			if( entry == null ) return Array.Empty<Mesh>();

			using var binaryReader = new BinaryReader( entry.Open() );
			var meshCount = binaryReader.ReadInt32();

			var meshes = new List<Mesh>( meshCount );
			for( var i = 0; i < meshCount; i++ )
			{
				meshes.Add( Mesh.Read( binaryReader, i, fileVersion ) );
			}

			return meshes.ToArray();
		}

		/// <summary>
		/// Determines whether the <see cref="MeshValues"/> of this instance contain an entry with the specified <paramref name="dataType"/>.
		/// </summary>
		/// <param name="dataType">Type of the data.</param>
		public bool ContainsMeshValueList( string dataType )
		{
			return string.IsNullOrEmpty( dataType ) || ( MeshValues != null && MeshValues.Any( m => string.Equals( m.Entry.DataType, dataType, StringComparison.OrdinalIgnoreCase ) ) );
		}

		/// <summary>
		/// Returns the dataset with the specified <paramref name="dataSetIndex" /> from the <see cref="MeshValueList" /> with the specified <paramref name="dataType" />.
		/// If no <paramref name="dataSetIndex" /> is specified, the method will return the first dataset or <code>null</code>, in case the list is empty.
		/// </summary>
		/// <param name="dataType">Type of the data.</param>
		/// <param name="dataSetIndex">Index of the data set.</param>
		/// <returns></returns>
		public MeshValueList GetMeshValueList( string dataType, int dataSetIndex = -1 )
		{
			var meshValueList = MeshValues.Where( list => list.Entry.DataType == dataType ).ToArray();

			if( meshValueList.Length == 0 )
				return null;

			if( dataSetIndex < 0 )
				return meshValueList[ 0 ];

			if( meshValueList.Length > 0 && dataSetIndex >= 0 && dataSetIndex < meshValueList.Length )
				return meshValueList[ dataSetIndex ];

			return null;
		}

		/// <summary>
		/// Returns all <see cref="MeshValueList" /> with the specified <paramref name="dataType" />.
		/// </summary>
		/// <param name="dataType">Type of the data.</param>
		/// <returns></returns>
		public MeshValueList[] GetMeshValueLists( string dataType )
		{
			return MeshValues.Where( list => string.Equals( list.Entry.DataType, dataType, StringComparison.OrdinalIgnoreCase ) ).ToArray();
		}

		private void UpdateTriangulationHash()
		{
			using var md5 = HashBuilder.CreateHashAlgorithm();

			foreach( var mesh in Meshes )
			{
				var indices = mesh.GetTriangleIndices();
				foreach( var (numberOfBytes, buffer) in IterateIndizes( indices ) )
				{
					md5.TransformBlock( buffer, 0, numberOfBytes, null, 0 );
				}
			}

			md5.TransformFinalBlock( Array.Empty<byte>(), 0, 0 );
			Metadata.TriangulationHash = new Guid( md5.Hash );
		}

		public IEnumerable<(int NumberOfBytes, byte[] Buffer)> IterateIndizes( int[] indices )
		{
			const int arrayLength = 8 * 1024;

			var processedBytes = 0;
			var totalBytes = sizeof( int ) * indices.Length;
			var buffer = ArrayPool<byte>.Shared.Rent( arrayLength );

			while( true )
			{
				var numberOfBytes = Math.Min( arrayLength, totalBytes - processedBytes );
				if( numberOfBytes <= 0 )
					break;

				Buffer.BlockCopy( indices, processedBytes, buffer, 0, numberOfBytes );
				yield return ( numberOfBytes, buffer );

				processedBytes += numberOfBytes;
			}

			ArrayPool<byte>.Shared.Return( buffer );
		}

		private static int ColorOrdering( Color? color )
		{
			if( color.HasValue )
				return color.Value.A << 24 | color.Value.R << 16 | color.Value.G << 8 | color.Value.B;
			return 0;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="layer"/> is currently enabled.
		/// </summary>
		/// <param name="layer">The layer.</param>
		public bool IsLayerEnabled( string layer )
		{
			return !_DisabledLayers.Contains( layer ?? "" );
		}


		/// <summary>
		/// Enables the specified <paramref name="layer"/>.
		/// </summary>
		/// <param name="layer">The layer.</param>
		public void EnableLayer( string layer )
		{
			_DisabledLayers.Remove( layer );
		}

		/// <summary>
		/// Disables the specified <paramref name="layer"/>.
		/// </summary>
		/// <param name="layer">The layer.</param>
		public void DisableLayer( string layer )
		{
			_DisabledLayers.Add( layer );
		}

		/// <summary>
		/// Creates a <see cref="MeshModel"/> that contains only this instance as <see cref="MeshModel.Parts"/>
		/// </summary>
		/// <returns></returns>
		public MeshModel ToMeshModel()
		{
			return MeshModel.FromMeshModelPart( this );
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		public override string ToString()
		{
			return $"CAD '{Metadata.Name}' ({Metadata.SourceFormat})";
		}

		#endregion
	}
}