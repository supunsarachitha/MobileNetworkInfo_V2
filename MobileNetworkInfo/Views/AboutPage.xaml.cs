using MobileNetworkInfo.ViewModels;

namespace MobileNetworkInfo.Views;

public partial class AboutPage : BasePage
{
	public AboutPage(AboutViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
