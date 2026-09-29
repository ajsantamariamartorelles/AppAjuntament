using System.Globalization;
using System.Text.Json;
using AppAjuntament.Models;
using AppAjuntament.Models.Idescat;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AppAjuntament.Services;

public interface IIdescatEmexService
{
    Task<IdescatEmexData> ObtenirDadesMunicipiAsync(CancellationToken cancellationToken = default);
}

public class IdescatEmexService : IIdescatEmexService
{
    private const string BaseUrl = "https://api.idescat.cat/emex/v1/dades.json";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(12);

    private readonly HttpClient _httpClient;
    private readonly IOptions<AjuntamentSettings> _ajuntamentSettings;
    private readonly DatabaseLoggerService _dbLogger;
    private readonly ILogger<IdescatEmexService> _logger;
    private readonly IMemoryCache _memoryCache;

    public IdescatEmexService(
        HttpClient httpClient,
        IOptions<AjuntamentSettings> ajuntamentSettings,
        DatabaseLoggerService dbLogger,
        ILogger<IdescatEmexService> logger,
        IMemoryCache memoryCache)
    {
        _httpClient = httpClient;
        _ajuntamentSettings = ajuntamentSettings;
        _dbLogger = dbLogger;
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public async Task<IdescatEmexData> ObtenirDadesMunicipiAsync(CancellationToken cancellationToken = default)
    {
        var codiMunicipi = (_ajuntamentSettings.Value.CodiINE6 ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(codiMunicipi))
        {
            await _dbLogger.LogWarningAsync(
                "No s'ha trobat el codi INE6 del municipi a la configuració",
                nameof(ObtenirDadesMunicipiAsync));

            return new IdescatEmexData();
        }

        var url = $"{BaseUrl}?id={Uri.EscapeDataString(codiMunicipi)}&lang=ca";
        var cacheKey = $"idescat:emex:{codiMunicipi}";

        if (_memoryCache.TryGetValue(cacheKey, out IdescatEmexData? dadesCachejades) && dadesCachejades != null)
        {
            return dadesCachejades;
        }

        try
        {
            await _dbLogger.LogDebugAsync(
                $"Cridant API IDESCAT EMEX: {url}",
                nameof(ObtenirDadesMunicipiAsync));

            var response = await _httpClient.GetWithRetryAsync(
                requestUri: url,
                maxRetries: 3,
                logger: _logger,
                cancellationToken: cancellationToken);

            if (response == null)
            {
                await _dbLogger.LogWarningAsync(
                    $"No s'ha pogut obtenir resposta de l'API IDESCAT per al municipi {codiMunicipi}",
                    nameof(ObtenirDadesMunicipiAsync));

                return new IdescatEmexData
                {
                    CodiMunicipi = codiMunicipi
                };
            }

            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            var dades = ParsejarResposta(codiMunicipi, payload);

            _memoryCache.Set(cacheKey, dades, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration
            });

            await _dbLogger.LogInformationAsync(
                $"IDESCAT: {dades.Indicadors.Count} indicadors carregats per al municipi {codiMunicipi}",
                nameof(ObtenirDadesMunicipiAsync));

            return dades;
        }
        catch (Exception ex)
        {
            await _dbLogger.LogFatalAsync(
                $"Error crític obtenint dades IDESCAT per al municipi {codiMunicipi}",
                ex,
                nameof(ObtenirDadesMunicipiAsync));

            return new IdescatEmexData
            {
                CodiMunicipi = codiMunicipi
            };
        }
    }

    private static IdescatEmexData ParsejarResposta(string codiMunicipi, string payload)
    {
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;

        if (!TryGetProperty(root, "fitxes", out var fitxes))
        {
            return new IdescatEmexData
            {
                CodiMunicipi = codiMunicipi
            };
        }

        var data = new IdescatEmexData
        {
            CodiMunicipi = codiMunicipi
        };

        CarregarColumnes(fitxes, data);
        CarregarIndicadors(fitxes, data);

        return data;
    }

    private static void CarregarColumnes(JsonElement fitxes, IdescatEmexData data)
    {
        if (!TryGetProperty(fitxes, "cols", out var colsElement))
        {
            return;
        }

        if (!TryGetProperty(colsElement, "col", out var colElement))
        {
            return;
        }

        foreach (var col in EnumerarElement(colElement))
        {
            var scheme = GetString(col, "scheme");
            var content = GetString(col, "content");

            if (string.IsNullOrWhiteSpace(scheme) || string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            if (scheme == "mun")
            {
                data.NomMunicipi = content;
            }
            else if (scheme == "com")
            {
                data.NomComarca = content;
            }
            else if (scheme == "ca")
            {
                data.NomCatalunya = content;
            }
        }
    }

    private static void CarregarIndicadors(JsonElement fitxes, IdescatEmexData data)
    {
        if (!TryGetProperty(fitxes, "gg", out var ggElement))
        {
            return;
        }

        if (!TryGetProperty(ggElement, "g", out var gElement))
        {
            return;
        }

        foreach (var grup in EnumerarElement(gElement))
        {
            var grupId = GetString(grup, "id") ?? string.Empty;
            var grupNom = GetString(grup, "c") ?? string.Empty;

            if (!TryGetProperty(grup, "tt", out var ttElement) || !TryGetProperty(ttElement, "t", out var tElement))
            {
                continue;
            }

            foreach (var taula in EnumerarElement(tElement))
            {
                var taulaId = GetString(taula, "id") ?? string.Empty;
                var taulaNom = GetString(taula, "c") ?? string.Empty;

                var rTaula = GetString(taula, "r");
                var uTaula = GetString(taula, "u");
                var sTaula = GetString(taula, "s");
                var lTaula = GetString(taula, "l");
                var updatedTaula = ParseDate(GetString(taula, "updated"));

                if (!TryGetProperty(taula, "ff", out var ffElement) || !TryGetProperty(ffElement, "f", out var fElement))
                {
                    continue;
                }

                foreach (var fila in EnumerarElement(fElement))
                {
                    var indicador = new IdescatEmexIndicatorRow
                    {
                        GrupId = grupId,
                        GrupNom = grupNom,
                        TaulaId = taulaId,
                        TaulaNom = taulaNom,
                        IndicadorId = GetString(fila, "id") ?? string.Empty,
                        IndicadorNom = GetString(fila, "calt") ?? GetString(fila, "c") ?? string.Empty,
                        Unitat = GetString(fila, "u") ?? uTaula,
                        ReferenciaTemporal = GetString(fila, "r") ?? rTaula,
                        DataActualitzacio = ParseDate(GetString(fila, "updated")) ?? updatedTaula,
                        Font = sTaula,
                        Enllac = lTaula
                    };

                    var vectorValors = GetString(fila, "v") ?? string.Empty;
                    AssignarValorsVector(indicador, vectorValors);

                    data.Indicadors.Add(indicador);
                }
            }
        }
    }

    private static void AssignarValorsVector(IdescatEmexIndicatorRow indicador, string vectorValors)
    {
        var valors = vectorValors.Split(',', StringSplitOptions.TrimEntries);

        indicador.ValorMunicipiText = ObtenirValorText(valors, 0);
        indicador.ValorComarcaText = ObtenirValorText(valors, 1);
        indicador.ValorCatalunyaText = ObtenirValorText(valors, 2);

        indicador.ValorMunicipi = ParseDecimal(indicador.ValorMunicipiText);
        indicador.ValorComarca = ParseDecimal(indicador.ValorComarcaText);
        indicador.ValorCatalunya = ParseDecimal(indicador.ValorCatalunyaText);
    }

    private static string ObtenirValorText(string[] valors, int index)
    {
        if (index >= valors.Length)
        {
            return "_";
        }

        return string.IsNullOrWhiteSpace(valors[index]) ? "_" : valors[index].Trim();
    }

    private static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "_")
        {
            return null;
        }

        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static DateTimeOffset? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
            ? parsed
            : null;
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var valueElement))
        {
            return null;
        }

        return valueElement.ValueKind == JsonValueKind.String
            ? valueElement.GetString()
            : valueElement.ToString();
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    private static IEnumerable<JsonElement> EnumerarElement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                yield return item;
            }

            yield break;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            yield return element;
        }
    }
}
