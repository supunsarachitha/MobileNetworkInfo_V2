using System.Runtime.Intrinsics.X86;
using Microsoft.Maui.Devices.Sensors;

namespace MobileNetworkInfo;

public partial class MainPage : ContentPage
{
    int count = 0;

    PermissionStatus locationPermission;
    PermissionStatus sensorsPermission;
    PermissionStatus batteryPermission;
    PermissionStatus networkPermission;

    public MainPage()
    {
        InitializeComponent();

        Device.BeginInvokeOnMainThread(async () =>
        {

            CheckAndRequestLocationPermission();
            CheckAndRequestNetworkPermission();
            CheckAndRequestBatteryPermission();
            CheckAndRequestSensorPermission();

            ReadDeviceInfo();

            Battery.Default.BatteryInfoChanged += Battery_BatteryInfoChanged;

            try
            {
                // Turn on accelerometer
                Accelerometer.Default.ReadingChanged += Accelerometer_ReadingChanged;
                Accelerometer.Default.Start(SensorSpeed.UI);
            }
            catch (Exception ex)
            {

            }

            try
            {
                // Turn on Barometer
                Barometer.Default.ReadingChanged += Barometer_ReadingChanged;
                Barometer.Default.Start(SensorSpeed.UI);
            }
            catch (Exception ex)
            {

            }

            try
            {
                // Turn on compass
                Compass.Default.ReadingChanged += Compass_ReadingChanged;
                Compass.Default.Start(SensorSpeed.UI);
            }
            catch (Exception ex)
            {

            }

            try
            {
                // Turn on Gyroscope
                Gyroscope.Default.ReadingChanged += Gyroscope_ReadingChanged;
                Gyroscope.Default.Start(SensorSpeed.UI);


            }
            catch (Exception ex)
            {
                lblBarometer.IsVisible = false;
            }

            try
            {
                // Turn on Magnetometer
                Magnetometer.Default.ReadingChanged += Magnetometer_ReadingChanged;
                Magnetometer.Default.Start(SensorSpeed.UI);
            }
            catch (Exception ex)
            {

            }

            try
            {
                // Turn on OrientationSensor
                OrientationSensor.Default.ReadingChanged += Orientation_ReadingChanged;
                OrientationSensor.Default.Start(SensorSpeed.UI);
            }
            catch (Exception ex)
            {

            }
        });


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
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"X : {e.Reading.MagneticField.X}");
        sb.AppendLine($"Y : {e.Reading.MagneticField.Y}");
        sb.AppendLine($"Z : {e.Reading.MagneticField.Z}");
        lblMagnetometer.Text = sb.ToString();
    }

    private void Gyroscope_ReadingChanged(object sender, GyroscopeChangedEventArgs e)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"X : {e.Reading.AngularVelocity.X}");
        sb.AppendLine($"Y : {e.Reading.AngularVelocity.Y}");
        sb.AppendLine($"Z : {e.Reading.AngularVelocity.Z}");
        lblGyroscope.Text = sb.ToString();
    }

    private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
    {
        lblCompass.Text = e.Reading.HeadingMagneticNorth.ToString();
        imgCompass.RotateTo((e.Reading.HeadingMagneticNorth) * 250 / 360);
    }

    private void Barometer_ReadingChanged(object sender, BarometerChangedEventArgs e)
    {
        lblBarometer.Text = "(hPa) " + e.Reading.PressureInHectopascals.ToString();
    }

    private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
    {
        // Update UI Label with accelerometer state
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"X : {e.Reading.Acceleration.X}");
        sb.AppendLine($"Y : {e.Reading.Acceleration.Y}");
        sb.AppendLine($"Z : {e.Reading.Acceleration.Z}");
        lblAccel.Text = sb.ToString();
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

        lblPowerSource.Text = Battery.Default.PowerSource switch
        {
            BatteryPowerSource.Wireless => "Wireless charging",
            BatteryPowerSource.Usb => "USB cable charging",
            BatteryPowerSource.AC => "Device is plugged in to a power source",
            BatteryPowerSource.Battery => "Device isn't charging",
            _ => "Unknown"
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

        
    }

    public async Task<PermissionStatus> CheckAndRequestLocationPermission()
    {
        PermissionStatus locationPermission = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (locationPermission == PermissionStatus.Granted)
            return locationPermission;

        if (locationPermission == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
        {
            // Prompt the user to turn on in settings
            // On iOS once a permission has been denied it may not be requested again from the application
            return locationPermission;
        }

        if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
        {
            // Prompt the user with additional information as to why the permission is needed
        }

        locationPermission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        return locationPermission;
    }

    public async Task<PermissionStatus> CheckAndRequestNetworkPermission()
    {
        PermissionStatus networkPermission = await Permissions.CheckStatusAsync<Permissions.NetworkState>();

        if (networkPermission == PermissionStatus.Granted)
            return networkPermission;

        if (networkPermission == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
        {
            // Prompt the user to turn on in settings
            // On iOS once a permission has been denied it may not be requested again from the application
            return networkPermission;
        }

        if (Permissions.ShouldShowRationale<Permissions.NetworkState>())
        {
            // Prompt the user with additional information as to why the permission is needed
        }

        networkPermission = await Permissions.RequestAsync<Permissions.NetworkState>();

        return networkPermission;
    }

    public async Task<PermissionStatus> CheckAndRequestBatteryPermission()
    {
        PermissionStatus batteryPermission = await Permissions.CheckStatusAsync<Permissions.Battery>();

        if (batteryPermission == PermissionStatus.Granted)
            return batteryPermission;

        if (batteryPermission == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
        {
            // Prompt the user to turn on in settings
            // On iOS once a permission has been denied it may not be requested again from the application
            return batteryPermission;
        }

        if (Permissions.ShouldShowRationale<Permissions.Battery>())
        {
            // Prompt the user with additional information as to why the permission is needed
        }

        batteryPermission = await Permissions.RequestAsync<Permissions.Battery>();

        return batteryPermission;
    }

    public async Task<PermissionStatus> CheckAndRequestSensorPermission()
    {
        PermissionStatus sensorPermission = await Permissions.CheckStatusAsync<Permissions.Sensors>();

        if (sensorPermission == PermissionStatus.Granted)
            return sensorPermission;

        if (sensorPermission == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
        {
            // Prompt the user to turn on in settings
            // On iOS once a permission has been denied it may not be requested again from the application
            return sensorPermission;
        }

        if (Permissions.ShouldShowRationale<Permissions.Sensors>())
        {
            // Prompt the user with additional information as to why the permission is needed
        }

        sensorPermission = await Permissions.RequestAsync<Permissions.Sensors>();

        return sensorPermission;
    }
}


