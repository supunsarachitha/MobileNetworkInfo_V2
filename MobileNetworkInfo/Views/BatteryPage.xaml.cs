using MobileNetworkInfo.ViewModels;

namespace MobileNetworkInfo.Views;

public partial class BatteryPage : BasePage
{
	public BatteryPage(BatteryViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
