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

	#endregion

	/// <summary>
	/// Describes a triangle consisting of three 3F coordinates.
	/// </summary>
	public struct Triangle3F : IEquatable<Triangle3F>
	{
		private Vector3F _A;
		private Vector3F _B;
		private Vector3F _C;

		private float? _Area;

		/// <summary>
		/// Gets the an empty triangle.
		/// </summary>
		/// <value>
		/// An empty triangle.
		/// </value>
		public static Triangle3F Empty { get; } = CreateEmptyTriangle();

		/// <summary>
		/// Gets a value indicating whether this instance is empty.
		/// </summary>
		/// <value>
		/// <see langword="True"/> if this instance is empty; otherwise, <see langword="False"/>.
		/// </value>
		public bool IsEmpty => _Area < 0;

		public float Area
		{
			get
			{
				if( _Area != null ) return _Area.GetValueOrDefault();

				if( A.Equals( B ) && B.Equals( C ) )
					_Area = 0;
				else
				{
					var vX = new Vector3F( B.X - A.X, B.Y - A.Y, B.Z - A.Z );
					var vY = new Vector3F( C.X - A.X, C.Y - A.Y, C.Z - A.Z );

					var cross = Vector3F.CrossProduct( vX, vY );
					_Area = cross.Length / 2;
					_Normal = cross;
					_Normal?.Normalize();
				}

				return _Area.GetValueOrDefault();
			}
		}

		private Vector3F? _Normal;

		/// <summary>
		/// Normal is calculated in a right hand order.
		/// </summary>
		public Vector3F Normal
		{
			get
			{
				if( _Normal != null ) return _Normal.GetValueOrDefault();

				if( A.Equals( B ) || B.Equals( C ) || C.Equals( A ) )
					_Normal = new Vector3F( 0, 0, 0 );
				else
				{
					var vX = new Vector3F( B.X - A.X, B.Y - A.Y, B.Z - A.Z );
					var vY = new Vector3F( C.X - A.X, C.Y - A.Y, C.Z - A.Z );

					_Normal = Vector3F.CrossProduct( vX, vY );
					_Normal?.Normalize();
				}

				return _Normal.GetValueOrDefault();
			}
		}

		/// <summary>
		/// Gets or sets the coordinate A.
		/// </summary>
		/// <exception cref="InvalidOperationException">An empty triangle cannot be modified</exception>
		public Vector3F A
		{
			get => _A;
			set
			{
				if( IsEmpty )
					throw new InvalidOperationException( "An empty triangle cannot be modified" );
				if( float.IsNaN( value.X ) || float.IsNaN( value.Y ) || float.IsNaN( value.Z ) )
					throw new ArgumentException( "NaN not allowed." );
				_A = value;
			}
		}

		/// <summary>
		/// Gets or sets the coordinate B.
		/// </summary>
		/// <exception cref="InvalidOperationException">An empty triangle cannot be modified</exception>
		public Vector3F B
		{
			get => _B;
			set
			{
				if( IsEmpty )
					throw new InvalidOperationException( "An empty triangle cannot be modified" );
				if( float.IsNaN( value.X ) || float.IsNaN( value.Y ) || float.IsNaN( value.Z ) )
					throw new ArgumentException( "NaN not allowed." );
				_B = value;
			}
		}

		/// <summary>
		/// Gets or sets the coordinate C.
		/// </summary>
		/// <exception cref="InvalidOperationException">An empty triangle cannot be modified</exception>
		public Vector3F C
		{
			get => _C;
			set
			{
				if( IsEmpty )
					throw new InvalidOperationException( "An empty triangle cannot be modified" );
				if( float.IsNaN( value.X ) || float.IsNaN( value.Y ) || float.IsNaN( value.Z ) )
					throw new ArgumentException( "NaN not allowed." );
				_C = value;
			}
		}

		public void Initialize( Vector3F a, Vector3F b, Vector3F c )
		{
			A = a;
			B = b;
			C = c;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Triangle3F"/> struct.
		/// </summary>
		public Triangle3F( Vector3F a, Vector3F b, Vector3F c )
		{
			if( float.IsNaN( a.X )
				|| float.IsNaN( a.Y )
				|| float.IsNaN( a.Z )
				|| float.IsNaN( b.X )
				|| float.IsNaN( b.Y )
				|| float.IsNaN( b.Z )
				|| float.IsNaN( c.X )
				|| float.IsNaN( c.Y )
				|| float.IsNaN( c.Z ) )
				throw new ArgumentException( "NaN is not a valid value." );

			_A = a;
			_B = b;
			_C = c;

			_Area = null;
			_Normal = null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Triangle3F"/> struct.
		/// </summary>
		/// <exception cref="ArgumentException">A rect cannot have a negative dimension</exception>
		public Triangle3F( float aX, float aY, float aZ, float bX, float bY, float bZ, float cX, float cY, float cZ )
			:
			this
			(
				new Vector3F( aX, aY, aZ ),
				new Vector3F( aX, aY, aZ ),
				new Vector3F( aX, aY, aZ )
			)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="Triangle3F"/> struct using an array of coordinate values.
		/// </summary>
		/// <param name="coordinateValues">An array of coordinate values in this form: [aX, aY, aZ, bX, bY, ... }.</param>
		public Triangle3F( float[] coordinateValues ) : this
		(
			coordinateValues.Length != 9 ? throw new ArgumentException( "A triangle consists of exactly 9 coordinate components." ) : coordinateValues[ 0 ],
			coordinateValues[ 1 ],
			coordinateValues[ 2 ],
			coordinateValues[ 3 ],
			coordinateValues[ 4 ],
			coordinateValues[ 5 ],
			coordinateValues[ 6 ],
			coordinateValues[ 7 ],
			coordinateValues[ 8 ]
		)
		{ }

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="triangle1">The first triangle.</param>
		/// <param name="triangle2">The second triangle.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==( Triangle3F triangle1, Triangle3F triangle2 )
		{
			return triangle1.Equals( triangle2 );
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="triangle1">The first triangle.</param>
		/// <param name="triangle2">The second triangle.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=( Triangle3F triangle1, Triangle3F triangle2 )
		{
			return !( triangle1 == triangle2 );
		}

		private static Triangle3F CreateEmptyTriangle()
		{
			return new Triangle3F
			(
				float.PositiveInfinity,
				float.PositiveInfinity,
				float.PositiveInfinity,
				float.PositiveInfinity,
				float.PositiveInfinity,
				float.PositiveInfinity,
				float.PositiveInfinity,
				float.PositiveInfinity,
				float.PositiveInfinity
			) { _Area = -1f };
		}

		/// <summary>
		/// Equalises the specified triangle1.
		/// </summary>
		/// <param name="triangle1">The triangle1.</param>
		/// <param name="triangle2">The triangle2.</param>
		/// <returns></returns>
		public static bool Equals( Triangle3F triangle1, Triangle3F triangle2 )
		{
			if( triangle1.IsEmpty )
				return triangle2.IsEmpty;

			return triangle1.A.Equals( triangle2.A )
					&& triangle1.B.Equals( triangle2.B )
					&& triangle1.C.Equals( triangle2.C );
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
			if( !( o is Triangle3F ) )
				return false;
			return Equals( this, (Triangle3F)o );
		}

		/// <summary>
		/// Equalises the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public bool Equals( Triangle3F value )
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
				var hashCode = _A.GetHashCode();
				hashCode = ( hashCode * 397 ) ^ _B.GetHashCode();
				hashCode = ( hashCode * 397 ) ^ _C.GetHashCode();
				return hashCode;
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Triangle. Points: [{A}, {B}, {C}], Area: {Area}";
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
			A = new Vector3F( A.X * scaleX, A.Y * scaleY, A.Z * scaleZ );
			B = new Vector3F( B.X * scaleX, B.Y * scaleY, B.Z * scaleZ );
			C = new Vector3F( C.X * scaleX, C.Y * scaleY, C.Z * scaleZ );
		}

		/// <summary>
		/// Creates an absolute scaled triangle from an existing rectangle relative to center.
		/// </summary>
		/// <param name="triangle">The triangle.</param>
		/// <param name="scaleX">Absolute value to scale in x dimension.</param>
		/// <param name="scaleY">Absolute value to scale in y dimension.</param>
		/// <param name="scaleZ">Absolute value to scale in z dimension.</param>
		/// <returns></returns>
		public static Triangle3F ScaleAbsolute( Triangle3F triangle, float scaleX, float scaleY, float scaleZ )
		{
			triangle.ScaleAbsolute( scaleX, scaleY, scaleZ );
			return triangle;
		}
	}
}