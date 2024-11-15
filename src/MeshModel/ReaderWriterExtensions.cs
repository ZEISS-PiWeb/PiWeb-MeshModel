﻿#region copyright

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
	using Zeiss.PiWeb.ColorScale;

	#endregion

	/// <summary>
	/// Helper class for reading and writing data into xml files or binary streams.
	/// </summary>
	public static class ReaderWriterExtensions
	{
		#region constants

		private const int KiB = 1024;

		#endregion

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
		internal static ZipArchiveEntry CreateNormalizedEntry( this ZipArchive zipArchive, string entryName, CompressionLevel compressionLevel )
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
		internal static void WriteConditionalStrings( this BinaryWriter binaryWriter, string[] values )
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
		/// Writes the color value as an integer value.
		/// </summary>
		private static void WriteArgbColor( this BinaryWriter writer, Color color )
		{
			writer.Write( color.A << 24 | color.R << 16 | color.G << 8 | color.B );
		}

		/// <summary>
		/// Writes the color with a boolean marker.
		/// * If the color is null, a <code>false</code> marker will be written
		/// * If the color is not null, a <code>true</code> marker will be written first and the color is written
		/// </summary>
		internal static void WriteConditionalColor( this BinaryWriter binaryWriter, Color? color )
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
		/// <param name="structArrayIo">Specific object for buffering values of struct type T.</param>
		/// <param name="writer">To write values.</param>
		/// <param name="data">To be written out.</param>
		internal static void WriteConditionalArray<T>(
			this BinaryWriter writer,
			IStructArrayIo<T> structArrayIo,
			T[] data )
			where T : struct
		{
			if( data != null && data.Length > 0 )
			{
				writer.Write( true );
				writer.WriteArray( structArrayIo, data );
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
		/// <param name="structArrayIo">Specific object for buffering values of struct type T.</param>
		/// <param name="writer">To write values.</param>
		/// <param name="data">To be written out.</param>
		internal static void WriteArray<T>(
			this BinaryWriter writer,
			IStructArrayIo<T> structArrayIo,
			T[] data )
			where T : struct
		{
			writer.WriteArrayBuffered( structArrayIo, data );
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
		/// <param name="structArrayIo">Fills the buffer with correct layout.</param>
		/// <param name="elements">Will be written out.</param>
		/// <param name="bufferSize">Length of the buffer array.</param>
		/// <typeparam name="T">Struct type of the elements.</typeparam>
		private static void WriteArrayBuffered<T>(
			this BinaryWriter writer,
			IStructArrayIo<T> structArrayIo,
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
				? structArrayIo.Stride * KiB
				: bufferSize;
			var bytesToWrite = elements.Length * structArrayIo.Stride;
			var bytesWritten = 0;
			var buffer = ArrayPool<byte>.Shared.Rent( arrayLength );

			var index = 0;
			while( bytesWritten < bytesToWrite )
			{
				var count = Math.Min( arrayLength, bytesToWrite - bytesWritten );

				index = structArrayIo.WriteBuffer( buffer, elements, count, index );

				writer.Write( buffer, 0, count );

				bytesWritten += count;
			}

			ArrayPool<byte>.Shared.Return( buffer );
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
		/// Reads an array of floats with a condition marker and interprets it as array of Vector3F. First the marker is
		/// read and
		/// * if true, the float array is read and returned as array of Vector3Fs
		/// * if false, an empty array is returned
		/// </summary>
		internal static Vector3F[] ReadConditionalVector3FArray(
			this BinaryReader binaryReader,
			Version fileVersion )
		{
			if( !binaryReader.ReadBoolean() )
				return Array.Empty<Vector3F>();

			return fileVersion == FileVersion10
				? binaryReader.ReadDoubleArrayAsVector3FArray()
				: binaryReader.ReadLengthAndArray( Vector3FIo.Instance );
		}

		/// <summary>
		/// Reads an array of strings with a condition marker. First the marker is read and
		/// * if true, the string array is read and returned
		/// * if false, an empty array is returned
		/// </summary>
		internal static string[] ReadConditionalStringArray( this BinaryReader binaryReader )
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

		internal static T[] ReadLengthAndArray<T>( this BinaryReader reader, IStructArrayIo<T> structArrayIo )
			where T : struct
		{
			var length = reader.ReadInt32(); // Number of vertices

			return reader.ReadArray( structArrayIo, length );
		}

		private static T[] ReadArray<T>( this BinaryReader reader, IStructArrayIo<T> structArrayIo, int numberOfElements ) where T : struct
		{
			var result = new T[ numberOfElements ];
			foreach( var (count, current, buffer) in reader.ReadByteArrays( numberOfElements, structArrayIo.Stride ) )
			{
				var index = current / structArrayIo.Stride;
				structArrayIo.ReadBuffer( buffer, result, count, index );
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
		internal static Vector3F[] ReadDoubleArrayAsVector3FArray( this BinaryReader rdr )
		{
			var result = new Vector3F[ rdr.ReadInt32() ];

			var data = rdr.ReadBytes( result.Length * 8 * 3 );
			for( int i = 0, j = 0; i < result.Length; j += 24, i++ )
			{
				result[ i ] = new Vector3F(
					(float)BitConverter.ToDouble( data, j ),
					(float)BitConverter.ToDouble( data, j + 8 ),
					(float)BitConverter.ToDouble( data, j + 16 ) );
			}

			return result;
		}

		/// <summary>
		/// Reads triangle indices. The <paramref name="pointCount"/> is used to estimate the largest triangle index and
		/// create a memory efficient storage type to store the indices.
		/// </summary>
		internal static Mesh.MeshIndices ReadIndices( this BinaryReader rdr, int pointCount )
		{
			const int indexStride = sizeof( int );

			var indexCount = rdr.ReadInt32();

			if( pointCount < byte.MaxValue )
			{
				var data = rdr.ReadBytes( indexCount * indexStride );
				var triangleIndices = new byte[ indexCount ];

				for( int i = 0, j = 0; i < triangleIndices.Length; j += indexStride, i++ )
				{
					triangleIndices[ i ] = (byte)BitConverter.ToInt32( data, j );
				}

				return new Mesh.ByteMeshIndices( triangleIndices );
			}

			if( pointCount < short.MaxValue )
			{
				var data = rdr.ReadBytes( indexCount * indexStride );
				var triangleIndices = new short[ indexCount ];

				for( int i = 0, j = 0; i < triangleIndices.Length; j += indexStride, i++ )
				{
					triangleIndices[ i ] = (short)BitConverter.ToInt32( data, j );
				}

				return new Mesh.ShortMeshIndices( triangleIndices );
			}
			else
			{
				var triangleIndices = new int[ indexCount ];
				foreach( var (count, current, buffer) in rdr.ReadByteArrays( indexCount, indexStride ) )
				{
					Buffer.BlockCopy( buffer, 0, triangleIndices, current, count );
				}

				return new Mesh.IntegerMeshIndices( triangleIndices );
			}
		}

		/// <summary>
		/// Reads byte arrays in an enumerable way, yielding buffered parts of it.
		///
		/// <list type="bullet">
		/// <item><term>Count:</term><description>number of bytes written into the buffer in this iteration</description></item>
		/// <item><term>Current:</term><description>at which byte this buffer starts</description></item>
		/// <item><term>buffer:</term><description>byte array containing as much bytes as given by "Count"</description></item>
		/// </list>
		/// </summary>
		/// <returns>Triple of (Count, Current, buffer).</returns>
		private static IEnumerable<(int Count, int Current, byte[] buffer)> ReadByteArrays( this BinaryReader rdr, int elementCount, int stride )
		{
			var totalBytes = elementCount * stride;
			var arrayLength = stride * 1024;

			var current = 0;
			var buffer = ArrayPool<byte>.Shared.Rent( arrayLength );

			while( true )
			{
				var count = Math.Min( arrayLength, totalBytes - current );
				if( count <= 0 )
					break;

				var totalRead = 0;
				while( totalRead < count )
				{
					var bytesRead = rdr.BaseStream.Read( buffer, totalRead, count - totalRead );
					if( bytesRead == 0 ) break;
					totalRead += bytesRead;
				}

				yield return ( count, current, buffer );

				current += count;
			}

			ArrayPool<byte>.Shared.Return( buffer );
		}

		#endregion
	}
}