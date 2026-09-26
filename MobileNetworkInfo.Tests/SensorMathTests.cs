using MobileNetworkInfo.Helpers;

namespace MobileNetworkInfo.Tests;

public class SensorMathTests
{
	[Theory]
	[InlineData(0, "N")]
	[InlineData(22, "N")]
	[InlineData(23, "NE")]
	[InlineData(90, "E")]
	[InlineData(180, "S")]
	[InlineData(270, "W")]
	[InlineData(315, "NW")]
	[InlineData(359, "N")]
	[InlineData(-45, "NW")]
	[InlineData(720, "N")]
	public void Cardinal_EightPoints(double heading, string expected) =>
		Assert.Equal(expected, SensorMath.Cardinal(heading));

	[Theory]
	[InlineData(-90, 270)]
	[InlineData(360, 0)]
	[InlineData(725, 5)]
	public void NormalizeDegrees(double input, double expected) =>
		Assert.Equal(expected, SensorMath.NormalizeDegrees(input), 6);

	[Fact]
	public void QuaternionToEuler_IdentityIsZero()
	{
		var (pitch, roll, yaw) = SensorMath.QuaternionToEuler(0, 0, 0, 1);
		Assert.Equal(0, pitch, 6);
		Assert.Equal(0, roll, 6);
		Assert.Equal(0, yaw, 6);
	}

	[Theory]
	[InlineData(1, 0, 0, 90, 0, 0)] // about X → pitch
	[InlineData(0, 1, 0, 0, 90, 0)] // about Y → roll
	[InlineData(0, 0, 1, 0, 0, 90)] // about Z → yaw
	public void QuaternionToEuler_SingleAxisRotations(double ax, double ay, double az, double pitch, double roll, double yaw)
	{
		var half = Math.PI / 4; // 90° rotation → half-angle 45°
		var s = Math.Sin(half);
		var result = SensorMath.QuaternionToEuler(ax * s, ay * s, az * s, Math.Cos(half));
		Assert.Equal(pitch, result.Pitch, 3);
		Assert.Equal(roll, result.Roll, 3);
		Assert.Equal(yaw, result.Yaw, 3);
	}

	[Fact]
	public void AltitudeFromPressure_SeaLevelIsZero() =>
		Assert.Equal(0, SensorMath.AltitudeFromPressure(1013.25), 3);

	[Fact]
	public void AltitudeFromPressure_About1000mAt899hPa() =>
		Assert.InRange(SensorMath.AltitudeFromPressure(898.75), 990, 1010);

	[Fact]
	public void AltitudeFromPressure_InvalidIsNaN() =>
		Assert.True(double.IsNaN(SensorMath.AltitudeFromPressure(0)));

	[Fact]
	public void Magnitude_OfGravityVector() =>
		Assert.Equal(1, SensorMath.Magnitude(0, 0.6, 0.8), 6);
}
