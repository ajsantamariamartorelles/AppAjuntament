using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Subvencions
{
    [Table("SUBV_terminis_subvencions")]
    public class TerminisSubvencio
    {
        public int Id { get; set; }

        public int SubvencioId { get; set; }
        public virtual Subvencio_? Subvencio { get; set; }

        public int TerminiId { get; set; }
        public virtual Termini? Termini { get; set; }

        public DateTime? DataInici { get; set; }
        public DateTime? DataFi { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
