using System.Text.RegularExpressions;
using AppAjuntament.Services;
using Xunit;

namespace AppAjuntament.Tests;

public class InfoVersioTests
{
    [Fact]
    public void La_versio_porta_data_i_hora_de_la_compilacio()
    {
        // "v1.0.<aaaammdd>.<hhmm>", amb el commit entre parèntesis quan el build el porta.
        Assert.Matches(new Regex(@"^v1\.0\.\d{8}\.\d{4}( \([0-9a-f]{7}\))?$"), InfoVersio.Text);
    }
}
