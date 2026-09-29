using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AppAjuntament.Services;

/// <summary>
/// Servei genèric per accedir a les APIs CKAN de dadesobertes.seu-e.cat
/// </summary>
public class CkanApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CkanApiService> _logger;
    private const string CKAN_BASE_URL = "https://dadesobertes.seu-e.cat/api/3/action";

    public CkanApiService(HttpClient httpClient, ILogger<CkanApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Obté dades d'un resource CKAN especificat
    /// </summary>
    public async Task<CkanSearchResult?> GetDatastoreSearch(
        string resourceId,
        int limit = 10,
        int offset = 0,
        Dictionary<string, object>? filters = null)
    {
        try
        {
            var url = $"{CKAN_BASE_URL}/datastore_search?resource_id={resourceId}&limit={limit}&offset={offset}";
            if (filters != null && filters.Count > 0)
            {
                var filtersJson = JsonSerializer.Serialize(filters);
                url += $"&filters={Uri.EscapeDataString(filtersJson)}";
            }

            _logger.LogInformation("[CKAN] GET {Url}", url);

            var response = await _httpClient.GetFromJsonAsync<CkanApiResponse>(url);

            if (response?.Success == true)
            {
                return response.Result;
            }

            _logger.LogWarning($"CKAN API returned success=false for resource {resourceId}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error calling CKAN API for resource {resourceId}");
            return null;
        }
    }

    /// <summary>
    /// Obté el total de registres d'un resource
    /// </summary>
    public async Task<int> GetTotalRecords(string resourceId)
    {
        try
        {
            var result = await GetDatastoreSearch(resourceId, limit: 1, offset: 0);
            return result?.Total ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting total records for resource {resourceId}");
            return 0;
        }
    }
}

public class CkanApiResponse
{
    public bool Success { get; set; }
    public CkanSearchResult? Result { get; set; }
}

public class CkanSearchResult
{
    public int Total { get; set; }
    public List<Dictionary<string, object>> Records { get; set; } = new();
    public List<CkanField> Fields { get; set; } = new();
}

public class CkanField
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
