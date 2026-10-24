using System.Text.Json;
using System.Text.RegularExpressions;
using AppAjuntament.Tests.Infraestructura;
using Xunit;

namespace AppAjuntament.Tests;

/// <summary>Comprovacions automàtiques de les regles d'AGENTS.md que es poden llegir al codi.</summary>
public class ReglesDelRepositoriTests
{
    // Regla 2: l'accés a dades és només amb EF Core, sense SQL cru ni ADO.NET.
    private static readonly Regex SqlCru = new(
        @"\b(FromSqlRaw|ExecuteSqlRaw|ExecuteSqlRawAsync|FromSqlInterpolated|ExecuteSqlInterpolated|SqlConnection|MySqlConnection|MySqlCommand)\b",
        RegexOptions.Compiled);

    /// <summary>
    /// Excepcions conegudes, cada una amb el seu motiu. Cap entrada nova sense el vistiplau
    /// dels responsables del repositori.
    /// </summary>
    private static readonly HashSet<string> Excepcions = new(StringComparer.OrdinalIgnoreCase)
    {
        // Registre d'errors de darrer recurs quan falla SaveChanges (evita recursivitat).
        "src/Web/Models/GestorSubvencionsContext.cs",
    };

    [Fact]
    public void No_hi_ha_sql_cru_fora_de_les_excepcions_conegudes()
    {
        var arrel = ArrelDelRepositori.Trobar();
        var infraccions = new List<string>();

        foreach (var fitxer in FitxersDeCodi(arrel))
        {
            var relatiu = Path.GetRelativePath(arrel, fitxer).Replace('\\', '/');
            if (Excepcions.Contains(relatiu)) continue;

            var linies = File.ReadAllLines(fitxer);
            for (var i = 0; i < linies.Length; i++)
            {
                var linia = linies[i].TrimStart();
                if (linia.StartsWith("//")) continue;
                if (SqlCru.IsMatch(linia))
                    infraccions.Add($"{relatiu}:{i + 1}  {linia}");
            }
        }

        Assert.True(infraccions.Count == 0,
            "SQL cru detectat (regla 2 d'AGENTS.md):\n" + string.Join("\n", infraccions));
    }

    // Regla 3: els appsettings versionats no porten cap valor sensible; aquests van
    // a appsettings.<Entorn>.secrets.json (ignorat pel git).
    private static readonly string[] ClausSensibles =
    {
        "ConnectionStrings:DefaultConnection", "AppBaseUrl",
        "AzureAd:Domain", "AzureAd:TenantId", "AzureAd:ClientId", "AzureAd:ClientSecret",
        "Authorization:Groups:GroupAccess1", "Authorization:Groups:GroupAccess2",
        "MinIO:AccessKey", "MinIO:SecretKey", "CursetsJwt:SigningKey",
    };

    [Theory]
    [InlineData("appsettings.json")]
    [InlineData("appsettings.Development.json")]
    [InlineData("appsettings.Production.json")]
    [InlineData("appsettings.secrets.sample.json")]
    public void Els_appsettings_versionats_no_porten_valors_sensibles(string fitxer)
    {
        var cami = Path.Combine(ArrelDelRepositori.Trobar(), "src", "Web", fitxer);
        Assert.True(File.Exists(cami), $"Falta {fitxer}");

        using var doc = JsonDocument.Parse(File.ReadAllText(cami),
            new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });

        var infraccions = new List<string>();
        foreach (var clau in ClausSensibles)
        {
            var node = doc.RootElement;
            var trobat = true;
            foreach (var part in clau.Split(':'))
            {
                if (node.ValueKind != JsonValueKind.Object || !node.TryGetProperty(part, out node))
                {
                    trobat = false;
                    break;
                }
            }
            if (trobat && node.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(node.GetString()))
                infraccions.Add(clau);
        }

        Assert.True(infraccions.Count == 0,
            $"{fitxer} porta valors sensibles (regla 3 d'AGENTS.md): {string.Join(", ", infraccions)}");
    }

    private static IEnumerable<string> FitxersDeCodi(string arrel) =>
        Directory.EnumerateFiles(arrel, "*.cs", SearchOption.AllDirectories)
            .Where(f =>
            {
                var relatiu = Path.GetRelativePath(arrel, f).Replace('\\', '/');
                return !relatiu.StartsWith("bin/") && !relatiu.StartsWith("obj/")
                    && !relatiu.StartsWith("tests/") && !relatiu.StartsWith(".claude/")
                    && !relatiu.Contains("/bin/") && !relatiu.Contains("/obj/");
            });
}
