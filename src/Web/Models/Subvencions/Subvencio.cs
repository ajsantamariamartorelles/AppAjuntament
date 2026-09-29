using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Subvencions
{
    [Table("SUBV_subvencions")]
    public class Subvencio_
    {
        public int Id { get; set; }
        public string? Subvencio { get; set; }
        public string? Actuacio { get; set; }
        public decimal? ImportAtorgat { get; set; }
        public decimal? ImportTotalActuacio { get; set; }
        public string? Obligacions { get; set; }

        // Extra fields
        public string? PersonaContacte { get; set; }
        public string? Justificacio { get; set; }
        public string? Partida { get; set; }
        public string? JustificacioVerificada { get; set; }
        public decimal? ImportsRetornar { get; set; }
        public string? FontResponsable { get; set; }

        // FK fields
        public int? EntitatId { get; set; }
        public int? AnyId { get; set; }
        public int? AreaId { get; set; }
        public int? EstatId { get; set; }
        public int? EnsId { get; set; }

        // Navigation properties
        public virtual AppAjuntament.Models.Base.Entitat.Entitat? Entitat { get; set; }
        public virtual AppAjuntament.Models.Base.Comarca.Any_? Any { get; set; }
        public virtual AppAjuntament.Models.Base.Comarca.Area? Area { get; set; }
        public virtual AppAjuntament.Models.Base.Estat.Estat? Estat { get; set; }
        public virtual AppAjuntament.Models.Base.Ens.Ens? Ens { get; set; }
        public virtual ICollection<RegidorsSubvencions> RegidorsSubvencions { get; set; } = new List<RegidorsSubvencions>();
        public virtual ICollection<Expedient> Expedients { get; set; } = new List<Expedient>();
        public virtual ICollection<Pagament> Pagaments { get; set; } = new List<Pagament>();
        public virtual ICollection<TerminisSubvencio> TerminisSubvencions { get; set; } = new List<TerminisSubvencio>();
    }
}
