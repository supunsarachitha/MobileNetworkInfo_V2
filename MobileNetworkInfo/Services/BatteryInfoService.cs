using Android.Content;
using Android.OS;
using MobileNetworkInfo.Helpers;

namespace MobileNetworkInfo.Services;

public sealed record BatterySnapshot(
	int Level,
	string Status,
	bool IsCharging,
	string PowerSource,
	string Health,
	double? TemperatureC,
	int? VoltageMv,
	string? Technology,
	int? CurrentMa,
	int? RemainingMah,
	int? EstimatedCapacityMah,
	int? CycleCount,
	TimeSpan? TimeToFull,
	bool PowerSaver,
	bool IsPresent)
{
	public double LevelFraction => Math.Clamp(Level / 100d, 0, 1);

	public RowList ToRows()
	{
		var rows = new RowList
		{
			{ "Level", $"{Level}%" },
			{ "Status", Status },
			{ "Power source", PowerSource },
			{ "Health", Health },
			{ "Temperature", TemperatureC is { } t ? Formatters.Temperature(t) : null },
			{ "Voltage", VoltageMv is { } v and > 0 ? Formatters.Voltage(v) : null },
			{ "Technology", Technology },
			{ "Current", CurrentMa is { } c ? $"{Math.Abs(c)} mA ({(IsCharging ? "charging" : "discharging")})" : null },
			{ "Remaining charge", RemainingMah is { } r ? $"{r} mAh" : null },
			{ "Est. full capacity", EstimatedCapacityMah is { } cap ? $"≈ {cap} mAh" : null },
			{ "Charge cycles", CycleCount?.ToString() },
			{ "Time to full", TimeToFull is { } ttf ? Formatters.Duration(ttf) : null },
			{ "Battery Saver", PowerSaver ? "On" : "Off" },
		};
		return rows;
	}
}

/// <summary>Reads the battery state from the sticky ACTION_BATTERY_CHANGED broadcast and BatteryManager.</summary>
public sealed class BatteryInfoService
{
	const string ExtraCycleCount = "android.os.extra.CYCLE_COUNT";

	readonly Context context = Android.App.Application.Context;

	public BatterySnapshot Get()
	{
		// Passing a null receiver returns the current sticky intent without registering anything.
		using var filter = new IntentFilter(Intent.ActionBatteryChanged);
		var intent = context.RegisterReceiver(null, filter);
		var manager = context.GetSystemService(Context.BatteryService) as BatteryManager;

		var level = intent?.GetIntExtra(BatteryManager.ExtraLevel, -1) ?? -1;
		var scale = intent?.GetIntExtra(BatteryManager.ExtraScale, 100) ?? 100;
		var percent = level >= 0 && scale > 0 ? (int)Math.Round(level * 100d / scale) : 0;

		var status = intent?.GetIntExtra(BatteryManager.ExtraStatus, 1) ?? 1;
		var plugged = intent?.GetIntExtra(BatteryManager.ExtraPlugged, 0) ?? 0;
		var health = intent?.GetIntExtra(BatteryManager.ExtraHealth, 1) ?? 1;
		var temperature = intent?.GetIntExtra(BatteryManager.ExtraTemperature, int.MinValue) ?? int.MinValue;
		var voltage = intent?.GetIntExtra(BatteryManager.ExtraVoltage, -1) ?? -1;
		var present = intent?.GetBooleanExtra(BatteryManager.ExtraPresent, true) ?? true;
		var cycles = OperatingSystem.IsAndroidVersionAtLeast(34) ? intent?.GetIntExtra(ExtraCycleCount, -1) ?? -1 : -1;
		var isCharging = status == 2 || (status == 5 && plugged != 0);

		int? current = null;
		int? remaining = null;
		int? capacity = null;
		TimeSpan? timeToFull = null;
		if (manager is not null)
		{
			current = NormalizeMicro(manager.GetIntProperty((int)BatteryProperty.CurrentNow));
			remaining = NormalizeMicro(manager.GetIntProperty((int)BatteryProperty.ChargeCounter));
			if (remaining < 100) // implausible for a phone battery (emulators report placeholder values)
				remaining = null;
			if (remaining is { } rem && percent >= 10)
				capacity = (int)Math.Round(rem * 100d / percent / 10) * 10;

			if (OperatingSystem.IsAndroidVersionAtLeast(28) && isCharging && status != 5)
			{
				var ms = manager.ComputeChargeTimeRemaining();
				if (ms > 0)
					timeToFull = TimeSpan.FromMilliseconds(ms);
			}
		}

		var power = context.GetSystemService(Context.PowerService) as PowerManager;

		return new BatterySnapshot(
			percent,
			present ? AndroidConstants.BatteryStatus(status) : "No battery",
			isCharging,
			AndroidConstants.PowerSource(plugged),
			AndroidConstants.BatteryHealth(health),
			temperature == int.MinValue ? null : temperature / 10d,
			voltage > 0 ? voltage : null,
			NullIfEmpty(intent?.GetStringExtra(BatteryManager.ExtraTechnology)),
			current,
			remaining,
			capacity,
			cycles >= 0 ? cycles : null,
			timeToFull,
			power?.IsPowerSaveMode == true,
			present);
	}

	/// <summary>
	/// BatteryManager reports µA / µAh, but some vendors report mA / mAh instead.
	/// Large magnitudes are treated as micro units; Integer.MIN_VALUE / 0 mean "unsupported".
	/// </summary>
	static int? NormalizeMicro(int value)
	{
		if (value == int.MinValue || value == 0)
			return null;
		return Math.Abs(value) >= 10_000 ? value / 1000 : value;
	}

	static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
