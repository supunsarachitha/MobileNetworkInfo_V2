using System.Globalization;
using System.Numerics;
using Android.Hardware;
using CommunityToolkit.Mvvm.ComponentModel;
using MobileNetworkInfo.Helpers;
using MobileNetworkInfo.Models;
using MobileNetworkInfo.Services;

namespace MobileNetworkInfo.ViewModels;

public partial class SensorsViewModel(SensorCatalogService catalog) : BaseViewModel
{
	static readonly CultureInfo Culture = CultureInfo.CurrentCulture;

	readonly List<IDisposable> androidSensors = [];

	// Latest raw readings, written by sensor callbacks and pushed to the UI at a fixed rate
	Vector3? acceleration, gyroscope, magnetic;
	double? heading, pressure, light, proximity;
	(double Pitch, double Roll, double Yaw)? orientation;

	public InfoSection Accelerometer { get; } = [];
	public InfoSection Gyroscope { get; } = [];
	public InfoSection Magnetometer { get; } = [];
	public InfoSection Orientation { get; } = [];
	public InfoSection Environment { get; } = [];
	public InfoSection AllSensors { get; } = [];

	[ObservableProperty] public partial double Heading { get; set; }
	[ObservableProperty] public partial string HeadingText { get; set; } = "—";
	[ObservableProperty] public partial string SensorCount { get; set; } = "";

	public bool HasCompass => Microsoft.Maui.Devices.Sensors.Compass.Default.IsSupported;
	public bool HasAccelerometer => Microsoft.Maui.Devices.Sensors.Accelerometer.Default.IsSupported;
	public bool HasGyroscope => Microsoft.Maui.Devices.Sensors.Gyroscope.Default.IsSupported;
	public bool HasMagnetometer => Microsoft.Maui.Devices.Sensors.Magnetometer.Default.IsSupported;
	public bool HasOrientation => OrientationSensor.Default.IsSupported;
	public bool HasEnvironment => Barometer.Default.IsSupported || catalog.HasSensor(SensorType.Light) || catalog.HasSensor(SensorType.Proximity);

	public override void OnAppearing()
	{
		var rows = catalog.GetSensorRows();
		SensorCount = rows.Count == 1 ? "1 sensor on this device" : $"{rows.Count} sensors on this device";
		AllSensors.Update(rows);

		var accelerometer = Microsoft.Maui.Devices.Sensors.Accelerometer.Default;
		accelerometer.ReadingChanged += OnAccelerometer;
		TryStart(accelerometer.IsSupported, accelerometer.IsMonitoring, () => accelerometer.Start(SensorSpeed.UI));

		var gyro = Microsoft.Maui.Devices.Sensors.Gyroscope.Default;
		gyro.ReadingChanged += OnGyroscope;
		TryStart(gyro.IsSupported, gyro.IsMonitoring, () => gyro.Start(SensorSpeed.UI));

		var magnetometer = Microsoft.Maui.Devices.Sensors.Magnetometer.Default;
		magnetometer.ReadingChanged += OnMagnetometer;
		TryStart(magnetometer.IsSupported, magnetometer.IsMonitoring, () => magnetometer.Start(SensorSpeed.UI));

		var compass = Microsoft.Maui.Devices.Sensors.Compass.Default;
		compass.ReadingChanged += OnCompass;
		TryStart(compass.IsSupported, compass.IsMonitoring, () => compass.Start(SensorSpeed.UI, applyLowPassFilter: true));

		OrientationSensor.Default.ReadingChanged += OnOrientation;
		TryStart(OrientationSensor.Default.IsSupported, OrientationSensor.Default.IsMonitoring, () => OrientationSensor.Default.Start(SensorSpeed.UI));

		Barometer.Default.ReadingChanged += OnBarometer;
		TryStart(Barometer.Default.IsSupported, Barometer.Default.IsMonitoring, () => Barometer.Default.Start(SensorSpeed.UI));

		if (catalog.Listen(SensorType.Light, v => light = v[0]) is { } lightSensor)
			androidSensors.Add(lightSensor);
		if (catalog.Listen(SensorType.Proximity, v => proximity = v[0]) is { } proximitySensor)
			androidSensors.Add(proximitySensor);

		StartPolling(TimeSpan.FromMilliseconds(100), PushReadings);
	}

	public override void OnDisappearing()
	{
		base.OnDisappearing();

		var accelerometer = Microsoft.Maui.Devices.Sensors.Accelerometer.Default;
		accelerometer.ReadingChanged -= OnAccelerometer;
		if (accelerometer.IsMonitoring)
			accelerometer.Stop();

		var gyro = Microsoft.Maui.Devices.Sensors.Gyroscope.Default;
		gyro.ReadingChanged -= OnGyroscope;
		if (gyro.IsMonitoring)
			gyro.Stop();

		var magnetometer = Microsoft.Maui.Devices.Sensors.Magnetometer.Default;
		magnetometer.ReadingChanged -= OnMagnetometer;
		if (magnetometer.IsMonitoring)
			magnetometer.Stop();

		var compass = Microsoft.Maui.Devices.Sensors.Compass.Default;
		compass.ReadingChanged -= OnCompass;
		if (compass.IsMonitoring)
			compass.Stop();

		OrientationSensor.Default.ReadingChanged -= OnOrientation;
		if (OrientationSensor.Default.IsMonitoring)
			OrientationSensor.Default.Stop();

		Barometer.Default.ReadingChanged -= OnBarometer;
		if (Barometer.Default.IsMonitoring)
			Barometer.Default.Stop();

		foreach (var sensor in androidSensors)
			sensor.Dispose();
		androidSensors.Clear();
	}

	static void TryStart(bool isSupported, bool isMonitoring, Action start)
	{
		if (!isSupported || isMonitoring)
			return;
		try
		{
			start();
		}
		catch (Exception ex) when (ex is FeatureNotSupportedException or InvalidOperationException)
		{
			// The card shows "not available" instead
		}
	}

	void OnAccelerometer(object? sender, AccelerometerChangedEventArgs e) => acceleration = e.Reading.Acceleration;
	void OnGyroscope(object? sender, GyroscopeChangedEventArgs e) => gyroscope = e.Reading.AngularVelocity;
	void OnMagnetometer(object? sender, MagnetometerChangedEventArgs e) => magnetic = e.Reading.MagneticField;
	void OnCompass(object? sender, CompassChangedEventArgs e) => heading = e.Reading.HeadingMagneticNorth;
	void OnBarometer(object? sender, BarometerChangedEventArgs e) => pressure = e.Reading.PressureInHectopascals;

	void OnOrientation(object? sender, OrientationSensorChangedEventArgs e)
	{
		var q = e.Reading.Orientation;
		orientation = SensorMath.QuaternionToEuler(q.X, q.Y, q.Z, q.W);
	}

	void PushReadings()
	{
		if (heading is { } h)
		{
			Heading = h;
			HeadingText = $"{h:0}° {SensorMath.Cardinal(h)}";
		}

		if (acceleration is { } a)
			Accelerometer.Update(Axes(a, "g", "0.000", includeMagnitude: true));
		if (gyroscope is { } g)
			Gyroscope.Update(Axes(g, "rad/s", "0.000"));
		if (magnetic is { } m)
			Magnetometer.Update(Axes(m, "µT", "0.0", includeMagnitude: true));

		if (orientation is { } o)
		{
			Orientation.Update(
			[
				new("Pitch", Format(o.Pitch, "0.0", "°")),
				new("Roll", Format(o.Roll, "0.0", "°")),
				new("Yaw", Format(o.Yaw, "0.0", "°")),
			]);
		}

		var environment = new RowList();
		if (pressure is { } p)
		{
			environment.Add("Air pressure", Format(p, "0.0", " hPa"));
			environment.Add("Est. altitude", Format(SensorMath.AltitudeFromPressure(p), "0", " m"));
		}
		if (light is { } lux)
			environment.Add("Light", Format(lux, "0", " lx"));
		if (proximity is { } cm)
			environment.Add("Proximity", Format(cm, "0.#", " cm"));
		if (environment.Count > 0)
			Environment.Update(environment);
	}

	static List<InfoRow> Axes(Vector3 v, string unit, string format, bool includeMagnitude = false)
	{
		var rows = new List<InfoRow>
		{
			new("X", Format(v.X, format, " " + unit)),
			new("Y", Format(v.Y, format, " " + unit)),
			new("Z", Format(v.Z, format, " " + unit)),
		};
		if (includeMagnitude)
			rows.Add(new("Total", Format(SensorMath.Magnitude(v.X, v.Y, v.Z), format, " " + unit)));
		return rows;
	}

	static string Format(double value, string format, string unit) => value.ToString(format, Culture) + unit;
}
