using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MobileNetworkInfo.Helpers;
using MobileNetworkInfo.Services;
using Icons = MobileNetworkInfo.Resources.Icons;

namespace MobileNetworkInfo.ViewModels;

public sealed class WifiNetworkItem(WifiNetwork network)
{
	public string Name => network.Ssid;
	public string Details => string.Join("  ·  ", new[]
	{
		network.Channel > 0 ? $"Ch {network.Channel}" : null,
		network.Band,
		network.Standard,
		network.Width,
		network.Security,
	}.Where(s => s is not null));
	public string Bssid => network.Bssid;
	public string Signal => Formatters.Dbm(network.Rssi);
	public string SignalGlyph => Icons.WifiBars[(int)network.Level];
	public Color SignalColor => OverviewViewModel.SignalColor(ConnectionKind.Wifi, network.Level);
	public bool IsConnected => network.IsConnected;
	public bool IsOpen => WifiHelper.IsOpen(network.Security);
}

public partial class WifiScanViewModel(NetworkInfoService network) : BaseViewModel
{
	bool hasScanned;
	bool permissionPermanentlyDenied;

	public ObservableCollection<WifiNetworkItem> Networks { get; } = [];

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(ScanCommand))]
	public partial bool IsScanning { get; set; }

	[ObservableProperty] public partial string Status { get; set; } = "";
	[ObservableProperty] public partial string BandSummary { get; set; } = "";
	[ObservableProperty] public partial bool NeedsPermission { get; set; }
	[ObservableProperty] public partial string ScanButtonText { get; set; } = "Scan";

	public override void OnAppearing()
	{
		NeedsPermission = !network.HasFineLocation;
		if (!NeedsPermission)
		{
			// Also covers returning from app settings after allowing location
			permissionPermanentlyDenied = false;
			ScanButtonText = "Scan";
			if (!hasScanned)
				ScanCommand.Execute(null);
		}
		else
		{
			ShowPermissionStatus();
		}
	}

	void ShowPermissionStatus()
	{
		Status = permissionPermanentlyDenied
			? "Location access is turned off for this app. Open settings and allow location to scan for Wi-Fi networks."
			: "Android needs location access to scan for Wi-Fi networks. It's only used on your device.";
		ScanButtonText = permissionPermanentlyDenied ? "Open settings" : "Allow";
	}

	[RelayCommand(CanExecute = nameof(CanScan))]
	async Task Scan()
	{
		if (!network.HasFineLocation)
		{
			if (permissionPermanentlyDenied)
			{
				SystemSettings.Open(SystemSettings.AppDetails);
				return;
			}

			var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
			NeedsPermission = status != PermissionStatus.Granted || !network.HasFineLocation;
			if (NeedsPermission)
			{
				permissionPermanentlyDenied = !Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>();
				ShowPermissionStatus();
				return;
			}
			ScanButtonText = "Scan";
		}

		if (!network.IsWifiEnabled)
		{
			Status = "Wi-Fi is turned off. Turn it on to scan for networks.";
			Networks.Clear();
			BandSummary = "";
			return;
		}

		IsScanning = true;
		Status = "Scanning…";
		var result = await network.ScanWifiAsync();
		hasScanned = true;

		Networks.Clear();
		foreach (var item in result.Networks)
			Networks.Add(new WifiNetworkItem(item));

		var count = result.Networks.Count;
		Status = count switch
		{
			0 when !network.IsLocationEnabled => "No networks found. Turn on device location to scan.",
			0 => "No networks found.",
			1 => "1 network found",
			_ => $"{count} networks found",
		};
		if (!result.IsFresh && count > 0)
			Status += " · Android limits how often apps can scan, showing the latest results";

		BandSummary = string.Join("   ", result.Networks
			.GroupBy(n => n.Band)
			.OrderBy(g => g.Key)
			.Select(g => $"{g.Key}: {g.Count()}"));
		IsScanning = false;
	}

	bool CanScan() => !IsScanning;
}
