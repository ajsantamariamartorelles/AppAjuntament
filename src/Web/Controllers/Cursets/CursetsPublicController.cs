using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using AppAjuntament.Models.Cursets.Dto;
using AppAjuntament.Services.Cursets;

namespace AppAjuntament.Controllers.Cursets
{
    /// <summary>
    /// Endpoints públics (sense login) del mòdul Cursets: catàleg de cursos i
    /// sol·licitud d'inscripció des de la web ciutadana.
    /// </summary>
    [ApiController]
    [Route("api/cursets/public")]
    [AllowAnonymous]
    public class CursetsPublicController : ControllerBase
    {
        private readonly ICursetsCatalogService _catalogService;

        public CursetsPublicController(ICursetsCatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        /// <summary>Cursos actius amb la informació pública (temàtica, horari, preus, estat d'inscripció).</summary>
        [HttpGet("cursos")]
        public async Task<IActionResult> GetCursos()
            => Ok(await _catalogService.GetCursetsPublicsAsync());

        /// <summary>Registra una sol·licitud d'inscripció a un curs.</summary>
        [HttpPost("inscripcions")]
        [EnableRateLimiting("inscripcio-publica")]
        public async Task<IActionResult> CrearSollicitud([FromBody] SollicitudInscripcioPublicaRequest request)
        {
            try
            {
                var nom = await _catalogService.CrearSollicitudPublicaAsync(request);
                return Ok(new { nom });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
