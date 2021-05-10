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
	using System.Text;

	#endregion

	/// <summary>
	/// Describes a collection of <see cref="MeshValue"/> objects. When adding a <see cref="MeshValueList"/> to a <see cref="MeshModelPart"/> or
	/// <see cref="MeshModel"/>, the number of <see cref="MeshValue"/> objects should be equal to the number of <see cref="Mesh"/> objects within
	/// the <see cref="MeshModel"/>.
	/// </summary>
	public sealed class MeshValueList
	{
		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshValueList"/> class.
		/// </summary>
		/// <param name="meshValues">The mesh values.</param>
		/// <param name="entry">The entry.</param>
		public MeshValueList( MeshValue[] meshValues, MeshValueEntry entry )
		{
			MeshValues = meshValues ?? Array.Empty<MeshValue>();
			Entry = entry;
		}

		#endregion

		#region properties

		/// <summary>
		/// Gets the mesh values.
		/// </summary>
		public MeshValue[] MeshValues { get; }

		/// <summary>
		/// Gets the entry this data belongs to.
		/// </summary>
		public MeshValueEntry Entry { get; }

		#endregion

		#region methods

		/// <summary>
		/// Reads the mesh values from the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="entry">Entry as specified in the metadata.</param>
		/// <param name="fileVersion">File version as specified in the metadata.</param>
		public static MeshValueList Load( Stream stream, MeshValueEntry entry, Version fileVersion )
		{
			using var reader = new BinaryReader( stream, Encoding.Unicode, true );

			return Read( reader, entry, fileVersion );
		}

		internal static MeshValueList Read( BinaryReader binaryReader, MeshValueEntry entry, Version fileVersion )
		{
			var meshValueCount = binaryReader.ReadInt32();

			var meshValues = new List<MeshValue>( meshValueCount );
			for( var i = 0; i < meshValueCount; i++ )
			{
				meshValues.Add( MeshValue.Read( binaryReader, fileVersion ) );
			}

			return new MeshValueList( meshValues.ToArray(), entry );
		}

		internal void Write( BinaryWriter binaryWriter )
		{
			binaryWriter.Write( MeshValues.Length );
			foreach( var meshValue in MeshValues )
			{
				meshValue.Write( binaryWriter );
			}
		}

		#endregion
	}
}