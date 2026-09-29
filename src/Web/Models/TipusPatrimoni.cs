
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AppAjuntament.Models;

/// <summary>
/// Tipus de patrimoni (Religiós, Civil, Popular, etc.)
/// </summary>
[Table("PATR_tipus_patrimoni")]
public class TipusPatrimoni
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("nom")]
    public string Nom { get; set; } = string.Empty;

    [Column("descripcio")]
    public string? Descripcio { get; set; }

    [MaxLength(7)]
    [Column("color")]
    public string Color { get; set; } = "#007BFF";

    [MaxLength(50)]
    [Column("icona")]
    public string Icona { get; set; } = "fa-building";

    [Column("actiu")]
    public bool Actiu { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navegació
    [JsonIgnore]
    public virtual ICollection<Patrimoni> Patrimonis { get; set; } = new List<Patrimoni>();
}
