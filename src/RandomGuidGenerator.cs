#region Copyright

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