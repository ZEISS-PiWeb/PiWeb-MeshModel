#region Copyright
/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss IMT (IZfM Dresden)                   */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2010                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */
#endregion

namespace Zeiss.IMT.PiWeb.Meshmodels
{
	#region using

	using System;
	using System.Collections.Generic;
	using System.IO;

	#endregion

	/// <summary>
	/// Describes a collection of <see cref="MeshValue"/> objects. When adding a <see cref="MeshValueList"/> to a <see cref="MeshModelPart"/> or 
	/// <see cref="MeshModel"/>, the number of <see cref="MeshValue"/> objects should be equal to the number of <see cref="Mesh"/> objects within
	/// the <see cref="MeshModel"/>.
	/// </summary>
	public class MeshValueList
	{
		public MeshValueList( MeshValue[] meshValues, MeshValueEntry entry )
		{
			MeshValues = meshValues ?? new MeshValue[0];
			Entry = entry;
		}

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