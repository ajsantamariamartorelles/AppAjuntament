using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppAjuntament.Models.Cursets.Dto;
using AppAjuntament.Services.Cursets;

namespace AppAjuntament.Controllers.Cursets
{
    /// <summary>Alta d'alumnes des de l'app: només professora.</summary>
    [ApiController]
    [Route("api/cursets/alumnes")]
    [Authorize(AuthenticationSchemes = "CursetsBearer", Roles = CursetsClaimsHelper.RolProfessora)]
    public class CursetsAlumnesController : ControllerBase
    {
        private readonly ICursetsCatalogService _catalogService;

        public CursetsAlumnesController(ICursetsCatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        [HttpPost]
        public async Task<IActionResult> CrearAlumne([FromBody] CrearAlumneRequest request)
        {
            try
            {
                var alumne = await _catalogService.CrearAlumneAsync(request);
                return Ok(new { alumne.Id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
