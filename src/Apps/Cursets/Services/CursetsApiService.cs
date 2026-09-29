using System.Net.Http.Json;
using AjuntamentSantaMariaMartorelles.Models;

namespace AjuntamentSantaMariaMartorelles.Services;

/// <summary>
/// Crides a l'API de Cursets que requereixen el token de professora (afegit
/// automàticament per AuthTokenHandler).
/// </summary>
public class CursetsApiService
{
	private readonly HttpClient _httpClient;

	public CursetsApiService(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <summary>Cursets assignats a la professora autenticada.</summary>
	public async Task<List<CursetDto>> GetCursetsAsync()
	{
		var cursets = await _httpClient.GetFromJsonAsync<List<CursetDto>>("api/cursets");
		return cursets ?? new List<CursetDto>();
	}

	/// <summary>"Iniciar classe": crea o recupera la sessió d'avui, amb totes les alumnes marcades presents per defecte.</summary>
	public async Task<SessioObertaDto> ObrirSessioAsync(int cursetId)
	{
		var response = await _httpClient.PostAsync($"api/cursets/sessions/obrir?cursetId={cursetId}", content: null);
		response.EnsureSuccessStatusCode();

		var sessio = await response.Content.ReadFromJsonAsync<SessioObertaDto>();
		return sessio ?? throw new InvalidOperationException("Resposta buida en obrir la sessió.");
	}

	/// <summary>Desa qui ha vingut (i qui no) i tanca la sessió.</summary>
	public async Task TancarSessioAsync(int sessioId, GuardarAssistenciaRequest request)
	{
		var response = await _httpClient.PostAsJsonAsync($"api/cursets/sessions/{sessioId}/tancar", request);
		response.EnsureSuccessStatusCode();
	}

	/// <summary>Històric de sessions d'un curset (data desc.) amb el detall d'assistència. Només lectura.</summary>
	public async Task<List<SessioObertaDto>> GetHistoricAsync(int cursetId)
	{
		var historic = await _httpClient.GetFromJsonAsync<List<SessioObertaDto>>($"api/cursets/sessions/historic?cursetId={cursetId}");
		return historic ?? new List<SessioObertaDto>();
	}

	// ---- Seguiment del controlador (regidor), només lectura ----

	/// <summary>Places, llista d'espera i pendents de pagar de cada curset que supervisa.</summary>
	public async Task<List<ResumSeguimentCursetDto>> GetResumSeguimentAsync()
	{
		var resum = await _httpClient.GetFromJsonAsync<List<ResumSeguimentCursetDto>>("api/cursets/seguiment/resum");
		return resum ?? new List<ResumSeguimentCursetDto>();
	}

	/// <summary>Persones inscrites a un curset amb la situació de les seves liquidacions.</summary>
	public async Task<InscritesCursetDto> GetInscritesAsync(int cursetId)
	{
		var inscrites = await _httpClient.GetFromJsonAsync<InscritesCursetDto>($"api/cursets/seguiment/cursets/{cursetId}/inscrites");
		return inscrites ?? throw new InvalidOperationException("Resposta buida.");
	}

	/// <summary>Fitxa d'una persona en un curset: inscripció, assistència i liquidacions.</summary>
	public async Task<PersonaFitxaDto> GetPersonaAsync(int cursetId, int alumneId)
	{
		var fitxa = await _httpClient.GetFromJsonAsync<PersonaFitxaDto>($"api/cursets/seguiment/cursets/{cursetId}/persones/{alumneId}");
		return fitxa ?? throw new InvalidOperationException("Resposta buida.");
	}

	/// <summary>Pendents de cobrament de tots els cursets que supervisa.</summary>
	public async Task<CobramentsDto> GetCobramentsAsync()
	{
		var cobraments = await _httpClient.GetFromJsonAsync<CobramentsDto>("api/cursets/seguiment/cobraments");
		return cobraments ?? new CobramentsDto();
	}
}
