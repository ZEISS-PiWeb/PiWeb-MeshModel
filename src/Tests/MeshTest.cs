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
				new Vector3F( -1f, -1f, -1f), 
				new Vector3F( 0f, 1f, 0f ), 
				new Vector3F( 1f, 1f, 0f ), 
				new Vector3F( 0f, 0f, 1f )
			};
			normals[0].Normalize();
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