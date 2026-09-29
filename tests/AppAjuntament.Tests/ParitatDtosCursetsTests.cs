using System.Text.RegularExpressions;
using AppAjuntament.Tests.Infraestructura;
using Xunit;

namespace AppAjuntament.Tests;

/// <summary>
/// L'app MAUI de Cursets no comparteix codi amb el Web (encara no hi ha
/// src/Compartit, vegeu AGENTS.md): té les seves pròpies classes de dades a
/// src/Apps/Cursets/Models, mantingudes a mà com a "mirall" dels DTO del Web
/// (src/Web/Models/Cursets/Dto). Aquest test vigila que no es desincronitzin:
/// per cada propietat de dades de la MAUI, si n'hi ha una del mateix nom al
/// DTO homònim del Web, el tipus ha de coincidir. No exigeix el contrari
/// (que el Web tingui totes les propietats de la MAUI, ni a l'inrevés): la
/// MAUI sovint només fa servir un subconjunt de camps.
/// </summary>
public class ParitatDtosCursetsTests
{
    private static readonly (string Web, string Maui)[] ParellsDeFitxers =
    {
        ("src/Web/Models/Cursets/Dto/CursetsDtos.cs", "src/Apps/Cursets/Models/CursetsModels.cs"),
        ("src/Web/Models/Cursets/Dto/SeguimentDtos.cs", "src/Apps/Cursets/Models/SeguimentModels.cs"),
    };

    /// <summary>
    /// Propietats que la MAUI afegeix expressament per damunt del contracte del Web
    /// (dades calculades pel client, no rebudes per JSON). (Classe, Propietat).
    /// </summary>
    private static readonly HashSet<(string Classe, string Propietat)> NomesMaui = new()
    {
        // Resum de seguiment que el client omple amb una crida a part, no ve amb el curset.
        ("CursetDto", "Seguiment"),
    };

    [Fact]
    public void Els_tipus_de_dades_de_la_MAUI_coincideixen_amb_els_del_Web()
    {
        var arrel = ArrelDelRepositori.Trobar();
        var discrepancies = new List<string>();

        foreach (var (rutaWeb, rutaMaui) in ParellsDeFitxers)
        {
            var classesWeb = LlegeixClasses(Path.Combine(arrel, rutaWeb));
            var classesMaui = LlegeixClasses(Path.Combine(arrel, rutaMaui));

            foreach (var (nomClasse, propietatsMaui) in classesMaui)
            {
                if (!classesWeb.TryGetValue(nomClasse, out var propietatsWeb))
                    continue; // classe que només existeix a la MAUI (p. ex. tipus auxiliars)

                foreach (var (nomPropietat, tipusMaui) in propietatsMaui)
                {
                    if (NomesMaui.Contains((nomClasse, nomPropietat))) continue;

                    if (!propietatsWeb.TryGetValue(nomPropietat, out var tipusWeb))
                    {
                        discrepancies.Add(
                            $"{rutaMaui}: {nomClasse}.{nomPropietat} no existeix a {nomClasse} del Web " +
                            "(si és una propietat nova només de la MAUI, afegeix-la a NomesMaui amb el motiu)");
                        continue;
                    }

                    if (!TipusEquivalents(tipusMaui, tipusWeb))
                        discrepancies.Add(
                            $"{rutaMaui}: {nomClasse}.{nomPropietat} és '{tipusMaui}' " +
                            $"però al Web és '{tipusWeb}'");
                }
            }
        }

        Assert.True(discrepancies.Count == 0,
            "Els DTO de la MAUI s'han desincronitzat dels del Web:\n" + string.Join("\n", discrepancies));
    }

    private static bool TipusEquivalents(string a, string b) =>
        Normalitza(a) == Normalitza(b);

    private static string Normalitza(string tipus) =>
        tipus.Replace(" ", "").Replace("global::", "");

    private static readonly Regex ClasseRegex = new(@"public\s+class\s+(\w+)\b[^{]*\{", RegexOptions.Compiled);

    private static readonly Regex PropietatRegex = new(
        @"public\s+([\w<>\?\.\[\],\s]+?)\s+(\w+)\s*\{\s*get;\s*(?:private\s+)?set;\s*\}",
        RegexOptions.Compiled);

    /// <summary>Nom de classe -> (nom de propietat -> tipus), només propietats automàtiques
    /// ("{ get; set; }"); s'ignoren les calculades ("=> ...") i les marcades [JsonIgnore].</summary>
    private static Dictionary<string, Dictionary<string, string>> LlegeixClasses(string fitxer)
    {
        var text = File.ReadAllText(fitxer);
        var resultat = new Dictionary<string, Dictionary<string, string>>();

        foreach (Match m in ClasseRegex.Matches(text))
        {
            var nomClasse = m.Groups[1].Value;
            var inici = m.Index + m.Length;
            var profunditat = 1;
            var i = inici;
            while (i < text.Length && profunditat > 0)
            {
                if (text[i] == '{') profunditat++;
                else if (text[i] == '}') profunditat--;
                i++;
            }
            var cos = text.Substring(inici, Math.Max(0, i - inici - 1));

            var propietats = new Dictionary<string, string>();
            foreach (Match p in PropietatRegex.Matches(cos))
            {
                var abans = cos.Substring(Math.Max(0, p.Index - 40), Math.Min(40, p.Index));
                if (abans.Contains("JsonIgnore")) continue;

                propietats[p.Groups[2].Value] = p.Groups[1].Value.Trim();
            }
            resultat[nomClasse] = propietats;
        }
        return resultat;
    }
}
