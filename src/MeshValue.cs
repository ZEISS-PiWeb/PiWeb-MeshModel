#region Copyright
/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss IMT (IZfM Dresden)                   */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2010                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */
#endregion

namespace Zeiss.IMT.PiWeb.MeshModel
{
	#region using

	using System;
	using System.IO;

	#endregion

	/// <summary>
	/// Describes a collection of values that is associated to a mesh.
	/// </summary>
	public sealed class MeshValue
	{
		#region constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="MeshValue"/> class.
		/// </summary>
		/// <remarks>
		/// The number of values must be equal to the number of positions in the <see cref="Mesh"/>, with which this <see cref="MeshValue"/> is associated.
		/// You can determine this number by dividing the length of the <see cref="Mesh.Positions"/> array by 3. In case not all points have a value, just
		/// set theirs to <code>float.NaN</code>.
		/// </remarks>
		/// <param name="values">The values.</param>
		public MeshValue( float[] values )
		{
			Values = values;
		}

		#endregion

		#region properties
		
		/// <summary>
		/// Gets the values. Invalid values can be marked with float.NaN.
		/// </summary>
		public float[] Values { get; }

		#endregion

		#region methods

		internal static MeshValue Read( BinaryReader binaryReader, Version fileVersion )
		{
			return binaryReader.ReadBoolean()? new MeshValue(binaryReader.ReadFloatArray(1)) : null;
		}

		internal void Write( BinaryWriter binaryWriter )
		{
			if (Values != null && Values.Length > 0)
			{
				binaryWriter.Write(true);
				binaryWriter.WriteFloatArray(Values, 1);
			}
			else
			{
				binaryWriter.Write(false);
			}
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		public override string ToString()
		{
			return $"MeshValues {Values.Length} values";
		}

		#endregion
	}
}