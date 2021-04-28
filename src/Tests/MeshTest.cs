#region copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss Industrielle Messtechnik GmbH        */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2021                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

using System;
using System.Collections.Generic;
using System.IO;

namespace Zeiss.PiWeb.MeshModel.Tests
{
	#region usings

	using System.Diagnostics;
	using NUnit.Framework;
	using Zeiss.PiWeb.MeshModel;

	#endregion

	[TestFixture]
	public class MeshTest
	{
		#region members

		private static readonly string MeshModelTestFile = Path.Combine(
			TestContext.CurrentContext.TestDirectory,
			"TestData",
			"test_deviation.meshModel" );

		private static readonly string MeshModelReWrittenFile = $"{MeshModelTestFile}.rewritten.meshModel";

		#endregion

		#region methods

		internal static Vector3F[] Create4Positions()
		{
			return new[]
			{
				new Vector3F( 0f, 0f, 0f ),
				new Vector3F( 0f, 1f, 0f ),
				new Vector3F( 1f, 1f, 0f ),
				new Vector3F( 0f, 0f, 1f )
			};
		}

		internal static Vector3F[] Create4Normals()
		{
			var normals = new[]
			{
				new Vector3F( -1f, -1f, -1f ),
				new Vector3F( 0f, 1f, 0f ),
				new Vector3F( 1f, 1f, 0f ),
				new Vector3F( 0f, 0f, 1f )
			};
			normals[ 0 ].Normalize();
			return normals;
		}

		internal static Vector2F[] Create4UVs()
		{
			return new[]
			{
				new Vector2F( 0f, 0f ),
				new Vector2F( 0f, 1f ),
				new Vector2F( 1f, 1f ),
				new Vector2F( 0f, 0f )
			};
		}

		internal static Color[] Create4Colors()
		{
			return new[]
			{
				Color.FromArgb( 0, 100, 0, 0 ),
				Color.FromArgb( 10, 10, 10, 10 ),
				Color.FromArgb( 100, 0, 255, 0 ),
				Color.FromArgb( 255, 0, 50, 50 )
			};
		}

		internal static int[] Create4Triangles()
		{
			return new[] { 0, 1, 2, 0, 3, 1, 0, 2, 3, 0, 3, 1 };
		}

		[Test, Description( "Checks if the getters return the correct results." )]
		public void GetterTest()
		{
			// ..................................................................................... GIVEN
			var positions = Create4Positions();
			var normals = Create4Normals();
			var triangleIndices = Create4Triangles();


			// ..................................................................................... WHEN
			var mesh = new Mesh( 0, positions, normals, triangleIndices );


			// ..................................................................................... THEN
			// Counts
			Assert.AreEqual( 4, mesh.TriangleCount );
			Assert.AreEqual( 12, mesh.TriangleIndicesCount );

			// Vertices
			Assert.AreEqual( positions[ 0 ], mesh.Positions[ 0 ] );
			Assert.AreEqual( positions[ 1 ], mesh.Positions[ 1 ] );
			Assert.AreEqual( positions[ 2 ], mesh.Positions[ 2 ] );
			Assert.AreEqual( positions[ 3 ], mesh.Positions[ 3 ] );

			// Normals
			Assert.AreEqual( normals[ 0 ], mesh.Normals[ 0 ] );
			Assert.AreEqual( normals[ 1 ], mesh.Normals[ 1 ] );
			Assert.AreEqual( normals[ 2 ], mesh.Normals[ 2 ] );
			Assert.AreEqual( normals[ 3 ], mesh.Normals[ 3 ] );


			// Triangle Normals
			Assert.AreEqual( ( normals[ 0 ] + normals[ 1 ] + normals[ 2 ] ) / 3f,
				mesh.GetTriangle( 0 ).MeanNormal );

			// Triangle Vertices
			Assert.AreEqual( positions[ 0 ], mesh.GetTriangle( 0 ).VertexA );
			Assert.AreEqual( positions[ 1 ], mesh.GetTriangle( 0 ).VertexB );
			Assert.AreEqual( positions[ 2 ], mesh.GetTriangle( 0 ).VertexC );
		}

		[Test, Description( "Given: Test file, When: imported, Then: Imported values are correct." )]
		public void ImportTest()
		{
			// ..................................................................................... GIVEN
			using var file = File.OpenRead( MeshModelTestFile );


			// ..................................................................................... WHEN
			var meshModel = MeshModel.Deserialize( file );


			// ..................................................................................... THEN
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
		public void ImportExportTest()
		{
			// ..................................................................................... GIVEN
			var originalInfo = new FileInfo( MeshModelTestFile );
			var reWrittenInfo = new FileInfo( MeshModelReWrittenFile );


			// ..................................................................................... WHEN
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


			// ..................................................................................... THEN
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
			// ..................................................................................... WHEN
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


			// ..................................................................................... THEN
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