using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MobileNetworkInfo.Controls;
using MobileNetworkInfo.Helpers;
using MobileNetworkInfo.Services;
using MobileNetworkInfo.Views;
using Icons = MobileNetworkInfo.Resources.Icons;

namespace MobileNetworkInfo.ViewModels;

public partial class OverviewViewModel(
	DeviceInfoService device,
	NetworkInfoService network,
	BatteryInfoService battery,
	ReportService report,
	WhatsNewService whatsNew) : BaseViewModel
{
	public string DeviceName => device.Headline;
	public string DeviceSubtitle => device.Subtitle;

	[ObservableProperty] public partial string Uptime { get; set; } = "";

	[ObservableProperty] public partial string NetworkTitle { get; set; } = "";
	[ObservableProperty] public partial string? NetworkDetail { get; set; }
	[ObservableProperty] public partial string? NetworkSignal { get; set; }
	[ObservableProperty] public partial string NetworkGlyph { get; set; } = Icons.CloudOff;
	[ObservableProperty] public partial Color NetworkColor { get; set; } = Colors.Gray;

	[ObservableProperty] public partial string BatteryPercent { get; set; } = "";
	[ObservableProperty] public partial string BatteryStatus { get; set; } = "";
	[ObservableProperty] public partial string BatteryGlyph { get; set; } = Icons.Battery;
	[ObservableProperty] public partial double BatteryFraction { get; set; }
	[ObservableProperty] public partial Color BatteryColor { get; set; } = Colors.Gray;

	[ObservableProperty] public partial string RamText { get; set; } = "";
	[ObservableProperty] public partial string RamDetail { get; set; } = "";
	[ObservableProperty] public partial double RamFraction { get; set; }

	[ObservableProperty] public partial string StorageText { get; set; } = "";
	[ObservableProperty] public partial string StorageDetail { get; set; } = "";
	[ObservableProperty] public partial double StorageFraction { get; set; }

	public bool HasDataUsageSettings => OperatingSystem.IsAndroidVersionAtLeast(28);

	public string UpdateTitle => $"Updated to version {AppInfo.Current.VersionString}";

	[ObservableProperty] public partial bool ShowUpdateBanner { get; set; }

	public override void OnAppearing()
	{
		ShowUpdateBanner = whatsNew.ShouldShowUpdateBanner;
		StartPolling(TimeSpan.FromSeconds(3), RefreshAsync);
	}

	async Task RefreshAsync()
	{
		// Read everything off the UI thread, then update the bindings
		var (net, b, ram, storage) = await Task.Run(() => (network.GetSnapshot(), battery.Get(), device.GetMemory(), device.GetStorage()));

		Uptime = $"Last restarted {Formatters.Duration(device.Uptime)} ago";

		NetworkTitle = net.IsVpn && net.Kind != ConnectionKind.Other ? $"{net.Title} + VPN" : net.Title;
		NetworkDetail = net.Detail;
		NetworkSignal = net.SignalDbm is { } dbm ? SignalQuality.Summary(dbm, net.Signal) : null;
		NetworkGlyph = net.Kind switch
		{
			ConnectionKind.Wifi => Icons.WifiBars[(int)net.Signal],
			ConnectionKind.Cellular => Icons.CellBars[(int)net.Signal],
			ConnectionKind.Ethernet => Icons.Ethernet,
			ConnectionKind.Other => Icons.VpnKey,
			_ => Icons.CloudOff,
		};
		NetworkColor = SignalColor(net.Kind, net.Signal);

		BatteryPercent = $"{b.Level}%";
		BatteryStatus = b.IsCharging ? $"{b.Status} · {b.PowerSource}" : b.Status;
		BatteryGlyph = b.IsCharging ? Icons.BatteryCharging : b.Level <= 15 ? Icons.BatteryAlert : Icons.Battery;
		BatteryFraction = b.LevelFraction;
		BatteryColor = LevelColor(b.Level, b.IsCharging);

		RamText = Formatters.Percent(ram.UsedFraction);
		RamDetail = Formatters.UsedOf(ram.Used, ram.Total);
		RamFraction = ram.UsedFraction;

		StorageText = Formatters.Percent(storage.UsedFraction);
		StorageDetail = Formatters.UsedOf(storage.Used, storage.Total);
		StorageFraction = storage.UsedFraction;
	}

	internal static Color SignalColor(ConnectionKind kind, SignalLevel level) => kind switch
	{
		ConnectionKind.None => ThemeColors.Get("TextSecondary"),
		ConnectionKind.Wifi or ConnectionKind.Cellular => level switch
		{
			SignalLevel.Excellent or SignalLevel.Good => ThemeColors.Get("GoodColor"),
			SignalLevel.Fair => ThemeColors.Get("FairColor"),
			SignalLevel.Poor => ThemeColors.Get("PoorColor"),
			_ => ThemeColors.Get("TextSecondary"),
		},
		_ => ThemeColors.Get("GoodColor"),
	};

	internal static Color LevelColor(int level, bool charging) =>
		charging || level > 50 ? ThemeColors.Get("GoodColor")
		: level > 20 ? ThemeColors.Get("FairColor")
		: ThemeColors.Get("PoorColor");

	[RelayCommand]
	Task Share() => report.ShareAsync();

	[RelayCommand]
	Task OpenWhatsNew()
	{
		ShowUpdateBanner = false;
		return Shell.Current.GoToAsync(nameof(WhatsNewPage));
	}

	[RelayCommand]
	void DismissUpdate()
	{
		whatsNew.MarkSeen();
		ShowUpdateBanner = false;
	}

	[RelayCommand]
	Task OpenSupport() => Shell.Current.GoToAsync(nameof(SupportPage));

	[RelayCommand]
	Task GoTo(string route) => Shell.Current.GoToAsync("//" + route);

	[RelayCommand]
	void OpenSettings(string target) => SystemSettings.Open(target);
}
