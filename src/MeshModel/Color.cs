#region copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss Industrielle Messtechnik GmbH        */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2021                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

using System.Runtime.InteropServices;

namespace Zeiss.PiWeb.MeshModel
{
	#region usings

	using System;

	#endregion

	/// <summary>
	/// Describes a color of a color scheme.
	/// </summary>
	/// <remarks>
	/// We use our own color struct, because the .net color struct is significantly slower.
	/// </remarks>
	[Serializable, StructLayout( LayoutKind.Sequential, Size = Stride, Pack = 1 )]
	public readonly struct Color : IEquatable<Color>
	{
		public const int Stride = sizeof( byte ) * 4;

		#region members

		/// <summary>
		/// Red channel.
		/// </summary>
		public readonly byte R;

		/// <summary>
		/// Green channel.
		/// </summary>
		public readonly byte G;

		/// <summary>
		/// Blue channel.
		/// </summary>
		public readonly byte B;

		/// <summary>
		/// Alpha channel.
		/// </summary>
		public readonly byte A;

		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Color"/> struct from ARGB values.
		/// </summary>
		/// <param name="a">Alpha channel.</param>
		/// <param name="r">Red channel.</param>
		/// <param name="g">Green channel.</param>
		/// <param name="b">Blue channel.</param>
		private Color( byte a, byte r, byte g, byte b )
		{
			A = a;
			R = r;
			G = g;
			B = b;
		}

		#endregion

		#region methods

		/// <summary>
		/// Initializes a new instance of the <see cref="Color"/> struct from RGB values, with a default alpha value of 255 (opaque).
		/// </summary>
		/// <param name="r">Red channel.</param>
		/// <param name="g">Green channel.</param>
		/// <param name="b">Blue channel.</param>
		/// <returns></returns>
		public static Color FromRgb( byte r, byte g, byte b )
		{
			return new Color( 255, r, g, b );
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Color"/> struct from ARGB values.
		/// </summary>
		/// <param name="a">Alpha channel.</param>
		/// <param name="r">Red channel.</param>
		/// <param name="g">Green channel.</param>
		/// <param name="b">Blue channel.</param>
		public static Color FromArgb( byte a, byte r, byte g, byte b )
		{
			return new Color( a, r, g, b );
		}

		/// <summary>
		/// Gets the R, G, B and A bytes of this struct packed into a 32 Bit float value.
		/// </summary>
		/// <returns></returns>
		public float ToPackedArgb()
		{
			return BitConverter.ToSingle( new[] { A, R, G, B }, 0 );
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Color"/> struct from a hex string in the form of '#AARRGGBB' or 'AARRGGBB'.
		/// </summary>
		/// <param name="value">The hex string representation of a color.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static Color ParseArgb( string value )
		{
			var trimmed = value.TrimStart( '#' );
			if( trimmed.Length != 8 )
				throw new ArgumentException( $"Invalid ARGB string '{value}'" );

			return new Color(
				Convert.ToByte( trimmed.Substring( 0, 2 ), 16 ),
				Convert.ToByte( trimmed.Substring( 2, 2 ), 16 ),
				Convert.ToByte( trimmed.Substring( 4, 2 ), 16 ),
				Convert.ToByte( trimmed.Substring( 6, 2 ), 16 )
			);
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals( object obj )
		{
			if( ReferenceEquals( null, obj ) ) return false;
			return obj is Color && Equals( (Color)obj );
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
				var hashCode = A.GetHashCode();
				hashCode = ( hashCode * 397 ) ^ R.GetHashCode();
				hashCode = ( hashCode * 397 ) ^ G.GetHashCode();
				hashCode = ( hashCode * 397 ) ^ B.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="color1">The color1.</param>
		/// <param name="color2">The color2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==( Color color1, Color color2 )
		{
			return color1.Equals( color2 );
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="color1">The color1.</param>
		/// <param name="color2">The color2.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=( Color color1, Color color2 )
		{
			return !color1.Equals( color2 );
		}

		#endregion

		#region interface IEquatable<Color>

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		public bool Equals( Color other )
		{
			return A == other.A && R == other.R && G == other.G && B == other.B;
		}

		#endregion
	}
}