using Android.Content;
using Android.Provider;

namespace MobileNetworkInfo.Services;

/// <summary>Opens system settings screens related to connectivity.</summary>
public static class SystemSettings
{
	public const string Internet = "internet";
	public const string Wifi = "wifi";
	public const string Mobile = "mobile";
	public const string DataUsage = "data";
	public const string Location = "location";
	public const string AppDetails = "app";

	public static void Open(string target)
	{
		var action = target switch
		{
			Internet when OperatingSystem.IsAndroidVersionAtLeast(29) => Settings.Panel.ActionInternetConnectivity,
			Internet or Wifi => Settings.ActionWifiSettings,
			Mobile => Settings.ActionDataRoamingSettings,
			DataUsage when OperatingSystem.IsAndroidVersionAtLeast(28) => Settings.ActionDataUsageSettings,
			Location => Settings.ActionLocationSourceSettings,
			AppDetails => Settings.ActionApplicationDetailsSettings,
			_ => Settings.ActionWirelessSettings,
		};

		if (!TryStart(action, target == AppDetails) && !TryStart(Settings.ActionWirelessSettings, false))
			TryStart(Settings.ActionSettings, false);
	}

	static bool TryStart(string? action, bool withPackage)
	{
		if (action is null)
			return false;

		var context = Platform.CurrentActivity ?? Android.App.Application.Context;
		var intent = new Intent(action);
		if (withPackage)
			intent.SetData(Android.Net.Uri.Parse("package:" + context.PackageName));
		if (context is not Android.App.Activity)
			intent.AddFlags(ActivityFlags.NewTask);

		try
		{
			context.StartActivity(intent);
			return true;
		}
		catch (ActivityNotFoundException)
		{
			return false;
		}
	}
}
