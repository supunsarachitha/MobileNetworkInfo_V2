using MobileNetworkInfo.ViewModels;

namespace MobileNetworkInfo.Views;

public partial class SensorsPage : BasePage
{
	public SensorsPage(SensorsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
