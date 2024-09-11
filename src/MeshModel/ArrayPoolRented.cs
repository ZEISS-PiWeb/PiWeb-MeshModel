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

	#endregion

	/// <summary>
	/// Wraps a rented array from the <see cref="ArrayPool{T}"/> in a disposable to improve the readability by avoiding
	/// the try-finally blocks.
	/// </summary>
	public sealed class ArrayPoolRented<T> : IDisposable
	{
		#region constructors

		public ArrayPoolRented( int minimumSize )
		{
			Data = ArrayPool<T>.Shared.Rent( minimumSize );
		}

		#endregion

		#region properties

		/// <summary>
		/// The rented array.
		/// </summary>
		public T[] Data { get; }

		#endregion

		#region interface IDisposable

		public void Dispose()
		{
			ArrayPool<T>.Shared.Return( Data );
		}

		#endregion
	}
}