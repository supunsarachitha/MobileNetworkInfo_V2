using System.Globalization;

namespace MobileNetworkInfo.Helpers;

public static class Formatters
{
	static readonly string[] SizeUnits = ["B", "KB", "MB", "GB", "TB", "PB"];

	static CultureInfo Culture => CultureInfo.CurrentCulture;

	/// <summary>Formats a byte count using SI units (1 KB = 1000 B), matching Android's own Settings app.</summary>
	public static string Bytes(long bytes)
	{
		if (bytes < 0)
			return "—";

		double value = bytes;
		var unit = 0;
		while (value >= 1000 && unit < SizeUnits.Length - 1)
		{
			value /= 1000;
			unit++;
		}

		if (unit == 0)
			return string.Create(Culture, $"{bytes} B");

		var format = value >= 100 ? "0" : "0.0";
		return value.ToString(format, Culture) + " " + SizeUnits[unit];
	}

	/// <summary>"3.2 GB of 8.0 GB"</summary>
	public static string UsedOf(long used, long total) => $"{Bytes(used)} of {Bytes(total)}";

	/// <summary>Fraction (0–1) of <paramref name="part"/> in <paramref name="total"/>, clamped.</summary>
	public static double Fraction(long part, long total) =>
		total <= 0 ? 0 : Math.Clamp((double)part / total, 0, 1);

	public static string Percent(double fraction) =>
		(Math.Clamp(fraction, 0, 1) * 100).ToString("0", Culture) + "%";

	/// <summary>"2d 4h 12m", "4h 12m", "12m 5s", "42s"</summary>
	public static string Duration(TimeSpan span)
	{
		if (span < TimeSpan.Zero)
			span = TimeSpan.Zero;

		if (span.TotalDays >= 1)
			return $"{(int)span.TotalDays}d {span.Hours}h {span.Minutes}m";
		if (span.TotalHours >= 1)
			return $"{span.Hours}h {span.Minutes}m";
		if (span.TotalMinutes >= 1)
			return $"{span.Minutes}m {span.Seconds}s";
		return $"{span.Seconds}s";
	}

	/// <summary>Formats a clock frequency given in kHz (as exposed by cpufreq): "2.84 GHz", "600 MHz".</summary>
	public static string FrequencyKhz(long khz)
	{
		if (khz <= 0)
			return "—";
		if (khz >= 1_000_000)
			return (khz / 1_000_000d).ToString("0.00", Culture) + " GHz";
		return (khz / 1000d).ToString("0", Culture) + " MHz";
	}

	public static string Temperature(double celsius)
	{
		var fahrenheit = celsius * 9 / 5 + 32;
		return string.Create(Culture, $"{celsius:0.0} °C / {fahrenheit:0.0} °F");
	}

	public static string Voltage(int millivolts) =>
		(millivolts / 1000d).ToString("0.00", Culture) + " V";

	public static string Dbm(int dbm) => string.Create(Culture, $"{dbm} dBm");

	/// <summary>Formats a link bandwidth given in kbps: "850 kbps", "12.5 Mbps", "1.2 Gbps".</summary>
	public static string BandwidthKbps(long kbps)
	{
		if (kbps <= 0)
			return "—";
		if (kbps >= 1_000_000)
			return (kbps / 1_000_000d).ToString("0.0", Culture) + " Gbps";
		if (kbps >= 1000)
			return (kbps / 1000d).ToString(kbps >= 100_000 ? "0" : "0.0", Culture) + " Mbps";
		return string.Create(Culture, $"{kbps} kbps");
	}

	public static string Milliseconds(double ms) =>
		ms < 10 ? ms.ToString("0.0", Culture) + " ms" : ms.ToString("0", Culture) + " ms";

	/// <summary>Formats a Unix epoch timestamp in milliseconds as a local date.</summary>
	public static string EpochDate(long epochMilliseconds)
	{
		if (epochMilliseconds <= 0)
			return "—";
		return DateTimeOffset.FromUnixTimeMilliseconds(epochMilliseconds).LocalDateTime.ToString("d MMM yyyy", Culture);
	}

	/// <summary>Converts Android's "2024-05-05" security patch string to "5 May 2024".</summary>
	public static string SecurityPatch(string? patch)
	{
		if (string.IsNullOrWhiteSpace(patch))
			return "—";
		return DateTime.TryParseExact(patch, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
			? date.ToString("d MMMM yyyy", Culture)
			: patch;
	}

	/// <summary>Title-cases an Android constant-style name: "android.sensor.game_rotation_vector" → "Game rotation vector".</summary>
	public static string HumanizeConstant(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return "—";

		var name = value[(value.LastIndexOf('.') + 1)..].Replace('_', ' ').Trim();
		if (name.Length == 0)
			return value;
		return char.ToUpper(name[0], Culture) + name[1..].ToLower(Culture);
	}
}
