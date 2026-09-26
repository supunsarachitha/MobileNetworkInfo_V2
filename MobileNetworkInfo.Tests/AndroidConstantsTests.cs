using MobileNetworkInfo.Helpers;

namespace MobileNetworkInfo.Tests;

public class AndroidConstantsTests : CultureTest
{
	[Theory]
	[InlineData(2, "Charging")]
	[InlineData(3, "Discharging")]
	[InlineData(4, "Not charging")]
	[InlineData(5, "Full")]
	[InlineData(1, "Unknown")]
	public void BatteryStatus(int status, string expected) =>
		Assert.Equal(expected, AndroidConstants.BatteryStatus(status));

	[Theory]
	[InlineData(0, "Battery")]
	[InlineData(1, "AC charger")]
	[InlineData(2, "USB")]
	[InlineData(4, "Wireless")]
	[InlineData(8, "Dock")]
	public void PowerSource_ReadsBitFlags(int plugged, string expected) =>
		Assert.Equal(expected, AndroidConstants.PowerSource(plugged));

	[Theory]
	[InlineData(2, "Good")]
	[InlineData(3, "Overheating")]
	[InlineData(7, "Cold")]
	[InlineData(99, "Unknown")]
	public void BatteryHealth(int health, string expected) =>
		Assert.Equal(expected, AndroidConstants.BatteryHealth(health));

	[Theory]
	[InlineData(13, "4G · LTE")]
	[InlineData(20, "5G · NR")]
	[InlineData(15, "3G · HSPA+")]
	[InlineData(0, null)]
	public void NetworkType(int type, string? expected) =>
		Assert.Equal(expected, AndroidConstants.NetworkType(type));

	[Theory]
	[InlineData(1, "No SIM")]
	[InlineData(5, "Ready")]
	[InlineData(0, "Unknown")]
	public void SimState(int state, string expected) =>
		Assert.Equal(expected, AndroidConstants.SimState(state));

	[Theory]
	[InlineData(160, "mdpi")]
	[InlineData(320, "xhdpi")]
	[InlineData(420, "xxhdpi")]
	[InlineData(560, "xxxhdpi")]
	public void DensityBucket(int dpi, string expected) =>
		Assert.Equal(expected, AndroidConstants.DensityBucket(dpi));

	[Fact]
	public void CpuClusters_GroupsFastestFirst()
	{
		long[] cores = [1_800_000, 1_800_000, 1_800_000, 1_800_000, 2_400_000, 2_400_000, 2_400_000, 3_000_000];
		Assert.Equal("1× 3.00 GHz + 3× 2.40 GHz + 4× 1.80 GHz", AndroidConstants.CpuClusters(cores));
	}

	[Fact]
	public void CpuClusters_EmptyIsNull() =>
		Assert.Null(AndroidConstants.CpuClusters([]));

	[Theory]
	[InlineData(4, "WPA3-Personal")]
	[InlineData(0, "Open")]
	[InlineData(-1, null)]
	public void WifiSecurityType(int type, string? expected) =>
		Assert.Equal(expected, AndroidConstants.WifiSecurityType(type));

	[Theory]
	[InlineData(36, "Baklava")]
	[InlineData(33, "Tiramisu")]
	[InlineData(99, null)]
	public void Codename(int api, string? expected) =>
		Assert.Equal(expected, AndroidVersions.Codename(api));

	[Theory]
	[InlineData(32, "12L")]
	[InlineData(36, "16")]
	[InlineData(20, null)]
	public void ReleaseName(int api, string? expected) =>
		Assert.Equal(expected, AndroidVersions.ReleaseName(api));

	[Fact]
	public void DisplayName_IncludesCodenameWhenKnown()
	{
		Assert.Equal("Android 16 (Baklava)", AndroidVersions.DisplayName("16", 36));
		Assert.Equal("Android 17", AndroidVersions.DisplayName("17", 37));
	}
}
