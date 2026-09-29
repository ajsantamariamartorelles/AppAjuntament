namespace AppAjuntament.Tests.Infraestructura;

/// <summary>Troba l'arrel del repositori des del directori de sortida dels tests.</summary>
public static class ArrelDelRepositori
{
    /// <summary>
    /// AGENTS.md és a l'arrel i no es mou (a diferència de l'aplicació web, que viu
    /// a src/Web): és la referència per pujar des del directori de sortida dels tests.
    /// </summary>
    public static string Trobar()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "AGENTS.md")))
            dir = dir.Parent;
        return dir?.FullName
               ?? throw new InvalidOperationException("No s'ha trobat AGENTS.md pujant des del directori de tests.");
    }
}
