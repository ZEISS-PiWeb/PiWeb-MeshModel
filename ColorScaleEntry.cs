#region Copyright
/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss IMT (IZfM Dresden)                   */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2010                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */
#endregion

namespace Zeiss.IMT.PiWeb.Meshmodels
{
	#region using

	using System;
	using System.Globalization;
	using System.Xml;

	#endregion

	public class ColorScaleEntry
	{
		#region constructor

		/// <summary>
		/// Creates a continous <see cref="ColorScaleEntry"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="color">The color.</param>
		public ColorScaleEntry(float value, Color color)
		{
			Value = value;

			LeftColor = color;
			RightColor = color;
		}

		/// <summary>
		/// Creates a discrete <see cref="ColorScaleEntry"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="leftColor">Color of the left.</param>
		/// <param name="rightColor">Color of the right.</param>
		public ColorScaleEntry(float value, Color leftColor, Color rightColor)
		{
			Value = value;

			LeftColor = leftColor;
			RightColor = rightColor;
		}

		#endregion

		#region properties

		/// <summary>
		/// Gets the value.
		/// </summary>
		public float Value { get; }

		/// <summary>
		/// Gets the color to the left.
		/// </summary>
		public Color LeftColor { get; }

		/// <summary>
		/// Gets the color to the right.
		/// </summary>
		public Color RightColor { get; }

		#endregion

		#region methods

		internal static ColorScaleEntry Read(XmlReader reader)
		{
			var valueString = reader.GetAttribute("Value");
			if (string.IsNullOrEmpty(valueString))
				throw new FormatException("Empty values are not allowed");

			var value = float.Parse(valueString, CultureInfo.InvariantCulture);

			var leftColor = reader.ReadColorAttribute("LeftColor");
			Color rightColor;

			try
			{
				rightColor = reader.ReadColorAttribute("RightColor");
			}
			catch (Exception)
			{
				rightColor = reader.ReadColorAttribute("RigthColor");
			}

			return new ColorScaleEntry(value, leftColor, rightColor);
		}



		internal void Write(XmlWriter writer)
		{
			writer.WriteAttributeString("Value", Value.ToString(CultureInfo.InvariantCulture));

			writer.WriteColorAttribute("LeftColor", LeftColor);
			writer.WriteColorAttribute("RightColor", RightColor);
		}

		public override string ToString()
		{
			return $"{Value} [{LeftColor}, {RightColor}] ";
		}

		#endregion
	}
}