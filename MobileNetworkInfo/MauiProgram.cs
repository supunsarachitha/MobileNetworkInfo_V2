using Microsoft.Extensions.Logging;
using MobileNetworkInfo.Services;
using MobileNetworkInfo.ViewModels;
using MobileNetworkInfo.Views;

namespace MobileNetworkInfo;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("MaterialIconsRound-Regular.otf", Resources.Icons.FontFamily);
			});

		builder.Services
			.AddSingleton<DeviceInfoService>()
			.AddSingleton<NetworkInfoService>()
			.AddSingleton<BatteryInfoService>()
			.AddSingleton<SensorCatalogService>()
			.AddSingleton<ReportService>()
			.AddSingleton<WhatsNewService>();

		builder.Services
			.AddTransient<OverviewViewModel>().AddTransient<OverviewPage>()
			.AddTransient<NetworkViewModel>().AddTransient<NetworkPage>()
			.AddTransient<WifiScanViewModel>().AddTransient<WifiScanPage>()
			.AddTransient<DeviceViewModel>().AddTransient<DevicePage>()
			.AddTransient<BatteryViewModel>().AddTransient<BatteryPage>()
			.AddTransient<SensorsViewModel>().AddTransient<SensorsPage>()
			.AddTransient<AboutViewModel>().AddTransient<AboutPage>()
			.AddTransient<WhatsNewViewModel>().AddTransient<WhatsNewPage>()
			.AddTransient<SupportViewModel>().AddTransient<SupportPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
