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
	using NUnit.Framework;

	#endregion

	[TestFixture]
	public class Vector3FTest
	{
		#region methods

		[Test, Description( "Checks if the vector is normalized correctly." )]
		public void VectorNormalizingTest()
		{
			// ------------------------------------------------------------------------------ GIVEN
			var a = new Vector3F( 1, 1, 1 );
			a.Normalize();

			var b = new Vector3F( 0, 0, 4 );
			b.Normalize();

			var c = new Vector3F( 0, 0, 4 );
			c.Normalize();

			// ------------------------------------------------------------------------------- THEN
			Assert.AreEqual( new Vector3F( 1, 1, 1 ) / (float)Math.Sqrt( 3 ), a );
			Assert.AreEqual( new Vector3F( 0, 0, 4 ) / (float)Math.Sqrt( 16 ), b );
			Assert.AreEqual( new Vector3F( 0, 0, 4 ) / (float)Math.Sqrt( 16 ), c );
		}

		#endregion
	}
}