#region copyright

/* * * * * * * * * * * * * * * * * * * * * * * * * */
/* Carl Zeiss Industrielle Messtechnik GmbH        */
/* Softwaresystem PiWeb                            */
/* (c) Carl Zeiss 2021                             */
/* * * * * * * * * * * * * * * * * * * * * * * * * */

#endregion

namespace Zeiss.PiWeb.MeshModel.Common
{
	#region usings

	using System;

	#endregion

	/// <summary>
	/// Klasse mit Hilfsmethoden für Vergleiche von Fließkommazahlen.
	/// (siehe https://msdn.microsoft.com/en-us/library/ya2zha7s(v=vs.110).aspx, Technik 1).
	///
	/// Der Vergleich von Nullables erfolgt analog .Net Framework:
	/// https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/nullable-types/using-nullable-types
	///
	/// Der Vergleich von Nullables liefert folgende Ergebnisse:
	/// double? num1 = 10;
	/// double? num2 = null;
	/// double? num3 = null;
	///
	/// num1 == num2 = False
	/// num1 != num2 = True
	/// num1 >  num2 = False
	/// num1 >= num2 = False
	/// num1 &lt;  num2 = False
	/// num1 &lt;= num2 = False
	///
	/// num3 == num2 = True
	/// num3 != num2 = False
	/// num3 >  num2 = False
	/// num3 >= num2 = False
	/// num3 &lt;  num2 = False
	/// num3 &lt;= num2 = False
	/// </summary>
	internal static class DoubleCompareExtensions
	{
		#region constants

		internal const double DefaultPrecision = 1e-15;

		#endregion

		#region methods

		/// <summary>
		/// Prüft, ob die angegebenen Fließkommazahlen unter Berücksichtigung der angegebenen Toleranz
		/// als 'gleich' eingestuft werden können.
		/// </summary>
		/// <param name="x">Die erste Fließkommazahl.</param>
		/// <param name="y">Die zweite Fließkommazahl.</param>
		/// <param name="tolerance">Die Toleranz für den Vergleich.</param>
		/// <returns><code>True</code>, wenn der Abstand zw. den beiden Fließkommazahlen
		/// die angegebene Toleranz nicht übersteigt, ansonsten <code>false</code>.</returns>
		internal static bool IsCloseTo( this float x, float y, double tolerance = DefaultPrecision )
		{
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			return x == y || Math.Abs( x - y ) <= tolerance;
		}

		/// <summary>
		/// Prüft, ob die angegebenen Fließkommazahlen unter Berücksichtigung der angegebenen Toleranz
		/// als 'gleich' eingestuft werden können.
		/// </summary>
		/// <param name="x">Die erste Fließkommazahl.</param>
		/// <param name="y">Die zweite Fließkommazahl.</param>
		/// <param name="tolerance">Die Toleranz für den Vergleich.</param>
		/// <returns><code>True</code>, wenn der Abstand zw. den beiden Fließkommazahlen
		/// die angegebene Toleranz nicht übersteigt, ansonsten <code>false</code>.</returns>
		internal static bool IsCloseTo( this double x, double y, double tolerance = DefaultPrecision )
		{
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			return x == y || Math.Abs( x - y ) <= tolerance;
		}

		/// <summary>
		/// Prüft, ob die angegebenen Fließkommazahlen unter Berücksichtigung der angegebenen Toleranz
		/// als 'gleich' eingestuft werden können.
		/// </summary>
		/// <param name="x">Die erste Fließkommazahl.</param>
		/// <param name="y">Die zweite Fließkommazahl.</param>
		/// <param name="tolerance">Die Toleranz für den Vergleich.</param>
		/// <returns><code>True</code>, wenn beide Fließkommazahlen keinen Wert haben oder
		/// der Abstand zw. den beiden Fließkommazahlen die angegebene Toleranz nicht übersteigt,
		/// ansonsten <code>false</code>.</returns>
		internal static bool IsCloseTo( this double? x, double? y, double tolerance = DefaultPrecision )
		{
			if( x.HasValue != y.HasValue ) return false;

			return !x.HasValue || IsCloseTo( x.Value, y.Value, tolerance );
		}

		#endregion
	}
}