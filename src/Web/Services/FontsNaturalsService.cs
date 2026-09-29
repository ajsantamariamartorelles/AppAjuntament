using AppAjuntament.Models;
using AppAjuntament.Models.FontsNaturals;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AppAjuntament.Services;

public class FontsNaturalsService
{
    private readonly HttpClient _httpClient;
    private readonly DatabaseLoggerService _dbLogger;
    private readonly IOptions<AjuntamentSettings> _settings;

    public FontsNaturalsService(HttpClient httpClient, DatabaseLoggerService dbLogger, IOptions<AjuntamentSettings> settings)
    {
        _httpClient = httpClient;
        _dbLogger = dbLogger;
        _settings = settings;
    }

    public async Task<List<FontNaturalAttributes>> GetFontsNaturalsAsync()
    {
        var codiDiba = (_settings.Value.CodiDiba ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(codiDiba))
        {
            await _dbLogger.LogWarningAsync("Manca CodiDiba per obtenir fonts naturals.", "GetFontsNaturalsAsync");
            return new();
        }

        var where = Uri.EscapeDataString($"FDB_CODI_INE='{codiDiba}'");
        var url = $"https://gissrv.diba.cat/arcgis/rest/services/SITAC/FONTS_NATURALS/MapServer/0/query?where={where}&outFields=*&returnGeometry=false&f=json";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                await _dbLogger.LogWarningAsync($"API fonts naturals retorna status: {response.StatusCode}", "GetFontsNaturalsAsync");
                return new();
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<FontsNaturalsResponse>(content, options);

            return result?.features.Select(f => f.attributes).ToList() ?? new();
        }
        catch (TaskCanceledException)
        {
            await _dbLogger.LogWarningAsync("Timeout connectant amb API fonts naturals.", "GetFontsNaturalsAsync");
            throw;
        }
        catch (Exception ex)
        {
            await _dbLogger.LogFatalAsync("Error obtenint fonts naturals", ex, "GetFontsNaturalsAsync");
            throw;
        }
    }
}
