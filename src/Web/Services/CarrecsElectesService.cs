using AppAjuntament.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AppAjuntament.Services;

public class CarrecElecte
{
    [JsonPropertyName("nom")]
    public string Nom { get; set; } = "";

    [JsonPropertyName("cognom1_persona")]
    public string Cognom1 { get; set; } = "";

    [JsonPropertyName("cognom2_persona")]
    public string Cognom2 { get; set; } = "";

    [JsonPropertyName("carrec_persona")]
    public string Carrec { get; set; } = "";

    [JsonPropertyName("partit_politic")]
    public string Partit { get; set; } = "";

    [JsonPropertyName("sigles_partit_politic")]
    public string Sigles { get; set; } = "";

    [JsonPropertyName("ine")]
    public string Ine { get; set; } = "";

    [JsonPropertyName("e_mail")]
    public string Email { get; set; } = "";

    [JsonPropertyName("area")]
    public string Area { get; set; } = "";

    [JsonPropertyName("nom_ens")]
    public string NomEns { get; set; } = "";

    [JsonPropertyName("data_nomenament")]
    public string DataNomenament { get; set; } = "";

    [JsonPropertyName("sexe")]
    public string Sexe { get; set; } = "";

    [JsonPropertyName("_id")]
    public string ExternId { get; set; } = "";

    [JsonPropertyName("ordre")]
    public int? Ordre { get; set; }

    public string NomComplet => $"{Nom} {Cognom1} {Cognom2}".Trim();

    public int OrdreCarrec => Ordre.HasValue ? Ordre.Value : Carrec switch
    {
        var c when c.StartsWith("Alcalde") || c.StartsWith("Alcaldessa") => 0,
        var c when c.Contains("1r.") || c.Contains("1a.") => 1,
        var c when c.Contains("2n.") || c.Contains("2a.") => 2,
        var c when c.Contains("3r.") || c.Contains("3a.") => 3,
        _ => 10
    };

    public string IconaCarrec => OrdreCarrec == 0 ? "fas fa-star" : "fas fa-user-tie";
    public string ColorCarrec => OrdreCarrec == 0 ? "warning" : "secondary";
}

public class CarrecsElectesService
{
    private readonly HttpClient _httpClient;
    private readonly DatabaseLoggerService _dbLogger;
    private readonly IOptions<AjuntamentSettings> _settings;
    private readonly IDbContextFactory<AppAjuntament.Models.GestorSubvencionsContext> _dbFactory;
    private readonly SeuEcService _seuService;

    public CarrecsElectesService(HttpClient httpClient, DatabaseLoggerService dbLogger, IOptions<AjuntamentSettings> settings,
        IDbContextFactory<AppAjuntament.Models.GestorSubvencionsContext> dbFactory, SeuEcService seuService)
    {
        _httpClient = httpClient;
        _dbLogger = dbLogger;
        _settings = settings;
        _dbFactory = dbFactory;
        _seuService = seuService;
    }

    public async Task<List<CarrecElecte>> GetCarrecsElectesAsync()
    {
        var codiDiba = (_settings.Value.CodiDiba ?? "08256").Trim();
        var codiIne6 = (_settings.Value.CodiINE6 ?? string.Empty).Trim();
        try
        {
            // Preferir registres locals a la BD si existeixen (per mostrar regidors desats/ajustats)
            try
            {
                using var ctx = _dbFactory.CreateDbContext();
                var ens = await ctx.Ens.FirstOrDefaultAsync(e => (e.INE6 != null && e.INE6 == codiIne6) || (e.CodiEns != null && e.CodiEns == codiDiba));
                int? ensId = ens?.Id;

                var regs = await ctx.Regidors
                    .Include(r => r.Sexe)
                    .Include(r => r.Ens)
                    .Where(r => (r.EnsId != null && ensId != null && r.EnsId == ensId) ||
                                ((r.CodiEns ?? string.Empty).Trim() == codiDiba) ||
                                (!string.IsNullOrEmpty(codiIne6) && (r.CodiEns ?? string.Empty).Trim() == codiIne6))
                    .ToListAsync();

                if (regs != null && regs.Count > 0)
                {
                    var mapped = regs.Select(r => new CarrecElecte
                    {
                        Nom = r.Nom ?? string.Empty,
                        Cognom1 = r.Cognom1 ?? string.Empty,
                        Cognom2 = r.Cognom2 ?? string.Empty,
                        Carrec = r.Carrec ?? string.Empty,
                        Partit = r.Partit ?? string.Empty,
                        Sigles = r.Sigles ?? string.Empty,
                        Ine = r.CodiEns ?? string.Empty,
                        Email = r.Email ?? string.Empty,
                        Area = r.Area ?? string.Empty,
                        NomEns = r.NomEns ?? string.Empty,
                        DataNomenament = r.DataNomenament?.ToString() ?? string.Empty,
                        Sexe = r.Sexe != null ? r.Sexe.Codi.ToString() : string.Empty,
                        ExternId = !string.IsNullOrEmpty(r.ExternId) ? r.ExternId : r.Id.ToString(),
                        Ordre = r.Orde
                    })
                    .OrderBy(c => c.OrdreCarrec)
                    .ThenBy(c => c.Nom)
                    .ToList();

                    return mapped;
                }
            }
            catch (Exception) { /* ignore and continue to remote fetch */ }

            // If no local DB records, query the Seu-e (preferred source)
            try
            {
                var seuList = await _seuService.GetRegidorsByCodiEnsAsync(codiDiba);
                if ((seuList?.Count ?? 0) > 0)
                {
                    return seuList.OrderBy(c => c.OrdreCarrec).ThenBy(c => c.Cognom1).ToList();
                }

                // Try fallback with INE6 if configured
                if (!string.IsNullOrEmpty(codiIne6))
                {
                    var seuList2 = await _seuService.GetRegidorsByCodiEnsAsync(codiIne6);
                    if ((seuList2?.Count ?? 0) > 0)
                        return seuList2.OrderBy(c => c.OrdreCarrec).ThenBy(c => c.Cognom1).ToList();
                }

                return new List<CarrecElecte>();
            }
            catch (Exception) { /* ignore and return empty */ }

            return new List<CarrecElecte>();
        }
        catch (Exception ex)
        {
            await _dbLogger.LogWarningAsync($"Error obtenint càrrecs electes: {ex.Message}", "GetCarrecsElectesAsync");
            return new List<CarrecElecte>();
        }
    }

    // DIBA extraction/pagination removed — using Seu-e as data source
}
