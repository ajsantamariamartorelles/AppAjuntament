using AjuntamentSantaMariaMartorelles.Services;

namespace AjuntamentSantaMariaMartorelles.Views;

/// <summary>
/// Pantalla d'entrada: continua visualment el splash natiu (escut sobre fons
/// clar), comprova si hi ha una sessió guardada vàlida i, segons el resultat,
/// salta al menú o a la pantalla de login. Així no es veu mai el login abans de
/// saber si l'usuari ja està identificat.
/// </summary>
public partial class LoadingPage : ContentPage
{
	private readonly AuthService _authService;
	private bool _fet;

	public LoadingPage(AuthService authService)
	{
		InitializeComponent();
		_authService = authService;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (_fet) return;
		_fet = true;

		// Animació d'entrada de l'escut + temps mínim en pantalla perquè no "parpellegi".
		var animacio = Task.WhenAll(
			Escut.FadeTo(1, 350, Easing.CubicOut),
			Escut.ScaleTo(1, 350, Easing.CubicOut),
			Peu.FadeTo(1, 350, Easing.CubicOut));

		var restauracio = SegurRestoreAsync();

		await Task.WhenAll(animacio, restauracio, Task.Delay(650));

		var autenticat = restauracio.Result;
		var desti = !autenticat ? "//login"
			: _authService.CalAcceptarTermes ? "//accepta-termes"
			: "//menu";
		await Shell.Current.GoToAsync(desti);
	}

	private async Task<bool> SegurRestoreAsync()
	{
		try
		{
			return await _authService.RestoreSessionAsync();
		}
		catch
		{
			return false;
		}
	}
}
