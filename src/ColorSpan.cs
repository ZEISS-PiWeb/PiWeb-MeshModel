#region copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss Industrielle Messtechnik GmbH        */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2021                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

namespace Zeiss.PiWeb.MeshModel
{
	#region usings

	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Runtime.CompilerServices;

	#endregion

	/// <summary>
	/// An interpolated span between two colors
	/// </summary>
	public readonly struct ColorSpan
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

		private readonly Color _LeftColor;
		private readonly Color _RightColor;

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
			_RightValue = rightValue;

			_LeftColor = leftColor;
			_RightColor = rightColor;

			var (leftH, leftS, leftL) = RgbToHsl( leftColor );
			var (rightH, rightS, rightL) = RgbToHsl( rightColor );

			_LeftColorH = leftH;
			_LeftColorS = leftS;
			_LeftColorL = leftL;

			_DistH = rightH - leftH;
			_DistS = rightS - leftS;
			_DistL = rightL - leftL;

			if( _DistH > 0.5 )
				_DistH -= 1;

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
			value = ( Math.Max( _LeftValue, Math.Min( _RightValue, value ) ) - _LeftValue ) / ( _RightValue - _LeftValue );

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
			var q = (double)( max - min ) / 255;
			var a = (byte)( _LeftColor.A + value * ( _RightColor.A - _LeftColor.A ) );

			if( h >= 0 && h <= 1.0 / 6.0 )
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
			value = ( Math.Max( _LeftValue, Math.Min( _RightValue, value ) ) - _LeftValue ) / ( _RightValue - _LeftValue );

			return Color.FromArgb(
				(byte)( _LeftColor.A + value * ( _RightColor.A - _LeftColor.A ) ),
				(byte)( _LeftColor.R + value * ( _RightColor.R - _LeftColor.R ) ),
				(byte)( _LeftColor.G + value * ( _RightColor.G - _LeftColor.G ) ),
				(byte)( _LeftColor.B + value * ( _RightColor.B - _LeftColor.B ) )
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
			return interpolation switch
			{
				ColorScaleInterpolation.HSV => GetColorHsv( value ),
				ColorScaleInterpolation.RGB => GetColorRgb( value ),
				_                           => throw new NotSupportedException( string.Concat( "Unsupported interpolation type '", interpolation, "'." ) )
			};
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static byte Round( double val )
		{
			var result = (byte)val;
			if( (int)( val * 100 ) % 100 >= 50 )
				result += 1;

			return result;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		[SuppressMessage( "ReSharper", "CompareOfFloatsByEqualityOperator" )]
		[SuppressMessage( "ReSharper", "PossibleLossOfFraction" )]
		private static (double h, double s, double l) RgbToHsl( Color c )
		{
			//	Of our RGB values, assign the highest value to Max, and the Smallest to Min
			var min = Math.Min( Math.Min( c.R, c.G ), c.B );
			var max = Math.Max( Math.Max( c.R, c.G ), c.B );

			var diff = (double)max - min;

			//	Luminance - a.k.a. Brightness - Adobe photoshop uses the logic that the
			//	site VBspeed regards (regarded) as too primitive = superior decides the
			//	level of brightness.
			var l = max / 255.0;

			//	Saturation
			var s = max == 0.0 ? 0.0 : diff / max;

			//	Hue		R is situated at the angel of 360 eller noll degrees;
			//			G vid 120 degrees
			//			B vid 240 degrees
			var q = diff == 0.0 ? 0.0 : 60.0 / diff;

			var h = 0.0;
			if( max == c.R )
			{
				if( c.G < c.B ) h = ( 360.0 + q * ( c.G - c.B ) ) / 360.0;
				else h = q * ( c.G - c.B ) / 360.0;
			}
			else if( max == c.G ) h = ( 120.0 + q * ( c.B - c.R ) ) / 360.0;
			else if( max == c.B ) h = ( 240.0 + q * ( c.R - c.G ) ) / 360.0;

			return ( h, s, l );
		}

		#endregion
	}
}