using AppAjuntament.Models;
using AppAjuntament.Models.Base.Log;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AppAjuntament.Services
{
    public class DatabaseLoggerService
    {
        private readonly IDbContextFactory<GestorSubvencionsContext> _contextFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;
        private readonly string _minimumLevel;

        public DatabaseLoggerService(
            IDbContextFactory<GestorSubvencionsContext> contextFactory,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _contextFactory = contextFactory;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            
            // Obtenir el nivell mínim de log segons l'entorn
            _minimumLevel = _environment.IsProduction() 
                ? configuration.GetValue<string>("DatabaseLogging:ProductionLevel") ?? "Fatal"
                : configuration.GetValue<string>("DatabaseLogging:DevelopmentLevel") ?? "Debug";
        }

        public async Task LogFatalAsync(string message, Exception? exception = null, string? action = null, string? additionalData = null)
        {
            await LogAsync("Fatal", message, exception, action, additionalData);
        }

        public async Task LogErrorAsync(string message, Exception? exception = null, string? action = null, string? additionalData = null)
        {
            await LogAsync("Error", message, exception, action, additionalData);
        }

        public async Task LogWarningAsync(string message, string? action = null, string? additionalData = null)
        {
            await LogAsync("Warning", message, null, action, additionalData);
        }

        public async Task LogInformationAsync(string message, string? action = null, string? additionalData = null)
        {
            await LogAsync("Information", message, null, action, additionalData);
        }

        public async Task LogDebugAsync(string message, string? action = null, string? additionalData = null)
        {
            await LogAsync("Debug", message, null, action, additionalData);
        }

        private async Task LogAsync(string level, string message, Exception? exception = null, string? action = null, string? additionalData = null)
        {
            // Filtrar segons el nivell configurat
            if (!ShouldLog(level))
            {
                return;
            }

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var httpContext = _httpContextAccessor.HttpContext;
                var userId = httpContext?.User?.Identity?.Name;
                var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
                
                // Obtenir session ID de forma segura sense llançar excepció si no està configurada
                var sessionFeature = httpContext?.Features.Get<Microsoft.AspNetCore.Http.Features.ISessionFeature>();
                string? sessionId = sessionFeature?.Session?.Id;

                var log = new Log
                {
                    Timestamp = DateTime.UtcNow,
                    Level = level,
                    Message = message,
                    Exception = exception?.ToString(),
                    UserId = userId,
                    Action = action,
                    IpAddress = ipAddress,
                    SessionId = sessionId,
                    AdditionalData = additionalData
                };

                context.Logs.Add(log);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Si falla el logging a BD, escriu a la consola per no perdre l'error
                Console.WriteLine($"[DatabaseLogger ERROR] Failed to log to database: {ex.Message}");
                Console.WriteLine($"[Original Log] Level={level}, Message={message}, Exception={exception?.Message}");
            }
        }

        private bool ShouldLog(string level)
        {
            var levels = new[] { "Debug", "Information", "Warning", "Error", "Fatal" };
            var currentLevelIndex = Array.IndexOf(levels, level);
            var minimumLevelIndex = Array.IndexOf(levels, _minimumLevel);

            return currentLevelIndex >= minimumLevelIndex;
        }
    }
}
