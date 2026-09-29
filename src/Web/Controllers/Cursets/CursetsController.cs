using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppAjuntament.Models.Cursets.Dto;
using AppAjuntament.Services.Cursets;

namespace AppAjuntament.Controllers.Cursets
{
    /// <summary>
    /// Cursets i temàtiques per a l'app MAUI. Lectura: professora o controlador
    /// (regidor). Escriptura: només professora.
    /// </summary>
    [ApiController]
    [Route("api/cursets")]
    [Authorize(AuthenticationSchemes = "CursetsBearer")]
    public class CursetsController : ControllerBase
    {
        private readonly ICursetsCatalogService _catalogService;

        public CursetsController(ICursetsCatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        /// <summary>Temàtiques disponibles (Pilates, Ioga, Zumba...) per crear cursets.</summary>
        [HttpGet("tipus")]
        public async Task<IActionResult> GetTipus()
        {
            var tipus = await _catalogService.GetTipusCursetsAsync();
            return Ok(tipus);
        }

        /// <summary>Dona d'alta una nova temàtica de curset (p.ex. "Ioga").</summary>
        [HttpPost("tipus")]
        [Authorize(Roles = CursetsClaimsHelper.RolProfessora)]
        public async Task<IActionResult> CrearTipus([FromBody] CrearTipusCursetRequest request)
        {
            try
            {
                var tipus = await _catalogService.CrearTipusCursetAsync(request);
                return Ok(new { tipus.Id, tipus.Nom });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cursets de qui consulta: els de la professora autenticada, o —si és un
        /// controlador— els cursets del seu regidor.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCursets()
        {
            if (this.TryGetRegidorId(out var regidorId))
                return Ok(await _catalogService.GetCursetsAsync(regidorId: regidorId));

            if (!this.TryGetProfessoraId(out var professoraId))
                return Unauthorized();

            return Ok(await _catalogService.GetCursetsAsync(professoraId));
        }

        [HttpPost]
        [Authorize(Roles = CursetsClaimsHelper.RolProfessora)]
        public async Task<IActionResult> CrearCurset([FromBody] CrearCursetRequest request)
        {
            try
            {
                var curset = await _catalogService.CrearCursetAsync(request);
                return Ok(new { curset.Id, curset.Nom });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{cursetId}/alumnes")]
        public async Task<IActionResult> GetAlumnesDeCurset(int cursetId)
        {
            var alumnes = await _catalogService.GetAlumnesDeCursetAsync(cursetId);
            return Ok(alumnes);
        }
    }
}
