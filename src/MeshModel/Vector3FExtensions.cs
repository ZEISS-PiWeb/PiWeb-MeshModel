#region copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss Industrielle Messtechnik GmbH        */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2021                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

// ReSharper disable NonReadonlyMemberInGetHashCode
namespace Zeiss.PiWeb.MeshModel
{
	#region usings

	using System;

	#endregion

	public static class Vector3FExtensions
	{
		#region methods

		public static Vector3F NormalizeExtended( this Vector3F vector )
		{
			return vector / (float)Math.Sqrt( vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z );
		}

		#endregion
	}
}