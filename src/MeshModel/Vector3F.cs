#region copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss Industrielle Messtechnik GmbH        */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2021                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

// ReSharper disable NonReadonlyMemberInGetHashCode
using System.Runtime.InteropServices;

namespace Zeiss.PiWeb.MeshModel
{
	#region usings

	using System;
	using System.Runtime.CompilerServices;

	#endregion

	/// <summary>
	/// Describes a vector with three <code>float</code> components. Adapted from the .NET Vector3D class.
	/// </summary>
	[StructLayout( LayoutKind.Sequential, Size = 12, Pack = 4 )]
	public struct Vector3F
	{
		/// <summary>
		/// Gets the vector's length.
		/// </summary>
		public float Length => (float)Math.Sqrt( X * X + Y * Y + Z * Z );

		/// <summary>
		/// Gets the vector's length squared.
		/// </summary>
		public float LengthSquared => X * X + Y * Y + Z * Z;

		/// <summary>
		/// Gets or sets the X component.
		/// </summary>
		public float X { get; set; }

		/// <summary>
		/// Gets or sets the Y component.
		/// </summary>
		public float Y { get; set; }

		/// <summary>
		/// Gets or sets the Z component.
		/// </summary>
		public float Z { get; set; }

		/// <summary>Constructor.</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Vector3F( float x, float y, float z )
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="Vector3F"/> to <see cref="Size3F"/>.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <returns>The result of the conversion.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static explicit operator Size3F( Vector3F vector )
		{
			return new Size3F( Math.Abs( vector.X ), Math.Abs( vector.Y ), Math.Abs( vector.Z ) );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F operator -( Vector3F vector )
		{
			return new Vector3F( -vector.X, -vector.Y, -vector.Z );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F operator +( Vector3F vector1, Vector3F vector2 )
		{
			return new Vector3F( vector1.X + vector2.X, vector1.Y + vector2.Y, vector1.Z + vector2.Z );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F operator -( Vector3F vector1, Vector3F vector2 )
		{
			return new Vector3F( vector1.X - vector2.X, vector1.Y - vector2.Y, vector1.Z - vector2.Z );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F operator *( Vector3F vector, float scalar )
		{
			return new Vector3F( vector.X * scalar, vector.Y * scalar, vector.Z * scalar );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F operator *( float scalar, Vector3F vector )
		{
			return new Vector3F( vector.X * scalar, vector.Y * scalar, vector.Z * scalar );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F operator /( Vector3F vector, float scalar )
		{
			return vector * ( 1.0f / scalar );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool operator ==( Vector3F vector1, Vector3F vector2 )
		{
			return vector1.Equals( vector2 );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool operator !=( Vector3F vector1, Vector3F vector2 )
		{
			return !( vector1 == vector2 );
		}

		/// <inheritdoc cref="Vector2F.Normalize"/>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Normalize()
		{
			var num1 = Math.Abs( X );
			var num2 = Math.Abs( Y );
			var num3 = Math.Abs( Z );

			if( num2 > num1 )
				num1 = num2;
			if( num3 > num1 )
				num1 = num3;

			X /= num1;
			Y /= num1;
			Z /= num1;
			this = this / (float)Math.Sqrt( X * X + Y * Y + Z * Z );
		}

		/// <inheritdoc cref="Vector2F.AngleBetween"/>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float AngleBetween( Vector3F vector1, Vector3F vector2 )
		{
			vector1.Normalize();
			vector2.Normalize();
			return RadiansToDegrees( DotProduct( vector1, vector2 ) >= 0.0
				? 2.0f * (float)Math.Asin( ( vector1 - vector2 ).Length / 2.0f )
				: (float)Math.PI - 2.0f * (float)Math.Asin( ( -vector1 - vector2 ).Length / 2.0f ) );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static float RadiansToDegrees( float value )
		{
			return value * 180 / (float)Math.PI;
		}

		/// <inheritdoc cref="Vector2F.Negate"/>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Negate()
		{
			X = -X;
			Y = -Y;
			Z = -Z;
		}

		/// <inheritdoc cref="Vector2F.Add"/>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F Add( Vector3F vector1, Vector3F vector2 )
		{
			return new Vector3F( vector1.X + vector2.X, vector1.Y + vector2.Y, vector1.Z + vector2.Z );
		}

		/// <inheritdoc cref="Vector2F.Subtract"/>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F Subtract( Vector3F vector1, Vector3F vector2 )
		{
			return new Vector3F( vector1.X - vector2.X, vector1.Y - vector2.Y, vector1.Z - vector2.Z );
		}

		/// <inheritdoc cref="Vector2F.Multiply(Vector2F,float)"/>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F Multiply( Vector3F vector, float scalar )
		{
			return new Vector3F( vector.X * scalar, vector.Y * scalar, vector.Z * scalar );
		}

		/// <inheritdoc cref="Vector2F.Multiply(float,Vector2F)"/>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F Multiply( float scalar, Vector3F vector )
		{
			return new Vector3F( vector.X * scalar, vector.Y * scalar, vector.Z * scalar );
		}

		/// <inheritdoc cref="Vector2F.DotProduct"/>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DotProduct( Vector3F vector1, Vector3F vector2 )
		{
			return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
		}

		/// <inheritdoc cref="Vector2F.DotProduct"/>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		internal static float DotProduct( ref Vector3F vector1, ref Vector3F vector2 )
		{
			return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
		}

		/// <summary>
		///  Calculates the cross product.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>vector1 x vector2</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F CrossProduct( Vector3F vector1, Vector3F vector2 )
		{
			return new Vector3F(
				vector1.Y * vector2.Z - vector1.Z * vector2.Y,
				vector1.Z * vector2.X - vector1.X * vector2.Z,
				vector1.X * vector2.Y - vector1.Y * vector2.X
			);
		}

		/// <summary>
		/// Equalses the specified vectors.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>True if the vectors can be considered equal. Otherwise false.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool Equals( Vector3F vector1, Vector3F vector2 )
		{
			return vector1.X == vector2.X
			       && vector1.Y == vector2.Y
			       && vector1.Z == vector2.Z;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="o">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		/// True if the specified <see cref="System.Object" /> is equal to this instance. Otherwise false.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override bool Equals( object o )
		{
			if( !( o is Vector3F ) )
				return false;
			return Equals( this, (Vector3F)o );
		}

		/// <summary>
		/// Equalses the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>True if the given value can be considered equal to this instance. Otherwise false.</returns>
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

		public override string ToString()
		{
			return $"[{X} | {Y} | {Z}]";
		}
	}
}