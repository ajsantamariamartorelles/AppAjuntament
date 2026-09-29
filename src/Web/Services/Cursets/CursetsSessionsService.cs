using System.Net;
using Microsoft.EntityFrameworkCore;
using AppAjuntament.Models;
using AppAjuntament.Models.Cursets;
using AppAjuntament.Models.Cursets.Dto;

namespace AppAjuntament.Services.Cursets
{
    /// <summary>Obertura de sessions de classe i passat/desat de l'assistència.</summary>
    public interface ICursetsSessionsService
    {
        /// <summary>
        /// "Inicia classe": crea la sessió del dia per al curset (o retorna la ja existent si
        /// ja s'havia obert avui) amb tots els alumnes actius del curset marcats presents.
        /// </summary>
        Task<SessioObertaDto> ObrirSessioAsync(int cursetId, int professoraId, DateTime? data = null);

        /// <summary>Desa l'assistència final (qui s'ha desmarcat com a absent) i tanca la sessió.</summary>
        Task TancarSessioAsync(int sessioId, GuardarAssistenciaRequest request);

        Task<List<SessioObertaDto>> GetHistoricAsync(int cursetId);

        /// <summary>Totes les sessions d'un curset amb el detall d'assistència de cada alumne.</summary>
        Task<List<SessioAssistenciaDto>> GetAssistenciesPerCursetAsync(int cursetId);

        /// <summary>Historial d'assistència d'un alumne a totes les seves sessions.</summary>
        Task<ResumAssistenciaAlumneDto?> GetAssistenciesPerAlumneAsync(int alumneId);

        /// <summary>
        /// Inscripcions actives (Admesa) amb absències consecutives: sessions tancades
        /// més recents en què l'alumne ha faltat seguides, sense cap presència pel mig.
        /// Només retorna les que en tenen alguna (&gt; 0); el filtre pel mínim el fa qui truca.
        /// </summary>
        Task<List<AbsenciaAlumneDto>> GetAbsenciesConsecutivesAsync();

        /// <summary>
        /// Dona de baixa les inscripcions seleccionades (per absències no justificades) i
        /// avisa per correu els alumnes que tinguin email registrat.
        /// </summary>
        Task<DonarBaixaPerAbsenciesResultDto> DonarBaixaPerAbsenciesAsync(
            List<BaixaPerAbsenciesItem> seleccio, string? missatge);

        /// <summary>
        /// Alumnes que consten apuntats a un curset en una data (Admesa o Baixa posterior a la
        /// data, amb l'alta ja feta), tots presents per defecte. Base per crear una llista des de la web.
        /// </summary>
        Task<List<AssistenciaAlumneDto>> GetAlumnesPerLlistaAsync(int cursetId, DateTime data);

        /// <summary>Crea una llista d'assistència (sessió tancada) des de la web. Retorna l'id de la sessió.</summary>
        Task<int> CrearLlistaWebAsync(int cursetId, DateTime data, string? notaSessio, List<AssistenciaAlumneDto> alumnes);

        /// <summary>Elimina una sessió amb la seva assistència, sempre que no estigui liquidada.</summary>
        Task EliminarSessioAsync(int sessioId);

        /// <summary>Configuració (SMTP + plantilla) del correu de baixa per absències.</summary>
        Task<EmailAbsenciesConfigDto> GetEmailConfigAsync();
        Task DesarEmailConfigAsync(EmailAbsenciesConfigDto dto);
    }

    public class CursetsSessionsService : ICursetsSessionsService
    {
        private readonly GestorSubvencionsContext _context;
        private readonly IEmailService _emailService;

        public CursetsSessionsService(GestorSubvencionsContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<SessioObertaDto> ObrirSessioAsync(int cursetId, int professoraId, DateTime? data = null)
        {
            var dataSessio = (data ?? DateTime.Now).Date;

            var cursetExisteix = await _context.Cursets.AnyAsync(c => c.Id == cursetId && c.Actiu);
            if (!cursetExisteix)
                throw new ArgumentException("El curset indicat no existeix o no està actiu.", nameof(cursetId));

            var sessio = await _context.CursetsSessions
                .FirstOrDefaultAsync(s => s.CursetId == cursetId && s.Data == dataSessio);

            if (sessio == null)
            {
                sessio = new Sessio
                {
                    CursetId = cursetId,
                    ProfessoraId = professoraId,
                    Data = dataSessio,
                    Estat = EstatSessio.Oberta
                };
                _context.CursetsSessions.Add(sessio);
                await _context.SaveChangesAsync();

                var alumnesCurset = await _context.AlumnesCursets
                    .Where(ac => ac.CursetId == cursetId && ac.Estat == EstatInscripcio.Admesa && ac.Alumne!.Actiu)
                    .Select(ac => ac.AlumneId)
                    .ToListAsync();

                foreach (var alumneId in alumnesCurset)
                {
                    _context.CursetsAssistencies.Add(new Assistencia
                    {
                        SessioId = sessio.Id,
                        AlumneId = alumneId,
                        Present = true
                    });
                }
                await _context.SaveChangesAsync();
            }

            return await CarregarSessioDtoAsync(sessio.Id);
        }

        public async Task TancarSessioAsync(int sessioId, GuardarAssistenciaRequest request)
        {
            var sessio = await _context.CursetsSessions
                .Include(s => s.Assistencies)
                .FirstOrDefaultAsync(s => s.Id == sessioId);

            if (sessio == null)
                throw new ArgumentException("La sessió indicada no existeix.", nameof(sessioId));

            var assistenciesPerAlumne = sessio.Assistencies.ToDictionary(a => a.AlumneId);
            foreach (var alumne in request.Alumnes)
            {
                if (assistenciesPerAlumne.TryGetValue(alumne.AlumneId, out var assistencia))
                {
                    assistencia.Present = alumne.Present;
                    assistencia.Nota = alumne.Nota;
                }
            }

            sessio.NotaSessio = request.NotaSessio;
            sessio.Estat = EstatSessio.Tancada;
            sessio.ClosedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task<List<SessioObertaDto>> GetHistoricAsync(int cursetId)
        {
            var sessioIds = await _context.CursetsSessions
                .Where(s => s.CursetId == cursetId)
                .OrderByDescending(s => s.Data)
                .Select(s => s.Id)
                .ToListAsync();

            var resultat = new List<SessioObertaDto>();
            foreach (var id in sessioIds)
                resultat.Add(await CarregarSessioDtoAsync(id));
            return resultat;
        }

        public async Task<List<SessioAssistenciaDto>> GetAssistenciesPerCursetAsync(int cursetId)
        {
            var sessions = await _context.CursetsSessions
                .Where(s => s.CursetId == cursetId)
                .Include(s => s.Curset)
                .Include(s => s.Assistencies)
                    .ThenInclude(a => a.Alumne)
                        .ThenInclude(al => al!.Tercer)
                .OrderByDescending(s => s.Data)
                .AsNoTracking()
                .ToListAsync();

            var periodes = await _context.CursetsLiquidacions
                .Where(l => l.CursetId == cursetId && l.Estat != EstatLiquidacio.Anullada)
                .Select(l => new { l.PeriodeInici, l.PeriodeFi })
                .AsNoTracking()
                .ToListAsync();

            return sessions.Select(s => new SessioAssistenciaDto
            {
                Liquidada = periodes.Any(p => s.Data >= p.PeriodeInici.Date && s.Data <= p.PeriodeFi.Date),
                SessioId = s.Id,
                CursetId = s.CursetId,
                CursetNom = s.Curset?.Nom ?? string.Empty,
                Data = s.Data,
                Estat = s.Estat.ToString(),
                NotaSessio = s.NotaSessio,
                NumTotal = s.Assistencies.Count,
                NumPresents = s.Assistencies.Count(a => a.Present),
                Alumnes = s.Assistencies
                    .Select(a => new AssistenciaAlumneDto
                    {
                        AlumneId = a.AlumneId,
                        NomComplet = NomTercer(a.Alumne?.Tercer),
                        Present = a.Present,
                        Nota = a.Nota
                    })
                    .OrderBy(a => a.NomComplet)
                    .ToList()
            }).ToList();
        }

        public async Task<ResumAssistenciaAlumneDto?> GetAssistenciesPerAlumneAsync(int alumneId)
        {
            var alumne = await _context.CursetsAlumnes
                .Include(a => a.Tercer)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == alumneId);
            if (alumne is null)
                return null;

            var files = await _context.CursetsAssistencies
                .Where(a => a.AlumneId == alumneId)
                .Include(a => a.Sessio)
                    .ThenInclude(s => s!.Curset)
                .AsNoTracking()
                .ToListAsync();

            var sessions = files
                .Select(f => new AssistenciaPerAlumneDto
                {
                    SessioId = f.SessioId,
                    CursetId = f.Sessio?.CursetId ?? 0,
                    CursetNom = f.Sessio?.Curset?.Nom ?? string.Empty,
                    Data = f.Sessio?.Data ?? default,
                    Estat = f.Sessio?.Estat.ToString() ?? string.Empty,
                    Present = f.Present,
                    Nota = f.Nota
                })
                .OrderByDescending(s => s.Data)
                .ToList();

            return new ResumAssistenciaAlumneDto
            {
                AlumneId = alumne.Id,
                NomComplet = NomTercer(alumne.Tercer),
                TotalSessions = sessions.Count,
                TotalPresents = sessions.Count(s => s.Present),
                Sessions = sessions
            };
        }

        public async Task<List<AssistenciaAlumneDto>> GetAlumnesPerLlistaAsync(int cursetId, DateTime data)
        {
            var dia = data.Date;
            var inscrits = await _context.AlumnesCursets
                .Where(ac => ac.CursetId == cursetId
                    && (ac.Estat == EstatInscripcio.Admesa || ac.Estat == EstatInscripcio.Baixa)
                    && ac.DataAlta <= dia
                    && (ac.DataBaixa == null || ac.DataBaixa >= dia))
                .Include(ac => ac.Alumne).ThenInclude(a => a!.Tercer)
                .AsNoTracking()
                .ToListAsync();

            return inscrits
                .Select(ac => new AssistenciaAlumneDto
                {
                    AlumneId = ac.AlumneId,
                    NomComplet = NomTercer(ac.Alumne?.Tercer),
                    Present = true
                })
                .OrderBy(a => a.NomComplet)
                .ToList();
        }

        public async Task<int> CrearLlistaWebAsync(int cursetId, DateTime data, string? notaSessio, List<AssistenciaAlumneDto> alumnes)
        {
            var dia = data.Date;
            if (dia > DateTime.Today)
                throw new ArgumentException("No es pot crear una llista amb una data futura.");

            var curset = await _context.Cursets.FirstOrDefaultAsync(c => c.Id == cursetId)
                ?? throw new ArgumentException("El curset indicat no existeix.");

            if (await _context.CursetsSessions.AnyAsync(s => s.CursetId == cursetId && s.Data == dia))
                throw new ArgumentException("Ja hi ha una llista d'assistència d'aquest curset per a aquesta data.");

            // Només s'accepten alumnes que consten apuntats en aquella data.
            var valids = (await GetAlumnesPerLlistaAsync(cursetId, dia)).Select(a => a.AlumneId).ToHashSet();
            var ara = DateTime.Now;

            var sessio = new Sessio
            {
                CursetId = cursetId,
                ProfessoraId = curset.ProfessoraId,
                Data = dia,
                Estat = EstatSessio.Tancada,
                NotaSessio = string.IsNullOrWhiteSpace(notaSessio) ? null : notaSessio.Trim(),
                CreatedAt = ara,
                ClosedAt = ara
            };
            _context.CursetsSessions.Add(sessio);
            await _context.SaveChangesAsync();

            foreach (var a in (alumnes ?? new()).Where(a => valids.Contains(a.AlumneId)).DistinctBy(a => a.AlumneId))
            {
                _context.CursetsAssistencies.Add(new Assistencia
                {
                    SessioId = sessio.Id,
                    AlumneId = a.AlumneId,
                    Present = a.Present,
                    Nota = string.IsNullOrWhiteSpace(a.Nota) ? null : a.Nota.Trim()
                });
            }
            await _context.SaveChangesAsync();
            return sessio.Id;
        }

        public async Task EliminarSessioAsync(int sessioId)
        {
            var sessio = await _context.CursetsSessions
                .Include(s => s.Assistencies)
                .FirstOrDefaultAsync(s => s.Id == sessioId)
                ?? throw new ArgumentException("La sessió indicada no existeix.");

            var liquidada = await _context.CursetsLiquidacions.AnyAsync(l =>
                l.CursetId == sessio.CursetId
                && l.Estat != EstatLiquidacio.Anullada
                && sessio.Data >= l.PeriodeInici && sessio.Data <= l.PeriodeFi);
            if (liquidada)
                throw new ArgumentException(
                    "No es pot eliminar: la sessió està inclosa en una liquidació. Anul·la primer la liquidació.");

            _context.CursetsAssistencies.RemoveRange(sessio.Assistencies);
            _context.CursetsSessions.Remove(sessio);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AbsenciaAlumneDto>> GetAbsenciesConsecutivesAsync()
        {
            var actives = await _context.AlumnesCursets
                .Where(ac => ac.Estat == EstatInscripcio.Admesa)
                .Include(ac => ac.Alumne).ThenInclude(a => a!.Tercer)
                .Include(ac => ac.Curset).ThenInclude(c => c!.TipusCurset)
                .AsNoTracking()
                .ToListAsync();

            if (actives.Count == 0)
                return new List<AbsenciaAlumneDto>();

            var cursetIds = actives.Select(ac => ac.CursetId).Distinct().ToList();
            var alumneIds = actives.Select(ac => ac.AlumneId).Distinct().ToList();

            var assistencies = await _context.CursetsAssistencies
                .Where(a => alumneIds.Contains(a.AlumneId)
                            && a.Sessio!.Estat == EstatSessio.Tancada
                            && cursetIds.Contains(a.Sessio.CursetId))
                .Select(a => new { a.AlumneId, CursetId = a.Sessio!.CursetId, a.Sessio.Data, a.Present })
                .AsNoTracking()
                .ToListAsync();

            var perAlumneCurset = assistencies
                .GroupBy(a => (a.AlumneId, a.CursetId))
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Data).ToList());

            var resultat = new List<AbsenciaAlumneDto>();
            foreach (var ac in actives)
            {
                if (!perAlumneCurset.TryGetValue((ac.AlumneId, ac.CursetId), out var files) || files.Count == 0)
                    continue;

                // Ja ordenades per data descendent: comptem les faltes seguides des de la
                // sessió més recent fins a la primera presència (o fins que s'acabin).
                var consecutives = 0;
                DateTime? ultimaPresencia = null;
                foreach (var f in files)
                {
                    if (!f.Present)
                    {
                        consecutives++;
                        continue;
                    }
                    ultimaPresencia = f.Data;
                    break;
                }

                if (consecutives == 0)
                    continue;

                resultat.Add(new AbsenciaAlumneDto
                {
                    AlumneId = ac.AlumneId,
                    NomComplet = NomTercer(ac.Alumne?.Tercer),
                    Email = ac.Alumne?.Tercer?.Email,
                    Telefon = ac.Alumne?.Tercer?.Telefon,
                    CursetId = ac.CursetId,
                    CursetTitol = ac.Curset?.TipusCurset != null
                        ? $"{ac.Curset.TipusCurset.Nom} - {ac.Curset.Nom}"
                        : (ac.Curset?.Nom ?? string.Empty),
                    AbsenciesConsecutives = consecutives,
                    DataUltimaSessio = files[0].Data,
                    DataUltimaPresencia = ultimaPresencia
                });
            }

            return resultat
                .OrderByDescending(r => r.AbsenciesConsecutives)
                .ThenBy(r => r.NomComplet)
                .ToList();
        }

        public async Task<DonarBaixaPerAbsenciesResultDto> DonarBaixaPerAbsenciesAsync(
            List<BaixaPerAbsenciesItem> seleccio, string? missatge)
        {
            var resultat = new DonarBaixaPerAbsenciesResultDto();
            if (seleccio is null || seleccio.Count == 0)
                return resultat;

            var cfg = await GetOrCreateEmailConfigAsync();
            var parametres = new EmailEnviamentParams
            {
                Host = cfg.SmtpHost ?? string.Empty,
                Port = cfg.SmtpPort,
                Ssl = cfg.SmtpSsl,
                User = cfg.SmtpUser,
                Password = cfg.SmtpPassword,
                From = cfg.RemitentEmail ?? string.Empty,
                FromName = cfg.RemitentNom
            };
            var institucio = string.IsNullOrWhiteSpace(cfg.RemitentNom) ? "l'Ajuntament" : cfg.RemitentNom;
            var assumptePatro = string.IsNullOrWhiteSpace(cfg.Assumpte) ? "Baixa del curset «{CURSET}»" : cfg.Assumpte;
            var cosPatro = string.IsNullOrWhiteSpace(cfg.CosHtml) ? EmailAbsenciesConfig.CosHtmlPerDefecte : cfg.CosHtml;

            foreach (var item in seleccio.DistinctBy(x => (x.AlumneId, x.CursetId)))
            {
                var ac = await _context.AlumnesCursets
                    .Include(x => x.Alumne).ThenInclude(a => a!.Tercer)
                    .Include(x => x.Curset).ThenInclude(c => c!.TipusCurset)
                    .FirstOrDefaultAsync(x => x.AlumneId == item.AlumneId && x.CursetId == item.CursetId);

                // Si ja no hi és o l'estat ja ha canviat (p. ex. doble clic), la saltem.
                if (ac is null || ac.Estat != EstatInscripcio.Admesa)
                    continue;

                var nom = NomTercer(ac.Alumne?.Tercer);
                var email = ac.Alumne?.Tercer?.Email;
                var titol = ac.Curset?.TipusCurset != null
                    ? $"{ac.Curset.TipusCurset.Nom} - {ac.Curset.Nom}"
                    : (ac.Curset?.Nom ?? string.Empty);

                ac.Estat = EstatInscripcio.Baixa;
                ac.DataBaixa = DateTime.Today;
                ac.OrdreLlistaEspera = null;
                await _context.SaveChangesAsync();
                resultat.NumBaixes++;

                if (string.IsNullOrWhiteSpace(email))
                {
                    resultat.NumSenseEmail++;
                    continue;
                }

                try
                {
                    var assumpte = assumptePatro
                        .Replace("{NOM}", nom)
                        .Replace("{CURSET}", titol)
                        .Replace("{INSTITUCIO}", institucio);
                    var missatgeHtml = string.IsNullOrWhiteSpace(missatge)
                        ? string.Empty
                        : $"<p>{WebUtility.HtmlEncode(missatge)}</p>";
                    var cos = cosPatro
                        .Replace("{NOM}", WebUtility.HtmlEncode(nom))
                        .Replace("{CURSET}", WebUtility.HtmlEncode(titol))
                        .Replace("{INSTITUCIO}", WebUtility.HtmlEncode(institucio))
                        .Replace("{MISSATGE}", missatgeHtml);

                    var enviat = await _emailService.EnviarAsync(parametres, email, assumpte, cos);
                    if (enviat)
                        resultat.NumEmailsEnviats++;
                    else
                        resultat.Errors.Add($"{nom}: no s'ha pogut enviar el correu (revisa la configuració a «Configuració del correu»).");
                }
                catch (Exception ex)
                {
                    resultat.Errors.Add($"{nom}: {ex.Message}");
                }
            }

            return resultat;
        }

        // ==================== Configuració del correu de baixa ====================

        public async Task<EmailAbsenciesConfigDto> GetEmailConfigAsync()
        {
            var c = await GetOrCreateEmailConfigAsync();
            return new EmailAbsenciesConfigDto
            {
                SmtpHost = c.SmtpHost ?? string.Empty,
                SmtpPort = c.SmtpPort,
                SmtpSsl = c.SmtpSsl,
                SmtpUser = c.SmtpUser ?? string.Empty,
                SmtpPassword = c.SmtpPassword ?? string.Empty,
                RemitentEmail = c.RemitentEmail ?? string.Empty,
                RemitentNom = c.RemitentNom ?? string.Empty,
                Assumpte = c.Assumpte ?? string.Empty,
                CosHtml = c.CosHtml ?? string.Empty
            };
        }

        public async Task DesarEmailConfigAsync(EmailAbsenciesConfigDto dto)
        {
            var c = await GetOrCreateEmailConfigAsync();
            c.SmtpHost = string.IsNullOrWhiteSpace(dto.SmtpHost) ? null : dto.SmtpHost.Trim();
            c.SmtpPort = dto.SmtpPort > 0 ? dto.SmtpPort : 587;
            c.SmtpSsl = dto.SmtpSsl;
            c.SmtpUser = string.IsNullOrWhiteSpace(dto.SmtpUser) ? null : dto.SmtpUser.Trim();
            c.SmtpPassword = string.IsNullOrWhiteSpace(dto.SmtpPassword) ? null : dto.SmtpPassword;
            c.RemitentEmail = string.IsNullOrWhiteSpace(dto.RemitentEmail) ? null : dto.RemitentEmail.Trim();
            c.RemitentNom = string.IsNullOrWhiteSpace(dto.RemitentNom)
                ? "Ajuntament de Santa Maria de Martorelles"
                : dto.RemitentNom.Trim();
            c.Assumpte = string.IsNullOrWhiteSpace(dto.Assumpte)
                ? "Baixa del curset «{CURSET}» per inassistència"
                : dto.Assumpte.Trim();
            c.CosHtml = string.IsNullOrWhiteSpace(dto.CosHtml) ? EmailAbsenciesConfig.CosHtmlPerDefecte : dto.CosHtml;
            c.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        private async Task<EmailAbsenciesConfig> GetOrCreateEmailConfigAsync()
        {
            var c = await _context.CursetsEmailAbsenciesConfig.FirstOrDefaultAsync(x => x.Id == 1);
            if (c != null)
                return c;

            c = new EmailAbsenciesConfig
            {
                Id = 1,
                SmtpPort = 587,
                SmtpSsl = true,
                RemitentNom = "Ajuntament de Santa Maria de Martorelles",
                Assumpte = "Baixa del curset «{CURSET}» per inassistència",
                CosHtml = EmailAbsenciesConfig.CosHtmlPerDefecte,
                UpdatedAt = DateTime.Now
            };
            _context.CursetsEmailAbsenciesConfig.Add(c);
            await _context.SaveChangesAsync();
            return c;
        }

        private static string NomTercer(AppAjuntament.Models.Tercers.Tercer? tercer)
        {
            if (tercer is null)
                return "(sense nom)";
            return string.IsNullOrWhiteSpace(tercer.Cognoms)
                ? tercer.Nom
                : $"{tercer.Nom} {tercer.Cognoms}";
        }

        private async Task<SessioObertaDto> CarregarSessioDtoAsync(int sessioId)
        {
            var sessio = await _context.CursetsSessions
                .Include(s => s.Assistencies)
                    .ThenInclude(a => a.Alumne)
                        .ThenInclude(al => al!.Tercer)
                .FirstAsync(s => s.Id == sessioId);

            return new SessioObertaDto
            {
                SessioId = sessio.Id,
                CursetId = sessio.CursetId,
                Data = sessio.Data,
                Alumnes = sessio.Assistencies
                    .Select(a => new AssistenciaAlumneDto
                    {
                        AlumneId = a.AlumneId,
                        NomComplet = a.Alumne?.Tercer != null
                            ? (string.IsNullOrWhiteSpace(a.Alumne.Tercer.Cognoms) ? a.Alumne.Tercer.Nom : $"{a.Alumne.Tercer.Nom} {a.Alumne.Tercer.Cognoms}")
                            : "(sense nom)",
                        Present = a.Present,
                        Nota = a.Nota
                    })
                    .OrderBy(a => a.NomComplet)
                    .ToList()
            };
        }
    }
}
