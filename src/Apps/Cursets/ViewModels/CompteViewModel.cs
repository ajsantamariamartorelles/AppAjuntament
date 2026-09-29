using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AjuntamentSantaMariaMartorelles.Services;

namespace AjuntamentSantaMariaMartorelles.ViewModels;

/// <summary>
/// "El meu compte": dades de la persona identificada i accions de la sessió.
/// Pensat per créixer (canvi d'idioma, notificacions, altres mòduls…).
/// </summary>
public partial class CompteViewModel : BaseViewModel
{
	private readonly AuthService _authService;

	public CompteViewModel(AuthService authService)
	{
		_authService = authService;
	}

	[ObservableProperty]
	private string nom = string.Empty;

	[ObservableProperty]
	private string rolText = string.Empty;

	[ObservableProperty]
	private bool teContrasenya;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MostraBotoCanviPassword))]
	private bool formPasswordObert;

	[ObservableProperty]
	private string passwordActual = string.Empty;

	[ObservableProperty]
	private string passwordNova = string.Empty;

	[ObservableProperty]
	private string passwordConfirma = string.Empty;

	public const string RequisitsPassword =
		"Mínim 8 caràcters, amb majúscules, minúscules, números i signes (per exemple ! ? # $ %).";

	private static bool EsPasswordForta(string p) =>
		p.Length >= 8 && p.Any(char.IsLower) && p.Any(char.IsUpper) && p.Any(char.IsDigit)
		&& p.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

	public bool MostraBotoCanviPassword => TeContrasenya && !FormPasswordObert;

	partial void OnTeContrasenyaChanged(bool value) => OnPropertyChanged(nameof(MostraBotoCanviPassword));

	public void Actualitza()
	{
		TeContrasenya = _authService.TeContrasenya;
		Nom = string.IsNullOrWhiteSpace(_authService.ProfessoraNom) ? "—" : _authService.ProfessoraNom!;
		RolText = _authService.EsControlador ? "Regidor/a — supervisió (només lectura)" : "Professor/a";
	}

	[RelayCommand]
	private void ObreCanviPassword()
	{
		NetejaFormPassword();
		FormPasswordObert = true;
	}

	[RelayCommand]
	private void CancelaCanviPassword()
	{
		NetejaFormPassword();
		FormPasswordObert = false;
	}

	[RelayCommand]
	private async Task CanviaPasswordAsync()
	{
		if (IsBusy) return;
		ErrorMessage = null;

		if (string.IsNullOrEmpty(PasswordActual) || string.IsNullOrEmpty(PasswordNova))
		{
			ErrorMessage = "Cal introduir la contrasenya actual i la nova.";
			return;
		}
		if (!EsPasswordForta(PasswordNova))
		{
			ErrorMessage = RequisitsPassword;
			return;
		}
		if (PasswordNova != PasswordConfirma)
		{
			ErrorMessage = "Les dues contrasenyes noves no coincideixen.";
			return;
		}

		try
		{
			IsBusy = true;
			var error = await _authService.CanviaPasswordAsync(PasswordActual, PasswordNova);
			if (error is not null)
			{
				ErrorMessage = error;
				return;
			}

			NetejaFormPassword();
			FormPasswordObert = false;
			await Shell.Current.DisplayAlertAsync("Contrasenya canviada",
				"La contrasenya s'ha canviat correctament.", "D'acord");
		}
		finally
		{
			IsBusy = false;
		}
	}

	private void NetejaFormPassword()
	{
		PasswordActual = string.Empty;
		PasswordNova = string.Empty;
		PasswordConfirma = string.Empty;
		ErrorMessage = null;
	}

	[RelayCommand]
	private async Task TancaSessioAsync()
	{
		var confirma = await Shell.Current.DisplayAlertAsync(
			"Tancar sessió", "Segur que vols tancar la sessió?", "Tanca la sessió", "Cancel·la");
		if (!confirma) return;

		_authService.Logout();
		await Shell.Current.GoToAsync("//login");
	}
}
