namespace MobileNetworkInfo.Helpers;

public static class SensorMath
{
	static readonly string[] Cardinals = ["N", "NE", "E", "SE", "S", "SW", "W", "NW"];

	/// <summary>Wraps any angle into the range [0, 360).</summary>
	public static double NormalizeDegrees(double degrees)
	{
		var d = degrees % 360;
		return d < 0 ? d + 360 : d;
	}

	/// <summary>8-point compass direction for a heading in degrees.</summary>
	public static string Cardinal(double heading)
	{
		var index = (int)Math.Round(NormalizeDegrees(heading) / 45) % 8;
		return Cardinals[index];
	}

	/// <summary>
	/// Converts a unit quaternion to Tait–Bryan angles in degrees.
	/// Yaw is around Z, pitch around X, roll around Y (Android device axes).
	/// </summary>
	public static (double Pitch, double Roll, double Yaw) QuaternionToEuler(double x, double y, double z, double w)
	{
		// Pitch (rotation about X)
		var sinPitch = 2 * (w * x + y * z);
		var cosPitch = 1 - 2 * (x * x + y * y);
		var pitch = Math.Atan2(sinPitch, cosPitch);

		// Roll (rotation about Y), clamped to avoid NaN at the poles
		var sinRoll = Math.Clamp(2 * (w * y - z * x), -1, 1);
		var roll = Math.Asin(sinRoll);

		// Yaw (rotation about Z)
		var sinYaw = 2 * (w * z + x * y);
		var cosYaw = 1 - 2 * (y * y + z * z);
		var yaw = Math.Atan2(sinYaw, cosYaw);

		const double toDegrees = 180 / Math.PI;
		return (pitch * toDegrees, roll * toDegrees, yaw * toDegrees);
	}

	/// <summary>Estimated altitude in metres from air pressure, using the international barometric formula.</summary>
	public static double AltitudeFromPressure(double pressureHpa, double seaLevelHpa = 1013.25)
	{
		if (pressureHpa <= 0 || seaLevelHpa <= 0)
			return double.NaN;
		return 44330 * (1 - Math.Pow(pressureHpa / seaLevelHpa, 1 / 5.255));
	}

	/// <summary>Magnitude of a 3D vector, e.g. total acceleration.</summary>
	public static double Magnitude(double x, double y, double z) => Math.Sqrt(x * x + y * y + z * z);
}
