using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MobileNetworkInfo.Services;

namespace MobileNetworkInfo.Models;

/// <summary>One row in an info card. The value updates in place so live data doesn't re-create the view.</summary>
public partial class InfoItem : ObservableObject
{
	public InfoItem(string label, string value)
	{
		Label = label;
		Value = value;
	}

	public string Label { get; }

	[ObservableProperty]
	public partial string Value { get; set; }

	[RelayCommand]
	Task Copy() => ClipboardHelper.CopyAsync(Label, Value);
}
