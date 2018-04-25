#region Copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss IMT (IZfM Dresden)                   */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2017                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace Zeiss.IMT.PiWeb.MeshModel
{
	#region usings

	using System;
	using System.Runtime.CompilerServices;

	#endregion

	/// <summary>
	/// Describes a vector with three <code>float</code> components. Adapted from the .NET Vector3D class.
	/// </summary>
	public struct Vector3F
	{
		/// <summary>
		/// Gets the length.
		/// </summary>
		/// <value>
		/// The length.
		/// </value>
		public float Length => ( float ) Math.Sqrt( X * X + Y * Y + Z * Z );

		/// <summary>
		/// Gets the length squared.
		/// </summary>
		/// <value>
		/// The length squared.
		/// </value>
		public float LengthSquared => X * X + Y * Y + Z * Z;

		/// <summary>
		/// Gets or sets the x.
		/// </summary>
		/// <value>
		/// The x.
		/// </value>
		public float X { get; set; }

		/// <summary>
		/// Gets or sets the y.
		/// </summary>
		/// <value>
		/// The y.
		/// </value>
		public float Y { get; set; }

		/// <summary>
		/// Gets or sets the z.
		/// </summary>
		/// <value>
		/// The z.
		/// </value>
		public float Z { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Vector3F"/> struct.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="z">The z.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Vector3F( float x, float y, float z )
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="Vector3F"/> to <see cref="Point3F"/>.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <returns>
		/// The result of the conversion.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static explicit operator Point3F( Vector3F vector )
		{
			return new Point3F( vector.X, vector.Y, vector.Z );
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="Vector3F"/> to <see cref="Size3F"/>.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <returns>
		/// The result of the conversion.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static explicit operator Size3F( Vector3F vector )
		{
			return new Size3F( Math.Abs( vector.X ), Math.Abs( vector.Y ), Math.Abs( vector.Z ) );
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F operator -( Vector3F vector )
		{
			return new Vector3F( -vector.X, -vector.Y, -vector.Z );
		}

		/// <summary>
		/// Implements the operator +.
		/// </summary>
		/// <param name="vector1">The vector1.</param>
		/// <param name="vector2">The vector2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F operator +( Vector3F vector1, Vector3F vector2 )
		{
			return new Vector3F( vector1.X + vector2.X, vector1.Y + vector2.Y, vector1.Z + vector2.Z );
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="vector1">The vector1.</param>
		/// <param name="vector2">The vector2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F operator -( Vector3F vector1, Vector3F vector2 )
		{
			return new Vector3F( vector1.X - vector2.X, vector1.Y - vector2.Y, vector1.Z - vector2.Z );
		}

		/// <summary>
		/// Implements the operator +.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <param name="point">The point.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Point3F operator +( Vector3F vector, Point3F point )
		{
			return new Point3F( vector.X + point.X, vector.Y + point.Y, vector.Z + point.Z );
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <param name="point">The point.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Point3F operator -( Vector3F vector, Point3F point )
		{
			return new Point3F( vector.X - point.X, vector.Y - point.Y, vector.Z - point.Z );
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <param name="scalar">The scalar.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F operator *( Vector3F vector, float scalar )
		{
			return new Vector3F( vector.X * scalar, vector.Y * scalar, vector.Z * scalar );
		}

		/// <summary>
		/// Implements the operator *.
		/// </summary>
		/// <param name="scalar">The scalar.</param>
		/// <param name="vector">The vector.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F operator *( float scalar, Vector3F vector )
		{
			return new Vector3F( vector.X * scalar, vector.Y * scalar, vector.Z * scalar );
		}

		/// <summary>
		/// Implements the operator /.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <param name="scalar">The scalar.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F operator /( Vector3F vector, float scalar )
		{
			return vector * ( 1.0f / scalar );
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="vector1">The vector1.</param>
		/// <param name="vector2">The vector2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool operator ==( Vector3F vector1, Vector3F vector2 )
		{
			if( vector1.X == vector2.X &&
			    vector1.Y == vector2.Y )
				return vector1.Z == vector2.Z;
			return false;
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="vector1">The vector1.</param>
		/// <param name="vector2">The vector2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool operator !=( Vector3F vector1, Vector3F vector2 )
		{
			return !( vector1 == vector2 );
		}

		/// <summary>
		/// Normalizes this instance.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Normalize()
		{
			float num1 = Math.Abs( X );
			float num2 = Math.Abs( Y );
			float num3 = Math.Abs( Z );
			if( num2 > num1 )
				num1 = num2;
			if( num3 > num1 )
				num1 = num3;
			X = X / num1;
			Y = Y / num1;
			Z = Z / num1;
			this = this / ( float ) Math.Sqrt( X * X + Y * Y + Z * Z );
		}

		/// <summary>
		/// Calculates the angle between the specified vectors.
		/// </summary>
		/// <param name="vector1">The vector1.</param>
		/// <param name="vector2">The vector2.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float AngleBetween( Vector3F vector1, Vector3F vector2 )
		{
			vector1.Normalize();
			vector2.Normalize();
			return RadiansToDegrees( DotProduct( vector1, vector2 ) >= 0.0f ? 2.0f * ( float ) Math.Asin( ( vector1 - vector2 ).Length / 2.0f ) : ( float ) Math.PI - 2.0f * ( float ) Math.Asin( ( -vector1 - vector2 ).Length / 2.0f ) );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static float RadiansToDegrees( float value )
		{
			return value * 180 / ( float ) Math.PI;
		}

		/// <summary>
		/// Negates this instance.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Negate()
		{
			X = -X;
			Y = -Y;
			Z = -Z;
		}

		/// <summary>
		/// Adds the specified vector1.
		/// </summary>
		/// <param name="vector1">The vector1.</param>
		/// <param name="vector2">The vector2.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F Add( Vector3F vector1, Vector3F vector2 )
		{
			return new Vector3F( vector1.X + vector2.X, vector1.Y + vector2.Y, vector1.Z + vector2.Z );
		}

		/// <summary>
		/// Subtracts the specified vector1.
		/// </summary>
		/// <param name="vector1">The vector1.</param>
		/// <param name="vector2">The vector2.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F Subtract( Vector3F vector1, Vector3F vector2 )
		{
			return new Vector3F( vector1.X - vector2.X, vector1.Y - vector2.Y, vector1.Z - vector2.Z );
		}

		/// <summary>
		/// Adds the specified vector.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <param name="point">The point.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Point3F Add( Vector3F vector, Point3F point )
		{
			return new Point3F( vector.X + point.X, vector.Y + point.Y, vector.Z + point.Z );
		}

		/// <summary>
		/// Subtracts the specified vector.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <param name="point">The point.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Point3F Subtract( Vector3F vector, Point3F point )
		{
			return new Point3F( vector.X - point.X, vector.Y - point.Y, vector.Z - point.Z );
		}

		/// <summary>
		/// Multiplies the specified vector with the specified scalar.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <param name="scalar">The scalar.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F Multiply( Vector3F vector, float scalar )
		{
			return new Vector3F( vector.X * scalar, vector.Y * scalar, vector.Z * scalar );
		}

		/// <summary>
		/// Multiplies the specified scalar.
		/// </summary>
		/// <param name="scalar">The scalar.</param>
		/// <param name="vector">The vector.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F Multiply( float scalar, Vector3F vector )
		{
			return new Vector3F( vector.X * scalar, vector.Y * scalar, vector.Z * scalar );
		}

		/// <summary>
		/// Calculates the dot product.
		/// </summary>
		/// <param name="vector1">The vector1.</param>
		/// <param name="vector2">The vector2.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DotProduct( Vector3F vector1, Vector3F vector2 )
		{
			return DotProduct( ref vector1, ref vector2 );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		internal static float DotProduct( ref Vector3F vector1, ref Vector3F vector2 )
		{
			return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
		}

		/// <summary>
		///  Calculates the cross product.
		/// </summary>
		/// <param name="vector1">The vector1.</param>
		/// <param name="vector2">The vector2.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F CrossProduct( Vector3F vector1, Vector3F vector2 )
		{
			Vector3F result;
			CrossProduct( ref vector1, ref vector2, out result );
			return result;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static void CrossProduct( ref Vector3F vector1, ref Vector3F vector2, out Vector3F result )
		{
			result = new Vector3F(
				vector1.Y * vector2.Z - vector1.Z * vector2.Y,
				vector1.Z * vector2.X - vector1.X * vector2.Z,
				vector1.X * vector2.Y - vector1.Y * vector2.X
			);
		}

		/// <summary>
		/// Equalses the specified vector1.
		/// </summary>
		/// <param name="vector1">The vector1.</param>
		/// <param name="vector2">The vector2.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool Equals( Vector3F vector1, Vector3F vector2 )
		{
			if( vector1.X.Equals( vector2.X ) && vector1.Y.Equals( vector2.Y ) )
				return vector1.Z.Equals( vector2.Z );
			return false;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="o">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override bool Equals( object o )
		{
			if( !( o is Vector3F ) )
				return false;
			return Equals( this, ( Vector3F ) o );
		}

		/// <summary>
		/// Equalses the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool Equals( Vector3F value )
		{
			return Equals( this, value );
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = X.GetHashCode();
				hashCode = ( hashCode * 397 ) ^ Y.GetHashCode();
				hashCode = ( hashCode * 397 ) ^ Z.GetHashCode();
				return hashCode;
			}
		}
	}
}