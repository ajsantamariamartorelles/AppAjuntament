using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AjuntamentSantaMariaMartorelles.Services;
using AjuntamentSantaMariaMartorelles.Views;

namespace AjuntamentSantaMariaMartorelles.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
	private readonly AuthService _authService;

	public LoginViewModel(AuthService authService)
	{
		_authService = authService;
	}

	[ObservableProperty]
	private string email = string.Empty;

	[ObservableProperty]
	private string password = string.Empty;

	[RelayCommand]
	private async Task LoginAsync()
	{
		if (IsBusy) return;

		ErrorMessage = null;

		if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
		{
			ErrorMessage = "Cal introduir el correu i la contrasenya.";
			return;
		}

		try
		{
			IsBusy = true;
			var ok = await _authService.LoginAsync(Email.Trim(), Password);
			if (!ok)
			{
				ErrorMessage = "Usuari o contrasenya incorrectes.";
				return;
			}

			Password = string.Empty;
			// Ruta absoluta: reinicia la pila, el login queda fora de l'històric.
			await Shell.Current.GoToAsync(DestiPostLogin());
		}
		catch (Exception ex)
		{
			ErrorMessage = $"No s'ha pogut connectar amb el servidor: {ex.Message}";
		}
		finally
		{
			IsBusy = false;
		}
	}

	/// <summary>Destí després d'un login correcte: la pantalla d'acceptació de
	/// termes si encara no s'ha acceptat la versió vigent, o el menú.</summary>
	private string DestiPostLogin() => _authService.CalAcceptarTermes ? "//accepta-termes" : "//menu";

	[RelayCommand]
	private async Task LoginMicrosoftAsync()
	{
		if (IsBusy) return;

		ErrorMessage = null;

		try
		{
			IsBusy = true;
			var ok = await _authService.LoginMicrosoftAsync();
			if (!ok)
			{
				ErrorMessage = _authService.UltimErrorMicrosoft
					?? "No s'ha pogut entrar amb el compte de l'Ajuntament. "
					+ "Comprova que ets professor/a o regidor/a del mandat actual.";
				return;
			}

			Password = string.Empty;
			await Shell.Current.GoToAsync(DestiPostLogin());
		}
		catch (Exception ex)
		{
			ErrorMessage = $"No s'ha pogut connectar amb el servidor: {ex.Message}";
		}
		finally
		{
			IsBusy = false;
		}
	}
}
