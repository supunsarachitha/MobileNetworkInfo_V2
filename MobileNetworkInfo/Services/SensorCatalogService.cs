using Android.Content;
using Android.Hardware;
using MobileNetworkInfo.Helpers;

namespace MobileNetworkInfo.Services;

/// <summary>Lists every hardware sensor and streams sensors that MAUI Essentials doesn't cover (light, proximity).</summary>
public sealed class SensorCatalogService
{
	readonly Context context = Android.App.Application.Context;

	SensorManager? Manager => context.GetSystemService(Context.SensorService) as SensorManager;

	public RowList GetSensorRows()
	{
		var rows = new RowList();
		var sensors = Manager?.GetSensorList(SensorType.All) ?? [];
		foreach (var sensor in sensors.OrderBy(s => s.StringType).ThenBy(s => s.Name))
		{
			var type = Formatters.HumanizeConstant(sensor.StringType);
			var vendor = string.IsNullOrWhiteSpace(sensor.Vendor) ? null : sensor.Vendor;
			rows.Add(sensor.Name ?? type, vendor is null ? type : $"{type}\n{vendor}");
		}
		return rows;
	}

	public bool HasSensor(SensorType type) => Manager?.GetDefaultSensor(type) is not null;

	/// <summary>Starts listening to a sensor; dispose the result to stop. Returns null if the sensor is missing.</summary>
	public IDisposable? Listen(SensorType type, Action<float[]> onReading)
	{
		if (Manager is not { } manager || manager.GetDefaultSensor(type) is not { } sensor)
			return null;

		var listener = new Listener(onReading);
		manager.RegisterListener(listener, sensor, SensorDelay.Ui);
		return new Subscription(manager, listener);
	}

	sealed class Listener(Action<float[]> onReading) : Java.Lang.Object, ISensorEventListener
	{
		public void OnAccuracyChanged(Sensor? sensor, SensorStatus accuracy)
		{
		}

		public void OnSensorChanged(SensorEvent? e)
		{
			if (e?.Values is { Count: > 0 } values)
				onReading(values.ToArray());
		}
	}

	sealed class Subscription(SensorManager manager, Listener listener) : IDisposable
	{
		public void Dispose()
		{
			manager.UnregisterListener(listener);
			listener.Dispose();
		}
	}
}
