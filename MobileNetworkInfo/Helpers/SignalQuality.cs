namespace MobileNetworkInfo.Helpers;

public enum SignalLevel
{
	None = 0,
	Poor = 1,
	Fair = 2,
	Good = 3,
	Excellent = 4,
}

public static class SignalQuality
{
	/// <summary>Classifies a Wi-Fi RSSI in dBm.</summary>
	public static SignalLevel FromWifiRssi(int rssi) => rssi switch
	{
		>= 0 or <= -127 => SignalLevel.None, // invalid / not connected
		>= -55 => SignalLevel.Excellent,
		>= -67 => SignalLevel.Good,
		>= -75 => SignalLevel.Fair,
		_ => SignalLevel.Poor,
	};

	/// <summary>Maps Android's 0–4 signal level (as returned by CellSignalStrength.getLevel) to a <see cref="SignalLevel"/>.</summary>
	public static SignalLevel FromAndroidLevel(int level) => level switch
	{
		<= 0 => SignalLevel.None,
		1 => SignalLevel.Poor,
		2 => SignalLevel.Fair,
		3 => SignalLevel.Good,
		_ => SignalLevel.Excellent,
	};

	public static string Describe(SignalLevel level) => level switch
	{
		SignalLevel.Excellent => "Excellent",
		SignalLevel.Good => "Good",
		SignalLevel.Fair => "Fair",
		SignalLevel.Poor => "Poor",
		_ => "No signal",
	};

	/// <summary>"-58 dBm · Good"</summary>
	public static string Summary(int dbm, SignalLevel level) => $"{Formatters.Dbm(dbm)} · {Describe(level)}";
}
