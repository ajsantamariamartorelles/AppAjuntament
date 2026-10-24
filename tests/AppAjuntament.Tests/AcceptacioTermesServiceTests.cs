using AppAjuntament.Services;
using AppAjuntament.Tests.Infraestructura;
using Xunit;

namespace AppAjuntament.Tests;

public class AcceptacioTermesServiceTests : IDisposable
{
    private readonly BdProves _bd = new();

    private IAcceptacioTermesService Servei() => new AcceptacioTermesService(_bd.NovaFactoria());

    public void Dispose() => _bd.Dispose();

    [Fact]
    public async Task Les_comprovacions_simultanies_no_es_trepitgen()
    {
        // Regressió: MainLayout comprova l'acceptació alhora que la pàgina consulta la BD;
        // amb un context compartit EF llançava "A second operation was started...".
        var servei = Servei();

        var resultats = await Task.WhenAll(Enumerable.Range(0, 8)
            .Select(_ => servei.HaAcceptatVersioActualAsync("anna@exemple.test")));

        Assert.All(resultats, Assert.False);
    }

    [Fact]
    public async Task Sense_acceptar_retorna_fals()
    {
        var servei = Servei();

        Assert.False(await servei.HaAcceptatVersioActualAsync("anna@exemple.test"));
    }

    [Fact]
    public async Task Despres_d_acceptar_retorna_cert()
    {
        var servei = Servei();
        await servei.AcceptaAsync("anna@exemple.test");

        Assert.True(await Servei().HaAcceptatVersioActualAsync("anna@exemple.test"));
    }

    [Fact]
    public async Task El_correu_es_normalitza_ignorant_majuscules_i_espais()
    {
        var servei = Servei();
        await servei.AcceptaAsync("  Anna@Exemple.Test  ");

        Assert.True(await Servei().HaAcceptatVersioActualAsync("anna@exemple.test"));
    }

    [Fact]
    public async Task Acceptar_dues_vegades_no_duplica_la_fila()
    {
        var servei = Servei();
        await servei.AcceptaAsync("anna@exemple.test");
        await Servei().AcceptaAsync("anna@exemple.test");

        using var context = _bd.NouContext();
        Assert.Single(context.AcceptacionsTermes);
    }

    [Fact]
    public async Task Correus_diferents_no_s_afecten_entre_si()
    {
        var servei = Servei();
        await servei.AcceptaAsync("anna@exemple.test");

        Assert.False(await Servei().HaAcceptatVersioActualAsync("berta@exemple.test"));
    }
}
