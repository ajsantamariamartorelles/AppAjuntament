using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models;

[Table("PATR_localitzacions")]
public class PatrimoniLocalitzacio
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("nom")]
    public string Nom { get; set; } = string.Empty;

    [Column("descripcio")]
    public string? Descripcio { get; set; }

    [Column("latitud")]
    public decimal Latitud { get; set; }

    [Column("longitud")]
    public decimal Longitud { get; set; }

    [Column("actiu")]
    public bool Actiu { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navegació
    public virtual ICollection<ArxiuPatrimoni> Arxius { get; set; } = new List<ArxiuPatrimoni>();
}
