using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppAjuntament.Services.Cursets;

namespace AppAjuntament.Controllers.Cursets
{
    /// <summary>
    /// Seguiment per al controlador (regidor) a l'app MAUI: persones inscrites i
    /// situació de les liquidacions dels cursets que supervisa. Només lectura i
    /// només el rol Controlador; un curset d'un altre regidor respon 404.
    /// </summary>
    [ApiController]
    [Route("api/cursets/seguiment")]
    [Authorize(AuthenticationSchemes = "CursetsBearer", Roles = CursetsClaimsHelper.RolControlador)]
    public class CursetsSeguimentController : ControllerBase
    {
        private readonly ICursetsSeguimentService _seguimentService;

        public CursetsSeguimentController(ICursetsSeguimentService seguimentService)
        {
            _seguimentService = seguimentService;
        }

        /// <summary>Places, llista d'espera i pendents de cada curset (per a la llista de cursets).</summary>
        [HttpGet("resum")]
        public async Task<IActionResult> GetResum()
        {
            if (!this.TryGetRegidorId(out var regidorId))
                return Forbid();

            return Ok(await _seguimentService.GetResumCursetsAsync(regidorId));
        }

        [HttpGet("cursets/{cursetId}/inscrites")]
        public async Task<IActionResult> GetInscrites(int cursetId)
        {
            if (!this.TryGetRegidorId(out var regidorId))
                return Forbid();

            var inscrites = await _seguimentService.GetInscritesAsync(regidorId, cursetId);
            return inscrites is null ? NotFound() : Ok(inscrites);
        }

        [HttpGet("cursets/{cursetId}/persones/{alumneId}")]
        public async Task<IActionResult> GetPersona(int cursetId, int alumneId)
        {
            if (!this.TryGetRegidorId(out var regidorId))
                return Forbid();

            var fitxa = await _seguimentService.GetPersonaAsync(regidorId, cursetId, alumneId);
            return fitxa is null ? NotFound() : Ok(fitxa);
        }

        /// <summary>Resum global: pendents de cobrament de tots els cursets que supervisa.</summary>
        [HttpGet("cobraments")]
        public async Task<IActionResult> GetCobraments()
        {
            if (!this.TryGetRegidorId(out var regidorId))
                return Forbid();

            return Ok(await _seguimentService.GetCobramentsAsync(regidorId));
        }
    }
}
