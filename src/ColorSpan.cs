#region Copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss IMT (IZfM Dresden)                   */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2010                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

// ReSharper disable CompareOfFloatsByEqualityOperator
namespace Zeiss.IMT.PiWeb.MeshModel
{
	#region usings

	using System;
	using System.Runtime.CompilerServices;

	#endregion

	/// <summary>
	/// An interpolated span between two colors
	/// </summary>
	public sealed class ColorSpan
	{
		#region members

		private readonly double _RightValue;
		private readonly double _LeftValue;

		private readonly double _LeftColorH;
		private readonly double _LeftColorS;
		private readonly double _LeftColorL;

		private readonly double _DistS;
		private readonly double _DistL;
		private readonly double _DistH;
		private readonly double _DistLeftRight;
		private readonly byte _LeftColorR;
		private readonly byte _LeftColorG;
		private readonly byte _LeftColorB;
		private readonly byte _LeftColorA;
		private readonly int _DistR;
		private readonly int _DistB;
		private readonly int _DistG;
		private readonly int _DistA;

		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ColorSpan"/> class.
		/// </summary>
		/// <param name="leftValue">The left value.</param>
		/// <param name="rightValue">The right value.</param>
		/// <param name="leftColor">Color of the left.</param>
		/// <param name="rightColor">Color of the right.</param>
		public ColorSpan( float leftValue, float rightValue, Color leftColor, Color rightColor )
		{
			_LeftValue = leftValue;
			var leftColorHsl = RgbToHsl( leftColor );
			var rightColorHsl = RgbToHsl( rightColor );

			_RightValue = rightValue;

			_DistLeftRight = _RightValue - _LeftValue;

			_LeftColorR = leftColor.R;
			_LeftColorG = leftColor.G;
			_LeftColorB = leftColor.B;
			_LeftColorA = leftColor.A;

			_DistR = rightColor.R - leftColor.R;
			_DistG = rightColor.G - leftColor.G;
			_DistB = rightColor.B - leftColor.B;
			_DistA = rightColor.A - leftColor.A;

			_LeftColorH = leftColorHsl.Item1;
			_LeftColorS = leftColorHsl.Item2;
			_LeftColorL = leftColorHsl.Item3;

			_DistH = rightColorHsl.Item1 - leftColorHsl.Item1;
			_DistS = rightColorHsl.Item2 - leftColorHsl.Item2;
			_DistL = rightColorHsl.Item3 - leftColorHsl.Item3;

			if( _DistH > 0.5 )
				_DistH = _DistH - 1;

			IsSolidColor = leftColor == rightColor;
		}

		#endregion

		#region properties

		/// <summary>
		/// Gets a value indicating whether this span has only one color.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is solid; otherwise, <c>false</c>.
		/// </value>
		public bool IsSolidColor { get; }

		#endregion

		#region methods

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private Color GetColorHsv( double value )
		{
			value = ( Math.Max( _LeftValue, Math.Min( _RightValue, value ) ) - _LeftValue ) / _DistLeftRight;

			var h = _LeftColorH + _DistH * value;
			var s = _LeftColorS + _DistS * value;
			var l = _LeftColorL + _DistL * value;

			// The hue is actually a normalized angle (0 = 0° to 1 = 360°). This means we can interpolate in both directions to reach the desired hue.
			// Lets make sure we go the shorter route. 
			if( h < 0 )
				h += 1;

			h = Math.Max( 0, Math.Min( 1, h ) );
			s = Math.Max( 0, Math.Min( 1, s ) );
			l = Math.Max( 0, Math.Min( 1, l ) );

			var max = Round( l * 255 );
			var min = Round( ( 1.0 - s ) * l * 255 );
			var q = ( double ) ( max - min ) / 255;
			var a = ( byte ) ( _LeftColorA + value * _DistA );

			if ( h >= 0 && h <= 1.0 / 6.0 )
			{
				var mid = Round( h * q * 1530.0 + min );
				return Color.FromArgb( a, max, mid, min );
			}
			if( h <= 1.0 / 3.0 )
			{
				var mid = Round( -( ( h - 1.0 / 6.0 ) * q ) * 1530.0 + max );
				return Color.FromArgb( a, mid, max, min );
			}
			if( h <= 0.5 )
			{
				var mid = Round( ( h - 1.0 / 3.0 ) * q * 1530.0 + min );
				return Color.FromArgb( a, min, max, mid );
			}
			if( h <= 2.0 / 3.0 )
			{
				var mid = Round( -( ( h - 0.5 ) * q ) * 1530.0 + max );
				return Color.FromArgb( a, min, mid, max );
			}
			if( h <= 5.0 / 6.0 )
			{
				var mid = Round( ( h - 2.0 / 3.0 ) * q * 1530.0 + min );
				return Color.FromArgb( a, mid, min, max );
			}
			if( h <= 1.0 )
			{
				var mid = Round( -( ( h - 5.0 / 6.0 ) * q ) * 1530.0 + max );
				return Color.FromArgb( a, max, min, mid );
			}
			return Color.FromArgb( a, 0, 0, 0 );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private Color GetColorRgb( double value )
		{
			value = ( Math.Max( _LeftValue, Math.Min( _RightValue, value ) ) - _LeftValue ) / _DistLeftRight;

			return Color.FromArgb(
				( byte ) ( _LeftColorA + value * _DistA ),
				( byte ) ( _LeftColorR + value * _DistR ),
				( byte ) ( _LeftColorG + value * _DistG ),
				( byte ) ( _LeftColorB + value * _DistB )
			);
		}

		/// <summary>
		/// Gets the color of hte span at the specified position.
		/// </summary>
		/// <param name="value">The offset.</param>
		/// <param name="interpolation">The interpolation type.</param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException"></exception>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Color GetColor( double value, ColorScaleInterpolation interpolation = ColorScaleInterpolation.HSV )
		{
			if( interpolation == ColorScaleInterpolation.HSV )
				return GetColorHsv( value );
			if( interpolation == ColorScaleInterpolation.RGB )
				return GetColorRgb( value );

			throw new NotSupportedException( string.Concat( "Unsupported interpolation type '", interpolation, "'." ) );
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static byte Round( double val )
		{
			var result = ( byte ) val;
			if( ( int ) ( val * 100 ) % 100 >= 50 )
				result += 1;

			return result;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static Tuple<double, double, double> RgbToHsl( Color c )
		{
			double h, s, l;
			double max, min;

			//	Of our RGB values, assign the highest value to Max, and the Smallest to Min
			if( c.R > c.G )
			{
				max = c.R;
				min = c.G;
			}
			else
			{
				max = c.G;
				min = c.R;
			}
			if( c.B > max ) max = c.B;
			else if( c.B < min ) min = c.B;

			var diff = max - min;

			//	Luminance - a.k.a. Brightness - Adobe photoshop uses the logic that the
			//	site VBspeed regards (regarded) as too primitive = superior decides the 
			//	level of brightness.
			l = max / 255.0;

			//	Saturation
			if( max == 0.0 ) s = 0.0; //	Protecting from the impossible operation of division by zero.
			else s = diff / max; //	The logic of Adobe Photoshops is this simple.

			//	Hue		R is situated at the angel of 360 eller noll degrees; 
			//			G vid 120 degrees
			//			B vid 240 degrees
			double q;
			if( diff == 0.0 ) q = 0.0; // Protecting from the impossible operation of division by zero.
			else q = 60.0 / diff;

			if( max == c.R )
			{
				if( c.G < c.B ) h = ( 360.0 + q * ( c.G - c.B ) ) / 360.0;
				else h = q * ( c.G - c.B ) / 360.0;
			}
			else if( max == c.G ) h = ( 120.0 + q * ( c.B - c.R ) ) / 360.0;
			else if( max == c.B ) h = ( 240.0 + q * ( c.R - c.G ) ) / 360.0;
			else h = 0.0;

			return Tuple.Create( h, s, l );
		}

		#endregion
	}
}