using System.Globalization;
using System.Text.Json;
using AppAjuntament.Models.Municipi;
using AppAjuntament.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AppAjuntament.Services
{
    public interface ICachedMunicipiService
    {
        Task<ApiMunicipiElement?> ObtindreInfoMunicipiAsync(string nomMunicipi);
        void InvalidateCache(string nomMunicipi);
    }

    /// <summary>
    /// Dades del municipi de l'API de la Diputació, amb tres capes perquè les pàgines no en depenguin:
    /// memòria cau (10 min), còpia al disc de l'última resposta bona (sobreviu als reinicis) i un
    /// "fre" de 2 minuts quan l'API falla, per no fer esperar cada visita. Si no hi ha còpia, es fan servir
    /// les dades de la taula <c>ens</c> (importades de la Diputació) i, com a últim recurs, un perfil mínim
    /// (marcat com a <see cref="ApiMunicipiElement.Provisional"/>) construït amb la configuració.
    /// </summary>
    public class CachedMunicipiService : ICachedMunicipiService
    {
        private readonly IMunicipiApiService _municipiApiService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachedMunicipiService> _logger;
        private readonly AjuntamentSettings? _ajuntament;
        private readonly IDbContextFactory<GestorSubvencionsContext>? _dbFactory;
        private readonly string _carpetaCopies;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private const int CACHE_DURATION_MINUTES = 10;
        private static readonly TimeSpan DuradaFallada = TimeSpan.FromMinutes(2);

        /// <param name="carpetaCopies">On es desa la còpia de l'última resposta bona; per defecte, App_Data/cache.</param>
        public CachedMunicipiService(
            IMunicipiApiService municipiApiService,
            IMemoryCache cache,
            ILogger<CachedMunicipiService> logger,
            IOptions<AjuntamentSettings>? ajuntament = null,
            IDbContextFactory<GestorSubvencionsContext>? dbFactory = null,
            string? carpetaCopies = null)
        {
            _ajuntament = ajuntament?.Value;
            _dbFactory = dbFactory;
            _municipiApiService = municipiApiService;
            _cache = cache;
            _logger = logger;
            _carpetaCopies = carpetaCopies ?? Path.Combine(AppContext.BaseDirectory, "App_Data", "cache");
        }

        private static string ClauCache(string nom) => $"municipi_{nom.ToLowerInvariant()}";

        private static string ClauFallada(string nom) => $"municipi_fallada_{nom.ToLowerInvariant()}";

        private string RutaCopia(string nom)
        {
            var segur = new string(nom.ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
            return Path.Combine(_carpetaCopies, $"municipi_{segur}.json");
        }

        public async Task<ApiMunicipiElement?> ObtindreInfoMunicipiAsync(string nomMunicipi)
        {
            var cacheKey = ClauCache(nomMunicipi);

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

                // Si l'API ha fallat fa poc, no insistim: es serveix la còpia (o res) fins que passi el fre.
                if (_cache.TryGetValue(ClauFallada(nomMunicipi), out _))
                    return await AlternativaAsync(nomMunicipi);

                _logger.LogInformation("Carregant dades del municipi {NomMunicipi} des de l'API - primera crida o cachó expirada", nomMunicipi);
                ApiMunicipiElement? municipi = null;
                try
                {
                    municipi = await _municipiApiService.ObtindreInfoMunicipiAsync(nomMunicipi);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "L'API de la Diputació ha fallat per al municipi {NomMunicipi}", nomMunicipi);
                }

                if (municipi != null)
                {
                    // Les imatges pròpies de l'Ajuntament (taula ens) manen sobre les de l'API: així la
                    // pantalla és la mateixa tant si la Diputació respon com si no.
                    await AplicaImatgesDeEnsAsync(municipi);

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES),
                        SlidingExpiration = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES / 2),
                        Priority = CacheItemPriority.Normal
                    };
                    _cache.Set(cacheKey, municipi, cacheOptions);
                    DesarCopia(nomMunicipi, municipi);
                    _logger.LogInformation("Dades del municipi {NomMunicipi} guardades a la cachó per {Minutes} minuts",
                        nomMunicipi, CACHE_DURATION_MINUTES);
                    return municipi;
                }

                // L'API no ha respost: es posa el fre i es serveix l'última resposta bona, si n'hi ha.
                _cache.Set(ClauFallada(nomMunicipi), true, DuradaFallada);
                _logger.LogWarning("L'API de la Diputació no respon per al municipi {NomMunicipi}: s'usen les dades alternatives", nomMunicipi);
                return await AlternativaAsync(nomMunicipi);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void InvalidateCache(string nomMunicipi)
        {
            _cache.Remove(ClauCache(nomMunicipi));
            _cache.Remove(ClauFallada(nomMunicipi)); // "Tornar a intentar" ha de tornar a cridar l'API de debò
            _logger.LogInformation("Cachó invalidada per al municipi {NomMunicipi}", nomMunicipi);
        }

        /// <summary>Imatge de portada que porta l'app (wwwroot), perquè la pantalla no depengui de cap servidor extern.</summary>
        private const string ImatgePortadaLocal = "/img/municipi/portada.jpg";

        /// <summary>Sobreescriu escut, bandera i vista amb les de la taula <c>imatges</c> de l'ens, si les té omplertes.</summary>
        private async Task AplicaImatgesDeEnsAsync(ApiMunicipiElement municipi)
        {
            if (_dbFactory is null || _ajuntament is null) return;
            var ine6 = (_ajuntament.CodiINE6 ?? string.Empty).Trim();
            var codiEns = (_ajuntament.CodiEns ?? string.Empty).Trim();
            var codiDiba = (_ajuntament.CodiDiba ?? string.Empty).Trim();   // la resta de l'app també busca ens.CodiEns amb aquest codi
            if (ine6.Length == 0 && codiEns.Length == 0 && codiDiba.Length == 0) return;

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync();
                var img = await db.Ens.AsNoTracking()
                    .Where(x => (ine6 != "" && x.INE6 == ine6) || (codiEns != "" && x.CodiEns == codiEns) || (codiDiba != "" && x.CodiEns == codiDiba))
                    .Select(x => new
                    {
                        Escut = x.Imatge == null ? null : x.Imatge.EscutURL,
                        Bandera = x.Imatge == null ? null : x.Imatge.BanderaURL,
                        Vista = x.Imatge == null ? null : x.Imatge.VistaURL,
                    })
                    .FirstOrDefaultAsync();
                if (img is null) return;

                if (!string.IsNullOrWhiteSpace(img.Escut)) municipi.municipi_escut = img.Escut;
                if (!string.IsNullOrWhiteSpace(img.Bandera)) municipi.municipi_bandera = img.Bandera;
                if (!string.IsNullOrWhiteSpace(img.Vista)) municipi.municipi_vista = img.Vista;
            }
            catch (Exception ex)
            {
                // Sense BD es queden les imatges de l'API; no és motiu per tombar la pàgina.
                _logger.LogWarning(ex, "No s'han pogut llegir les imatges del municipi de la taula ens");
            }
        }

        /// <summary>Quan l'API no respon: còpia de l'última resposta bona, taula ens o perfil mínim de la configuració.</summary>
        private async Task<ApiMunicipiElement?> AlternativaAsync(string nom) =>
            LlegirCopia(nom) ?? await LlegirDeEnsAsync(nom) ?? PerfilProvisional(nom);

        /// <summary>
        /// Dades del municipi de la taula <c>ens</c> (que omplen els treballs d'importació de la Diputació).
        /// Només es llegeixen les columnes necessàries, sense les geomètriques.
        /// </summary>
        private async Task<ApiMunicipiElement?> LlegirDeEnsAsync(string nom)
        {
            if (_dbFactory is null || _ajuntament is null) return null;
            var ine6 = (_ajuntament.CodiINE6 ?? string.Empty).Trim();
            var codiEns = (_ajuntament.CodiEns ?? string.Empty).Trim();
            var codiDiba = (_ajuntament.CodiDiba ?? string.Empty).Trim();   // la resta de l'app també busca ens.CodiEns amb aquest codi
            if (ine6.Length == 0 && codiEns.Length == 0 && codiDiba.Length == 0) return null;

            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync();
                var e = await db.Ens.AsNoTracking()
                    .Where(x => (ine6 != "" && x.INE6 == ine6) || (codiEns != "" && x.CodiEns == codiEns) || (codiDiba != "" && x.CodiEns == codiDiba))
                    .Select(x => new
                    {
                        x.Nom, x.NomCurt, x.Article, x.CIF, x.AdrecaCompleta, x.INE6, x.NomDBPedia,
                        Comarca = x.Comarca == null ? null : x.Comarca.Nom,
                        Provincia = x.Provincia == null ? null : x.Provincia.Nom,
                        Adreca = x.Contacte == null ? null : x.Contacte.Adreca,
                        CodiPostal = x.Contacte == null ? null : x.Contacte.CodiPostal,
                        Telefon = x.Contacte == null ? null : (x.Contacte.TelefonContacte ?? x.Contacte.Telefon),
                        Fax = x.Contacte == null ? null : x.Contacte.Fax,
                        Email = x.Contacte == null ? null : x.Contacte.Email,
                        Web = x.Contacte == null ? null : x.Contacte.Web,
                        Escut = x.Imatge == null ? null : x.Imatge.EscutURL,
                        Bandera = x.Imatge == null ? null : x.Imatge.BanderaURL,
                        Vista = x.Imatge == null ? null : x.Imatge.VistaURL,
                        Habitants = x.DadesGeografiques == null ? (int?)null : x.DadesGeografiques.NombreHabitants,
                        Extensio = x.DadesGeografiques == null ? (decimal?)null : x.DadesGeografiques.Extensio,
                        Altitud = x.DadesGeografiques == null ? (int?)null : x.DadesGeografiques.Altitud,
                        Lat = x.DadesGeografiques == null ? 0d : x.DadesGeografiques.CentreMunicipalLatitud,
                        Lon = x.DadesGeografiques == null ? 0d : x.DadesGeografiques.CentreMunicipalLongitud,
                    })
                    .FirstOrDefaultAsync();
                if (e is null) return null;

                var perfil = PerfilProvisional(nom);   // només per completar el que falti (imatges, comarca...)
                static string? Text(string? v) => string.IsNullOrWhiteSpace(v) ? null : v;
                var inv = CultureInfo.InvariantCulture;
                return new ApiMunicipiElement
                {
                    ine = Text(_ajuntament.CodiDiba),
                    ine6 = Text(e.INE6) ?? Text(_ajuntament.CodiINE6),
                    municipi_nom = Text(_ajuntament.NomMunicipi) ?? Text(e.NomCurt) ?? Text(e.Nom),
                    municipi_nom_curt = Text(e.NomCurt) ?? Text(_ajuntament.NomMunicipi),
                    municipi_article = Text(e.Article),
                    nom_dbpedia = Text(e.NomDBPedia),
                    grup_comarca = new ApiGrupComarca { comarca_nom = Text(e.Comarca) ?? perfil?.grup_comarca?.comarca_nom },
                    grup_provincia = new ApiGrupProvincia { provincia_nom = Text(e.Provincia) ?? perfil?.grup_provincia?.provincia_nom },
                    grup_ajuntament = new ApiGrupAjuntament
                    {
                        adreca_completa = Text(e.AdrecaCompleta), adreca = Text(e.Adreca), codi_postal = Text(e.CodiPostal),
                        telefon_contacte = Text(e.Telefon), fax = Text(e.Fax), email = Text(e.Email),
                        url_general = Text(e.Web), cif = Text(e.CIF),
                    },
                    municipi_escut = Text(e.Escut) ?? perfil?.municipi_escut,
                    municipi_bandera = Text(e.Bandera) ?? perfil?.municipi_bandera,
                    municipi_vista = Text(e.Vista) ?? perfil?.municipi_vista,
                    nombre_habitants = e.Habitants?.ToString(inv),
                    extensio = e.Extensio?.ToString(inv),
                    altitud = e.Altitud?.ToString(inv),
                    centre_municipal = e.Lat != 0 && e.Lon != 0 ? $"{e.Lat.ToString(inv)},{e.Lon.ToString(inv)}" : null,
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No s'ha pogut llegir el municipi {NomMunicipi} de la taula ens", nom);
                return null;
            }
        }

        /// <summary>
        /// Perfil mínim amb el que ja sabem del municipi (configuració i imatges de la Diputació, que van
        /// per un altre servidor que l'API de dades). No s'inventa res: la resta de camps queden buits.
        /// </summary>
        private ApiMunicipiElement? PerfilProvisional(string nom)
        {
            if (_ajuntament is null || string.IsNullOrWhiteSpace(_ajuntament.NomMunicipi))
                return null;

            var diba = _ajuntament.CodiDiba;
            const string imatges = "https://media.diba.cat/diba/municipis/img";
            return new ApiMunicipiElement
            {
                Provisional = true,
                ine = diba,
                ine6 = _ajuntament.CodiINE6,
                municipi_nom = _ajuntament.NomMunicipi,
                municipi_nom_curt = _ajuntament.NomMunicipi,
                grup_comarca = string.IsNullOrWhiteSpace(_ajuntament.Comarca) ? null : new ApiGrupComarca { comarca_nom = _ajuntament.Comarca },
                grup_provincia = string.IsNullOrWhiteSpace(_ajuntament.Provincia) ? null : new ApiGrupProvincia { provincia_nom = _ajuntament.Provincia },
                municipi_escut = string.IsNullOrWhiteSpace(diba) ? null : $"{imatges}/escuts/ec{diba}.png",
                municipi_bandera = string.IsNullOrWhiteSpace(diba) ? null : $"{imatges}/banderes/bn{diba}.gif",
                municipi_vista = ImatgePortadaLocal,
            };
        }

        private void DesarCopia(string nom, ApiMunicipiElement municipi)
        {
            try
            {
                Directory.CreateDirectory(_carpetaCopies);
                var ruta = RutaCopia(nom);
                var temporal = ruta + ".tmp";
                File.WriteAllText(temporal, JsonSerializer.Serialize(municipi));
                File.Move(temporal, ruta, overwrite: true);
            }
            catch (Exception ex)
            {
                // Sense còpia al disc el servei funciona igual, només perd el recurs de darrera instància.
                _logger.LogWarning(ex, "No s'ha pogut desar la còpia del municipi {NomMunicipi}", nom);
            }
        }

        private ApiMunicipiElement? LlegirCopia(string nom)
        {
            try
            {
                var ruta = RutaCopia(nom);
                return File.Exists(ruta)
                    ? JsonSerializer.Deserialize<ApiMunicipiElement>(File.ReadAllText(ruta))
                    : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No s'ha pogut llegir la còpia del municipi {NomMunicipi}", nom);
                return null;
            }
        }
    }
}
