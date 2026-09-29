using AppAjuntament.Tests.Infraestructura;
using Xunit;

namespace AppAjuntament.Tests;

public class BdProvesTests
{
    [Fact]
    public void El_model_complet_es_pot_crear_en_sqlite()
    {
        using var bd = new BdProves();
        using var context = bd.NouContext();

        Assert.True(context.Database.CanConnect());
    }
}
