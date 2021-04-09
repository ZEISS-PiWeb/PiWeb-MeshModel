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
	/// Describes a CAD model that is composed of multiple triangle meshes and discrete edges.
	/// </summary>
	public sealed class MeshModelPart
	{
		#region members

		private readonly HashSet<string> _DisabledLayers = new HashSet<string>();

		private Rect3F? _Bounds;
		private byte[] _Thumbnail;

		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshModelPart" /> class.
		/// </summary>
		/// <param name="metaData">The meta data.</param>
		private MeshModelPart( MeshModelMetadata metaData )
		{
			ConstructGeometry( new Mesh[0], new Edge[0] );
			Metadata = metaData;

			UpdateTriangulationHash();
			MeshValues = new MeshValueList[0];

			Thumbnail = null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshModelPart"/> class.
		/// </summary>
		/// <param name="metaData">The meta data.</param>
		/// <param name="meshes">The meshes.</param>
		/// <param name="edges">The edges.</param>
		/// <param name="meshValueLists">A list of data values for this model (deviations, minima, maxima, mean values, etc.).</param>
		/// <param name="thumbnail">The thumbnail.</param>
		public MeshModelPart( MeshModelMetadata metaData, IEnumerable<Mesh> meshes, IEnumerable<Edge> edges = null, IEnumerable<MeshValueList> meshValueLists = null, byte[] thumbnail = null )
		{
			ConstructGeometry( meshes, edges );
			Metadata = metaData;

			UpdateTriangulationHash();
			MeshValues = ( meshValueLists ?? new MeshValueList[0] ).ToArray();

			CheckMeshValueIntegrity();

			Metadata.MeshValueEntries = MeshValues.Select( m => m.Entry ).ToArray();

			Thumbnail = thumbnail;
		}

		#endregion

		#region properties

		/// <summary>
		/// Gets the triangulated meshes.
		/// </summary>
		public Mesh[] Meshes { get; private set; }

		/// <summary>
		/// Gets a list of data values for this model (deviations, minima, maxima, mean values, etc.)
		/// </summary>
		public MeshValueList[] MeshValues { get; private set; }

		/// <summary>
		/// Gets the edges.
		/// </summary>
		public Edge[] Edges { get; private set; }

		/// <summary>
		/// Gets the metadata.
		/// </summary>
		public MeshModelMetadata Metadata { get; private set; }

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
		public bool HasThumbnail => _Thumbnail != null;

		/// <summary>
		/// Gets or sets the thumbnail of this <see cref="MeshModelPart"/>.
		/// </summary>
		/// <value>
		/// A byte array containing image data.
		/// </value>
		public byte[] Thumbnail
		{
			get { return _Thumbnail; }
			set
			{
				_Thumbnail = value;
				if( _Thumbnail != null && _Thumbnail.Length == 0 )
					_Thumbnail = null;
			}
		}


		/// <summary>
		/// Gets a value indicating whether this instance has mesh values.
		/// </summary>
		public bool HasMeshValueLists => ( MeshValues?.Length ?? 0 ) > 0;

		#endregion

		#region methods

		private void CheckMeshValueIntegrity()
		{
			if( MeshValues == null || MeshValues.Length == 0 || Meshes == null || Meshes.Length == 0 )
				return;

			foreach (var meshValue in MeshValues)
			{
				if (meshValue.MeshValues.Length != Meshes.Length)
					throw new ArgumentException( $"When specifiying a MeshValueList, every MeshValue must be exactly as long as the {nameof( Meshes )}" );
			}
		}

		private void ConstructGeometry( IEnumerable<Mesh> meshes, IEnumerable<Edge> edges )
		{
			Meshes = meshes.Where( m => m.Positions != null && m.Positions.Length > 0 ).OrderBy( m => ColorOrdering( m.Color ) ).ToArray();
			Edges = ( edges ?? new Edge[0] ).Where( e => e.Points != null && e.Points.Length > 0 ).OrderBy( e => ColorOrdering( e.Color ) ).ToArray();
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
				using( var entryStream = zipFile.CreateNormalizedEntry( Path.Combine( subFolder, "PreviewImage.png" ), CompressionLevel.NoCompression ).Open() )
				{
					entryStream.Write( _Thumbnail, 0, _Thumbnail.Length );
				}
			}

			// Triangulierungsdaten schreiben
			using( var entryStream = zipFile.CreateNormalizedEntry( Path.Combine( subFolder, "Meshes.dat" ), CompressionLevel.Fastest ).Open() )
			using( var binaryWriter = new BinaryWriter( entryStream, Encoding.UTF8, true ) )
			{
				binaryWriter.Write( Meshes.Length );
				foreach( var mesh in Meshes )
				{
					mesh.Write( binaryWriter );
				}
			}

			// Edgedaten schreiben
			using( var entryStream = zipFile.CreateNormalizedEntry( Path.Combine( subFolder, "Edges.dat" ), CompressionLevel.Fastest ).Open() )
			using( var binaryWriter = new BinaryWriter( entryStream, Encoding.UTF8, true ) )
			{
				binaryWriter.Write( Edges.Length );
				foreach( var edge in Edges )
				{
					edge.Write( binaryWriter );
				}
			}

			// Datenwerte schreiben
			foreach( var entry in MeshValues )
			{
				using( var entryStream = zipFile.CreateNormalizedEntry( Path.Combine( subFolder, entry.Entry.Filename ), CompressionLevel.Fastest ).Open() )
				using( var binaryWriter = new BinaryWriter( entryStream, Encoding.UTF8, true ) )
				{
					entry.Write( binaryWriter );
				}
			}
		}

		/// <summary>
		/// Reads the data from the specified <paramref name="subFolder"/> in the specified <paramref name="zipFile"/> and deserializes the data found in it.
		/// If no <paramref name="subFolder"/> is specified, the method deserializes the data in the <paramref name="zipFile"/>s root directory.
		/// </summary>
		public static MeshModelPart Deserialize( ZipArchive zipFile, string subFolder = "" )
		{
			var fileVersion10 = new Version( 1, 0, 0, 0 );

			var result = new MeshModelPart( new MeshModelMetadata() );
			result.Metadata = MeshModelMetadata.ExtractFrom( zipFile, subFolder );

			// Versionschecks
			var fileVersion = result.Metadata.FileVersion;
			if( fileVersion < fileVersion10 )
		        throw new InvalidOperationException( MeshModelHelper.GetResource<MeshModel>( "OldFileVersionError_Text" ) );

			if( fileVersion.Major > MeshModel.MeshModelFileVersion.Major )
				throw new InvalidOperationException( MeshModelHelper.FormatResource<MeshModel>( "FileVersionError_Text", fileVersion, MeshModel.MeshModelFileVersion ) );
			
			// Vorschaubild lesen
			var thumbnailEntry = zipFile.GetEntry( Path.Combine( subFolder, "PreviewImage.png" ) );
			if( thumbnailEntry != null )
			{
				using( var entryStream = thumbnailEntry.Open() )
				{
					result._Thumbnail = MeshModelHelper.StreamToArray( entryStream );
				}
			}

			// Triangulierungsdaten lesen
			using( var binaryReader = new BinaryReader( zipFile.GetEntry( Path.Combine( subFolder, "Meshes.dat" ) ).Open() ) )
			{
				var meshCount = binaryReader.ReadInt32();

				var meshes = new List<Mesh>( meshCount );
				for( var i = 0; i < meshCount; i++ )
				{
					meshes.Add( Mesh.Read( binaryReader, i, fileVersion ) );
				}
				result.Meshes = meshes.ToArray();
			}

			// Edgedaten lesen
			using( var binaryReader = new BinaryReader( zipFile.GetEntry( Path.Combine( subFolder, "Edges.dat" ) ).Open() ) )
			{
				var edgeCount = binaryReader.ReadInt32();

				var edges = new List<Edge>( edgeCount );
				for( var i = 0; i < edgeCount; i++ )
				{
					edges.Add( Edge.Read( binaryReader, fileVersion ) );
				}
				// we don't try to create a single-point edge
				result.Edges = edges.Where( e => e.Points?.Length > 3 ).ToArray();
			}

			// Datenwerte lesen
			var meshValueList = new List<MeshValueList>( result.Metadata.MeshValueEntries?.Length ?? 0 );
			foreach( var entry in result.Metadata.MeshValueEntries ?? new MeshValueEntry[0] )
			{
				using( var binaryReader = new BinaryReader( zipFile.GetEntry( Path.Combine( subFolder, entry.Filename ) ).Open() ) )
				{
					meshValueList.Add( MeshValueList.Read( binaryReader, entry, fileVersion ) );
				}
			}
			result.MeshValues = meshValueList.ToArray();

			return result;
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
			using( var md5 = HashBuilder.CreateHashAlgorithm() )
			{
				foreach( var mesh in Meshes )
				{
					var indices = mesh.GetTriangleIndices();
					var block = new byte[indices.Length * sizeof( int )];
					Buffer.BlockCopy( indices, 0, block, 0, block.Length );

					md5.TransformBlock( block, 0, block.Length, block, 0 );
				}
				md5.TransformFinalBlock( new byte[0], 0, 0 );
				Metadata.TriangulationHash = new Guid( md5.Hash );
			}
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