using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models;

/// <summary>
/// Estats de conservació del patrimoni
/// </summary>
[Table("PATR_estats_conservacio")]
public class EstatConservacio
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("nom")]
    public string Nom { get; set; } = string.Empty;

    [Column("descripcio")]
    public string? Descripcio { get; set; }

    [MaxLength(7)]
    [Column("color")]
    public string Color { get; set; } = "#28A745";

    [Column("prioritat_intervencio")]
    public int PrioritatIntervencio { get; set; } = 1;

    [Column("actiu")]
    public bool Actiu { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navegació
    public virtual ICollection<Patrimoni> Patrimonis { get; set; } = new List<Patrimoni>();
}
