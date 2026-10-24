using System.Reflection;

namespace AppAjuntament.Services;

/// <summary>
/// Versió de l'aplicació que s'està executant: número (que inclou la data i l'hora de la compilació, generat
/// sol a cada publicació) i commit (si el build el porta). Es mostra al desplegable de l'usuari per saber què hi ha publicat.
/// </summary>
public static class InfoVersio
{
    /// <summary>Text curt per a la interfície, p. ex. "v1.0.20261023.1432 (a1b2c3d)".</summary>
    public static string Text { get; } = Calcula();

    private static string Calcula()
    {
        var assembly = typeof(InfoVersio).Assembly;
        var informacional = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                            ?? assembly.GetName().Version?.ToString(3) ?? "?";

        // L'SDK afegeix "+<hash del commit>" a la versió informacional.
        var parts = informacional.Split('+', 2);
        var text = "v" + parts[0];
        if (parts.Length == 2 && parts[1].Length >= 7)
            text += $" ({parts[1][..7]})";

        return text;
    }
}
