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
	using System.Buffers;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Compression;
	using System.Xml;

	#endregion

	/// <summary>
	/// Helper class for reading and writing data into xml files or binary streams.
	/// </summary>
	internal static class ReaderWriterExtensions
	{
		private const int KiB = 1024;
		
		#region members

		private static readonly Version FileVersion10 = new Version( 1, 0, 0, 0 );
		
		#endregion

		#region methods

		/// <summary>
		/// Creates an empty zip entry with the specified <paramref name="entryName"/> and the specified
		/// <paramref name="compressionLevel"/>. The <code>LastWriteTime</code> is set to 01-01-1980 instead of the
		/// current time. By doing so, two zip archives with the same binary content become binary identical which leads
		/// to the same hash, which makes change detection easier.
		/// </summary>
		public static ZipArchiveEntry CreateNormalizedEntry( this ZipArchive zipArchive, string entryName, CompressionLevel compressionLevel )
		{
			var entry = zipArchive.CreateEntry( entryName, compressionLevel );
			entry.LastWriteTime = new DateTime( 1980, 1, 1 );
			return entry;
		}
		
		/// <summary>
		/// Writes the string array with a boolean marker.
		/// * If the array is empty, a <code>false</code> marker will be written
		/// * If the array is not empty, a <code>true</code> marker will be written first and the values are written
		/// </summary>
		public static void WriteConditionalStrings( this BinaryWriter binaryWriter, string[] values )
		{
			if( values != null && values.Length > 0 )
			{
				binaryWriter.Write( true );
				binaryWriter.Write( values.Length );
				foreach( var l in values )
				{
					binaryWriter.Write( l );
				}
			}
			else
			{
				binaryWriter.Write( false );
			}
		}

		/// <summary>
		/// Writes the <paramref name="color"/> as an hex string at an attribute named <paramref name="name"/>.
		/// </summary>
		public static void WriteColorAttribute( this XmlWriter writer, string name, Color color )
		{
			writer.WriteAttributeString( name, $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}" );
		}

		/// <summary>
		/// Writes the color value as an integer value.
		/// </summary>
		public static void WriteArgbColor( this BinaryWriter writer, Color color )
		{
			writer.Write( color.A << 24 | color.R << 16 | color.G << 8 | color.B );
		}

		/// <summary>
		/// Writes the color with a boolean marker.
		/// * If the color is null, a <code>false</code> marker will be written
		/// * If the color is not null, a <code>true</code> marker will be written first and the color is written
		/// </summary>
		public static void WriteConditionalColor( this BinaryWriter binaryWriter, Color? color )
		{
			if( color.HasValue )
			{
				binaryWriter.Write( true );
				binaryWriter.WriteArgbColor( color.Value );
			}
			else
			{
				binaryWriter.Write( false );
			}
		}
		
		/// <summary>
		/// Writes a boolean marker and an array of struct type T using a <see cref="BinaryWriter"/>:
		/// <list type="bullet">
		/// <item><term>if array is empty,</term><description>false marker will be written</description></item>
		/// <item><term>if array is not empty,</term><description>true marker and values will be written</description></item>
		/// </list>
		/// </summary>
		/// <param name="arrayIo">Specific object for buffering values of struct type T.</param>
		/// <param name="writer">To write values.</param>
		/// <param name="data">To be written out.</param>
		public static void WriteConditionalArray<T>(
			this BinaryWriter writer, 
			IArrayIo<T> arrayIo,  
			T[] data )
			where T : struct
		{
			if( data != null && data.Length > 0 )
			{
				writer.Write( true );
				writer.WriteArray( arrayIo, data );
			}
			else
			{
				writer.Write( false );
			}
		}

		/// <summary>
		/// Writes an array of T using a <see cref="BinaryWriter"/> to the following layout:
		///
		/// <code>
		/// [ArrayLength][T0,T1,...][T0,T1,...][T0,T1,...]...
		/// |4B         |stride    |stride    |stride    |...
		/// </code>
		/// </summary>
		/// <param name="arrayIo">Specific object for buffering values of struct type T.</param>
		/// <param name="writer">To write values.</param>
		/// <param name="data">To be written out.</param>
		public static void WriteArray<T>(
			this BinaryWriter writer, 
			IArrayIo<T> arrayIo, 
			T[] data )
			where T : struct
		{
			writer.WriteArrayBuffered( arrayIo, data );
		}

		/// <summary>
		/// Writes an array of T using a <see cref="BinaryWriter"/> to the following layout:
		///
		/// <code>
		/// [ArrayLength][T0,T1,...][T0,T1,...][T0,T1,...]...
		/// |4B         |stride    |stride    |stride    |...
		/// </code>
		/// Where T0, T1, ... are the single components of the T struct.
		/// </summary>
		/// <remarks>
		/// If having a stride > 10, consider to provide a custom buffer size. Default buffer size is stride * KiB.
		/// If the stride becomes to big it might end up on the large object heap which has worse performance.
		/// </remarks>
		/// <param name="writer">Writes the array into a stream.</param>
		/// <param name="arrayIo">Fills the buffer with correct layout.</param>
		/// <param name="elements">Will be written out.</param>
		/// <param name="bufferSize">Length of the buffer array.</param>
		/// <typeparam name="T">Struct type of the elements.</typeparam>
		private static void WriteArrayBuffered<T>(
			this BinaryWriter writer, 
			IArrayIo<T> arrayIo,
			T[] elements,
			int bufferSize = 0 )
			where T : struct
		{
			if( elements == null || elements.Length == 0 )
			{
				writer.Write( 0 );
				return;
			}
			
			// Write length of the array.
			writer.Write( elements.Length );
			
			// We write chunks of data (stride * KiB) into a buffer and write it out into the file for better performance.
			// We do not use a bigger buffer, as we don't want it to reside on the "large object heap".
			var arrayLength = bufferSize <= 0
				? arrayIo.Stride * KiB
				: bufferSize;
			var bytesToWrite = elements.Length * arrayIo.Stride;
			var bytesWritten = 0;
			var buffer = ArrayPool<byte>.Shared.Rent( arrayLength );
			
			var index = 0;
			while( bytesWritten < bytesToWrite )
			{
				var count = Math.Min( arrayLength, bytesToWrite - bytesWritten );

				index = arrayIo.BufferFunction(buffer, elements, count, index);

				writer.Write( buffer, 0, count );

				bytesWritten += count;
			}
			ArrayPool<byte>.Shared.Return( buffer );
		}

		/// <summary>
		/// Reads a hex color value from an attribute with name <paramref name="name"/>.
		/// </summary>
		public static Color ReadColorAttribute( this XmlReader reader, string name )
		{
			var value = reader.GetAttribute( name );
			return Color.ParseArgb( value );
		}

		/// <summary>
		/// Reads the color value as an integer value.
		/// </summary>
		public static Color ReadArgbColor( this BinaryReader rdr )
		{
			var color = rdr.ReadInt32();

			var a = (byte)( (ulong)( color >> 24 ) & byte.MaxValue );
			var r = (byte)( (ulong)( color >> 16 ) & byte.MaxValue );
			var g = (byte)( (ulong)( color >> 8 ) & byte.MaxValue );
			var b = (byte)( (ulong)color & byte.MaxValue );

			return Color.FromArgb( a, r, g, b );
		}

		/// <summary>
		/// Reads an array of floats with a condition marker. First the marker is read and
		/// * if true, the float array is read and returned
		/// * if false, an empty array is returned
		/// </summary>
		public static float[] ReadConditionalFloatArray( this BinaryReader binaryReader, int componentCount, Version fileVersion )
		{
			if( !binaryReader.ReadBoolean() )
				return Array.Empty<float>();

			return fileVersion == FileVersion10
				? binaryReader.ReadDoubleArray( componentCount )
				: binaryReader.ReadFloatArray( componentCount );
		}
		
		/// <summary>
		/// Reads an array of floats with a condition marker and interprets it as array of Vector3F. First the marker is
		/// read and
		/// * if true, the float array is read and returned as array of Vector3Fs
		/// * if false, an empty array is returned
		/// </summary>
		public static Vector3F[] ReadConditionalFloatArrayAsVector3FArray( 
			this BinaryReader binaryReader,
			Version fileVersion )
		{
			if( !binaryReader.ReadBoolean() )
				return Array.Empty<Vector3F>();

			return fileVersion == FileVersion10
				? binaryReader.ReadDoubleArrayAsVector3FArray()
				: binaryReader.ReadFloatArrayAsVector3FArray();
		}

		/// <summary>
		/// Reads an array of strings with a condition marker. First the marker is read and
		/// * if true, the string array is read and returned
		/// * if false, an empty array is returned
		/// </summary>
		public static string[] ReadConditionalStringArray( this BinaryReader binaryReader )
		{
			if( !binaryReader.ReadBoolean() )
				return Array.Empty<string>();

			var layer = new string[ binaryReader.ReadInt32() ];
			for( var l = 0; l < layer.Length; l++ )
			{
				layer[ l ] = binaryReader.ReadString();
			}
			return layer;
		}

		/// <summary>
		/// Returns a float array representing vertices in the following format:
		///
		/// array = { x, y, z, x, y, z, x, y, z, x, y, z, ... }
		/// </summary>
		/// <param name="rdr">A <see cref="BinaryReader"/> having access to the mesh data.</param>
		/// <param name="componentCount">The number of connected components.</param>
		/// <returns>Array of floats: x, y, z, x, y, z, x, y, z, ...</returns>
		public static float[] ReadFloatArray( this BinaryReader rdr, int componentCount )
		{
			var length = rdr.ReadInt32(); // Number of vertices
			var size = length * componentCount; // Number of vertex components

			var result = new float[ size ];
			foreach( var (count, current, buffer) in rdr.ReadByteArrays( size * sizeof( float ) ) )
			{
				Buffer.BlockCopy( buffer, 0, result, current, count );
			}
			return result;
		}
		
		/// <summary>
		/// Reads an array of <see cref="Color"/> using a <see cref="BinaryReader"/>.
		/// </summary>
		/// <param name="reader">A <see cref="BinaryReader"/> having access to the mesh data.</param>
		/// <returns>Array of colors.</returns>
		public static Color[] ReadColorArray( this BinaryReader reader )
		{
			var length = reader.ReadInt32(); // Number of vertices

			var result = new Color[ length ];
			foreach( var (count, current, buffer) in reader.ReadByteArrays( length * 4 ) )
			{
				var index = current / 4;
					
				for (var i = 0; i < count; i += 4)
				{
					result[index] = Color.FromArgb(buffer[i], buffer[i+1], buffer[i+2], buffer[i+3]);
					index++;
				}
			}
			return result;
		}
		
		/// <summary>
		/// Reads an array of <see cref="Vector2F"/> using a <see cref="BinaryReader"/>.
		/// </summary>
		/// <param name="reader">A <see cref="BinaryReader"/> having access to the mesh data.</param>
		/// <returns>Array of vectors.</returns>
		public static Vector2F[] ReadVector2FArray( this BinaryReader reader )
		{
			const int compSize = sizeof(float);
			const int stride = compSize * 2;
			
			var length = reader.ReadInt32(); // Number of vertices

			var result = new Vector2F[ length ];
			foreach( var (count, current, buffer) in reader.ReadByteArrays( length * stride ) )
			{
				var index = current / stride;
					
				for (var i = 0; i < count; i += stride)
				{
					result[index] = new Vector2F(
						BitConverter.ToSingle(buffer, i),
						BitConverter.ToSingle(buffer, i + compSize));
					index++;
				}
			}
			return result;
		}

		/// <summary>
		/// Returns a Vector3F array representing vertices in the following format:
		///
		/// array = { v0, v1, v2, v3, v4, ... }
		/// </summary>
		/// <param name="rdr">A <see cref="BinaryReader"/> having access to the mesh data.</param>
		/// <returns>Array of Vector3Fs: v0, v1, v2, v3, v4, ...</returns>
		public static Vector3F[] ReadFloatArrayAsVector3FArray(this BinaryReader rdr )
		{
			const int componentSize = 4; // Size of one float.
			const int vectorSize = componentSize * 3;
			
			var result = new Vector3F[ rdr.ReadInt32() ];
			
			var data = rdr.ReadBytes( result.Length * vectorSize );
			for( int i = 0, j = 0; i < result.Length; j += vectorSize, i++ )
			{
				result[i] = new Vector3F(
					BitConverter.ToSingle( data, j ),
					BitConverter.ToSingle( data, j + componentSize ),
					BitConverter.ToSingle( data, j + componentSize * 2 ));
			}

			return result;
		}

		/// <summary>
		/// Returns a double array representing vertices in the following format:
		///
		/// array = { x, y, z, x, y, z, x, y, z, x, y, z, ... }
		/// </summary>
		/// <param name="rdr">A <see cref="BinaryReader"/> having access to the mesh data.</param>
		/// <param name="componentCount">The number of connected components.</param>
		/// <returns>Array of doubles: x, y, z, x, y, z, x, y, z, ...</returns>
		public static float[] ReadDoubleArray( this BinaryReader rdr, int componentCount )
		{
			var result = new float[ rdr.ReadInt32() * componentCount ];

			var data = rdr.ReadBytes( result.Length * 8 );
			for( int i = 0, j = 0; i < result.Length; j += 8, i++ )
			{
				result[ i ] = (float)BitConverter.ToDouble( data, j );
			}

			return result;
		}
		
		/// <summary>
		/// Returns a Vector3F array representing vertices in the following format:
		///
		/// array = { v0, v1, v2, v3, v4, ... }
		/// </summary>
		/// <param name="rdr">A <see cref="BinaryReader"/> having access to the mesh data.</param>
		/// <returns>Array of Vector3Fs: v0, v1, v2, v3, v4, ...</returns>
		public static Vector3F[] ReadDoubleArrayAsVector3FArray( this BinaryReader rdr )
		{
			var result = new Vector3F[ rdr.ReadInt32() ];

			var data = rdr.ReadBytes( result.Length * 8 * 3 );
			for( int i = 0, j = 0; i < result.Length; j += 24, i++ )
			{
				result[i] = new Vector3F(
					(float)BitConverter.ToDouble( data, j ),
					(float)BitConverter.ToDouble( data, j+8 ),
					(float)BitConverter.ToDouble( data, j+16 ));
			}

			return result;
		}

		/// <summary>
		/// Reads triangle indices. The <paramref name="pointCount"/> is used to estimate the largest triangle indice and create
		/// a memory efficient storage type to store the indices.
		/// </summary>
		public static Mesh.MeshIndices ReadIndices( this BinaryReader rdr, int pointCount )
		{
			var indexCount = rdr.ReadInt32();

			if( pointCount < byte.MaxValue )
			{
				var data = rdr.ReadBytes( indexCount * 4 );
				var triangleIndices = new byte[ indexCount ];

				for( int i = 0, j = 0; i < triangleIndices.Length; j += 4, i++ )
				{
					triangleIndices[ i ] = (byte)BitConverter.ToInt32( data, j );
				}

				return new Mesh.ByteMeshIndices( triangleIndices );
			}

			if( pointCount < short.MaxValue )
			{
				var data = rdr.ReadBytes( indexCount * 4 );
				var triangleIndices = new short[ indexCount ];

				for( int i = 0, j = 0; i < triangleIndices.Length; j += 4, i++ )
				{
					triangleIndices[ i ] = (short)BitConverter.ToInt32( data, j );
				}

				return new Mesh.ShortMeshIndices( triangleIndices );
			}
			else
			{
				var triangleIndices = new int[ indexCount ];
				foreach( var (count, current, buffer) in rdr.ReadByteArrays( indexCount * sizeof( int ) ) )
				{
					Buffer.BlockCopy( buffer, 0, triangleIndices, current, count );
				}

				return new Mesh.IntegerMeshIndices( triangleIndices );
			}
		}

		/// <param name="rdr"></param>
		/// <param name="totalBytes"></param>
		/// <returns>
		/// Triple of (Count, Current, buffer) where
		/// <ol>
		///		<li>Count ... number of bytes written into the buffer in this iteration</li>
		///		<li>Current ... at which byte this buffer starts</li>
		///		<li>buffer ... byte array containing as much bytes as given by "Count"</li>
		/// </ol>
		/// </returns>
		private static IEnumerable<(int Count, int Current, byte[] buffer)> ReadByteArrays(this BinaryReader rdr, int totalBytes)
		{
			const int arrayLength = 8 * 1024;

			var current = 0;
			var buffer = ArrayPool<byte>.Shared.Rent( arrayLength );

			while( true )
			{
				var count = Math.Min( arrayLength, totalBytes - current );
				if( count <= 0 )
					break;

				rdr.BaseStream.Read( buffer, 0, count );
				yield return ( count, current, buffer );

				current += count;
			}
			ArrayPool<byte>.Shared.Return( buffer );
		}

		#endregion
	}
}