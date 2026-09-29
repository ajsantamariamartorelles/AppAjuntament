using AppAjuntament.Models.Conveni;
using System.Text.Json;

namespace AppAjuntament.Services
{
    public class ConvenisApiService
    {
        private readonly HttpClient _httpClient;
        private readonly DatabaseLoggerService _dbLogger;

        public ConvenisApiService(HttpClient httpClient, DatabaseLoggerService dbLogger)
        {
            _httpClient = httpClient;
            _dbLogger = dbLogger;
        }

        public async Task<List<Conveni>?> GetConvenisAsync(string? codiEns = null)
        {
            try
            {
                var baseUrl = "http://dadesobertes.seu-e.cat/api/action/datastore_search?resource_id=8747a24f-aa98-4a7e-938a-df81cc16769a";
                var filters = codiEns != null ? $"&filters={{\"CODI_ENS\":\"{codiEns}\"}}" : "";
                var url = $"{baseUrl}{filters}&limit=1000"; // Augmentem el limit per obtenir més registres

                await _dbLogger.LogDebugAsync($"Cridant API de convenis: {url}", "GetConvenisAsync", codiEns != null ? $"CodiEns={codiEns}" : null);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                    };

                    var result = JsonSerializer.Deserialize<ConvenisApiResponse>(content, options);

                    if (result?.success == true && result.result?.records != null)
                    {
                        await _dbLogger.LogInformationAsync($"S'han obtingut {result.result.records.Count} convenis", "GetConvenisAsync");
                        // Set IsLocal to false for all convenis from API
                        foreach (var conveni in result.result.records)
                        {
                            conveni.IsLocal = false;
                        }
                        return result.result.records;
                    }
                    else
                    {
                        await _dbLogger.LogWarningAsync("L'API de convenis no ha retornat resultats vàlids", "GetConvenisAsync");
                        return new List<Conveni>();
                    }
                }
                else
                {
                    await _dbLogger.LogWarningAsync($"API convenis retorna status: {response.StatusCode}", "GetConvenisAsync");
                    return null;
                }
            }
            catch (Exception ex)
            {
                await _dbLogger.LogFatalAsync("Error crític obtenint convenis", ex, "GetConvenisAsync", codiEns != null ? $"CodiEns={codiEns}" : null);
                return null;
            }
        }
    }
}