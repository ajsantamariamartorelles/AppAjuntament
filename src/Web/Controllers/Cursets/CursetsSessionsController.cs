using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppAjuntament.Models.Cursets.Dto;
using AppAjuntament.Services.Cursets;

namespace AppAjuntament.Controllers.Cursets
{
    /// <summary>
    /// Sessions de classe. Obrir/tancar (escriptura) només professora;
    /// l'històric el poden consultar també els controladors (regidors).
    /// </summary>
    [ApiController]
    [Route("api/cursets/sessions")]
    [Authorize(AuthenticationSchemes = "CursetsBearer")]
    public class CursetsSessionsController : ControllerBase
    {
        private readonly ICursetsSessionsService _sessionsService;

        /// <summary>Missatge per a la professora quan falla alguna cosa inesperada del servidor.</summary>
        public const string MissatgeErrorInesperat =
            "Hi ha hagut un problema al servidor i no s'ha pogut fer l'operació. " +
            "Torna-ho a provar d'aquí una estona i, si continua passant, avisa l'Ajuntament.";

        private readonly ILogger<CursetsSessionsController> _logger;

        public CursetsSessionsController(ICursetsSessionsService sessionsService, ILogger<CursetsSessionsController> logger)
        {
            _sessionsService = sessionsService;
            _logger = logger;
        }

        /// <summary>"Iniciar classe": crea (o recupera) la sessió d'avui per al curset, amb tothom present per defecte.</summary>
        [HttpPost("obrir")]
        [Authorize(Roles = CursetsClaimsHelper.RolProfessora)]
        public async Task<IActionResult> ObrirSessio([FromQuery] int cursetId)
        {
            if (!this.TryGetProfessoraId(out var professoraId))
                return Unauthorized();

            try
            {
                var sessio = await _sessionsService.ObrirSessioAsync(cursetId, professoraId);
                return Ok(sessio);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en obrir la sessió del curset {CursetId}", cursetId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = MissatgeErrorInesperat });
            }
        }

        /// <summary>Desa l'assistència final (alumnes desmarcats com a absents) i tanca la sessió.</summary>
        [HttpPost("{sessioId}/tancar")]
        [Authorize(Roles = CursetsClaimsHelper.RolProfessora)]
        public async Task<IActionResult> TancarSessio(int sessioId, [FromBody] GuardarAssistenciaRequest request)
        {
            try
            {
                await _sessionsService.TancarSessioAsync(sessioId, request);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en tancar la sessió {SessioId}", sessioId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = MissatgeErrorInesperat });
            }
        }

        [HttpGet("historic")]
        public async Task<IActionResult> GetHistoric([FromQuery] int cursetId)
        {
            var historic = await _sessionsService.GetHistoricAsync(cursetId);
            return Ok(historic);
        }
    }
}
