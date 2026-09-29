using AppAjuntament.Models.Establiments;
using System.Text.Json;

namespace AppAjuntament.Services
{
    public interface IEstablimentsApiService
    {
        Task<EstablimentsApiResponse?> ObtindreEstablimentsAsync(string codiMunicipi = "08256");
    }

    public class EstablimentsApiService : IEstablimentsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly DatabaseLoggerService _dbLogger;
        private const string API_URL = "https://do.diba.cat/api/dataset/establiments/camp-rel_municipi";

        public EstablimentsApiService(HttpClient httpClient, DatabaseLoggerService dbLogger)
        {
            _httpClient = httpClient;
            _dbLogger = dbLogger;
        }

        public async Task<EstablimentsApiResponse?> ObtindreEstablimentsAsync(string codiMunicipi = "08256")
        {
            try
            {
                var url = $"{API_URL}/{codiMunicipi}";
                
                await _dbLogger.LogDebugAsync($"Obtenint establiments del municipi {codiMunicipi}: {url}", "ObtindreEstablimentsAsync");

                var response = await _httpClient.GetWithRetryAsync(url, maxRetries: 3);
                if (response == null)
                {
                    await _dbLogger.LogWarningAsync($"No s'ha pogut obtenir establiments per al municipi {codiMunicipi}", "ObtindreEstablimentsAsync");
                    return null;
                }
                
                var jsonContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<EstablimentsApiResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                });

                if (apiResponse?.elements == null)
                {
                    await _dbLogger.LogWarningAsync($"L'API no ha retornat establiments per al municipi {codiMunicipi}", "ObtindreEstablimentsAsync");
                    return null;
                }

                await _dbLogger.LogInformationAsync($"S'han obtingut {apiResponse.elements.Count} establiments per al municipi {codiMunicipi}", "ObtindreEstablimentsAsync");

                return apiResponse;
            }
            catch (Exception ex)
            {
                await _dbLogger.LogFatalAsync($"Error crític obtenint establiments del municipi {codiMunicipi}", ex, "ObtindreEstablimentsAsync", $"CodiMunicipi={codiMunicipi}");
                return null;
            }
        }
    }
}