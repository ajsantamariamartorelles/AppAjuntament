using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AjuntamentSantaMariaMartorelles.Services;

namespace AjuntamentSantaMariaMartorelles.ViewModels;

public partial class AcceptaTermesViewModel : BaseViewModel
{
	private readonly AuthService _authService;

	public AcceptaTermesViewModel(AuthService authService)
	{
		_authService = authService;
	}

	[ObservableProperty]
	private bool accepta;

	[RelayCommand]
	private async Task ObreTermesAsync()
		=> await Browser.Default.OpenAsync(
			new Uri(new Uri(Config.ApiConfig.BaseUrl), "terms-and-conditions"),
			BrowserLaunchMode.SystemPreferred);

	[RelayCommand]
	private async Task ObrePrivacitatAsync()
		=> await Browser.Default.OpenAsync(
			new Uri(new Uri(Config.ApiConfig.BaseUrl), "data-privacy"),
			BrowserLaunchMode.SystemPreferred);

	[RelayCommand]
	private async Task ContinuarAsync()
	{
		if (IsBusy) return;

		ErrorMessage = null;
		if (!Accepta)
		{
			ErrorMessage = "Cal marcar la casella per continuar.";
			return;
		}

		try
		{
			IsBusy = true;
			var ok = await _authService.AcceptaTermesAsync();
			if (!ok)
			{
				ErrorMessage = "No s'ha pogut registrar l'acceptació. Comprova la connexió i torna-ho a provar.";
				return;
			}

			await Shell.Current.GoToAsync("//menu");
		}
		finally
		{
			IsBusy = false;
		}
	}
}
