using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;

namespace MobileNetworkInfo;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);
		SystemBars.Refresh(this);
	}

	public override void OnConfigurationChanged(Configuration newConfig)
	{
		base.OnConfigurationChanged(newConfig);
		// Run after MAUI has finished applying the theme change, which also touches the system bars
		Window?.DecorView.Post(() => SystemBars.Refresh(this));
	}
}
