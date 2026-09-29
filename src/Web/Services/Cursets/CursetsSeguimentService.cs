using Microsoft.EntityFrameworkCore;
using AppAjuntament.Models;
using AppAjuntament.Models.Cursets;
using AppAjuntament.Models.Cursets.Dto;

namespace AppAjuntament.Services.Cursets
{
    /// <summary>
    /// Seguiment de només lectura per al controlador (regidor): qui hi ha inscrit a
    /// cada curset que supervisa i com té les liquidacions. Tots els mètodes
    /// filtren per <see cref="Curset.RegidorId"/>: un curset d'un altre regidor es
    /// comporta com si no existís (null / llista buida).
    /// </summary>
    public interface ICursetsSeguimentService
    {
        Task<List<ResumSeguimentCursetDto>> GetResumCursetsAsync(int regidorId);
        Task<InscritesCursetDto?> GetInscritesAsync(int regidorId, int cursetId);
        Task<PersonaFitxaDto?> GetPersonaAsync(int regidorId, int cursetId, int alumneId);
        Task<CobramentsDto> GetCobramentsAsync(int regidorId);
    }

    public class CursetsSeguimentService : ICursetsSeguimentService
    {
        private readonly GestorSubvencionsContext _context;

        public CursetsSeguimentService(GestorSubvencionsContext context)
        {
            _context = context;
        }

        // Estats que es mostren al regidor. Les baixes hi són sempre (amb les dates),
        // les sol·licituds només mentre el període d'inscripció és obert i les
        // rebutjades mai.
        private static readonly EstatInscripcio[] EstatsVisibles =
        {
            EstatInscripcio.Admesa, EstatInscripcio.LlistaEspera,
            EstatInscripcio.Baixa, EstatInscripcio.Sollicitada
        };

        private sealed record CursetInfo(int Id, string Titol, int? MaxPlaces,
            DateTime? InscripcioInici, DateTime? InscripcioFi,
            decimal PreuEmpadronat, decimal PreuNoEmpadronat)
        {
            public bool InscripcioOberta
            {
                get
                {
                    var avui = DateTime.Today;
                    if (InscripcioInici is null && InscripcioFi is null) return false;
                    if (InscripcioInici is not null && avui < InscripcioInici.Value.Date) return false;
                    if (InscripcioFi is not null && avui > InscripcioFi.Value.Date) return false;
                    return true;
                }
            }
        }

        private sealed record InscripcioInfo(int CursetId, int AlumneId, string Nom, string? Cognoms,
            bool Empadronat, EstatInscripcio Estat, int? OrdreLlistaEspera, DateTime? DataSollicitud,
            string? Origen, DateTime DataAlta, DateTime? DataBaixa);

        private sealed record LiqInfo(int Id, int CursetId, int AlumneId, string Numero, string PeriodeEtiqueta,
            int NumSessions, decimal PreuPerSessio, decimal Import, EstatLiquidacio Estat,
            DateTime DataEmissio, DateTime? DataCobrament, DateTime? DataAnullacio, string? MotiuAnullacio,
            int RemesaId);

        // ==================== Consultes ====================

        private Task<List<CursetInfo>> CursetsDelRegidorAsync(int regidorId, int? cursetId = null)
        {
            var q = _context.Cursets.Where(c => c.RegidorId == regidorId);
            if (cursetId.HasValue)
                q = q.Where(c => c.Id == cursetId.Value);
            else
                q = q.Where(c => c.Actiu);

            return q.Select(c => new CursetInfo(
                    c.Id,
                    c.TipusCurset != null ? c.TipusCurset.Nom + " - " + c.Nom : c.Nom,
                    c.MaxPlaces, c.InscripcioInici, c.InscripcioFi,
                    c.PreuPerSessioEmpadronat, c.PreuPerSessioNoEmpadronat))
                .AsNoTracking()
                .ToListAsync();
        }

        private Task<List<InscripcioInfo>> InscripcionsAsync(IReadOnlyCollection<int> cursetIds, int? alumneId = null)
        {
            var q = _context.AlumnesCursets
                .Where(ac => cursetIds.Contains(ac.CursetId) && EstatsVisibles.Contains(ac.Estat));
            if (alumneId.HasValue)
                q = q.Where(ac => ac.AlumneId == alumneId.Value);

            return q.Select(ac => new InscripcioInfo(
                    ac.CursetId, ac.AlumneId,
                    ac.Alumne!.Tercer!.Nom, ac.Alumne.Tercer.Cognoms,
                    ac.Alumne.Empadronat, ac.Estat, ac.OrdreLlistaEspera, ac.DataSollicitud,
                    ac.Origen, ac.DataAlta, ac.DataBaixa))
                .AsNoTracking()
                .ToListAsync();
        }

        private Task<List<LiqInfo>> LiquidacionsAsync(IReadOnlyCollection<int> cursetIds, int? alumneId = null)
        {
            var q = _context.CursetsLiquidacions.Where(l => cursetIds.Contains(l.CursetId));
            if (alumneId.HasValue)
                q = q.Where(l => l.AlumneId == alumneId.Value);

            return q.Select(l => new LiqInfo(
                    l.Id, l.CursetId, l.AlumneId, l.Numero, l.PeriodeEtiqueta,
                    l.NumSessions, l.PreuPerSessio, l.Import, l.Estat,
                    l.DataEmissio, l.DataCobrament, l.DataAnullacio, l.MotiuAnullacio,
                    l.RemesaId))
                .AsNoTracking()
                .ToListAsync();
        }

        // ==================== Càlculs ====================

        private static ResumImportsDto Resum(IEnumerable<LiqInfo> liquidacions)
        {
            var valides = liquidacions.Where(l => l.Estat != EstatLiquidacio.Anullada).ToList();
            var ultima = valides.OrderByDescending(l => l.DataEmissio).FirstOrDefault();
            return new ResumImportsDto
            {
                Liquidat = valides.Sum(l => l.Import),
                Cobrat = valides.Where(l => l.Estat == EstatLiquidacio.Cobrada).Sum(l => l.Import),
                Pendent = valides.Where(l => l.Estat == EstatLiquidacio.Emesa).Sum(l => l.Import),
                NumLiquidacions = valides.Count,
                UltimaRemesaEtiqueta = ultima?.PeriodeEtiqueta,
                UltimaRemesaData = ultima?.DataEmissio
            };
        }

        private static PersonaInscritaDto Persona(InscripcioInfo i, IEnumerable<LiqInfo> liquidacionsPersona)
        {
            var valides = liquidacionsPersona.Where(l => l.Estat != EstatLiquidacio.Anullada).ToList();
            var pendents = valides.Where(l => l.Estat == EstatLiquidacio.Emesa).ToList();

            // Qui no ha tingut mai plaça no factura; però si en algun moment en va
            // tenir i encara deu alguna cosa, el pendent mana.
            string situacio;
            if (pendents.Count > 0)
                situacio = SituacioPagament.Pendent;
            else if (valides.Count > 0)
                situacio = SituacioPagament.AlCorrent;
            else if (i.Estat is EstatInscripcio.LlistaEspera or EstatInscripcio.Sollicitada)
                situacio = SituacioPagament.NoFactura;
            else
                situacio = SituacioPagament.SenseLiquidar;

            return new PersonaInscritaDto
            {
                AlumneId = i.AlumneId,
                NomComplet = string.IsNullOrWhiteSpace(i.Cognoms) ? i.Nom : $"{i.Nom} {i.Cognoms}".Trim(),
                Empadronat = i.Empadronat,
                Estat = i.Estat.ToString(),
                OrdreLlistaEspera = i.OrdreLlistaEspera,
                DataSollicitud = i.DataSollicitud,
                Origen = i.Origen,
                DataAlta = i.DataAlta,
                DataBaixa = i.DataBaixa,
                Situacio = situacio,
                ImportPendent = pendents.Sum(l => l.Import),
                NumPendents = pendents.Count,
                PendentMesAntic = pendents.Count > 0 ? pendents.Min(l => l.DataEmissio) : null
            };
        }

        /// <summary>
        /// Persones visibles d'un curset. Les sol·licituds només surten amb el període
        /// obert. Si algú té liquidacions però ja no té cap inscripció visible (p.ex.
        /// rebutjada després d'haver-hi estat), no apareix: el que es liquida sempre
        /// va lligat a una inscripció admesa o de baixa.
        /// </summary>
        private static List<PersonaInscritaDto> Persones(CursetInfo curset,
            IEnumerable<InscripcioInfo> inscripcions, IEnumerable<LiqInfo> liquidacions)
        {
            var perAlumne = liquidacions.ToLookup(l => l.AlumneId);
            return inscripcions
                .Where(i => i.CursetId == curset.Id)
                .Where(i => i.Estat != EstatInscripcio.Sollicitada || curset.InscripcioOberta)
                .Select(i => Persona(i, perAlumne[i.AlumneId]))
                .OrderBy(p => OrdreEstat(p.Estat))
                .ThenBy(p => p.OrdreLlistaEspera ?? int.MaxValue)
                .ThenBy(p => p.DataSollicitud ?? DateTime.MaxValue)
                .ThenBy(p => p.NomComplet)
                .ToList();
        }

        private static int OrdreEstat(string estat) => estat switch
        {
            nameof(EstatInscripcio.Admesa) => 0,
            nameof(EstatInscripcio.LlistaEspera) => 1,
            nameof(EstatInscripcio.Sollicitada) => 2,
            _ => 3
        };

        // ==================== API ====================

        public async Task<List<ResumSeguimentCursetDto>> GetResumCursetsAsync(int regidorId)
        {
            var cursets = await CursetsDelRegidorAsync(regidorId);
            var ids = cursets.Select(c => c.Id).ToList();
            var inscripcions = await InscripcionsAsync(ids);
            var liquidacions = await LiquidacionsAsync(ids);

            return cursets.Select(c =>
            {
                var persones = Persones(c, inscripcions, liquidacions.Where(l => l.CursetId == c.Id));
                var pendents = persones.Where(p => p.Situacio == SituacioPagament.Pendent).ToList();
                return new ResumSeguimentCursetDto
                {
                    CursetId = c.Id,
                    NumAdmeses = persones.Count(p => p.Estat == nameof(EstatInscripcio.Admesa)),
                    NumLlistaEspera = persones.Count(p => p.Estat == nameof(EstatInscripcio.LlistaEspera)),
                    NumPendents = pendents.Count,
                    ImportPendent = pendents.Sum(p => p.ImportPendent),
                    TeLiquidacions = liquidacions.Any(l => l.CursetId == c.Id && l.Estat != EstatLiquidacio.Anullada)
                };
            }).ToList();
        }

        public async Task<InscritesCursetDto?> GetInscritesAsync(int regidorId, int cursetId)
        {
            var curset = (await CursetsDelRegidorAsync(regidorId, cursetId)).FirstOrDefault();
            if (curset is null) return null;

            var ids = new[] { curset.Id };
            var inscripcions = await InscripcionsAsync(ids);
            var liquidacions = await LiquidacionsAsync(ids);

            return new InscritesCursetDto
            {
                CursetId = curset.Id,
                Titol = curset.Titol,
                MaxPlaces = curset.MaxPlaces,
                InscripcioOberta = curset.InscripcioOberta,
                Resum = Resum(liquidacions),
                Persones = Persones(curset, inscripcions, liquidacions)
            };
        }

        public async Task<PersonaFitxaDto?> GetPersonaAsync(int regidorId, int cursetId, int alumneId)
        {
            var curset = (await CursetsDelRegidorAsync(regidorId, cursetId)).FirstOrDefault();
            if (curset is null) return null;

            var ids = new[] { curset.Id };
            var inscripcio = (await InscripcionsAsync(ids, alumneId)).FirstOrDefault();
            if (inscripcio is null) return null;

            var liquidacions = await LiquidacionsAsync(ids, alumneId);

            // Assistència: sessions tancades mentre hi estava apuntada (alta..baixa).
            var desDe = inscripcio.DataAlta.Date;
            var finsA = (inscripcio.DataBaixa ?? DateTime.Today).Date;
            var sessions = await _context.CursetsSessions
                .Where(s => s.CursetId == curset.Id && s.Estat == EstatSessio.Tancada
                            && s.Data >= desDe && s.Data <= finsA)
                .Select(s => new
                {
                    s.Data,
                    Present = s.Assistencies.Any(a => a.AlumneId == alumneId && a.Present)
                })
                .AsNoTracking()
                .ToListAsync();

            var teSessions = inscripcio.Estat is EstatInscripcio.Admesa or EstatInscripcio.Baixa;

            return new PersonaFitxaDto
            {
                Persona = Persona(inscripcio, liquidacions),
                CursetId = curset.Id,
                CursetTitol = curset.Titol,
                PreuPerSessio = inscripcio.Empadronat ? curset.PreuEmpadronat : curset.PreuNoEmpadronat,
                SessionsImpartides = teSessions ? sessions.Count : 0,
                SessionsAssistides = teSessions ? sessions.Count(s => s.Present) : 0,
                UltimaAssistencia = teSessions ? sessions.Where(s => s.Present).Select(s => (DateTime?)s.Data).Max() : null,
                Liquidacions = liquidacions
                    .OrderByDescending(l => l.DataEmissio).ThenByDescending(l => l.Id)
                    .Select(l => new LiquidacioSeguimentDto
                    {
                        Id = l.Id,
                        Numero = l.Numero,
                        PeriodeEtiqueta = l.PeriodeEtiqueta,
                        NumSessions = l.NumSessions,
                        PreuPerSessio = l.PreuPerSessio,
                        Import = l.Import,
                        Estat = l.Estat.ToString(),
                        DataEmissio = l.DataEmissio,
                        DataCobrament = l.DataCobrament,
                        DataAnullacio = l.DataAnullacio,
                        MotiuAnullacio = l.MotiuAnullacio
                    })
                    .ToList()
            };
        }

        public async Task<CobramentsDto> GetCobramentsAsync(int regidorId)
        {
            var cursets = await CursetsDelRegidorAsync(regidorId);
            var ids = cursets.Select(c => c.Id).ToList();
            var inscripcions = await InscripcionsAsync(ids);
            var liquidacions = await LiquidacionsAsync(ids);

            return new CobramentsDto
            {
                Total = Resum(liquidacions),
                Cursets = cursets
                    .OrderBy(c => c.Titol)
                    .Select(c =>
                    {
                        var liqCurset = liquidacions.Where(l => l.CursetId == c.Id).ToList();
                        return new CobramentsCursetDto
                        {
                            CursetId = c.Id,
                            Titol = c.Titol,
                            Resum = Resum(liqCurset),
                            Pendents = Persones(c, inscripcions, liqCurset)
                                .Where(p => p.Situacio == SituacioPagament.Pendent)
                                .OrderByDescending(p => p.ImportPendent)
                                .ThenBy(p => p.NomComplet)
                                .ToList()
                        };
                    })
                    .ToList()
            };
        }
    }
}
