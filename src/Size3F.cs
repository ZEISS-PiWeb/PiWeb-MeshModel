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
	using Zeiss.PiWeb.MeshModel.Common;

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
		/// Gets en empty size.
		/// </summary>
		/// <value>
		/// Empty size.
		/// </value>
		public static Size3F Empty => CreateEmptySize();

		/// <summary>
		/// Gets a value indicating whether this instance is empty.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
		/// </value>
		public bool IsEmpty => _X < 0.0;

		/// <summary>
		/// Gets or sets the x.
		/// </summary>
		/// <value>
		/// The x.
		/// </value>
		/// <exception cref="InvalidOperationException">An empty size cannot be modified</exception>
		/// <exception cref="ArgumentException">A size cannot have a negative dimension</exception>
		public float X
		{
			get { return _X; }
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
		/// Gets or sets the y.
		/// </summary>
		/// <value>
		/// The y.
		/// </value>
		/// <exception cref="InvalidOperationException">An empty size cannot be modified</exception>
		/// <exception cref="ArgumentException">A size cannot have a negative dimension</exception>
		public float Y
		{
			get { return _Y; }
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
		/// Gets or sets the z.
		/// </summary>
		/// <value>
		/// The z.
		/// </value>
		/// <exception cref="InvalidOperationException">An empty size cannot be modified</exception>
		/// <exception cref="ArgumentException">A size cannot have a negative dimension</exception>
		public float Z
		{
			get { return _Z; }
			set
			{
				if( IsEmpty )
					throw new InvalidOperationException( "An empty size cannot be modified" );
				if( value < 0.0 )
					throw new ArgumentException( "A size cannot have a negative dimension" );
				_Z = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Size3F"/> struct.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="z">The z.</param>
		/// <exception cref="ArgumentException">A size cannot have a negative dimension</exception>
		public Size3F( float x, float y, float z )
		{
			if( x < 0.0 || y < 0.0 || z < 0.0 )
				throw new ArgumentException( "A size cannot have a negative dimension" );
			_X = x;
			_Y = y;
			_Z = z;
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="size1">The size1.</param>
		/// <param name="size2">The size2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==( Size3F size1, Size3F size2 )
		{
			return
				size1.X.IsCloseTo( size2.X ) &&
				size1.Y.IsCloseTo( size2.Y ) &&
				size1.Z.IsCloseTo( size2.Z );
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="size1">The size1.</param>
		/// <param name="size2">The size2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
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

		/// <summary>
		/// Performs an explicit conversion from <see cref="Size3F"/> to <see cref="Point3F"/>.
		/// </summary>
		/// <param name="size">The size.</param>
		/// <returns>
		/// The result of the conversion.
		/// </returns>
		public static explicit operator Point3F( Size3F size )
		{
			return new Point3F( size.X, size.Y, size.Z );
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

		/// <summary>
		/// Equalses the specified size1.
		/// </summary>
		/// <param name="size1">The size1.</param>
		/// <param name="size2">The size2.</param>
		/// <returns></returns>
		public static bool Equals( Size3F size1, Size3F size2 )
		{
			if( size1.IsEmpty )
				return size2.IsEmpty;

			return
				size1.X.IsCloseTo( size2.X ) &&
				size1.Y.IsCloseTo( size2.Y ) &&
				size1.Z.IsCloseTo( size2.Z );
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="o">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals( object o )
		{
			if( !( o is Size3F ) )
				return false;
			return Equals( this, (Size3F)o );
		}

		/// <summary>
		/// Equalses the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public bool Equals( Size3F value )
		{
			return Equals( this, value );
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