using MobileNetworkInfo.Helpers;

namespace MobileNetworkInfo.Tests;

public class SignalQualityTests : CultureTest
{
	[Theory]
	[InlineData(-40, SignalLevel.Excellent)]
	[InlineData(-55, SignalLevel.Excellent)]
	[InlineData(-56, SignalLevel.Good)]
	[InlineData(-67, SignalLevel.Good)]
	[InlineData(-70, SignalLevel.Fair)]
	[InlineData(-80, SignalLevel.Poor)]
	[InlineData(-127, SignalLevel.None)]
	[InlineData(0, SignalLevel.None)]
	public void FromWifiRssi_Classifies(int rssi, SignalLevel expected) =>
		Assert.Equal(expected, SignalQuality.FromWifiRssi(rssi));

	[Theory]
	[InlineData(-1, SignalLevel.None)]
	[InlineData(0, SignalLevel.None)]
	[InlineData(1, SignalLevel.Poor)]
	[InlineData(2, SignalLevel.Fair)]
	[InlineData(3, SignalLevel.Good)]
	[InlineData(4, SignalLevel.Excellent)]
	public void FromAndroidLevel_MapsZeroToFour(int level, SignalLevel expected) =>
		Assert.Equal(expected, SignalQuality.FromAndroidLevel(level));

	[Fact]
	public void Summary_CombinesDbmAndDescription() =>
		Assert.Equal("-58 dBm · Good", SignalQuality.Summary(-58, SignalLevel.Good));

	[Fact]
	public void Describe_NoneReadsAsNoSignal() =>
		Assert.Equal("No signal", SignalQuality.Describe(SignalLevel.None));
}
