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
	using System.Linq;
	using NUnit.Framework;
	using Zeiss.PiWeb.MeshModel;

	#endregion

	[TestFixture]
	public class TriangulationHashTest
	{
		#region methods

		[Test]
		public void TriangulationHash_CalculatesProperly()
		{
			var positions = new[]
			{
				new Vector3F( 0f, 0f, 0f ), 
				new Vector3F(0f, 1f, 0f), 
				new Vector3F(1f, 1f, 0f), 
				new Vector3F(0f, 0f, 1f)
			};
			var normals = new[]
			{
				new Vector3F(-1f, -1f, -1f), 
				new Vector3F(0f, 1f, 0f), 
				new Vector3F(1f, 1f, 0f), 
				new Vector3F(0f, 0f, 1f)
			};
			var triangleIndices1 = new[] { 0, 1, 2, 0, 3, 1, 0, 2, 3, 0, 3, 1 };
			var triangleIndices2 = new[] { 3, 1, 2, 1, 2, 1, 0, 2, 1, 0, 3, 2 };
			var mesh1 = new Mesh( 0, positions, normals, triangleIndices1 );
			var mesh2 = new Mesh( 0, positions, normals, triangleIndices2 );

			var meshModelPart1 = new MeshModelPart( new MeshModelMetadata(), new[] { mesh1 } );
			var meshModelPart2 = new MeshModelPart( new MeshModelMetadata(), new[] { mesh2 } );
			var meshModelPart3 = new MeshModelPart( new MeshModelMetadata(), new[] { mesh1, mesh2 } );

			Assert.That( meshModelPart1.Metadata.TriangulationHash, Is.EqualTo( new Guid( "10800c7b-6d77-7050-c2d2-a015cc70fdb3" ) ) );
			Assert.That( meshModelPart2.Metadata.TriangulationHash, Is.EqualTo( new Guid( "f01db9d6-4ae9-5747-b2df-6ed0bf2e6f8c" ) ) );
			Assert.That( meshModelPart3.Metadata.TriangulationHash, Is.EqualTo( new Guid( "cad6e562-c877-344a-00c9-745bcdb6a41a" ) ) );
		}

		[Test]
		public void TriangulationHash_CalculatesProperly_ForLargeMeshes()
		{
			var positions = new[]
			{
				new Vector3F( 0f, 0f, 0f ), 
				new Vector3F(0f, 1f, 0f), 
				new Vector3F(1f, 1f, 0f), 
				new Vector3F(0f, 0f, 1f)
			};
			var normals = new[]
			{
				new Vector3F(-1f, -1f, -1f), 
				new Vector3F(0f, 1f, 0f), 
				new Vector3F(1f, 1f, 0f), 
				new Vector3F(0f, 0f, 1f)
			};
			var triangleIndices = new[] { 0, 1, 2, 0, 3, 1, 0, 2, 3, 0, 3, 1 };
			var largeTriangleList = Enumerable.Range( 0, 10 * 1024 ).SelectMany( _ => triangleIndices ).ToArray();

			var mesh = new Mesh( 0, positions, normals, largeTriangleList );
			var meshModelPart = new MeshModelPart( new MeshModelMetadata(), new[] { mesh } );

			Assert.That( meshModelPart.Metadata.TriangulationHash, Is.EqualTo( new Guid( "69c431dc-e986-4703-1f1e-eb5ce52ebc0a" ) ) );
		}

		[Test, Description( "This test makes sure, that the triangulation has does only depend on the triangle indices and not on positions or normals." )]
		public void TriangulationHash_DoesNotDependOn_PositionsOrNormals()
		{
			var positions1 = new[]
			{
				new Vector3F( 0f, 0f, 0f ), 
				new Vector3F(0f, 1f, 0f), 
				new Vector3F(1f, 1f, 0f), 
				new Vector3F(0f, 0f, 1f)
			};
			var normals1 = new[]
			{
				new Vector3F(-1f, -1f, -1f), 
				new Vector3F(0f, 1f, 0f), 
				new Vector3F(1f, 1f, 0f), 
				new Vector3F(0f, 0f, 1f)
			};
			var positions2 = new[]
			{
				new Vector3F( 0f, 0f, 0f ), 
				new Vector3F(0f, 1f, 0f), 
				new Vector3F(1f, 1f, 0f), 
				new Vector3F(0f, 0f, 1f)
			};
			var normals2 = new[]
			{
				new Vector3F(-1f, -1f, -1f), 
				new Vector3F(0f, 1f, 0f), 
				new Vector3F(1f, 1f, 0f), 
				new Vector3F(0f, 0f, 1f)
			};
			var triangleIndices = new[] { 0, 1, 2, 0, 3, 1, 0, 2, 3, 0, 3, 1 };

			var mesh1 = new Mesh( 0, positions1, normals1, triangleIndices );
			var mesh2 = new Mesh( 0, positions2, normals1, triangleIndices );
			var mesh3 = new Mesh( 0, positions1, normals2, triangleIndices );
			var mesh4 = new Mesh( 0, positions2, normals2, triangleIndices );

			var meshModelPart1 = new MeshModelPart( new MeshModelMetadata(), new[] { mesh1 } );
			var meshModelPart2 = new MeshModelPart( new MeshModelMetadata(), new[] { mesh2 } );
			var meshModelPart3 = new MeshModelPart( new MeshModelMetadata(), new[] { mesh3 } );
			var meshModelPart4 = new MeshModelPart( new MeshModelMetadata(), new[] { mesh4 } );

			// All hashes have to have the same value
			Assert.That( meshModelPart1.Metadata.TriangulationHash, Is.EqualTo( meshModelPart2.Metadata.TriangulationHash ) );
			Assert.That( meshModelPart1.Metadata.TriangulationHash, Is.EqualTo( meshModelPart3.Metadata.TriangulationHash ) );
			Assert.That( meshModelPart1.Metadata.TriangulationHash, Is.EqualTo( meshModelPart4.Metadata.TriangulationHash ) );
		}

		[Test, Description( "This test makes sure, that the triangulation depends on all meshes inside a part." )]
		public void TriangulationHash_IsCalculated_AcrossAllMeshes()
		{
			var positions = new[]
			{
				new Vector3F( 0f, 0f, 0f ), 
				new Vector3F(0f, 1f, 0f), 
				new Vector3F(1f, 1f, 0f), 
				new Vector3F(0f, 0f, 1f)
			};
			var normals = new[]
			{
				new Vector3F(-1f, -1f, -1f), 
				new Vector3F(0f, 1f, 0f), 
				new Vector3F(1f, 1f, 0f), 
				new Vector3F(0f, 0f, 1f)
			};
			var triangleIndices1 = new[] { 0, 1, 2, 0, 3, 1, 0, 2, 3, 0, 3, 1 };
			var triangleIndices2 = new[] { 3, 1, 2, 1, 3, 1, 0, 2, 3, 0, 3, 1 };
			var triangleIndices3 = new[] { 1, 0, 2, 0, 3, 1, 0, 2, 3, 0, 3, 1 };

			var mesh1 = new Mesh( 0, positions, normals, triangleIndices1 );
			var mesh2 = new Mesh( 0, positions, normals, triangleIndices2 );
			var mesh3 = new Mesh( 0, positions, normals, triangleIndices3 );

			var meshModelPart1 = new MeshModelPart( new MeshModelMetadata(), new[] { mesh1 } );
			var meshModelPart2 = new MeshModelPart( new MeshModelMetadata(), new[] { mesh1, mesh2 } );
			var meshModelPart3 = new MeshModelPart( new MeshModelMetadata(), new[] { mesh1, mesh2, mesh3 } );

			// All hashes have to be different
			Assert.That( meshModelPart1.Metadata.TriangulationHash, Is.Not.EqualTo( meshModelPart2.Metadata.TriangulationHash ) );
			Assert.That( meshModelPart1.Metadata.TriangulationHash, Is.Not.EqualTo( meshModelPart2.Metadata.TriangulationHash ) );
			Assert.That( meshModelPart1.Metadata.TriangulationHash, Is.Not.EqualTo( meshModelPart3.Metadata.TriangulationHash ) );
			Assert.That( meshModelPart2.Metadata.TriangulationHash, Is.Not.EqualTo( meshModelPart3.Metadata.TriangulationHash ) );
		}

		#endregion
	}
}