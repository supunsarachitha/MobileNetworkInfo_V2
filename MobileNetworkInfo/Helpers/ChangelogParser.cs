namespace MobileNetworkInfo.Helpers;

public sealed record ChangelogSection(string Title, IReadOnlyList<string> Items);

public sealed record ChangelogRelease(string Title, IReadOnlyList<ChangelogSection> Sections);

/// <summary>
/// Parses CHANGELOG.md: "## " starts a release, "### " a section (New, Improved, Fixed, Removed)
/// and "- " a bullet. Everything else (title, intro text) is ignored.
/// </summary>
public static class ChangelogParser
{
	public static IReadOnlyList<ChangelogRelease> Parse(string markdown)
	{
		var releases = new List<ChangelogRelease>();
		string? releaseTitle = null;
		List<ChangelogSection> sections = [];
		string? sectionTitle = null;
		List<string> items = [];

		void FlushSection()
		{
			if (items.Count > 0)
				sections.Add(new ChangelogSection(sectionTitle ?? "", items));
			items = [];
		}

		void FlushRelease()
		{
			FlushSection();
			if (releaseTitle is not null && sections.Count > 0)
				releases.Add(new ChangelogRelease(releaseTitle, sections));
			sections = [];
		}

		foreach (var raw in markdown.ReplaceLineEndings("\n").Split('\n'))
		{
			var line = raw.TrimEnd();
			if (line.StartsWith("## ", StringComparison.Ordinal))
			{
				FlushRelease();
				releaseTitle = line[3..].Trim();
				sectionTitle = null;
			}
			else if (line.StartsWith("### ", StringComparison.Ordinal))
			{
				FlushSection();
				sectionTitle = line[4..].Trim();
			}
			else if (releaseTitle is not null && (line.StartsWith("- ", StringComparison.Ordinal) || line.StartsWith("* ", StringComparison.Ordinal)))
			{
				items.Add(line[2..].Trim());
			}
			else if (items.Count > 0 && line.Length > 0 && char.IsWhiteSpace(raw[0]))
			{
				// Wrapped continuation of the previous bullet
				items[^1] += " " + line.Trim();
			}
		}

		FlushRelease();
		return releases;
	}
}
