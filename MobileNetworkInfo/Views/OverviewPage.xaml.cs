using MobileNetworkInfo.ViewModels;

namespace MobileNetworkInfo.Views;

public partial class OverviewPage : BasePage
{
	public OverviewPage(OverviewViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
