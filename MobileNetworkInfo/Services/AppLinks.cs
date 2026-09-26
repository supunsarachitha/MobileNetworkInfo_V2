namespace MobileNetworkInfo.Services;

/// <summary>External links used across the app.</summary>
public static class AppLinks
{
	public const string Repository = "https://github.com/supunsarachitha/MobileNetworkInfo_V2";
	public const string Issues = Repository + "/issues";
	public const string PrivacyPolicy = Repository + "/blob/main/PRIVACY_POLICY.md";
	public const string License = Repository + "/blob/main/LICENSE";

	public static string StoreUrl => $"https://play.google.com/store/apps/details?id={AppInfo.Current.PackageName}";

	public static Task OpenAsync(string url) => Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);

	/// <summary>Opens the Play Store app when available, otherwise the Play website.</summary>
	public static async Task OpenStoreAsync()
	{
		if (!await Launcher.Default.TryOpenAsync($"market://details?id={AppInfo.Current.PackageName}"))
			await OpenAsync(StoreUrl);
	}

	public static Task ShareAppAsync() => Share.Default.RequestAsync(new ShareTextRequest
	{
		Title = "Share app",
		Text = $"{AppInfo.Current.Name}: see everything about your phone's network, battery and sensors. Free and ad-free. {StoreUrl}",
	});
}
