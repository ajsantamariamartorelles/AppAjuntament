using AppAjuntament.Models.Municipi;
using Microsoft.Extensions.Caching.Memory;

namespace AppAjuntament.Services
{
    public interface ICachedMunicipiService
    {
        Task<ApiMunicipiElement?> ObtindreInfoMunicipiAsync(string nomMunicipi);
        void InvalidateCache(string nomMunicipi);
    }

    public class CachedMunicipiService : ICachedMunicipiService
    {
        private readonly IMunicipiApiService _municipiApiService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachedMunicipiService> _logger;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private const int CACHE_DURATION_MINUTES = 10;

        public CachedMunicipiService(
            IMunicipiApiService municipiApiService, 
            IMemoryCache cache, 
            ILogger<CachedMunicipiService> logger)
        {
            _municipiApiService = municipiApiService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ApiMunicipiElement?> ObtindreInfoMunicipiAsync(string nomMunicipi)
        {
            var cacheKey = $"municipi_{nomMunicipi.ToLowerInvariant()}";
            
            // Intentar obtenir de la cachó primer
            if (_cache.TryGetValue(cacheKey, out ApiMunicipiElement? cachedMunicipi))
            {
                _logger.LogInformation("Dades del municipi {NomMunicipi} obtingudes de la cachó - evitant crida duplicada a l'API", nomMunicipi);
                return cachedMunicipi;
            }

            // Si no estó en cachó, obtenir de l'API amb semófor per evitar crides duplicades
            await _semaphore.WaitAsync();
            try
            {
                // Verificar novament la cachó per si un altre thread ja ha carregat les dades
                if (_cache.TryGetValue(cacheKey, out cachedMunicipi))
                {
                    _logger.LogInformation("Dades del municipi {NomMunicipi} obtingudes de la cachó (segon intent) - evitant crida duplicada a l'API", nomMunicipi);
                    return cachedMunicipi;
                }

                _logger.LogInformation("Carregant dades del municipi {NomMunicipi} des de l'API - primera crida o cachó expirada", nomMunicipi);
                var municipi = await _municipiApiService.ObtindreInfoMunicipiAsync(nomMunicipi);
                
                if (municipi != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                        SlidingExpiration = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES / 2),
                        Priority = CacheItemPriority.Normal
                    };
                    
                    _cache.Set(cacheKey, municipi, cacheOptions);
                    _logger.LogInformation("Dades del municipi {NomMunicipi} guardades a la cachó per {Minutes} minuts", 
                        nomMunicipi, CACHE_DURATION_MINUTES);
                }

                return municipi;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void InvalidateCache(string nomMunicipi)
        {
            var cacheKey = $"municipi_{nomMunicipi.ToLowerInvariant()}";
            _cache.Remove(cacheKey);
            _logger.LogInformation("Cachó invalidada per al municipi {NomMunicipi}", nomMunicipi);
        }
    }
}