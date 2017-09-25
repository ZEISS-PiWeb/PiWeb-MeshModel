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
	/// Describes a cuboid with floating point coordinates. Adapted from the .NET Size3D class.
	/// </summary>
	public struct Size3F : IEquatable<Size3F>
	{
		private float _X;
		private float _Y;
		private float _Z;

		public static Size3F Empty => CreateEmptySize();

		public bool IsEmpty => _X < 0.0;

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
			if( size1.X == size2.X &&
			    size1.Y == size2.Y )
				return size1.Z == size2.Z;
			return false;
		}

		public static bool operator !=( Size3F size1, Size3F size2 )
		{
			return !( size1 == size2 );
		}

		public static explicit operator Vector3F(Size3F size)
		{
			return new Vector3F(size.X, size.Y, size.Z);
		}

		public static explicit operator Point3F(Size3F size)
		{
			return new Point3F(size.X, size.Y, size.Z);
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

		public static bool Equals( Size3F size1, Size3F size2 )
		{
			if( size1.IsEmpty )
				return size2.IsEmpty;
			if( size1.X.Equals( size2.X ) && size1.Y.Equals( size2.Y ) )
				return size1.Z.Equals( size2.Z );
			return false;
		}

		public override bool Equals( object o )
		{
			if( !( o is Size3F ) )
				return false;
			return Equals( this, ( Size3F ) o );
		}

		public bool Equals( Size3F value )
		{
			return Equals( this, value );
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = _X.GetHashCode();
				hashCode = (hashCode * 397) ^ _Y.GetHashCode();
				hashCode = (hashCode * 397) ^ _Z.GetHashCode();
				return hashCode;
			}
		}
	}
}