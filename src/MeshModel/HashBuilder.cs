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

	using System.IO;
	using System.Security.Cryptography;

	#endregion

	/// <summary>
	/// This class is used to calculate MD5 hashes. Since MD5 is not FIPS compliant, the .NET implementation will not work on
	/// systems that have enforced FIPS compliance. On the other hand, the .NET implementation is much faster than the RFC
	/// implementation, so we decide which algorith to use right before calculating the hash.
	/// </summary>
	internal static class HashBuilder
	{
		#region methods

		internal static HashAlgorithm CreateHashAlgorithm()
		{
			return CryptoConfig.AllowOnlyFipsAlgorithms ? (HashAlgorithm)new MD5Managed() : MD5.Create();
		}

		internal static byte[] ComputeHash( Stream stream )
		{
			using var hashAlgorithm = CreateHashAlgorithm();
			return hashAlgorithm.ComputeHash( stream );
		}

		internal static byte[] ComputeHash( byte[] buffer )
		{
			using var hashAlgorithm = CreateHashAlgorithm();
			return hashAlgorithm.ComputeHash( buffer );
		}

		#endregion
	}
}