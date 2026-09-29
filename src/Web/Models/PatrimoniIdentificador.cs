using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models;

/// <summary>
/// Identificadors externs per a un patrimoni (Diputació, Generalitat, etc.)
/// </summary>
[Table("PatrimoniIdentificador")]
public class PatrimoniIdentificador
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("patrimoni_id")]
    public int PatrimoniId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("tipus")]
    public string Tipus { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("valor")]
    public string Valor { get; set; } = string.Empty;

    [ForeignKey("PatrimoniId")]
    public virtual Patrimoni Patrimoni { get; set; } = null!;
}
