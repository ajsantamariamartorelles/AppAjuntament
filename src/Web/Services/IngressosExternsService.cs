using System.Globalization;
using System.Text.Json;
using AppAjuntament.Models;
using AppAjuntament.Models.IngressosExterns;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AppAjuntament.Services;

public class IngressosExternsService
{
    private const string FontFclc = "FCLC";
    private const string FclcUrl = "https://analisi.transparenciacatalunya.cat/resource/ka4a-fyht.json";
    private const string PoblacioUrl = "https://api.idescat.cat/pob/v1/cerca.json?tipus=mun&posicio=";
    // Obligacions reconegudes PENDENTS de pagament de la Generalitat (Secretaria de Governs Locals) als ens locals.
    // Import important: aquest dataset NO representa ingressos ja cobrats, sinó deutes encara pendents.
    private const string PendentsPagamentUrl = "https://analisi.transparenciacatalunya.cat/resource/4w8y-gf8x.json";
    private readonly HttpClient _httpClient;
    private readonly IOptions<AjuntamentSettings> _settings;
    private readonly IDbContextFactory<GestorSubvencionsContext> _dbFactory;
    private readonly IMemoryCache _cache;

    public IngressosExternsService(HttpClient httpClient, IOptions<AjuntamentSettings> settings,
        IDbContextFactory<GestorSubvencionsContext> dbFactory, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _settings = settings;
        _dbFactory = dbFactory;
        _cache = cache;
    }

    // La pantalla llegeix només les dades persistides; cap API externa s'invoca des d'aquí.
    public async Task<IngressosExternsDashboard> ObtenirFonsCooperacioAsync(CancellationToken cancellationToken = default)
    {
        var codiMunicipi = (_settings.Value.CodiINE6 ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(codiMunicipi)) return new IngressosExternsDashboard();

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var exercici = await db.IngressosExternsFonsCooperacio
            .Where(x => x.Font == FontFclc).Select(x => (int?)x.Exercici).MaxAsync(cancellationToken) ?? 0;
        if (exercici == 0) return new IngressosExternsDashboard();

        var rows = await db.IngressosExternsFonsCooperacio.AsNoTracking()
            .Where(x => x.Font == FontFclc && x.Exercici == exercici && x.Poblacio != null && x.Poblacio > 0)
            .ToListAsync(cancellationToken);
        var municipis = rows.Select(x => ToDashboardRow(x, x.CodiIne6 == codiMunicipi)).ToList();
        var actual = municipis.FirstOrDefault(x => x.EsMunicipiActual);
        var historic = await db.IngressosExternsFonsCooperacio.AsNoTracking()
            .Where(x => x.Font == FontFclc && x.CodiIne6 == codiMunicipi)
            .OrderBy(x => x.Exercici)
            .Select(x => new FonsCooperacioHistoric { Exercici = x.Exercici, Import = x.Import, ImportComplementari = x.ImportComplementari })
            .ToListAsync(cancellationToken);
        if (actual == null) return new IngressosExternsDashboard { Exercici = exercici, Historic = historic, PagamentsPendents = await ObtenirPagamentsPendentsAsync(db, cancellationToken) };

        var similars = municipis.Where(x => x.Poblacio >= actual.Poblacio / 2 && x.Poblacio <= actual.Poblacio * 2)
            .OrderByDescending(x => x.ImportPerHabitant).ToList();
        var comarca = municipis.Where(x => string.Equals(x.Comarca, actual.Comarca, StringComparison.OrdinalIgnoreCase)).ToList();
        return new IngressosExternsDashboard
        {
            Exercici = exercici,
            Municipi = actual,
            MunicipisComparables = similars.Count,
            MedianaMunicipisSimilarsPerHabitant = Mediana(similars.Select(x => x.ImportPerHabitant)),
            MedianaComarcaPerHabitant = Mediana(comarca.Select(x => x.ImportPerHabitant)),
            PosicioPercentil = similars.Count == 0 ? null : Math.Round(100m * similars.Count(x => x.ImportPerHabitant <= actual.ImportPerHabitant) / similars.Count, 0),
            MunicipisSimilars = similars,
            Historic = historic,
            PagamentsPendents = await ObtenirPagamentsPendentsAsync(db, cancellationToken)
        };
    }

    private static async Task<List<PagamentPendent>> ObtenirPagamentsPendentsAsync(GestorSubvencionsContext db, CancellationToken ct) =>
        await db.IngressosExternsPendentsPagament.AsNoTracking()
            .OrderByDescending(x => x.Exercici).ThenBy(x => x.TipusAjut)
            .Select(x => new PagamentPendent { TipusAjut = x.TipusAjut, Exercici = x.Exercici, Import = x.Import, Retingut = x.Retingut, DataPublicacio = x.DataPublicacio })
            .ToListAsync(ct);

    // Invocat únicament pel job de Hangfire: descarrega, calcula i substitueix la fotografia completa.
    public async Task<int> SincronitzarFonsCooperacioAsync(CancellationToken cancellationToken = default)
    {
        var distribucioTask = DescarregarDistribucioAsync(cancellationToken);
        var poblacionsTask = DescarregarPoblacionsAsync(cancellationToken);
        await Task.WhenAll(distribucioTask, poblacionsTask);
        var ara = DateTime.UtcNow;
        var registres = distribucioTask.Result.Select(row => new IngressosExternsFonsCooperacio
        {
            Font = FontFclc, CodiIne6 = row.CodiIne6, Municipi = row.Municipi, Comarca = row.Comarca,
            Exercici = row.Exercici, Import = row.Import, ImportComplementari = row.ImportComplementari,
            Poblacio = poblacionsTask.Result.TryGetValue(row.CodiIne6, out var poblacio) ? poblacio : null,
            SincronitzatUtc = ara
        }).ToList();
        if (registres.Count == 0) throw new InvalidOperationException("La font FCLC no ha retornat registres vàlids.");

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        await db.IngressosExternsFonsCooperacio.Where(x => x.Font == FontFclc).ExecuteDeleteAsync(cancellationToken);
        await db.IngressosExternsFonsCooperacio.AddRangeAsync(registres, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return registres.Count;
    }

    // Invocat únicament pel job de Hangfire: descarrega els pagaments pendents de la Generalitat per al nostre ens.
    public async Task<int> SincronitzarPagamentsPendentsAsync(CancellationToken cancellationToken = default)
    {
        var codiMunicipi = (_settings.Value.CodiINE6 ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(codiMunicipi)) throw new InvalidOperationException("Falta configurar Ajuntament:CodiINE6.");

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var cif = await db.Ens.Where(e => e.INE6 == codiMunicipi).Select(e => e.CIF).FirstOrDefaultAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(cif)) throw new InvalidOperationException("No s'ha trobat el CIF de l'ens per al CodiINE6 configurat.");

        var where = Uri.EscapeDataString($"nifcreditor='{cif}'");
        var json = await _httpClient.GetStringAsync($"{PendentsPagamentUrl}?$limit=5000&$where={where}&$order=data DESC", cancellationToken);
        using var doc = JsonDocument.Parse(json);
        var ara = DateTime.UtcNow;

        // Un mateix tipus_ajut/exercici es publica repetidament mentre resta pendent; ens quedem amb l'últim (data més recent).
        var registres = doc.RootElement.EnumerateArray().Select(ParsePendentPagament).Where(x => x != null).Cast<PendentSourceRow>()
            .GroupBy(x => (x.TipusAjut, x.Exercici)).Select(g => g.OrderByDescending(x => x.Data).First())
            .Select(row => new IngressosExternsPendentPagament
            {
                TipusAjut = row.TipusAjut, Exercici = row.Exercici, Import = row.Import,
                Retingut = row.Retingut, DataPublicacio = row.Data, SincronitzatUtc = ara
            }).ToList();

        await db.IngressosExternsPendentsPagament.ExecuteDeleteAsync(cancellationToken);
        await db.IngressosExternsPendentsPagament.AddRangeAsync(registres, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return registres.Count;
    }

    private static PendentSourceRow? ParsePendentPagament(JsonElement row)
    {
        var tipusAjut = GetString(row, "tipusajut");
        if (string.IsNullOrWhiteSpace(tipusAjut) || !int.TryParse(GetString(row, "exercici"), out var exercici)
            || !decimal.TryParse(GetString(row, "importnet"), NumberStyles.Number, CultureInfo.InvariantCulture, out var import)) return null;
        var data = DateTime.TryParse(GetString(row, "data"), CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : (DateTime?)null;
        var retingut = string.Equals(GetString(row, "retenci"), "Pagament retingut", StringComparison.OrdinalIgnoreCase);
        return new PendentSourceRow(tipusAjut, exercici, import, retingut, data);
    }

    private async Task<List<SourceRow>> DescarregarDistribucioAsync(CancellationToken ct)
    {
        var where = Uri.EscapeDataString("ens_beneficiari='Municipi'");
        var json = await _httpClient.GetStringAsync($"{FclcUrl}?$limit=20000&$where={where}", ct);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.EnumerateArray().Select(ParseFclc).Where(x => x != null).Cast<SourceRow>().ToList();
    }

    private async Task<Dictionary<string, int>> DescarregarPoblacionsAsync(CancellationToken ct)
    {
        var first = await DescarregarPaginaPoblacioAsync(1, ct);
        var pages = (int)Math.Ceiling(first.Total / 50m);
        var pagesRestants = await Task.WhenAll(Enumerable.Range(2, Math.Max(0, pages - 1))
            .Select(page => DescarregarPaginaPoblacioAsync((page - 1) * 50 + 1, ct)));
        return new[] { first }.Concat(pagesRestants).SelectMany(x => x.Poblacions)
            .GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.First().Value);
    }

    private async Task<(int Total, Dictionary<string, int> Poblacions)> DescarregarPaginaPoblacioAsync(int posicio, CancellationToken ct)
    {
        var json = await _httpClient.GetStringAsync(PoblacioUrl + posicio, ct);
        using var doc = JsonDocument.Parse(json);
        var feed = doc.RootElement.GetProperty("feed");
        var data = new Dictionary<string, int>();
        if (feed.TryGetProperty("entry", out var entries)) foreach (var entry in Enumerar(entries))
        {
            var section = entry.GetProperty("cross:DataSet").GetProperty("cross:Section");
            var codi = GetString(section, "AREA");
            if (!section.TryGetProperty("cross:Obs", out var observacions)) continue;
            var totalObs = Enumerar(observacions).FirstOrDefault(x => GetString(x, "SEX") == "T");
            if (!string.IsNullOrWhiteSpace(codi) && totalObs.ValueKind != JsonValueKind.Undefined && int.TryParse(GetString(totalObs, "OBS_VALUE"), out var poblacio)) data[codi] = poblacio;
        }
        return (int.TryParse(GetString(feed, "opensearch:totalResults"), out var total) ? total : 0, data);
    }

    private static SourceRow? ParseFclc(JsonElement row)
    {
        var codi10 = GetString(row, "codi_10");
        if (codi10.Length < 6 || !int.TryParse(GetString(row, "any"), out var exercici) || !decimal.TryParse(GetString(row, "import"), NumberStyles.Number, CultureInfo.InvariantCulture, out var import)) return null;
        // El component complementari canvia de nom segons l'exercici (fclc_supramunicipal / fclc_extraordinari) i no sempre existeix.
        var complementariText = row.TryGetProperty("fclc_supramunicipal", out var supra) ? supra.GetString()
            : row.TryGetProperty("fclc_extraordinari", out var extra) ? extra.GetString() : null;
        decimal? complementari = decimal.TryParse(complementariText, NumberStyles.Number, CultureInfo.InvariantCulture, out var c) ? c : null;
        return new SourceRow(codi10[..6], GetString(row, "municipi"), GetString(row, "comarca"), exercici, import, complementari);
    }

    private static FonsCooperacioMunicipi ToDashboardRow(IngressosExternsFonsCooperacio x, bool actual) => new()
    {
        CodiIne6 = x.CodiIne6, Municipi = x.Municipi, Comarca = x.Comarca ?? string.Empty, Exercici = x.Exercici,
        Import = x.Import, ImportComplementari = x.ImportComplementari, Poblacio = x.Poblacio ?? 0, EsMunicipiActual = actual
    };
    private static IEnumerable<JsonElement> Enumerar(JsonElement element) => element.ValueKind == JsonValueKind.Array ? element.EnumerateArray() : new[] { element };
    private static string GetString(JsonElement element, string property) => element.TryGetProperty(property, out var value) ? value.GetString() ?? string.Empty : string.Empty;
    private static decimal? Mediana(IEnumerable<decimal> values) { var v = values.OrderBy(x => x).ToArray(); return v.Length == 0 ? null : v.Length % 2 == 0 ? (v[v.Length / 2 - 1] + v[v.Length / 2]) / 2 : v[v.Length / 2]; }
    private sealed record SourceRow(string CodiIne6, string Municipi, string Comarca, int Exercici, decimal Import, decimal? ImportComplementari);
    private sealed record PendentSourceRow(string TipusAjut, int Exercici, decimal Import, bool Retingut, DateTime? Data);
}
