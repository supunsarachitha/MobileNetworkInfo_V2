using CommunityToolkit.Mvvm.ComponentModel;
using MobileNetworkInfo.Models;
using MobileNetworkInfo.Services;
using Icons = MobileNetworkInfo.Resources.Icons;

namespace MobileNetworkInfo.ViewModels;

public partial class BatteryViewModel(BatteryInfoService battery) : BaseViewModel
{
	public InfoSection Details { get; } = [];

	[ObservableProperty] public partial string LevelText { get; set; } = "";
	[ObservableProperty] public partial string StatusText { get; set; } = "";
	[ObservableProperty] public partial string? Hint { get; set; }
	[ObservableProperty] public partial double Fraction { get; set; }
	[ObservableProperty] public partial Color RingColor { get; set; } = Colors.Gray;
	[ObservableProperty] public partial string Glyph { get; set; } = Icons.Battery;

	public override void OnAppearing() => StartPolling(TimeSpan.FromSeconds(2), Refresh);

	void Refresh()
	{
		var b = battery.Get();
		LevelText = $"{b.Level}%";
		StatusText = b.IsCharging ? $"{b.Status} · {b.PowerSource}" : b.Status;
		Fraction = b.LevelFraction;
		RingColor = OverviewViewModel.LevelColor(b.Level, b.IsCharging);
		Glyph = b.IsCharging ? Icons.BatteryCharging : b.Level <= 15 ? Icons.BatteryAlert : Icons.Battery;
		Hint = b switch
		{
			{ TimeToFull: { } ttf } => $"Full in about {Helpers.Formatters.Duration(ttf)}",
			{ TemperatureC: > 45 } => "Battery is hot. Let the device cool down.",
			{ PowerSaver: true } => "Battery Saver is on",
			_ => null,
		};
		Details.Update(b.ToRows());
	}
}
