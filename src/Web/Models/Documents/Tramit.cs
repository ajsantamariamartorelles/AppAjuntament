using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Documents;

/// <summary>
/// Tràmit funcional per configurar quins tipus de document són permesos.
/// Mapeig: Aux_Tramit
/// </summary>
[Table("Aux_Tramit")]
public class Tramit
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Codi { get; set; } = string.Empty;

    [Required, MaxLength(191)]
    public string Nom { get; set; } = string.Empty;

    public string? Descripcio { get; set; }

    public bool Actiu { get; set; } = true;
}
