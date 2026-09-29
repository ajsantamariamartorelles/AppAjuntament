using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Armes;

[Table("Aux_TipusArma")]
public class AuxTipusArma
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    public string? Descripcio { get; set; }

    public bool Actiu { get; set; } = true;
}
