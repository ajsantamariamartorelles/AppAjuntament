using AppAjuntament.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppAjuntament.Services;

public class ComplexEsportiu
{
    public string PuntId { get; set; } = "";
    public string Nom { get; set; } = "";
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string Adreca { get; set; } = "";
    public string Telefon { get; set; } = "";
    public bool TeCoordenades => Lat != 0 && Lng != 0;
    public List<EspaiEsportiu> Espais { get; set; } = [];
}

public class EspaiEsportiu
{
    public string TipusInstalacio { get; set; } = "";
    public string Descripcio { get; set; } = "";
    public string TipusGestio { get; set; } = "";
    public string NombreEspais { get; set; } = "";
    public string SuperficieEspai { get; set; } = "";
    public string Dimensions { get; set; } = "";

    public string IconaTipus => TipusInstalacio.ToLowerInvariant() switch
    {
        var t when t.Contains("piscin") => "fas fa-swimming-pool",
        var t when t.Contains("pavelló") || t.Contains("pavello") || t.Contains("sala") => "fas fa-warehouse",
        var t when t.Contains("pista") || t.Contains("camp") || t.Contains("futbol") => "fas fa-futbol",
        var t when t.Contains("tennis") || t.Contains("pàdel") || t.Contains("padel") => "fas fa-table-tennis",
        var t when t.Contains("atletis") => "fas fa-running",
        var t when t.Contains("gimnàs") || t.Contains("fitnes") => "fas fa-dumbbell",
        _ => "fas fa-running"
    };
}

file class PuntEsportRaw
{
    [JsonPropertyName("punt_id")]
    public string PuntId { get; set; } = "";

    [JsonPropertyName("adreca_nom")]
    public string AdrecaNom { get; set; } = "";

    [JsonPropertyName("localitzacio")]
    public string Localitzacio { get; set; } = "";

    [JsonPropertyName("grup_adreca")]
    public GrupAdrecaRaw? GrupAdreca { get; set; }

    [JsonPropertyName("telefon_contacte")]
    public JsonElement TelefonContacte { get; set; }
}

file class GrupAdrecaRaw
{
    [JsonPropertyName("adreca_completa")]
    public string AdrecaCompleta { get; set; } = "";
}

file class DetallEsportRaw
{
    [JsonPropertyName("punt_esport_nom_tipus_instalacio")]
    public string TipusInstalacio { get; set; } = "";

    [JsonPropertyName("punt_esport_descripcio_instalacio")]
    public string Descripcio { get; set; } = "";

    [JsonPropertyName("punt_esport_nom_tipus_gestio")]
    public string TipusGestio { get; set; } = "";

    [JsonPropertyName("punt_esport_nombre_espais")]
    public string NombreEspais { get; set; } = "";

    [JsonPropertyName("punt_esport_sup_espai_esp")]
    public string SuperficieEspai { get; set; } = "";

    [JsonPropertyName("punt_esport_dimensio_espai_practica")]
    public string Dimensions { get; set; } = "";

    [JsonPropertyName("rel_complex_esportiu")]
    public ComplexRefRaw? Complex { get; set; }
}

file class ComplexRefRaw
{
    [JsonPropertyName("elements")]
    public string Elements { get; set; } = "";
}

public class EquipamentsEsportiusService
{
    private readonly HttpClient _httpClient;
    private readonly DatabaseLoggerService _dbLogger;
    private readonly IOptions<AjuntamentSettings> _settings;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public EquipamentsEsportiusService(HttpClient httpClient, DatabaseLoggerService dbLogger, IOptions<AjuntamentSettings> settings)
    {
        _httpClient = httpClient;
        _dbLogger = dbLogger;
        _settings = settings;
    }

    public async Task<List<ComplexEsportiu>> GetComplexosAsync()
    {
        var codiIne = (_settings.Value.CodiDiba ?? "08256").Trim();

        try
        {
            var (complexosTask, detallsTask) = (
                FetchElementsAsync<PuntEsportRaw>($"https://do.diba.cat/api/dataset/puntesports/camp-rel_municipis/{codiIne}/format/json"),
                FetchElementsAsync<DetallEsportRaw>($"https://do.diba.cat/api/dataset/puntesports_detall/camp-rel_municipi/{codiIne}/format/json")
            );
            await Task.WhenAll(complexosTask, detallsTask);

            var complexosRaw = complexosTask.Result;
            var detallsRaw = detallsTask.Result;

            return complexosRaw.Select(c =>
            {
                var coords = ParseCoords(c.Localitzacio);
                var telefon = ExtractTelefon(c.TelefonContacte);

                return new ComplexEsportiu
                {
                    PuntId = c.PuntId,
                    Nom = c.AdrecaNom,
                    Lat = coords.lat,
                    Lng = coords.lng,
                    Adreca = c.GrupAdreca?.AdrecaCompleta ?? "",
                    Telefon = telefon,
                    Espais = detallsRaw
                        .Where(d => d.Complex?.Elements == c.PuntId)
                        .Select(d => new EspaiEsportiu
                        {
                            TipusInstalacio = d.TipusInstalacio,
                            Descripcio = d.Descripcio,
                            TipusGestio = d.TipusGestio,
                            NombreEspais = d.NombreEspais,
                            SuperficieEspai = d.SuperficieEspai,
                            Dimensions = d.Dimensions
                        }).ToList()
                };
            }).OrderBy(c => c.Nom).ToList();
        }
        catch (Exception ex)
        {
            await _dbLogger.LogWarningAsync($"Error obtenint equipaments esportius: {ex.Message}", "GetComplexosAsync");
            return [];
        }
    }

    private async Task<List<T>> FetchElementsAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return [];

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);

        if (!doc.RootElement.TryGetProperty("elements", out var elements))
            return [];

        return elements.Deserialize<List<T>>(_jsonOptions) ?? [];
    }

    private static (double lat, double lng) ParseCoords(string localitzacio)
    {
        if (string.IsNullOrEmpty(localitzacio)) return (0, 0);
        var parts = localitzacio.Split(',');
        if (parts.Length < 2) return (0, 0);
        var culture = System.Globalization.CultureInfo.InvariantCulture;
        if (double.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Any, culture, out var lat) &&
            double.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Any, culture, out var lng))
            return (lat, lng);
        return (0, 0);
    }

    private static string ExtractTelefon(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var tel = item.ValueKind == JsonValueKind.String ? item.GetString() : item.ToString();
                if (!string.IsNullOrWhiteSpace(tel)) return tel!.Trim();
            }
        }
        else if (element.ValueKind == JsonValueKind.String)
        {
            return element.GetString() ?? "";
        }
        return "";
    }
}
