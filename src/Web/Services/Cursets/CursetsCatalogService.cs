using Microsoft.EntityFrameworkCore;
using AppAjuntament.Models;
using AppAjuntament.Models.Tercers;
using AppAjuntament.Models.Cursets;
using AppAjuntament.Models.Cursets.Dto;

namespace AppAjuntament.Services.Cursets
{
    /// <summary>Gestió de tipus de curset (temàtiques), cursets i alumnes.</summary>
    public interface ICursetsCatalogService
    {
        Task<List<TipusCursetDto>> GetTipusCursetsAsync(bool nomesActius = true);
        Task<TipusCurset> CrearTipusCursetAsync(CrearTipusCursetRequest request);
        Task ActualitzarTipusCursetAsync(int id, ActualitzarTipusCursetRequest request);

        /// <summary>Cursets actius filtrats per professora o per regidor responsable (segons qui consulta).</summary>
        Task<List<CursetDto>> GetCursetsAsync(int? professoraId = null, int? regidorId = null);
        /// <summary>Cursos actius amb només la informació pública (pàgina oberta, sense login).</summary>
        Task<List<CursetPublicDto>> GetCursetsPublicsAsync();
        /// <summary>Tots els cursets (actius i de baixa) per a la gestió web.</summary>
        Task<List<CursetDto>> GetCursetsAdminAsync();
        Task<CursetDto?> GetCursetAsync(int id);
        Task<Curset> CrearCursetAsync(CrearCursetRequest request);
        Task<Curset> ActualitzarCursetAsync(int id, CrearCursetRequest request);
        Task CanviarEstatCursetAsync(int id, bool actiu);

        Task<List<AlumneDto>> GetAlumnesDeCursetAsync(int cursetId);
        /// <summary>Tots els alumnes per a la gestió web.</summary>
        Task<List<AlumneAdminDto>> GetAlumnesAsync(bool incloureInactius = true);
        Task<AlumneAdminDto?> GetAlumneAsync(int id);
        /// <summary>Fitxa completa d'un alumne: dades, inscripcions i liquidacions.</summary>
        Task<AlumneFitxaDto?> GetAlumneFitxaAsync(int id);
        Task<Alumne> CrearAlumneAsync(CrearAlumneRequest request);
        Task<Alumne> ActualitzarAlumneAsync(int id, ActualitzarAlumneRequest request);
        Task CanviarEstatAlumneAsync(int id, bool actiu);

        /// <summary>Canvia l'estat d'una inscripció concreta (admetre / llista d'espera / baixa / rebutjar).</summary>
        Task CanviarEstatInscripcioAsync(int alumneId, int cursetId, EstatInscripcio nou);
        /// <summary>Alta manual d'una inscripció (queda admesa directament).</summary>
        Task AfegirInscripcioAsync(int alumneId, int cursetId);

        /// <summary>Registra una sol·licitud d'inscripció del formulari públic. Torna el nom de l'interessat.</summary>
        Task<string> CrearSollicitudPublicaAsync(SollicitudInscripcioPublicaRequest req);
        /// <summary>Estat d'un curset de cara al sorteig (places, admesos, sol·licituds pendents).</summary>
        Task<SorteigEstatDto> GetSorteigEstatAsync(int cursetId);
        /// <summary>Fa el sorteig d'un curset: adjudica les places lliures i posa la resta a la llista d'espera.</summary>
        Task<SorteigResultDto> FerSorteigAsync(int cursetId);
    }

    public class CursetsCatalogService : ICursetsCatalogService
    {
        private readonly GestorSubvencionsContext _context;

        public CursetsCatalogService(GestorSubvencionsContext context)
        {
            _context = context;
        }

        // ==================== Tipus de curset ====================

        public async Task<List<TipusCursetDto>> GetTipusCursetsAsync(bool nomesActius = true)
        {
            var query = _context.TipusCursets.AsQueryable();
            if (nomesActius)
                query = query.Where(t => t.Actiu);

            return await query
                .Select(t => new TipusCursetDto { Id = t.Id, Nom = t.Nom, CodiLiquidacio = t.CodiLiquidacio })
                .OrderBy(t => t.Nom)
                .ToListAsync();
        }

        public async Task<TipusCurset> CrearTipusCursetAsync(CrearTipusCursetRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nom))
                throw new ArgumentException("El nom de la temàtica és obligatori.", nameof(request));

            var nom = request.Nom.Trim();
            if (await _context.TipusCursets.AnyAsync(t => t.Nom == nom))
                throw new ArgumentException("Ja existeix una temàtica amb aquest nom.", nameof(request));

            var tipus = new TipusCurset { Nom = nom, CodiLiquidacio = NetejaCodi(request.CodiLiquidacio) };
            _context.TipusCursets.Add(tipus);
            await _context.SaveChangesAsync();
            return tipus;
        }

        public async Task ActualitzarTipusCursetAsync(int id, ActualitzarTipusCursetRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nom))
                throw new ArgumentException("El nom de la temàtica és obligatori.", nameof(request));

            var tipus = await _context.TipusCursets.FindAsync(id)
                ?? throw new ArgumentException("La temàtica indicada no existeix.", nameof(id));

            var nom = request.Nom.Trim();
            if (await _context.TipusCursets.AnyAsync(t => t.Nom == nom && t.Id != id))
                throw new ArgumentException("Ja existeix una temàtica amb aquest nom.", nameof(request));

            tipus.Nom = nom;
            tipus.CodiLiquidacio = NetejaCodi(request.CodiLiquidacio);
            await _context.SaveChangesAsync();
        }

        private static string? NetejaCodi(string? codi)
        {
            if (string.IsNullOrWhiteSpace(codi))
                return null;
            var net = new string(codi.Trim().ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
            return net.Length == 0 ? null : (net.Length > 16 ? net[..16] : net);
        }

        private static string? Net(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private static readonly string[] CadenciesValides = { "Trimestral", "Mensual", "Setmanal" };

        private static string? NetejaCadencia(string? cadencia)
        {
            if (string.IsNullOrWhiteSpace(cadencia))
                return null;
            return CadenciesValides.FirstOrDefault(c => string.Equals(c, cadencia.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        // ==================== Cursets ====================

        public Task<List<CursetDto>> GetCursetsAsync(int? professoraId = null, int? regidorId = null)
            => QueryCursets(_context.Cursets.Where(c => c.Actiu), professoraId, regidorId);

        public async Task<List<CursetPublicDto>> GetCursetsPublicsAsync()
        {
            var rows = await _context.Cursets
                .Where(c => c.Actiu)
                .Select(c => new
                {
                    c.Id,
                    Tematica = c.TipusCurset != null ? c.TipusCurset.Nom : string.Empty,
                    c.Nom,
                    c.DiaSetmana,
                    c.HoraInici,
                    c.HoraFi,
                    c.PreuPerSessioEmpadronat,
                    c.PreuPerSessioNoEmpadronat,
                    c.MaxPlaces,
                    c.InscripcioInici,
                    c.InscripcioFi,
                    c.UrlInscripcio
                })
                .ToListAsync();

            var avui = DateTime.Today;

            return rows
                .Select(c =>
                {
                    var estat = EstatPeriodeInscripcio(c.InscripcioInici, c.InscripcioFi, avui);
                    return new CursetPublicDto
                    {
                        Id = c.Id,
                        Tematica = c.Tematica,
                        Nom = c.Nom,
                        Titol = string.IsNullOrWhiteSpace(c.Tematica) ? c.Nom : $"{c.Tematica} - {c.Nom}",
                        DiaSetmana = c.DiaSetmana,
                        HoraInici = c.HoraInici,
                        HoraFi = c.HoraFi,
                        PreuPerSessioEmpadronat = c.PreuPerSessioEmpadronat,
                        PreuPerSessioNoEmpadronat = c.PreuPerSessioNoEmpadronat,
                        PlacesTotals = c.MaxPlaces,
                        EstatInscripcio = estat,
                        InscripcioInici = c.InscripcioInici,
                        InscripcioFi = c.InscripcioFi,
                        UrlInscripcio = estat == "Oberta" ? Net(c.UrlInscripcio) : null
                    };
                })
                .OrderBy(c => c.Tematica).ThenBy(c => c.Nom)
                .ToList();
        }

        /// <summary>Estat del període d'inscripció d'un curset segons les dates i la data d'avui.</summary>
        private static string EstatPeriodeInscripcio(DateTime? inici, DateTime? fi, DateTime avui)
        {
            if (inici is null && fi is null)
                return "SenseDates";
            if (inici is not null && avui < inici.Value.Date)
                return "Properament";
            if (fi is not null && avui > fi.Value.Date)
                return "Tancada";
            return "Oberta";
        }

        private static readonly string[] NomsDies =
            { "Diumenge", "Dilluns", "Dimarts", "Dimecres", "Dijous", "Divendres", "Dissabte" };

        private static string? FormatDiaHora(Curset? c)
        {
            if (c is null) return null;
            var dia = c.DiaSetmana.HasValue ? NomsDies[(int)c.DiaSetmana.Value] : null;
            string? hora = c.HoraInici.HasValue
                ? (c.HoraFi.HasValue
                    ? $"{c.HoraInici.Value:hh\\:mm}–{c.HoraFi.Value:hh\\:mm}"
                    : c.HoraInici.Value.ToString(@"hh\:mm"))
                : null;
            var s = string.Join(" ", new[] { dia, hora }.Where(x => !string.IsNullOrWhiteSpace(x)));
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }

        /// <summary>Normalitza un DNI/NIF: només alfanumèric, majúscules.</summary>
        private static string? NetDni(string? dni)
        {
            if (string.IsNullOrWhiteSpace(dni)) return null;
            var net = new string(dni.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
            return net.Length == 0 ? null : net;
        }

        public Task<List<CursetDto>> GetCursetsAdminAsync()
            => QueryCursets(_context.Cursets, null, null);

        private static Task<List<CursetDto>> QueryCursets(IQueryable<Curset> query, int? professoraId, int? regidorId)
        {
            if (professoraId.HasValue)
                query = query.Where(c => c.ProfessoraId == professoraId.Value);
            if (regidorId.HasValue)
                query = query.Where(c => c.RegidorId == regidorId.Value);

            return query
                .Select(c => new CursetDto
                {
                    Id = c.Id,
                    Nom = c.Nom,
                    Titol = c.TipusCurset != null ? c.TipusCurset.Nom + " - " + c.Nom : c.Nom,
                    TipusCursetId = c.TipusCursetId,
                    TipusCursetNom = c.TipusCurset != null ? c.TipusCurset.Nom : string.Empty,
                    ProfessoraId = c.ProfessoraId,
                    ProfessoraNom = c.Professora != null ? (c.Professora.Nom ?? string.Empty) : string.Empty,
                    RegidorId = c.RegidorId,
                    RegidorNom = c.Regidor != null ? (c.Regidor.NomComplet ?? c.Regidor.Nom) : null,
                    DiaSetmana = c.DiaSetmana,
                    HoraInici = c.HoraInici,
                    HoraFi = c.HoraFi,
                    Actiu = c.Actiu,
                    NumAlumnes = c.Alumnes.Count(ac => ac.Estat == EstatInscripcio.Admesa),
                    MaxPlaces = c.MaxPlaces,
                    PreuPerSessioEmpadronat = c.PreuPerSessioEmpadronat,
                    PreuPerSessioNoEmpadronat = c.PreuPerSessioNoEmpadronat,
                    CodiLiquidacio = c.CodiLiquidacio,
                    CadenciaLiquidacio = c.CadenciaLiquidacio,
                    OrdenancaLiquidacio = c.OrdenancaLiquidacio,
                    InscripcioInici = c.InscripcioInici,
                    InscripcioFi = c.InscripcioFi,
                    UrlInscripcio = c.UrlInscripcio
                })
                .OrderByDescending(c => c.Actiu)
                .ThenBy(c => c.TipusCursetNom)
                .ThenBy(c => c.Nom)
                .ToListAsync();
        }

        public async Task<CursetDto?> GetCursetAsync(int id)
        {
            var cursets = await QueryCursets(_context.Cursets.Where(c => c.Id == id), null, null);
            return cursets.FirstOrDefault();
        }

        public async Task<Curset> CrearCursetAsync(CrearCursetRequest request)
        {
            await ValidarCursetAsync(request);

            var curset = new Curset
            {
                Nom = request.Nom.Trim(),
                TipusCursetId = request.TipusCursetId,
                ProfessoraId = request.ProfessoraId,
                RegidorId = request.RegidorId,
                DiaSetmana = request.DiaSetmana,
                HoraInici = request.HoraInici,
                HoraFi = request.HoraFi,
                PreuPerSessioEmpadronat = request.PreuPerSessioEmpadronat,
                PreuPerSessioNoEmpadronat = request.PreuPerSessioNoEmpadronat,
                MaxPlaces = request.MaxPlaces is > 0 ? request.MaxPlaces : null,
                CodiLiquidacio = NetejaCodi(request.CodiLiquidacio),
                CadenciaLiquidacio = NetejaCadencia(request.CadenciaLiquidacio),
                OrdenancaLiquidacio = Net(request.OrdenancaLiquidacio),
                InscripcioInici = request.InscripcioInici?.Date,
                InscripcioFi = request.InscripcioFi?.Date,
                UrlInscripcio = Net(request.UrlInscripcio)
            };
            _context.Cursets.Add(curset);
            await _context.SaveChangesAsync();
            return curset;
        }

        public async Task<Curset> ActualitzarCursetAsync(int id, CrearCursetRequest request)
        {
            var curset = await _context.Cursets.FindAsync(id)
                ?? throw new ArgumentException("El curset indicat no existeix.", nameof(id));

            await ValidarCursetAsync(request);

            curset.Nom = request.Nom.Trim();
            curset.TipusCursetId = request.TipusCursetId;
            curset.ProfessoraId = request.ProfessoraId;
            curset.RegidorId = request.RegidorId;
            curset.DiaSetmana = request.DiaSetmana;
            curset.HoraInici = request.HoraInici;
            curset.HoraFi = request.HoraFi;
            curset.PreuPerSessioEmpadronat = request.PreuPerSessioEmpadronat;
            curset.PreuPerSessioNoEmpadronat = request.PreuPerSessioNoEmpadronat;
            curset.MaxPlaces = request.MaxPlaces is > 0 ? request.MaxPlaces : null;
            curset.CodiLiquidacio = NetejaCodi(request.CodiLiquidacio);
            curset.CadenciaLiquidacio = NetejaCadencia(request.CadenciaLiquidacio);
            curset.OrdenancaLiquidacio = Net(request.OrdenancaLiquidacio);
            curset.InscripcioInici = request.InscripcioInici?.Date;
            curset.InscripcioFi = request.InscripcioFi?.Date;
            curset.UrlInscripcio = Net(request.UrlInscripcio);
            curset.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return curset;
        }

        public async Task CanviarEstatCursetAsync(int id, bool actiu)
        {
            var curset = await _context.Cursets.FindAsync(id)
                ?? throw new ArgumentException("El curset indicat no existeix.", nameof(id));

            curset.Actiu = actiu;
            curset.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        private async Task ValidarCursetAsync(CrearCursetRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nom))
                throw new ArgumentException("El nom del curset és obligatori.", nameof(request));

            var tipusExisteix = await _context.TipusCursets.AnyAsync(t => t.Id == request.TipusCursetId);
            if (!tipusExisteix)
                throw new ArgumentException("La temàtica indicada no existeix.", nameof(request));

            var esProfessora = await _context.UsuarisRols
                .AnyAsync(ur => ur.UsuariId == request.ProfessoraId && ur.Rol!.Nom == "Professora");
            if (!esProfessora)
                throw new ArgumentException("La professora indicada no existeix.", nameof(request));

            if (request.RegidorId.HasValue)
            {
                var regidorVigent = await _context.Regidors
                    .AnyAsync(r => r.Id == request.RegidorId.Value && r.Orde != null);
                if (!regidorVigent)
                    throw new ArgumentException("El regidor indicat no existeix o no és del mandat actual.", nameof(request));
            }

            var diaSencer = TimeSpan.FromHours(24);
            if ((request.HoraInici.HasValue && (request.HoraInici < TimeSpan.Zero || request.HoraInici >= diaSencer)) ||
                (request.HoraFi.HasValue && (request.HoraFi < TimeSpan.Zero || request.HoraFi >= diaSencer)))
                throw new ArgumentException("Les hores han d'estar entre 00:00 i 23:59.", nameof(request));

            if (request.HoraInici.HasValue && request.HoraFi.HasValue && request.HoraFi <= request.HoraInici)
                throw new ArgumentException("L'hora de fi ha de ser posterior a la d'inici.", nameof(request));

            if (request.PreuPerSessioEmpadronat < 0 || request.PreuPerSessioNoEmpadronat < 0)
                throw new ArgumentException("Els preus no poden ser negatius.", nameof(request));

            if (request.InscripcioInici.HasValue && request.InscripcioFi.HasValue &&
                request.InscripcioFi.Value.Date < request.InscripcioInici.Value.Date)
                throw new ArgumentException("La data de fi d'inscripció ha de ser igual o posterior a la d'inici.", nameof(request));
        }

        // ==================== Alumnes ====================

        public async Task<List<AlumneDto>> GetAlumnesDeCursetAsync(int cursetId)
        {
            return await _context.AlumnesCursets
                .Where(ac => ac.CursetId == cursetId && ac.Estat == EstatInscripcio.Admesa && ac.Alumne!.Actiu)
                .Select(ac => new AlumneDto
                {
                    Id = ac.Alumne!.Id,
                    NomComplet = string.IsNullOrWhiteSpace(ac.Alumne.Tercer!.Cognoms)
                        ? ac.Alumne.Tercer.Nom
                        : ac.Alumne.Tercer.Nom + " " + ac.Alumne.Tercer.Cognoms,
                    Telefon = ac.Alumne.Tercer!.Telefon,
                    Email = ac.Alumne.Tercer!.Email,
                    Actiu = ac.Alumne.Actiu,
                    Empadronat = ac.Alumne.Empadronat,
                    Notes = ac.Alumne.Notes
                })
                .OrderBy(a => a.NomComplet)
                .ToListAsync();
        }

        public async Task<List<AlumneAdminDto>> GetAlumnesAsync(bool incloureInactius = true)
        {
            var query = _context.CursetsAlumnes
                .Include(a => a.Tercer)
                .Include(a => a.Cursets).ThenInclude(ac => ac.Curset)
                .AsQueryable();

            if (!incloureInactius)
                query = query.Where(a => a.Actiu);

            var alumnes = await query.AsNoTracking().ToListAsync();

            var deutes = await _context.CursetsLiquidacions
                .Where(l => l.Estat == EstatLiquidacio.Emesa)
                .GroupBy(l => l.AlumneId)
                .Select(g => new { AlumneId = g.Key, Total = g.Sum(x => x.Import) })
                .ToDictionaryAsync(x => x.AlumneId, x => x.Total);

            return alumnes
                .Select(a =>
                {
                    var dto = MapAlumne(a);
                    dto.Deute = deutes.TryGetValue(a.Id, out var d) ? d : 0m;
                    return dto;
                })
                .OrderByDescending(a => a.Actiu)
                .ThenBy(a => a.NomComplet)
                .ToList();
        }

        public async Task<AlumneAdminDto?> GetAlumneAsync(int id)
        {
            var alumne = await _context.CursetsAlumnes
                .Include(a => a.Tercer)
                .Include(a => a.Cursets).ThenInclude(ac => ac.Curset)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            return alumne is null ? null : MapAlumne(alumne);
        }

        private static AlumneAdminDto MapAlumne(Alumne a)
        {
            var inscripcions = a.Cursets.Where(ac => ac.Estat == EstatInscripcio.Admesa).ToList();
            return new AlumneAdminDto
            {
                Id = a.Id,
                Nom = a.Tercer?.Nom ?? string.Empty,
                Cognoms = a.Tercer?.Cognoms,
                NomComplet = a.Tercer?.NomComplet ?? "(sense persona)",
                Dni = a.Tercer?.DNI,
                Telefon = a.Tercer?.Telefon,
                Email = a.Tercer?.Email,
                Adreca = a.Tercer?.Adreca,
                CodiPostal = a.Tercer?.CodiPostal,
                Poblacio = a.Tercer?.Poblacio,
                Notes = a.Notes,
                Actiu = a.Actiu,
                Empadronat = a.Empadronat,
                CursetIds = inscripcions.Select(ac => ac.CursetId).ToList(),
                CursetsResum = string.Join(", ", inscripcions
                    .Select(ac => ac.Curset?.Nom)
                    .Where(n => !string.IsNullOrWhiteSpace(n))),
                DadesLiquidacioFalten = LiquidacioDades.QueFalten(
                    a.Tercer?.DNI, a.Tercer?.Adreca, a.Tercer?.CodiPostal, a.Tercer?.Poblacio)
            };
        }

        public async Task<Alumne> CrearAlumneAsync(CrearAlumneRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nom))
                throw new ArgumentException("El nom de l'alumne és obligatori.", nameof(request));

            // Si el DNI ja existeix a Tercers no es crea ningú nou, així que no cal
            // tornar a demanar l'autorització (ja es va confirmar en el seu moment).
            var dniJaExisteix = NetDni(request.Dni) is { } dniNet
                && await _context.Tercers.AnyAsync(t => t.DNI == dniNet);
            if (!dniJaExisteix && !request.AutoritzacioTractamentSignada)
                throw new ArgumentException(
                    "Cal confirmar que la persona ha signat l'autorització de tractament de dades.");

            // L'alumne no deixa de ser un Tercer (ciutadà): si el DNI ja existeix, es reutilitza.
            var alumne = await ResoldreAlumneAsync(request.Nom, request.Cognoms, request.Dni, request.Telefon,
                request.Email, request.Adreca, request.CodiPostal, request.Poblacio,
                empadronatSiNou: request.Empadronat, notes: request.Notes,
                autoritzacioTractamentSignada: request.AutoritzacioTractamentSignada);

            await SincronitzarInscripcionsAsync(alumne.Id, request.CursetIds ?? new List<int>());
            return alumne;
        }

        /// <summary>
        /// Resol l'alumne (i el seu Tercer) a partir de dades de persona. Si el DNI ja
        /// existeix a Tercers es reutilitza (i l'Alumne associat, si n'hi ha); si no, es crea.
        /// En reutilitzar només omple els camps de contacte/domicili que estiguessin buits.
        /// </summary>
        private async Task<Alumne> ResoldreAlumneAsync(string nom, string? cognoms, string? dni,
            string? telefon, string? email, string? adreca, string? codiPostal, string? poblacio,
            bool? empadronatSiNou, string? notes, bool autoritzacioTractamentSignada)
        {
            var dniNet = NetDni(dni);

            Tercer? tercer = null;
            if (dniNet is not null)
            {
                var candidats = await _context.Tercers
                    .Where(t => t.DNI != null)
                    .Select(t => new { t.Id, t.DNI })
                    .ToListAsync();
                var match = candidats.FirstOrDefault(c => NetDni(c.DNI) == dniNet);
                if (match is not null)
                    tercer = await _context.Tercers.FirstAsync(t => t.Id == match.Id);
            }

            if (tercer is null)
            {
                tercer = new Tercer
                {
                    Nom = nom.Trim(),
                    Cognoms = Net(cognoms),
                    DNI = dniNet,
                    Telefon = Net(telefon),
                    Email = Net(email),
                    Adreca = Net(adreca),
                    CodiPostal = Net(codiPostal),
                    Poblacio = Net(poblacio),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    AutoritzacioTractamentSignada = autoritzacioTractamentSignada,
                    DataAutoritzacio = autoritzacioTractamentSignada ? DateTime.Now : null
                };
                _context.Tercers.Add(tercer);
                await _context.SaveChangesAsync();
            }
            else
            {
                tercer.Cognoms ??= Net(cognoms);
                tercer.Telefon ??= Net(telefon);
                tercer.Email ??= Net(email);
                tercer.Adreca ??= Net(adreca);
                tercer.CodiPostal ??= Net(codiPostal);
                tercer.Poblacio ??= Net(poblacio);
                tercer.UpdatedAt = DateTime.Now;
            }

            var alumne = await _context.CursetsAlumnes.FirstOrDefaultAsync(a => a.TercerId == tercer.Id);
            if (alumne is null)
            {
                alumne = new Alumne
                {
                    TercerId = tercer.Id,
                    Notes = Net(notes),
                    Empadronat = empadronatSiNou ?? false
                };
                _context.CursetsAlumnes.Add(alumne);
            }
            else if (!alumne.Actiu)
            {
                alumne.Actiu = true;
                alumne.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return alumne;
        }

        public async Task<Alumne> ActualitzarAlumneAsync(int id, ActualitzarAlumneRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nom))
                throw new ArgumentException("El nom de l'alumne és obligatori.", nameof(request));

            var alumne = await _context.CursetsAlumnes
                .Include(a => a.Tercer)
                .FirstOrDefaultAsync(a => a.Id == id)
                ?? throw new ArgumentException("L'alumne indicat no existeix.", nameof(id));

            if (alumne.Tercer is null)
                throw new ArgumentException("L'alumne no té persona associada.", nameof(id));

            alumne.Tercer.Nom = request.Nom.Trim();
            alumne.Tercer.Cognoms = string.IsNullOrWhiteSpace(request.Cognoms) ? null : request.Cognoms.Trim();
            alumne.Tercer.DNI = Net(request.Dni);
            alumne.Tercer.Telefon = request.Telefon;
            alumne.Tercer.Email = request.Email;
            alumne.Tercer.Adreca = Net(request.Adreca);
            alumne.Tercer.CodiPostal = Net(request.CodiPostal);
            alumne.Tercer.Poblacio = Net(request.Poblacio);
            alumne.Tercer.UpdatedAt = DateTime.Now;

            alumne.Notes = request.Notes;
            alumne.Empadronat = request.Empadronat;
            alumne.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            if (request.CursetIds is not null)
                await SincronitzarInscripcionsAsync(alumne.Id, request.CursetIds);
            return alumne;
        }

        public async Task CanviarEstatAlumneAsync(int id, bool actiu)
        {
            var alumne = await _context.CursetsAlumnes.FindAsync(id)
                ?? throw new ArgumentException("L'alumne indicat no existeix.", nameof(id));

            alumne.Actiu = actiu;
            alumne.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        /// <summary>Deixa les inscripcions ADMESES de l'alumne exactament iguals a <paramref name="cursetIds"/>
        /// (marcar = admesa, desmarcar = baixa). No toca sol·licituds ni llista d'espera.</summary>
        private async Task SincronitzarInscripcionsAsync(int alumneId, List<int> cursetIds)
        {
            var avui = DateTime.Today;

            var desitjats = await _context.Cursets
                .Where(c => cursetIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            var actuals = await _context.AlumnesCursets
                .Where(ac => ac.AlumneId == alumneId)
                .ToListAsync();

            foreach (var ac in actuals)
            {
                var voltHiSer = desitjats.Contains(ac.CursetId);
                if (voltHiSer && ac.Estat != EstatInscripcio.Admesa)
                {
                    ac.Estat = EstatInscripcio.Admesa;
                    ac.OrdreLlistaEspera = null;
                    ac.DataBaixa = null;
                    ac.DataAlta = avui;
                }
                else if (!voltHiSer && ac.Estat == EstatInscripcio.Admesa)
                {
                    ac.Estat = EstatInscripcio.Baixa;
                    ac.DataBaixa = avui;
                }
            }

            var nous = desitjats.Where(cid => actuals.All(ac => ac.CursetId != cid));
            foreach (var cid in nous)
            {
                _context.AlumnesCursets.Add(new AlumneCurset
                {
                    AlumneId = alumneId,
                    CursetId = cid,
                    Estat = EstatInscripcio.Admesa,
                    Origen = "Manual",
                    DataSollicitud = avui,
                    DataAlta = avui
                });
            }

            await _context.SaveChangesAsync();
        }

        // ==================== Fitxa d'alumne ====================

        public async Task<AlumneFitxaDto?> GetAlumneFitxaAsync(int id)
        {
            var a = await _context.CursetsAlumnes
                .Include(x => x.Tercer)
                .Include(x => x.Cursets).ThenInclude(ac => ac.Curset).ThenInclude(c => c!.TipusCurset)
                .Include(x => x.Cursets).ThenInclude(ac => ac.Curset).ThenInclude(c => c!.Professora)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (a is null) return null;

            var avui = DateTime.Today;
            var cursetIds = a.Cursets.Select(ac => ac.CursetId).ToList();

            var totalsEspera = await _context.AlumnesCursets
                .Where(ac => cursetIds.Contains(ac.CursetId) && ac.Estat == EstatInscripcio.LlistaEspera)
                .GroupBy(ac => ac.CursetId)
                .Select(g => new { CursetId = g.Key, N = g.Count() })
                .ToDictionaryAsync(x => x.CursetId, x => x.N);

            var liquidacions = await _context.CursetsLiquidacions
                .Where(l => l.AlumneId == id)
                .OrderByDescending(l => l.DataEmissio)
                .Select(l => new LiquidacioFitxaDto
                {
                    Id = l.Id,
                    Numero = l.Numero,
                    PeriodeEtiqueta = l.PeriodeEtiqueta,
                    CursetTitol = l.CursetTitol,
                    Import = l.Import,
                    Estat = l.Estat.ToString(),
                    DataCobrament = l.DataCobrament
                })
                .ToListAsync();

            static int OrdreEstat(EstatInscripcio e) => e switch
            {
                EstatInscripcio.Admesa => 0,
                EstatInscripcio.LlistaEspera => 1,
                EstatInscripcio.Sollicitada => 2,
                EstatInscripcio.Baixa => 3,
                _ => 4
            };

            var inscripcions = a.Cursets
                .OrderBy(ac => OrdreEstat(ac.Estat))
                .ThenBy(ac => ac.Curset?.Nom)
                .Select(ac => new InscripcioFitxaDto
                {
                    CursetId = ac.CursetId,
                    CursetTitol = ac.Curset?.TipusCurset != null
                        ? $"{ac.Curset.TipusCurset.Nom} - {ac.Curset.Nom}"
                        : (ac.Curset?.Nom ?? string.Empty),
                    DiaHora = FormatDiaHora(ac.Curset),
                    ProfessoraNom = ac.Curset?.Professora?.Nom,
                    Estat = ac.Estat.ToString(),
                    OrdreLlistaEspera = ac.OrdreLlistaEspera,
                    TotalLlistaEspera = totalsEspera.TryGetValue(ac.CursetId, out var n) ? n : 0,
                    EstatPeriode = EstatPeriodeInscripcio(ac.Curset?.InscripcioInici, ac.Curset?.InscripcioFi, avui),
                    DataSollicitud = ac.DataSollicitud,
                    DataAlta = ac.DataAlta,
                    DataBaixa = ac.DataBaixa,
                    Origen = ac.Origen
                })
                .ToList();

            return new AlumneFitxaDto
            {
                Id = a.Id,
                Nom = a.Tercer?.Nom ?? string.Empty,
                Cognoms = a.Tercer?.Cognoms,
                NomComplet = a.Tercer?.NomComplet ?? "(sense persona)",
                Dni = a.Tercer?.DNI,
                Telefon = a.Tercer?.Telefon,
                Email = a.Tercer?.Email,
                Adreca = a.Tercer?.Adreca,
                CodiPostal = a.Tercer?.CodiPostal,
                Poblacio = a.Tercer?.Poblacio,
                Notes = a.Notes,
                Actiu = a.Actiu,
                Empadronat = a.Empadronat,
                DataAlta = a.CreatedAt,
                Deute = liquidacions.Where(l => l.Estat == nameof(EstatLiquidacio.Emesa)).Sum(l => l.Import),
                DadesLiquidacioFalten = LiquidacioDades.QueFalten(
                    a.Tercer?.DNI, a.Tercer?.Adreca, a.Tercer?.CodiPostal, a.Tercer?.Poblacio),
                Inscripcions = inscripcions,
                Liquidacions = liquidacions
            };
        }

        public async Task CanviarEstatInscripcioAsync(int alumneId, int cursetId, EstatInscripcio nou)
        {
            var ac = await _context.AlumnesCursets
                .FirstOrDefaultAsync(x => x.AlumneId == alumneId && x.CursetId == cursetId)
                ?? throw new ArgumentException("La inscripció indicada no existeix.");

            var avui = DateTime.Today;

            switch (nou)
            {
                case EstatInscripcio.Admesa:
                {
                    var abansEspera = ac.Estat == EstatInscripcio.LlistaEspera ? ac.OrdreLlistaEspera : null;
                    ac.Estat = EstatInscripcio.Admesa;
                    ac.OrdreLlistaEspera = null;
                    ac.DataBaixa = null;
                    ac.DataAlta = avui;
                    if (abansEspera is int pos)
                    {
                        var posteriors = await _context.AlumnesCursets
                            .Where(x => x.CursetId == cursetId && x.Estat == EstatInscripcio.LlistaEspera
                                        && x.OrdreLlistaEspera > pos)
                            .ToListAsync();
                        foreach (var p in posteriors)
                            p.OrdreLlistaEspera -= 1;
                    }
                    break;
                }
                case EstatInscripcio.LlistaEspera:
                {
                    var maxOrdre = await _context.AlumnesCursets
                        .Where(x => x.CursetId == cursetId && x.Estat == EstatInscripcio.LlistaEspera
                                    && x.AlumneId != alumneId)
                        .MaxAsync(x => (int?)x.OrdreLlistaEspera) ?? 0;
                    ac.Estat = EstatInscripcio.LlistaEspera;
                    ac.OrdreLlistaEspera = maxOrdre + 1;
                    ac.DataBaixa = null;
                    break;
                }
                case EstatInscripcio.Baixa:
                    ac.Estat = EstatInscripcio.Baixa;
                    ac.DataBaixa = avui;
                    ac.OrdreLlistaEspera = null;
                    break;
                case EstatInscripcio.Rebutjada:
                    ac.Estat = EstatInscripcio.Rebutjada;
                    ac.OrdreLlistaEspera = null;
                    break;
                default:
                    throw new ArgumentException("Estat d'inscripció no vàlid.", nameof(nou));
            }

            await _context.SaveChangesAsync();
        }

        public async Task AfegirInscripcioAsync(int alumneId, int cursetId)
        {
            if (!await _context.CursetsAlumnes.AnyAsync(a => a.Id == alumneId))
                throw new ArgumentException("L'alumne indicat no existeix.", nameof(alumneId));
            if (!await _context.Cursets.AnyAsync(c => c.Id == cursetId))
                throw new ArgumentException("El curset indicat no existeix.", nameof(cursetId));

            var avui = DateTime.Today;
            var ac = await _context.AlumnesCursets
                .FirstOrDefaultAsync(x => x.AlumneId == alumneId && x.CursetId == cursetId);

            if (ac is null)
            {
                _context.AlumnesCursets.Add(new AlumneCurset
                {
                    AlumneId = alumneId,
                    CursetId = cursetId,
                    Estat = EstatInscripcio.Admesa,
                    Origen = "Manual",
                    DataSollicitud = avui,
                    DataAlta = avui
                });
            }
            else if (ac.Estat != EstatInscripcio.Admesa)
            {
                ac.Estat = EstatInscripcio.Admesa;
                ac.OrdreLlistaEspera = null;
                ac.DataBaixa = null;
                ac.DataAlta = avui;
            }
            else
            {
                return;
            }

            await _context.SaveChangesAsync();
        }

        // ==================== Sol·licituds públiques / sorteig ====================

        public async Task<string> CrearSollicitudPublicaAsync(SollicitudInscripcioPublicaRequest req)
        {
            if (!string.IsNullOrWhiteSpace(req.Honeypot))
                throw new ArgumentException("Sol·licitud no vàlida.");
            if (!req.ConsentimentRgpd)
                throw new ArgumentException("Cal acceptar el tractament de dades per continuar.");
            if (string.IsNullOrWhiteSpace(req.Nom))
                throw new ArgumentException("El nom és obligatori.");
            if (NetDni(req.Dni) is null)
                throw new ArgumentException("El DNI/NIF és obligatori.");

            var curset = await _context.Cursets.FirstOrDefaultAsync(c => c.Id == req.CursetId && c.Actiu)
                ?? throw new ArgumentException("El curs indicat no existeix.");

            if (EstatPeriodeInscripcio(curset.InscripcioInici, curset.InscripcioFi, DateTime.Today) != "Oberta")
                throw new ArgumentException("El període d'inscripció d'aquest curs no està obert.");

            // req.ConsentimentRgpd ja s'ha validat més amunt: la pròpia persona interessada
            // ha donat el consentiment, així que compleix l'autorització.
            var alumne = await ResoldreAlumneAsync(req.Nom, req.Cognoms, req.Dni, req.Telefon, req.Email,
                req.Adreca, req.CodiPostal, req.Poblacio, empadronatSiNou: req.DeclaraEmpadronat, notes: null,
                autoritzacioTractamentSignada: true);

            var ara = DateTime.Now;
            var ac = await _context.AlumnesCursets
                .FirstOrDefaultAsync(x => x.AlumneId == alumne.Id && x.CursetId == curset.Id);

            if (ac is null)
            {
                _context.AlumnesCursets.Add(new AlumneCurset
                {
                    AlumneId = alumne.Id,
                    CursetId = curset.Id,
                    Estat = EstatInscripcio.Sollicitada,
                    Origen = "Web",
                    DataSollicitud = ara,
                    DataAlta = ara
                });
            }
            else if (ac.Estat is EstatInscripcio.Sollicitada or EstatInscripcio.LlistaEspera)
            {
                throw new ArgumentException("Ja tens una sol·licitud registrada per a aquest curs.");
            }
            else if (ac.Estat == EstatInscripcio.Admesa)
            {
                throw new ArgumentException("Ja estàs inscrit/a en aquest curs.");
            }
            else
            {
                ac.Estat = EstatInscripcio.Sollicitada;
                ac.Origen = "Web";
                ac.DataSollicitud = ara;
                ac.OrdreLlistaEspera = null;
                ac.DataBaixa = null;
            }

            await _context.SaveChangesAsync();
            return alumne.NomComplet;
        }

        public async Task<SorteigEstatDto> GetSorteigEstatAsync(int cursetId)
        {
            var curset = await _context.Cursets
                .Include(c => c.TipusCurset)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == cursetId)
                ?? throw new ArgumentException("El curset indicat no existeix.", nameof(cursetId));

            var inscripcions = await _context.AlumnesCursets
                .Where(ac => ac.CursetId == cursetId)
                .Include(ac => ac.Alumne).ThenInclude(a => a!.Tercer)
                .AsNoTracking()
                .ToListAsync();

            var admesos = inscripcions.Count(ac => ac.Estat == EstatInscripcio.Admesa);
            var pendents = inscripcions
                .Where(ac => ac.Estat is EstatInscripcio.Sollicitada or EstatInscripcio.LlistaEspera)
                .OrderBy(ac => ac.Estat == EstatInscripcio.LlistaEspera ? 0 : 1)
                .ThenBy(ac => ac.OrdreLlistaEspera ?? int.MaxValue)
                .ThenBy(ac => ac.DataSollicitud ?? ac.DataAlta)
                .Select(ac => new SollicitudPendentDto
                {
                    AlumneId = ac.AlumneId,
                    NomComplet = ac.Alumne?.Tercer?.NomComplet ?? "(sense persona)",
                    Dni = ac.Alumne?.Tercer?.DNI,
                    Empadronat = ac.Alumne?.Empadronat ?? false,
                    Estat = ac.Estat.ToString(),
                    OrdreLlistaEspera = ac.OrdreLlistaEspera,
                    DataSollicitud = ac.DataSollicitud,
                    Origen = ac.Origen
                })
                .ToList();

            var nSollicituds = inscripcions.Count(ac => ac.Estat == EstatInscripcio.Sollicitada);
            int? lliures = curset.MaxPlaces.HasValue ? Math.Max(0, curset.MaxPlaces.Value - admesos) : null;
            var nAdmetria = lliures.HasValue ? Math.Min(nSollicituds, lliures.Value) : nSollicituds;

            return new SorteigEstatDto
            {
                CursetId = curset.Id,
                CursetTitol = curset.TipusCurset != null ? $"{curset.TipusCurset.Nom} - {curset.Nom}" : curset.Nom,
                PlacesTotals = curset.MaxPlaces,
                Admesos = admesos,
                PlacesLliures = lliures,
                Sollicituds = nSollicituds,
                EnLlistaEspera = inscripcions.Count(ac => ac.Estat == EstatInscripcio.LlistaEspera),
                NAdmetria = nAdmetria,
                NAEspera = nSollicituds - nAdmetria,
                UltimSorteigNumero = curset.SorteigNumero,
                UltimSorteigData = curset.SorteigData,
                Pendents = pendents
            };
        }

        /// <summary>
        /// Sorteig "per número de tall" (com els sorteigs públics): els sol·licitants
        /// s'ordenen per data de sol·licitud (posicions 1..N), es treu un número a
        /// l'atzar entre 1 i N i s'adjudiquen les places lliures a partir d'aquesta
        /// posició, en ordre i tornant al principi si cal. La resta van a la llista
        /// d'espera en el mateix ordre (continuant després de l'últim admès).
        /// </summary>
        public async Task<SorteigResultDto> FerSorteigAsync(int cursetId)
        {
            var curset = await _context.Cursets.FirstOrDefaultAsync(c => c.Id == cursetId)
                ?? throw new ArgumentException("El curset indicat no existeix.", nameof(cursetId));

            var totes = await _context.AlumnesCursets
                .Where(ac => ac.CursetId == cursetId)
                .Include(ac => ac.Alumne).ThenInclude(a => a!.Tercer)
                .ToListAsync();

            // Ordre estable dels sol·licitants: per data de sol·licitud, després per alumne.
            var candidats = totes
                .Where(ac => ac.Estat == EstatInscripcio.Sollicitada)
                .OrderBy(ac => ac.DataSollicitud ?? ac.DataAlta)
                .ThenBy(ac => ac.AlumneId)
                .ToList();
            if (candidats.Count == 0)
                throw new ArgumentException("No hi ha cap sol·licitud pendent per sortejar.");

            var n = candidats.Count;
            var admesos = totes.Count(ac => ac.Estat == EstatInscripcio.Admesa);
            var maxOrdreEspera = totes.Where(ac => ac.Estat == EstatInscripcio.LlistaEspera)
                .Select(ac => ac.OrdreLlistaEspera ?? 0)
                .DefaultIfEmpty(0)
                .Max();

            int lliures = curset.MaxPlaces.HasValue
                ? Math.Max(0, curset.MaxPlaces.Value - admesos)
                : n;

            // Número de tall: posició 1..N a partir de la qual s'adjudiquen les places.
            var numero = new Random().Next(1, n + 1);
            var avui = DateTime.Today;
            var resultat = new SorteigResultDto { Numero = numero, TotalInscrits = n };

            for (int k = 0; k < n; k++)
            {
                var ac = candidats[(numero - 1 + k) % n];
                var nom = ac.Alumne?.Tercer?.NomComplet ?? "(sense persona)";
                if (k < lliures)
                {
                    ac.Estat = EstatInscripcio.Admesa;
                    ac.OrdreLlistaEspera = null;
                    ac.DataBaixa = null;
                    ac.DataAlta = avui;
                    resultat.Admesos.Add(nom);
                }
                else
                {
                    ac.Estat = EstatInscripcio.LlistaEspera;
                    ac.OrdreLlistaEspera = ++maxOrdreEspera;
                    resultat.AEspera.Add(nom);
                }
            }

            curset.SorteigNumero = numero;
            curset.SorteigData = avui;
            await _context.SaveChangesAsync();

            resultat.NAdmesos = resultat.Admesos.Count;
            resultat.NAEspera = resultat.AEspera.Count;
            return resultat;
        }
    }
}
