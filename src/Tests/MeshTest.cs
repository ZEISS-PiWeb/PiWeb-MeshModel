#region Copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss IMT (IZfM Dresden)                   */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2018                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

namespace Zeiss.PiWeb.MeshModel.Tests
{
	#region usings

	using NUnit.Framework;
	using Zeiss.PiWeb.MeshModel;

	#endregion

	[TestFixture]
	public class MeshTest
	{
		#region methods

		// [OneTimeSetUp]
		// public void Setup()
		// {
		// 	using var file = File.OpenRead(@"C:\temp\blech1.meshModel");
		// 	var meshModel = MeshModel.Deserialize( file );
		// }

		private static Vector3F[] Create4Positions()
		{
			return new[]
			{
				new Vector3F( 0f, 0f, 0f ), 
				new Vector3F( 0f, 1f, 0f ), 
				new Vector3F( 1f, 1f, 0f ), 
				new Vector3F( 0f, 0f, 1f )
			};
		}
		
		private static Vector3F[] Create4Normals()
		{
			var normals = new[]
			{
				new Vector3F( -1f, -1f, -1f), 
				new Vector3F( 0f, 1f, 0f ), 
				new Vector3F( 1f, 1f, 0f ), 
				new Vector3F( 0f, 0f, 1f )
			};
			normals[0].Normalize();
			return normals;
		}

		private static int[] Create4Triangles()
		{
			return new[] { 0, 1, 2, 0, 3, 1, 0, 2, 3, 0, 3, 1 };
		}
		
		[Test, Description("Checks if the getters return the correct results.")]
		public void GetterTest()
		{
			// ------------------------------------------------------------------------------ GIVEN
			var positions = Create4Positions();
			var normals = Create4Normals();
			var triangleIndices = Create4Triangles();
			var mesh = new Mesh( 0, positions, normals, triangleIndices );


			// ------------------------------------------------------------------------------- THEN
			// Counts
			Assert.AreEqual( 4, mesh.TriangleCount );
			Assert.AreEqual( 12,  mesh.TriangleIndicesCount );

			// Vertices
			Assert.AreEqual( positions[0], mesh.Positions[ 0 ] );
			Assert.AreEqual( positions[1], mesh.Positions[ 1 ] );
			Assert.AreEqual( positions[2], mesh.Positions[ 2 ] );
			Assert.AreEqual( positions[3], mesh.Positions[ 3 ] );

			// Normals
			Assert.AreEqual( normals[0], mesh.Normals[0] );
			Assert.AreEqual( normals[1], mesh.Normals[1] );
			Assert.AreEqual( normals[2], mesh.Normals[2] );
			Assert.AreEqual( normals[3], mesh.Normals[3] );
			

			// Triangle Normals
			Assert.AreEqual( (normals[0] + normals[1] + normals[2]) / 3f,
				mesh.GetTriangle( 0 ).MeanNormal );

			// Triangle Vertices
			Assert.AreEqual( positions[0], mesh.GetTriangle( 0 ).VertexA );
			Assert.AreEqual( positions[1], mesh.GetTriangle( 0 ).VertexB );
			Assert.AreEqual( positions[2], mesh.GetTriangle( 0 ).VertexC );
		}

		#endregion
	}
}