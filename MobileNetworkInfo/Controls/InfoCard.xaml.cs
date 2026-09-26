using System.Collections;

namespace MobileNetworkInfo.Controls;

/// <summary>A card with an icon, title, optional subtitle, a list of label/value rows and optional extra content.</summary>
public partial class InfoCard : ContentView
{
	public static readonly BindableProperty TitleProperty =
		BindableProperty.Create(nameof(Title), typeof(string), typeof(InfoCard), string.Empty);

	public static readonly BindableProperty SubtitleProperty =
		BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(InfoCard), null,
			propertyChanged: (b, _, _) => ((InfoCard)b).OnPropertyChanged(nameof(HasSubtitle)));

	public static readonly BindableProperty GlyphProperty =
		BindableProperty.Create(nameof(Glyph), typeof(string), typeof(InfoCard), string.Empty);

	public static readonly BindableProperty ItemsProperty =
		BindableProperty.Create(nameof(Items), typeof(IEnumerable), typeof(InfoCard));

	public InfoCard()
	{
		InitializeComponent();
	}

	public string Title
	{
		get => (string)GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	public string? Subtitle
	{
		get => (string?)GetValue(SubtitleProperty);
		set => SetValue(SubtitleProperty, value);
	}

	public bool HasSubtitle => !string.IsNullOrEmpty(Subtitle);

	public string Glyph
	{
		get => (string)GetValue(GlyphProperty);
		set => SetValue(GlyphProperty, value);
	}

	public IEnumerable? Items
	{
		get => (IEnumerable?)GetValue(ItemsProperty);
		set => SetValue(ItemsProperty, value);
	}
}
