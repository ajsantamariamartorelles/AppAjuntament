using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AppAjuntament.Models;
using AppAjuntament.Models.Base.Usuari;
using AppAjuntament.Models.Base.Rol;
using AppAjuntament.Models.Base.Regidor;
using AppAjuntament.Models.Cursets;
using AppAjuntament.Models.Cursets.Dto;

namespace AppAjuntament.Services.Cursets
{
    public interface ICursetsAuthService
    {
        /// <summary>Login a l'app de Cursets: professora (taula usuaris) o controlador/regidor (taula regidors). Retorna null si les credencials no són vàlides.</summary>
        Task<LoginResponse?> LoginAsync(string email, string password);

        /// <summary>
        /// Login a l'app via compte de Microsoft (Azure AD): l'email ja ve autenticat
        /// per l'AAD, no cal contrasenya. Professora si té l'usuari + rol; si no,
        /// regidor del mandat actual (Orde != null). Retorna null si cap dels correus
        /// candidats del token correspon a una professora ni a un regidor vigent.
        /// Es passen diversos candidats perquè l'UPN d'Azure (…@…onmicrosoft.com) sol
        /// diferir del correu corporatiu guardat a la BD.
        /// </summary>
        Task<LoginResponse?> LoginMicrosoftAsync(IReadOnlyList<string> emails);

        /// <summary>Crea o actualitza les credencials d'una professora. Només s'ha de cridar des d'un endpoint protegit amb autenticació web (Azure AD), mai des del Bearer de Cursets.</summary>
        Task<Usuari> CrearOActualitzarProfessoraAsync(string nom, string email, string password);

        /// <summary>Llistat de professores per a la gestió web (municipal).</summary>
        Task<List<ProfessoraDto>> LlistarProfessoresAsync(bool incloureInactives = true);

        /// <summary>Actualitza nom i email d'una professora existent.</summary>
        Task<Usuari> ActualitzarProfessoraAsync(int id, string nom, string email);

        /// <summary>Dona d'alta o de baixa una professora (sense esborrar l'usuari).</summary>
        Task CanviarEstatProfessoraAsync(int id, bool activa);

        /// <summary>Assigna una contrasenya nova a una professora.</summary>
        Task ReiniciarPasswordAsync(int id, string novaPassword);

        /// <summary>
        /// Canvi de contrasenya pel propi usuari de l'app (professora o controlador).
        /// Exigeix la contrasenya actual. Llança <see cref="ArgumentException"/> amb un
        /// missatge per mostrar a l'usuari si no es pot fer.
        /// </summary>
        Task CanviarPasswordPropiaAsync(bool esControlador, int subjecteId, string passwordActual, string passwordNova);

        // ---- Controladors (regidors amb accés de lectura a l'app) ----

        /// <summary>Regidors del mandat actual, per assignar-los com a responsables d'un curset.</summary>
        Task<List<RegidorVigentDto>> GetRegidorsVigentsAsync();

        /// <summary>Estat d'accés a l'app d'un regidor concret.</summary>
        Task<RegidorVigentDto?> GetAccesRegidorAsync(int regidorId);

        /// <summary>Dona (o actualitza) l'accés a l'app d'un regidor: desa email + contrasenya.</summary>
        Task DonarOActualitzarAccesAsync(int regidorId, string email, string password);

        /// <summary>Treu l'accés a l'app d'un regidor (esborra la contrasenya, manté l'email).</summary>
        Task TreureAccesAsync(int regidorId);

        /// <summary>Esborra l'email del regidor (i, per força, l'accés a l'app).</summary>
        Task EliminarEmailAsync(int regidorId);
    }

    public class CursetsAuthService : ICursetsAuthService
    {
        private const string RolProfessora = "Professora";
        private const string RolControlador = "Controlador";

        private readonly GestorSubvencionsContext _context;
        private readonly IPasswordHasher<Usuari> _passwordHasher;
        private readonly IPasswordHasher<Regidor> _regidorHasher;
        private readonly IAcceptacioTermesService _acceptacioTermesService;
        private readonly CursetsJwtSettings _jwtSettings;
        private readonly string? _dominiCorporatiu;

        public CursetsAuthService(
            GestorSubvencionsContext context,
            IPasswordHasher<Usuari> passwordHasher,
            IPasswordHasher<Regidor> regidorHasher,
            IAcceptacioTermesService acceptacioTermesService,
            IOptions<CursetsJwtSettings> jwtSettings,
            IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _regidorHasher = regidorHasher;
            _acceptacioTermesService = acceptacioTermesService;
            _jwtSettings = jwtSettings.Value;
            _dominiCorporatiu = configuration["AzureAd:Domain"];
        }

        public async Task<LoginResponse?> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            email = email.Trim();

            // 1) Professora (taula usuaris + rol Professora)
            var usuari = await _context.Usuaris.FirstOrDefaultAsync(u => u.Email == email);
            if (usuari != null && !string.IsNullOrEmpty(usuari.PasswordHash) && usuari.Actiu)
            {
                var esProfessora = await _context.UsuarisRols
                    .AnyAsync(ur => ur.UsuariId == usuari.Id && ur.Rol!.Nom == RolProfessora);
                if (esProfessora &&
                    _passwordHasher.VerifyHashedPassword(usuari, usuari.PasswordHash, password) != PasswordVerificationResult.Failed)
                {
                    var (t, exp) = GenerarToken(usuari.Id, usuari.Email ?? string.Empty, usuari.Nom ?? string.Empty, RolProfessora, null);
                    return new LoginResponse
                    {
                        Token = t,
                        ExpiresAt = exp,
                        Rol = RolProfessora,
                        ProfessoraId = usuari.Id,
                        ProfessoraNom = usuari.Nom ?? string.Empty,
                        CalAcceptarTermes = !await _acceptacioTermesService.HaAcceptatVersioActualAsync(usuari.Email ?? string.Empty)
                    };
                }
            }

            // 2) Controlador (regidor amb contrasenya)
            var regidor = await _context.Regidors.FirstOrDefaultAsync(r => r.Email == email);
            if (regidor != null && !string.IsNullOrEmpty(regidor.PasswordHash) &&
                _regidorHasher.VerifyHashedPassword(regidor, regidor.PasswordHash, password) != PasswordVerificationResult.Failed)
            {
                return await RespostaControladorAsync(regidor);
            }

            return null;
        }

        public async Task<LoginResponse?> LoginMicrosoftAsync(IReadOnlyList<string> emails)
        {
            foreach (var candidat in CandidatsCorreu(emails))
            {
                var r = await ResoldreLoginMicrosoftAsync(candidat);
                if (r != null)
                    return r;
            }
            return null;
        }

        /// <summary>Amplia la llista de correus del token amb variants de domini:
        /// l'UPN d'Azure sol acabar en …@…onmicrosoft.com mentre que a la BD hi ha
        /// el correu corporatiu (AzureAd:Domain). Prova totes dues formes.</summary>
        private IEnumerable<string> CandidatsCorreu(IEnumerable<string> emails)
        {
            var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var brut in emails)
            {
                if (string.IsNullOrWhiteSpace(brut)) continue;
                var e = brut.Trim();
                if (!e.Contains('@')) continue;

                if (vistos.Add(e))
                    yield return e;

                var tall = e.IndexOf('@');
                var local = e[..tall];
                var domini = e[(tall + 1)..];

                if (domini.EndsWith(".onmicrosoft.com", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(_dominiCorporatiu))
                {
                    var alt = $"{local}@{_dominiCorporatiu}";
                    if (vistos.Add(alt))
                        yield return alt;
                }
            }
        }

        private async Task<LoginResponse?> ResoldreLoginMicrosoftAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            email = email.Trim();

            // 1) Professora: usuari amb rol Professora i actiu (l'AAD ja ha autenticat).
            var usuari = await _context.Usuaris.FirstOrDefaultAsync(u => u.Email == email);
            if (usuari != null && usuari.Actiu)
            {
                var esProfessora = await _context.UsuarisRols
                    .AnyAsync(ur => ur.UsuariId == usuari.Id && ur.Rol!.Nom == RolProfessora);
                if (esProfessora)
                {
                    var (t, exp) = GenerarToken(usuari.Id, usuari.Email ?? string.Empty, usuari.Nom ?? string.Empty, RolProfessora, null);
                    return new LoginResponse
                    {
                        Token = t,
                        ExpiresAt = exp,
                        Rol = RolProfessora,
                        ProfessoraId = usuari.Id,
                        ProfessoraNom = usuari.Nom ?? string.Empty,
                        CalAcceptarTermes = !await _acceptacioTermesService.HaAcceptatVersioActualAsync(usuari.Email ?? string.Empty)
                    };
                }
            }

            // 2) Controlador: regidor del mandat actual (no cal contrasenya via Microsoft).
            var regidor = await _context.Regidors.FirstOrDefaultAsync(r => r.Email == email && r.Orde != null);
            if (regidor != null)
                return await RespostaControladorAsync(regidor);

            return null;
        }

        private async Task<LoginResponse> RespostaControladorAsync(Regidor regidor)
        {
            var nom = string.IsNullOrWhiteSpace(regidor.NomComplet) ? (regidor.Nom ?? string.Empty) : regidor.NomComplet!;
            var (t, exp) = GenerarToken(regidor.Id, regidor.Email ?? string.Empty, nom, RolControlador, regidor.Id);
            return new LoginResponse
            {
                Token = t,
                ExpiresAt = exp,
                Rol = RolControlador,
                ProfessoraId = regidor.Id,
                ProfessoraNom = nom,
                CalAcceptarTermes = !await _acceptacioTermesService.HaAcceptatVersioActualAsync(regidor.Email ?? string.Empty)
            };
        }

        public async Task<Usuari> CrearOActualitzarProfessoraAsync(string nom, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Nom, email i contrasenya són obligatoris.");
            if (password.Length < 6)
                throw new ArgumentException("La contrasenya ha de tenir com a mínim 6 caràcters.");

            nom = nom.Trim();
            email = email.Trim();

            var rol = await _context.Rols.FirstOrDefaultAsync(r => r.Nom == RolProfessora);
            if (rol == null)
            {
                rol = new Rol { Nom = RolProfessora };
                _context.Rols.Add(rol);
                await _context.SaveChangesAsync();
            }

            var usuari = await _context.Usuaris.FirstOrDefaultAsync(u => u.Email == email);
            if (usuari == null)
            {
                usuari = new Usuari { Nom = nom, Email = email, Actiu = true };
                _context.Usuaris.Add(usuari);
            }
            else
            {
                usuari.Nom = nom;
                // Crear/actualitzar la professora la reactiva si estava de baixa.
                usuari.Actiu = true;
            }

            // El hasher no necessita l'Id de l'usuari; és segur cridar-lo abans del primer SaveChanges
            usuari.PasswordHash = _passwordHasher.HashPassword(usuari, password);
            await _context.SaveChangesAsync();

            var teRol = await _context.UsuarisRols.AnyAsync(ur => ur.UsuariId == usuari.Id && ur.RolId == rol.Id);
            if (!teRol)
            {
                _context.UsuarisRols.Add(new UsuariRol { UsuariId = usuari.Id, RolId = rol.Id });
                await _context.SaveChangesAsync();
            }

            return usuari;
        }

        public async Task<List<ProfessoraDto>> LlistarProfessoresAsync(bool incloureInactives = true)
        {
            var rol = await _context.Rols.FirstOrDefaultAsync(r => r.Nom == RolProfessora);
            if (rol == null)
                return new List<ProfessoraDto>();

            var professoraIds = await _context.UsuarisRols
                .Where(ur => ur.RolId == rol.Id)
                .Select(ur => ur.UsuariId)
                .ToListAsync();

            var query = _context.Usuaris.Where(u => professoraIds.Contains(u.Id));
            if (!incloureInactives)
                query = query.Where(u => u.Actiu);

            return await query
                .Select(u => new ProfessoraDto
                {
                    Id = u.Id,
                    Nom = u.Nom ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    Activa = u.Actiu,
                    NumCursets = _context.Cursets.Count(c => c.ProfessoraId == u.Id && c.Actiu)
                })
                .OrderBy(p => p.Nom)
                .ToListAsync();
        }

        public async Task<Usuari> ActualitzarProfessoraAsync(int id, string nom, string email)
        {
            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Nom i email són obligatoris.");

            nom = nom.Trim();
            email = email.Trim();

            var usuari = await _context.Usuaris.FindAsync(id)
                ?? throw new ArgumentException("La professora indicada no existeix.");

            var emailEnUs = await _context.Usuaris.AnyAsync(u => u.Id != id && u.Email == email);
            if (emailEnUs)
                throw new ArgumentException("Ja hi ha un altre usuari amb aquest email.");

            usuari.Nom = nom;
            usuari.Email = email;
            await _context.SaveChangesAsync();
            return usuari;
        }

        public async Task CanviarEstatProfessoraAsync(int id, bool activa)
        {
            var usuari = await _context.Usuaris.FindAsync(id)
                ?? throw new ArgumentException("La professora indicada no existeix.");

            usuari.Actiu = activa;
            await _context.SaveChangesAsync();
        }

        public async Task ReiniciarPasswordAsync(int id, string novaPassword)
        {
            if (string.IsNullOrWhiteSpace(novaPassword) || novaPassword.Length < 6)
                throw new ArgumentException("La contrasenya ha de tenir com a mínim 6 caràcters.");

            var usuari = await _context.Usuaris.FindAsync(id)
                ?? throw new ArgumentException("La professora indicada no existeix.");

            usuari.PasswordHash = _passwordHasher.HashPassword(usuari, novaPassword);
            await _context.SaveChangesAsync();
        }

        public async Task CanviarPasswordPropiaAsync(bool esControlador, int subjecteId, string passwordActual, string passwordNova)
        {
            if (string.IsNullOrEmpty(passwordActual))
                throw new ArgumentException("Cal indicar la contrasenya actual.");
            var errorForca = ValidarForcaPassword(passwordNova);
            if (errorForca != null)
                throw new ArgumentException(errorForca);
            if (passwordNova == passwordActual)
                throw new ArgumentException("La contrasenya nova ha de ser diferent de l'actual.");

            const string sensePassword = "El teu compte no té contrasenya pròpia (entres amb el compte de Microsoft).";
            const string actualIncorrecta = "La contrasenya actual no és correcta.";

            if (esControlador)
            {
                var regidor = await _context.Regidors.FindAsync(subjecteId)
                    ?? throw new ArgumentException("L'usuari indicat no existeix.");
                if (string.IsNullOrEmpty(regidor.PasswordHash))
                    throw new ArgumentException(sensePassword);
                if (_regidorHasher.VerifyHashedPassword(regidor, regidor.PasswordHash, passwordActual) == PasswordVerificationResult.Failed)
                    throw new ArgumentException(actualIncorrecta);

                regidor.PasswordHash = _regidorHasher.HashPassword(regidor, passwordNova);
            }
            else
            {
                var usuari = await _context.Usuaris.FindAsync(subjecteId)
                    ?? throw new ArgumentException("L'usuari indicat no existeix.");
                if (!usuari.Actiu)
                    throw new ArgumentException("El teu compte no està actiu.");
                if (string.IsNullOrEmpty(usuari.PasswordHash))
                    throw new ArgumentException(sensePassword);
                if (_passwordHasher.VerifyHashedPassword(usuari, usuari.PasswordHash, passwordActual) == PasswordVerificationResult.Failed)
                    throw new ArgumentException(actualIncorrecta);

                usuari.PasswordHash = _passwordHasher.HashPassword(usuari, passwordNova);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>Longitud mínima de la contrasenya que tria l'usuari a l'app.</summary>
        public const int LongitudMinimaPassword = 8;

        /// <summary>
        /// Política de contrasenya forta: mínim 8 caràcters amb minúscula, majúscula, número i
        /// signe (qualsevol caràcter que no sigui lletra ni número). Retorna null si és vàlida
        /// o el missatge que s'ha de mostrar a l'usuari.
        /// </summary>
        public static string? ValidarForcaPassword(string? password)
        {
            const string missatge = "La contrasenya ha de tenir com a mínim 8 caràcters i incloure "
                + "majúscules, minúscules, números i signes (per exemple ! ? # $ %).";

            if (string.IsNullOrEmpty(password) || password.Length < LongitudMinimaPassword)
                return missatge;
            if (!password.Any(char.IsLower) || !password.Any(char.IsUpper) || !password.Any(char.IsDigit)
                || !password.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
                return missatge;
            return null;
        }

        // ==================== Controladors ====================

        public async Task<List<RegidorVigentDto>> GetRegidorsVigentsAsync()
        {
            return await _context.Regidors
                .Where(r => r.Orde != null)
                .OrderBy(r => r.Orde)
                .Select(r => new RegidorVigentDto
                {
                    Id = r.Id,
                    Nom = r.NomComplet ?? r.Nom ?? string.Empty,
                    Carrec = r.Carrec,
                    Email = r.Email,
                    TeAcces = r.PasswordHash != null,
                    NumCursets = _context.Cursets.Count(c => c.RegidorId == r.Id && c.Actiu)
                })
                .ToListAsync();
        }

        public async Task<RegidorVigentDto?> GetAccesRegidorAsync(int regidorId)
        {
            return await _context.Regidors
                .Where(r => r.Id == regidorId)
                .Select(r => new RegidorVigentDto
                {
                    Id = r.Id,
                    Nom = r.NomComplet ?? r.Nom ?? string.Empty,
                    Carrec = r.Carrec,
                    Email = r.Email,
                    TeAcces = r.PasswordHash != null,
                    NumCursets = _context.Cursets.Count(c => c.RegidorId == r.Id && c.Actiu)
                })
                .FirstOrDefaultAsync();
        }

        public async Task DonarOActualitzarAccesAsync(int regidorId, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("L'email és obligatori per donar accés a l'app.");
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                throw new ArgumentException("La contrasenya ha de tenir com a mínim 6 caràcters.");

            email = email.Trim();

            var regidor = await _context.Regidors.FindAsync(regidorId)
                ?? throw new ArgumentException("El regidor indicat no existeix.");

            var emailEnUs = await _context.Regidors.AnyAsync(r => r.Id != regidorId && r.Email == email);
            if (emailEnUs)
                throw new ArgumentException("Ja hi ha un altre regidor amb aquest email.");

            regidor.Email = email;
            regidor.PasswordHash = _regidorHasher.HashPassword(regidor, password);
            await _context.SaveChangesAsync();
        }

        public async Task TreureAccesAsync(int regidorId)
        {
            var regidor = await _context.Regidors.FindAsync(regidorId)
                ?? throw new ArgumentException("El regidor indicat no existeix.");

            regidor.PasswordHash = null;
            await _context.SaveChangesAsync();
        }

        public async Task EliminarEmailAsync(int regidorId)
        {
            var regidor = await _context.Regidors.FindAsync(regidorId)
                ?? throw new ArgumentException("El regidor indicat no existeix.");

            regidor.Email = null;
            regidor.PasswordHash = null;
            await _context.SaveChangesAsync();
        }

        // ==================== Token ====================

        private (string token, DateTime expiresAt) GenerarToken(int sub, string email, string nom, string rol, int? regidorId)
        {
            if (string.IsNullOrWhiteSpace(_jwtSettings.SigningKey))
                throw new InvalidOperationException("CursetsJwt:SigningKey no està configurada.");

            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, sub.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Name, nom),
                new Claim(ClaimTypes.Role, rol),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            if (regidorId.HasValue)
                claims.Add(new Claim("regidorId", regidorId.Value.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}
