namespace MobileNetworkInfo.Helpers;

public static class WifiHelper
{
	/// <summary>Returns the IEEE channel number for a centre frequency in MHz, or -1 when unknown.</summary>
	public static int ChannelFromFrequency(int mhz) => mhz switch
	{
		2484 => 14,
		>= 2412 and < 2484 => (mhz - 2407) / 5,
		>= 4915 and <= 4980 => (mhz - 4000) / 5, // Japan 4.9 GHz
		>= 5035 and <= 5895 => (mhz - 5000) / 5,
		5935 => 2,
		>= 5955 and <= 7115 => (mhz - 5950) / 5,
		>= 58320 and <= 70200 => (mhz - 56160) / 2160,
		_ => -1,
	};

	public static string BandFromFrequency(int mhz) => mhz switch
	{
		>= 2400 and < 2500 => "2.4 GHz",
		>= 4900 and < 5925 => "5 GHz",
		>= 5925 and <= 7125 => "6 GHz",
		>= 57000 and <= 71000 => "60 GHz",
		_ => "Unknown band",
	};

	/// <summary>Maps Android's ScanResult.WIFI_STANDARD_* constants to a marketing name.</summary>
	public static string StandardName(int standard, int frequencyMhz = 0) => standard switch
	{
		1 => "Legacy (802.11a/b/g)",
		4 => "Wi-Fi 4 (802.11n)",
		5 => "Wi-Fi 5 (802.11ac)",
		6 => frequencyMhz >= 5925 && frequencyMhz <= 7125 ? "Wi-Fi 6E (802.11ax)" : "Wi-Fi 6 (802.11ax)",
		7 => "WiGig (802.11ad)",
		8 => "Wi-Fi 7 (802.11be)",
		_ => "Unknown",
	};

	/// <summary>Maps Android's ScanResult.CHANNEL_WIDTH_* constants to MHz text.</summary>
	public static string ChannelWidthName(int width) => width switch
	{
		0 => "20 MHz",
		1 => "40 MHz",
		2 => "80 MHz",
		3 => "160 MHz",
		4 => "80+80 MHz",
		5 => "320 MHz",
		_ => "Unknown",
	};

	/// <summary>Derives a readable security type from a ScanResult capabilities string such as "[WPA2-PSK-CCMP][RSN-PSK-CCMP][ESS]".</summary>
	public static string SecurityFromCapabilities(string? capabilities)
	{
		var caps = capabilities ?? string.Empty;
		bool Has(string token) => caps.Contains(token, StringComparison.OrdinalIgnoreCase);

		if (Has("EAP"))
			return Has("SUITE_B_192") || Has("SUITE-B-192") ? "WPA3-Enterprise" : "Enterprise (802.1X)";
		var sae = Has("SAE");
		var psk = Has("PSK");
		if (sae && psk)
			return "WPA2/WPA3";
		if (sae)
			return "WPA3";
		if (Has("OWE"))
			return "Enhanced Open (OWE)";
		var rsn = Has("RSN") || Has("WPA2");
		var wpa = Has("[WPA-");
		if (psk && rsn && wpa)
			return "WPA/WPA2";
		if (psk && rsn)
			return "WPA2";
		if (psk || wpa)
			return "WPA";
		if (Has("WEP"))
			return "WEP";
		return "Open";
	}

	/// <summary>True for networks that transmit data unencrypted.</summary>
	public static bool IsOpen(string security) => security is "Open";

	/// <summary>Removes the surrounding quotes Android puts on SSIDs and hides placeholder values.</summary>
	public static string? CleanSsid(string? ssid)
	{
		if (string.IsNullOrWhiteSpace(ssid) || ssid == "<unknown ssid>" || ssid == "0x")
			return null;
		if (ssid.Length >= 2 && ssid[0] == '"' && ssid[^1] == '"')
			ssid = ssid[1..^1];
		return ssid.Length == 0 ? null : ssid;
	}

	/// <summary>Android reports this placeholder BSSID when location permission is missing.</summary>
	public static string? CleanBssid(string? bssid) =>
		string.IsNullOrWhiteSpace(bssid) || bssid == "02:00:00:00:00:00" ? null : bssid.ToUpperInvariant();
}
