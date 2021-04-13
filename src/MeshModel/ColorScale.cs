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
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using System.Xml;

	#endregion

	/// <summary>
	/// Describes value range that is represented by discrete or continuous colors.
	/// </summary>
	public sealed class ColorScale
	{
		#region members

		private readonly float[] _OrderedValueList;
		private readonly ColorSpan[] _ColorSpans;
		private readonly bool _HasConstantColors;

		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ColorScale" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="invalidColor">Color of the invalid.</param>
		/// <param name="entries">The entries.</param>
		/// <param name="interpolation">The interpolation mode.</param>
		/// <exception cref="ArgumentException">Error creating color scale: Duplicate color entries (values) are not allowed.</exception>
		/// <exception cref="System.ArgumentException">Error creating color scale: Duplicate color entries (values) are not allowed.</exception>
		public ColorScale( string name, Color invalidColor, IEnumerable<ColorScaleEntry> entries = null, ColorScaleInterpolation interpolation = ColorScaleInterpolation.HSV )
		{
			Name = name;
			InvalidColor = invalidColor;
			Interpolation = interpolation;

			Precision = 1;

			Entries = entries?.OrderBy( entry => entry.Value ).ToArray() ?? Array.Empty<ColorScaleEntry>();
			_OrderedValueList = Entries.Select( entry => entry.Value ).ToArray();

			if( _OrderedValueList.Distinct().Count() != _OrderedValueList.Length )
			{
				throw new ArgumentException( "Error creating color scale: Duplicate color entries (values) are not allowed." ); //Prevents division by zero in color interpolation.
			}

			_ColorSpans = new ColorSpan[ _OrderedValueList.Length - 1 ];
			_HasConstantColors = true;

			for( var i = 0; i < _OrderedValueList.Length - 1; i++ )
			{
				_ColorSpans[ i ] = new ColorSpan( _OrderedValueList[ i ], _OrderedValueList[ i + 1 ], Entries[ i ].RightColor, Entries[ i + 1 ].LeftColor );
				if( i > 0 && Entries[ i - 1 ].RightColor != Entries[ i ].LeftColor )
					_HasConstantColors = false;

				Precision = Math.Max( Precision, CalculatePrecision( _OrderedValueList[ i ] ) );
			}

			Precision = Math.Max( Precision, CalculatePrecision( _OrderedValueList[ _OrderedValueList.Length - 1 ] ) );

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
		/// Gets the precision of values
		/// </summary>
		public double Precision { get; }

		/// <summary>
		/// Gets the color scale entries as array copy.
		/// </summary>
		public ColorScaleEntry[] Entries { get; }

		/// <summary>
		/// Gets a value indicating whether the scale is continuous. (Has gradients between discrete values)
		/// </summary>
		public bool IsContinuous => !_ColorSpans.All( s => s.IsSolidColor );

		/// <summary>
		/// Gets the interpolation mode.
		/// </summary>
		public ColorScaleInterpolation Interpolation { get; }

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
			if( ( (double)value ) <= MinValue )
				return Entries[ 0 ].LeftColor;

			// Overflow (value >= outer most right value)
			if( ( (double)value ) >= MaxValue )
				return Entries[ Entries.Length - 1 ].RightColor;

			var index = GetIndex( value );

			// Constant color span
			if( _HasConstantColors && Entries[ index - 1 ].RightColor == Entries[ index ].LeftColor )
				return Entries[ index ].LeftColor;

			// Linear color interpolation
			return _ColorSpans[ index - 1 ].GetColor( value, Interpolation );
		}

		internal static ColorScale Read( XmlReader reader )
		{
			var name = reader.GetAttribute( "Name" );
			var invalidColor = reader.ReadColorAttribute( "InvalidColor" );
			var interpolation = reader.GetAttribute( "Interpolation" );

			var entries = new List<ColorScaleEntry>();

			var colorScaleReader = reader.ReadSubtree();

			while( colorScaleReader.ReadToFollowing( "Entry" ) && reader.NodeType == XmlNodeType.Element )
			{
				entries.Add( ColorScaleEntry.Read( colorScaleReader ) );
			}


			return new ColorScale( name, invalidColor, entries.ToArray(), interpolation != null ? (ColorScaleInterpolation)Enum.Parse( typeof( ColorScaleInterpolation ), interpolation ) : ColorScaleInterpolation.HSV );
		}

		internal void Write( XmlWriter writer )
		{
			writer.WriteAttributeString( "Name", Name );
			writer.WriteAttributeString( "Interpolation", Interpolation.ToString() );
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

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private double CalculatePrecision( double value )
		{
			value = Math.Abs( value );
			var mag = Math.Floor( Math.Log10( value ) );
			double precision = !IsFinite( mag ) ? 0 : -(int)mag;
			precision = Math.Max( 1, precision );
			return precision;
		}
		
		/// <summary>
		/// Checks whether a double value is a finite number (is not NaN or Infinity).
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns><see langword="true"/>, if the value is a finite number; otherwise <see langword="false"/>.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private static bool IsFinite( double value )
		{
			// TODO: This method is duplicated from double implementation that is available in .Net Standard 2.1 and .Net Core
			var bits = BitConverter.DoubleToInt64Bits( value );
			return ( bits & 0x7FFFFFFFFFFFFFFF ) < 0x7FF0000000000000;
		}
		
		#endregion
	}
}