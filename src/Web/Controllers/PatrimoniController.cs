using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using System.Text.Json;
using AppAjuntament.Models;
using Microsoft.AspNetCore.Authorization;
using AppAjuntament.Services;

namespace AppAjuntament.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Removed to allow calls from authorized Blazor pages
    public class PatrimoniController : ControllerBase
    {
        private readonly GestorSubvencionsContext _context;
        private readonly MinioStorageService _minioService;
        private readonly IPatrimoniService _patrimoniService;

        public PatrimoniController(GestorSubvencionsContext context, MinioStorageService minioService, IPatrimoniService patrimoniService)
        {
            _context = context;
            _minioService = minioService;
            _patrimoniService = patrimoniService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPatrimonis() => Ok(await _context.Patrimonis
            .Include(p => p.TipusPatrimoni)
            .Include(p => p.EstatConservacio)
            .Include(p => p.Ubicacio)
            .ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatrimoni(int id)
        {
            var patrimoni = await _context.Patrimonis
                .Include(p => p.TipusPatrimoni)
                .Include(p => p.EstatConservacio)
                .Include(p => p.Ubicacio)
                .Include(p => p.Arxius)
                .FirstOrDefaultAsync(p => p.Id == id);
            return patrimoni == null ? NotFound() : Ok(patrimoni);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePatrimoni([FromForm] Patrimoni patrimoni, [FromForm] List<IFormFile> documents)
        {
            Console.WriteLine($"[CreatePatrimoni] INICI - Dades rebudes: {System.Text.Json.JsonSerializer.Serialize(patrimoni)}");
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"[CreatePatrimoni] ModelState INVALID: {System.Text.Json.JsonSerializer.Serialize(ModelState)}");
                return BadRequest(ModelState);
            }
            var arxius = new List<ArxiuPatrimoni>();
            foreach (var doc in documents)
            {
                var key = $"patrimoni/{Guid.NewGuid()}_{doc.FileName}";
                var contentType = doc.ContentType ?? "application/octet-stream";
                await _minioService.UploadFileAsync(key, doc.OpenReadStream(), contentType);
                arxius.Add(new ArxiuPatrimoni
                {
                    NomOriginal = doc.FileName,
                    NomArxiu = key,
                    PathMinio = key,
                    Bucket = _minioService.BucketName,
                    MidaBytes = doc.Length,
                    MimeType = doc.ContentType ?? "application/octet-stream",
                    TipusArxiuId = 1, // Assumeix un tipus per defecte
                    CreatedAt = DateTime.Now
                });
            }
            patrimoni.Arxius = arxius;
            try
            {
                var result = await _patrimoniService.CreateAsync(patrimoni);
                Console.WriteLine($"[CreatePatrimoni] PATRIMONI DESAT: {System.Text.Json.JsonSerializer.Serialize(result)}");
                return Ok(result);
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"[CreatePatrimoni] ERROR DB: {ex.Message}");
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreatePatrimoni] ERROR GENERAL: {ex.Message}");
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatrimoni(int id, Patrimoni patrimoni)
        {
            var existing = await _context.Patrimonis.FindAsync(id);
            if (existing == null) return NotFound();
            // Actualitza camps (exclou Id, CreatedAt)
            existing.CodiInventari = patrimoni.CodiInventari;
            existing.Nom = patrimoni.Nom;
            // ... actualitza altres camps segons necessitat
            existing.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatrimoni(int id)
        {
            var patrimoni = await _context.Patrimonis.FindAsync(id);
            if (patrimoni == null) return NotFound();
            _context.Patrimonis.Remove(patrimoni);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // CRUD per auxiliars (exemple: TipusPatrimoni)
        [HttpGet("tipus")]
        public async Task<IActionResult> GetTipusPatrimoni() => Ok(await _context.TipusPatrimonis.ToListAsync());

        [HttpPost("tipus")]
        public async Task<IActionResult> CreateTipusPatrimoni(TipusPatrimoni tipus)
        {
            _context.TipusPatrimonis.Add(tipus);
            await _context.SaveChangesAsync();
            return Ok(tipus);
        }

        [HttpPut("tipus/{id}")]
        public async Task<IActionResult> UpdateTipusPatrimoni(int id, TipusPatrimoni tipus)
        {
            var existing = await _context.TipusPatrimonis.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Nom = tipus.Nom;
            // ... altres camps
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        /// <summary>
        /// Retorna els subtipus de patrimoni per un tipus donat
        /// </summary>
        [HttpGet("subtipus/{tipusId}")]
        public async Task<IActionResult> GetSubtipusPerTipus(int tipusId)
        {
            var subtipus = await _context.SubtipusPatrimonis
                .Where(s => s.TipusPatrimoniId == tipusId && s.Actiu)
                .OrderBy(s => s.Nom)
                .Select(s => s.Nom)
                .ToListAsync();
            return Ok(subtipus);
        }

        [HttpDelete("tipus/{id}")]
        public async Task<IActionResult> DeleteTipusPatrimoni(int id)
        {
            var tipus = await _context.TipusPatrimonis.FindAsync(id);
            if (tipus == null) return NotFound();
            _context.TipusPatrimonis.Remove(tipus);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("estats")]
        public async Task<IActionResult> GetEstatsConservacio() => Ok(await _context.EstatsConservacio.ToListAsync());

        [HttpPost("estats")]
        public async Task<IActionResult> CreateEstatConservacio(EstatConservacio estat)
        {
            _context.EstatsConservacio.Add(estat);
            await _context.SaveChangesAsync();
            return Ok(estat);
        }

        [HttpPut("estats/{id}")]
        public async Task<IActionResult> UpdateEstatConservacio(int id, EstatConservacio estat)
        {
            var existing = await _context.EstatsConservacio.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Nom = estat.Nom;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("estats/{id}")]
        public async Task<IActionResult> DeleteEstatConservacio(int id)
        {
            var estat = await _context.EstatsConservacio.FindAsync(id);
            if (estat == null) return NotFound();
            _context.EstatsConservacio.Remove(estat);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("ubicacions")]
        public async Task<IActionResult> GetUbicacions() => Ok(await _context.Ubicacions.ToListAsync());

        [HttpPost("ubicacions")]
        public async Task<IActionResult> CreateUbicacio(Ubicacio ubicacio)
        {
            _context.Ubicacions.Add(ubicacio);
            await _context.SaveChangesAsync();
            return Ok(ubicacio);
        }

        [HttpPut("ubicacions/{id}")]
        public async Task<IActionResult> UpdateUbicacio(int id, Ubicacio ubicacio)
        {
            var existing = await _context.Ubicacions.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Nom = ubicacio.Nom;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("ubicacions/{id}")]
        public async Task<IActionResult> DeleteUbicacio(int id)
        {
            var ubicacio = await _context.Ubicacions.FindAsync(id);
            if (ubicacio == null) return NotFound();
            _context.Ubicacions.Remove(ubicacio);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // CRUD Identificadors externs
        [HttpGet("{id}/identificadors")]
        public async Task<IActionResult> GetIdentificadors(int id)
        {
            var identificadors = await _context.PatrimoniIdentificadors.Where(i => i.PatrimoniId == id).ToListAsync();
            return Ok(identificadors);
        }

        [HttpPost("{id}/identificadors")]
        public async Task<IActionResult> AddIdentificador(int id, [FromBody] PatrimoniIdentificador identificador)
        {
            identificador.PatrimoniId = id;
            _context.PatrimoniIdentificadors.Add(identificador);
            await _context.SaveChangesAsync();
            return Ok(identificador);
        }

        [HttpPut("identificadors/{identificadorId}")]
        public async Task<IActionResult> UpdateIdentificador(int identificadorId, [FromBody] PatrimoniIdentificador identificador)
        {
            var existing = await _context.PatrimoniIdentificadors.FindAsync(identificadorId);
            if (existing == null) return NotFound();
            existing.Tipus = identificador.Tipus;
            existing.Valor = identificador.Valor;
            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("identificadors/{identificadorId}")]
        public async Task<IActionResult> DeleteIdentificador(int identificadorId)
        {
            var identificador = await _context.PatrimoniIdentificadors.FindAsync(identificadorId);
            if (identificador == null) return NotFound();
            _context.PatrimoniIdentificadors.Remove(identificador);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}