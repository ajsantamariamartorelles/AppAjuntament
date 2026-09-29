using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AjuntamentSantaMariaMartorelles.Models;
using AjuntamentSantaMariaMartorelles.Services;

namespace AjuntamentSantaMariaMartorelles.ViewModels;

/// <summary>
/// Pantalla principal: "Iniciar classe" obre (o recupera) la sessió d'avui
/// amb totes les alumnes marcades presents; la professora desmarca les que
/// no han vingut i desa. Es factura per classe impartida, no per aquesta
/// assistència personal — aquí només es registra qui hi era.
/// </summary>
[QueryProperty(nameof(CursetId), "cursetId")]
[QueryProperty(nameof(Titol), "titol")]
public partial class SessioViewModel : BaseViewModel
{
	private readonly CursetsApiService _apiService;
	private int _sessioId;

	public SessioViewModel(CursetsApiService apiService)
	{
		_apiService = apiService;
	}

	[ObservableProperty]
	private int cursetId;

	[ObservableProperty]
	private string titol = string.Empty;

	[ObservableProperty]
	private DateTime data = DateTime.Today;

	[ObservableProperty]
	private bool desada;

	public ObservableCollection<AlumnaAssistenciaViewModel> Alumnes { get; } = new();

	async partial void OnCursetIdChanged(int value)
	{
		await ObrirSessioAsync();
	}

	private async Task ObrirSessioAsync()
	{
		if (IsBusy || CursetId <= 0) return;

		try
		{
			IsBusy = true;
			ErrorMessage = null;
			Desada = false;

			var sessio = await _apiService.ObrirSessioAsync(CursetId);
			_sessioId = sessio.SessioId;
			Data = sessio.Data;

			Alumnes.Clear();
			foreach (var alumna in sessio.Alumnes.OrderBy(a => a.NomComplet))
				Alumnes.Add(new AlumnaAssistenciaViewModel(alumna));

			if (Alumnes.Count == 0)
				ErrorMessage = "Aquest curset no té alumnes actives.";
		}
		catch (Exception ex)
		{
			ErrorMessage = $"No s'ha pogut iniciar la classe: {ex.Message}";
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	private async Task GuardarAsync()
	{
		if (IsBusy || _sessioId == 0) return;

		try
		{
			IsBusy = true;
			ErrorMessage = null;

			var request = new GuardarAssistenciaRequest
			{
				Alumnes = Alumnes.Select(a => a.ToDto()).ToList()
			};

			await _apiService.TancarSessioAsync(_sessioId, request);
			Desada = true;
		}
		catch (Exception ex)
		{
			ErrorMessage = $"No s'ha pogut desar l'assistència: {ex.Message}";
		}
		finally
		{
			IsBusy = false;
		}
	}
}
