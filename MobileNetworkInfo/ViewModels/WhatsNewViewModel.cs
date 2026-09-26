using System.Collections.ObjectModel;
using MobileNetworkInfo.Controls;
using MobileNetworkInfo.Helpers;
using MobileNetworkInfo.Services;
using Icons = MobileNetworkInfo.Resources.Icons;

namespace MobileNetworkInfo.ViewModels;

public sealed class ReleaseItem(ChangelogRelease release, bool isLatest)
{
	public string Title { get; } = release.Title;
	public bool IsLatest { get; } = isLatest;
	public IReadOnlyList<ChangeGroup> Groups { get; } = release.Sections.Select(s => new ChangeGroup(s)).ToList();
}

public sealed class ChangeGroup(ChangelogSection section)
{
	public string Title { get; } = section.Title;
	public bool HasTitle => Title.Length > 0;
	public IReadOnlyList<string> Items { get; } = section.Items;

	public string Glyph => Title switch
	{
		"New" => Icons.Star,
		"Improved" => Icons.TrendingUp,
		"Fixed" => Icons.Build,
		"Removed" => Icons.RemoveCircle,
		_ => Icons.Info,
	};

	public Color Color => Title switch
	{
		"New" => ThemeColors.Get("GoodColor"),
		"Improved" => ThemeColors.Get("InfoColor"),
		"Fixed" => ThemeColors.Get("FairColor"),
		"Removed" => ThemeColors.Get("PoorColor"),
		_ => ThemeColors.Get("TextSecondary"),
	};
}

public partial class WhatsNewViewModel(WhatsNewService whatsNew) : BaseViewModel
{
	public ObservableCollection<ReleaseItem> Releases { get; } = [];

	public override async void OnAppearing()
	{
		whatsNew.MarkSeen();
		if (Releases.Count > 0)
			return;

		try
		{
			var releases = await whatsNew.GetReleasesAsync();
			for (var i = 0; i < releases.Count; i++)
				Releases.Add(new ReleaseItem(releases[i], i == 0));
		}
		catch (Exception ex) when (ex is IOException or FileNotFoundException)
		{
			System.Diagnostics.Debug.WriteLine(ex);
		}
	}
}
