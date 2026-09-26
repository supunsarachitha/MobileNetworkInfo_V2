namespace MobileNetworkInfo.Helpers;

/// <summary>Human readable names for Android integer constants (values taken from the Android SDK).</summary>
public static class AndroidConstants
{
	/// <summary>BatteryManager.BATTERY_STATUS_*</summary>
	public static string BatteryStatus(int status) => status switch
	{
		2 => "Charging",
		3 => "Discharging",
		4 => "Not charging",
		5 => "Full",
		_ => "Unknown",
	};

	/// <summary>BatteryManager.BATTERY_PLUGGED_* (bit flags)</summary>
	public static string PowerSource(int plugged) => plugged switch
	{
		0 => "Battery",
		_ when (plugged & 1) != 0 => "AC charger",
		_ when (plugged & 2) != 0 => "USB",
		_ when (plugged & 4) != 0 => "Wireless",
		_ when (plugged & 8) != 0 => "Dock",
		_ => "Unknown",
	};

	/// <summary>BatteryManager.BATTERY_HEALTH_*</summary>
	public static string BatteryHealth(int health) => health switch
	{
		2 => "Good",
		3 => "Overheating",
		4 => "Dead",
		5 => "Over voltage",
		6 => "Failure",
		7 => "Cold",
		_ => "Unknown",
	};

	/// <summary>TelephonyManager.NETWORK_TYPE_*</summary>
	public static string? NetworkType(int type) => type switch
	{
		1 => "2G · GPRS",
		2 => "2G · EDGE",
		3 => "3G · UMTS",
		4 => "2G · CDMA",
		5 => "3G · EVDO rev. 0",
		6 => "3G · EVDO rev. A",
		7 => "2G · 1xRTT",
		8 => "3G · HSDPA",
		9 => "3G · HSUPA",
		10 => "3G · HSPA",
		11 => "2G · iDEN",
		12 => "3G · EVDO rev. B",
		13 => "4G · LTE",
		14 => "3G · eHRPD",
		15 => "3G · HSPA+",
		16 => "2G · GSM",
		17 => "3G · TD-SCDMA",
		18 => "Wi-Fi calling (IWLAN)",
		19 => "4G · LTE-CA",
		20 => "5G · NR",
		_ => null,
	};

	/// <summary>TelephonyManager.SIM_STATE_*</summary>
	public static string SimState(int state) => state switch
	{
		1 => "No SIM",
		2 => "PIN required",
		3 => "PUK required",
		4 => "Network locked",
		5 => "Ready",
		6 => "Not ready",
		7 => "Permanently disabled",
		8 => "Card error",
		9 => "Restricted",
		10 => "Loaded",
		11 => "Present",
		_ => "Unknown",
	};

	/// <summary>TelephonyManager.PHONE_TYPE_*</summary>
	public static string? PhoneType(int type) => type switch
	{
		1 => "GSM",
		2 => "CDMA",
		3 => "SIP",
		_ => null,
	};

	/// <summary>WifiInfo.SECURITY_TYPE_*</summary>
	public static string? WifiSecurityType(int type) => type switch
	{
		0 => "Open",
		1 => "WEP",
		2 => "WPA2-Personal",
		3 => "Enterprise (802.1X)",
		4 => "WPA3-Personal",
		5 => "WPA3-Enterprise 192-bit",
		6 => "Enhanced Open (OWE)",
		7 => "WAPI-PSK",
		8 => "WAPI-CERT",
		9 => "WPA3-Enterprise",
		10 => "OSEN",
		11 => "Passpoint R1/R2",
		12 => "Passpoint R3",
		13 => "Easy Connect (DPP)",
		_ => null,
	};

	/// <summary>Display.HdrCapabilities.HDR_TYPE_*</summary>
	public static string? HdrType(int type) => type switch
	{
		1 => "Dolby Vision",
		2 => "HDR10",
		3 => "HLG",
		4 => "HDR10+",
		_ => null,
	};

	/// <summary>ConnectivityManager.RESTRICT_BACKGROUND_STATUS_*</summary>
	public static string? DataSaver(int status) => status switch
	{
		1 => "Off",
		2 => "On (app allowed)",
		3 => "On",
		_ => null,
	};

	/// <summary>Android density bucket for a dpi value.</summary>
	public static string DensityBucket(int dpi) => dpi switch
	{
		<= 120 => "ldpi",
		<= 160 => "mdpi",
		<= 240 => "hdpi",
		<= 320 => "xhdpi",
		<= 480 => "xxhdpi",
		_ => "xxxhdpi",
	};

	/// <summary>
	/// Groups per-core max frequencies (kHz) into clusters, fastest first: "1× 3.00 GHz + 3× 2.40 GHz + 4× 1.80 GHz".
	/// </summary>
	public static string? CpuClusters(IEnumerable<long> maxFrequenciesKhz)
	{
		var clusters = maxFrequenciesKhz
			.Where(f => f > 0)
			.GroupBy(f => f)
			.OrderByDescending(g => g.Key)
			.Select(g => $"{g.Count()}× {Formatters.FrequencyKhz(g.Key)}")
			.ToList();
		return clusters.Count == 0 ? null : string.Join(" + ", clusters);
	}
}
