using MobileNetworkInfo.Models;
using MobileNetworkInfo.Services;

namespace MobileNetworkInfo.ViewModels;

public partial class DeviceViewModel(DeviceInfoService device) : BaseViewModel
{
	public string Headline => device.Headline;
	public string Subtitle => device.Subtitle;

	public InfoSection Device { get; } = [];
	public InfoSection SystemInfo { get; } = [];
	public InfoSection Processor { get; } = [];
	public InfoSection Memory { get; } = [];
	public InfoSection Display { get; } = [];
	public InfoSection Features { get; } = [];

	public override void OnAppearing()
	{
		Device.Update(device.GetDeviceRows());
		Processor.Update(device.GetProcessorRows());
		Display.Update(device.GetDisplayRows());
		Features.Update(device.GetFeatureRows());

		// Memory and uptime change over time
		StartPolling(TimeSpan.FromSeconds(5), () =>
		{
			SystemInfo.Update(device.GetSystemRows());
			Memory.Update(device.GetMemoryRows());
		});
	}
}
