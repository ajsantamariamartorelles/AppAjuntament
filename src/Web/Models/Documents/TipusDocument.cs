using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppAjuntament.Models.Base.Usuari;

namespace AppAjuntament.Models.Documents;

/// <summary>
/// Tipus de document auxiliar (DNI, Llicència, Acord de voluntariat, ...).
/// Mapeig: Aux_TipusDocument
/// </summary>
[Table("Aux_TipusDocument")]
public class TipusDocument
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcio { get; set; }

    public bool Actiu { get; set; } = true;
}
