#region copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss Industrielle Messtechnik GmbH        */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2021                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

namespace Zeiss.PiWeb.MeshModel.Tests
{
	#region usings

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Diagnostics;
	using NUnit.Framework;
	using Zeiss.PiWeb.MeshModel;

	#endregion

	[TestFixture]
	public class MeshModelTest
	{
		#region members

		private static readonly string MeshModelTestFile = Path.Combine(
			TestContext.CurrentContext.TestDirectory,
			"TestData",
			"test_deviation.meshModel" );

		private static readonly string MeshModelReWrittenFile = $"{MeshModelTestFile}.rewritten.meshModel";

		#endregion

		#region methods

		[Test, Description( "Checks if MeshModel::Parts works properly." )]
		public void Test_MeshModel_Parts()
		{
			// Given
			var part1 = new MeshModelPart( new MeshModelMetadata(), Array.Empty<Mesh>());
			var part2 = new MeshModelPart( new MeshModelMetadata(), Array.Empty<Mesh>());
			var part3 = new MeshModelPart( new MeshModelMetadata(), Array.Empty<Mesh>());
			var parts = new[] { part1, part2, part3 };
			var metaData = new MeshModelMetadata( partCount: 3 );

			// When
			var meshModel = new MeshModel( metaData, parts );

			// Then
			Assert.That( meshModel.Parts, Is.EquivalentTo( parts ) );
		}

		[Test, Description( "Checks if MeshModel::MetaData works properly." )]
		public void Test_MeshModel_MetaData()
		{
			// Given
			var part1 = new MeshModelPart( new MeshModelMetadata(), Array.Empty<Mesh>());
			var part2 = new MeshModelPart( new MeshModelMetadata(), Array.Empty<Mesh>());
			var parts = new[] { part1, part2 };
			var metaData = new MeshModelMetadata( partCount: 2 );

			// When
			var meshModel = new MeshModel( metaData, parts );

			// Then
			Assert.That( meshModel.Metadata, Is.EqualTo( metaData ) );
		}

		[Test, Description( "Checks if MeshModels consisting of multiple MeshModelParts return the correct bounds." )]
		public void Test_Bounds_Combination()
		{
			// Given
			var triangleIndices = new[] { 0, 1, 2 };
			var dummyNormals = Array.Empty<Vector3F>();
			var dummyMetaData = new MeshModelMetadata();

			var triangle1 = new[] { new Vector3F( 0, 0, 0 ), new Vector3F( 5, 0, 0 ), new Vector3F( 5, 5, 5 ) };
			var triangle2 = new[] { new Vector3F( -1, -1, -1 ), new Vector3F( 1, 0, 0 ), new Vector3F( 1, 1, 1 ) };
			var triangle3 = new[] { new Vector3F( 6, 6, 6 ), new Vector3F( 7, 6, 6 ), new Vector3F( 8, 8, 8 ) };

			var meshes1 = new[] { new Mesh( 0, triangle1, dummyNormals, triangleIndices ) };
			var meshes2 = new[] { new Mesh( 1, triangle2, dummyNormals, triangleIndices ) };
			var meshes3 = new[] { new Mesh( 2, triangle3, dummyNormals, triangleIndices ) };

			var parts = new[]
			{
				new MeshModelPart( dummyMetaData, meshes1 ),
				new MeshModelPart( dummyMetaData, meshes2 ),
				new MeshModelPart( dummyMetaData, meshes3 )
			};

			var metaData = new MeshModelMetadata( partCount: 3 );
			var expectedCombinedBounds = new Rect3F( -1, -1, -1, 9, 9, 9 );


			// When
			var meshModel = new MeshModel( metaData, parts );


			// Then
			Assert.That( meshModel.Bounds, Is.EqualTo( expectedCombinedBounds ) );
		}

		[Test, Description( "Checks if MeshModel::Thumbnail works properly." )]
		public void Test_MeshModel_Thumbnail()
		{
			// Given
			var thumbnail = new byte[] { 3, 53, 53, 5, 3, 5, 35, 76, 35, 237, 37, 72, 73, 4, 123, 3 };
			var metaData = new MeshModelMetadata();

			// When
			var meshModel = new MeshModel( metaData, Array.Empty<Mesh>() )
			{
				Thumbnail = thumbnail
			};

			// Then
			Assert.That( meshModel.Thumbnail, Is.EqualTo( thumbnail ) );
			Assert.That( meshModel.Parts[0].Thumbnail, Is.EqualTo( thumbnail ) );
		}

		[Test, Description( "Checks if MeshModel::Thumbnail works properly for combined MeshModels." )]
		public void Test_Combined_MeshModel_Thumbnail()
		{
			// Given
			var thumbnail = new byte[] { 3, 53, 53, 5, 3, 5, 35, 76, 35, 237, 37, 72, 73, 4, 123, 3 };
			var part1 = new MeshModelPart( new MeshModelMetadata(), Array.Empty<Mesh>());
			var part2 = new MeshModelPart( new MeshModelMetadata(), Array.Empty<Mesh>());
			var parts = new[] { part1, part2 };
			var metaData = new MeshModelMetadata( partCount: 2 );

			// When
			var meshModel = new MeshModel( metaData, parts )
			{
				Thumbnail = thumbnail
			};

			// Then
			Assert.That( meshModel.Thumbnail, Is.EqualTo( thumbnail ) );
			Assert.That( meshModel.Parts[0].Thumbnail, Is.Null );
			Assert.That( meshModel.Parts[1].Thumbnail, Is.Null );
		}

		[Test, Description( "Given: Test file, When: imported, Then: Imported values are correct." )]
		public void Test_Serialization_Deserialization()
		{
			// Given
			using var file = File.OpenRead( MeshModelTestFile );

			// When
			var meshModel = MeshModel.Deserialize( file );

			// Then
			Assert.That( meshModel.Metadata.FileVersion == new Version( 5, 1, 0, 0 ) );
			Assert.That( meshModel.Metadata.Guid, Is.EqualTo( new Guid( "8b935631492d434bb5d0b31b89f9ea7a" ) ) );
			Assert.That( meshModel.Metadata.TriangulationHash, Is.EqualTo( new Guid( "3da241a2ab39cfed8a278b4c828014af" ) ) );
			Assert.That( meshModel.Metadata.Name, Is.EqualTo( "[5x] Blech_P" ) );
			Assert.That( meshModel.Metadata.PartCount, Is.EqualTo( 1 ) );
			Assert.That( meshModel.Metadata.SourceFormat, Is.EqualTo( "Iges" ) );
			Assert.That( meshModel.Metadata.MeshValueEntries.Length, Is.EqualTo( 5 ) );
			Assert.That( meshModel.Metadata.MeshValueEntries[ 0 ].Filename, Is.EqualTo( "deviation0.dat" ) );
			Assert.That( meshModel.Metadata.MeshValueEntries[ 1 ].Filename, Is.EqualTo( "deviation1.dat" ) );
			Assert.That( meshModel.Metadata.MeshValueEntries[ 0 ].ColorScale.Name, Is.EqualTo( "SimpleDynamic" ) );
			Assert.That( meshModel.Metadata.MeshValueEntries[ 0 ].ColorScale.Interpolation,
				Is.EqualTo( ColorScaleInterpolation.HSV ) );
			Assert.That( meshModel.Metadata.MeshValueEntries[ 0 ].ColorScale.InvalidColor,
				Is.EqualTo( Color.FromRgb( 128, 128, 128 ) ) );
			Assert.That( meshModel.Metadata.MeshValueEntries[ 0 ].ColorScale.Entries.Length, Is.EqualTo( 3 ) );
			Assert.That( meshModel.Metadata.MeshValueEntries[ 0 ].ColorScale.Entries[ 0 ].RightColor,
				Is.EqualTo( Color.FromRgb( 67, 122, 180 ) ) );
			Assert.That( meshModel.Metadata.MeshValueEntries[ 0 ].ColorScale.Entries[ 0 ].LeftColor,
				Is.EqualTo( Color.FromRgb( 67, 122, 180 ) ) );
			Assert.That( meshModel.Metadata.MeshValueEntries[ 0 ].ColorScale.Entries[ 0 ].Value, Is.LessThan( -2f ) );
		}

		[Test, Description( "Given: Test file, When: imported and exported, Then: Both files are binary equal." )]
		public void Test_Import_Export()
		{
			// Given
			var originalInfo = new FileInfo( MeshModelTestFile );
			var reWrittenInfo = new FileInfo( MeshModelReWrittenFile );

			// When
			using( var fsOriginal = originalInfo.OpenRead() )
			{
				using( var fsReWritten = reWrittenInfo.OpenWrite() )
				{
					var stopwatch = new Stopwatch();

					stopwatch.Start();
					var meshModel = MeshModel.Deserialize( fsOriginal );
					stopwatch.Stop();
					var stopwatchElapsed = stopwatch.Elapsed;
					TestContext.Out.WriteLine($"Deserialization: {Convert.ToInt32(stopwatchElapsed.TotalMilliseconds)} ms");

					stopwatch.Start();
					meshModel.Serialize( fsReWritten );
					stopwatch.Stop();
					stopwatchElapsed = stopwatch.Elapsed;
					TestContext.Out.WriteLine($"Serialization: {Convert.ToInt32(stopwatchElapsed.TotalMilliseconds)} ms");
				}
			}

			reWrittenInfo.Refresh();

			// Then
			Assert.That( reWrittenInfo.Length, Is.EqualTo( originalInfo.Length ) );

			var byteWiseEqual = true;
			using( var fsOriginal = originalInfo.OpenRead() )
			{
				using( var fsReWritten = reWrittenInfo.OpenRead() )
				{
					var originalBuffer = new byte[ originalInfo.Length ];
					var reWrittenBuffer = new byte[ originalInfo.Length ];
					fsOriginal.Read( originalBuffer, 0, originalBuffer.Length );
					fsReWritten.Read( reWrittenBuffer, 0, reWrittenBuffer.Length );

					for( var i = 0; i < originalBuffer.Length; ++i )
					{
						byteWiseEqual &= originalBuffer[ i ] == reWrittenBuffer[ i ];
						if( !byteWiseEqual )
							break;
					}
				}
			}

			Assert.That( byteWiseEqual, Is.True );
		}

		[Test, Description( "Given: CAD information, When: Create MeshModel in code, Then: MeshModel is complete." )]
		public void ConstructionTest()
		{
			// Given
			var meshModel = new MeshModel(
				new MeshModelMetadata( partCount: 2 ),
				new[]
				{
					new MeshModelPart(
						new MeshModelMetadata(),
						CreateExampleMeshes(),
						CreateExampleEdges(),
						CreateExampleMeshValueLists() ),
					new MeshModelPart(
						new MeshModelMetadata(),
						CreateExampleMeshes(),
						CreateExampleEdges(),
						CreateExampleMeshValueLists() )
				} );

			// Then
			Assert.That( meshModel.Metadata.PartCount, Is.EqualTo( meshModel.Parts.Count ) );
			Assert.That( meshModel.Metadata.MeshValueEntries.Length, Is.EqualTo( 0 ) );
			Assert.That( meshModel.Parts[ 0 ].Meshes.Length, Is.EqualTo( 1 ) );
			Assert.That( meshModel.Parts[ 0 ].Edges.Length, Is.EqualTo( 1 ) );
		}

		private static ColorScale CreateGreyscale()
		{
			return new ColorScale( "Greyscale",
				Color.FromArgb( 255, 255, 0, 255 ), new[]
				{
					new ColorScaleEntry( 0, Color.FromArgb( 255, 0, 0, 0 ) ),
					new ColorScaleEntry( 1, Color.FromArgb( 255, 255, 255, 255 ) )
				},
				ColorScaleInterpolation.RGB );
		}

		private static Edge CreateSquareEdge()
		{
			return new Edge( new[]
				{
					new Vector3F( 0, 0, 0 ),
					new Vector3F( 1, 0, 0 ),
					new Vector3F( 1, 0, 0 ),
					new Vector3F( 1, 1, 0 ),
					new Vector3F( 1, 1, 0 ),
					new Vector3F( 0, 1, 0 ),
					new Vector3F( 0, 1, 0 ),
					new Vector3F( 0, 0, 0 )
				},
				Color.FromArgb( 255, 0, 0, 0 ) );
		}

		private static MeshValue CreateRandomMeshValuesFrom0To1( int count )
		{
			var values = new float[ count ];
			var random = new Random();

			for( var i = 0; i < count; ++i )
			{
				values[ i ] = random.Next( 0, 100 ) / 100f;
			}

			return new MeshValue( values );
		}

		private static Mesh CreateQuad( float scale, Vector3F offset )
		{
			return new Mesh(
				0,
				new[]
				{
					new Vector3F( 0, 0, 0 ) * scale + offset,
					new Vector3F( 1, 0, 0 ) * scale + offset,
					new Vector3F( 1, 1, 0 ) * scale + offset,
					new Vector3F( 0, 1, 0 ) * scale + offset
				},
				new[]
				{
					new Vector3F( 0, 0, 1 ),
					new Vector3F( 0, 0, 1 ),
					new Vector3F( 0, 0, 1 ),
				},
				new[] { 0, 1, 2, 0, 2, 3 },
				new[]
				{
					new Vector2F( 0, 0 ),
					new Vector2F( 1, 0 ),
					new Vector2F( 1, 1 ),
					new Vector2F( 0, 1 )
				},
				Color.FromArgb( 100, 100, 100, 100 ),
				new[]
				{
					Color.FromArgb( 255, 255, 0, 0 ),
					Color.FromArgb( 255, 0, 255, 0 ),
					Color.FromArgb( 255, 0, 0, 255 ),
					Color.FromArgb( 255, 255, 255, 255 )
				} );
		}

		private static IEnumerable<Mesh> CreateExampleMeshes()
		{
			return new[]
			{
				CreateQuad( 100, new Vector3F() )
			};
		}

		private static IEnumerable<Edge> CreateExampleEdges()
		{
			return new[]
			{
				CreateSquareEdge()
			};
		}

		private static IEnumerable<MeshValueList> CreateExampleMeshValueLists()
		{
			return new[]
			{
				new MeshValueList( new[]
					{
						CreateRandomMeshValuesFrom0To1( 4 )
					},
					new MeshValueEntry( "floats", MeshModelTestFile, CreateGreyscale() ) )
			};
		}

		#endregion
	}
}