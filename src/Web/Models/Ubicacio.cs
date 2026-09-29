using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ComarcaModel = AppAjuntament.Models.Base.Comarca.Comarca;

namespace AppAjuntament.Models;

/// <summary>
/// Ubicacions administratives (districtes, barris, zones)
/// </summary>
[Table("PATR_ubicacions")]
public class Ubicacio
{
    [Column("comarca_id")]
    public int? ComarcaId { get; set; }
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("nom")]
    public string Nom { get; set; } = string.Empty;

    [Column("tipus")]
    public TipusUbicacio Tipus { get; set; } = TipusUbicacio.Barri;

    [Column("parent_id")]
    public int? ParentId { get; set; }

    [Column("actiu")]
    public bool Actiu { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navegació
    [ForeignKey("ParentId")]
    public virtual Ubicacio? Parent { get; set; }

    public virtual ICollection<Ubicacio> Children { get; set; } = new List<Ubicacio>();
    public virtual ICollection<Patrimoni> Patrimonis { get; set; } = new List<Patrimoni>();

    [ForeignKey("ComarcaId")]
    public virtual ComarcaModel? Comarca { get; set; }
}

/// <summary>
/// Tipus d'ubicació
/// </summary>
public enum TipusUbicacio
{
    Districte,
    Barri,
    Zona,
    Altra
}
