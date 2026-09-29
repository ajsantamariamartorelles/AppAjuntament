using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AppAjuntament.Models.Cursets.Dto;
using AppAjuntament.Services;
using AppAjuntament.Services.Cursets;

namespace AppAjuntament.Controllers.Cursets
{
    /// <summary>
    /// Login per a l'app MAUI de Cursets:
    /// - <c>login</c>: usuari/contrasenya propis (professores, monitors externs).
    /// - <c>login-microsoft</c>: compte de Microsoft (Azure AD) per a personal i
    ///   regidors; l'app hi envia l'id_token d'AAD i rep de tornada un token
    ///   CursetsBearer, igual que amb el login de contrasenya.
    /// </summary>
    [ApiController]
    [Route("api/cursets/auth")]
    public class CursetsAuthController : ControllerBase
    {
        private readonly ICursetsAuthService _authService;
        private readonly IAcceptacioTermesService _acceptacioTermesService;

        public CursetsAuthController(ICursetsAuthService authService, IAcceptacioTermesService acceptacioTermesService)
        {
            _authService = authService;
            _acceptacioTermesService = acceptacioTermesService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var resultat = await _authService.LoginAsync(request.Email, request.Password);
            if (resultat == null)
                return Unauthorized(new { message = "Usuari o contrasenya incorrectes." });

            return Ok(resultat);
        }

        /// <summary>
        /// L'app envia l'id_token d'Azure AD a la capçalera Authorization; l'esquema
        /// <c>CursetsAzureBearer</c> el valida i n'extreu l'email. Si l'email és
        /// d'una professora o d'un regidor del mandat actual, es retorna un
        /// <see cref="LoginResponse"/> amb un token CursetsBearer.
        /// </summary>
        [HttpPost("login-microsoft")]
        [Authorize(AuthenticationSchemes = "CursetsAzureBearer")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> LoginMicrosoft()
        {
            // L'UPN d'Azure (…@…onmicrosoft.com) sol diferir del correu corporatiu
            // guardat a la BD: recollim tots els candidats que porti el token.
            var candidats = new[]
                {
                    User.FindFirstValue("preferred_username"),
                    User.FindFirstValue(ClaimTypes.Upn),
                    User.FindFirstValue("upn"),
                    User.FindFirstValue(ClaimTypes.Email),
                    User.FindFirstValue("email"),
                    User.FindFirstValue("verified_primary_email"),
                }
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (candidats.Length == 0)
                return Unauthorized(new { message = "El token de Microsoft no porta cap adreça de correu." });

            var resultat = await _authService.LoginMicrosoftAsync(candidats);
            if (resultat == null)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Aquest compte no té accés a l'app (no és professora ni regidor/a del mandat actual)." });

            return Ok(resultat);
        }

        /// <summary>
        /// Canvi de la contrasenya pel propi usuari de l'app (professora o controlador).
        /// Cal el token Bearer de Cursets i la contrasenya actual. Els comptes que
        /// només entren amb Microsoft no tenen contrasenya pròpia.
        /// </summary>
        [HttpPost("canviar-password")]
        [Authorize(AuthenticationSchemes = "CursetsBearer")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> CanviarPassword([FromBody] CanviarPasswordPropiaRequest request)
        {
            var esControlador = this.EsControlador();
            int subjecteId;
            var ok = esControlador ? this.TryGetRegidorId(out subjecteId) : this.TryGetProfessoraId(out subjecteId);
            if (!ok)
                return Unauthorized(new { message = "Sessió no vàlida." });

            try
            {
                await _authService.CanviarPasswordPropiaAsync(esControlador, subjecteId, request.PasswordActual, request.PasswordNova);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Registra l'acceptació dels termes i condicions i la política de
        /// privacitat per a l'usuari del token Bearer actual.</summary>
        [HttpPost("accepta-termes")]
        [Authorize(AuthenticationSchemes = "CursetsBearer")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> AcceptaTermes()
        {
            var email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new { message = "Sessió no vàlida." });

            await _acceptacioTermesService.AcceptaAsync(email);
            return NoContent();
        }
    }
}
