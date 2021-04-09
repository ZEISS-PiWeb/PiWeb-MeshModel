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

		[Test, Description("Checks if the getters return the correct results.")]
		public void GetterTest()
		{
			// ------------------------------------------------------------------------------ GIVEN
			var positions = new[] { 0f, 0f, 0f, 0f, 1f, 0f, 1f, 1f, 0f, 0f, 0f, 1f };
			var normals = new[] { -1f, -1f, -1f, 0f, 1f, 0f, 1f, 1f, 0f, 0f, 0f, 1f };
			var triangleIndices = new[] { 0, 1, 2, 0, 3, 1, 0, 2, 3, 0, 3, 1 };
			var mesh = new Mesh( 0, positions, normals, triangleIndices );


			// ------------------------------------------------------------------------------- THEN
			// Counts
			Assert.AreEqual( 4, mesh.TriangleCount );
			Assert.AreEqual( 12,  mesh.TriangleIndicesCount );

			// Vertices
			Assert.AreEqual( 0, mesh.GetVertex( 0 ).X );
			Assert.AreEqual( 0, mesh.GetVertex( 0 ).Y );
			Assert.AreEqual( 0, mesh.GetVertex( 0 ).Z );
			Assert.AreEqual( 0, mesh.GetVertex( 1 ).X );
			Assert.AreEqual( 1, mesh.GetVertex( 1 ).Y );
			Assert.AreEqual( 0, mesh.GetVertex( 1 ).Z );
			Assert.AreEqual( 1, mesh.GetVertex( 2 ).X );
			Assert.AreEqual( 1, mesh.GetVertex( 2 ).Y );
			Assert.AreEqual( 0, mesh.GetVertex( 2 ).Z );
			Assert.AreEqual( 0, mesh.GetVertex( 3 ).X );
			Assert.AreEqual( 0, mesh.GetVertex( 3 ).Y );
			Assert.AreEqual( 1, mesh.GetVertex( 3 ).Z );

			// Normals
			Assert.AreEqual( -1, mesh.GetVertex( 0 ).NormalX );
			Assert.AreEqual( -1, mesh.GetVertex( 0 ).NormalY );
			Assert.AreEqual( -1, mesh.GetVertex( 0 ).NormalZ );
			Assert.AreEqual( 0, mesh.GetVertex( 1 ).NormalX );
			Assert.AreEqual( 1, mesh.GetVertex( 1 ).NormalY );
			Assert.AreEqual( 0, mesh.GetVertex( 1 ).NormalZ );

			// Triangle Normals
			Assert.AreEqual( new Vector3F( ( -1f + 0f + 1f ) / 3f, ( -1f + 1f + 1f ) / 3f, ( -1f + 0f + 0f ) / 3f ),
				mesh.GetTriangle( 0 ).MeanNormal );

			// Triangle Vertices
			Assert.AreEqual( 0, mesh.GetTriangle( 0 ).VertexA.X );
			Assert.AreEqual( 0, mesh.GetTriangle( 0 ).VertexA.Y );
			Assert.AreEqual( 0, mesh.GetTriangle( 0 ).VertexA.Z );
			Assert.AreEqual( 0, mesh.GetTriangle( 0 ).VertexB.X );
			Assert.AreEqual( 1, mesh.GetTriangle( 0 ).VertexB.Y );
			Assert.AreEqual( 0, mesh.GetTriangle( 0 ).VertexB.Z );
			Assert.AreEqual( 1, mesh.GetTriangle( 0 ).VertexC.X );
			Assert.AreEqual( 1, mesh.GetTriangle( 0 ).VertexC.Y );
			Assert.AreEqual( 0, mesh.GetTriangle( 0 ).VertexC.Z );
		}

		#endregion
	}
}