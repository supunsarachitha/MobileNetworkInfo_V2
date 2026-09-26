using Android.App;
using Android.Content.Res;
using Android.Views;
using AndroidX.Core.Content;
using AndroidX.Core.View;
using Google.Android.Material.AppBar;

namespace MobileNetworkInfo;

/// <summary>
/// Keeps the status/navigation bar icons and the app bar background (which sits behind the
/// status bar when edge-to-edge) in sync with the current light/dark theme.
/// </summary>
public static class SystemBars
{
	public static void Refresh(Activity? activity)
	{
		if (activity?.Window is not { } window)
			return;

		var isDark = (activity.Resources?.Configuration?.UiMode & UiMode.NightMask) == UiMode.NightYes;
		if (WindowCompat.GetInsetsController(window, window.DecorView) is { } controller)
		{
			controller.AppearanceLightStatusBars = !isDark;
			controller.AppearanceLightNavigationBars = !isDark;
		}

		// Before edge-to-edge (Android 15) the bars have their own background. Dark navigation
		// buttons need a light bar, which is only supported from Android 8.
		if (OperatingSystem.IsAndroidVersionAtLeast(26) && !OperatingSystem.IsAndroidVersionAtLeast(35))
		{
			window.SetNavigationBarColor(new Android.Graphics.Color(ContextCompat.GetColor(activity, Resource.Color.nav_bar_background)));
			window.SetStatusBarColor(new Android.Graphics.Color(ContextCompat.GetColor(activity, Resource.Color.app_bar_background)));
		}

		// The activity handles uiMode changes itself, so views inflated before a theme switch
		// keep their old background; recolor the app bars explicitly.
		var color = new Android.Graphics.Color(ContextCompat.GetColor(activity, Resource.Color.app_bar_background));
		Recolor(window.DecorView, color);
	}

	static void Recolor(Android.Views.View? view, Android.Graphics.Color color)
	{
		if (view is AppBarLayout appBar)
		{
			appBar.SetBackgroundColor(color);
			return;
		}

		if (view is ViewGroup group)
		{
			for (var i = 0; i < group.ChildCount; i++)
				Recolor(group.GetChildAt(i), color);
		}
	}
}
