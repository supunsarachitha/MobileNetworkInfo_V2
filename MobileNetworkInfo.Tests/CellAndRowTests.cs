using MobileNetworkInfo.Helpers;

namespace MobileNetworkInfo.Tests;

public class CellAndRowTests
{
	[Fact]
	public void SplitLteCellId_SeparatesENodeBAndSector() =>
		Assert.Equal((184, 4), CellMath.SplitLteCellId(47108));

	[Theory]
	[InlineData(int.MaxValue, false)]
	[InlineData(int.MinValue, false)]
	[InlineData(-95, true)]
	[InlineData(0, true)]
	public void IsValid_RejectsUnavailableMarker(int value, bool expected) =>
		Assert.Equal(expected, CellMath.IsValid(value));

	[Fact]
	public void WithUnit_ReturnsNullForUnavailable()
	{
		Assert.Equal("-95 dBm", CellMath.WithUnit(-95, "dBm"));
		Assert.Null(CellMath.WithUnit(int.MaxValue, "dBm"));
	}

	[Fact]
	public void RowList_SkipsEmptyValues()
	{
		var rows = new RowList
		{
			{ "Model", "Pixel 9" },
			{ "Empty", "" },
			{ "Whitespace", "   " },
			{ "Missing", (string?)null },
			{ "Roaming", false },
			{ "Unknown flag", (bool?)null },
		};

		Assert.Equal([new InfoRow("Model", "Pixel 9"), new InfoRow("Roaming", "No")], rows);
	}

	[Fact]
	public void RowList_TrimsValues()
	{
		var rows = new RowList { { "Kernel", "  6.12  " } };
		Assert.Equal("6.12", rows[0].Value);
	}
}
