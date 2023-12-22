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
	using System.IO;

	#endregion

	public interface IStructArrayIo<in T> where T : struct
	{
		#region properties

		/// <summary>
		/// Returns the data stride.
		/// </summary>
		int Stride { get; }

		#endregion

		#region methods

		/// <summary>
		/// Fills the given buffer array with the given number of elements, applying a specific layout:
		///
		/// <code>
		/// [ArrayLength][T0,T1,...][T0,T1,...][T0,T1,...]...
		/// |4B         |stride    |stride    |stride    |...
		/// </code>
		/// </summary>
		/// <param name="buffer">Buffer to be filled.</param>
		/// <param name="source">Source data array.</param>
		/// <param name="count">Number of items to take from the source array.</param>
		/// <param name="index">Where to start copying from the source array.</param>
		/// <returns>Position in the source array after filling the buffer.</returns>
		int WriteBuffer( byte[] buffer, T[] source, int count, int index );

		/// <summary>
		/// Reads the given buffer array with the given number of elements.
		/// </summary>
		/// <seealso cref="WriteBuffer"/>
		/// <param name="buffer">Buffer to be read.</param>
		/// <param name="result">Result data array.</param>
		/// <param name="count">Number of items to take from the source array.</param>
		/// <param name="index">Where to start copying from the source array.</param>
		void ReadBuffer( byte[] buffer, T[] result, int count, int index );

		#endregion
	}

	public class FloatIo : IStructArrayIo<float>
	{
		#region constructors

		private FloatIo() { }

		#endregion

		#region properties

		public static FloatIo Instance { get; } = new FloatIo();

		#endregion

		#region interface IStructArrayIo<float>

		/// <inheritdoc />
		/// <remarks>
		/// Specific layout:
		/// <code>
		/// [ArrayLength][float][float][float]...
		/// |4B         |4B    |4B    |4B    |...
		/// </code>
		/// </remarks>
		public int WriteBuffer( byte[] buffer, float[] source, int count, int index )
		{
			Buffer.BlockCopy( source, index, buffer, 0, count );

			return index + count;
		}

		public void ReadBuffer( byte[] buffer, float[] result, int count, int index )
		{
			Buffer.BlockCopy( buffer, 0, result, index * Stride, count );
		}

		public int Stride => sizeof( float );

		#endregion
	}

	public class ColorIo : IStructArrayIo<Color>
	{
		#region constructors

		private ColorIo() { }

		#endregion

		#region properties

		public static ColorIo Instance { get; } = new ColorIo();

		#endregion

		#region interface IStructArrayIo<Color>

		/// <inheritdoc />
		/// <remarks>
		/// Specific layout:
		/// <code>
		/// [ArrayLength][ARGB][ARGB][ARGB]...
		/// |4B         |4B   |4B   |4B   |...
		/// </code>
		/// </remarks>
		public int WriteBuffer( byte[] buffer, Color[] source, int count, int index )
		{
			for( var i = 0; i < count; i += Stride, index++ )
			{
				// We use fixed byte order, to keep things consistent across machine boundaries.
				buffer[ i ] = source[ index ].A;
				buffer[ i + 1 ] = source[ index ].R;
				buffer[ i + 2 ] = source[ index ].G;
				buffer[ i + 3 ] = source[ index ].B;
			}

			return index;
		}

		public void ReadBuffer( byte[] buffer, Color[] result, int count, int index )
		{
			for( var i = 0; i < count; i += Stride )
			{
				result[ index ] = Color.FromArgb( buffer[ i ], buffer[ i + 1 ], buffer[ i + 2 ], buffer[ i + 3 ] );
				index++;
			}
		}

		public int Stride => Color.Stride;

		#endregion
	}

	public class Vector2FIo : IStructArrayIo<Vector2F>
	{
		#region constructors

		private Vector2FIo() { }

		#endregion

		#region properties

		public static Vector2FIo Instance { get; } = new Vector2FIo();

		#endregion

		#region interface IStructArrayIo<Vector2F>

		/// <inheritdoc />
		/// <remarks>
		/// Specific layout:
		/// <code>
		/// [ArrayLength][XY][XY][XY]...
		/// |4B         |8B |8B |8B |...
		/// </code>
		/// </remarks>
		public int WriteBuffer( byte[] buffer, Vector2F[] source, int count, int index )
		{
			using var stream = new MemoryStream( buffer, true );
			using var binaryWriter = new BinaryWriter( stream );

			for( var i = 0; i < count; i += Stride, index++ )
			{
				binaryWriter.Write( source[ index ].X );
				binaryWriter.Write( source[ index ].Y );
			}

			return index;
		}

		public void ReadBuffer( byte[] buffer, Vector2F[] result, int count, int index )
		{
			for( var i = 0; i < count; i += Stride )
			{
				result[ index ] = new Vector2F(
					BitConverter.ToSingle( buffer, i ),
					BitConverter.ToSingle( buffer, i + sizeof( float ) ) );
				index++;
			}
		}

		public int Stride => Vector2F.Stride;

		#endregion
	}

	public class Vector3FIo : IStructArrayIo<Vector3F>
	{
		#region constructors

		private Vector3FIo() { }

		#endregion

		#region properties

		public static Vector3FIo Instance { get; } = new Vector3FIo();

		#endregion

		#region interface IStructArrayIo<Vector3F>

		/// <inheritdoc />
		/// <remarks>
		/// Specific layout:
		/// <code>
		/// [ArrayLength][XYZ][XYZ][XYZ]...
		/// |4B         |12B |12B |12B |...
		/// </code>
		/// </remarks>
		public int WriteBuffer( byte[] buffer, Vector3F[] source, int count, int index )
		{
			using var stream = new MemoryStream( buffer, true );
			using var binaryWriter = new BinaryWriter( stream );

			for( var i = 0; i < count; i += Stride, index++ )
			{
				binaryWriter.Write( source[ index ].X );
				binaryWriter.Write( source[ index ].Y );
				binaryWriter.Write( source[ index ].Z );
			}

			return index;
		}

		public void ReadBuffer( byte[] buffer, Vector3F[] result, int count, int index )
		{
			const int yOffset = sizeof( float );
			const int zOffset = sizeof( float ) * 2;

			for( var i = 0; i < count; i += Stride )
			{
				result[ index ] = new Vector3F(
					BitConverter.ToSingle( buffer, i ),
					BitConverter.ToSingle( buffer, i + yOffset ),
					BitConverter.ToSingle( buffer, i + zOffset ) );
				index++;
			}
		}

		public int Stride => Vector3F.Stride;

		#endregion
	}
}