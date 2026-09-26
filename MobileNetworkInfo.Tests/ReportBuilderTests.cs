using MobileNetworkInfo.Helpers;

namespace MobileNetworkInfo.Tests;

public class ReportBuilderTests
{
	static readonly DateTimeOffset Generated = new(2026, 9, 26, 17, 15, 0, TimeSpan.FromHours(-4));

	[Fact]
	public void Build_IncludesHeaderAndSections()
	{
		var report = ReportBuilder.Build("Mobile Network Info", "5.0", Generated,
		[
			new("Device", [new("Model", "Pixel 9"), new("Manufacturer", "Google")]),
			new("Battery", [new("Level", "80%")]),
		]);

		var lines = report.Split(Environment.NewLine);
		Assert.Equal("Mobile Network Info report", lines[0]);
		Assert.Equal("Generated 2026-09-26 17:15 -04:00 · v5.0", lines[1]);
		Assert.Contains("DEVICE", lines);
		Assert.Contains("Model        : Pixel 9", lines);
		Assert.Contains("Manufacturer : Google", lines);
		Assert.Contains("BATTERY", lines);
		Assert.Contains("Level : 80%", lines);
	}

	[Fact]
	public void Build_SkipsEmptySections()
	{
		var report = ReportBuilder.Build("App", "1.0", Generated, [new("Wi-Fi", []), new("Device", [new("Model", "X")])]);
		Assert.DoesNotContain("WI-FI", report);
		Assert.Contains("DEVICE", report);
	}

	[Fact]
	public void Build_IndentsMultiLineValues()
	{
		var report = ReportBuilder.Build("App", "1.0", Generated,
			[new("IP", [new("DNS", $"8.8.8.8{Environment.NewLine}1.1.1.1")])]);
		var lines = report.Split(Environment.NewLine);
		Assert.Contains("DNS : 8.8.8.8", lines);
		Assert.Contains("      1.1.1.1", lines);
	}
}
