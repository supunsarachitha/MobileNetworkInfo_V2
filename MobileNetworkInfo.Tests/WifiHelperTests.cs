using MobileNetworkInfo.Helpers;

namespace MobileNetworkInfo.Tests;

public class WifiHelperTests
{
	[Theory]
	[InlineData(2412, 1)]
	[InlineData(2437, 6)]
	[InlineData(2447, 8)]
	[InlineData(2472, 13)]
	[InlineData(2484, 14)]
	[InlineData(5180, 36)]
	[InlineData(5500, 100)]
	[InlineData(5825, 165)]
	[InlineData(5935, 2)]
	[InlineData(5955, 1)]
	[InlineData(6115, 33)]
	[InlineData(7115, 233)]
	[InlineData(60480, 2)]
	[InlineData(1000, -1)]
	public void ChannelFromFrequency_CoversAllBands(int mhz, int expected) =>
		Assert.Equal(expected, WifiHelper.ChannelFromFrequency(mhz));

	[Theory]
	[InlineData(2437, "2.4 GHz")]
	[InlineData(5180, "5 GHz")]
	[InlineData(5955, "6 GHz")]
	[InlineData(60480, "60 GHz")]
	[InlineData(900, "Unknown band")]
	public void BandFromFrequency(int mhz, string expected) =>
		Assert.Equal(expected, WifiHelper.BandFromFrequency(mhz));

	[Theory]
	[InlineData(4, 2437, "Wi-Fi 4 (802.11n)")]
	[InlineData(5, 5180, "Wi-Fi 5 (802.11ac)")]
	[InlineData(6, 5180, "Wi-Fi 6 (802.11ax)")]
	[InlineData(6, 5955, "Wi-Fi 6E (802.11ax)")]
	[InlineData(8, 5955, "Wi-Fi 7 (802.11be)")]
	[InlineData(0, 2437, "Unknown")]
	public void StandardName_DistinguishesWifi6E(int standard, int frequency, string expected) =>
		Assert.Equal(expected, WifiHelper.StandardName(standard, frequency));

	[Theory]
	[InlineData("[WPA2-PSK-CCMP][RSN-PSK-CCMP][ESS]", "WPA2")]
	[InlineData("[WPA-PSK-CCMP+TKIP][WPA2-PSK-CCMP+TKIP][ESS]", "WPA/WPA2")]
	[InlineData("[RSN-SAE-CCMP][ESS]", "WPA3")]
	[InlineData("[RSN-PSK+SAE-CCMP][ESS]", "WPA2/WPA3")]
	[InlineData("[WPA2-EAP-CCMP][RSN-EAP-CCMP][ESS]", "Enterprise (802.1X)")]
	[InlineData("[RSN-EAP_SUITE_B_192-GCMP-256][ESS]", "WPA3-Enterprise")]
	[InlineData("[RSN-OWE-CCMP][ESS]", "Enhanced Open (OWE)")]
	[InlineData("[WEP][ESS]", "WEP")]
	[InlineData("[ESS]", "Open")]
	[InlineData(null, "Open")]
	public void SecurityFromCapabilities(string? capabilities, string expected) =>
		Assert.Equal(expected, WifiHelper.SecurityFromCapabilities(capabilities));

	[Theory]
	[InlineData("\"Home WiFi\"", "Home WiFi")]
	[InlineData("Office", "Office")]
	[InlineData("<unknown ssid>", null)]
	[InlineData("\"\"", null)]
	[InlineData("", null)]
	[InlineData(null, null)]
	public void CleanSsid_StripsQuotesAndPlaceholders(string? ssid, string? expected) =>
		Assert.Equal(expected, WifiHelper.CleanSsid(ssid));

	[Theory]
	[InlineData("00:13:10:85:fe:01", "00:13:10:85:FE:01")]
	[InlineData("02:00:00:00:00:00", null)]
	[InlineData(null, null)]
	public void CleanBssid_HidesPlaceholder(string? bssid, string? expected) =>
		Assert.Equal(expected, WifiHelper.CleanBssid(bssid));

	[Theory]
	[InlineData(0, "20 MHz")]
	[InlineData(2, "80 MHz")]
	[InlineData(5, "320 MHz")]
	[InlineData(9, "Unknown")]
	public void ChannelWidthName(int width, string expected) =>
		Assert.Equal(expected, WifiHelper.ChannelWidthName(width));
}
