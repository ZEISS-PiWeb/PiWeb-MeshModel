#region Copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss IMT (IZfM Dresden)                   */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2017                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

// ReSharper disable NonReadonlyMemberInGetHashCode
namespace Zeiss.IMT.PiWeb.Meshmodels
{
	#region usings

	using System;

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

		public static Rect3F Empty { get; } = CreateEmptyCuboid();

		public bool IsEmpty => _SizeX < 0.0;

		public Point3F Location
		{
			get { return new Point3F(_X, _Y, _Z); }
			set
			{
				if (IsEmpty)
					throw new InvalidOperationException("An empty rect cannot be modified");
				_X = value.X;
				_Y = value.Y;
				_Z = value.Z;
			}
		}

		public Size3F Size
		{
			get { return IsEmpty ? Size3F.Empty : new Size3F(_SizeX, _SizeY, _SizeZ); }
			set
			{
				if (value.IsEmpty)
				{
					this = Empty;
				}
				else
				{
					if (IsEmpty)
						throw new InvalidOperationException("An empty rect cannot be modified");
					_SizeX = value.X;
					_SizeY = value.Y;
					_SizeZ = value.Z;
				}
			}
		}

		public float SizeX
		{
			get { return _SizeX; }
			set
			{
				if (IsEmpty)
					throw new InvalidOperationException("An empty rect cannot be modified");
				if (value < 0.0)
					throw new ArgumentException("A rect cannot have a negative dimension");
				_SizeX = value;
			}
		}

		public float SizeY
		{
			get { return _SizeY; }
			set
			{
				if (IsEmpty)
					throw new InvalidOperationException("An empty rect cannot be modified");
				if (value < 0.0)
					throw new ArgumentException("A rect cannot have a negative dimension");
				_SizeY = value;
			}
		}

		public float SizeZ
		{
			get { return _SizeZ; }
			set
			{
				if (IsEmpty)
					throw new InvalidOperationException("An empty rect cannot be modified");
				if (value < 0.0)
					throw new ArgumentException("A rect cannot have a negative dimension");
				_SizeZ = value;
			}
		}

		public float X
		{
			get { return _X; }
			set
			{
				if (IsEmpty)
					throw new InvalidOperationException("An empty rect cannot be modified");
				_X = value;
			}
		}

		public float Y
		{
			get { return _Y; }
			set
			{
				if (IsEmpty)
					throw new InvalidOperationException("An empty rect cannot be modified");
				_Y = value;
			}
		}

		public float Z
		{
			get { return _Z; }
			set
			{
				if (IsEmpty)
					throw new InvalidOperationException("An empty rect cannot be modified");
				_Z = value;
			}
		}

		public Rect3F(Point3F location, Size3F size)
		{
			if (size.IsEmpty)
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

		public Rect3F(float x, float y, float z, float sizeX, float sizeY, float sizeZ)
		{
			if (sizeX < 0.0 || sizeY < 0.0 || sizeZ < 0.0)
				throw new ArgumentException("A rect cannot have a negative dimension");
			_X = x;
			_Y = y;
			_Z = z;
			_SizeX = sizeX;
			_SizeY = sizeY;
			_SizeZ = sizeZ;
		}

		internal Rect3F(Point3F point1, Point3F point2)
		{
			_X = Math.Min(point1.X, point2.X);
			_Y = Math.Min(point1.Y, point2.Y);
			_Z = Math.Min(point1.Z, point2.Z);
			_SizeX = Math.Max(point1.X, point2.X) - _X;
			_SizeY = Math.Max(point1.Y, point2.Y) - _Y;
			_SizeZ = Math.Max(point1.Z, point2.Z) - _Z;
		}

		public static bool operator ==(Rect3F rect1, Rect3F rect2)
		{
			return rect1.Equals(rect2);
		}

		public static bool operator !=(Rect3F rect1, Rect3F rect2)
		{
			return !(rect1 == rect2);
		}

		public bool Contains(Point3F point)
		{
			return Contains(point.X, point.Y, point.Z);
		}

		public bool Contains(float x, float y, float z)
		{
			if (IsEmpty)
				return false;
			return ContainsInternal(x, y, z);
		}

		public bool Contains(Rect3F rect)
		{
			if (IsEmpty || rect.IsEmpty || (_X > rect._X || _Y > rect._Y) || (_Z > rect._Z || _X + _SizeX < rect._X + rect._SizeX || _Y + _SizeY < rect._Y + rect._SizeY))
				return false;
			return _Z + _SizeZ >= rect._Z + rect._SizeZ;
		}

		public bool IntersectsWith(Rect3F rect)
		{
			if (IsEmpty || rect.IsEmpty || (rect._X > _X + _SizeX || rect._X + rect._SizeX < _X) || (rect._Y > _Y + _SizeY || rect._Y + rect._SizeY < _Y || rect._Z > _Z + _SizeZ))
				return false;
			return rect._Z + rect._SizeZ >= _Z;
		}

		public void Intersect(Rect3F rect)
		{
			if (IsEmpty || rect.IsEmpty || !IntersectsWith(rect))
			{
				this = Empty;
			}
			else
			{
				var num1 = Math.Max(_X, rect._X);
				var num2 = Math.Max(_Y, rect._Y);
				var num3 = Math.Max(_Z, rect._Z);
				_SizeX = Math.Min(_X + _SizeX, rect._X + rect._SizeX) - num1;
				_SizeY = Math.Min(_Y + _SizeY, rect._Y + rect._SizeY) - num2;
				_SizeZ = Math.Min(_Z + _SizeZ, rect._Z + rect._SizeZ) - num3;
				_X = num1;
				_Y = num2;
				_Z = num3;
			}
		}

		public static Rect3F Intersect(Rect3F rect1, Rect3F rect2)
		{
			rect1.Intersect(rect2);
			return rect1;
		}

		public void Union(Rect3F rect)
		{
			if (IsEmpty)
			{
				this = rect;
			}
			else
			{
				if (rect.IsEmpty)
					return;
				var num1 = Math.Min(_X, rect._X);
				var num2 = Math.Min(_Y, rect._Y);
				var num3 = Math.Min(_Z, rect._Z);
				_SizeX = Math.Max(_X + _SizeX, rect._X + rect._SizeX) - num1;
				_SizeY = Math.Max(_Y + _SizeY, rect._Y + rect._SizeY) - num2;
				_SizeZ = Math.Max(_Z + _SizeZ, rect._Z + rect._SizeZ) - num3;
				_X = num1;
				_Y = num2;
				_Z = num3;
			}
		}

		public static Rect3F Union(Rect3F rect1, Rect3F rect2)
		{
			rect1.Union(rect2);
			return rect1;
		}

		public void Union(Point3F point)
		{
			Union(new Rect3F(point, point));
		}

		public static Rect3F Union(Rect3F rect, Point3F point)
		{
			rect.Union(new Rect3F(point, point));
			return rect;
		}

		private bool ContainsInternal(float x, float y, float z)
		{
			if (x >= _X && x <= _X + _SizeX && (y >= _Y && y <= _Y + _SizeY) && z >= _Z)
				return z <= _Z + _SizeZ;
			return false;
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

		public static bool Equals(Rect3F rect1, Rect3F rect2)
		{
			if (rect1.IsEmpty)
				return rect2.IsEmpty;

			if (rect1.X.Equals(rect2.X) &&
				rect1.Y.Equals(rect2.Y) &&
				rect1.Z.Equals(rect2.Z) &&
				rect1.SizeX.Equals(rect2.SizeX) &&
				rect1.SizeY.Equals(rect2.SizeY))
				return rect1.SizeZ.Equals(rect2.SizeZ);
			return false;
		}

		public override bool Equals(object o)
		{
			if (!(o is Rect3F))
				return false;
			return Equals(this, (Rect3F)o);
		}

		public bool Equals(Rect3F value)
		{
			return Equals(this, value);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = _X.GetHashCode();
				hashCode = (hashCode * 397) ^ _Y.GetHashCode();
				hashCode = (hashCode * 397) ^ _Z.GetHashCode();
				hashCode = (hashCode * 397) ^ _SizeX.GetHashCode();
				hashCode = (hashCode * 397) ^ _SizeY.GetHashCode();
				hashCode = (hashCode * 397) ^ _SizeZ.GetHashCode();
				return hashCode;
			}
		}
	}
}
