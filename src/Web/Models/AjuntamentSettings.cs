namespace AppAjuntament.Models;

/// <summary>
/// Configuració de l'Ajuntament per a l'aplicació
/// </summary>
public class AjuntamentSettings
{
    public const string SectionName = "Ajuntament";

    /// <summary>Codi INE6 del municipi (ex: 082567)</summary>
    public string CodiINE6 { get; set; } = string.Empty;
    /// <summary>Codi Diba del municipi (ex: 08256)</summary>
    public string CodiDiba { get; set; } = string.Empty;

    /// <summary>Codi d'ens de la Seu-e (ex: 825670005).</summary>
    public string CodiEns { get; set; } = string.Empty;

    /// <summary>Nom oficial del municipi (ex: Santa Maria de Martorelles)</summary>
    public string NomMunicipi { get; set; } = string.Empty;

    /// <summary>Nom oficial del municipi (ex: Ajuntament de Santa Maria de Martorelles)</summary>
    public string NomInstitucio { get; set; } = string.Empty;

    /// <summary>Comarca del municipi (ex: Vallès Oriental). Només per al perfil provisional de la pàgina d'Inici.</summary>
    public string Comarca { get; set; } = string.Empty;

    /// <summary>Província del municipi (ex: Barcelona). Només per al perfil provisional de la pàgina d'Inici.</summary>
    public string Provincia { get; set; } = string.Empty;
}
