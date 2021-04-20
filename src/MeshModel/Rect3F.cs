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
	using System.Runtime.CompilerServices;

	#endregion

	/// <summary>
	/// Describes a cuboid on a certain position with floating point coordinates. Adapted from the .Net Rect3D class.
	/// </summary>
	public struct Rect3F : IEquatable<Rect3F>
	{
		private float _X;
		private float _Y;
		private float _Z;
		private float _SizeX;
		private float _SizeY;
		private float _SizeZ;

		/// <summary>
		/// Gets the an empty rect.
		/// </summary>
		/// <value>
		/// An empty rect.
		/// </value>
		public static Rect3F Empty { get; } = CreateEmptyCuboid();

		/// <summary>
		/// Gets a value indicating whether this instance is empty.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
		/// </value>
		public readonly bool IsEmpty => _SizeX < 0.0;

		/// <summary>
		/// Gets or sets the location.
		/// </summary>
		/// <value>
		/// The location.
		/// </value>
		/// <exception cref="InvalidOperationException">An empty rect cannot be modified</exception>
		public Vector3F Location
		{
			get { return new Vector3F( _X, _Y, _Z ); }
			set
			{
				if( IsEmpty )
					throw new InvalidOperationException( "An empty rect cannot be modified" );
				_X = value.X;
				_Y = value.Y;
				_Z = value.Z;
			}
		}

		/// <summary>
		/// Gets or sets the size.
		/// </summary>
		/// <value>
		/// The size.
		/// </value>
		/// <exception cref="InvalidOperationException">An empty rect cannot be modified</exception>
		public Size3F Size
		{
			get { return IsEmpty ? Size3F.Empty : new Size3F( _SizeX, _SizeY, _SizeZ ); }
			set
			{
				if( value.IsEmpty )
				{
					this = Empty;
				}
				else
				{
					if( IsEmpty )
						throw new InvalidOperationException( "An empty rect cannot be modified" );
					_SizeX = value.X;
					_SizeY = value.Y;
					_SizeZ = value.Z;
				}
			}
		}

		/// <summary>
		/// Gets or sets the size x.
		/// </summary>
		/// <value>
		/// The size x.
		/// </value>
		/// <exception cref="InvalidOperationException">An empty rect cannot be modified</exception>
		/// <exception cref="ArgumentException">A rect cannot have a negative dimension</exception>
		public float SizeX
		{
			get { return _SizeX; }
			set
			{
				if( IsEmpty )
					throw new InvalidOperationException( "An empty rect cannot be modified" );
				if( value < 0.0 )
					throw new ArgumentException( "A rect cannot have a negative dimension" );
				_SizeX = value;
			}
		}

		/// <summary>
		/// Gets or sets the size y.
		/// </summary>
		/// <value>
		/// The size y.
		/// </value>
		/// <exception cref="InvalidOperationException">An empty rect cannot be modified</exception>
		/// <exception cref="ArgumentException">A rect cannot have a negative dimension</exception>
		public float SizeY
		{
			get { return _SizeY; }
			set
			{
				if( IsEmpty )
					throw new InvalidOperationException( "An empty rect cannot be modified" );
				if( value < 0.0 )
					throw new ArgumentException( "A rect cannot have a negative dimension" );
				_SizeY = value;
			}
		}

		/// <summary>
		/// Gets or sets the size z.
		/// </summary>
		/// <value>
		/// The size z.
		/// </value>
		/// <exception cref="InvalidOperationException">An empty rect cannot be modified</exception>
		/// <exception cref="ArgumentException">A rect cannot have a negative dimension</exception>
		public float SizeZ
		{
			get { return _SizeZ; }
			set
			{
				if( IsEmpty )
					throw new InvalidOperationException( "An empty rect cannot be modified" );
				if( value < 0.0 )
					throw new ArgumentException( "A rect cannot have a negative dimension" );
				_SizeZ = value;
			}
		}

		/// <summary>
		/// Gets or sets the x.
		/// </summary>
		/// <value>
		/// The x.
		/// </value>
		/// <exception cref="InvalidOperationException">An empty rect cannot be modified</exception>
		public float X
		{
			get { return _X; }
			set
			{
				if( IsEmpty )
					throw new InvalidOperationException( "An empty rect cannot be modified" );
				_X = value;
			}
		}

		/// <summary>
		/// Gets or sets the y.
		/// </summary>
		/// <value>
		/// The y.
		/// </value>
		/// <exception cref="InvalidOperationException">An empty rect cannot be modified</exception>
		public float Y
		{
			get { return _Y; }
			set
			{
				if( IsEmpty )
					throw new InvalidOperationException( "An empty rect cannot be modified" );
				_Y = value;
			}
		}

		/// <summary>
		/// Gets or sets the z.
		/// </summary>
		/// <value>
		/// The z.
		/// </value>
		/// <exception cref="InvalidOperationException">An empty rect cannot be modified</exception>
		public float Z
		{
			get { return _Z; }
			set
			{
				if( IsEmpty )
					throw new InvalidOperationException( "An empty rect cannot be modified" );
				_Z = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Rect3F"/> struct.
		/// </summary>
		/// <param name="location">The location.</param>
		/// <param name="size">The size.</param>
		public Rect3F( Vector3F location, Size3F size )
		{
			if( size.IsEmpty )
			{
				this = Empty;
			}
			else
			{
				_X = location.X;
				_Y = location.Y;
				_Z = location.Z;
				_SizeX = size.X;
				_SizeY = size.Y;
				_SizeZ = size.Z;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Rect3F"/> struct.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="z">The z.</param>
		/// <param name="sizeX">The size x.</param>
		/// <param name="sizeY">The size y.</param>
		/// <param name="sizeZ">The size z.</param>
		/// <exception cref="ArgumentException">A rect cannot have a negative dimension</exception>
		public Rect3F( float x, float y, float z, float sizeX, float sizeY, float sizeZ )
		{
			if( sizeX < 0.0 || sizeY < 0.0 || sizeZ < 0.0 )
				throw new ArgumentException( "A rect cannot have a negative dimension" );
			_X = x;
			_Y = y;
			_Z = z;
			_SizeX = sizeX;
			_SizeY = sizeY;
			_SizeZ = sizeZ;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Rect3F"/> struct using two opposite corner points.
		/// </summary>
		/// <param name="point1">First corner point.</param>
		/// <param name="point2">Second corner point.</param>
		public Rect3F( Vector3F point1, Vector3F point2 )
		{
			_X = Math.Min( point1.X, point2.X );
			_Y = Math.Min( point1.Y, point2.Y );
			_Z = Math.Min( point1.Z, point2.Z );
			_SizeX = Math.Abs( point2.X - point1.X );
			_SizeY = Math.Abs( point2.Y - point1.Y );
			_SizeZ = Math.Abs( point2.Z - point1.Z );
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="rect1">The rect1.</param>
		/// <param name="rect2">The rect2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==( Rect3F rect1, Rect3F rect2 )
		{
			return rect1.Equals( rect2 );
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="rect1">The rect1.</param>
		/// <param name="rect2">The rect2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=( Rect3F rect1, Rect3F rect2 )
		{
			return !( rect1 == rect2 );
		}

		/// <summary>
		/// Determines whether [contains] [the specified point].
		/// </summary>
		/// <param name="point">The point.</param>
		/// <returns>
		///   <c>true</c> if [contains] [the specified point]; otherwise, <c>false</c>.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly bool Contains( Vector3F point )
		{
			return Contains( point.X, point.Y, point.Z );
		}

		/// <summary>
		/// Determines whether [contains] [the specified x].
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="z">The z.</param>
		/// <returns>
		///   <c>true</c> if [contains] [the specified x]; otherwise, <c>false</c>.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly bool Contains( float x, float y, float z )
		{
			if( IsEmpty )
				return false;
			return ContainsInternal( x, y, z );
		}

		/// <summary>
		/// Determines whether [contains] [the specified rect].
		/// </summary>
		/// <param name="rect">The rect.</param>
		/// <returns>
		///   <c>true</c> if [contains] [the specified rect]; otherwise, <c>false</c>.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly bool Contains( Rect3F rect )
		{
			if( IsEmpty || rect.IsEmpty )
				return false;

			return ( (double)_X ) <= rect._X
					&& ( (double)_Y ) <= rect._Y
					&& ( (double)_Z ) <= rect._Z
					&& ( (double)( _X + _SizeX ) ) >= rect._X + rect._SizeX
					&& ( (double)( _Y + _SizeY ) ) >= rect._Y + rect._SizeY
					&& ( (double)( _Z + _SizeZ ) ) >= rect._Z + rect._SizeZ;
		}

		/// <summary>
		/// Determines whether this instance intersects with the specified <paramref name="rect"/>.
		/// </summary>
		/// <param name="rect">The rect.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly bool IntersectsWith( Rect3F rect )
		{
			if( IsEmpty || rect.IsEmpty )
				return false;

			return rect.X <= _X + _SizeX && rect._X + rect._SizeX >= _X
										&& rect._Y <= _Y + _SizeY && rect._Y + rect._SizeY >= _Y
										&& rect._Z <= _Z + _SizeZ && rect._Z + rect._SizeZ >= _Z;
		}

		/// <summary>
		/// Intersects with specified rect.
		/// </summary>
		/// <param name="rect">The rect.</param>
		public void Intersect( Rect3F rect )
		{
			if( IsEmpty || rect.IsEmpty || !IntersectsWith( rect ) )
			{
				this = Empty;
			}
			else
			{
				var num1 = Math.Max( _X, rect._X );
				var num2 = Math.Max( _Y, rect._Y );
				var num3 = Math.Max( _Z, rect._Z );
				_SizeX = Math.Min( _X + _SizeX, rect._X + rect._SizeX ) - num1;
				_SizeY = Math.Min( _Y + _SizeY, rect._Y + rect._SizeY ) - num2;
				_SizeZ = Math.Min( _Z + _SizeZ, rect._Z + rect._SizeZ ) - num3;
				_X = num1;
				_Y = num2;
				_Z = num3;
			}
		}

		/// <summary>
		/// Returns the intersection of the specified rectangles.
		/// </summary>
		/// <param name="rect1">The rect1.</param>
		/// <param name="rect2">The rect2.</param>
		/// <returns></returns>
		public static Rect3F Intersect( Rect3F rect1, Rect3F rect2 )
		{
			rect1.Intersect( rect2 );
			return rect1;
		}

		/// <summary>
		/// Unions the specified rect.
		/// </summary>
		/// <param name="rect">The rect.</param>
		public void Union( Rect3F rect )
		{
			if( IsEmpty )
			{
				this = rect;
			}
			else
			{
				if( rect.IsEmpty )
					return;
				var num1 = Math.Min( _X, rect._X );
				var num2 = Math.Min( _Y, rect._Y );
				var num3 = Math.Min( _Z, rect._Z );
				_SizeX = Math.Max( _X + _SizeX, rect._X + rect._SizeX ) - num1;
				_SizeY = Math.Max( _Y + _SizeY, rect._Y + rect._SizeY ) - num2;
				_SizeZ = Math.Max( _Z + _SizeZ, rect._Z + rect._SizeZ ) - num3;
				_X = num1;
				_Y = num2;
				_Z = num3;
			}
		}

		/// <summary>
		/// Unions the specified rect1.
		/// </summary>
		/// <param name="rect1">The rect1.</param>
		/// <param name="rect2">The rect2.</param>
		/// <returns></returns>
		public static Rect3F Union( Rect3F rect1, Rect3F rect2 )
		{
			rect1.Union( rect2 );
			return rect1;
		}

		/// <summary>
		/// Unions the specified vector.
		/// </summary>
		/// <param name="vector">The vector.</param>
		public void Union( Vector3F vector )
		{
			Union( new Rect3F( vector, vector ) );
		}

		/// <summary>
		/// Unions the specified rect.
		/// </summary>
		/// <param name="rect">The rect.</param>
		/// <param name="vector">The vector.</param>
		/// <returns></returns>
		public static Rect3F Union( Rect3F rect, Vector3F vector )
		{
			rect.Union( new Rect3F( vector, vector ) );
			return rect;
		}

		private readonly bool ContainsInternal( float x, float y, float z )
		{
			return x >= _X && x <= _X + _SizeX
							&& y >= _Y && y <= _Y + _SizeY
							&& z >= _Z && z <= _Z + _SizeZ;
		}

		private static Rect3F CreateEmptyCuboid()
		{
			return new Rect3F
			{
				_X = float.PositiveInfinity,
				_Y = float.PositiveInfinity,
				_Z = float.PositiveInfinity,
				_SizeX = float.NegativeInfinity,
				_SizeY = float.NegativeInfinity,
				_SizeZ = float.NegativeInfinity
			};
		}

		/// <summary>
		/// Equalses the specified rect1.
		/// </summary>
		/// <param name="rect1">The rect1.</param>
		/// <param name="rect2">The rect2.</param>
		/// <returns></returns>
		public static bool Equals( Rect3F rect1, Rect3F rect2 )
		{
			if( rect1.IsEmpty )
				return rect2.IsEmpty;

			return rect1.X == rect2.X
			       && rect1.Y == rect2.Y
			       && rect1.Z == rect2.Z
			       && rect1.SizeX == rect2.SizeX 
			       && rect1.SizeY == rect2.SizeY
			       && rect1.SizeZ == rect2.SizeZ;
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
			if( !( o is Rect3F ) )
				return false;
			return Equals( this, (Rect3F)o );
		}

		/// <summary>
		/// Equalses the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public bool Equals( Rect3F value )
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
				hashCode = ( hashCode * 397 ) ^ _SizeX.GetHashCode();
				hashCode = ( hashCode * 397 ) ^ _SizeY.GetHashCode();
				hashCode = ( hashCode * 397 ) ^ _SizeZ.GetHashCode();

				return hashCode;
			}
		}
		
		/// <summary>
		/// Scales the instance absolute by scaling value relative to center.
		/// </summary>
		/// <param name="scale">Absolute value to scale in all dimensions.</param>
		public void ScaleAbsolute( float scale )
		{
			ScaleAbsolute( scale, scale, scale );
		}

		/// <summary>
		/// Scales the instance absolute by scaling values relative to center.
		/// </summary>
		/// <param name="scaleX">Absolute value to scale in x dimension.</param>
		/// <param name="scaleY">Absolute value to scale in y dimension.</param>
		/// <param name="scaleZ">Absolute value to scale in z dimension.</param>
		public void ScaleAbsolute( float scaleX, float scaleY, float scaleZ )
		{
			var sX = SizeX + scaleX;
			var sY = SizeY + scaleY;
			var sZ = SizeZ + scaleZ;

			if( sX < 0.0 || sY < 0.0 || sZ < 0.0 )
				throw new ArgumentException( "A rect cannot have a negative dimension" );

			// adjust rectangle
			SizeX = sX;
			SizeY = sY;
			SizeZ = sZ;
			X -= scaleX / 2;
			Y -= scaleY / 2;
			Z -= scaleZ / 2;
		}

		/// <summary>
		/// Creates an absolute scaled rectangle from an existing rectangle relative to center.
		/// </summary>
		/// <param name="rect">The rect.</param>
		/// <param name="scale">The absolute scale value for all dimensions</param>
		/// <returns></returns>
		public static Rect3F ScaleAbsolute( Rect3F rect, float scale )
		{
			rect.ScaleAbsolute( scale );
			return rect;
		}

		/// <summary>
		/// Creates an absolute scaled rectangle from an existing rectangle relative to center.
		/// </summary>
		/// <param name="rect">The rect.</param>
		/// <param name="scaleX">Absolute value to scale in x dimension.</param>
		/// <param name="scaleY">Absolute value to scale in y dimension.</param>
		/// <param name="scaleZ">Absolute value to scale in z dimension.</param>
		/// <returns></returns>
		public static Rect3F ScaleAbsolute( Rect3F rect, float scaleX, float scaleY, float scaleZ )
		{
			rect.ScaleAbsolute( scaleX, scaleY, scaleZ );
			return rect;
		}
		
		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Location: {Location}, Size: {new Vector3F( X + SizeX, Y + SizeY, Z + SizeZ )}";
		}
	}
}