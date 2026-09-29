using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using AppAjuntament.Models;

namespace AppAjuntament.Services;

public class SeuEcService
{
    private readonly HttpClient _httpClient;
    private readonly DatabaseLoggerService _dbLogger;
    private readonly IOptions<AjuntamentSettings> _settings;
    private const string ResourceId = "eb131bb1-f521-4aeb-9004-2fea1f372e89";
    private const string BaseUrl = "http://dadesobertes.seu-e.cat/api/action/datastore_search";

    public SeuEcService(HttpClient httpClient, DatabaseLoggerService dbLogger, IOptions<AjuntamentSettings> settings)
    {
        _httpClient = httpClient;
        _dbLogger = dbLogger;
        _settings = settings;
    }

    public async Task<List<CarrecElecte>> GetRegidorsByCodiEnsAsync(string codiEns)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(codiEns)) return new List<CarrecElecte>();
            // Try with given code first. CKAN expects numeric CODI_ENS (no leading 'P').
            async Task<List<CarrecElecte>> QueryWith(string code)
            {
                var filterObj = new Dictionary<string, object> { ["CODI_ENS"] = (object) (int.TryParse(code, out var n) ? n : (object)code) };
                var filtersJson = JsonSerializer.Serialize(filterObj);
                var url = $"{BaseUrl}?resource_id={ResourceId}&filters={Uri.EscapeDataString(filtersJson)}";
                var resp = await _httpClient.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    await _dbLogger.LogWarningAsync($"Seu-e request failed: {resp.StatusCode} for code={code}", "SeuEcService");
                    return new List<CarrecElecte>();
                }

                var content = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                // CKAN returns success flag and result.records
                if (doc.RootElement.TryGetProperty("success", out var successProp) && successProp.ValueKind == JsonValueKind.False)
                {
                    return new List<CarrecElecte>();
                }

                if (doc.RootElement.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.Object && result.TryGetProperty("records", out var records) && records.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<CarrecElecte>();
                    foreach (var rec in records.EnumerateArray())
                    {
                        var car = MapRecordToCarrec(rec);
                        list.Add(car);
                    }
                    return list.OrderBy(c => c.OrdreCarrec).ThenBy(c => c.Cognom1).ToList();
                }

                return new List<CarrecElecte>();
            }

            // First try raw code
            var primary = await QueryWith(codiEns);
            if (primary != null && primary.Count > 0) return primary;

            // If code has leading non-digits (e.g., 'P825670005'), try stripping non-digits and re-query
            var digits = new string(codiEns.Where(char.IsDigit).ToArray());
            if (!string.IsNullOrEmpty(digits) && digits != codiEns)
            {
                var secondary = await QueryWith(digits);
                if (secondary != null && secondary.Count > 0)
                    return secondary;
            }

            return new List<CarrecElecte>();
        }
        catch (Exception ex)
        {
            await _dbLogger.LogWarningAsync($"Error querying Seu-e regidors: {ex.Message}", "SeuEcService");
            return new List<CarrecElecte>();
        }
    }

    private CarrecElecte MapRecordToCarrec(JsonElement rec)
    {
        string GetByCandidates(params string[] names)
        {
            foreach (var n in names)
            {
                if (rec.TryGetProperty(n, out var p))
                {
                    if (p.ValueKind == JsonValueKind.String)
                        return p.GetString() ?? string.Empty;
                    if (p.ValueKind == JsonValueKind.Number)
                        return p.ToString();
                }
            }
            return string.Empty;
        }

        // Full name usually in NOM_REGIDOR (e.g. "JOAN MARC FLORES RIERA")
        var fullName = GetByCandidates("NOM_REGIDOR", "nom_regidor", "nom", "nom_persona", "NOM_PERSONA");
        string nom = string.Empty;
        string cognom1 = string.Empty;
        string cognom2 = string.Empty;
        if (!string.IsNullOrEmpty(fullName))
        {
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                nom = parts[0];
            }
            else if (parts.Length == 2)
            {
                nom = parts[0];
                cognom1 = parts[1];
            }
            else
            {
                // last two tokens as surnames, rest as given names
                cognom2 = parts[^1];
                cognom1 = parts[^2];
                nom = string.Join(' ', parts.Take(parts.Length - 2));
            }
        }

        var carrec = GetByCandidates("CARREC", "carrec", "carrec_persona");
        var partit = GetByCandidates("PARTIT", "partit", "partit_politic").Trim();
        var email = GetByCandidates("E_MAIL", "e_mail", "email");
        var area = GetByCandidates("AREA", "area");
        var nomEns = GetByCandidates("NOM_ENS", "nom_ens");
        var dataNom = GetByCandidates("DATA_NOMENAMENT", "data_nomenament", "data_nom");
        var sexe = GetByCandidates("SEXE", "sexe");

        var externId = string.Empty;
        if (rec.TryGetProperty("_id", out var idp)) externId = idp.ToString();

        // derive sigles from partit if available (e.g., "ERC - AM" -> "ERC")
        string sigles = string.Empty;
        if (!string.IsNullOrEmpty(partit))
        {
            // take first token that looks like an acronym (letters/digits) before a separator
            var tokens = partit.Split(new[] { '-', '/' , ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length > 0)
            {
                // prefer token composed mostly of letters/digits
                sigles = tokens[0].Trim();
            }
        }

        var car = new CarrecElecte
        {
            Nom = nom,
            Cognom1 = cognom1,
            Cognom2 = cognom2,
            Carrec = carrec,
            Partit = partit,
            Sigles = string.IsNullOrEmpty(sigles) ? string.Empty : sigles,
            Ine = string.Empty,
            Email = email,
            Area = area,
            NomEns = nomEns,
            DataNomenament = dataNom,
            Sexe = sexe,
            ExternId = externId
        };

        // ORDRE can be numeric
        if (rec.TryGetProperty("ORDRE", out var ordreProp) && ordreProp.ValueKind == JsonValueKind.Number)
        {
            if (ordreProp.TryGetInt32(out var ord)) car.Ordre = ord;
        }

        // CODI_ENS is numeric in CKAN; keep it as string in Ine for downstream usage
        if (rec.TryGetProperty("CODI_ENS", out var codiProp))
        {
            car.Ine = codiProp.ToString();
        }

        return car;
    }
}
