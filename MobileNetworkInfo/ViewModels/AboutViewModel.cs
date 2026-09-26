using CommunityToolkit.Mvvm.Input;
using MobileNetworkInfo.Services;
using MobileNetworkInfo.Views;

namespace MobileNetworkInfo.ViewModels;

public partial class AboutViewModel : BaseViewModel
{
	public string AppName => AppInfo.Current.Name;
	public string Version => $"Version {AppInfo.Current.VersionString} (build {AppInfo.Current.BuildString})";

	[RelayCommand]
	Task OpenSupport() => Shell.Current.GoToAsync(nameof(SupportPage));

	[RelayCommand]
	Task OpenWhatsNew() => Shell.Current.GoToAsync(nameof(WhatsNewPage));

	[RelayCommand]
	Task Rate() => AppLinks.OpenStoreAsync();

	[RelayCommand]
	Task ShareApp() => AppLinks.ShareAppAsync();

	[RelayCommand]
	Task OpenPrivacyPolicy() => AppLinks.OpenAsync(AppLinks.PrivacyPolicy);

	[RelayCommand]
	Task OpenSourceCode() => AppLinks.OpenAsync(AppLinks.Repository);

	[RelayCommand]
	Task OpenLicense() => AppLinks.OpenAsync(AppLinks.License);

	[RelayCommand]
	void OpenAppSettings() => AppInfo.Current.ShowSettingsUI();
}
