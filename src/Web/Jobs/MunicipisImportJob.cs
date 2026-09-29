using System.Net.Http.Json;
using AppAjuntament.Models;
using AppAjuntament.Models.Base.Comarca;
using MunicipiModel = AppAjuntament.Models.Municipi.Municipi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppAjuntament.Jobs;

public class MunicipisImportJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MunicipisImportJob> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public MunicipisImportJob(IServiceProvider serviceProvider, ILogger<MunicipisImportJob> logger, IHttpClientFactory httpClientFactory)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task ImportMunicipisAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetFromJsonAsync<MunicipisApiResponse>("https://do.diba.cat/api/tipus/municipi");
            if (response?.datasets?[0]?.elements == null)
            {
                _logger.LogWarning("No municipis data found in API response.");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GestorSubvencionsContext>();

            // Assegurar que totes les comarques referenciades existeixen abans d'inserir municipis (FK constraint)
            var existingComarcaCodes = (await db.Comarques.Select(c => c.Codi).ToListAsync()).ToHashSet();
            foreach (var m in response.datasets[0].elements!)
            {
                if (m.grup_comarca?.comarca_codi != null &&
                    int.TryParse(m.grup_comarca.comarca_codi, out var comarcaCodi) &&
                    !existingComarcaCodes.Contains(comarcaCodi))
                {
                    db.Comarques.Add(new Comarca
                    {
                        Codi = comarcaCodi,
                        Nom = m.grup_comarca.comarca_nom ?? $"Comarca {comarcaCodi}"
                    });
                    existingComarcaCodes.Add(comarcaCodi);
                }
            }
            await db.SaveChangesAsync();

            foreach (var m in response.datasets[0].elements!)
            {
                var ine = m.ine;
                var municipi = await db.Municipis.FindAsync(ine) ?? new MunicipiModel { Ine = ine };
                municipi.MunicipiNom = m.municipi_nom;
                municipi.MunicipiNomCurt = m.municipi_nom_curt;
                municipi.MunicipiArticle = m.municipi_article;
                municipi.MunicipiTransliterat = m.municipi_transliterat;
                municipi.MunicipiCurtTransliterat = m.municipi_curt_transliterat;
                municipi.CentreMunicipal = m.centre_municipal;
                municipi.ComarcaCodi = int.TryParse(m.grup_comarca?.comarca_codi, out var cc) ? cc : null;
                municipi.ProvinciaCodi = m.grup_provincia?.provincia_codi;
                municipi.AdrecaCompleta = m.grup_ajuntament?.adreca_completa;
                municipi.Adreca = m.grup_ajuntament?.adreca;
                municipi.CodiPostal = m.grup_ajuntament?.codi_postal;
                municipi.Localitzacio = m.grup_ajuntament?.localitzacio;
                municipi.TelefonContacte = m.grup_ajuntament?.telefon_contacte;
                municipi.Fax = m.grup_ajuntament?.fax;
                municipi.Email = m.grup_ajuntament?.email;
                municipi.UrlGeneral = m.grup_ajuntament?.url_general;
                municipi.CIF = m.grup_ajuntament?.cif;
                municipi.MunicipiEscut = m.municipi_escut;
                municipi.MunicipiBandera = m.municipi_bandera;
                municipi.MunicipiVista = m.municipi_vista;
                municipi.Ine6 = m.ine6;
                municipi.NomDBPedia = m.nom_dbpedia;
                municipi.NombreHabitants = int.TryParse(m.nombre_habitants, out var nh) ? nh : null;
                municipi.Extensio = decimal.TryParse(m.extensio, out var ext) ? ext : null;
                municipi.Altitud = int.TryParse(m.altitud, out var alt) ? alt : null;
                if (db.Entry(municipi).State == EntityState.Detached)
                    db.Municipis.Add(municipi);
            }
            await db.SaveChangesAsync();
            _logger.LogInformation("Municipis import completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing municipis");
        }
    }

    // Classes per deserialitzar la resposta de l'API
    public class MunicipisApiResponse
    {
        public List<Dataset>? datasets { get; set; }
    }
    public class Dataset
    {
        public List<Element>? elements { get; set; }
    }
    public class Element
    {
        public string ine { get; set; } = "";
        public string municipi_nom { get; set; } = "";
        public string municipi_nom_curt { get; set; } = "";
        public string municipi_article { get; set; } = "";
        public string municipi_transliterat { get; set; } = "";
        public string municipi_curt_transliterat { get; set; } = "";
        public string centre_municipal { get; set; } = "";
        public GrupComarca? grup_comarca { get; set; }
        public GrupProvincia? grup_provincia { get; set; }
        public GrupAjuntament? grup_ajuntament { get; set; }
        public string municipi_escut { get; set; } = "";
        public string municipi_bandera { get; set; } = "";
        public string municipi_vista { get; set; } = "";
        public string ine6 { get; set; } = "";
        public string nom_dbpedia { get; set; } = "";
        public string nombre_habitants { get; set; } = "";
        public string extensio { get; set; } = "";
        public string altitud { get; set; } = "";
    }
    public class GrupComarca
    {
        public string? comarca_codi { get; set; }
        public string? comarca_nom { get; set; }
    }
    public class GrupProvincia
    {
        public string? provincia_codi { get; set; }
        public string? provincia_nom { get; set; }
    }
    public class GrupAjuntament
    {
        public string? adreca_completa { get; set; }
        public string? adreca { get; set; }
        public string? codi_postal { get; set; }
        public string? localitzacio { get; set; }
        public string? telefon_contacte { get; set; }
        public string? fax { get; set; }
        public string? email { get; set; }
        public string? url_general { get; set; }
        public string? cif { get; set; }
    }
}
