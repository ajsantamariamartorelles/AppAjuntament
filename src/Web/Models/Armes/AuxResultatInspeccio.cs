using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Armes;

[Table("Aux_ResultatInspeccio")]
public class AuxResultatInspeccio
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    public bool Actiu { get; set; } = true;
}
