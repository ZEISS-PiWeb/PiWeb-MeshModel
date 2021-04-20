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
	/// Describes a vector with two <code>float</code> components. Adapted from the .NET Vector3D class.
	/// </summary>
	[StructLayout( LayoutKind.Sequential, Size = 8, Pack = 4 )]
	public struct Vector2F
	{
		/// <summary>
		/// Gets the length.
		/// </summary>
		/// <value>
		/// The length.
		/// </value>
		public float Length => (float)Math.Sqrt( LengthSquared );

		/// <summary>
		/// Gets the length squared.
		/// </summary>
		/// <value>
		/// The length squared.
		/// </value>
		public float LengthSquared => X * X + Y * Y;

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
		/// Initializes a new instance of the <see cref="Vector2F"/> struct.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Vector2F( float x, float y )
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2F operator -( Vector2F vector )
		{
			return new Vector2F( -vector.X, -vector.Y );
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
		public static Vector2F operator +( Vector2F vector1, Vector2F vector2 )
		{
			return new Vector2F( vector1.X + vector2.X, vector1.Y + vector2.Y );
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
		public static Vector2F operator -( Vector2F vector1, Vector2F vector2 )
		{
			return new Vector2F( vector1.X - vector2.X, vector1.Y - vector2.Y );
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
		public static Vector2F operator *( Vector2F vector, float scalar )
		{
			return new Vector2F( vector.X * scalar, vector.Y * scalar );
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
		public static Vector2F operator *( float scalar, Vector2F vector )
		{
			return new Vector2F( vector.X * scalar, vector.Y * scalar );
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
		public static Vector2F operator /( Vector2F vector, float scalar )
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
		public static bool operator ==( Vector2F vector1, Vector2F vector2 )
		{
			return vector1.Equals( vector2 );
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
		public static bool operator !=( Vector2F vector1, Vector2F vector2 )
		{
			return !( vector1 == vector2 );
		}

		/// <summary>
		/// Normalizes this instance.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Normalize()
		{
			this = this / Length;
		}

		/// <summary>
		/// Calculates the angle between the specified vectors.
		/// </summary>
		/// <param name="vector1">The vector1.</param>
		/// <param name="vector2">The vector2.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float AngleBetween( Vector2F vector1, Vector2F vector2 )
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

		/// <summary>
		/// Negates this instance.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Negate()
		{
			X = -X;
			Y = -Y;
		}

		/// <summary>
		/// Adds the specified vector1.
		/// </summary>
		/// <param name="vector1">The vector1.</param>
		/// <param name="vector2">The vector2.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2F Add( Vector2F vector1, Vector2F vector2 )
		{
			return new Vector2F( vector1.X + vector2.X, vector1.Y + vector2.Y );
		}

		/// <summary>
		/// Subtracts the specified vector1.
		/// </summary>
		/// <param name="vector1">The vector1.</param>
		/// <param name="vector2">The vector2.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2F Subtract( Vector2F vector1, Vector2F vector2 )
		{
			return new Vector2F( vector1.X - vector2.X, vector1.Y - vector2.Y );
		}

		/// <summary>
		/// Multiplies the specified vector with the specified scalar.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <param name="scalar">The scalar.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2F Multiply( Vector2F vector, float scalar )
		{
			return new Vector2F( vector.X * scalar, vector.Y * scalar );
		}

		/// <summary>
		/// Multiplies the specified scalar.
		/// </summary>
		/// <param name="scalar">The scalar.</param>
		/// <param name="vector">The vector.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2F Multiply( float scalar, Vector2F vector )
		{
			return new Vector2F( vector.X * scalar, vector.Y * scalar );
		}

		/// <summary>
		/// Calculates the dot product.
		/// </summary>
		/// <param name="vector1">The vector1.</param>
		/// <param name="vector2">The vector2.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DotProduct( Vector2F vector1, Vector2F vector2 )
		{
			return vector1.X * vector2.X + vector1.Y * vector2.Y;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		internal static float DotProduct( ref Vector2F vector1, ref Vector2F vector2 )
		{
			return vector1.X * vector2.X + vector1.Y * vector2.Y;
		}

		/// <summary>
		/// Equalises the specified vector1.
		/// </summary>
		/// <param name="vector1">The vector1.</param>
		/// <param name="vector2">The vector2.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool Equals( Vector2F vector1, Vector2F vector2 )
		{
			return vector1.X == vector2.X
			       && vector1.Y == vector2.Y;
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
			return o is Vector2F f && Equals( this, f );
		}

		/// <summary>
		/// Equalses the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool Equals( Vector2F value )
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
				return hashCode;
			}
		}

		public override string ToString()
		{
			return $"[{X} | {Y}]";
		}
	}
}