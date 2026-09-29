using AppAjuntament.Models;
using AppAjuntament.Models.PuntsAigua;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AppAjuntament.Services;

public class PuntsAiguaService
{
    private readonly HttpClient _httpClient;
    private readonly DatabaseLoggerService _dbLogger;
    private readonly IOptions<AjuntamentSettings> _settings;

    public PuntsAiguaService(HttpClient httpClient, DatabaseLoggerService dbLogger, IOptions<AjuntamentSettings> settings)
    {
        _httpClient = httpClient;
        _dbLogger = dbLogger;
        _settings = settings;
    }

    public async Task<List<PuntAigua>> GetPuntsAiguaAsync()
    {
        var nomMunicipi = (_settings.Value.NomMunicipi ?? string.Empty).Trim().ToUpperInvariant();

        try
        {
            var url = "https://incendis.diba.cat/fitxers/DadesObertes/json/PuntsAigua.json";
            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                await _dbLogger.LogWarningAsync($"API punts d'aigua retorna status: {response.StatusCode}", "GetPuntsAiguaAsync");
                return new();
            }

            var stream = await response.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var tots = await JsonSerializer.DeserializeAsync<List<PuntAigua>>(stream, options) ?? new();

            return tots.Where(p => string.Equals(p.Municipi?.Trim(), nomMunicipi, StringComparison.OrdinalIgnoreCase)
                                && p.Lat.HasValue && p.Long.HasValue).ToList();
        }
        catch (TaskCanceledException)
        {
            await _dbLogger.LogWarningAsync("Timeout obtenint punts d'aigua.", "GetPuntsAiguaAsync");
            throw;
        }
        catch (Exception ex)
        {
            await _dbLogger.LogFatalAsync("Error obtenint punts d'aigua", ex, "GetPuntsAiguaAsync");
            throw;
        }
    }
}
