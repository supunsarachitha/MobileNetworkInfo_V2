using System.Globalization;

namespace MobileNetworkInfo.Controls;

/// <summary>True when the value is a non-empty string (or any non-null object).</summary>
public sealed class IsNotEmptyConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		value is string s ? !string.IsNullOrWhiteSpace(s) : value is not null;

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}

/// <summary>Inverts a boolean.</summary>
public sealed class InverseBoolConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is not true;

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value is not true;
}
