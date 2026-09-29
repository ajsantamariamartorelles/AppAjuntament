using AppAjuntament.Models;
using AppAjuntament.Models.Base.Regidor;
using AppAjuntament.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppAjuntament.Jobs;

public class RegidorsImportJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RegidorsImportJob> _logger;

    public RegidorsImportJob(IServiceProvider serviceProvider, ILogger<RegidorsImportJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ImportRegidorsAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<GestorSubvencionsContext>();

            var settings = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AppAjuntament.Models.AjuntamentSettings>>();
            var codiDiba = (settings.Value.CodiDiba ?? string.Empty).Trim();
            var codiIne6 = (settings.Value.CodiINE6 ?? string.Empty).Trim();

            // Always prefer Seu-e as the source for regidors: resolve the ENS code and call SeuEcService
            var seuService = scope.ServiceProvider.GetRequiredService<SeuEcService>();

            // Resolve Ens by INE6 or by configured CodiEns (codiDiba). If we have a local ENS record, prefer its CodiEns.
            var ens = await db.Ens.FirstOrDefaultAsync(e => (e.INE6 != null && e.INE6 == codiIne6) || (e.CodiEns != null && e.CodiEns == codiDiba));
            int? ensId = ens?.Id;

            string? codiEnsToCall = null;
            if (ens != null && !string.IsNullOrEmpty(ens.CodiEns))
                codiEnsToCall = ens.CodiEns.Trim();
            else if (!string.IsNullOrEmpty(codiDiba))
                codiEnsToCall = codiDiba;

            if (string.IsNullOrEmpty(codiEnsToCall))
            {
                _logger.LogInformation("No CODI_ENS (from ENS or settings) available to call Seu-e. Aborting import.");
                return;
            }

            var list = await seuService.GetRegidorsByCodiEnsAsync(codiEnsToCall);
            var returned = list?.Count ?? 0;
            _logger.LogInformation("Called Seu-e for CODI_ENS={CodiEns}, returned {Count} records.", codiEnsToCall, returned);
            if (list == null || list.Count == 0)
            {
                _logger.LogInformation("Seu-e returned no regidors for CODI_ENS={CodiEns}. Aborting import.", codiEnsToCall);
                return;
            }

            // Prepare existing regidors lookup by ExternId to update instead of deleting
            List<Regidor> existingForEns;
            if (ensId != null)
            {
                existingForEns = await db.Regidors.Where(r => r.EnsId == ensId).ToListAsync();
            }
            else if (!string.IsNullOrEmpty(codiDiba))
            {
                existingForEns = await db.Regidors.Where(r => r.CodiEns == codiDiba).ToListAsync();
            }
            else
            {
                existingForEns = new List<Regidor>();
            }

            var existingByExtern = existingForEns
                .Where(r => !string.IsNullOrEmpty(r.ExternId))
                .ToDictionary(r => r.ExternId!, r => r);

            // Regidors previs a la introducció d'ExternId: fem fallback per nom complet perquè el
            // primer cop que arriba un ExternId no es crei un duplicat en lloc d'actualitzar el registre històric.
            var existingByName = existingForEns
                .Where(r => string.IsNullOrEmpty(r.ExternId))
                .GroupBy(r => NormalizeName(r.NomComplet ?? r.Nom))
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var c in list)
            {
                var matchedByName = string.IsNullOrEmpty(c.ExternId) ? null
                    : existingByName.TryGetValue(NormalizeName(c.NomComplet ?? c.Nom), out var byName) ? byName : null;

                // If we have an ExternId and it matches an existing regidor, update that regidor instead of creating or deleting
                if ((!string.IsNullOrEmpty(c.ExternId) && existingByExtern.TryGetValue(c.ExternId, out var existing)) ||
                    (existing = matchedByName) != null)
                {
                    existing.NomComplet = !string.IsNullOrEmpty(c.NomComplet)
                        ? c.NomComplet
                        : existing.NomComplet ?? string.Join(" ", new[] { c.Nom, c.Cognom1, c.Cognom2 }.Where(s => !string.IsNullOrEmpty(s)).Select(s => s!.Trim()));

                    existing.Nom = !string.IsNullOrEmpty(c.Nom) ? c.Nom : existing.Nom;
                    existing.Cognom1 = !string.IsNullOrEmpty(c.Cognom1) ? c.Cognom1 : existing.Cognom1;
                    existing.Cognom2 = !string.IsNullOrEmpty(c.Cognom2) ? c.Cognom2 : existing.Cognom2;

                    existing.Carrec = c.Carrec;
                    existing.Partit = c.Partit;
                    existing.Sigles = string.IsNullOrEmpty(c.Sigles) ? existing.Sigles : c.Sigles;
                    existing.Area = string.IsNullOrEmpty(c.Area) ? existing.Area : c.Area;
                    existing.DataNomenament = TryParseDate(c.DataNomenament) ?? existing.DataNomenament;
                    existing.Email = string.IsNullOrEmpty(c.Email) ? existing.Email : c.Email;
                    existing.Orde = c.OrdreCarrec;
                    existing.CodiEns = string.IsNullOrEmpty(c.Ine) ? (string.IsNullOrEmpty(existing.CodiEns) ? codiDiba : existing.CodiEns) : c.Ine;
                    existing.NomEns = string.IsNullOrEmpty(c.NomEns) ? existing.NomEns : c.NomEns;
                    existing.EnsId = ensId ?? existing.EnsId;
                    existing.ExternId = c.ExternId;

                    if (!string.IsNullOrEmpty(c.Sexe))
                    {
                        var code = c.Sexe.Trim()[0];
                        var sexe = await db.Sexes.FirstOrDefaultAsync(s => s.Codi == code);
                        if (sexe != null)
                            existing.SexeId = sexe.Id;
                    }

                    // EF is tracking 'existing' so changes will be saved on SaveChangesAsync
                }
                else
                {
                    var reg = new Regidor
                    {
                        Nom = string.IsNullOrEmpty(c.Nom) ? (c.NomComplet ?? string.Empty) : c.Nom,
                        NomComplet = string.IsNullOrEmpty(c.NomComplet) ? string.Join(" ", new[] { c.Nom, c.Cognom1, c.Cognom2 }.Where(s => !string.IsNullOrEmpty(s)).Select(s => s!.Trim())) : c.NomComplet,
                        Cognom1 = string.IsNullOrEmpty(c.Cognom1) ? null : c.Cognom1,
                        Cognom2 = string.IsNullOrEmpty(c.Cognom2) ? null : c.Cognom2,
                        Carrec = c.Carrec,
                        Partit = c.Partit,
                        Sigles = string.IsNullOrEmpty(c.Sigles) ? null : c.Sigles,
                        Area = string.IsNullOrEmpty(c.Area) ? null : c.Area,
                        DataNomenament = TryParseDate(c.DataNomenament),
                        Email = string.IsNullOrEmpty(c.Email) ? null : c.Email,
                        Orde = c.OrdreCarrec,
                        CodiEns = string.IsNullOrEmpty(c.Ine) ? codiDiba : c.Ine,
                        NomEns = string.IsNullOrEmpty(c.NomEns) ? null : c.NomEns,
                        EnsId = ensId
                    };

                    if (!string.IsNullOrEmpty(c.ExternId))
                        reg.ExternId = c.ExternId;

                    if (!string.IsNullOrEmpty(c.Sexe))
                    {
                        var code = c.Sexe.Trim()[0];
                        var sexe = await db.Sexes.FirstOrDefaultAsync(s => s.Codi == code);
                        if (sexe != null)
                            reg.SexeId = sexe.Id;
                    }
                    db.Regidors.Add(reg);
                }
            }

            await db.SaveChangesAsync();
            _logger.LogInformation("Regidors import completed successfully. Inserted {Count} items.", list.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing regidors");
        }
    }

    private static string NormalizeName(string? name) =>
        string.IsNullOrWhiteSpace(name) ? string.Empty : string.Join(" ", name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();

    private DateTime? TryParseDate(string dateStr)
    {
        if (string.IsNullOrEmpty(dateStr)) return null;
        if (DateTime.TryParse(dateStr, out var dt)) return dt;
        // try trimming time / iso formats
        var idx = dateStr.IndexOf('T');
        if (idx > 0)
            dateStr = dateStr.Substring(0, idx);
        if (DateTime.TryParse(dateStr, out dt)) return dt;
        return null;
    }
}
