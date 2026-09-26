using CommunityToolkit.Mvvm.Input;
using MobileNetworkInfo.Services;

namespace MobileNetworkInfo.ViewModels;

public partial class SupportViewModel : BaseViewModel
{
	[RelayCommand]
	Task Rate() => AppLinks.OpenStoreAsync();

	[RelayCommand]
	Task ShareApp() => AppLinks.ShareAppAsync();

	[RelayCommand]
	Task ReportProblem() => AppLinks.OpenAsync(AppLinks.Issues);

	[RelayCommand]
	Task OpenSourceCode() => AppLinks.OpenAsync(AppLinks.Repository);
}
