using System.Text.RegularExpressions;
using AppAjuntament.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace AppAjuntament.Tests;

/// <summary>
/// Regla 6 d'AGENTS.md: l'estructura de la BD viatja amb el model i viu només a db/schema.sql.
/// Aquest test falla si el model d'EF té una taula o una columna que schema.sql no defineix
/// (el cas típic: s'afegeix una propietat a una entitat i s'oblida d'actualitzar el schema).
/// </summary>
public class ParitatSchemaBdTests : IDisposable
{
    private readonly BdProves _bd = new();

    public void Dispose() => _bd.Dispose();

    private static readonly Regex Taula = new(
        @"CREATE TABLE (?:IF NOT EXISTS )?`(?<nom>[^`]+)` \((?<cos>.*?)\n\) ENGINE",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex Columna = new(@"^\s{2}`(?<nom>[^`]+)`\s", RegexOptions.Multiline | RegexOptions.Compiled);

    private static Dictionary<string, HashSet<string>> LlegirSchema()
    {
        var cami = Path.Combine(ArrelDelRepositori.Trobar(), "db", "schema.sql");
        var text = File.ReadAllText(cami);

        var taules = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (Match t in Taula.Matches(text))
        {
            var columnes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match c in Columna.Matches(t.Groups["cos"].Value))
                columnes.Add(c.Groups["nom"].Value);
            taules[t.Groups["nom"].Value] = columnes;
        }
        return taules;
    }

    [Fact]
    public void Totes_les_taules_i_columnes_del_model_son_a_db_schema_sql()
    {
        var schema = LlegirSchema();
        using var context = _bd.NouContext();

        var problemes = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var entitat in context.Model.GetEntityTypes())
        {
            var taula = entitat.GetTableName();
            if (taula is null || entitat.GetViewName() is not null) continue;

            if (!schema.TryGetValue(taula, out var columnes))
            {
                problemes.Add($"Falta la taula `{taula}`");
                continue;
            }

            var objecte = StoreObjectIdentifier.Table(taula, entitat.GetSchema());
            foreach (var propietat in entitat.GetProperties())
            {
                var columna = propietat.GetColumnName(objecte);
                if (columna is not null && !columnes.Contains(columna))
                    problemes.Add($"Falta la columna `{taula}`.`{columna}`");
            }
        }

        Assert.True(problemes.Count == 0,
            "db/schema.sql no està al dia amb el model (AGENTS.md, regla 6):\n" + string.Join("\n", problemes));
    }
}
