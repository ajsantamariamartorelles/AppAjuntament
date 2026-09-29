using AppAjuntament.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AppAjuntament.Services;

public class IndicadorTransparencia
{
    public string Titol { get; set; } = "";
    public string Valor { get; set; } = "";
    public string? Detall { get; set; }
    public string Icona { get; set; } = "fas fa-info-circle";
    public string Color { get; set; } = "primary";
    public string? Font { get; set; }
    public int? Any { get; set; }
    public string Estat { get; set; } = "ok"; // ok | pendent | nodata
    public string? EnllacUrl { get; set; }
    public string? EnllacText { get; set; }
}

public class TransparenciaService
{
    private readonly HttpClient _httpClient;
    private readonly DatabaseLoggerService _dbLogger;
    private readonly IOptions<AjuntamentSettings> _settings;

    public TransparenciaService(HttpClient httpClient, DatabaseLoggerService dbLogger, IOptions<AjuntamentSettings> settings)
    {
        _httpClient = httpClient;
        _dbLogger = dbLogger;
        _settings = settings;
    }

    public async Task<IndicadorTransparencia> GetPlaAccessibilitatAsync()
    {
        var codiIne = (_settings.Value.CodiDiba ?? string.Empty).Trim();
        var url = $"https://gissrv.diba.cat/arcgis/rest/services/SITCAR/CSA/MapServer/4/query?where=CODI_INE%3D'{codiIne}'&outFields=NOMMUNI,CODI_INE,ANY_,ESTAT&returnGeometry=false&f=json";

        try
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return IndicadorNoDisponible("Pla d'Accessibilitat", "fas fa-wheelchair", "warning");

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var features = doc.RootElement.GetProperty("features");

            if (features.GetArrayLength() == 0)
                return IndicadorNoDisponible("Pla d'Accessibilitat", "fas fa-wheelchair", "secondary");

            var attr = features[0].GetProperty("attributes");
            var estat = attr.GetProperty("ESTAT").GetString() ?? "";
            var any = attr.TryGetProperty("ANY_", out var anyProp) && anyProp.ValueKind == JsonValueKind.Number
                ? (int?)anyProp.GetInt32() : null;

            return new IndicadorTransparencia
            {
                Titol = "Pla d'Accessibilitat",
                Valor = estat,
                Detall = any.HasValue ? $"Finalitzat l'any {any}" : null,
                Icona = "fas fa-wheelchair",
                Color = estat == "Acabat" ? "success" : "warning",
                Font = "Diputació de Barcelona – SITCAR",
                Any = any,
                Estat = "ok"
            };
        }
        catch (Exception ex)
        {
            await _dbLogger.LogWarningAsync($"Error obtenint pla accessibilitat: {ex.Message}", "GetPlaAccessibilitatAsync");
            return IndicadorNoDisponible("Pla d'Accessibilitat", "fas fa-wheelchair", "secondary");
        }
    }

    public async Task<IndicadorTransparencia> GetMobilitatSostenibleAsync()
    {
        var codiIne = (_settings.Value.CodiDiba ?? string.Empty).Trim();
        var url = $"https://gissrv.diba.cat/arcgis/rest/services/SITCAR/CSA/MapServer/2/query?where=CODI_INE%3D'{codiIne}'&outFields=TITOL,TIPUS,OBLIGAT,ANY_,ESTAT&returnGeometry=false&f=json";

        try
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return IndicadorNoDisponible("Mobilitat Urbana Sostenible", "fas fa-bicycle", "secondary");

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var features = doc.RootElement.GetProperty("features");

            if (features.GetArrayLength() == 0)
                return IndicadorNoDisponible("Mobilitat Urbana Sostenible", "fas fa-bicycle", "secondary");

            var attr = features[0].GetProperty("attributes");
            var estat = attr.GetProperty("ESTAT").GetString() ?? "";
            var tipus = attr.GetProperty("TIPUS").GetString() ?? "";
            var any = attr.TryGetProperty("ANY_", out var anyProp) && anyProp.ValueKind == JsonValueKind.Number
                ? (int?)anyProp.GetInt32() : null;

            return new IndicadorTransparencia
            {
                Titol = "Mobilitat Urbana Sostenible",
                Valor = estat,
                Detall = any.HasValue ? $"{tipus} · {any}" : tipus,
                Icona = "fas fa-bicycle",
                Color = estat == "Acabat" ? "success" : "warning",
                Font = "Diputació de Barcelona – SITCAR",
                Any = any,
                Estat = "ok"
            };
        }
        catch (Exception ex)
        {
            await _dbLogger.LogWarningAsync($"Error obtenint mobilitat sostenible: {ex.Message}", "GetMobilitatSostenibleAsync");
            return IndicadorNoDisponible("Mobilitat Urbana Sostenible", "fas fa-bicycle", "secondary");
        }
    }

    public async Task<IndicadorTransparencia> GetPacteAlcaldesAsync()
    {
        var nomMunicipi = (_settings.Value.NomMunicipi ?? string.Empty).Trim();
        var nomEscaped = Uri.EscapeDataString($"PAL_MUNICIPI='{nomMunicipi}'");
        var url = $"https://gissrv.diba.cat/arcgis/rest/services/SITAC/PACTE_ALCALDES/MapServer/0/query?where={nomEscaped}&outFields=PAL_TIP_ADHESIO,PAL_DATA_APROV_NOU_PACTE,PAL_ZONA_CLIMATICA,PERFIL_CLIMA_AMP&returnGeometry=false&f=json";

        try
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return IndicadorNoDisponible("Pacte dels Alcaldes pel Clima", "fas fa-leaf", "secondary");

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var features = doc.RootElement.GetProperty("features");

            if (features.GetArrayLength() == 0)
                return IndicadorNoDisponible("Pacte dels Alcaldes pel Clima", "fas fa-leaf", "secondary");

            var attr = features[0].GetProperty("attributes");
            var tipusAdhesio = attr.GetProperty("PAL_TIP_ADHESIO").GetString() ?? "0";
            var zonaClimatica = attr.GetProperty("PAL_ZONA_CLIMATICA").GetString();
            var adherit = tipusAdhesio != "0";

            // Extrau l'URL del perfil climàtic de l'HTML
            string? perfilUrl = null;
            if (attr.TryGetProperty("PERFIL_CLIMA_AMP", out var perfilProp))
            {
                var html = perfilProp.GetString() ?? "";
                var match = System.Text.RegularExpressions.Regex.Match(html, @"href=""([^""]+)""");
                if (match.Success) perfilUrl = match.Groups[1].Value;
            }

            return new IndicadorTransparencia
            {
                Titol = "Pacte dels Alcaldes pel Clima",
                Valor = adherit ? "Adherit" : "Pendent d'adhesió",
                Detall = zonaClimatica != null ? $"Zona climàtica: {zonaClimatica}" : null,
                Icona = "fas fa-leaf",
                Color = adherit ? "success" : "warning",
                Font = "Diputació de Barcelona – SITAC",
                Estat = "ok",
                EnllacUrl = perfilUrl,
                EnllacText = "Perfil climàtic"
            };
        }
        catch (Exception ex)
        {
            await _dbLogger.LogWarningAsync($"Error obtenint pacte alcaldes: {ex.Message}", "GetPacteAlcaldesAsync");
            return IndicadorNoDisponible("Pacte dels Alcaldes pel Clima", "fas fa-leaf", "secondary");
        }
    }

    public async Task<IndicadorTransparencia> GetAforamentsIMDAsync()
    {
        const string carretera = "BV-5006";
        var url = $"https://gissrv.diba.cat/arcgis/rest/services/SITCAR/AforamentsPublic/MapServer/2/query?where=CAR_CARRETERA%3D'{carretera}'&outFields=UIM_CODI_ESTACIO,UIM_PQI,UIM_PQF,IMD21,IMD22,IMD23&returnGeometry=false&f=json";

        try
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return IndicadorNoDisponible($"Trànsit {carretera}", "fas fa-car", "warning");

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var features = doc.RootElement.GetProperty("features");

            if (features.GetArrayLength() == 0)
                return IndicadorNoDisponible($"Trànsit {carretera}", "fas fa-car", "secondary");

            // Seleccionem el tram amb més trànsit (IMD23 màxim)
            int millorImd23 = -1, millorImd22 = -1;
            string millorEstacio = "", millorTram = "";

            foreach (var feature in features.EnumerateArray())
            {
                var attr = feature.GetProperty("attributes");
                var imd23 = attr.TryGetProperty("IMD23", out var v23) && v23.ValueKind == JsonValueKind.Number ? v23.GetInt32() : 0;
                if (imd23 > millorImd23)
                {
                    millorImd23 = imd23;
                    millorImd22 = attr.TryGetProperty("IMD22", out var v22) && v22.ValueKind == JsonValueKind.Number ? v22.GetInt32() : 0;
                    millorEstacio = attr.TryGetProperty("UIM_CODI_ESTACIO", out var est) ? est.GetString() ?? "" : "";
                    var pqi = attr.TryGetProperty("UIM_PQI", out var pqi_) && pqi_.ValueKind == JsonValueKind.Number ? pqi_.GetDouble() : 0;
                    var pqf = attr.TryGetProperty("UIM_PQF", out var pqf_) && pqf_.ValueKind == JsonValueKind.Number ? pqf_.GetDouble() : 0;
                    millorTram = $"pk {pqi / 1000:F1}+{pqf / 1000:F1}";
                }
            }

            if (millorImd23 <= 0)
                return IndicadorNoDisponible($"Trànsit {carretera}", "fas fa-car", "secondary");

            var tendencia = millorImd22 > 0
                ? (millorImd23 > millorImd22 ? " ↑" : millorImd23 < millorImd22 ? " ↓" : " →")
                : "";
            var color = millorImd23 > 5000 ? "danger" : millorImd23 > 2000 ? "warning" : "success";

            return new IndicadorTransparencia
            {
                Titol = $"Intensitat Trànsit – {carretera}",
                Valor = $"{millorImd23:N0} veh/dia{tendencia}",
                Detall = $"Any 2023 · {millorTram} · estació {millorEstacio}",
                Icona = "fas fa-car",
                Color = color,
                Font = "Diputació de Barcelona – SITCAR",
                Any = 2023,
                Estat = "ok"
            };
        }
        catch (Exception ex)
        {
            await _dbLogger.LogWarningAsync($"Error obtenint aforaments IMD: {ex.Message}", "GetAforamentsIMDAsync");
            return IndicadorNoDisponible($"Trànsit {carretera}", "fas fa-car", "secondary");
        }
    }

    private static IndicadorTransparencia IndicadorNoDisponible(string titol, string icona, string color) =>
        new() { Titol = titol, Valor = "–", Icona = icona, Color = color, Estat = "nodata" };
}
