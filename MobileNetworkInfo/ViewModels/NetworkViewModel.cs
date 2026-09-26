using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MobileNetworkInfo.Helpers;
using MobileNetworkInfo.Models;
using MobileNetworkInfo.Services;
using MobileNetworkInfo.Views;
using Icons = MobileNetworkInfo.Resources.Icons;

namespace MobileNetworkInfo.ViewModels;

public partial class NetworkViewModel(NetworkInfoService network) : BaseViewModel
{
	bool locationPermanentlyDenied;

	public InfoSection Connection { get; } = [];
	public InfoSection Wifi { get; } = [];
	public InfoSection Cellular { get; } = [];
	public InfoSection ServingCell { get; } = [];
	public InfoSection Addresses { get; } = [];
	public InfoSection Latency { get; } = [];

	[ObservableProperty] public partial string Title { get; set; } = "";
	[ObservableProperty] public partial string? Detail { get; set; }
	[ObservableProperty] public partial string? SignalText { get; set; }
	[ObservableProperty] public partial string Glyph { get; set; } = Icons.CloudOff;
	[ObservableProperty] public partial Color SignalColor { get; set; } = Colors.Gray;

	[ObservableProperty] public partial bool HasWifi { get; set; }
	[ObservableProperty] public partial bool HasCellular { get; set; }
	[ObservableProperty] public partial bool HasServingCell { get; set; }

	[ObservableProperty] public partial bool ShowLocationPrompt { get; set; }
	[ObservableProperty] public partial string LocationPromptText { get; set; } = "";
	[ObservableProperty] public partial string LocationButtonText { get; set; } = "";

	[ObservableProperty] public partial string PublicIp { get; set; } = "Not checked";

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(CheckPublicIpCommand))]
	public partial bool IsCheckingIp { get; set; }

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(RunLatencyTestCommand))]
	public partial bool IsTestingLatency { get; set; }

	[ObservableProperty] public partial bool HasLatencyResults { get; set; }

	public override void OnAppearing() => StartPolling(TimeSpan.FromSeconds(2), RefreshAsync);

	async Task RefreshAsync()
	{
		// System service calls (cell info, link properties) can take a while; keep them off the UI thread
		var snapshot = await Task.Run(network.GetSnapshot);

		Title = snapshot.IsVpn && snapshot.Kind != ConnectionKind.Other ? $"{snapshot.Title} + VPN" : snapshot.Title;
		Detail = snapshot.Detail;
		SignalText = snapshot.SignalDbm is { } dbm ? SignalQuality.Summary(dbm, snapshot.Signal) : null;
		Glyph = snapshot.Kind switch
		{
			ConnectionKind.Wifi => Icons.WifiBars[(int)snapshot.Signal],
			ConnectionKind.Cellular => Icons.CellBars[(int)snapshot.Signal],
			ConnectionKind.Ethernet => Icons.Ethernet,
			ConnectionKind.Other => Icons.VpnKey,
			_ => Icons.CloudOff,
		};
		SignalColor = OverviewViewModel.SignalColor(snapshot.Kind, snapshot.Signal);

		Connection.Update(snapshot.Connection);
		Wifi.Update(snapshot.Wifi ?? []);
		Cellular.Update(snapshot.Cellular ?? []);
		ServingCell.Update(snapshot.ServingCell ?? []);
		Addresses.Update(snapshot.Addresses);

		HasWifi = snapshot.Wifi is not null;
		HasCellular = snapshot.Cellular is not null;
		HasServingCell = snapshot.ServingCell is { Count: > 0 };

		UpdateLocationPrompt();
	}

	void UpdateLocationPrompt()
	{
		if (!network.HasFineLocation)
		{
			ShowLocationPrompt = true;
			LocationPromptText = "Android only shares the Wi-Fi name, nearby networks and cell tower details with apps that have precise location access. It is used on your device only and is never stored or shared.";
			LocationButtonText = locationPermanentlyDenied ? "Open app settings" : "Allow location access";
		}
		else if (!network.IsLocationEnabled)
		{
			ShowLocationPrompt = true;
			LocationPromptText = "Device location is turned off. Turn it on to see the Wi-Fi name and cell tower details.";
			LocationButtonText = "Turn on location";
		}
		else
		{
			ShowLocationPrompt = false;
		}
	}

	[RelayCommand]
	async Task RequestLocation()
	{
		if (network.HasFineLocation)
		{
			SystemSettings.Open(SystemSettings.Location);
			return;
		}

		if (locationPermanentlyDenied)
		{
			SystemSettings.Open(SystemSettings.AppDetails);
			return;
		}

		var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
		if (status != PermissionStatus.Granted && !Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
			locationPermanentlyDenied = true;
		await RefreshAsync();
	}

	[RelayCommand(CanExecute = nameof(CanCheckPublicIp))]
	async Task CheckPublicIp()
	{
		IsCheckingIp = true;
		PublicIp = "Checking…";
		PublicIp = await network.GetPublicIpAsync() ?? "Unavailable (offline?)";
		IsCheckingIp = false;
	}

	bool CanCheckPublicIp() => !IsCheckingIp;

	[RelayCommand(CanExecute = nameof(CanRunLatencyTest))]
	async Task RunLatencyTest()
	{
		IsTestingLatency = true;
		HasLatencyResults = true;
		Latency.Update(LatencyTester.DefaultTargets.Select(t => new InfoRow(t.Name, "Testing…")).ToList());

		var rows = new List<InfoRow>();
		foreach (var target in LatencyTester.DefaultTargets)
		{
			var result = await Task.Run(() => LatencyTester.MeasureAsync(target));
			rows.Add(new InfoRow(target.Name, result.Summary));
			Latency.Update(rows.Concat(LatencyTester.DefaultTargets.Skip(rows.Count).Select(t => new InfoRow(t.Name, "Testing…"))).ToList());
		}

		IsTestingLatency = false;
	}

	bool CanRunLatencyTest() => !IsTestingLatency;

	[RelayCommand]
	Task OpenWifiScanner() => Shell.Current.GoToAsync(nameof(WifiScanPage));

	[RelayCommand]
	void OpenSettings(string target) => SystemSettings.Open(target);
}
