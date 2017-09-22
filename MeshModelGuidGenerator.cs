#region Copyright
/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss IMT (IZfM Dresden)                   */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2011                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */
#endregion

namespace Zeiss.IMT.PiWeb.Meshmodels
{
	#region using

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Reflection;
	using System.Security.Cryptography;

	#endregion

	/// <summary>
	/// Helper class to generate stable guids for MeshModel-files with versions older than 3.1.
	/// </summary>
	internal static class MeshModelGuidGenerator
	{
		#region methods

		public static Guid GenerateGuids( IEnumerable<ZipArchiveEntry> entries )
		{
			var stream = new MemoryStream();
			foreach( var entry in entries.OrderBy( x => x.Name, StringComparer.Ordinal ) )
			{
				stream.Write( BitConverter.GetBytes( entry.CompressedLength ), 0, 8 );
				stream.Write( BitConverter.GetBytes( entry.Length ), 0, 8 );
				stream.Write( BitConverter.GetBytes( GetChecksum( entry ) ), 0, 4 );
			}

			stream.Position = 0;
			return new Guid( HashBuilder.ComputeHash( stream ) );
		}

		private static uint GetChecksum( ZipArchiveEntry entry )
		{
			var crcField = typeof( ZipArchiveEntry ).GetField( "_crc32", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance );
			return (uint)crcField.GetValue( entry );
		}

		#endregion
	}
}