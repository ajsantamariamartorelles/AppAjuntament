using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Base.Estat
{
    [Table("SUBV_estats")]
    public class Estat
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
    }
}
