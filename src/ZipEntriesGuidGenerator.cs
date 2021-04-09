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
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Reflection;

	#endregion

	/// <summary>
	/// Implements uuid generation for a number of zip file entries based on their internal checksum.
	/// 
	/// This class is internal for now because it uses private fields of datatypes defined in the zip archive library.
	/// We can not guarantee this will still work in the future. 
	/// </summary>
	internal class ZipEntriesGuidGenerator : IGuidGenerator
	{
		#region members

		private readonly IEnumerable<ZipArchiveEntry> _Entries;

		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ZipEntriesGuidGenerator" /> class.
		/// </summary>
		/// <param name="entries">The relevant entries of a zip archive.</param>
		public ZipEntriesGuidGenerator( IEnumerable<ZipArchiveEntry> entries )
		{
			if( entries == null )
				throw new ArgumentNullException( nameof(entries) );

			_Entries = entries;
		}

		#endregion

		#region methods

		/// <summary>
		/// Generates a uuid for a number of zip file entries based on their internal checksum.
		/// </summary>
		public static Guid GenerateGuidStatic( IEnumerable<ZipArchiveEntry> entries )
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
			return ( uint ) crcField.GetValue( entry );
		}

		#endregion

		#region interface IGuidGenerator

		/// <summary>
		/// Generates a uuid for a number of zip file entries based on their internal checksum.
		/// </summary>
		public Guid CreateGuid()
		{
			return GenerateGuidStatic( _Entries );
		}

		#endregion
	}
}