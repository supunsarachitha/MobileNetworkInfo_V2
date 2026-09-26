using MobileNetworkInfo.Helpers;

namespace MobileNetworkInfo.Services;

/// <summary>Loads the bundled changelog and remembers whether the user has seen the latest one.</summary>
public sealed class WhatsNewService
{
	const string LastSeenKey = "whats_new_last_seen_version";

	IReadOnlyList<ChangelogRelease>? releases;

	public async Task<IReadOnlyList<ChangelogRelease>> GetReleasesAsync()
	{
		if (releases is not null)
			return releases;

		await using var stream = await FileSystem.Current.OpenAppPackageFileAsync("CHANGELOG.md");
		using var reader = new StreamReader(stream);
		releases = ChangelogParser.Parse(await reader.ReadToEndAsync());
		return releases;
	}

	/// <summary>True right after an update, until the user opens or dismisses "What's new". Never on a fresh install.</summary>
	public bool ShouldShowUpdateBanner
	{
		get
		{
			var current = AppInfo.Current.VersionString;
			var lastSeen = Preferences.Default.Get<string?>(LastSeenKey, null);
			if (lastSeen == current)
				return false;

			if (lastSeen is null && IsFreshInstall())
			{
				MarkSeen();
				return false;
			}
			return true;
		}
	}

	public void MarkSeen() => Preferences.Default.Set(LastSeenKey, AppInfo.Current.VersionString);

	static bool IsFreshInstall()
	{
		var context = Android.App.Application.Context;
		var info = context.PackageManager?.GetPackageInfo(context.PackageName!, 0);
		return info is null || info.FirstInstallTime == info.LastUpdateTime;
	}
}
