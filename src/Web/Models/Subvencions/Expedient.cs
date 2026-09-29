using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Subvencions
{
    [Table("SUBV_expedients")]
    public class Expedient
    {
        public int Id { get; set; }
        public int SubvencioId { get; set; }
        public string? NumeroExpedient { get; set; } // Format típic: 9999/AAAA (màx 20 caràcters)

        // Navegació
        public virtual Subvencio_? Subvencio { get; set; }
    }
}
