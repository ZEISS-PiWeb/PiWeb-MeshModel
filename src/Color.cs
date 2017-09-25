#region Copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss IMT (IZfM Dresden)                   */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2010                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

namespace Zeiss.IMT.PiWeb.MeshModel
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
	public struct Color : IEquatable<Color>
	{
		public readonly byte A;
		public readonly byte R;
		public readonly byte G;
		public readonly byte B;

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
		
		public bool Equals( Color other )
		{
			return A == other.A && R == other.R && G == other.G && B == other.B;
		}
		
		public override bool Equals( object obj )
		{
			if( ReferenceEquals( null, obj ) ) return false;
			return obj is Color && Equals( ( Color ) obj );
		}
		
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
		
		public static bool operator ==( Color color1, Color color2 )
		{
			return color1.Equals( color2 );
		}

		public static bool operator !=( Color color1, Color color2 )
		{
			return !color1.Equals( color2 );
		}
	}
}