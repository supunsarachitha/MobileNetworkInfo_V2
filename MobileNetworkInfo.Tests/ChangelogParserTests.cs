using MobileNetworkInfo.Helpers;

namespace MobileNetworkInfo.Tests;

public class ChangelogParserTests
{
	const string Sample = """
		# What's new

		Intro text that is ignored.

		## Version 2.0 · May 2026

		### New
		- First feature.
		- Second feature that is
		  wrapped onto two lines.

		### Fixed
		* A bug.

		## Version 1.0 · January 2026
		- Initial release.
		""";

	[Fact]
	public void Parse_ReadsReleasesSectionsAndItems()
	{
		var releases = ChangelogParser.Parse(Sample);

		Assert.Equal(2, releases.Count);
		var latest = releases[0];
		Assert.Equal("Version 2.0 · May 2026", latest.Title);
		Assert.Equal(["New", "Fixed"], latest.Sections.Select(s => s.Title));
		Assert.Equal(["First feature.", "Second feature that is wrapped onto two lines."], latest.Sections[0].Items);
		Assert.Equal(["A bug."], latest.Sections[1].Items);
	}

	[Fact]
	public void Parse_ItemsWithoutSectionGetEmptyTitle()
	{
		var release = ChangelogParser.Parse(Sample)[1];
		Assert.Equal("", release.Sections[0].Title);
		Assert.Equal(["Initial release."], release.Sections[0].Items);
	}

	[Fact]
	public void Parse_IgnoresEmptyReleases() =>
		Assert.Empty(ChangelogParser.Parse("# Title\n\n## Version 1.0\n\nNothing here yet."));

	[Fact]
	public void RepositoryChangelog_IsValid()
	{
		// The same file is bundled into the app; make sure it parses and the latest release is first.
		var path = Path.Combine(AppContext.BaseDirectory, "CHANGELOG.md");
		var releases = ChangelogParser.Parse(File.ReadAllText(path));

		Assert.NotEmpty(releases);
		Assert.StartsWith("Version ", releases[0].Title);
		Assert.All(releases, r => Assert.All(r.Sections, s => Assert.Contains(s.Title, new[] { "New", "Improved", "Fixed", "Removed" })));
	}
}
