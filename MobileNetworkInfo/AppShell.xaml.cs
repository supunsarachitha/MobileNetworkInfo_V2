using MobileNetworkInfo.Views;

namespace MobileNetworkInfo;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(WifiScanPage), typeof(WifiScanPage));
		Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));
		Routing.RegisterRoute(nameof(WhatsNewPage), typeof(WhatsNewPage));
		Routing.RegisterRoute(nameof(SupportPage), typeof(SupportPage));
	}
}
