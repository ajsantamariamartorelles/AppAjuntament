using AppAjuntament.Services;
using AppAjuntament.Tests.Infraestructura;
using Xunit;

namespace AppAjuntament.Tests;

public class AcceptacioTermesServiceTests : IDisposable
{
    private readonly BdProves _bd = new();

    private IAcceptacioTermesService Servei() => new AcceptacioTermesService(_bd.NouContext());

    public void Dispose() => _bd.Dispose();

    [Fact]
    public async Task Sense_acceptar_retorna_fals()
    {
        var servei = Servei();

        Assert.False(await servei.HaAcceptatVersioActualAsync("anna@santamariademartorelles.cat"));
    }

    [Fact]
    public async Task Despres_d_acceptar_retorna_cert()
    {
        var servei = Servei();
        await servei.AcceptaAsync("anna@santamariademartorelles.cat");

        Assert.True(await Servei().HaAcceptatVersioActualAsync("anna@santamariademartorelles.cat"));
    }

    [Fact]
    public async Task El_correu_es_normalitza_ignorant_majuscules_i_espais()
    {
        var servei = Servei();
        await servei.AcceptaAsync("  Anna@SantaMariaDeMartorelles.cat  ");

        Assert.True(await Servei().HaAcceptatVersioActualAsync("anna@santamariademartorelles.cat"));
    }

    [Fact]
    public async Task Acceptar_dues_vegades_no_duplica_la_fila()
    {
        var servei = Servei();
        await servei.AcceptaAsync("anna@santamariademartorelles.cat");
        await Servei().AcceptaAsync("anna@santamariademartorelles.cat");

        using var context = _bd.NouContext();
        Assert.Single(context.AcceptacionsTermes);
    }

    [Fact]
    public async Task Correus_diferents_no_s_afecten_entre_si()
    {
        var servei = Servei();
        await servei.AcceptaAsync("anna@santamariademartorelles.cat");

        Assert.False(await Servei().HaAcceptatVersioActualAsync("berta@santamariademartorelles.cat"));
    }
}
