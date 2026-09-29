using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppAjuntament.Models.Base.Usuari;
using AppAjuntament.Models.Tercers;

namespace AppAjuntament.Models.Armes;

[Table("ARMES_licencies")]
public class ArmaLlicencia
{
    public int Id { get; set; }
    public int ArmaId { get; set; }
    public int TercerId { get; set; }
    public int TipusLicenciaId { get; set; }
    public int EstatLicenciaId { get; set; }

    [MaxLength(100)]
    public string? NumLicencia { get; set; }

    public DateTime? DataExpedicio { get; set; }
    public DateTime? DataCaducitat { get; set; }

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
    public virtual Tercer? Tercer { get; set; }
    public virtual AuxTipusLicencia? TipusLicencia { get; set; }
    public virtual AuxEstatLicencia? EstatLicencia { get; set; }
    public virtual Usuari? CreatedByUser { get; set; }
    public virtual Usuari? UpdatedByUser { get; set; }
}
