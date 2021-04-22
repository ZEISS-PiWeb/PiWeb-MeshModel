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
	using System.Globalization;
	using System.IO;
	using System.Resources;

	#endregion

	internal static class MeshModelHelper
	{
		#region methods

		/// <summary>
		/// Removes duplicate subsequent positions from the specified <paramref name="positions"/> array.
		/// </summary>
		internal static Vector3F[] RemoveDuplicatePositions( Vector3F[] positions )
		{
			if( positions == null || positions.Length == 0 )
				return positions;

			var last = positions[ 0 ];
			var result = new List<Vector3F>( positions.Length ) { positions[ 0 ] };

			for( var i = 0; i < positions.Length; i++ )
			{
				var current = positions[ i ];

				var distance = ( current - last ).LengthSquared;
				if( distance > 0.0000001 )
				{
					result.Add( current );

					last = current;
				}
			}

			return result.ToArray();
		}

		/// <summary>
		/// Reads all data from the specified stream and returns it as a byte array.
		/// </summary>
		internal static byte[] StreamToArray( Stream stream, int expectedSize = 64 * 1024 )
		{
			if( stream == null )
				return null;

			using var memStream = new MemoryStream( expectedSize );

			const int arrayLength = 64 * 1024;
			var buffer = ArrayPool<byte>.Shared.Rent( arrayLength );

			int count;
			while( ( count = stream.Read( buffer, 0, arrayLength ) ) > 0 )
			{
				memStream.Write( buffer, 0, count );
			}

			ArrayPool<byte>.Shared.Return( buffer );

			return memStream.ToArray();
		}

		/// <summary>
		/// Gets the resource with the specified name.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		internal static string GetResource<T>( string name )
		{
			return new ResourceManager( typeof( T ) ).GetString( name, CultureInfo.CurrentUICulture );
		}

		/// <summary>
		/// Formats the resource with the specified <paramref name="name"/> with the specified <paramref name="args"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name">The name.</param>
		/// <param name="args">The arguments.</param>
		/// <returns></returns>
		internal static string FormatResource<T>( string name, params object[] args )
		{
			var value = new ResourceManager( typeof( T ) ).GetString( name, CultureInfo.CurrentUICulture );
			return string.IsNullOrEmpty( value ) ? "" : string.Format( value, args );
		}

		/// <summary>
		/// Converts an array of <see cref="Vector3F"/> to a flat array of floats:
		///
		/// From {v0, v1, v2, ... } to {x0, y0, z0, x1, y1, z1, x2, y2, z2, ... }.
		/// </summary>
		/// <param name="vectors">Array that will be converted.</param>
		/// <returns>Flattened vector array.</returns>
		internal static float[] AsArrayOfFloats(Vector3F[] vectors)
		{
			var floats = new float[vectors.Length * 3];
			for ( var i = 0; i < vectors.Length; i++ )
			{
				floats[i * 3 + 0] = vectors[i].X;
				floats[i * 3 + 1] = vectors[i].Y;
				floats[i * 3 + 2] = vectors[i].Z;
			}

			return floats;
		}
		
		/// <summary>
		/// Converts an array of <see cref="Vector2F"/> to a flat array of floats:
		///
		/// From {v0, v1, v2, ... } to {x0, y0, x1, y1, x2, y2, ... }.
		/// </summary>
		/// <param name="vectors">Array that will be converted.</param>
		/// <returns>Flattened vector array.</returns>
		internal static float[] AsArrayOfFloats(Vector2F[] vectors)
		{
			var floats = new float[vectors.Length * 2];
			for ( var i = 0; i < vectors.Length; i++ )
			{
				floats[i * 2 + 0] = vectors[i].X;
				floats[i * 2 + 1] = vectors[i].Y;
			}

			return floats;
		}
		
		/// <summary>
		/// Converts an array of <see cref="Color"/>s to an array of floats by encoding the four
		/// byte components A, R, G and B into a single float.
		/// </summary>
		/// <param name="colors">Array that will be converted.</param>
		/// <returns>Array of floats where each one represents all four color components.</returns>
		internal static float[] AsArrayOfFloats( Color[] colors )
		{
			var floats = new float[colors.Length];
			for ( var i = 0; i < colors.Length; i++ )
			{

				var color = colors[i];
				floats[i] = BitConverter.ToSingle(new[] {color.A, color.R, color.G, color.B}, 0);
			}

			return floats;
		}

		#endregion
	}
}