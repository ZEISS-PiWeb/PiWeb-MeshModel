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

		#endregion
	}
}