using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AjuntamentSantaMariaMartorelles.Models;
using AjuntamentSantaMariaMartorelles.Services;
using AjuntamentSantaMariaMartorelles.Views;

namespace AjuntamentSantaMariaMartorelles.ViewModels;

/// <summary>
/// Resum global per al controlador (regidor): qui té liquidacions pendents en
/// algun dels cursets que supervisa, agrupat per curset. Només lectura.
/// </summary>
public partial class CobramentsViewModel : BaseViewModel
{
	private readonly CursetsApiService _apiService;

	public CobramentsViewModel(CursetsApiService apiService)
	{
		_apiService = apiService;
		PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(IsBusy))
				OnPropertyChanged(nameof(MostraCarregant));
		};
	}

	[ObservableProperty]
	private ResumImportsDto? total;

	[ObservableProperty]
	private string subtitolText = string.Empty;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MostraCarregant))]
	private bool isRefreshing;

	public bool MostraCarregant => IsBusy && !IsRefreshing;

	public ObservableCollection<GrupCobraments> Grups { get; } = new();

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
	private Task ObrePersonaAsync(PersonaInscritaDto persona)
		=> Shell.Current.GoToAsync($"{nameof(PersonaPage)}?cursetId={persona.CursetId}&alumneId={persona.AlumneId}");

	public async Task CarregarAsync()
	{
		if (IsBusy) return;

		try
		{
			IsBusy = true;
			ErrorMessage = null;

			var cobraments = await _apiService.GetCobramentsAsync();
			Total = cobraments.Total;

			var persones = cobraments.Cursets.Sum(c => c.Pendents.Count);
			var cursets = cobraments.Cursets.Count == 1 ? "1 curset" : $"{cobraments.Cursets.Count} cursets";
			SubtitolText = cobraments.Cursets.Count == 0
				? "No superviseu cap curset actiu."
				: persones == 0
					? $"{cursets} · cap pagament pendent"
					: $"{cursets} · {persones} {(persones == 1 ? "persona" : "persones")} amb pagaments pendents";

			Grups.Clear();
			foreach (var curset in cobraments.Cursets)
			{
				foreach (var p in curset.Pendents)
					p.CursetId = curset.CursetId;
				Grups.Add(new GrupCobraments(curset));
			}
		}
		catch (Exception ex)
		{
			ErrorMessage = $"No s'han pogut carregar els cobraments: {ex.Message}";
		}
		finally
		{
			IsBusy = false;
		}
	}
}

public class GrupCobraments : List<PersonaInscritaDto>
{
	public GrupCobraments(CobramentsCursetDto curset) : base(curset.Pendents)
	{
		Titol = curset.Titol;
		PendentText = curset.Resum.Pendent > 0 ? Format.Euros(curset.Resum.Pendent) : string.Empty;
		MissatgeBuit = curset.Pendents.Count > 0
			? null
			: curset.Resum.NumLiquidacions > 0 ? "Tot cobrat" : "Encara no s'ha emès cap liquidació";
	}

	public string Titol { get; }
	public string PendentText { get; }
	public string? MissatgeBuit { get; }
	public bool TeMissatge => MissatgeBuit is not null;
}
