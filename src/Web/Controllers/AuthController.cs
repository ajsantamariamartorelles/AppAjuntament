using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppAjuntament.Services;
using Microsoft.AspNetCore.RateLimiting;  // Afegir aquesta importació

namespace AppAjuntament.Controllers
{
    [Route("[controller]/[action]")]
    public class AuthController : Controller
    {
        private readonly DatabaseLoggerService _dbLogger;

        public AuthController(DatabaseLoggerService dbLogger)
        {
            _dbLogger = dbLogger;
        }

        [HttpGet]
        [EnableRateLimiting("login")]  // Aplicar rate limiting
        public async Task<IActionResult> Login(string returnUrl = "/")
        {
            try
            {
                var redirectUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
                var hasQuery = redirectUrl.Contains("?");
                var redirectWithFlag = redirectUrl + (hasQuery ? "&" : "?") + "refreshAuth=true";

                return Challenge(
                    new AuthenticationProperties { RedirectUri = redirectWithFlag },
                    OpenIdConnectDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                await _dbLogger.LogFatalAsync("Error crític durant el procés de login", ex, "Login");
                return BadRequest("Error durant el procés d'autenticació");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userName = User?.Identity?.Name ?? "Anonymous";
                await _dbLogger.LogInformationAsync($"Inici de procés de logout per a l'usuari: {userName}", "Logout");

                await HttpContext.SignOutAsync();
                await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

                return LocalRedirect("/");
            }
            catch (Exception ex)
            {
                await _dbLogger.LogFatalAsync("Error crític durant el procés de logout", ex, "Logout");
                return LocalRedirect("/");
            }
        }

        [HttpGet]
        public IActionResult Status()
        {
            var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
            var userName = User?.Identity?.Name ?? "Anonymous";
            
            return Json(new { 
                IsAuthenticated = isAuthenticated, 
                UserName = userName,
                Claims = User?.Claims?.Select(c => new { c.Type, c.Value })?.ToList() 
            });
        }
    }
}
