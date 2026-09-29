using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models;

[Table("PATR_coleccions")]
public class PatrimoniColeccio
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

    [MaxLength(200)]
    [Column("autor")]
    public string? Autor { get; set; }

    [Column("any_inici")]
    public short? AnyInici { get; set; }

    [Column("any_fi")]
    public short? AnyFi { get; set; }

    [Column("actiu")]
    public bool Actiu { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navegació
    public virtual ICollection<ArxiuPatrimoni> Arxius { get; set; } = new List<ArxiuPatrimoni>();

    [NotMapped]
    public string PeriodeFormatat => AnyInici.HasValue && AnyFi.HasValue
        ? $"{AnyInici}–{AnyFi}"
        : AnyInici.HasValue ? $"Des de {AnyInici}" : string.Empty;
}
