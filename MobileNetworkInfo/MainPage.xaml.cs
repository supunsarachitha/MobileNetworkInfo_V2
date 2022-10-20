using System.Runtime.Intrinsics.X86;
using Microsoft.Maui.Devices.Sensors;

namespace MobileNetworkInfo;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
        ReadDeviceInfo();
        Battery.Default.BatteryInfoChanged += Battery_BatteryInfoChanged;

        // Turn on accelerometer
        Accelerometer.Default.ReadingChanged += Accelerometer_ReadingChanged;
        Accelerometer.Default.Start(SensorSpeed.UI);

        // Turn on accelerometer
        Barometer.Default.ReadingChanged += Barometer_ReadingChanged;
        Barometer.Default.Start(SensorSpeed.UI);

        // Turn on compass
        Compass.Default.ReadingChanged += Compass_ReadingChanged;
        Compass.Default.Start(SensorSpeed.UI);

        // Turn on compass
        Gyroscope.Default.ReadingChanged += Gyroscope_ReadingChanged;
        Gyroscope.Default.Start(SensorSpeed.UI);

        // Turn on compass
        Magnetometer.Default.ReadingChanged += Magnetometer_ReadingChanged;
        Magnetometer.Default.Start(SensorSpeed.UI);

        // Turn on compass
        OrientationSensor.Default.ReadingChanged += Orientation_ReadingChanged;
        OrientationSensor.Default.Start(SensorSpeed.UI);

    }

    private void Orientation_ReadingChanged(object sender, OrientationSensorChangedEventArgs e)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"X : {e.Reading.Orientation.X}");
        sb.AppendLine($"Y : {e.Reading.Orientation.Y}");
        sb.AppendLine($"Z : {e.Reading.Orientation.Z}");
        sb.AppendLine($"W : {e.Reading.Orientation.W}");
        lblOrientationSensor.Text = sb.ToString();
    }

    private void Magnetometer_ReadingChanged(object sender, MagnetometerChangedEventArgs e)
    {
        lblMagnetometer.Text = e.Reading.ToString();
    }

    private void Gyroscope_ReadingChanged(object sender, GyroscopeChangedEventArgs e)
    {
        lblGyroscope.Text = e.Reading.ToString();
    }

    private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
    {
        lblCompass.Text = e.Reading.HeadingMagneticNorth.ToString();
        imgCompass.RotateTo((e.Reading.HeadingMagneticNorth) * 250/360);
    }

    private void Barometer_ReadingChanged(object sender, BarometerChangedEventArgs e)
    {
        lblBarometer.Text = "(hPa) " + e.Reading.PressureInHectopascals.ToString();
    }

    private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
    {
        // Update UI Label with accelerometer state
        lblAccel.Text = e.Reading.ToString();
    }

    private void Battery_BatteryInfoChanged(object sender, BatteryInfoChangedEventArgs e)
    {
        lblBatteryState.Text = e.State switch
        {
            BatteryState.Charging => "Battery is currently charging",
            BatteryState.Discharging => "Charger is not connected and the battery is discharging",
            BatteryState.Full => "Battery is full",
            BatteryState.NotCharging => "The battery isn't charging.",
            BatteryState.NotPresent => "Battery is not available.",
            BatteryState.Unknown => "Battery is unknown",
            _ => "Battery is unknown"
        };
    }

    private void ReadDeviceInfo()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        lblModel.Text = DeviceInfo.Current.Model;
        lblManufacturer.Text = DeviceInfo.Current.Manufacturer;
        lblName.Text = DeviceInfo.Name;
        lblVersionString.Text = DeviceInfo.VersionString.ToString();
        lblIdiom.Text = DeviceInfo.Current.Idiom.ToString();
        lblPlatform.Text = DeviceInfo.Current.Platform.ToString();
        lblDeviceType.Text = DeviceInfo.Current.DeviceType.ToString();

        
        lblPixelWidth.Text = DeviceDisplay.Current.MainDisplayInfo.Width.ToString();
        lblPixelHeight.Text = DeviceDisplay.Current.MainDisplayInfo.Height.ToString();
        lblDensity.Text = DeviceDisplay.Current.MainDisplayInfo.Density.ToString();
        lblOrientation.Text = DeviceDisplay.Current.MainDisplayInfo.Orientation.ToString();
        lblRotation.Text = DeviceDisplay.Current.MainDisplayInfo.Rotation.ToString();
        lblRefreshRate.Text = DeviceDisplay.Current.MainDisplayInfo.RefreshRate.ToString("##.##");
        
        lblPowerSource.Text = Battery.Default.PowerSource switch
        {
            BatteryPowerSource.Wireless => "Wireless charging",
            BatteryPowerSource.Usb => "USB cable charging",
            BatteryPowerSource.AC => "Device is plugged in to a power source",
            BatteryPowerSource.Battery => "Device isn't charging",
            _ => "Unknown"
        };

    }
}


