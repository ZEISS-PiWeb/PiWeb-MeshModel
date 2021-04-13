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
	using System.Resources;

	#endregion

	/// <summary>
	/// A collection of data types that are interpreted by PiWeb when used as <see cref="MeshValueEntry.DataType"/>.
	/// </summary>
	public static class WellKnownDataTypes
	{
		#region constants

		/// <summary>
		/// Measurement values.
		/// </summary>
		public const string Deviation = "deviation";

		/// <summary>
		/// Minimum values.
		/// </summary>
		public const string Minimum = "minimum";

		/// <summary>
		/// Maximum values.
		/// </summary>
		public const string Maximum = "maximum";

		/// <summary>
		/// Mean values.
		/// </summary>
		public const string Mean = "mean";

		/// <summary>
		/// Value range.
		/// </summary>
		public const string Range = "range";

		/// <summary>
		/// Standard deviation.
		/// </summary>
		public const string StandardDeviation = "standarddeviation";

		/// <summary>
		/// Variance.
		/// </summary>
		public const string Variance = "variance";

		/// <summary>
		/// Machine capability (without distribution analysis).
		/// </summary>
		public const string Cm = "cm";

		/// <summary>
		/// Machine capability index (without distribution analysis).
		/// </summary>
		public const string Cmk = "cmk";

		#endregion

		#region methods

		/// <summary>
		/// Returns a localized description of the datatype with the name <code>name</code>.
		/// </summary>
		public static string GetDescription( string name )
		{
			if( string.IsNullOrEmpty( name ) )
				return null;
			return new ResourceManager( typeof( WellKnownDataTypes ) ).GetString( name.ToLower() ) ?? name;
		}

		/// <summary>
		/// Determines whether the data type with the name <code>name</code> is a statistics dataset.
		/// </summary>
		/// <param name="name">The name.</param>
		public static bool IsStatisticsDataType( string name )
		{
			// Aktuell alles außer "Deviation" als Statistikdatensatz betrachten.
			return !string.Equals( name, Deviation, StringComparison.OrdinalIgnoreCase );
		}

		/// <summary>
		/// Returns an index to sort the dataset with the name <code>name</code>.
		/// </summary>
		public static int GetOrder( string name )
		{
			if( string.IsNullOrEmpty( name ) )
				return -100;

			switch( name.ToLower() )
			{
				case Deviation:
					return 0;
				case Minimum:
					return 1;
				case Maximum:
					return 2;
				case Mean:
					return 3;
				case Range:
					return 4;
				case Variance:
					return 5;
				case StandardDeviation:
					return 6;
				case Cm:
					return 7;
				case Cmk:
					return 8;
			}

			return 100;
		}

		#endregion
	}
}