using MobileNetworkInfo.ViewModels;

namespace MobileNetworkInfo.Views;

public partial class SupportPage : BasePage
{
	public SupportPage(SupportViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
