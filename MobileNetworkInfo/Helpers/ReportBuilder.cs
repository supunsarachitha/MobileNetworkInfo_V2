using System.Text;

namespace MobileNetworkInfo.Helpers;

public sealed record ReportSection(string Title, IReadOnlyList<InfoRow> Rows);

/// <summary>Builds the plain-text device report used by "Share report".</summary>
public static class ReportBuilder
{
	public static string Build(string appName, string appVersion, DateTimeOffset generatedAt, IEnumerable<ReportSection> sections)
	{
		var sb = new StringBuilder();
		sb.AppendLine($"{appName} report");
		sb.AppendLine($"Generated {generatedAt:yyyy-MM-dd HH:mm zzz} · v{appVersion}");

		foreach (var section in sections)
		{
			if (section.Rows.Count == 0)
				continue;

			sb.AppendLine();
			sb.AppendLine(section.Title.ToUpperInvariant());
			var width = section.Rows.Max(r => r.Label.Length);
			foreach (var row in section.Rows)
			{
				// Multi-line values are indented under their label
				var value = row.Value.ReplaceLineEndings(Environment.NewLine + new string(' ', width + 3));
				sb.Append(row.Label.PadRight(width)).Append(" : ").AppendLine(value);
			}
		}

		return sb.ToString().TrimEnd();
	}
}
