#region Copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss IMT (IZfM Dresden)                   */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2010                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

namespace Zeiss.IMT.PiWeb.Meshmodels
{
	#region usings

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using System.Xml;

	#endregion

	public class ColorScale
	{
		#region members

		private readonly float[] _OrderedValueList;
		private readonly ColorSpan[] _ColorSpans;
		private readonly bool _HasConstantColors;

		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ColorScale"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="invalidColor">Color of the invalid.</param>
		/// <param name="entries">The entries.</param>
		/// <exception cref="System.ArgumentException">Error creating color scale: Duplicate color entries (values) are not allowed.</exception>
		public ColorScale( string name, Color invalidColor, IEnumerable<ColorScaleEntry> entries = null )
		{
			Name = name;
			InvalidColor = invalidColor;

			Entries = entries?.OrderBy( entry => entry.Value ).ToArray() ?? new ColorScaleEntry[0];
			_OrderedValueList = Entries.Select( entry => entry.Value ).ToArray();

			if( _OrderedValueList.Distinct().Count() != _OrderedValueList.Length )
			{
				throw new ArgumentException( "Error creating color scale: Duplicate color entries (values) are not allowed." ); //Prevents division by zero in color interpolation.
			}

			_ColorSpans = new ColorSpan[_OrderedValueList.Length - 1];
			_HasConstantColors = true;

			for( var i = 0; i < _OrderedValueList.Length - 1; i++ )
			{
				_ColorSpans[ i ] = new ColorSpan( _OrderedValueList[ i ], _OrderedValueList[ i + 1 ], Entries[ i ].RightColor, Entries[ i + 1 ].LeftColor );
				if( i > 0 && Entries[ i - 1 ].RightColor != Entries[ i ].LeftColor )
					_HasConstantColors = false;
			}

			if( _OrderedValueList.Length > 0 )
			{
				MinValue = Entries[ 0 ].Value;
				MaxValue = Entries[ Entries.Length - 1 ].Value;
			}
			else
			{
				MinValue = float.MaxValue;
				MaxValue = float.MinValue;
			}
		}

		#endregion

		#region properties

		/// <summary>
		/// Gets the color scales name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the color for invalid values (NaN, Infinity).
		/// </summary>
		public Color InvalidColor { get; }

		/// <summary>
		/// Gets the minimum value.
		/// </summary>
		public float MinValue { get; }

		/// <summary>
		/// Gets the maximum value.
		/// </summary>
		public float MaxValue { get; }

		/// <summary>
		/// Gets the color scale entries as array copy.
		/// </summary>
		public ColorScaleEntry[] Entries { get; }

		/// <summary>
		/// Gets a value indicating whether the scale is continious. (Has gradients between discrete values)
		/// </summary>
		public bool IsContinious => !_ColorSpans.All( s => s.IsSolidColor );

		#endregion

		#region methods

		/// <summary>
		/// Gets the color scheme color for the specified value. The value must be between the color scales <see cref="MinValue"/> and <see cref="MaxValue"/>
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public Color GetColor( float value )
		{
			// Invalid color
			if( float.IsNaN( value ) )
				return InvalidColor;

			// Underflow (value <= outer most left value)
			if( value <= MinValue )
				return Entries[ 0 ].LeftColor;

			// Overflow (value >= outer most right value)
			if( value >= MaxValue )
				return Entries[ Entries.Length - 1 ].RightColor;

			var index = GetIndex( value );

			// Constant color span
			if( _HasConstantColors && Entries[ index - 1 ].RightColor == Entries[ index ].LeftColor )
				return Entries[ index ].LeftColor;

			// Linear color interpolation
			return _ColorSpans[ index - 1 ].GetColor( value );
		}

		internal static ColorScale Read( XmlReader reader )
		{
			var name = reader.GetAttribute( "Name" );
			var invalidColor = reader.ReadColorAttribute( "InvalidColor" );

			var entries = new List<ColorScaleEntry>();

			var colorScaleReader = reader.ReadSubtree();

			while( colorScaleReader.ReadToFollowing( "Entry" ) && reader.NodeType == XmlNodeType.Element )
			{
				entries.Add( ColorScaleEntry.Read( colorScaleReader ) );
			}

			return new ColorScale( name, invalidColor, entries.ToArray() );
		}

		internal void Write( XmlWriter writer )
		{
			writer.WriteAttributeString( "Name", Name );
			writer.WriteColorAttribute( "InvalidColor", InvalidColor );

			if( Entries != null && Entries.Length > 0 )
			{
				writer.WriteStartElement( "Entries" );

				foreach( var colorScaleEntry in Entries )
				{
					writer.WriteStartElement( "Entry" );
					colorScaleEntry.Write( writer );
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
			}
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private int GetIndex( float value )
		{
			for( var i = 0; i < _OrderedValueList.Length; i++ )
			{
				if( _OrderedValueList[ i ] >= value )
					return i;
			}

			// Fall sollte durch die Checks in GetColor ausgeschlossen sein
			throw new InvalidOperationException( "Error calculating color value. Invalid value: " + value );
		}

		#endregion

		#region class ColorSpan

		private class ColorSpan
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

			#endregion

			#region constructors

			internal ColorSpan( float leftValue, float rightValue, Color leftColor, Color rightColor )
			{
				_LeftValue = leftValue;
				var leftColorHsl = RgbToHsl( leftColor );
				var rightColorHsl = RgbToHsl( rightColor );

				_RightValue = rightValue;

				_DistLeftRight = _RightValue - _LeftValue;

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

			public bool IsSolidColor { get; }

			#endregion

			#region methods

			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			internal Color GetColor( double value )
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

				if( h >= 0 && h <= 1.0 / 6.0 )
				{
					var mid = Round( h * q * 1530.0 + min );
					return Color.FromRgb( max, mid, min );
				}
				if( h <= 1.0 / 3.0 )
				{
					var mid = Round( -( ( h - 1.0 / 6.0 ) * q ) * 1530.0 + max );
					return Color.FromRgb( mid, max, min );
				}
				if( h <= 0.5 )
				{
					var mid = Round( ( h - 1.0 / 3.0 ) * q * 1530.0 + min );
					return Color.FromRgb( min, max, mid );
				}
				if( h <= 2.0 / 3.0 )
				{
					var mid = Round( -( ( h - 0.5 ) * q ) * 1530.0 + max );
					return Color.FromRgb( min, mid, max );
				}
				if( h <= 5.0 / 6.0 )
				{
					var mid = Round( ( h - 2.0 / 3.0 ) * q * 1530.0 + min );
					return Color.FromRgb( mid, min, max );
				}
				if( h <= 1.0 )
				{
					var mid = Round( -( ( h - 5.0 / 6.0 ) * q ) * 1530.0 + max );
					return Color.FromRgb( max, min, mid );
				}
				return Color.FromRgb( 0, 0, 0 );
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

		#endregion
	}
}