#region copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss Industrielle Messtechnik GmbH        */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2021                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

namespace Zeiss.PiWeb.MeshModel
{
	/// <summary>
	/// Determines how to interpolate between two colors
	/// </summary>
	public enum ColorScaleInterpolation
	{
		/// <summary>
		/// Interpolates between the RGB values of two colors
		/// </summary>
		RGB,

		/// <summary>
		/// Interpolates between the HSV values of two colors
		/// </summary>
		HSV
	}
}