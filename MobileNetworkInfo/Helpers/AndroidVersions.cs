namespace MobileNetworkInfo.Helpers;

public static class AndroidVersions
{
	/// <summary>Internal dessert codename for an API level, or null when unknown.</summary>
	public static string? Codename(int apiLevel) => apiLevel switch
	{
		21 or 22 => "Lollipop",
		23 => "Marshmallow",
		24 or 25 => "Nougat",
		26 or 27 => "Oreo",
		28 => "Pie",
		29 => "Quince Tart",
		30 => "Red Velvet Cake",
		31 or 32 => "Snow Cone",
		33 => "Tiramisu",
		34 => "Upside Down Cake",
		35 => "Vanilla Ice Cream",
		36 => "Baklava",
		_ => null,
	};

	/// <summary>Marketing version for an API level ("12L" for 32), or null when unknown.</summary>
	public static string? ReleaseName(int apiLevel) => apiLevel switch
	{
		21 => "5.0",
		22 => "5.1",
		23 => "6.0",
		24 => "7.0",
		25 => "7.1",
		26 => "8.0",
		27 => "8.1",
		28 => "9",
		29 => "10",
		30 => "11",
		31 => "12",
		32 => "12L",
		33 => "13",
		34 => "14",
		35 => "15",
		36 => "16",
		_ => null,
	};

	/// <summary>"Android 16 (Baklava)"</summary>
	public static string DisplayName(string release, int apiLevel)
	{
		var codename = Codename(apiLevel);
		return codename is null ? $"Android {release}" : $"Android {release} ({codename})";
	}
}
