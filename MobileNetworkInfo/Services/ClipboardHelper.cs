using Android.Widget;

namespace MobileNetworkInfo.Services;

public static class ClipboardHelper
{
	public static async Task CopyAsync(string label, string value)
	{
		await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.SetTextAsync(value);

		// Android 13+ shows its own clipboard confirmation; older versions need a toast.
		if (!OperatingSystem.IsAndroidVersionAtLeast(33))
			Toast.MakeText(Android.App.Application.Context, $"{label} copied", ToastLength.Short)?.Show();
	}

	public static void ShowMessage(string message) =>
		Toast.MakeText(Android.App.Application.Context, message, ToastLength.Short)?.Show();
}
