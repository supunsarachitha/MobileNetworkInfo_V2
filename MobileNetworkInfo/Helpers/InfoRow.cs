namespace MobileNetworkInfo.Helpers;

/// <summary>A single label/value pair shown in an info card and in the shared report.</summary>
public readonly record struct InfoRow(string Label, string Value);

/// <summary>
/// Ordered list of rows that silently skips empty values, so unsupported or unavailable
/// properties simply don't appear. Supports collection initializers: <c>new RowList { { "Model", model } }</c>.
/// </summary>
public sealed class RowList : List<InfoRow>
{
	public void Add(string label, string? value)
	{
		if (!string.IsNullOrWhiteSpace(value))
			Add(new InfoRow(label, value.Trim()));
	}

	public void Add(string label, bool? value)
	{
		if (value is { } v)
			Add(new InfoRow(label, v ? "Yes" : "No"));
	}
}
