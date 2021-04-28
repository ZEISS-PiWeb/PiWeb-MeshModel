#region copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss Industrielle Messtechnik GmbH        */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2021                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

// ReSharper disable NonReadonlyMemberInGetHashCode
namespace Zeiss.PiWeb.MeshModel
{
	#region usings

	using System;

	#endregion

	/// <summary>
	/// Describes a cuboid with floating point coordinates. Adapted from the .NET Size3D class.
	/// </summary>
	public struct Size3F : IEquatable<Size3F>
	{
		private float _X;
		private float _Y;
		private float _Z;

		/// <summary>
		/// Gets an empty size.
		/// </summary>
		public static Size3F Empty => CreateEmptySize();

		/// <summary>
		/// Gets a value indicating whether this instance is empty.
		/// </summary>
		public bool IsEmpty => _X < 0.0;

		/// <summary>
		/// Gets or sets the X component.
		/// </summary>
		/// <exception cref="InvalidOperationException">An empty size cannot be modified</exception>
		/// <exception cref="ArgumentException">A size cannot have a negative dimension</exception>
		public float X
		{
			get => _X;
			set
			{
				if( IsEmpty )
					throw new InvalidOperationException( "An empty size cannot be modified" );
				if( value < 0.0 )
					throw new ArgumentException( "A size cannot have a negative dimension" );
				_X = value;
			}
		}

		/// <summary>
		/// Gets or sets the Y component.
		/// </summary>
		/// <exception cref="InvalidOperationException">An empty size cannot be modified</exception>
		/// <exception cref="ArgumentException">A size cannot have a negative dimension</exception>
		public float Y
		{
			get => _Y;
			set
			{
				if( IsEmpty )
					throw new InvalidOperationException( "An empty size cannot be modified" );
				if( value < 0.0 )
					throw new ArgumentException( "A size cannot have a negative dimension" );
				_Y = value;
			}
		}

		/// <summary>
		/// Gets or sets the Z component.
		/// </summary>
		/// <exception cref="InvalidOperationException">An empty size cannot be modified</exception>
		/// <exception cref="ArgumentException">A size cannot have a negative dimension</exception>
		public float Z
		{
			get => _Z;
			set
			{
				if( IsEmpty )
					throw new InvalidOperationException( "An empty size cannot be modified" );
				if( value < 0.0 )
					throw new ArgumentException( "A size cannot have a negative dimension" );
				_Z = value;
			}
		}

		/// <summary>Constructor.</summary>
		public Size3F( float x, float y, float z )
		{
			if( x < 0.0 || y < 0.0 || z < 0.0 )
				throw new ArgumentException( "A size cannot have a negative dimension" );
			_X = x;
			_Y = y;
			_Z = z;
		}

		public static bool operator ==( Size3F size1, Size3F size2 )
		{
			return size1.Equals( size2 );
		}

		public static bool operator !=( Size3F size1, Size3F size2 )
		{
			return !( size1 == size2 );
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="Size3F"/> to <see cref="Vector3F"/>.
		/// </summary>
		/// <param name="size">The size.</param>
		/// <returns>
		/// The result of the conversion.
		/// </returns>
		public static explicit operator Vector3F( Size3F size )
		{
			return new Vector3F( size.X, size.Y, size.Z );
		}

		private static Size3F CreateEmptySize()
		{
			return new Size3F
			{
				_X = float.NegativeInfinity,
				_Y = float.NegativeInfinity,
				_Z = float.NegativeInfinity
			};
		}

		/// <inheritdoc />
		public override bool Equals( object o )
		{
			return o is Size3F s && Equals( s );
		}

		/// <inheritdoc />
		public bool Equals( Size3F value )
		{
			if( IsEmpty )
				return value.IsEmpty;

			return X == value.X && Y == value.Y && Z == value.Z;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = _X.GetHashCode();
				hashCode = ( hashCode * 397 ) ^ _Y.GetHashCode();
				hashCode = ( hashCode * 397 ) ^ _Z.GetHashCode();
				return hashCode;
			}
		}
	}
}