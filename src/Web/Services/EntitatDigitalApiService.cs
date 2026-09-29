using System.Net.Http.Json;
using System.Text.Json;
using AppAjuntament.Models.Municipi;
using Microsoft.Extensions.Logging;

namespace AppAjuntament.Services;

public class EntitatDigitalApiService
{
    private readonly HttpClient _httpClient;
    private readonly DatabaseLoggerService _dbLogger;
    private readonly string baseUrl = "http://dadesobertes.seu-e.cat/api/action/datastore_search";
    private readonly string resourceId = "2dd896a1-3d81-4c31-92d8-ab17a1fc5199";

    public EntitatDigitalApiService(HttpClient httpClient, DatabaseLoggerService dbLogger)
    {
        _httpClient = httpClient;
        _dbLogger = dbLogger;
    }

    public async Task<EntitatDigitalRecord?> GetEntitatDigitalAsync(string codiEns)
    {
        try
        {
            var filters = $"{{\"CODI_ENS\":\"{codiEns}\"}}";
            var url = $"{baseUrl}?resource_id={resourceId}&filters={filters}&limit=100";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var apiResponse = await response.Content.ReadFromJsonAsync<EntitatDigitalApiResponse>();
            if (apiResponse?.success == true && apiResponse.result?.records?.Count > 0)
            {
                return apiResponse.result.records[0];
            }
            else
            {
                return null;
            }
        }
        catch (TaskCanceledException ex)
        {
            await _dbLogger.LogFatalAsync($"Timeout obtenint dades d'entitat digital per al codi ENS {codiEns} (API externa no respon)", ex, "GetEntitatDigitalAsync", $"CodiEns={codiEns}");
            return null;
        }
        catch (HttpRequestException ex)
        {
            await _dbLogger.LogFatalAsync($"Error de xarxa obtenint dades d'entitat digital per al codi ENS {codiEns}", ex, "GetEntitatDigitalAsync", $"CodiEns={codiEns}");
            return null;
        }
        catch (Exception ex)
        {
            await _dbLogger.LogFatalAsync($"Error crític obtenint dades d'entitat digital per al codi ENS {codiEns}", ex, "GetEntitatDigitalAsync", $"CodiEns={codiEns}");
            return null;
        }
    }
}