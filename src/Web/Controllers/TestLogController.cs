using Microsoft.AspNetCore.Mvc;
using AppAjuntament.Services;
using Microsoft.EntityFrameworkCore;

namespace AppAjuntament.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestLogController : ControllerBase
    {
        private readonly DatabaseLoggerService _logger;

        public TestLogController(DatabaseLoggerService logger)
        {
            _logger = logger;
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestLogging()
        {
            try
            {
                await _logger.LogInformationAsync("Test de logging - això hauria d'aparèixer a la taula logs", "TestLog");
                await _logger.LogErrorAsync("Test d'error - això també hauria d'aparèixer a la taula logs", null, "TestLog");
                
                return Ok(new { message = "Logs guardats correctament. Comprova la taula logs!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        
        [HttpGet("test-exception")]
        public async Task<IActionResult> TestException()
        {
            await _logger.LogWarningAsync("Abans de llançar excepció", "TestException");
            throw new Exception("Aquesta és una excepció de prova per verificar el logging global");
        }

        [HttpPost("import-regidors")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> ImportRegidors()
        {
            try
            {
                // Crear instància del job utilitzant el ServiceProvider actual
                var provider = HttpContext.RequestServices;
                var jobLogger = provider.GetRequiredService<ILogger<AppAjuntament.Jobs.RegidorsImportJob>>();
                var job = new AppAjuntament.Jobs.RegidorsImportJob(provider, jobLogger);

                await job.ImportRegidorsAsync();
                return Ok(new { message = "Import regidors executat. Comprova els logs." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("debug-ens")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> DebugEns([FromServices] AppAjuntament.Models.GestorSubvencionsContext db, [FromServices] Microsoft.Extensions.Options.IOptions<AppAjuntament.Models.AjuntamentSettings> settings)
        {
            try
            {
                var codiDiba = (settings.Value.CodiDiba ?? string.Empty).Trim();
                var codiIne6 = (settings.Value.CodiINE6 ?? string.Empty).Trim();
                var ens = await db.Ens.FirstOrDefaultAsync(e => (e.INE6 != null && e.INE6 == codiIne6) || (e.CodiEns != null && e.CodiEns == codiDiba));
                if (ens == null) return NotFound(new { message = "No ens found for configured codes", codiDiba, codiIne6 });
                return Ok(new { ens.Id, ens.Nom, ens.CodiEns, ens.INE6 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("debug-regidors")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> DebugRegidors([FromServices] AppAjuntament.Models.GestorSubvencionsContext db, [FromServices] Microsoft.Extensions.Options.IOptions<AppAjuntament.Models.AjuntamentSettings> settings)
        {
            try
            {
                var codiDiba = (settings.Value.CodiDiba ?? string.Empty).Trim();
                var codiIne6 = (settings.Value.CodiINE6 ?? string.Empty).Trim();
                var regs = await db.Regidors.Where(r => ((r.CodiEns ?? string.Empty).Trim() == codiDiba) || (!string.IsNullOrEmpty(codiIne6) && (r.CodiEns ?? string.Empty).Trim() == codiIne6)).ToListAsync();
                return Ok(new { count = regs.Count, items = regs.Select(r => new { r.Id, r.Nom, r.Carrec, r.Partit, r.CodiEns, r.NomEns, r.DataNomenament, r.Email, r.Orde, r.ExternId }) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
