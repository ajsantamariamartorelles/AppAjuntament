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

        public CursetsSessionsController(ICursetsSessionsService sessionsService)
        {
            _sessionsService = sessionsService;
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
        }

        [HttpGet("historic")]
        public async Task<IActionResult> GetHistoric([FromQuery] int cursetId)
        {
            var historic = await _sessionsService.GetHistoricAsync(cursetId);
            return Ok(historic);
        }
    }
}
