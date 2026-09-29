using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AjuntamentSantaMariaMartorelles.Services;
using AjuntamentSantaMariaMartorelles.Views;

namespace AjuntamentSantaMariaMartorelles.ViewModels;

/// <summary>
/// Menú principal de la professora un cop identificada. És l'arrel de
/// navegació després del login: la fletxa "enrere" des d'aquí surt de l'app,
/// mai torna a la pantalla d'accés.
/// </summary>
public partial class MenuViewModel : BaseViewModel
{
	private readonly AuthService _authService;

	public MenuViewModel(AuthService authService)
	{
		_authService = authService;
	}

	[ObservableProperty]
	private string benvingudaText = "Menú";

	[ObservableProperty]
	private string cursetsTitol = "Els meus cursets";

	[ObservableProperty]
	private string cursetsDescripcio = "Consulta els cursets i passa llista d'assistència";

	[ObservableProperty]
	private bool esControlador;

	public void Actualitza()
	{
		BenvingudaText = string.IsNullOrEmpty(_authService.ProfessoraNom)
			? "Menú"
			: $"Hola, {_authService.ProfessoraNom}";

		EsControlador = _authService.EsControlador;
		if (EsControlador)
		{
			CursetsTitol = "Cursets que superviso";
			CursetsDescripcio = "Inscrites, pagaments i assistència (només lectura)";
		}
		else
		{
			CursetsTitol = "Els meus cursets";
			CursetsDescripcio = "Consulta els cursets i passa llista d'assistència";
		}
	}

	[RelayCommand]
	private Task ElsMeusCursetsAsync()
		=> Shell.Current.GoToAsync(nameof(CursetsListPage));

	[RelayCommand]
	private Task CobramentsAsync()
		=> Shell.Current.GoToAsync(nameof(CobramentsPage));

	[RelayCommand]
	private Task ElMeuCompteAsync()
		=> Shell.Current.GoToAsync(nameof(ComptePage));
}
