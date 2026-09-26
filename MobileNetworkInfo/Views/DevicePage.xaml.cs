using MobileNetworkInfo.ViewModels;

namespace MobileNetworkInfo.Views;

public partial class DevicePage : BasePage
{
	public DevicePage(DeviceViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
