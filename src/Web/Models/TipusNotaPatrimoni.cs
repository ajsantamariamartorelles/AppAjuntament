using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models;

/// <summary>
/// Catàleg de tipus de nota de patrimoni
/// </summary>
[Table("PATR_tipus_notes")]
public class TipusNotaPatrimoni
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("nom")]
    public string Nom { get; set; } = string.Empty;

    [Column("actiu")]
    public bool Actiu { get; set; } = true;

    public virtual ICollection<NotaPatrimoni> Notes { get; set; } = new List<NotaPatrimoni>();
}