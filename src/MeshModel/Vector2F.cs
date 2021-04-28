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
	[StructLayout( LayoutKind.Sequential, Size = Stride, Pack = 4 )]
	public struct Vector2F : IEquatable<Vector2F>
	{
		public const int Stride = sizeof( float ) * 2;

		/// <summary>
		/// Gets the vector's length.
		/// </summary>
		public float Length => (float)Math.Sqrt( LengthSquared );

		/// <summary>
		/// Gets the vector's length squared.
		/// </summary>
		public float LengthSquared => X * X + Y * Y;

		/// <summary>
		/// Gets or sets the X component.
		/// </summary>
		public float X { get; set; }

		/// <summary>
		/// Gets or sets the Y component.
		/// </summary>
		public float Y { get; set; }

		/// <summary>Constructor.</summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Vector2F( float x, float y )
		{
			X = x;
			Y = y;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2F operator -( Vector2F vector )
		{
			return new Vector2F( -vector.X, -vector.Y );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2F operator +( Vector2F vector1, Vector2F vector2 )
		{
			return new Vector2F( vector1.X + vector2.X, vector1.Y + vector2.Y );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2F operator -( Vector2F vector1, Vector2F vector2 )
		{
			return new Vector2F( vector1.X - vector2.X, vector1.Y - vector2.Y );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2F operator *( Vector2F vector, float scalar )
		{
			return new Vector2F( vector.X * scalar, vector.Y * scalar );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2F operator *( float scalar, Vector2F vector )
		{
			return new Vector2F( vector.X * scalar, vector.Y * scalar );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2F operator /( Vector2F vector, float scalar )
		{
			return vector * ( 1.0f / scalar );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool operator ==( Vector2F vector1, Vector2F vector2 )
		{
			return vector1.Equals( vector2 );
		}

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
		/// Returns a new normalized vector, based on this instance.
		/// </summary>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Vector2F GetNormalized()
		{
			var v = this;
			v.Normalize();
			return v;
		}

		/// <summary>
		/// Calculates the angle between the specified vectors.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>The angle between both vectors.</returns>
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

		/// <inheritdoc cref="Vector3F"/>
		/// <summary>
		/// Adds the given vectors.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>vector1 + vector2</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2F Add( Vector2F vector1, Vector2F vector2 )
		{
			return new Vector2F( vector1.X + vector2.X, vector1.Y + vector2.Y );
		}

		/// <summary>
		/// Subtracts the given vectors.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>vector1 - vector2</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2F Subtract( Vector2F vector1, Vector2F vector2 )
		{
			return new Vector2F( vector1.X - vector2.X, vector1.Y - vector2.Y );
		}

		/// <summary>
		/// Multiplies the given vector with the given scalar element-wise.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <param name="scalar">The scalar.</param>
		/// <returns>vector * scalar</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2F Multiply( Vector2F vector, float scalar )
		{
			return new Vector2F( vector.X * scalar, vector.Y * scalar );
		}

		/// <summary>
		/// Multiplies the given vector with the given scalar element-wise.
		/// </summary>
		/// <param name="scalar">The scalar.</param>
		/// <param name="vector">The vector.</param>
		/// <returns>scalar * vector</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2F Multiply( float scalar, Vector2F vector )
		{
			return new Vector2F( vector.X * scalar, vector.Y * scalar );
		}

		/// <summary>
		/// Calculates the dot product.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>vector1 * vector2</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float DotProduct( Vector2F vector1, Vector2F vector2 )
		{
			return vector1.X * vector2.X + vector1.Y * vector2.Y;
		}

		/// <inheritdoc cref="DotProduct"/>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		internal static float DotProduct( ref Vector2F vector1, ref Vector2F vector2 )
		{
			return vector1.X * vector2.X + vector1.Y * vector2.Y;
		}

		/// <inheritdoc />
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public override bool Equals( object o )
		{
			return o is Vector2F v && Equals( v );
		}

		/// <inheritdoc />
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool Equals( Vector2F value )
		{
			return X == value.X && Y == value.Y;
		}

		/// <inheritdoc />
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

		/// <inheritdoc />
		public override string ToString()
		{
			return $"[{X} | {Y}]";
		}
	}
}