using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Subvencions
{
    [Table("SUBV_terminis")]
    public class Termini
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public required string Nom { get; set; } // "Execució", "Justificació", etc.

        [MaxLength(500)]
        public string? Descripcio { get; set; }

        public bool Actiu { get; set; } = true;

        // Relacions
        public virtual ICollection<TerminisSubvencio> TerminisSubvencions { get; set; } = new List<TerminisSubvencio>();
    }
}
