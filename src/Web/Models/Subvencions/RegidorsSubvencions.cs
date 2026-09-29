using System.ComponentModel.DataAnnotations.Schema;
using AppAjuntament.Models.Base.Regidor;

namespace AppAjuntament.Models.Subvencions
{
    [Table("SUBV_regidorssubvencions")]
    public class RegidorsSubvencions
    {
        public int Id { get; set; }
        public int SubvencioId { get; set; }
        public Subvencio_? Subvencio { get; set; }
        public int RegidorId { get; set; }
        public Regidor? Regidor { get; set; }
    }
}
