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
	using System.IO.Compression;
	using System.Xml;

	#endregion

	internal static class ReaderWriterExtensions
	{
		#region methods
		
		/// <summary>
		/// Creates an empty zip entry with the specified <paramref name="entryName"/> and the specified <paramref name="compressionLevel"/>.
		/// The <code>LastWriteTime</code> is set to 01-01-1980 instead of the current time. By doing so, two zip archives with the same binary content
		/// become binary identical which leads to the same hash, which makes change detection easier.
		/// </summary>
		public static ZipArchiveEntry CreateNormalizedEntry(this ZipArchive zipArchive, string entryName, CompressionLevel compressionLevel)
		{
			var entry = zipArchive.CreateEntry(entryName, compressionLevel);
			entry.LastWriteTime = new DateTime(1980, 1, 1);
			return entry;
		}

	
		public static void WriteColorAttribute(this XmlWriter writer, string name, Color color)
		{
			writer.WriteAttributeString(name, $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}");
		}

		public static void WriteArgbColor(this BinaryWriter writer, Color color)
		{
			writer.Write(color.A << 24 | color.R << 16 | color.G << 8 | color.B);
		}

		public static void WriteFloatArray(this BinaryWriter writer, float[] data, int componentCount)
		{
			if (data == null || data.Length == 0)
			{
				writer.Write(0);
				return;
			}

			writer.Write(data.Length / componentCount);

			var buffer = new byte[data.Length * 4];
			Buffer.BlockCopy(data, 0, buffer, 0, buffer.Length);

			writer.Write(buffer);
		}

		

		public static Color ReadColorAttribute(this XmlReader reader, string name)
		{
			var value = reader.GetAttribute(name);
			return Color.ParseArgb(value);
		}

		public static Color ReadArgbColor(this BinaryReader rdr)
		{
			var color = rdr.ReadInt32();

			var a = (byte)((ulong)(color >> 24) & byte.MaxValue);
			var r = (byte)((ulong)(color >> 16) & byte.MaxValue);
			var g = (byte)((ulong)(color >> 8) & byte.MaxValue);
			var b = (byte)((ulong)color & byte.MaxValue);

			return Color.FromArgb(a, r, g, b);
		}

		public static float[] ReadFloatArray(this BinaryReader rdr, int componentCount)
		{
			var length = rdr.ReadInt32();
			var size = length * componentCount;

			var result = new float[size];

			var data = rdr.ReadBytes(result.Length * 4);
			Buffer.BlockCopy(data, 0, result, 0, data.Length);

			return result;
		}

		public static float[] ReadDoubleArray(this BinaryReader rdr, int componentCount)
		{
			var result = new float[rdr.ReadInt32() * componentCount];

			var data = rdr.ReadBytes(result.Length * 8);
			for (int i = 0, j = 0; i < result.Length; j += 8, i++)
			{
				result[i] = (float)BitConverter.ToDouble(data, j);
			}
			return result;
		}

		public static Mesh.MeshIndices ReadIndices(this BinaryReader rdr, int pointCount)
		{
			var indexCount = rdr.ReadInt32();
			var data = rdr.ReadBytes(indexCount * 4);

			if (pointCount < byte.MaxValue)
			{
				var triangleIndices = new byte[indexCount];
				for (int i = 0, j = 0; i < triangleIndices.Length; j += 4, i++)
				{
					triangleIndices[i] = (byte)BitConverter.ToInt32(data, j);
				}
				return new Mesh.ByteMeshIndices(triangleIndices);
			}
			if (pointCount < short.MaxValue)
			{
				var triangleIndices = new short[indexCount];
				for (int i = 0, j = 0; i < triangleIndices.Length; j += 4, i++)
				{
					triangleIndices[i] = (short)BitConverter.ToInt32(data, j);
				}
				return new Mesh.ShortMeshIndices(triangleIndices);
			}
			else
			{
				var triangleIndices = new int[indexCount];
				Buffer.BlockCopy(data, 0, triangleIndices, 0, data.Length);

				return new Mesh.IntegerMeshIndices(triangleIndices);
			}
		}

		#endregion
	}
}