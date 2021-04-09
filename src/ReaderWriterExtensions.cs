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
		#region members

		private static readonly Version FileVersion10 = new Version( 1, 0, 0, 0 );

		#endregion

		#region methods

		/// <summary>
		/// Creates an empty zip entry with the specified <paramref name="entryName"/> and the specified <paramref name="compressionLevel"/>.
		/// The <code>LastWriteTime</code> is set to 01-01-1980 instead of the current time. By doing so, two zip archives with the same binary content
		/// become binary identical which leads to the same hash, which makes change detection easier.
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
		/// Writes the float array with a boolean marker.
		/// * If the array is empty, a <code>false</code> marker will be written
		/// * If the array is not empty, a <code>true</code> marker will be written first and the values are written
		/// </summary>
		public static void WriteConditionalFloatArray( this BinaryWriter binaryWriter, float[] values, int componentCount )
		{
			if( values != null && values.Length > 0 )
			{
				binaryWriter.Write( true );
				binaryWriter.WriteFloatArray( values, componentCount );
			}
			else
			{
				binaryWriter.Write( false );
			}
		}

		/// <summary>
		/// Writes the float array. The first 4 bytes indicates the number of entries. Afterwards the float array is written
		/// as a binary stream. The <paramref name="componentCount"/> indicates the number of components (i.e. 3 for a
		/// position/vector, 2 for uv corrdinates, 4 for rgba color values).
		/// </summary>
		public static void WriteFloatArray( this BinaryWriter writer, float[] data, int componentCount )
		{
			if( data == null || data.Length == 0 )
			{
				writer.Write( 0 );
				return;
			}

			writer.Write( data.Length / componentCount );

			const int arrayLength = 8 * 1024;
			var bytesToWrite = data.Length * 4;
			var bytesWritten = 0;
			var buffer = ArrayPool<byte>.Shared.Rent( arrayLength );
			while( bytesWritten < bytesToWrite )
			{
				var count = Math.Min( arrayLength, bytesToWrite - bytesWritten );

				Buffer.BlockCopy( data, bytesWritten, buffer, 0, count );
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