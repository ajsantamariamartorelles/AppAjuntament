using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models;

/// <summary>
/// Notes associades a un element de patrimoni
/// </summary>
[Table("PATR_notes_patrimoni")]
public class NotaPatrimoni
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("patrimoni_id")]
    public int PatrimoniId { get; set; }

    [Column("tipus_nota_id")]
    public int TipusNotaId { get; set; }

    [MaxLength(200)]
    [Column("titol")]
    public string? Titol { get; set; }

    [Required]
    [Column("contingut")]
    public string Contingut { get; set; } = string.Empty;

    [Column("visible_public")]
    public bool VisiblePublic { get; set; } = false;

    [Column("actiu")]
    public bool Actiu { get; set; } = true;

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey("PatrimoniId")]
    public virtual Patrimoni Patrimoni { get; set; } = null!;

    [ForeignKey("TipusNotaId")]
    public virtual TipusNotaPatrimoni? TipusNota { get; set; }
}