using MobileNetworkInfo.ViewModels;

namespace MobileNetworkInfo.Views;

public partial class WhatsNewPage : BasePage
{
	public WhatsNewPage(WhatsNewViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
