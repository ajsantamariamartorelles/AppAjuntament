using AppAjuntament.Models;
using AppAjuntament.Models.CIDO;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AppAjuntament.Services
{
    public class OrdenancesApiService
    {
        private readonly HttpClient _httpClient;
        private readonly DatabaseLoggerService _dbLogger;
        private readonly IOptions<AjuntamentSettings> _ajuntamentSettings;

        public OrdenancesApiService(
            HttpClient httpClient,
            DatabaseLoggerService dbLogger,
            IOptions<AjuntamentSettings> ajuntamentSettings)
        {
            _httpClient = httpClient;
            _dbLogger = dbLogger;
            _ajuntamentSettings = ajuntamentSettings;
        }

        public async Task<OrdenancesApiResponse?> GetOrdenancesAsync()
        {
            try
            {
                var institucio = (_ajuntamentSettings.Value.NomInstitucio ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(institucio))
                {
                    await _dbLogger.LogWarningAsync(
                        "Manca la configuració NomInstitucio per a la petició d'ordenances.",
                        "GetOrdenancesAsync");
                    return null;
                }

                var url = $"https://api.diba.cat/dadesobertes/cido/v1/normatives-locals?filter[esVigent]=true&filter[institucioDesenvolupat]={Uri.EscapeDataString(institucio)}";

                await _dbLogger.LogDebugAsync($"Cridant API d'ordenances: {url}", "GetOrdenancesAsync");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var result = JsonSerializer.Deserialize<OrdenancesApiResponse>(content, options);

                    await _dbLogger.LogInformationAsync($"Ordenances obtingudes: {result?.meta?.totalResourceCount ?? 0}", "GetOrdenancesAsync");

                    return result;
                }
                else
                {
                    await _dbLogger.LogWarningAsync($"API ordenances retorna status: {response.StatusCode}", "GetOrdenancesAsync");
                    return null;
                }
            }
            catch (TaskCanceledException ex)
            {
                await _dbLogger.LogWarningAsync($"Timeout connectant amb API ordenances: {ex.Message}", "GetOrdenancesAsync");
                throw;
            }
            catch (HttpRequestException ex)
            {
                await _dbLogger.LogWarningAsync($"Error de xarxa connectant amb API ordenances: {ex.Message}", "GetOrdenancesAsync");
                throw;
            }
            catch (Exception ex)
            {
                await _dbLogger.LogFatalAsync("Error crític obtenint ordenances", ex, "GetOrdenancesAsync");
                throw;
            }
        }
    }
}
