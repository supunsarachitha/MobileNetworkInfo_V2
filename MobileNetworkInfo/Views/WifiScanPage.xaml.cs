using MobileNetworkInfo.ViewModels;

namespace MobileNetworkInfo.Views;

public partial class WifiScanPage : BasePage
{
	public WifiScanPage(WifiScanViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
