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

	using System.IO;
	using NUnit.Framework;
	using Zeiss.PiWeb.ColorScale;

	#endregion

	[TestFixture]
	public class ReaderWriterExtensionsTest
	{
		#region constants

		private const int ArrayHeaderSize = 4;

		#endregion

		#region methods

		[Test, Description( "Given: float array, When: writing to binary stream, Then: output is correct." )]
		public void TestWritingFloatArray()
		{
			// ## Given ##
			var floats = new[] { 1f, 2f, 3f, 4f, 5f, 6f };
			var bytes = new byte[ floats.Length * sizeof( float ) + ArrayHeaderSize ];
			float[] readFloats;


			// ## When ##
			using( var stream = new MemoryStream( bytes ) )
			{
				using var binaryWriter = new BinaryWriter( stream );
				binaryWriter.WriteArray( FloatIo.Instance, floats );
			}

			using( var stream = new MemoryStream( bytes ) )
			{
				using var binaryReader = new BinaryReader( stream );
				readFloats = binaryReader.ReadLengthAndArray( FloatIo.Instance );
			}


			// ## Then ##
			Assert.That( readFloats, Is.EquivalentTo( floats ) );
		}

		[Test, Description( "Given: vector array, When: writing to binary stream, Then: output is correct." )]
		public void TestWritingVector3FArray()
		{
			// ## Given ##
			var vectors = MeshTest.Create4Positions();
			var bytes = new byte[ vectors.Length * sizeof( float ) * 3 + ArrayHeaderSize ];
			Vector3F[] readVectors;


			// ## When ##
			using( var stream = new MemoryStream( bytes ) )
			{
				using var binaryWriter = new BinaryWriter( stream );
				binaryWriter.WriteArray( Vector3FIo.Instance, vectors );
			}

			using( var stream = new MemoryStream( bytes ) )
			{
				using var binaryReader = new BinaryReader( stream );
				readVectors = binaryReader.ReadLengthAndArray( Vector3FIo.Instance );
			}


			// ## Then ##
			Assert.That( readVectors, Is.EquivalentTo( vectors ) );
		}

		[Test, Description( "Given: vector array, When: writing to binary stream, Then: output is correct." )]
		public void TestWritingVector2FArray()
		{
			// ## Given ##
			var vectors = MeshTest.Create4UVs();
			var bytes = new byte[ vectors.Length * sizeof( float ) * 2 + ArrayHeaderSize ];
			Vector2F[] readVectors;


			// ## When ##
			using( var stream = new MemoryStream( bytes ) )
			{
				using var binaryWriter = new BinaryWriter( stream );
				binaryWriter.WriteArray( Vector2FIo.Instance, vectors );
			}

			using( var stream = new MemoryStream( bytes ) )
			{
				using var binaryReader = new BinaryReader( stream );
				readVectors = binaryReader.ReadLengthAndArray( Vector2FIo.Instance );
			}


			// ## Then ##
			Assert.That( readVectors, Is.EquivalentTo( vectors ) );
		}

		[Test, Description( "Given: color array, When: writing to binary stream, Then: output is correct." )]
		public void TestWritingColorArray()
		{
			// ## Given ##
			var colors = MeshTest.Create4Colors();
			var bytes = new byte[ colors.Length * sizeof( byte ) * 4 + ArrayHeaderSize ];
			Color[] readColors;


			// ## When ##
			using( var stream = new MemoryStream( bytes ) )
			{
				using var binaryWriter = new BinaryWriter( stream );
				binaryWriter.WriteArray( ColorIo.Instance, colors );
			}

			using( var stream = new MemoryStream( bytes ) )
			{
				using var binaryReader = new BinaryReader( stream );
				readColors = binaryReader.ReadLengthAndArray( ColorIo.Instance );
			}


			// ## Then ##
			Assert.That( readColors, Is.EquivalentTo( colors ) );
		}

		[Test, Description( "Given: double array, When: writing to binary stream, Then: output is correct." )]
		public void TestReadingDoubleArrayAsVector3F()
		{
			// ## Given ##
			var doubles = new[] { 1.2, 1.3, 2.0, 3.0, 6.0, 8.5 };
			var bytes = new byte[ doubles.Length * sizeof( double ) + ArrayHeaderSize ];
			var expectedVectors = new[] { new Vector3F( 1.2f, 1.3f, 2.0f ), new Vector3F( 3.0f, 6.0f, 8.5f ) };
			Vector3F[] readVectors;


			// ## When ##
			using( var stream = new MemoryStream( bytes ) )
			{
				using var binaryWriter = new BinaryWriter( stream );
				binaryWriter.Write( doubles.Length / 3 );
				foreach( var d in doubles )
				{
					binaryWriter.Write( d );
				}
			}

			using( var stream = new MemoryStream( bytes ) )
			{
				using var binaryReader = new BinaryReader( stream );
				readVectors = binaryReader.ReadDoubleArrayAsVector3FArray();
			}


			// ## Then ##
			Assert.That( readVectors, Is.EquivalentTo( expectedVectors ) );
		}

		#endregion
	}
}