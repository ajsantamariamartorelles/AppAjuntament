using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Armes;

[Table("Aux_EstatLicencia")]
public class AuxEstatLicencia
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    public bool Actiu { get; set; } = true;
}
