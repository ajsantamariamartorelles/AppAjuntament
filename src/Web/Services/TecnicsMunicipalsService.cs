using AppAjuntament.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppAjuntament.Services;

public class TecnicMunicipal
{
    [JsonPropertyName("name")]
    public string Nom { get; set; } = "";

    [JsonPropertyName("surname")]
    public string Cognom { get; set; } = "";

    [JsonPropertyName("position")]
    public string Carrec { get; set; } = "";

    public string NomComplet => $"{Nom} {Cognom}".Trim();
}

public class TecnicsMunicipalsService
{
    private readonly HttpClient _httpClient;
    private readonly DatabaseLoggerService _dbLogger;
    private readonly IOptions<AjuntamentSettings> _settings;

    public TecnicsMunicipalsService(HttpClient httpClient, DatabaseLoggerService dbLogger, IOptions<AjuntamentSettings> settings)
    {
        _httpClient = httpClient;
        _dbLogger = dbLogger;
        _settings = settings;
    }

    public async Task<List<TecnicMunicipal>> GetTecnicsAsync()
    {
        var codiIne = (_settings.Value.CodiDiba ?? "08256").Trim();
        var url = $"https://do.diba.cat/api/dataset/tecnics/camp-codi_ine/{codiIne}/format/json";

        try
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<TecnicMunicipal>();

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Helper: try to find an array element in the JSON structure.
            JsonElement? FindArrayElement(JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Array)
                    return element;

                // common root properties that may contain arrays or nested objects with arrays
                foreach (var name in new[] { "entitats", "records", "data", "items", "results", "result" })
                {
                    if (element.TryGetProperty(name, out var prop))
                    {
                        if (prop.ValueKind == JsonValueKind.Array)
                            return prop;
                        if (prop.ValueKind == JsonValueKind.Object)
                        {
                            // inside object, look for typical array properties
                            foreach (var inner in new[] { "records", "data", "items", "entitats" })
                            {
                                if (prop.TryGetProperty(inner, out var innerProp) && innerProp.ValueKind == JsonValueKind.Array)
                                    return innerProp;
                            }
                        }
                    }
                }

                // as a last resort, scan properties for the first array found (shallow)
                foreach (var p in element.EnumerateObject())
                {
                    if (p.Value.ValueKind == JsonValueKind.Array)
                        return p.Value;
                }

                return null;
            }

            var arrayElement = FindArrayElement(doc.RootElement);
            if (arrayElement.HasValue)
            {
                var results = new List<TecnicMunicipal>();
                foreach (var item in arrayElement.Value.EnumerateArray())
                {
                    if (TryParseTecnic(item, out var t))
                    {
                        results.Add(t);
                    }
                }

                if (results.Count == 0)
                {
                    await _dbLogger.LogWarningAsync("No tècnics municipals could be parsed from Seu-e JSON array.", "GetTecnicsAsync");
                }
                return results;
            }

            return new List<TecnicMunicipal>();
        }
        catch (Exception ex)
        {
            await _dbLogger.LogWarningAsync($"Error obtenint tècnics municipals: {ex.Message}", "GetTecnicsAsync");
            return new List<TecnicMunicipal>();
        }
    }

    private bool TryParseTecnic(JsonElement item, out TecnicMunicipal? result)
    {
        result = null;
        try
        {
            if (item.ValueKind != JsonValueKind.Object)
                return false;

            // helper to get a string from an object or nested object
            static string? GetStringFromObject(JsonElement obj, params string[] keys)
            {
                foreach (var k in keys)
                {
                    if (obj.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.String)
                        return v.GetString();
                }
                // case-insensitive match
                foreach (var p in obj.EnumerateObject())
                {
                    foreach (var k in keys)
                    {
                        if (string.Equals(p.Name, k, StringComparison.OrdinalIgnoreCase) && p.Value.ValueKind == JsonValueKind.String)
                            return p.Value.GetString();
                    }
                }
                return null;
            }

            // Try direct fields first
            var name = GetStringFromObject(item, "name", "nom", "firstname", "first_name");
            var surname = GetStringFromObject(item, "surname", "cognom", "lastname", "last_name");
            var position = GetStringFromObject(item, "position", "carrec", "role", "ocupacio");

            // If not present, try common nested shapes
            if (name == null || surname == null || position == null)
            {
                foreach (var prop in item.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Object)
                    {
                        name ??= GetStringFromObject(prop.Value, "name", "nom", "firstname", "first_name");
                        surname ??= GetStringFromObject(prop.Value, "surname", "cognom", "lastname", "last_name");
                        position ??= GetStringFromObject(prop.Value, "position", "carrec", "role", "ocupacio");
                    }
                    else if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        // sometimes records are nested in an array of objects
                        foreach (var sub in prop.Value.EnumerateArray())
                        {
                            if (sub.ValueKind != JsonValueKind.Object) continue;
                            name ??= GetStringFromObject(sub, "name", "nom", "firstname", "first_name");
                            surname ??= GetStringFromObject(sub, "surname", "cognom", "lastname", "last_name");
                            position ??= GetStringFromObject(sub, "position", "carrec", "role", "ocupacio");
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(surname))
                return false;

            result = new TecnicMunicipal
            {
                Nom = name ?? string.Empty,
                Cognom = surname ?? string.Empty,
                Carrec = position ?? string.Empty
            };
            return true;
        }
        catch
        {
            return false;
        }
    }
}
