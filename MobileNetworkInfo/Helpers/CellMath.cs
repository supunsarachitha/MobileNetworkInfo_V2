namespace MobileNetworkInfo.Helpers;

public static class CellMath
{
	/// <summary>Android reports unavailable cell values as int.MaxValue (CellInfo.UNAVAILABLE).</summary>
	public const int Unavailable = int.MaxValue;

	public static bool IsValid(int value) => value != Unavailable && value != int.MinValue;

	public static bool IsValid(long value) => value != long.MaxValue && value != int.MaxValue && value >= 0;

	/// <summary>An LTE 28-bit cell identity is the 20-bit eNodeB id followed by an 8-bit sector id.</summary>
	public static (int ENodeB, int Sector) SplitLteCellId(int ci) => (ci >> 8, ci & 0xFF);

	/// <summary>Formats a value with a unit, or returns null when Android marked it unavailable.</summary>
	public static string? WithUnit(int value, string unit) => IsValid(value) ? $"{value} {unit}" : null;

	public static string? Value(int value) => IsValid(value) ? value.ToString() : null;

	public static string? Value(long value) => IsValid(value) ? value.ToString() : null;
}
