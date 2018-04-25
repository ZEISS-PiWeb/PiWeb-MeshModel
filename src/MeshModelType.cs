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
	/// Describes the type of a <see cref="MeshModel"/>.
	/// </summary>
	public enum MeshModelType
	{
		/// <summary>
		/// The Meshmodel consists of a single <see cref="MeshModelPart"/>.
		/// </summary>
		Part,

		/// <summary>
		/// The Meshmodel a composition of multiple <see cref="MeshModelPart"/>s.
		/// </summary>
		Composite
	}
}