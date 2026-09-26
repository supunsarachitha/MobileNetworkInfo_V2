using MobileNetworkInfo.Services;
using MobileNetworkInfo.ViewModels;
using Icons = MobileNetworkInfo.Resources.Icons;

namespace MobileNetworkInfo.Views;

/// <summary>
/// Forwards visibility to the view model (so live data only updates while visible and in the foreground)
/// and adds the shared "Share report" / "About" toolbar actions to the main tabs.
/// </summary>
public class BasePage : ContentPage
{
	bool isOnScreen;
	bool isActive;
	Window? window;

	public BasePage()
	{
		Loaded += (_, _) => AddToolbarIfNeeded();
	}

	/// <summary>Set to false on pushed pages that should not show the shared toolbar actions.</summary>
	public bool ShowStandardToolbar { get; set; } = true;

	BaseViewModel? ViewModel => BindingContext as BaseViewModel;

	protected override void OnAppearing()
	{
		base.OnAppearing();
		isOnScreen = true;
		AttachWindow();
		Activate();
		SystemBars.Refresh(Platform.CurrentActivity);
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		isOnScreen = false;
		DetachWindow();
		Deactivate();
	}

	void Activate()
	{
		if (isActive)
			return;
		isActive = true;
		ViewModel?.OnAppearing();
	}

	void Deactivate()
	{
		if (!isActive)
			return;
		isActive = false;
		ViewModel?.OnDisappearing();
	}

	void AttachWindow()
	{
		// Pages pushed onto the navigation stack aren't parented yet when OnAppearing runs
		var target = Window ?? Application.Current?.Windows.FirstOrDefault();
		if (target is null || target == window)
			return;
		DetachWindow();
		window = target;
		window.Deactivated += OnWindowDeactivated;
		window.Activated += OnWindowActivated;
	}

	void DetachWindow()
	{
		if (window is null)
			return;
		window.Deactivated -= OnWindowDeactivated;
		window.Activated -= OnWindowActivated;
		window = null;
	}

	// Deactivated/Activated follow the activity's pause/resume, so this also covers returning from
	// system screens (settings, permission dialogs) that only pause the app.
	void OnWindowDeactivated(object? sender, EventArgs e) => Deactivate();

	void OnWindowActivated(object? sender, EventArgs e)
	{
		if (isOnScreen)
			Activate();
	}

	void AddToolbarIfNeeded()
	{
		if (!ShowStandardToolbar || ToolbarItems.Count > 0)
			return;

		ToolbarItems.Add(CreateToolbarItem("Share report", Icons.Share, async () =>
		{
			var report = Handler?.MauiContext?.Services.GetService<ReportService>();
			if (report is not null)
				await report.ShareAsync();
		}));
		ToolbarItems.Add(CreateToolbarItem("About", Icons.Info, () => Shell.Current.GoToAsync(nameof(AboutPage))));
	}

	static ToolbarItem CreateToolbarItem(string text, string glyph, Func<Task> action)
	{
		var icon = new FontImageSource { FontFamily = Icons.FontFamily, Glyph = glyph, Size = 22 };
		icon.SetAppThemeColor(FontImageSource.ColorProperty, ThemeColor("TextPrimaryLight"), ThemeColor("TextPrimaryDark"));
		var item = new ToolbarItem { Text = text, IconImageSource = icon, Order = ToolbarItemOrder.Primary };
		item.Clicked += async (_, _) => await action();
		SemanticProperties.SetDescription(item, text);
		return item;
	}

	static Color ThemeColor(string key) =>
		Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color ? color : Colors.Gray;
}
