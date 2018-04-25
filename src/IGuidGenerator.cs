#region copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss IMT (IZfM Dresden)                   */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2018                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

namespace Zeiss.IMT.PiWeb.MeshModel
{
	#region usings

	using System;

	#endregion

	/// <summary>
	/// Interface to generate uuids.
	/// </summary>
	internal interface IGuidGenerator
	{
		#region methods

		/// <summary>
		/// Generates a single uuid.
		/// </summary>
		Guid CreateGuid();

		#endregion
	}
}