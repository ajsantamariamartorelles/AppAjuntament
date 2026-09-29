using AppAjuntament.Models.Cursets.Dto;
using AppAjuntament.Models.Tercers;
using AppAjuntament.Services.Cursets;
using AppAjuntament.Tests.Infraestructura;
using Xunit;

namespace AppAjuntament.Tests;

/// <summary>
/// L'alta d'un alumne des de la gestió web crea un Tercer nou si el DNI no
/// existia. En aquest cas cal que el personal confirmi que la persona ha
/// signat l'autorització de tractament de dades (AGENTS.md).
/// </summary>
public class CursetsCatalogServiceAutoritzacioTests : IDisposable
{
    private readonly BdProves _bd = new();

    private CursetsCatalogService Servei() => new(_bd.NouContext());

    public void Dispose() => _bd.Dispose();

    [Fact]
    public async Task Crear_alumne_amb_dni_nou_sense_autoritzacio_falla()
    {
        var servei = Servei();

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => servei.CrearAlumneAsync(new CrearAlumneRequest
        {
            Nom = "Anna",
            Cognoms = "Puig",
            Dni = "11111111H",
            AutoritzacioTractamentSignada = false
        }));

        Assert.Contains("autorització", ex.Message);
    }

    [Fact]
    public async Task Crear_alumne_amb_dni_nou_i_autoritzacio_funciona()
    {
        var servei = Servei();

        var alumne = await servei.CrearAlumneAsync(new CrearAlumneRequest
        {
            Nom = "Anna",
            Cognoms = "Puig",
            Dni = "11111111H",
            AutoritzacioTractamentSignada = true
        });

        using var context = _bd.NouContext();
        var tercer = context.Tercers.Single(t => t.Id == alumne.TercerId);
        Assert.True(tercer.AutoritzacioTractamentSignada);
        Assert.NotNull(tercer.DataAutoritzacio);
    }

    [Fact]
    public async Task Reutilitzar_un_tercer_existent_no_requereix_autoritzacio()
    {
        using var dades = _bd.NouContext();
        var existent = new Tercer { Nom = "Berta", Cognoms = "Roig", DNI = "22222222J" };
        dades.Tercers.Add(existent);
        dades.SaveChanges();

        var servei = Servei();
        var alumne = await servei.CrearAlumneAsync(new CrearAlumneRequest
        {
            Nom = "Berta",
            Cognoms = "Roig",
            Dni = "22222222J",
            AutoritzacioTractamentSignada = false
        });

        Assert.Equal(existent.Id, alumne.TercerId);
    }
}
