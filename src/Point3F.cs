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
	using System.Runtime.CompilerServices;

	/// <summary>
	/// Describes a point in 3D space with floating point coordinates. Adapted from the .NET Point3D class.
	/// </summary>
	public struct Point3F
	{
		/// <summary>
		/// Gets or sets the x value.
		/// </summary>
		/// <value>
		/// The x value.
		/// </value>
		public float X { get; set; }

		/// <summary>
		/// Gets or sets the y value.
		/// </summary>
		/// <value>
		/// The y value.
		/// </value>
		public float Y { get; set; }

		/// <summary>
		/// Gets or sets the z value.
		/// </summary>
		/// <value>
		/// The z value.
		/// </value>
		public float Z { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Point3F"/> struct.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="z">The z.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Point3F( float x, float y, float z )
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="point1">The point1.</param>
		/// <param name="point2">The point2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool operator ==( Point3F point1, Point3F point2 )
		{
			if( point1.X == point2.X &&
			    point1.Y == point2.Y )
				return point1.Z == point2.Z;

			return false;
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="point1">The point1.</param>
		/// <param name="point2">The point2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool operator !=( Point3F point1, Point3F point2 )
		{
			return !( point1 == point2 );
		}

		/// <summary>
		/// Implements the operator +.
		/// </summary>
		/// <param name="point">The point.</param>
		/// <param name="vector">The vector.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Point3F operator +( Point3F point, Vector3F vector )
		{
			return new Point3F( point.X + vector.X, point.Y + vector.Y, point.Z + vector.Z );
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="point">The point.</param>
		/// <param name="vector">The vector.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Point3F operator -( Point3F point, Vector3F vector )
		{
			return new Point3F( point.X - vector.X, point.Y - vector.Y, point.Z - vector.Z );
		}

		/// <summary>
		/// Implements the operator -.
		/// </summary>
		/// <param name="point1">The point1.</param>
		/// <param name="point2">The point2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3F operator -( Point3F point1, Point3F point2 )
		{
			return new Vector3F( point1.X - point2.X, point1.Y - point2.Y, point1.Z - point2.Z );
		}

		/// <summary>
		/// Offsets the point.
		/// </summary>
		/// <param name="offsetX">The offset x.</param>
		/// <param name="offsetY">The offset y.</param>
		/// <param name="offsetZ">The offset z.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void Offset( float offsetX, float offsetY, float offsetZ )
		{
			X = X + offsetX;
			Y = Y + offsetY;
			Z = Z + offsetZ;
		}

		/// <summary>
		/// Equalses the specified point1.
		/// </summary>
		/// <param name="point1">The point1.</param>
		/// <param name="point2">The point2.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool Equals( Point3F point1, Point3F point2 )
		{
			if( point1.X.Equals( point2.X ) && point1.Y.Equals( point2.Y ) )
				return point1.Z.Equals( point2.Z );
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
			if( !( o is Point3F ) )
				return false;

			return Equals( this, ( Point3F ) o );
		}

		/// <summary>
		/// Equalses the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool Equals( Point3F value )
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
			var num1 = X;
			var hashCode1 = num1.GetHashCode();
			num1 = Y;
			var hashCode2 = num1.GetHashCode();
			var num2 = hashCode1 ^ hashCode2;
			num1 = Z;
			var hashCode3 = num1.GetHashCode();
			return num2 ^ hashCode3;
		}

		/// <summary>
		/// Support explicit conversion from point to vector when used as position vector.
		/// </summary>
		/// <param name="point">Source <see cref="Point3F"/> instance.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static explicit operator Vector3F( Point3F point )
		{
			return new Vector3F( point.X, point.Y, point.Z );
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"[{X} {Y} {Z}]";
		}
	}
}