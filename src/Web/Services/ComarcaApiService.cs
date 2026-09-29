using AppAjuntament.Models;
using AppAjuntament.Models.Base.Comarca;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Net.Sockets;
using System.Net;

namespace AppAjuntament.Services
{
    public interface IComarcaApiService
    {
        Task<(int actualitzades, int errors, List<string> missatges)> SincronitzarComarquesAsync();
    }

    public class ComarcaApiService : IComarcaApiService
    {
        private readonly HttpClient _httpClient;
        private readonly GestorSubvencionsContext _context;
        private readonly DatabaseLoggerService _dbLogger;
        private const string API_URL = "https://do.diba.cat/api/tipus/comarca";

        public ComarcaApiService(HttpClient httpClient, GestorSubvencionsContext context, DatabaseLoggerService dbLogger)
        {
            _httpClient = httpClient;
            _context = context;
            _dbLogger = dbLogger;
        }

        public async Task<(int actualitzades, int errors, List<string> missatges)> SincronitzarComarquesAsync()
        {
            var missatges = new List<string>();
            int actualitzades = 0;
            int errors = 0;

            await _dbLogger.LogInformationAsync("Iniciant sincronització de comarques des de l'API", "SincronitzarComarquesAsync");

            try
            {
                await _dbLogger.LogDebugAsync($"Cridant API: {API_URL}", "SincronitzarComarquesAsync");

                // Obtenir dades de l'API amb retry logic
                var response = await _httpClient.GetWithRetryAsync(API_URL, maxRetries: 3);
                if (response == null)
                {
                    await _dbLogger.LogWarningAsync("No s'ha pogut connectar amb l'API després de 3 intents", "SincronitzarComarquesAsync");
                    missatges.Add("Error: No s'ha pogut connectar amb l'API després de múltiples intents");
                    return (0, 1, missatges);
                }
                
                var jsonContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiComarcaResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.datasets?.FirstOrDefault()?.elements == null)
                {
                    await _dbLogger.LogWarningAsync("L'API no ha retornat dades de comarques", "SincronitzarComarquesAsync");
                    missatges.Add("Error: No s'han trobat dades de comarques a l'API");
                    return (0, 1, missatges);
                }

                var elements = apiResponse.datasets.First().elements;
                await _dbLogger.LogInformationAsync($"S'han obtingut {elements!.Count} comarques de l'API", "SincronitzarComarquesAsync");
                missatges.Add($"S'han obtingut {elements!.Count} comarques de l'API");

                // Obtenir comarques existents de la base de dades
                var comarquesExistents = await _context.Comarques.ToListAsync();

                foreach (var element in elements)
                {
                    try
                    {
                        // Convertir comarca_id de string a int
                        if (!int.TryParse(element.comarca_id, out int comarcaIdInt))
                        {
                            missatges.Add($"Error: No es pot convertir comarca_id '{element.comarca_id}' a número per a {element.comarca_nom}");
                            errors++;
                            continue;
                        }

                        var comarcaExistent = comarquesExistents.FirstOrDefault(c => c.Codi == comarcaIdInt);
                        
                        // Convertir emails de l'API a JSON string
                        var emailsJson = element.grup_cc?.cc_email?.Any() == true 
                            ? JsonSerializer.Serialize(element.grup_cc.cc_email)
                            : "[]";

                        if (comarcaExistent == null)
                        {
                            // Crear nova comarca
                            var novaComarca = new Comarca
                            {
                                Codi = comarcaIdInt,  // Utilitzar el valor convertit a int
                                Nom = element.comarca_nom ?? "",
                                Coordenades = element.comarca_geo,
                                NomDBPedia = element.nom_dbpedia,
                                CCAdrecaCompleta = element.grup_cc?.cc_adreca_completa ?? "",
                                CCAdreca = element.grup_cc?.cc_adreca ?? "",
                                CCCodiPostal = element.grup_cc?.cc_cpostal ?? "",
                                CCEmail = emailsJson,
                                CCFax = element.grup_cc?.cc_fax ?? "",
                                CCWeb = element.grup_cc?.cc_web ?? ""
                            };

                            _context.Comarques.Add(novaComarca);
                            actualitzades++;
                            missatges.Add($"Comarca nova creada: {element.comarca_nom} (Codi: {comarcaIdInt})");
                        }
                        else
                        {
                            // Verificar si hi ha canvis
                            bool haCanvis = false;
                            var canvis = new List<string>();

                            if (comarcaExistent.Nom != element.comarca_nom)
                            {
                                canvis.Add($"Nom: '{comarcaExistent.Nom}' -> '{element.comarca_nom}'");
                                comarcaExistent.Nom = element.comarca_nom ?? "";
                                haCanvis = true;
                            }

                            if (comarcaExistent.Coordenades != element.comarca_geo)
                            {
                                canvis.Add($"Coordenades: '{comarcaExistent.Coordenades}' -> '{element.comarca_geo}'");
                                comarcaExistent.Coordenades = element.comarca_geo;
                                haCanvis = true;
                            }

                            if (comarcaExistent.NomDBPedia != element.nom_dbpedia)
                            {
                                canvis.Add($"NomDBPedia: '{comarcaExistent.NomDBPedia}' -> '{element.nom_dbpedia}'");
                                comarcaExistent.NomDBPedia = element.nom_dbpedia;
                                haCanvis = true;
                            }

                            // Verificar canvis en dades del Consell Comarcal
                            var ccAdrecaCompleta = element.grup_cc?.cc_adreca_completa ?? "";
                            if (comarcaExistent.CCAdrecaCompleta != ccAdrecaCompleta)
                            {
                                canvis.Add($"CC Adreça Completa canviada");
                                comarcaExistent.CCAdrecaCompleta = ccAdrecaCompleta;
                                haCanvis = true;
                            }

                            var ccAdreca = element.grup_cc?.cc_adreca ?? "";
                            if (comarcaExistent.CCAdreca != ccAdreca)
                            {
                                canvis.Add($"CC Adreça canviada");
                                comarcaExistent.CCAdreca = ccAdreca;
                                haCanvis = true;
                            }

                            var ccCodiPostal = element.grup_cc?.cc_cpostal ?? "";
                            if (comarcaExistent.CCCodiPostal != ccCodiPostal)
                            {
                                canvis.Add($"CC Codi Postal canviat");
                                comarcaExistent.CCCodiPostal = ccCodiPostal;
                                haCanvis = true;
                            }

                            if (comarcaExistent.CCEmail != emailsJson)
                            {
                                canvis.Add($"CC Email canviat");
                                comarcaExistent.CCEmail = emailsJson;
                                haCanvis = true;
                            }

                            var ccFax = element.grup_cc?.cc_fax ?? "";
                            if (comarcaExistent.CCFax != ccFax)
                            {
                                canvis.Add($"CC Fax canviat");
                                comarcaExistent.CCFax = ccFax;
                                haCanvis = true;
                            }

                            var ccWeb = element.grup_cc?.cc_web ?? "";
                            if (comarcaExistent.CCWeb != ccWeb)
                            {
                                canvis.Add($"CC Web canviat");
                                comarcaExistent.CCWeb = ccWeb;
                                haCanvis = true;
                            }

                            if (haCanvis)
                            {
                                actualitzades++;
                                missatges.Add($"Comarca actualitzada: {element.comarca_nom} - Canvis: {string.Join(", ", canvis)}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errors++;
                        missatges.Add($"Error processant comarca {element.comarca_nom}: {ex.Message}");
                        await _dbLogger.LogFatalAsync($"Error crític processant comarca {element.comarca_nom}", ex, "SincronitzarComarquesAsync", $"ComarcaNom={element.comarca_nom}");
                    }
                }

                // Guardar canvis
                if (actualitzades > 0)
                {
                    await _context.SaveChangesAsync();
                    missatges.Add($"S'han desat {actualitzades} canvis a la base de dades");
                }
                else
                {
                    missatges.Add("No s'han detectat canvis en les dades de comarques");
                }

                await _dbLogger.LogInformationAsync($"Sincronització completada: {actualitzades} actualitzades, {errors} errors", "SincronitzarComarquesAsync");
            }
            catch (Exception ex)
            {
                errors++;
                missatges.Add($"Error general en la sincronitzaci\u00f3: {ex.Message}");
                await _dbLogger.LogFatalAsync("Error crític general en la sincronització de comarques", ex, "SincronitzarComarquesAsync");
            }

            return (actualitzades, errors, missatges);
        }
    }
}