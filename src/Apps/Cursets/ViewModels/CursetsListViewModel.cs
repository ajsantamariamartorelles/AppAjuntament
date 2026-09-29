using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AjuntamentSantaMariaMartorelles.Models;
using AjuntamentSantaMariaMartorelles.Services;
using AjuntamentSantaMariaMartorelles.Views;

namespace AjuntamentSantaMariaMartorelles.ViewModels;

public partial class CursetsListViewModel : BaseViewModel
{
	private readonly CursetsApiService _apiService;
	private readonly AuthService _authService;

	public CursetsListViewModel(CursetsApiService apiService, AuthService authService)
	{
		_apiService = apiService;
		_authService = authService;
		// IsBusy viu a BaseViewModel: cal avisar de MostraCarregant quan canvia.
		PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(IsBusy))
				OnPropertyChanged(nameof(MostraCarregant));
		};
	}

	public ObservableCollection<CursetDto> Cursets { get; } = new();

	[ObservableProperty]
	private string benvingudaText = string.Empty;

	[ObservableProperty]
	private string subtitolText = "Els teus cursets";

	[ObservableProperty]
	private CursetDto? selectedCurset;

	// El "tira per refrescar" té la seva pròpia propietat: lligar-lo a IsBusy feia que a
	// Android el cercle es quedés girant per sempre si la càrrega acabava abans que la
	// vista estigués pintada.
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MostraCarregant))]
	private bool isRefreshing;

	/// <summary>Indicador de la càrrega inicial (no el del gest de refrescar).</summary>
	public bool MostraCarregant => IsBusy && !IsRefreshing;

	private DateTime? _ultimaCarregaUtc;

	public bool EsControlador => _authService.EsControlador;

	async partial void OnSelectedCursetChanged(CursetDto? value)
	{
		if (value is null) return;

		var curset = value;
		SelectedCurset = null; // neteja la selecció visual del CollectionView

		// Professora → passar llista (escriptura). Controlador → inscrites + històric (només lectura).
		var pagina = _authService.EsControlador ? nameof(CursetSeguimentPage) : nameof(SessioPage);
		await Shell.Current.GoToAsync(
			$"{pagina}?cursetId={curset.Id}&titol={Uri.EscapeDataString(curset.Titol)}");
	}

	/// <summary>En entrar a la pantalla: carrega només si no hi ha dades o són velles (2 min).</summary>
	public async Task CarregaSiCalAsync()
	{
		var velles = _ultimaCarregaUtc is null || DateTime.UtcNow - _ultimaCarregaUtc > TimeSpan.FromMinutes(2);
		if (Cursets.Count == 0 || velles)
			await CarregarAsync();
	}

	/// <summary>Gest de tirar per refrescar: sempre torna a carregar i, passi el que passi, atura el cercle.</summary>
	[RelayCommand]
	private async Task RefrescaAsync()
	{
		try
		{
			await CarregarAsync();
		}
		finally
		{
			IsRefreshing = false;
		}
	}

	[RelayCommand]
	private async Task CarregarAsync()
	{
		if (IsBusy) return;

		try
		{
			IsBusy = true;
			ErrorMessage = null;
			BenvingudaText = string.IsNullOrEmpty(_authService.ProfessoraNom)
				? "Cursets"
				: $"Hola, {_authService.ProfessoraNom}";
			SubtitolText = _authService.EsControlador ? "Cursets que superviso" : "Els teus cursets";

			var cursets = await _apiService.GetCursetsAsync();

			if (_authService.EsControlador)
			{
				// El resum (places, espera, pendents) és una ajuda: si falla, la llista es mostra igual.
				try
				{
					var resum = (await _apiService.GetResumSeguimentAsync()).ToDictionary(r => r.CursetId);
					foreach (var curset in cursets)
						curset.Seguiment = resum.GetValueOrDefault(curset.Id);
				}
				catch (Exception)
				{
					// Sense resum: les targetes surten sense etiquetes.
				}
			}

			Cursets.Clear();
			foreach (var curset in cursets)
				Cursets.Add(curset);
			_ultimaCarregaUtc = DateTime.UtcNow;

			if (Cursets.Count == 0)
				ErrorMessage = _authService.EsControlador
					? "Encara no tens cap curset assignat com a regidor responsable."
					: "Encara no tens cap curset assignat.";
		}
		catch (Exception ex)
		{
			ErrorMessage = $"No s'han pogut carregar els cursets: {ex.Message}";
		}
		finally
		{
			IsBusy = false;
		}
	}

}
