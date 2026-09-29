using AppAjuntament.Models;
using AppAjuntament.Models.Cursets;
using AppAjuntament.Models.Cursets.Dto;
using AppAjuntament.Services.Cursets;
using AppAjuntament.Tests.Infraestructura;
using Xunit;
using static AppAjuntament.Tests.Infraestructura.DadesCursets;

namespace AppAjuntament.Tests;

/// <summary>
/// Seguiment del regidor: només veu els seus cursets i les xifres han de ser exactes,
/// perquè són el que es fa servir per controlar cobraments.
/// </summary>
public class CursetsSeguimentServiceTests : IDisposable
{
    private const int RegidorA = 1;
    private const int RegidorB = 2;

    private readonly BdProves _bd = new();

    /// <summary>El servei llegeix amb un context i les dades es preparen amb un altre.</summary>
    private (CursetsSeguimentService servei, GestorSubvencionsContext dades) Preparar() =>
        (new CursetsSeguimentService(_bd.NouContext()), _bd.NouContext());

    public void Dispose() => _bd.Dispose();

    // ---------- Aïllament entre regidors ----------

    [Fact]
    public async Task Un_regidor_nomes_veu_els_seus_cursets()
    {
        var (servei, bd) = Preparar();
        var seu = Curset(bd, "Dimarts", RegidorA);
        Curset(bd, "Dijous", RegidorB);
        Curset(bd, "Sense regidor", regidorId: null);

        var resum = await servei.GetResumCursetsAsync(RegidorA);

        var unic = Assert.Single(resum);
        Assert.Equal(seu.Id, unic.CursetId);
    }

    [Fact]
    public async Task Un_curset_d_un_altre_regidor_es_comporta_com_si_no_existis()
    {
        var (servei, bd) = Preparar();
        var alienat = Curset(bd, "Dijous", RegidorB);
        var alumne = Alumne(bd, "Anna");
        Inscripcio(bd, alienat, alumne, EstatInscripcio.Admesa);

        Assert.Null(await servei.GetInscritesAsync(RegidorA, alienat.Id));
        Assert.Null(await servei.GetPersonaAsync(RegidorA, alienat.Id, alumne.Id));
    }

    [Fact]
    public async Task Els_cobraments_no_inclouen_liquidacions_d_altres_regidors()
    {
        var (servei, bd) = Preparar();
        var seu = Curset(bd, "Dimarts", RegidorA);
        var alienat = Curset(bd, "Dijous", RegidorB);
        var alumne = Alumne(bd, "Anna");
        Inscripcio(bd, seu, alumne, EstatInscripcio.Admesa);
        Inscripcio(bd, alienat, alumne, EstatInscripcio.Admesa);
        Liquidacio(bd, seu, alumne, 20m);
        Liquidacio(bd, alienat, alumne, 999m);

        var cobraments = await servei.GetCobramentsAsync(RegidorA);

        Assert.Equal(20m, cobraments.Total.Liquidat);
        Assert.Single(cobraments.Cursets);
    }

    [Fact]
    public async Task Els_cursets_inactius_no_surten_al_resum()
    {
        var (servei, bd) = Preparar();
        Curset(bd, "Antic", RegidorA, actiu: false);

        Assert.Empty(await servei.GetResumCursetsAsync(RegidorA));
    }

    // ---------- Imports ----------

    [Fact]
    public async Task Les_liquidacions_anullades_no_compten_als_imports()
    {
        var (servei, bd) = Preparar();
        var curset = Curset(bd, "Dimarts", RegidorA);
        var alumne = Alumne(bd, "Anna");
        Inscripcio(bd, curset, alumne, EstatInscripcio.Admesa);
        Liquidacio(bd, curset, alumne, 30m, EstatLiquidacio.Cobrada);
        Liquidacio(bd, curset, alumne, 20m, EstatLiquidacio.Emesa);
        Liquidacio(bd, curset, alumne, 500m, EstatLiquidacio.Anullada);

        var inscrites = await servei.GetInscritesAsync(RegidorA, curset.Id);

        Assert.NotNull(inscrites);
        Assert.Equal(50m, inscrites.Resum.Liquidat);
        Assert.Equal(30m, inscrites.Resum.Cobrat);
        Assert.Equal(20m, inscrites.Resum.Pendent);
        Assert.Equal(2, inscrites.Resum.NumLiquidacions);
    }

    // ---------- Situació de pagament ----------

    [Fact]
    public async Task Una_persona_amb_liquidacio_emesa_te_pagament_pendent()
    {
        var (servei, bd) = Preparar();
        var curset = Curset(bd, "Dimarts", RegidorA);
        var alumne = Alumne(bd, "Anna");
        Inscripcio(bd, curset, alumne, EstatInscripcio.Admesa);
        Liquidacio(bd, curset, alumne, 20m, EstatLiquidacio.Emesa, DateTime.Today.AddDays(-40));
        Liquidacio(bd, curset, alumne, 15m, EstatLiquidacio.Emesa, DateTime.Today.AddDays(-5), "T2");

        var persona = (await servei.GetInscritesAsync(RegidorA, curset.Id))!.Persones.Single();

        Assert.Equal(SituacioPagament.Pendent, persona.Situacio);
        Assert.Equal(35m, persona.ImportPendent);
        Assert.Equal(2, persona.NumPendents);
        Assert.Equal(DateTime.Today.AddDays(-40), persona.PendentMesAntic);
    }

    [Fact]
    public async Task Una_persona_amb_tot_cobrat_esta_al_corrent()
    {
        var (servei, bd) = Preparar();
        var curset = Curset(bd, "Dimarts", RegidorA);
        var alumne = Alumne(bd, "Anna");
        Inscripcio(bd, curset, alumne, EstatInscripcio.Admesa);
        Liquidacio(bd, curset, alumne, 20m, EstatLiquidacio.Cobrada);

        var persona = (await servei.GetInscritesAsync(RegidorA, curset.Id))!.Persones.Single();

        Assert.Equal(SituacioPagament.AlCorrent, persona.Situacio);
    }

    [Fact]
    public async Task Una_persona_admesa_sense_liquidacions_queda_sense_liquidar()
    {
        var (servei, bd) = Preparar();
        var curset = Curset(bd, "Dimarts", RegidorA);
        Inscripcio(bd, curset, Alumne(bd, "Anna"), EstatInscripcio.Admesa);

        var persona = (await servei.GetInscritesAsync(RegidorA, curset.Id))!.Persones.Single();

        Assert.Equal(SituacioPagament.SenseLiquidar, persona.Situacio);
    }

    [Fact]
    public async Task La_llista_d_espera_no_factura()
    {
        var (servei, bd) = Preparar();
        var curset = Curset(bd, "Dimarts", RegidorA);
        Inscripcio(bd, curset, Alumne(bd, "Anna"), EstatInscripcio.LlistaEspera, ordreLlistaEspera: 1);

        var persona = (await servei.GetInscritesAsync(RegidorA, curset.Id))!.Persones.Single();

        Assert.Equal(SituacioPagament.NoFactura, persona.Situacio);
    }

    [Fact]
    public async Task Qui_ha_deixat_el_curset_pero_deu_encara_apareix_com_a_pendent()
    {
        var (servei, bd) = Preparar();
        var curset = Curset(bd, "Dimarts", RegidorA);
        var alumne = Alumne(bd, "Anna");
        Inscripcio(bd, curset, alumne, EstatInscripcio.Baixa, dataBaixa: DateTime.Today.AddDays(-20));
        Liquidacio(bd, curset, alumne, 12m);

        var persona = (await servei.GetInscritesAsync(RegidorA, curset.Id))!.Persones.Single();

        Assert.Equal(SituacioPagament.Pendent, persona.Situacio);
        Assert.Equal(12m, persona.ImportPendent);
    }

    // ---------- Qui és visible ----------

    [Fact]
    public async Task Les_solicituds_nomes_surten_amb_el_periode_d_inscripcio_obert()
    {
        var (servei, bd) = Preparar();
        var obert = Curset(bd, "Obert", RegidorA, DateTime.Today.AddDays(-5), DateTime.Today.AddDays(5));
        var tancat = Curset(bd, "Tancat", RegidorA, DateTime.Today.AddDays(-30), DateTime.Today.AddDays(-10));
        Inscripcio(bd, obert, Alumne(bd, "Anna"), EstatInscripcio.Sollicitada);
        Inscripcio(bd, tancat, Alumne(bd, "Berta"), EstatInscripcio.Sollicitada);

        Assert.Single((await servei.GetInscritesAsync(RegidorA, obert.Id))!.Persones);
        Assert.Empty((await servei.GetInscritesAsync(RegidorA, tancat.Id))!.Persones);
    }

    [Fact]
    public async Task Les_rebutjades_no_surten_mai()
    {
        var (servei, bd) = Preparar();
        var curset = Curset(bd, "Dimarts", RegidorA, DateTime.Today.AddDays(-5), DateTime.Today.AddDays(5));
        Inscripcio(bd, curset, Alumne(bd, "Anna"), EstatInscripcio.Rebutjada);

        Assert.Empty((await servei.GetInscritesAsync(RegidorA, curset.Id))!.Persones);
    }

    [Fact]
    public async Task Les_persones_s_ordenen_admeses_llista_d_espera_i_baixes()
    {
        var (servei, bd) = Preparar();
        var curset = Curset(bd, "Dimarts", RegidorA);
        Inscripcio(bd, curset, Alumne(bd, "Baixa"), EstatInscripcio.Baixa, dataBaixa: DateTime.Today.AddDays(-2));
        Inscripcio(bd, curset, Alumne(bd, "Espera2"), EstatInscripcio.LlistaEspera, 2);
        Inscripcio(bd, curset, Alumne(bd, "Espera1"), EstatInscripcio.LlistaEspera, 1);
        Inscripcio(bd, curset, Alumne(bd, "Admesa"), EstatInscripcio.Admesa);

        var noms = (await servei.GetInscritesAsync(RegidorA, curset.Id))!.Persones
            .Select(p => p.NomComplet).ToList();

        Assert.Equal(new[] { "Admesa", "Espera1", "Espera2", "Baixa" }, noms);
    }

    [Fact]
    public async Task El_resum_compta_admeses_llista_d_espera_i_pendents()
    {
        var (servei, bd) = Preparar();
        var curset = Curset(bd, "Dimarts", RegidorA);
        var deutora = Alumne(bd, "Deutora");
        Inscripcio(bd, curset, deutora, EstatInscripcio.Admesa);
        Inscripcio(bd, curset, Alumne(bd, "Al corrent"), EstatInscripcio.Admesa);
        Inscripcio(bd, curset, Alumne(bd, "Espera"), EstatInscripcio.LlistaEspera, 1);
        Liquidacio(bd, curset, deutora, 25m);

        var resum = (await servei.GetResumCursetsAsync(RegidorA)).Single();

        Assert.Equal(2, resum.NumAdmeses);
        Assert.Equal(1, resum.NumLlistaEspera);
        Assert.Equal(1, resum.NumPendents);
        Assert.Equal(25m, resum.ImportPendent);
        Assert.True(resum.TeLiquidacions);
    }

    // ---------- Fitxa de la persona ----------

    [Fact]
    public async Task La_fitxa_compta_nomes_les_sessions_tancades_mentre_hi_era_apuntada()
    {
        var (servei, bd) = Preparar();
        var curset = Curset(bd, "Dimarts", RegidorA);
        var anna = Alumne(bd, "Anna");
        Inscripcio(bd, curset, anna, EstatInscripcio.Admesa, dataAlta: new DateTime(2026, 2, 1));
        Sessio(bd, curset, new DateTime(2026, 1, 20), assistencies: (anna, true));        // abans de l'alta
        Sessio(bd, curset, new DateTime(2026, 2, 3), assistencies: (anna, true));         // present
        Sessio(bd, curset, new DateTime(2026, 2, 10), assistencies: (anna, false));       // absent
        Sessio(bd, curset, new DateTime(2026, 2, 17), EstatSessio.Oberta, (anna, true));  // no tancada

        var fitxa = await servei.GetPersonaAsync(RegidorA, curset.Id, anna.Id);

        Assert.NotNull(fitxa);
        Assert.Equal(2, fitxa.SessionsImpartides);
        Assert.Equal(1, fitxa.SessionsAssistides);
        Assert.Equal(new DateTime(2026, 2, 3), fitxa.UltimaAssistencia);
    }

    [Theory]
    [InlineData(true, 2)]
    [InlineData(false, 3)]
    public async Task La_fitxa_aplica_el_preu_segons_si_esta_empadronada(bool empadronada, int preuEsperat)
    {
        var (servei, bd) = Preparar();
        var curset = Curset(bd, "Dimarts", RegidorA, preuEmpadronat: 2m, preuNoEmpadronat: 3m);
        var alumne = Alumne(bd, "Anna", empadronat: empadronada);
        Inscripcio(bd, curset, alumne, EstatInscripcio.Admesa);

        var fitxa = await servei.GetPersonaAsync(RegidorA, curset.Id, alumne.Id);

        Assert.Equal(preuEsperat, fitxa!.PreuPerSessio);
    }

    [Fact]
    public async Task La_fitxa_d_una_persona_no_inscrita_al_curset_es_nul_la()
    {
        var (servei, bd) = Preparar();
        var curset = Curset(bd, "Dimarts", RegidorA);
        var forastera = Alumne(bd, "Anna");

        Assert.Null(await servei.GetPersonaAsync(RegidorA, curset.Id, forastera.Id));
    }
}
