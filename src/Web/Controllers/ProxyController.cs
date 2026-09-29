using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppAjuntament.Services;

namespace AppAjuntament.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly DatabaseLoggerService _dbLogger;

        public ProxyController(HttpClient httpClient, DatabaseLoggerService dbLogger)
        {
            _httpClient = httpClient;
            _dbLogger = dbLogger;
        }

        [AllowAnonymous]
        [HttpGet("image")]
        public async Task<IActionResult> GetImage([FromQuery] string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest("URL parameter is required");
            }

            // Validar que la URL sigui de media.diba.cat per seguretat
            if (!url.StartsWith("https://media.diba.cat/", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only images from media.diba.cat are allowed");
            }

            try
            {
                await _dbLogger.LogDebugAsync($"Proxying image request to: {url}", "GetImage");
                
                // Configurar headers per simular un navegador real
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", 
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                _httpClient.DefaultRequestHeaders.Add("Accept", 
                    "image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8");
                _httpClient.DefaultRequestHeaders.Add("Accept-Language", "ca-ES,ca;q=0.9,es;q=0.8,en;q=0.7");
                _httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    await _dbLogger.LogWarningAsync($"Failed to fetch image from {url}. Status: {response.StatusCode}", "GetImage");
                    return NotFound();
                }

                var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                var imageBytes = await response.Content.ReadAsByteArrayAsync();

                // Detectar si ós un placeholder verificant la mida (placeholders solen ser petits)
                var isPlaceholder = imageBytes.Length < 5000; // Menys de 5KB probablement ós un placeholder
                
                if (isPlaceholder)
                {
                    await _dbLogger.LogDebugAsync($"Detected placeholder image for {url}, size: {imageBytes.Length} bytes", "GetImage");
                    
                    // Afegir header per indicar que ós un placeholder
                    Response.Headers.Append("X-Image-Type", "placeholder");
                }

                // Afegir headers de cache per millorar el rendiment
                Response.Headers["Cache-Control"] = "public, max-age=3600"; // Cache 1 hora
                Response.Headers["Expires"] = DateTime.UtcNow.AddHours(1).ToString("R");

                return File(imageBytes, contentType);
            }
            catch (Exception ex)
            {
                await _dbLogger.LogFatalAsync($"Error crític obtenint imatge des de {url}", ex, "GetImage", $"URL={url}");
                return StatusCode(500, "Error fetching image");
            }
        }
    }
}