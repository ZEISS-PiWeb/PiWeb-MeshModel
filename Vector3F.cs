#region Copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss IMT (IZfM Dresden)                   */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2017                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace Zeiss.IMT.PiWeb.Meshmodels
{
	#region usings

	using System;

	#endregion

	/// <summary>
	/// Describes a vector with three <code>float</code> components. Adapted from the .NET Vector3D class.
	/// </summary>
	public struct Vector3F
	{
		public float Length => ( float ) Math.Sqrt( X * X + Y * Y + Z * Z );

		public float LengthSquared => X * X + Y * Y + Z * Z;

		public float X { get; set; }

		public float Y { get; set; }

		public float Z { get; set; }

		public Vector3F( float x, float y, float z )
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static explicit operator Point3F( Vector3F vector )
		{
			return new Point3F( vector.X, vector.Y, vector.Z );
		}

		public static explicit operator Size3F( Vector3F vector )
		{
			return new Size3F( Math.Abs( vector.X ), Math.Abs( vector.Y ), Math.Abs( vector.Z ) );
		}

		public static Vector3F operator -( Vector3F vector )
		{
			return new Vector3F( -vector.X, -vector.Y, -vector.Z );
		}

		public static Vector3F operator +( Vector3F vector1, Vector3F vector2 )
		{
			return new Vector3F( vector1.X + vector2.X, vector1.Y + vector2.Y, vector1.Z + vector2.Z );
		}

		public static Vector3F operator -( Vector3F vector1, Vector3F vector2 )
		{
			return new Vector3F( vector1.X - vector2.X, vector1.Y - vector2.Y, vector1.Z - vector2.Z );
		}

		public static Point3F operator +( Vector3F vector, Point3F point )
		{
			return new Point3F( vector.X + point.X, vector.Y + point.Y, vector.Z + point.Z );
		}

		public static Point3F operator -( Vector3F vector, Point3F point )
		{
			return new Point3F( vector.X - point.X, vector.Y - point.Y, vector.Z - point.Z );
		}

		public static Vector3F operator *( Vector3F vector, float scalar )
		{
			return new Vector3F( vector.X * scalar, vector.Y * scalar, vector.Z * scalar );
		}

		public static Vector3F operator *( float scalar, Vector3F vector )
		{
			return new Vector3F( vector.X * scalar, vector.Y * scalar, vector.Z * scalar );
		}

		public static Vector3F operator /( Vector3F vector, float scalar )
		{
			return vector * ( 1.0f / scalar );
		}
		
		public static bool operator ==( Vector3F vector1, Vector3F vector2 )
		{
			if( vector1.X == vector2.X &&
			    vector1.Y == vector2.Y )
				return vector1.Z == vector2.Z;
			return false;
		}

		public static bool operator !=( Vector3F vector1, Vector3F vector2 )
		{
			return !( vector1 == vector2 );
		}

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

		public static float AngleBetween( Vector3F vector1, Vector3F vector2 )
		{
			vector1.Normalize();
			vector2.Normalize();
			return RadiansToDegrees( DotProduct( vector1, vector2 ) >= 0.0f ? 2.0f * ( float ) Math.Asin( ( vector1 - vector2 ).Length / 2.0f ) : ( float ) Math.PI - 2.0f * ( float ) Math.Asin( ( -vector1 - vector2 ).Length / 2.0f ) );
		}

		private static float RadiansToDegrees( float value )
		{
			return value * 180 / ( float ) Math.PI;
		}

		public void Negate()
		{
			X = -X;
			Y = -Y;
			Z = -Z;
		}

		public static Vector3F Add( Vector3F vector1, Vector3F vector2 )
		{
			return new Vector3F( vector1.X + vector2.X, vector1.Y + vector2.Y, vector1.Z + vector2.Z );
		}

		public static Vector3F Subtract( Vector3F vector1, Vector3F vector2 )
		{
			return new Vector3F( vector1.X - vector2.X, vector1.Y - vector2.Y, vector1.Z - vector2.Z );
		}

		public static Point3F Add( Vector3F vector, Point3F point )
		{
			return new Point3F( vector.X + point.X, vector.Y + point.Y, vector.Z + point.Z );
		}

		public static Point3F Subtract( Vector3F vector, Point3F point )
		{
			return new Point3F( vector.X - point.X, vector.Y - point.Y, vector.Z - point.Z );
		}

		public static Vector3F Multiply( Vector3F vector, float scalar )
		{
			return new Vector3F( vector.X * scalar, vector.Y * scalar, vector.Z * scalar );
		}

		public static Vector3F Multiply( float scalar, Vector3F vector )
		{
			return new Vector3F( vector.X * scalar, vector.Y * scalar, vector.Z * scalar );
		}

		public static float DotProduct( Vector3F vector1, Vector3F vector2 )
		{
			return DotProduct( ref vector1, ref vector2 );
		}

		internal static float DotProduct( ref Vector3F vector1, ref Vector3F vector2 )
		{
			return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
		}

		public static Vector3F CrossProduct( Vector3F vector1, Vector3F vector2 )
		{
			Vector3F result;
			CrossProduct( ref vector1, ref vector2, out result );
			return result;
		}

		private static void CrossProduct( ref Vector3F vector1, ref Vector3F vector2, out Vector3F result )
		{
			result = new Vector3F(
				vector1.Y * vector2.Z - vector1.Z * vector2.Y,
				vector1.Z * vector2.X - vector1.X * vector2.Z,
				vector1.X * vector2.Y - vector1.Y * vector2.X
			);
		}

		public static bool Equals( Vector3F vector1, Vector3F vector2 )
		{
			if( vector1.X.Equals( vector2.X ) && vector1.Y.Equals( vector2.Y ) )
				return vector1.Z.Equals( vector2.Z );
			return false;
		}

		public override bool Equals( object o )
		{
			if( !( o is Vector3F ) )
				return false;
			return Equals( this, ( Vector3F ) o );
		}

		public bool Equals( Vector3F value )
		{
			return Equals( this, value );
		}

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