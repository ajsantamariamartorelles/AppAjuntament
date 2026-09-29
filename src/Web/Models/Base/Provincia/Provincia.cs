using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Base.Provincia
{
    [Table("provincies")]
    public class Provincia
    {
        [Key]
        [MaxLength(10)]
        public string Id { get; set; } = string.Empty;
        public string? Nom { get; set; }
    }
}
