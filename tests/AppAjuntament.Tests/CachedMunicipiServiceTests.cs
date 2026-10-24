using AppAjuntament.Models;
using AppAjuntament.Models.Base.Comarca;
using AppAjuntament.Models.Base.Contacte;
using AppAjuntament.Models.Base.Ens;
using AppAjuntament.Models.Base.Geo;
using AppAjuntament.Models.Base.Imatge;
using AppAjuntament.Models.Base.Provincia;
using AppAjuntament.Models.Municipi;
using AppAjuntament.Tests.Infraestructura;
using AppAjuntament.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace AppAjuntament.Tests;

public class CachedMunicipiServiceTests : IDisposable
{
    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "municipi_tests_" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_carpeta)) Directory.Delete(_carpeta, recursive: true);
    }

    private sealed class ApiFalsa : IMunicipiApiService
    {
        public ApiMunicipiElement? Resposta { get; set; }
        public bool Llanca { get; set; }
        public int Crides { get; private set; }

        public Task<ApiMunicipiElement?> ObtindreInfoMunicipiAsync(string nomMunicipi)
        {
            Crides++;
            if (Llanca) throw new HttpRequestException("500");
            return Task.FromResult(Resposta);
        }
    }

    private static readonly AjuntamentSettings Config = new()
    {
        CodiINE6 = "000007", CodiDiba = "00000", NomMunicipi = "Poble de Prova",
        Comarca = "Comarca de Prova", Provincia = "Provincia de Prova"
    };

    private static readonly AjuntamentSettings ConfigAmbEns = new()
    {
        CodiINE6 = "000007", CodiEns = "000070005", CodiDiba = "00000", NomMunicipi = "Poble de Prova",
        Comarca = "Comarca de Prova", Provincia = "Provincia de Prova"
    };

    private CachedMunicipiService Servei(ApiFalsa api, IMemoryCache? cache = null, AjuntamentSettings? config = null, BdProves? bd = null) =>
        new(api, cache ?? new MemoryCache(new MemoryCacheOptions()), NullLogger<CachedMunicipiService>.Instance,
            config is null ? null : Options.Create(config), bd?.NovaFactoria(), _carpeta);

    private static void SembraEns(BdProves bd)
    {
        using var db = bd.NouContext();
        db.Ens.Add(new Ens
        {
            Nom = "Ajuntament de Poble de Prova", NomCurt = "Poble de Prova", CodiEns = "000070005", INE6 = "000007",
            CIF = "P0000000X", AdrecaCompleta = "Pl. de Prova, 1, 00000 Poble de Prova",
            Comarca = new Comarca { Codi = 99, Nom = "Comarca de la BD" },
            Provincia = new Provincia { Id = "9", Nom = "Provincia de la BD" },
            Contacte = new Contacte { Adreca = "Pl. de Prova, 1", CodiPostal = "00000", TelefonContacte = "900000000", Email = "ajuntament@exemple.test", Web = "https://exemple.test" },
            Imatge = new Imatge { EscutURL = "https://exemple.test/escut.png", BanderaURL = "https://exemple.test/bandera.gif", VistaURL = "https://exemple.test/vista.jpg" },
            DadesGeografiques = new DadesGeografiques { NombreHabitants = 4321, Extensio = 8.79m, Altitud = 158, CentreMunicipalLatitud = 41.5, CentreMunicipalLongitud = 2.25 },
        });
        db.SaveChanges();
    }

    private static ApiMunicipiElement Municipi() =>
        new() { ine = "000000", municipi_nom = "Poble de Prova", nombre_habitants = "1234" };

    [Fact]
    public async Task Si_l_api_respon_es_serveix_i_es_desa_una_copia()
    {
        var api = new ApiFalsa { Resposta = Municipi() };

        var resultat = await Servei(api).ObtindreInfoMunicipiAsync("Poble de Prova");

        Assert.Equal("Poble de Prova", resultat?.municipi_nom);
        Assert.Single(Directory.GetFiles(_carpeta, "*.json"));
    }

    [Fact]
    public async Task Si_l_api_falla_es_serveix_l_ultima_copia_bona_encara_que_es_reinici_l_app()
    {
        await Servei(new ApiFalsa { Resposta = Municipi() }).ObtindreInfoMunicipiAsync("Poble de Prova");

        // "Reinici": memòria cau nova, i ara la Diputació no respon.
        var resultat = await Servei(new ApiFalsa { Resposta = null }).ObtindreInfoMunicipiAsync("Poble de Prova");

        Assert.Equal("Poble de Prova", resultat?.municipi_nom);
        Assert.Equal("1234", resultat?.nombre_habitants);
    }

    [Fact]
    public async Task Si_l_api_llanca_una_excepcio_tambe_es_serveix_la_copia()
    {
        await Servei(new ApiFalsa { Resposta = Municipi() }).ObtindreInfoMunicipiAsync("Poble de Prova");

        var resultat = await Servei(new ApiFalsa { Llanca = true }).ObtindreInfoMunicipiAsync("Poble de Prova");

        Assert.Equal("Poble de Prova", resultat?.municipi_nom);
    }

    [Fact]
    public async Task Sense_copia_i_amb_l_api_caiguda_es_construeix_un_perfil_provisional_amb_la_configuracio()
    {
        var resultat = await Servei(new ApiFalsa { Resposta = null }, config: Config).ObtindreInfoMunicipiAsync("Poble de Prova");

        Assert.NotNull(resultat);
        Assert.True(resultat!.Provisional);
        Assert.Equal("Poble de Prova", resultat.municipi_nom);
        Assert.Equal("Comarca de Prova", resultat.grup_comarca?.comarca_nom);
        Assert.Equal("https://media.diba.cat/diba/municipis/img/escuts/ec00000.png", resultat.municipi_escut);
        // No s'inventa res que no sapiguem.
        Assert.Null(resultat.nombre_habitants);
        Assert.Null(resultat.grup_ajuntament);
    }

    [Fact]
    public async Task Sense_copia_i_amb_l_api_caiguda_es_serveixen_les_dades_de_la_taula_ens()
    {
        using var bd = new BdProves();
        SembraEns(bd);

        var resultat = await Servei(new ApiFalsa { Resposta = null }, config: ConfigAmbEns, bd: bd)
            .ObtindreInfoMunicipiAsync("Poble de Prova");

        Assert.NotNull(resultat);
        Assert.False(resultat!.Provisional);
        Assert.Equal("Poble de Prova", resultat.municipi_nom);
        Assert.Equal("Comarca de la BD", resultat.grup_comarca?.comarca_nom);
        Assert.Equal("Provincia de la BD", resultat.grup_provincia?.provincia_nom);
        Assert.Equal("4321", resultat.nombre_habitants);
        Assert.Equal("8.79", resultat.extensio);
        Assert.Equal("158", resultat.altitud);
        Assert.Equal("41.5,2.25", resultat.centre_municipal);
        Assert.Equal("900000000", resultat.grup_ajuntament?.telefon_contacte);
        Assert.Equal("ajuntament@exemple.test", resultat.grup_ajuntament?.email);
        Assert.Equal("https://exemple.test/escut.png", resultat.municipi_escut);
    }

    [Fact]
    public async Task Les_imatges_de_la_taula_ens_manen_sobre_les_de_l_api_quan_aquesta_respon()
    {
        using var bd = new BdProves();
        SembraEns(bd);
        var api = new ApiFalsa
        {
            Resposta = new ApiMunicipiElement { municipi_nom = "Poble de Prova", municipi_vista = "https://media.diba.cat/vista-api.jpg", municipi_escut = "https://media.diba.cat/escut-api.png" }
        };

        var resultat = await Servei(api, config: ConfigAmbEns, bd: bd).ObtindreInfoMunicipiAsync("Poble de Prova");

        Assert.Equal("https://exemple.test/vista.jpg", resultat!.municipi_vista);
        Assert.Equal("https://exemple.test/escut.png", resultat.municipi_escut);
    }

    [Fact]
    public async Task Si_ens_no_te_imatge_es_queden_les_de_l_api()
    {
        using var bd = new BdProves();
        using (var db = bd.NouContext())
        {
            db.Ens.Add(new Ens { Nom = "Ajuntament de Poble de Prova", CodiEns = "000070005", INE6 = "000007" });
            db.SaveChanges();
        }
        var api = new ApiFalsa { Resposta = new ApiMunicipiElement { municipi_nom = "Poble de Prova", municipi_vista = "https://media.diba.cat/vista-api.jpg" } };

        var resultat = await Servei(api, config: ConfigAmbEns, bd: bd).ObtindreInfoMunicipiAsync("Poble de Prova");

        Assert.Equal("https://media.diba.cat/vista-api.jpg", resultat!.municipi_vista);
    }

    [Fact]
    public void La_imatge_de_portada_que_porta_l_app_existeix_al_projecte()
    {
        // El perfil provisional i el script db/actualitza_imatge_portada.sql la fan servir.
        var cami = Path.Combine(ArrelDelRepositori.Trobar(), "src", "Web", "wwwroot", "img", "municipi", "portada.jpg");

        Assert.True(File.Exists(cami), "Falta wwwroot/img/municipi/portada.jpg");
    }

    [Fact]
    public async Task El_perfil_provisional_fa_servir_la_portada_local()
    {
        var resultat = await Servei(new ApiFalsa { Resposta = null }, config: Config).ObtindreInfoMunicipiAsync("Poble de Prova");

        Assert.Equal("/img/municipi/portada.jpg", resultat!.municipi_vista);
    }

    [Fact]
    public async Task La_fila_d_ens_es_troba_tambe_pel_codi_de_la_diputacio_com_fa_la_resta_de_l_app()
    {
        // Regressió: a la BD real ens.CodiEns porta el codi de la Diputació (08256), no el de la Seu-e,
        // i sense INE6 (o amb un altre). Abans no es trobava i la pàgina queia al perfil de la configuració.
        using var bd = new BdProves();
        using (var db = bd.NouContext())
        {
            db.Ens.Add(new Ens
            {
                Nom = "Ajuntament de Poble de Prova", CodiEns = "00000", INE6 = null,
                Imatge = new Imatge { VistaURL = "/img/municipi/portada.jpg" },
                DadesGeografiques = new DadesGeografiques { NombreHabitants = 4321 },
            });
            db.SaveChanges();
        }

        var resultat = await Servei(new ApiFalsa { Resposta = null }, config: ConfigAmbEns, bd: bd)
            .ObtindreInfoMunicipiAsync("Poble de Prova");

        Assert.False(resultat!.Provisional);
        Assert.Equal("4321", resultat.nombre_habitants);
    }

    [Fact]
    public async Task La_copia_bona_es_prefereix_a_la_taula_ens()
    {
        using var bd = new BdProves();
        SembraEns(bd);
        await Servei(new ApiFalsa { Resposta = Municipi() }, config: ConfigAmbEns, bd: bd).ObtindreInfoMunicipiAsync("Poble de Prova");

        var resultat = await Servei(new ApiFalsa { Resposta = null }, config: ConfigAmbEns, bd: bd).ObtindreInfoMunicipiAsync("Poble de Prova");

        Assert.Equal("1234", resultat!.nombre_habitants);   // el de la còpia, no el 4321 de la BD
    }

    [Fact]
    public async Task Si_el_municipi_no_es_a_ens_s_usa_el_perfil_de_la_configuracio()
    {
        using var bd = new BdProves();   // sense cap fila a ens

        var resultat = await Servei(new ApiFalsa { Resposta = null }, config: ConfigAmbEns, bd: bd).ObtindreInfoMunicipiAsync("Poble de Prova");

        Assert.True(resultat!.Provisional);
    }

    [Fact]
    public async Task El_perfil_provisional_no_substitueix_la_copia_bona_ni_es_desa()
    {
        await Servei(new ApiFalsa { Resposta = null }, config: Config).ObtindreInfoMunicipiAsync("Poble de Prova");

        Assert.False(Directory.Exists(_carpeta) && Directory.GetFiles(_carpeta, "*.json").Length > 0);
    }

    [Fact]
    public async Task Amb_copia_bona_es_prefereix_a_la_provisional()
    {
        await Servei(new ApiFalsa { Resposta = Municipi() }, config: Config).ObtindreInfoMunicipiAsync("Poble de Prova");

        var resultat = await Servei(new ApiFalsa { Resposta = null }, config: Config).ObtindreInfoMunicipiAsync("Poble de Prova");

        Assert.False(resultat!.Provisional);
        Assert.Equal("1234", resultat.nombre_habitants);
    }

    [Fact]
    public async Task Sense_copia_ni_configuracio_i_amb_l_api_caiguda_torna_null()
    {
        var resultat = await Servei(new ApiFalsa { Resposta = null }).ObtindreInfoMunicipiAsync("Poble de Prova");

        Assert.Null(resultat);
    }

    [Fact]
    public async Task Despres_d_una_fallada_no_es_torna_a_cridar_l_api_a_cada_visita()
    {
        var api = new ApiFalsa { Resposta = null };
        var servei = Servei(api);

        for (var i = 0; i < 5; i++)
            await servei.ObtindreInfoMunicipiAsync("Poble de Prova");

        Assert.Equal(1, api.Crides);
    }

    [Fact]
    public async Task Invalidar_la_cache_permet_tornar_a_provar_l_api()
    {
        var api = new ApiFalsa { Resposta = null };
        var servei = Servei(api);
        await servei.ObtindreInfoMunicipiAsync("Poble de Prova");

        api.Resposta = Municipi();
        servei.InvalidateCache("Poble de Prova");
        var resultat = await servei.ObtindreInfoMunicipiAsync("Poble de Prova");

        Assert.Equal(2, api.Crides);
        Assert.Equal("Poble de Prova", resultat?.municipi_nom);
    }

    [Fact]
    public void El_contenidor_d_injeccio_el_sap_construir_com_a_Program_cs()
    {
        // Regressió: el paràmetre opcional de la carpeta de còpies no ha de trencar l'arrencada.
        var serveis = new ServiceCollection();
        serveis.AddLogging();
        serveis.AddMemoryCache();
        serveis.AddScoped<IMunicipiApiService>(_ => new ApiFalsa());
        serveis.AddScoped<ICachedMunicipiService, CachedMunicipiService>();

        using var proveidor = serveis.BuildServiceProvider(validateScopes: true);
        using var scope = proveidor.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ICachedMunicipiService>());
    }
}
