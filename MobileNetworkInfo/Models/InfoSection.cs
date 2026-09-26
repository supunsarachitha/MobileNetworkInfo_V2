using System.Collections.ObjectModel;
using MobileNetworkInfo.Helpers;

namespace MobileNetworkInfo.Models;

/// <summary>Rows of an info card, kept in sync with freshly read data.</summary>
public class InfoSection : ObservableCollection<InfoItem>
{
	/// <summary>
	/// Updates values in place when the set of labels is unchanged (the common case for live data),
	/// otherwise rebuilds the list so the row order always matches <paramref name="rows"/>.
	/// </summary>
	public void Update(IReadOnlyList<InfoRow> rows)
	{
		if (Count == rows.Count && this.Select(i => i.Label).SequenceEqual(rows.Select(r => r.Label)))
		{
			for (var i = 0; i < rows.Count; i++)
				this[i].Value = rows[i].Value;
			return;
		}

		ClearItems();
		foreach (var row in rows)
			Add(new InfoItem(row.Label, row.Value));
	}
}
