using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models;

/// <summary>
/// Subtipus de patrimoni (dependent de TipusPatrimoni)
/// </summary>
[Table("PATR_subtipus_patrimoni")]
public class SubtipusPatrimoni
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("tipus_patrimoni_id")]
    public int TipusPatrimoniId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("nom")]
    public string Nom { get; set; } = string.Empty;

    [Column("descripcio")]
    public string? Descripcio { get; set; }

    [Column("actiu")]
    public bool Actiu { get; set; } = true;

    // Navegació
    [ForeignKey("TipusPatrimoniId")]
    public virtual TipusPatrimoni TipusPatrimoni { get; set; } = null!;
}
