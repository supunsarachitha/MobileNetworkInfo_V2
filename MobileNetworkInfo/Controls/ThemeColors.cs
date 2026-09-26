namespace MobileNetworkInfo.Controls;

/// <summary>Resolves the light/dark variant of a color token defined in Colors.xaml ("Primary" → PrimaryLight / PrimaryDark).</summary>
public static class ThemeColors
{
	public static bool IsDark => Application.Current?.RequestedTheme == AppTheme.Dark;

	public static Color Get(string token)
	{
		var key = token + (IsDark ? "Dark" : "Light");
		if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color themed)
			return themed;
		if (Application.Current?.Resources.TryGetValue(token, out value) == true && value is Color plain)
			return plain;
		return Colors.Gray;
	}
}
