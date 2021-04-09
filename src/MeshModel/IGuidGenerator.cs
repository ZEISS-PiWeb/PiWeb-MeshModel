#region copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss Industrielle Messtechnik GmbH        */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2021                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

namespace Zeiss.PiWeb.MeshModel
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