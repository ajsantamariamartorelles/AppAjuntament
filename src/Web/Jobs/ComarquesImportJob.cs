using System.Net.Http.Json;
using AppAjuntament.Models;
using AppAjuntament.Models.Base.Comarca;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppAjuntament.Jobs;

public class ComarquesImportJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ComarquesImportJob> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ComarquesImportJob(IServiceProvider serviceProvider, ILogger<ComarquesImportJob> logger, IHttpClientFactory httpClientFactory)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task ImportComarquesAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetFromJsonAsync<ComarquesApiResponse>("https://do.diba.cat/api/tipus/comarca");
            if (response?.datasets?[0]?.elements == null)
            {
                _logger.LogWarning("No comarques data found in API response.");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GestorSubvencionsContext>();

            foreach (var c in response.datasets[0].elements!)
            {
                int codi = int.Parse(c.comarca_id);
                var comarca = await db.Comarques.FindAsync(codi) ?? new Comarca { Codi = codi, Nom = c.comarca_nom };
                comarca.Nom = c.comarca_nom;
                comarca.Coordenades = c.comarca_geo;
                comarca.NomDBPedia = c.nom_dbpedia;
                comarca.CCAdrecaCompleta = c.grup_cc?.cc_adreca_completa;
                comarca.CCAdreca = c.grup_cc?.cc_adreca;
                comarca.CCCodiPostal = c.grup_cc?.cc_cpostal;
                comarca.CCEmail = c.grup_cc?.cc_email != null ? System.Text.Json.JsonSerializer.Serialize(c.grup_cc.cc_email) : null;
                comarca.CCFax = c.grup_cc?.cc_fax;
                comarca.CCWeb = c.grup_cc?.cc_web;
                if (db.Entry(comarca).State == EntityState.Detached)
                    db.Comarques.Add(comarca);
            }
            await db.SaveChangesAsync();
            _logger.LogInformation("Comarques import completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing comarques");
        }
    }

    // Classes per deserialitzar la resposta de l'API
    public class ComarquesApiResponse
    {
        public List<Dataset>? datasets { get; set; }
    }
    public class Dataset
    {
        public List<Element>? elements { get; set; }
    }
    public class Element
    {
        public string comarca_id { get; set; } = "";
        public string comarca_nom { get; set; } = "";
        public string comarca_geo { get; set; } = "";
        public string nom_dbpedia { get; set; } = "";
        public GrupCC? grup_cc { get; set; }
    }
    public class GrupCC
    {
        public string? cc_adreca_completa { get; set; }
        public string? cc_adreca { get; set; }
        public string? cc_cpostal { get; set; }
        public List<string>? cc_email { get; set; }
        public string? cc_fax { get; set; }
        public string? cc_web { get; set; }
    }
}
