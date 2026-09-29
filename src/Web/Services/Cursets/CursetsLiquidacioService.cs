using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using AppAjuntament.Models;
using AppAjuntament.Models.Cursets;
using AppAjuntament.Models.Cursets.Dto;

namespace AppAjuntament.Services.Cursets
{
    /// <summary>
    /// Liquidacions dels cursets amb cost: es factura per dia de classe impartit
    /// (sessió <see cref="EstatSessio.Tancada"/>) a totes les alumnes apuntades, hi
    /// hagin anat o no, al preu segons empadronament i respectant
    /// <see cref="AlumneCurset.DataAlta"/>/<see cref="AlumneCurset.DataBaixa"/>.
    ///
    /// - <see cref="GetLiquidacioAsync"/>: informe (sense registre) per curset.
    /// - Remeses: emissió oficial d'autoliquidacions registrades (número correlatiu,
    ///   estat) per (alumna × curset × període) i generació de PDFs (individual + ZIP).
    /// </summary>
    public interface ICursetsLiquidacioService
    {
        Task<LiquidacioCursetDto> GetLiquidacioAsync(int cursetId, DateTime dataInici, DateTime dataFi);

        Task<PreviewRemesaDto> PreviewRemesaAsync(PeriodeLiquidacioRequest req);
        Task<EmetreRemesaResultDto> EmetreRemesaAsync(EmetreRemesaRequest req, int? userId);

        Task<List<RemesaDto>> GetRemesesAsync();
        Task<RemesaDetallDto?> GetRemesaAsync(int remesaId);

        Task<byte[]> GetPdfLiquidacioAsync(int liquidacioId);
        Task<byte[]> GetPdfAlumnaAsync(int remesaId, int alumneId);
        Task<byte[]> GetZipRemesaAsync(int remesaId);

        Task MarcarCobradaAsync(int liquidacioId, DateTime data);
        Task AnullarAsync(int liquidacioId, string motiu);

        Task<LiquidacioConfigDto> GetConfigAsync();
        Task DesarConfigAsync(LiquidacioConfigDto dto);
    }

    public class CursetsLiquidacioService : ICursetsLiquidacioService
    {
        private static readonly CultureInfo Ca = CultureInfo.GetCultureInfo("ca-ES");
        private static readonly string[] MesosCurt =
            { "GEN", "FEB", "MAR", "ABR", "MAI", "JUN", "JUL", "AGO", "SET", "OCT", "NOV", "DES" };
        private static readonly string[] DiesSetmana =
            { "Diumenge", "Dilluns", "Dimarts", "Dimecres", "Dijous", "Divendres", "Dissabte" };

        private readonly GestorSubvencionsContext _context;
        private readonly IWebHostEnvironment _env;
        private byte[]? _escutCache;

        public CursetsLiquidacioService(GestorSubvencionsContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ==================== Informe per curset (existent) ====================

        public async Task<LiquidacioCursetDto> GetLiquidacioAsync(int cursetId, DateTime dataInici, DateTime dataFi)
        {
            var dInici = dataInici.Date;
            var dFi = dataFi.Date;
            if (dFi < dInici)
                throw new ArgumentException("La data de fi ha de ser posterior o igual a la data d'inici.", nameof(dataFi));

            var curset = await _context.Cursets
                .Include(c => c.TipusCurset)
                .FirstOrDefaultAsync(c => c.Id == cursetId)
                ?? throw new ArgumentException("El curset indicat no existeix.", nameof(cursetId));

            var (linies, numSessionsFetes) = await CalcularLiniesCursetAsync(cursetId, dInici, dFi);

            return new LiquidacioCursetDto
            {
                CursetId = curset.Id,
                CursetTitol = curset.Titol,
                DataInici = dInici,
                DataFi = dFi,
                NumSessionsFetes = numSessionsFetes,
                Alumnes = linies
                    .Select(l => new LiquidacioAlumnaDto
                    {
                        AlumneId = l.AlumneId,
                        NomComplet = l.NomComplet,
                        Empadronat = l.Empadronat,
                        NumSessions = l.NumSessions,
                        PreuPerSessio = l.Preu,
                        ImportTotal = l.Import
                    })
                    .OrderBy(a => a.NomComplet)
                    .ToList(),
                ImportTotal = linies.Sum(l => l.Import)
            };
        }

        // ==================== Càlcul base (compartit) ====================

        private sealed class LiniaCalcul
        {
            public int AlumneId { get; init; }
            public string NomComplet { get; init; } = string.Empty;
            public string? Nif { get; init; }
            public string? Adreca { get; init; }
            public bool Empadronat { get; init; }
            public int NumSessions { get; set; }
            public decimal Preu { get; init; }
            public decimal Import => NumSessions * Preu;
            public List<DateTime> Dates { get; init; } = new();
            /// <summary>Camps de la persona interessada que falten per poder liquidar (buit = OK).</summary>
            public List<string> DadesFalten { get; init; } = new();
        }

        /// <summary>Línies de cost per alumna d'un curset en un període (nre. sessions Tancada × preu).</summary>
        private async Task<(List<LiniaCalcul> Linies, int NumSessionsFetes)> CalcularLiniesCursetAsync(
            int cursetId, DateTime dInici, DateTime dFi)
        {
            var curset = await _context.Cursets.FirstAsync(c => c.Id == cursetId);

            var sessionsFetes = await _context.CursetsSessions
                .Where(s => s.CursetId == cursetId
                    && s.Estat == EstatSessio.Tancada
                    && s.Data >= dInici && s.Data <= dFi)
                .Select(s => s.Data)
                .ToListAsync();

            var inscripcions = await _context.AlumnesCursets
                .Where(ac => ac.CursetId == cursetId
                    && (ac.Estat == EstatInscripcio.Admesa || ac.Estat == EstatInscripcio.Baixa)
                    && ac.DataAlta <= dFi
                    && (ac.DataBaixa == null || ac.DataBaixa >= dInici))
                .Include(ac => ac.Alumne)
                    .ThenInclude(a => a!.Tercer)
                .ToListAsync();

            var linies = new List<LiniaCalcul>();
            foreach (var inscripcio in inscripcions)
            {
                var alumne = inscripcio.Alumne;
                if (alumne?.Tercer == null)
                    continue;

                var preu = alumne.Empadronat ? curset.PreuPerSessioEmpadronat : curset.PreuPerSessioNoEmpadronat;

                var dates = sessionsFetes
                    .Where(data => data >= inscripcio.DataAlta.Date
                        && (inscripcio.DataBaixa == null || data <= inscripcio.DataBaixa.Value.Date))
                    .OrderBy(d => d)
                    .ToList();

                if (dates.Count == 0)
                    continue;

                linies.Add(new LiniaCalcul
                {
                    AlumneId = alumne.Id,
                    NomComplet = alumne.Tercer.NomComplet,
                    Nif = alumne.Tercer.DNI,
                    Adreca = ComposaAdreca(alumne.Tercer),
                    Empadronat = alumne.Empadronat,
                    NumSessions = dates.Count,
                    Preu = preu,
                    Dates = dates,
                    DadesFalten = LiquidacioDades.QueFalten(
                        alumne.Tercer.DNI, alumne.Tercer.Adreca, alumne.Tercer.CodiPostal, alumne.Tercer.Poblacio)
                });
            }

            return (linies, sessionsFetes.Count);
        }

        /// <summary>El curset té ordenança / tarifa (pròpia, o la per defecte de la configuració).
        /// No és obligatòria per liquidar; només serveix per avisar.</summary>
        private static bool OrdenancaOk(Curset curset, LiquidacioConfig config) =>
            !string.IsNullOrWhiteSpace(curset.OrdenancaLiquidacio) || !string.IsNullOrWhiteSpace(config.OrdenancaTarifa);

        private static bool EsEmetible(LiquidacioLiniaPreviewDto l) =>
            !l.JaLiquidada && l.DadesCompletes;

        private static string? ComposaAdreca(AppAjuntament.Models.Tercers.Tercer t)
        {
            var parts = new[]
            {
                t.Adreca,
                string.Join(" ", new[] { t.CodiPostal, t.Poblacio }.Where(s => !string.IsNullOrWhiteSpace(s)))
            }.Where(s => !string.IsNullOrWhiteSpace(s));
            var s2 = string.Join(", ", parts);
            return string.IsNullOrWhiteSpace(s2) ? null : s2;
        }

        // ==================== Període ====================

        private sealed record PeriodeResolt(
            DateTime Inici, DateTime Fi, string CodiPeriode, string Etiqueta, string Cadencia, int Any);

        private static PeriodeResolt ResoldrePeriode(PeriodeLiquidacioRequest req)
        {
            var cadencia = string.IsNullOrWhiteSpace(req.Cadencia) ? "Trimestral" : req.Cadencia.Trim();
            var any = req.Any > 0 ? req.Any : DateTime.Today.Year;

            switch (cadencia)
            {
                case "Trimestral":
                {
                    var t = req.Periode is >= 1 and <= 4 ? req.Periode!.Value : (DateTime.Today.Month - 1) / 3 + 1;
                    var inici = new DateTime(any, (t - 1) * 3 + 1, 1);
                    var fi = inici.AddMonths(3).AddDays(-1);
                    var ord = t switch { 1 => "1r", 2 => "2n", 3 => "3r", _ => "4t" };
                    return new PeriodeResolt(inici, fi, $"{t}T", $"{ord} trimestre {any}", cadencia, any);
                }
                case "Mensual":
                {
                    var m = req.Periode is >= 1 and <= 12 ? req.Periode!.Value : DateTime.Today.Month;
                    var inici = new DateTime(any, m, 1);
                    var fi = inici.AddMonths(1).AddDays(-1);
                    var nomMes = Ca.DateTimeFormat.GetMonthName(m);
                    nomMes = char.ToUpper(nomMes[0], Ca) + nomMes[1..];
                    return new PeriodeResolt(inici, fi, MesosCurt[m - 1], $"{nomMes} {any}", cadencia, any);
                }
                case "Setmanal":
                {
                    var w = req.Periode is >= 1 and <= 53 ? req.Periode!.Value : ISOWeek.GetWeekOfYear(DateTime.Today);
                    var inici = ISOWeek.ToDateTime(any, w, DayOfWeek.Monday);
                    var fi = inici.AddDays(6);
                    return new PeriodeResolt(inici, fi, $"S{w:00}",
                        $"Setmana {w} de {any} ({inici:dd/MM}–{fi:dd/MM/yyyy})", cadencia, any);
                }
                default: // Lliure
                {
                    var inici = (req.DataInici ?? DateTime.Today).Date;
                    var fi = (req.DataFi ?? inici).Date;
                    if (fi < inici) (inici, fi) = (fi, inici);
                    return new PeriodeResolt(inici, fi, "LLIURE",
                        $"{inici:dd/MM/yyyy}–{fi:dd/MM/yyyy}", "Lliure", inici.Year);
                }
            }
        }

        // ==================== Previsualització ====================

        public async Task<PreviewRemesaDto> PreviewRemesaAsync(PeriodeLiquidacioRequest req)
        {
            var p = ResoldrePeriode(req);
            var config = await GetOrCreateConfigAsync();

            var cursets = await CursetsDelPeriodeAsync(req.CursetIds, p.Inici, p.Fi);

            var result = new PreviewRemesaDto
            {
                Cadencia = p.Cadencia,
                CodiPeriode = p.CodiPeriode,
                PeriodeEtiqueta = p.Etiqueta,
                DataInici = p.Inici,
                DataFi = p.Fi
            };

            foreach (var curset in cursets)
            {
                var (linies, _) = await CalcularLiniesCursetAsync(curset.Id, p.Inici, p.Fi);
                var facturables = linies.Where(l => l.Import > 0m).ToList();
                if (facturables.Count == 0)
                    continue;

                if (string.IsNullOrWhiteSpace(curset.TipusCurset?.CodiLiquidacio) || string.IsNullOrWhiteSpace(curset.CodiLiquidacio))
                    result.Avisos.Add($"El curset «{curset.Titol}» no té codi de liquidació definit; s'usarà un codi generat automàticament.");

                var ordenancaOk = OrdenancaOk(curset, config);
                if (!ordenancaOk)
                    result.CursetsSenseOrdenanca.Add(curset.Titol);

                var existents = await _context.CursetsLiquidacions
                    .Where(l => l.CursetId == curset.Id
                        && l.Estat != EstatLiquidacio.Anullada
                        && l.PeriodeInici <= p.Fi && l.PeriodeFi >= p.Inici)
                    .ToListAsync();

                foreach (var l in facturables)
                {
                    var ex = existents
                        .Where(e => e.AlumneId == l.AlumneId)
                        .OrderByDescending(e => e.Id)
                        .FirstOrDefault();

                    result.Linies.Add(new LiquidacioLiniaPreviewDto
                    {
                        AlumneId = l.AlumneId,
                        AlumneNom = l.NomComplet,
                        CursetId = curset.Id,
                        CursetTitol = curset.Titol,
                        Empadronat = l.Empadronat,
                        NumSessions = l.NumSessions,
                        PreuPerSessio = l.Preu,
                        Import = l.Import,
                        JaLiquidada = ex != null,
                        NumeroExistent = ex?.Numero,
                        EstatExistent = ex?.Estat.ToString(),
                        DadesCompletes = l.DadesFalten.Count == 0,
                        DadesFalten = l.DadesFalten.Count == 0 ? null : string.Join(", ", l.DadesFalten),
                        CursetOrdenancaOk = ordenancaOk
                    });
                }
            }

            result.Linies = result.Linies
                .OrderBy(x => x.CursetTitol).ThenBy(x => x.AlumneNom)
                .ToList();
            result.CursetsSenseOrdenanca = result.CursetsSenseOrdenanca.Distinct().OrderBy(s => s).ToList();
            result.NumNoves = result.Linies.Count(x => EsEmetible(x));
            result.NumJaLiquidades = result.Linies.Count(x => x.JaLiquidada);
            result.NumBloquejades = result.Linies.Count(x => !x.JaLiquidada && !x.DadesCompletes);
            result.ImportNoves = result.Linies.Where(x => EsEmetible(x)).Sum(x => x.Import);
            result.AlumnesIncompletes = result.Linies
                .Where(x => !x.DadesCompletes)
                .GroupBy(x => x.AlumneId)
                .Select(g => new AlumnaIncompletaDto
                {
                    AlumneId = g.Key,
                    Nom = g.First().AlumneNom,
                    DadesFalten = g.First().DadesFalten ?? string.Empty
                })
                .OrderBy(a => a.Nom)
                .ToList();
            return result;
        }

        private async Task<List<Curset>> CursetsDelPeriodeAsync(List<int>? cursetIds, DateTime dInici, DateTime dFi)
        {
            var query = _context.Cursets.Include(c => c.TipusCurset).AsQueryable();

            if (cursetIds is { Count: > 0 })
            {
                query = query.Where(c => cursetIds.Contains(c.Id));
            }
            else
            {
                query = query.Where(c => c.Actiu
                    && _context.CursetsSessions.Any(s => s.CursetId == c.Id
                        && s.Estat == EstatSessio.Tancada
                        && s.Data >= dInici && s.Data <= dFi));
            }

            return await query.OrderBy(c => c.Nom).ToListAsync();
        }

        // ==================== Emissió de la remesa ====================

        public async Task<EmetreRemesaResultDto> EmetreRemesaAsync(EmetreRemesaRequest req, int? userId)
        {
            var p = ResoldrePeriode(req);
            var referTot = string.Equals(req.Mode, "ReferTot", StringComparison.OrdinalIgnoreCase);
            var config = await GetOrCreateConfigAsync();

            await using var tx = await _context.Database.BeginTransactionAsync();

            var cursets = await CursetsDelPeriodeAsync(req.CursetIds, p.Inici, p.Fi);

            // Candidates: (curset, linia de càlcul)
            var candidats = new List<(Curset Curset, LiniaCalcul Linia)>();
            var omeses = 0;
            var anullades = 0;
            var bloquejades = 0;
            var incompletes = new Dictionary<int, AlumnaIncompletaDto>();
            var cursetsSenseOrdenanca = new List<string>();

            foreach (var curset in cursets)
            {
                var (linies, _) = await CalcularLiniesCursetAsync(curset.Id, p.Inici, p.Fi);
                var facturables = linies.Where(l => l.Import > 0m).ToList();
                if (facturables.Count == 0)
                    continue;

                // L'ordenança / tarifa no és obligatòria: si no en té, es liquida
                // igualment i només s'avisa (l'autoliquidació sortirà sense aquesta dada).
                if (!OrdenancaOk(curset, config))
                    cursetsSenseOrdenanca.Add(curset.Titol);

                var existents = await _context.CursetsLiquidacions
                    .Where(l => l.CursetId == curset.Id
                        && l.Estat != EstatLiquidacio.Anullada
                        && l.PeriodeInici <= p.Fi && l.PeriodeFi >= p.Inici)
                    .ToListAsync();

                var reemetre = new List<LiniaCalcul>();
                foreach (var l in facturables)
                {
                    // Sense les dades bàsiques de la persona interessada no es pot
                    // emetre l'autoliquidació: se salta i s'avisa.
                    if (l.DadesFalten.Count > 0)
                    {
                        bloquejades++;
                        incompletes[l.AlumneId] = new AlumnaIncompletaDto
                        {
                            AlumneId = l.AlumneId,
                            Nom = l.NomComplet,
                            DadesFalten = string.Join(", ", l.DadesFalten)
                        };
                        continue;
                    }

                    var jaHiEs = existents.Any(e => e.AlumneId == l.AlumneId);
                    if (jaHiEs && !referTot)
                    {
                        omeses++;
                        continue;
                    }
                    candidats.Add((curset, l));
                    reemetre.Add(l);
                }

                if (referTot)
                {
                    var aAnullar = existents
                        .Where(e => reemetre.Any(f => f.AlumneId == e.AlumneId))
                        .ToList();
                    foreach (var e in aAnullar)
                    {
                        e.Estat = EstatLiquidacio.Anullada;
                        e.DataAnullacio = DateTime.Now;
                        e.MotiuAnullacio = "Reemesa en una remesa posterior";
                        e.UpdatedAt = DateTime.Now;
                        anullades++;
                    }
                }
            }

            if (candidats.Count == 0)
            {
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return new EmetreRemesaResultDto
                {
                    RemesaId = 0,
                    NumEmeses = 0,
                    NumAnullades = anullades,
                    NumOmeses = omeses,
                    NumBloquejades = bloquejades,
                    ImportTotal = 0m,
                    AlumnesIncompletes = incompletes.Values.OrderBy(a => a.Nom).ToList(),
                    CursetsSenseOrdenanca = cursetsSenseOrdenanca.Distinct().OrderBy(s => s).ToList()
                };
            }

            var remesa = new LiquidacioRemesa
            {
                Descripcio = string.IsNullOrWhiteSpace(req.Descripcio) ? p.Etiqueta : req.Descripcio.Trim(),
                CadenciaTipus = p.Cadencia,
                CodiPeriode = p.CodiPeriode,
                PeriodeEtiqueta = p.Etiqueta,
                PeriodeInici = p.Inici,
                PeriodeFi = p.Fi,
                Expedient = ResoldreExpedient(config.ExpedientPatro, p),
                DataCreacio = DateTime.Now,
                CreatedByUserId = userId
            };
            _context.CursetsLiquidacionsRemeses.Add(remesa);
            await _context.SaveChangesAsync();

            var clau = $"{p.Any}-{p.CodiPeriode}";
            var comptador = await _context.CursetsLiquidacionsComptador.FirstOrDefaultAsync(c => c.Clau == clau);
            if (comptador == null)
            {
                comptador = new LiquidacioComptador { Clau = clau, Ultim = 0 };
                _context.CursetsLiquidacionsComptador.Add(comptador);
            }

            var ordenats = candidats
                .OrderBy(c => CodiCursetDe(c.Curset))
                .ThenBy(c => c.Linia.NomComplet)
                .ToList();

            var ara = DateTime.Now;
            decimal total = 0m;
            foreach (var (curset, linia) in ordenats)
            {
                comptador.Ultim++;
                var codiServei = CodiServeiDe(curset);
                var codiCurset = CodiCursetDe(curset);
                var numero = $"{codiServei}-{p.CodiPeriode}-{codiCurset}-{comptador.Ultim:000}";
                var concepte = ResoldreConcepte(config.ConceptePatro, curset, p);

                _context.CursetsLiquidacions.Add(new Liquidacio
                {
                    Numero = numero,
                    Serie = comptador.Ultim,
                    RemesaId = remesa.Id,
                    AlumneId = linia.AlumneId,
                    CursetId = curset.Id,
                    CodiServei = codiServei,
                    CodiPeriode = p.CodiPeriode,
                    CodiCurset = codiCurset,
                    PeriodeInici = p.Inici,
                    PeriodeFi = p.Fi,
                    PeriodeEtiqueta = p.Etiqueta,
                    NumSessions = linia.NumSessions,
                    PreuPerSessio = linia.Preu,
                    Import = linia.Import,
                    Empadronat = linia.Empadronat,
                    Estat = EstatLiquidacio.Emesa,
                    DataEmissio = ara,
                    AlumneNomComplet = linia.NomComplet,
                    AlumneNif = linia.Nif,
                    AlumneAdreca = linia.Adreca,
                    CursetTitol = curset.Titol,
                    ConcepteText = concepte,
                    OrdenancaText = !string.IsNullOrWhiteSpace(curset.OrdenancaLiquidacio)
                        ? curset.OrdenancaLiquidacio!.Trim()
                        : (string.IsNullOrWhiteSpace(config.OrdenancaTarifa) ? null : config.OrdenancaTarifa.Trim()),
                    DatesHorariText = DiaHoraText(curset),
                    DetallSessionsJson = JsonSerializer.Serialize(linia.Dates.Select(d => d.ToString("yyyy-MM-dd"))),
                    CreatedAt = ara
                });
                total += linia.Import;
            }

            remesa.NumLiquidacions = ordenats.Count;
            remesa.ImportTotal = total;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new EmetreRemesaResultDto
            {
                RemesaId = remesa.Id,
                NumEmeses = ordenats.Count,
                NumAnullades = anullades,
                NumOmeses = omeses,
                NumBloquejades = bloquejades,
                ImportTotal = total,
                AlumnesIncompletes = incompletes.Values.OrderBy(a => a.Nom).ToList(),
                CursetsSenseOrdenanca = cursetsSenseOrdenanca.Distinct().OrderBy(s => s).ToList()
            };
        }

        private static string CodiServeiDe(Curset c) =>
            !string.IsNullOrWhiteSpace(c.TipusCurset?.CodiLiquidacio)
                ? c.TipusCurset!.CodiLiquidacio!.Trim().ToUpperInvariant()
                : CodiAuto(c.TipusCurset?.Nom ?? "SRV", 4);

        private static string CodiCursetDe(Curset c) =>
            !string.IsNullOrWhiteSpace(c.CodiLiquidacio)
                ? c.CodiLiquidacio!.Trim().ToUpperInvariant()
                : CodiAuto(c.Nom, 3);

        private static string CodiAuto(string nom, int len)
        {
            var net = new string((nom ?? "").Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
            if (net.Length == 0) net = "XXX";
            return net.Length <= len ? net : net[..len];
        }

        private static string ResoldreExpedient(string? patro, PeriodeResolt p) =>
            AplicaTokens(string.IsNullOrWhiteSpace(patro) ? "CUR/{ANY}/LIQ-{PERIODE}" : patro!, null, p);

        private static string ResoldreConcepte(string? patro, Curset curset, PeriodeResolt p) =>
            AplicaTokens(string.IsNullOrWhiteSpace(patro) ? "{TIPUS} - {CURSET} ({PERIODE_ETIQUETA})" : patro!, curset, p);

        private static string AplicaTokens(string text, Curset? curset, PeriodeResolt p) => text
            .Replace("{ANY}", p.Any.ToString())
            .Replace("{PERIODE}", p.CodiPeriode)
            .Replace("{PERIODE_ETIQUETA}", p.Etiqueta)
            .Replace("{TIPUS}", curset?.TipusCurset?.Nom ?? "")
            .Replace("{CURSET}", curset?.Nom ?? "");

        private static string? DiaHoraText(Curset c)
        {
            var dia = c.DiaSetmana.HasValue ? DiesSetmana[(int)c.DiaSetmana.Value] : null;
            string? hores = null;
            if (c.HoraInici.HasValue && c.HoraFi.HasValue)
                hores = $"{c.HoraInici.Value:hh\\:mm}–{c.HoraFi.Value:hh\\:mm}";
            else if (c.HoraInici.HasValue)
                hores = c.HoraInici.Value.ToString(@"hh\:mm");

            var parts = new[] { dia, hores }.Where(s => !string.IsNullOrWhiteSpace(s));
            var s = string.Join(", ", parts);
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }

        // ==================== Consulta de remeses ====================

        public async Task<List<RemesaDto>> GetRemesesAsync()
        {
            return await _context.CursetsLiquidacionsRemeses
                .OrderByDescending(r => r.Id)
                .Select(r => new RemesaDto
                {
                    Id = r.Id,
                    Descripcio = r.Descripcio,
                    CodiPeriode = r.CodiPeriode,
                    PeriodeEtiqueta = r.PeriodeEtiqueta,
                    Expedient = r.Expedient,
                    DataCreacio = r.DataCreacio,
                    NumLiquidacions = r.NumLiquidacions,
                    ImportTotal = r.ImportTotal
                })
                .ToListAsync();
        }

        public async Task<RemesaDetallDto?> GetRemesaAsync(int remesaId)
        {
            var remesa = await _context.CursetsLiquidacionsRemeses.FirstOrDefaultAsync(r => r.Id == remesaId);
            if (remesa == null)
                return null;

            var liquidacions = await _context.CursetsLiquidacions
                .Where(l => l.RemesaId == remesaId)
                .OrderBy(l => l.Serie)
                .Select(l => new LiquidacioDto
                {
                    Id = l.Id,
                    Numero = l.Numero,
                    AlumneId = l.AlumneId,
                    AlumneNom = l.AlumneNomComplet,
                    CursetId = l.CursetId,
                    CursetTitol = l.CursetTitol,
                    NumSessions = l.NumSessions,
                    Import = l.Import,
                    Estat = l.Estat.ToString(),
                    DataEmissio = l.DataEmissio,
                    DataCobrament = l.DataCobrament,
                    DataAnullacio = l.DataAnullacio,
                    MotiuAnullacio = l.MotiuAnullacio
                })
                .ToListAsync();

            var alumnes = liquidacions
                .Where(l => l.Estat != nameof(EstatLiquidacio.Anullada))
                .GroupBy(l => new { l.AlumneId, l.AlumneNom })
                .Select(g => new AlumnaResumRemesaDto
                {
                    AlumneId = g.Key.AlumneId,
                    Nom = g.Key.AlumneNom,
                    NumFulls = g.Count(),
                    Import = g.Sum(x => x.Import)
                })
                .OrderBy(a => a.Nom)
                .ToList();

            return new RemesaDetallDto
            {
                Remesa = new RemesaDto
                {
                    Id = remesa.Id,
                    Descripcio = remesa.Descripcio,
                    CodiPeriode = remesa.CodiPeriode,
                    PeriodeEtiqueta = remesa.PeriodeEtiqueta,
                    Expedient = remesa.Expedient,
                    DataCreacio = remesa.DataCreacio,
                    NumLiquidacions = remesa.NumLiquidacions,
                    ImportTotal = remesa.ImportTotal
                },
                Liquidacions = liquidacions,
                Alumnes = alumnes
            };
        }

        // ==================== PDF ====================

        public async Task<byte[]> GetPdfLiquidacioAsync(int liquidacioId)
        {
            var liq = await _context.CursetsLiquidacions.FirstOrDefaultAsync(l => l.Id == liquidacioId)
                ?? throw new ArgumentException("La liquidació indicada no existeix.", nameof(liquidacioId));
            return await GeneraPdfAsync(new[] { liq });
        }

        public async Task<byte[]> GetPdfAlumnaAsync(int remesaId, int alumneId)
        {
            var liqs = await _context.CursetsLiquidacions
                .Where(l => l.RemesaId == remesaId && l.AlumneId == alumneId && l.Estat != EstatLiquidacio.Anullada)
                .OrderBy(l => l.CodiCurset).ThenBy(l => l.Serie)
                .ToListAsync();
            if (liqs.Count == 0)
                throw new ArgumentException("No hi ha liquidacions per a aquesta alumna en la remesa.", nameof(alumneId));
            return await GeneraPdfAsync(liqs);
        }

        public async Task<byte[]> GetZipRemesaAsync(int remesaId)
        {
            var remesa = await _context.CursetsLiquidacionsRemeses.FirstOrDefaultAsync(r => r.Id == remesaId)
                ?? throw new ArgumentException("La remesa indicada no existeix.", nameof(remesaId));

            var liqs = await _context.CursetsLiquidacions
                .Where(l => l.RemesaId == remesaId && l.Estat != EstatLiquidacio.Anullada)
                .OrderBy(l => l.AlumneNomComplet).ThenBy(l => l.CodiCurset).ThenBy(l => l.Serie)
                .ToListAsync();

            var config = await GetOrCreateConfigAsync();
            var escut = LlegeixEscut();

            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                var usats = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var grup in liqs.GroupBy(l => l.AlumneId))
                {
                    var alumnaLiqs = grup.ToList();
                    var pdf = LiquidacioPdfDocument.Genera(alumnaLiqs, config, remesa.Expedient, escut);

                    var nom = $"{NomFitxer(remesa.PeriodeEtiqueta)}_{NomFitxer(alumnaLiqs[0].AlumneNomComplet)}";
                    var final = nom;
                    var n = 1;
                    while (!usats.Add(final + ".pdf"))
                        final = $"{nom}_{++n}";

                    var entry = zip.CreateEntry(final + ".pdf", CompressionLevel.Optimal);
                    await using var es = entry.Open();
                    await es.WriteAsync(pdf);
                }
            }
            return ms.ToArray();
        }

        private async Task<byte[]> GeneraPdfAsync(IReadOnlyList<Liquidacio> liqs)
        {
            var config = await GetOrCreateConfigAsync();
            var expedient = await _context.CursetsLiquidacionsRemeses
                .Where(r => r.Id == liqs[0].RemesaId)
                .Select(r => r.Expedient)
                .FirstOrDefaultAsync() ?? "";
            return LiquidacioPdfDocument.Genera(liqs, config, expedient, LlegeixEscut());
        }

        private byte[]? LlegeixEscut()
        {
            if (_escutCache != null)
                return _escutCache.Length == 0 ? null : _escutCache;

            var root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var path = Path.Combine(root, "img", "escut-ajuntament.png");
            _escutCache = File.Exists(path) ? File.ReadAllBytes(path) : Array.Empty<byte>();
            return _escutCache.Length == 0 ? null : _escutCache;
        }

        private static string NomFitxer(string s)
        {
            var net = new string((s ?? "").Select(ch => char.IsLetterOrDigit(ch) || ch is ' ' or '-' or '_' ? ch : '_').ToArray());
            net = string.Join("_", net.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            return string.IsNullOrWhiteSpace(net) ? "liquidacio" : net;
        }

        // ==================== Estats ====================

        public async Task MarcarCobradaAsync(int liquidacioId, DateTime data)
        {
            var liq = await _context.CursetsLiquidacions.FirstOrDefaultAsync(l => l.Id == liquidacioId)
                ?? throw new ArgumentException("La liquidació indicada no existeix.", nameof(liquidacioId));
            if (liq.Estat == EstatLiquidacio.Anullada)
                throw new InvalidOperationException("No es pot cobrar una liquidació anul·lada.");

            liq.Estat = EstatLiquidacio.Cobrada;
            liq.DataCobrament = data.Date;
            liq.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task AnullarAsync(int liquidacioId, string motiu)
        {
            var liq = await _context.CursetsLiquidacions.FirstOrDefaultAsync(l => l.Id == liquidacioId)
                ?? throw new ArgumentException("La liquidació indicada no existeix.", nameof(liquidacioId));

            liq.Estat = EstatLiquidacio.Anullada;
            liq.DataAnullacio = DateTime.Now;
            liq.MotiuAnullacio = string.IsNullOrWhiteSpace(motiu) ? "Anul·lada manualment" : motiu.Trim();
            liq.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        // ==================== Configuració ====================

        public async Task<LiquidacioConfigDto> GetConfigAsync()
        {
            var c = await GetOrCreateConfigAsync();
            return new LiquidacioConfigDto
            {
                ExpedientPatro = c.ExpedientPatro ?? string.Empty,
                OrdenancaTarifa = c.OrdenancaTarifa ?? string.Empty,
                ConceptePatro = c.ConceptePatro ?? string.Empty,
                EntitatsColaboradoresText = c.EntitatsColaboradoresText ?? string.Empty,
                MitjansPagamentText = c.MitjansPagamentText ?? string.Empty,
                OficinaCobratoriaText = c.OficinaCobratoriaText ?? string.Empty,
                TextRecursos = c.TextRecursos ?? string.Empty,
                TextImportant = c.TextImportant ?? string.Empty,
                TextTerminis = c.TextTerminis ?? string.Empty,
                CapcaleraMunicipi = c.CapcaleraMunicipi ?? string.Empty
            };
        }

        public async Task DesarConfigAsync(LiquidacioConfigDto dto)
        {
            var c = await GetOrCreateConfigAsync();
            c.ExpedientPatro = dto.ExpedientPatro?.Trim() ?? "";
            c.OrdenancaTarifa = dto.OrdenancaTarifa ?? "";
            c.ConceptePatro = string.IsNullOrWhiteSpace(dto.ConceptePatro)
                ? "{TIPUS} - {CURSET} ({PERIODE_ETIQUETA})"
                : dto.ConceptePatro.Trim();
            c.EntitatsColaboradoresText = dto.EntitatsColaboradoresText ?? "";
            c.MitjansPagamentText = dto.MitjansPagamentText ?? "";
            c.OficinaCobratoriaText = dto.OficinaCobratoriaText ?? "";
            c.TextRecursos = dto.TextRecursos ?? "";
            c.TextImportant = dto.TextImportant ?? "";
            c.TextTerminis = dto.TextTerminis ?? "";
            c.CapcaleraMunicipi = string.IsNullOrWhiteSpace(dto.CapcaleraMunicipi)
                ? "Ajuntament de Santa Maria de Martorelles"
                : dto.CapcaleraMunicipi.Trim();
            c.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        private async Task<LiquidacioConfig> GetOrCreateConfigAsync()
        {
            var c = await _context.CursetsLiquidacionsConfig.FirstOrDefaultAsync(x => x.Id == 1);
            if (c != null)
                return c;

            c = new LiquidacioConfig
            {
                Id = 1,
                ExpedientPatro = "CUR/{ANY}/LIQ-{PERIODE}",
                ConceptePatro = "{TIPUS} - {CURSET} ({PERIODE_ETIQUETA})",
                CapcaleraMunicipi = "Ajuntament de Santa Maria de Martorelles",
                OrdenancaTarifa = "",
                EntitatsColaboradoresText = "",
                MitjansPagamentText = "Pagament a les Oficines municipals.",
                OficinaCobratoriaText = "",
                TextRecursos = "Contra aquesta autoliquidació podeu interposar recurs de reposició previ al contenciós administratiu davant aquest Ajuntament dins del termini d'un mes de la recepció de la present notificació, d'acord amb la normativa aplicable.",
                TextImportant = "La interposició de recurs o reclamació no paralitzarà l'acció administrativa per al cobrament del deute, llevat que s'estableixi la suspensió del deute en els termes que assenyala la normativa vigent.",
                TextTerminis = "Si ha rebut la notificació entre els dies 1 i 15, pot efectuar el pagament fins al dia 20 del mes següent. Si ha rebut la notificació entre els dies 16 i 31, pot efectuar el pagament fins al dia 5 del segon mes següent. Si no efectua el pagament dins dels terminis anteriors, s'aplicarà el recàrrec corresponent i s'iniciarà, si escau, la via d'apressament.",
                UpdatedAt = DateTime.Now
            };
            _context.CursetsLiquidacionsConfig.Add(c);
            await _context.SaveChangesAsync();
            return c;
        }
    }
}
