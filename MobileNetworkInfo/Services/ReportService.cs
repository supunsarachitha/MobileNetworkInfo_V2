using MobileNetworkInfo.Helpers;

namespace MobileNetworkInfo.Services;

/// <summary>Collects every section into a plain-text report and opens the share sheet.</summary>
public sealed class ReportService(
	DeviceInfoService device,
	NetworkInfoService network,
	BatteryInfoService battery,
	SensorCatalogService sensors)
{
	public string BuildReport()
	{
		var snapshot = network.GetSnapshot();
		var sections = new List<ReportSection>
		{
			new("Device", device.GetDeviceRows()),
			new("System", device.GetSystemRows()),
			new("Processor", device.GetProcessorRows()),
			new("Memory & storage", device.GetMemoryRows()),
			new("Display", device.GetDisplayRows()),
			new("Connection", snapshot.Connection),
			new("Wi-Fi", snapshot.Wifi ?? []),
			new("Mobile network", snapshot.Cellular ?? []),
			new("Serving cell", snapshot.ServingCell ?? []),
			new("IP & DNS", snapshot.Addresses),
			new("Battery", battery.Get().ToRows()),
			new("Hardware features", device.GetFeatureRows()),
			new("Sensors", sensors.GetSensorRows()),
		};

		return ReportBuilder.Build(AppInfo.Current.Name, AppInfo.Current.VersionString, DateTimeOffset.Now, sections);
	}

	public Task ShareAsync() =>
		Share.Default.RequestAsync(new ShareTextRequest
		{
			Title = "Share device report",
			Subject = $"{device.Headline} – device report",
			Text = BuildReport(),
		});
}
