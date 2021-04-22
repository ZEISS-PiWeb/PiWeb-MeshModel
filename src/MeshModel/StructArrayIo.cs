#region copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss Industrielle Messtechnik GmbH        */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2021                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

using System;

namespace Zeiss.PiWeb.MeshModel
{
    interface IStructArrayIo<in T>
		where T : struct
	{
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
		int BufferFunction( byte[] buffer, T[] source, int count, int index );

		int ReadBuffer(byte[] buffer, T[] result, int count, int index);
		
		int Stride { get; }
	}
	
	public class FloatIo : IStructArrayIo<float>
	{
		private FloatIo(){}

		public static FloatIo Instance { get; } = new FloatIo();
		
		/// <inheritdoc />
		/// <remarks>
		/// Specific layout:
		/// <code>
		/// [ArrayLength][float][float][float]...
		/// |4B         |4B    |4B    |4B    |...
		/// </code>
		/// </remarks>
		public int BufferFunction( byte[] buffer, float[] source, int count, int index )
		{
			Buffer.BlockCopy( source, index, buffer, 0, count );
			
			return index + count;
		}

		public int ReadBuffer(byte[] buffer, float[] result, int count, int index)
		{
			Buffer.BlockCopy( buffer, 0, result, index*Stride, count );
			return index + count;
		}

		public int Stride => sizeof(float);
	}

	public class ColorIo : IStructArrayIo<Color>
	{
		private ColorIo(){}

		public static ColorIo Instance { get; } = new ColorIo();
		
		/// <inheritdoc />
		/// <remarks>
		/// Specific layout:
		/// <code>
		/// [ArrayLength][ARGB][ARGB][ARGB]...
		/// |4B         |4B   |4B   |4B   |...
		/// </code>
		/// </remarks>
		public int BufferFunction( byte[] buffer, Color[] source, int count, int index )
		{
			for (var i = 0; i < count; i+=Stride, index++)
			{
				// We use fixed byte order, to keep things consistent across machine boundaries.
				buffer[i] = source[index].A;
				buffer[i+1] = source[index].R;
				buffer[i+2] = source[index].G;
				buffer[i+3] = source[index].B;
			}
			
			return index;
		}

		public int ReadBuffer(byte[] buffer, Color[] result, int count, int index)
		{
			for (var i = 0; i < count; i += Stride)
			{
				result[index] = Color.FromArgb(buffer[i], buffer[i+1], buffer[i+2], buffer[i+3]);
				index++;
			}

			return index;
		}

		public int Stride => Color.Stride;
	}
	
	public class Vector2FIo : IStructArrayIo<Vector2F>
	{
		private Vector2FIo(){}

		public static Vector2FIo Instance { get; } = new Vector2FIo();
		
		/// <inheritdoc />
		/// <remarks>
		/// Specific layout:
		/// <code>
		/// [ArrayLength][XY][XY][XY]...
		/// |4B         |8B |8B |8B |...
		/// </code>
		/// </remarks>
		public int BufferFunction( byte[] buffer, Vector2F[] source, int count, int index )
		{
			const int componentSize = sizeof(float);
			
			for (var i = 0; i < count; i+=Stride, index++)
			{
				Array.Copy( BitConverter.GetBytes( source[index].X ), 0, buffer, i, componentSize );
				Array.Copy( BitConverter.GetBytes( source[index].Y ), 0, buffer, i + componentSize, componentSize );
			}
			
			return index;
		}

		public int ReadBuffer(byte[] buffer, Vector2F[] result, int count, int index)
		{
			for (var i = 0; i < count; i += Stride)
			{
				result[index] = new Vector2F(
					BitConverter.ToSingle(buffer, i),
					BitConverter.ToSingle(buffer, i + sizeof(float)));
				index++;
			}

			return index;
		}

		public int Stride => Vector2F.Stride;
	}
	
	public class Vector3FIo : IStructArrayIo<Vector3F>
	{
		private Vector3FIo(){}

		public static Vector3FIo Instance { get; } = new Vector3FIo();
		
		/// <inheritdoc />
		/// <remarks>
		/// Specific layout:
		/// <code>
		/// [ArrayLength][XYZ][XYZ][XYZ]...
		/// |4B         |12B |12B |12B |...
		/// </code>
		/// </remarks>
		public int BufferFunction( byte[] buffer, Vector3F[] source, int count, int index )
		{
			const int componentSize = sizeof(float);
			
			for (var i = 0; i < count; i+=Stride, index++)
			{
				Array.Copy( BitConverter.GetBytes( source[index].X ), 0, buffer, i, componentSize );
				Array.Copy( BitConverter.GetBytes( source[index].Y ), 0, buffer, i + componentSize, componentSize );
				Array.Copy( BitConverter.GetBytes( source[index].Z ), 0, buffer, i + componentSize*2, componentSize );
			}
			
			return index;
		}

		public int ReadBuffer(byte[] buffer, Vector3F[] result, int count, int index)
		{
			for (var i = 0; i < count; i += Stride)
			{
				result[index] = new Vector3F(
					BitConverter.ToSingle(buffer, i),
					BitConverter.ToSingle(buffer, i + sizeof(float)),
					BitConverter.ToSingle(buffer, i + sizeof(float)*2));
				index++;
			}

			return index;
		}

		public int Stride => Vector3F.Stride;
	}
}