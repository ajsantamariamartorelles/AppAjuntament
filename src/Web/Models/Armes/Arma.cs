using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppAjuntament.Models.Base.Ens;
using AppAjuntament.Models.Base.Usuari;
using AppAjuntament.Models.Tercers;

namespace AppAjuntament.Models.Armes;

[Table("ARMES_armes")]
public class Arma
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string NumSerie { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Marca { get; set; }

    [MaxLength(100)]
    public string? Model { get; set; }

    [MaxLength(50)]
    public string? Calibre { get; set; }

    public int TipusArmaId { get; set; }
    public int? TercerId { get; set; }
    public int EnsId { get; set; }

    public DateTime? DataAlta { get; set; }
    public DateTime? DataBaixa { get; set; }

    [MaxLength(500)]
    public string? MotiuBaixa { get; set; }

    public string? Observacions { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("updated_by")]
    public int? UpdatedBy { get; set; }

    public virtual AuxTipusArma? TipusArma { get; set; }
    public virtual Tercer? Tercer { get; set; }
    public virtual Ens? Ens { get; set; }
    public virtual Usuari? CreatedByUser { get; set; }
    public virtual Usuari? UpdatedByUser { get; set; }

    [NotMapped]
    public bool Activa => !DataBaixa.HasValue;
}
