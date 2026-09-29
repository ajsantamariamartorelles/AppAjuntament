using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AjuntamentSantaMariaMartorelles.Models;
using AjuntamentSantaMariaMartorelles.Services;
using AjuntamentSantaMariaMartorelles.Views;

namespace AjuntamentSantaMariaMartorelles.ViewModels;

/// <summary>
/// Curset vist pel controlador (regidor), només lectura. Dues pestanyes:
/// "Inscrites" (persones i situació de les liquidacions) i "Sessions" (històric
/// d'assistència, que es carrega la primera vegada que s'obre la pestanya).
/// </summary>
public partial class CursetSeguimentViewModel : BaseViewModel, IQueryAttributable
{
	private readonly CursetsApiService _apiService;
	private List<PersonaInscritaDto> _persones = new();
	private bool _sessionsCarregades;

	public CursetSeguimentViewModel(CursetsApiService apiService)
	{
		_apiService = apiService;
		Filtres = new ObservableCollection<FiltreChip>
		{
			new(FiltreTotes, "Totes"),
			new(FiltrePendent, "Amb pendent"),
			new(FiltreAlCorrent, "Al corrent"),
			new(FiltreEspera, "En espera"),
			new(FiltreBaixes, "Baixes"),
		};
		Filtres[0].Seleccionat = true;

		PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(IsBusy))
				OnPropertyChanged(nameof(MostraCarregant));
		};
	}

	private const string FiltreTotes = "totes";
	private const string FiltrePendent = "pendent";
	private const string FiltreAlCorrent = "corrent";
	private const string FiltreEspera = "espera";
	private const string FiltreBaixes = "baixes";

	[ObservableProperty]
	private int cursetId;

	[ObservableProperty]
	private string titol = string.Empty;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(PestanyaSessions))]
	private bool pestanyaInscrites = true;

	public bool PestanyaSessions => !PestanyaInscrites;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(MostraCarregant))]
	private bool isRefreshing;

	/// <summary>Indicador de la càrrega inicial (no el del gest de refrescar).</summary>
	public bool MostraCarregant => IsBusy && !IsRefreshing;

	[ObservableProperty]
	private ResumImportsDto? resum;

	[ObservableProperty]
	private string placesText = string.Empty;

	[ObservableProperty]
	private string? missatgeBuit;

	public ObservableCollection<FiltreChip> Filtres { get; }
	public ObservableCollection<GrupPersones> Grups { get; } = new();
	public ObservableCollection<HistoricSessioViewModel> Sessions { get; } = new();

	private string FiltreActiu => Filtres.FirstOrDefault(f => f.Seleccionat)?.Clau ?? FiltreTotes;

	public async void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetValue("titol", out var t))
			Titol = Uri.UnescapeDataString(t?.ToString() ?? string.Empty);
		if (query.TryGetValue("cursetId", out var id) && int.TryParse(id?.ToString(), out var cursetId))
		{
			CursetId = cursetId;
			await CarregarInscritesAsync();
		}
	}

	[RelayCommand]
	private async Task SeleccionaPestanyaAsync(string pestanya)
	{
		PestanyaInscrites = pestanya == "inscrites";
		ErrorMessage = null;
		if (PestanyaSessions && !_sessionsCarregades)
			await CarregarSessionsAsync();
	}

	[RelayCommand]
	private async Task RefrescaAsync()
	{
		try
		{
			if (PestanyaInscrites)
				await CarregarInscritesAsync();
			else
				await CarregarSessionsAsync();
		}
		finally
		{
			IsRefreshing = false;
		}
	}

	[RelayCommand]
	private void Filtra(FiltreChip chip)
	{
		foreach (var f in Filtres)
			f.Seleccionat = f == chip;
		Agrupa();
	}

	[RelayCommand]
	private Task ObrePersonaAsync(PersonaInscritaDto persona)
		=> Shell.Current.GoToAsync($"{nameof(PersonaPage)}?cursetId={CursetId}&alumneId={persona.AlumneId}");

	private async Task CarregarInscritesAsync()
	{
		if (IsBusy || CursetId <= 0) return;

		try
		{
			IsBusy = true;
			ErrorMessage = null;

			var inscrites = await _apiService.GetInscritesAsync(CursetId);
			_persones = inscrites.Persones;
			Resum = inscrites.Resum;

			var admeses = _persones.Count(p => p.EsAdmesa);
			PlacesText = inscrites.MaxPlaces is int max
				? $"{admeses} de {max} places ocupades"
				: $"{admeses} persones amb plaça";

			ActualitzaComptadors();
			Agrupa();
		}
		catch (Exception ex)
		{
			ErrorMessage = $"No s'han pogut carregar les inscrites: {ex.Message}";
		}
		finally
		{
			IsBusy = false;
		}
	}

	private async Task CarregarSessionsAsync()
	{
		if (IsBusy || CursetId <= 0) return;

		try
		{
			IsBusy = true;
			ErrorMessage = null;

			var historic = await _apiService.GetHistoricAsync(CursetId);

			Sessions.Clear();
			foreach (var s in historic.OrderByDescending(s => s.Data))
				Sessions.Add(new HistoricSessioViewModel(s));
			_sessionsCarregades = true;
		}
		catch (Exception ex)
		{
			ErrorMessage = $"No s'ha pogut carregar l'històric: {ex.Message}";
		}
		finally
		{
			IsBusy = false;
		}
	}

	private bool Passa(PersonaInscritaDto p, string filtre) => filtre switch
	{
		FiltrePendent => p.TePendent,
		FiltreAlCorrent => p.Situacio == "AlCorrent",
		FiltreEspera => p.EsEspera || p.EsSollicitada,
		FiltreBaixes => p.EsBaixa,
		_ => true
	};

	private void ActualitzaComptadors()
	{
		foreach (var f in Filtres)
			f.Comptador = _persones.Count(p => Passa(p, f.Clau));
	}

	private void Agrupa()
	{
		var filtre = FiltreActiu;
		var visibles = _persones.Where(p => Passa(p, filtre)).ToList();

		Grups.Clear();
		AfegeixGrup("Amb plaça", visibles.Where(p => p.EsAdmesa));
		AfegeixGrup("Llista d'espera", visibles.Where(p => p.EsEspera));
		AfegeixGrup("Pendents de sorteig", visibles.Where(p => p.EsSollicitada));
		AfegeixGrup("Baixes", visibles.Where(p => p.EsBaixa));

		MissatgeBuit = _persones.Count == 0
			? "Aquest curset encara no té cap persona inscrita."
			: visibles.Count == 0 ? "Cap persona compleix aquest filtre." : null;
	}

	private void AfegeixGrup(string titol, IEnumerable<PersonaInscritaDto> persones)
	{
		var llista = persones.ToList();
		if (llista.Count > 0)
			Grups.Add(new GrupPersones($"{titol} · {llista.Count}", llista));
	}
}

public class GrupPersones : List<PersonaInscritaDto>
{
	public GrupPersones(string titol, IEnumerable<PersonaInscritaDto> persones) : base(persones)
	{
		Titol = titol;
	}

	public string Titol { get; }
}

public partial class FiltreChip : ObservableObject
{
	public FiltreChip(string clau, string text)
	{
		Clau = clau;
		Text = text;
	}

	public string Clau { get; }
	public string Text { get; }

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(Etiqueta))]
	private int comptador;

	[ObservableProperty]
	private bool seleccionat;

	public string Etiqueta => $"{Text} ({Comptador})";
}

/// <summary>Una sessió de l'històric (pestanya "Sessions"), plegable per veure qui hi va venir.</summary>
public partial class HistoricSessioViewModel : ObservableObject
{
	private readonly SessioObertaDto _dto;

	public HistoricSessioViewModel(SessioObertaDto dto)
	{
		_dto = dto;
	}

	[ObservableProperty]
	private bool expanded;

	public string DataText => _dto.Data.ToString("dd/MM/yyyy");
	public IReadOnlyList<AssistenciaAlumneDto> Alumnes => _dto.Alumnes;
	public int Presents => _dto.Alumnes.Count(a => a.Present);
	public int Total => _dto.Alumnes.Count;
	public string ResumText => $"{Presents}/{Total} presents";

	[RelayCommand]
	private void Toggle() => Expanded = !Expanded;
}
