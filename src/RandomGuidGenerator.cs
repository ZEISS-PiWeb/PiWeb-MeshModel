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
	/// Implements random uuid generation.
	/// </summary>
	internal class RandomGuidGenerator : IGuidGenerator
	{
		#region interface IGuidGenerator

		/// <summary>
		/// Generates a single random uuid on each call.
		/// </summary>
		/// <returns>The generated uuid.</returns>
		public Guid CreateGuid()
		{
			return Guid.NewGuid();
		}

		#endregion
	}
}