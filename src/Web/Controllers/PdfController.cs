using Microsoft.AspNetCore.Mvc;
using AppAjuntament.Services;
using AppAjuntament.Models;
using Microsoft.EntityFrameworkCore;

namespace AppAjuntament.Controllers
{
    [ApiController]
    [Route("api/pdf")]
    public class PdfController : ControllerBase
    {
        private readonly PdfService _pdfService;
        private readonly GestorSubvencionsContext _context;

        public PdfController(PdfService pdfService, GestorSubvencionsContext context)
        {
            _pdfService = pdfService;
            _context = context;
        }

        [HttpGet("volunteer-form/{voluntariId}")]
        public async Task<IActionResult> GetVolunteerForm(int voluntariId)
        {
            // Find the "Colònies felines" subambit
            var coloniesFelinesSubambit = await _context.SubambitsVoluntaris
                .FirstOrDefaultAsync(s => s.Nom == "Colònies felines");

            if (coloniesFelinesSubambit == null)
            {
                return NotFound("Subàmbit 'Colònies felines' no trobat");
            }

            // Check if volunteer has this specific subambit assigned
            var hasColoniesFelines = await _context.VoluntarisAmbits
                .AnyAsync(va => va.VoluntariId == voluntariId && va.SubambitVoluntariId == coloniesFelinesSubambit.Id);

            if (!hasColoniesFelines)
            {
                return BadRequest("El voluntari no té assignat el subàmbit 'Colònies felines'");
            }

            var voluntari = await _context.Voluntaris
                .Include(v => v.VoluntarisAmbits).ThenInclude(va => va.AmbitVoluntari)
                .Include(v => v.Tercer)
                .FirstOrDefaultAsync(v => v.Id == voluntariId);

            if (voluntari == null)
            {
                return NotFound();
            }

            var pdfBytes = _pdfService.GenerateFilledVolunteerForm(voluntari);
            var nom = voluntari.Tercer?.Nom ?? "";
            var cognoms = voluntari.Tercer?.Cognoms ?? "";
            return File(pdfBytes, "application/pdf", $"Fitxa_{nom}_{cognoms}.pdf");
        }
    }
}