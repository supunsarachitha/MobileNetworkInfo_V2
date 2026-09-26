using MobileNetworkInfo.ViewModels;

namespace MobileNetworkInfo.Views;

public partial class NetworkPage : BasePage
{
	public NetworkPage(NetworkViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
