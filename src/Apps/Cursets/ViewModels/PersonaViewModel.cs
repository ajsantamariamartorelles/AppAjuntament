using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using AjuntamentSantaMariaMartorelles.Models;
using AjuntamentSantaMariaMartorelles.Services;

namespace AjuntamentSantaMariaMartorelles.ViewModels;

/// <summary>
/// Fitxa d'una persona en un curset, per al controlador (regidor): inscripció
/// (amb les dates de la baixa si n'ha fet), assistència i liquidacions. Només
/// lectura, i sense DNI, adreça ni dades de contacte.
/// </summary>
public partial class PersonaViewModel : BaseViewModel, IQueryAttributable
{
	private readonly CursetsApiService _apiService;

	public PersonaViewModel(CursetsApiService apiService)
	{
		_apiService = apiService;
	}

	[ObservableProperty]
	private string nom = string.Empty;

	[ObservableProperty]
	private string cursetTitol = string.Empty;

	[ObservableProperty]
	private Pill? etiquetaEstat;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(SenseLiquidacions))]
	private bool teDades;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(SenseLiquidacions))]
	private bool teLiquidacions;

	public bool SenseLiquidacions => TeDades && !TeLiquidacions;

	/// <summary>Files etiqueta/valor de la targeta de dades (només les que apliquen a l'estat).</summary>
	public ObservableCollection<FilaDada> Dades { get; } = new();

	public ObservableCollection<LiquidacioSeguimentDto> Liquidacions { get; } = new();

	public async void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetValue("cursetId", out var c) && int.TryParse(c?.ToString(), out var cursetId)
			&& query.TryGetValue("alumneId", out var a) && int.TryParse(a?.ToString(), out var alumneId))
		{
			await CarregarAsync(cursetId, alumneId);
		}
	}

	private async Task CarregarAsync(int cursetId, int alumneId)
	{
		if (IsBusy) return;

		try
		{
			IsBusy = true;
			ErrorMessage = null;

			var fitxa = await _apiService.GetPersonaAsync(cursetId, alumneId);
			var p = fitxa.Persona;

			Nom = p.NomComplet;
			CursetTitol = fitxa.CursetTitol;
			EtiquetaEstat = p.EtiquetaEstat;

			Dades.Clear();
			if (p.EsSollicitada || p.EsEspera)
			{
				if (p.DataSollicitud is DateTime ds)
					Dades.Add(new("Sol·licitud", $"{ds:dd/MM/yyyy}"));
				if (p.EsEspera && p.OrdreLlistaEspera is int n)
					Dades.Add(new("Posició a la llista", $"Núm. {n}"));
			}
			else
			{
				Dades.Add(new("Alta al curset", $"{p.DataAlta:dd/MM/yyyy}"));
				if (p.EsBaixa)
					Dades.Add(new("Baixa del curset", p.DataBaixa is DateTime db ? $"{db:dd/MM/yyyy}" : "Sense data", Destacat: true));
				Dades.Add(new("Tarifa", $"{(p.Empadronat ? "Empadronat" : "No empadronat")} · {Format.Euros(fitxa.PreuPerSessio)}/sessió"));
				Dades.Add(new("Assistència", fitxa.SessionsImpartides == 0
					? "Cap sessió impartida"
					: $"{fitxa.SessionsAssistides} de {fitxa.SessionsImpartides} sessions"));
				Dades.Add(new("Última assistència", fitxa.UltimaAssistencia is DateTime u ? $"{u:dd/MM/yyyy}" : "—"));
			}
			if (!string.IsNullOrWhiteSpace(p.Origen))
				Dades.Add(new("Origen", string.Equals(p.Origen, "Web", StringComparison.OrdinalIgnoreCase) ? "Formulari web" : "Alta presencial"));
			Dades.Add(new("Pendent de pagar", Format.Euros(p.ImportPendent), Destacat: p.TePendent));

			Liquidacions.Clear();
			foreach (var l in fitxa.Liquidacions)
				Liquidacions.Add(l);
			TeLiquidacions = Liquidacions.Count > 0;
			TeDades = true;
		}
		catch (Exception ex)
		{
			ErrorMessage = $"No s'ha pogut carregar la fitxa: {ex.Message}";
		}
		finally
		{
			IsBusy = false;
		}
	}
}

/// <summary>Fila etiqueta/valor. <paramref name="Destacat"/> pinta el valor en vermell.</summary>
public record FilaDada(string Etiqueta, string Valor, bool Destacat = false);
