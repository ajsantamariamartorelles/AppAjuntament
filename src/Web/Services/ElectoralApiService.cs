using AppAjuntament.Models.Electoral;
using System.Text.Json;

namespace AppAjuntament.Services
{
    public interface IElectoralApiService
    {
        Task<List<ElectoralRecord>> ObtenirResultatsElectoralsAsync(long codiEns);
        Task<List<int>> ObtenirAnysElectoralsAsync(long codiEns);
        Task<List<ElectoralRecord>> ObtenirResultatsPerAnyAsync(long codiEns, int any);
    }

    public class ElectoralApiService : IElectoralApiService
    {
        private readonly HttpClient _httpClient;
        private readonly DatabaseLoggerService _dbLogger;
        private const string API_BASE_URL = "http://dadesobertes.seu-e.cat/api/action/datastore_search";
        private const string RESOURCE_ID = "3539f7e6-4a48-4b57-9b55-b8c41079b3cd";

        public ElectoralApiService(HttpClient httpClient, DatabaseLoggerService dbLogger)
        {
            _httpClient = httpClient;
            _dbLogger = dbLogger;
        }

        public async Task<List<ElectoralRecord>> ObtenirResultatsElectoralsAsync(long codiEns)
        {
            try
            {
                var url = $"{API_BASE_URL}?resource_id={RESOURCE_ID}&filters={{\"CODI_ENS\":\"{codiEns}\"}}&limit=1000";

                await _dbLogger.LogDebugAsync($"Obtenint resultats electorals per al codi ENS: {codiEns}", "ObtenirResultatsElectoralsAsync", $"URL={url}");

                var response = await _httpClient.GetWithRetryAsync(url, maxRetries: 3);
                if (response == null)
                {
                    await _dbLogger.LogWarningAsync($"No s'ha pogut obtenir resultats electorals per al codi ENS {codiEns}", "ObtenirResultatsElectoralsAsync");
                    return new List<ElectoralRecord>();
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiElectoralResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                });

                if (apiResponse?.success == true && apiResponse.result?.records != null)
                {
                    await _dbLogger.LogInformationAsync($"S'han obtingut {apiResponse.result.records.Count} resultats electorals", "ObtenirResultatsElectoralsAsync");
                    return apiResponse.result.records;
                }
                else
                {
                    await _dbLogger.LogWarningAsync("L'API electoral no conté resultats vàlids", "ObtenirResultatsElectoralsAsync");
                    return new List<ElectoralRecord>();
                }
            }
            catch (Exception ex)
            {
                await _dbLogger.LogFatalAsync($"Error crític obtenint resultats electorals per al codi ENS {codiEns}", ex, "ObtenirResultatsElectoralsAsync", $"CodiEns={codiEns}");
                return new List<ElectoralRecord>();
            }
        }

        public async Task<List<int>> ObtenirAnysElectoralsAsync(long codiEns)
        {
            var resultats = await ObtenirResultatsElectoralsAsync(codiEns);
            return resultats.Select(r => r.ANY_ELECCIO).Distinct().OrderByDescending(a => a).ToList();
        }

        public async Task<List<ElectoralRecord>> ObtenirResultatsPerAnyAsync(long codiEns, int any)
        {
            var resultats = await ObtenirResultatsElectoralsAsync(codiEns);
            return resultats.Where(r => r.ANY_ELECCIO == any).OrderByDescending(r => r.VOTS).ToList();
        }
    }
}