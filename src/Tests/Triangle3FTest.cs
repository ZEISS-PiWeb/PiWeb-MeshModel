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
	public class Triangle3FTest
	{
		#region methods

		[Test, Description( "Checks if normal is calculated correctly." )]
		public void NormalCalculationTest()
		{
			// ------------------------------------------------------------------------------ GIVEN
			var a = new Vector3F( 0, 0, 0 );
			var b = new Vector3F( 2, 0, 0 );
			var c = new Vector3F( 0, 2, 0 );

			var t = new Triangle3F( a, b, c );

			// ------------------------------------------------------------------------------- THEN
			Assert.True( t.Normal.Z > 0 );
			Assert.AreEqual( 1, t.Normal.Z );
			Assert.AreEqual( new Vector3F( 0, 0, 1 ), t.Normal );
		}

		[Test, Description( "Checks if normal is calculated correctly." )]
		public void AreaCalculationTest()
		{
			// ------------------------------------------------------------------------------ GIVEN
			var a = new Vector3F( 0, 0, 0 );
			var b = new Vector3F( 1, 0, 0 );
			var c = new Vector3F( 0, 1, 0 );

			var t = new Triangle3F( a, b, c );

			// ------------------------------------------------------------------------------- THEN
			Assert.True( t.Area > 0 );
			Assert.AreEqual( 0.5f, t.Area );
		}

		#endregion
	}
}