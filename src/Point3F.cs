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
	using Zeiss.IMT.PiWeb.Meshmodels;

	/// <summary>
	/// Describes a point in 3D space with floating point coordinates. Adapted from the .NET Point3D class.
	/// </summary>
	public struct Point3F
	{
		public float X { get; set; }

		public float Y { get; set; }

		public float Z { get; set; }

		public Point3F( float x, float y, float z )
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static bool operator ==( Point3F point1, Point3F point2 )
		{
			if( point1.X == point2.X &&
			    point1.Y == point2.Y )
				return point1.Z == point2.Z;

			return false;
		}

		public static bool operator !=( Point3F point1, Point3F point2 )
		{
			return !( point1 == point2 );
		}

		public static Point3F operator +( Point3F point, Vector3F vector )
		{
			return new Point3F( point.X + vector.X, point.Y + vector.Y, point.Z + vector.Z );
		}

		public static Point3F operator -( Point3F point, Vector3F vector )
		{
			return new Point3F( point.X - vector.X, point.Y - vector.Y, point.Z - vector.Z );
		}

		public static Vector3F operator -( Point3F point1, Point3F point2 )
		{
			return new Vector3F( point1.X - point2.X, point1.Y - point2.Y, point1.Z - point2.Z );
		}

		public void Offset( float offsetX, float offsetY, float offsetZ )
		{
			X = X + offsetX;
			Y = Y + offsetY;
			Z = Z + offsetZ;
		}

		public static bool Equals( Point3F point1, Point3F point2 )
		{
			if( point1.X.Equals( point2.X ) && point1.Y.Equals( point2.Y ) )
				return point1.Z.Equals( point2.Z );
			return false;
		}

		public override bool Equals( object o )
		{
			if( !( o is Point3F ) )
				return false;

			return Equals( this, ( Point3F ) o );
		}

		public bool Equals( Point3F value )
		{
			return Equals( this, value );
		}

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
	}
}