using AppAjuntament.Models.Municipi;
using AppAjuntament.Extensions;
using System.Text.Json;

namespace AppAjuntament.Services;

public interface IMunicipiApiService
{
    Task<ApiMunicipiElement?> ObtindreInfoMunicipiAsync(string nomMunicipi);
}

public class MunicipiApiService : IMunicipiApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MunicipiApiService> _logger;
    private const string API_URL = "https://do.diba.cat/api/dataset/municipis";
    private const int MAX_RETRY_ATTEMPTS = 3;
    private const int RETRY_DELAY_MS = 2000;

    public MunicipiApiService(HttpClient httpClient, ILogger<MunicipiApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ApiMunicipiElement?> ObtindreInfoMunicipiAsync(string nomMunicipi)
    {
        try
        {
            _logger.LogInformation("Obtenint informació del municipi {NomMunicipi} des de l'API de la Diputació", nomMunicipi);
            
            var response = await _httpClient.GetWithRetryAsync(API_URL, maxRetries: MAX_RETRY_ATTEMPTS, logger: _logger);
            if (response == null)
            {
                _logger.LogError("No s'ha pogut connectar amb l'API després de {MaxAttempts} intents", MAX_RETRY_ATTEMPTS);
                return null;
            }
            
            var jsonContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiMunicipiResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse?.elements == null)
            {
                _logger.LogWarning("No s'han trobat dades de municipis a l'API");
                return null;
            }

            // Buscar el municipi per nom (case insensitive)
            var municipi = apiResponse.elements.FirstOrDefault(m => 
                string.Equals(m.municipi_nom, nomMunicipi, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(m.municipi_nom_curt, nomMunicipi, StringComparison.OrdinalIgnoreCase));

            if (municipi == null)
            {
                _logger.LogWarning("No s'ha trobat el municipi {NomMunicipi}", nomMunicipi);
            }
            else
            {
                _logger.LogInformation("Municipi {NomMunicipi} obtingut correctament", nomMunicipi);
            }

            return municipi;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obtenint informació del municipi {NomMunicipi}", nomMunicipi);
            return null;
        }
    }
}
