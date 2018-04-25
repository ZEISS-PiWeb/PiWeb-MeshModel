#region Copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss IMT (IZfM Dresden)                   */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2018                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion


namespace Zeiss.IMT.PiWeb.MeshModel
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