using System.ComponentModel.DataAnnotations.Schema;
using AppAjuntament.Models.Base.Usuari;

namespace AppAjuntament.Models.Armes;

[Table("ARMES_inspeccions")]
public class ArmaInspeccio
{
    public int Id { get; set; }
    public int ArmaId { get; set; }
    public DateTime Data { get; set; }
    public int ResultatId { get; set; }

    public string? Observacions { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("updated_by")]
    public int? UpdatedBy { get; set; }

    public virtual Arma? Arma { get; set; }
    public virtual AuxResultatInspeccio? Resultat { get; set; }
    public virtual Usuari? CreatedByUser { get; set; }
    public virtual Usuari? UpdatedByUser { get; set; }
}
