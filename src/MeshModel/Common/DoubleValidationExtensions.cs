#region copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss Industrielle Messtechnik GmbH        */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2021                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

namespace Zeiss.PiWeb.MeshModel.Common
{
	#region usings

	using System;
	using System.Runtime.CompilerServices;

	#endregion

	/// <summary>
	/// Provides extension methods for validation of double values.
	/// </summary>
	public static class DoubleValidationExtensions
	{
		#region methods

		/// <summary>
		/// Checks wehther a double value is a finite number (is not NaN or Infinity).
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns><see langword="true"/>, if the value is a finite number; otherwise <see langword="false"/>.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool IsFinite( this double value )
		{
			// TODO: This method is duplicated from double implementation that is available in .Net Standard 2.1 and .Net Core
			var bits = BitConverter.DoubleToInt64Bits( value );
			return ( bits & 0x7FFFFFFFFFFFFFFF ) < 0x7FF0000000000000;
		}

		#endregion
	}
}