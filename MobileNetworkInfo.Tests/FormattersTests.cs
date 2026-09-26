using MobileNetworkInfo.Helpers;

namespace MobileNetworkInfo.Tests;

public class FormattersTests : CultureTest
{
	[Theory]
	[InlineData(0, "0 B")]
	[InlineData(999, "999 B")]
	[InlineData(1000, "1.0 KB")]
	[InlineData(1_500_000, "1.5 MB")]
	[InlineData(2_147_483_648, "2.1 GB")]
	[InlineData(128_000_000_000, "128 GB")]
	[InlineData(1_000_000_000_000, "1.0 TB")]
	[InlineData(-1, "—")]
	public void Bytes_UsesSiUnits(long bytes, string expected) =>
		Assert.Equal(expected, Formatters.Bytes(bytes));

	[Fact]
	public void UsedOf_CombinesBothValues() =>
		Assert.Equal("3.2 GB of 8.0 GB", Formatters.UsedOf(3_200_000_000, 8_000_000_000));

	[Theory]
	[InlineData(0, 100, 0)]
	[InlineData(25, 100, 0.25)]
	[InlineData(150, 100, 1)]
	[InlineData(10, 0, 0)]
	public void Fraction_IsClamped(long part, long total, double expected) =>
		Assert.Equal(expected, Formatters.Fraction(part, total), 5);

	[Theory]
	[InlineData(0.0, "0%")]
	[InlineData(0.456, "46%")]
	[InlineData(1.2, "100%")]
	public void Percent_RoundsAndClamps(double fraction, string expected) =>
		Assert.Equal(expected, Formatters.Percent(fraction));

	[Theory]
	[InlineData(42, "42s")]
	[InlineData(125, "2m 5s")]
	[InlineData(3 * 3600 + 7 * 60 + 3, "3h 7m")]
	[InlineData(2 * 86400 + 4 * 3600 + 12 * 60, "2d 4h 12m")]
	[InlineData(-5, "0s")]
	public void Duration_PicksSensibleUnits(int seconds, string expected) =>
		Assert.Equal(expected, Formatters.Duration(TimeSpan.FromSeconds(seconds)));

	[Theory]
	[InlineData(2_841_600, "2.84 GHz")]
	[InlineData(1_000_000, "1.00 GHz")]
	[InlineData(600_000, "600 MHz")]
	[InlineData(0, "—")]
	public void FrequencyKhz_FormatsClockSpeeds(long khz, string expected) =>
		Assert.Equal(expected, Formatters.FrequencyKhz(khz));

	[Fact]
	public void Temperature_ShowsCelsiusAndFahrenheit() =>
		Assert.Equal("25.0 °C / 77.0 °F", Formatters.Temperature(25));

	[Fact]
	public void Voltage_ConvertsMillivolts() =>
		Assert.Equal("4.12 V", Formatters.Voltage(4120));

	[Theory]
	[InlineData(850, "850 kbps")]
	[InlineData(12_500, "12.5 Mbps")]
	[InlineData(300_000, "300 Mbps")]
	[InlineData(1_200_000, "1.2 Gbps")]
	[InlineData(0, "—")]
	public void BandwidthKbps_PicksUnit(long kbps, string expected) =>
		Assert.Equal(expected, Formatters.BandwidthKbps(kbps));

	[Theory]
	[InlineData(4.25, "4.3 ms")]
	[InlineData(23.4, "23 ms")]
	public void Milliseconds_UsesDecimalOnlyForSmallValues(double ms, string expected) =>
		Assert.Equal(expected, Formatters.Milliseconds(ms));

	[Theory]
	[InlineData("2025-09-05", "5 September 2025")]
	[InlineData("not-a-date", "not-a-date")]
	[InlineData(null, "—")]
	public void SecurityPatch_IsHumanReadable(string? patch, string expected) =>
		Assert.Equal(expected, Formatters.SecurityPatch(patch));

	[Theory]
	[InlineData("android.sensor.game_rotation_vector", "Game rotation vector")]
	[InlineData("android.sensor.accelerometer", "Accelerometer")]
	[InlineData("com.vendor.CUSTOM_SENSOR", "Custom sensor")]
	[InlineData("", "—")]
	public void HumanizeConstant_TurnsConstantsIntoWords(string value, string expected) =>
		Assert.Equal(expected, Formatters.HumanizeConstant(value));

	[Fact]
	public void EpochDate_FormatsLocalDate()
	{
		var epoch = new DateTimeOffset(2025, 9, 3, 12, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
		var expected = DateTimeOffset.FromUnixTimeMilliseconds(epoch).LocalDateTime.ToString("d MMM yyyy");
		Assert.Equal(expected, Formatters.EpochDate(epoch));
		Assert.Equal("—", Formatters.EpochDate(0));
	}
}
