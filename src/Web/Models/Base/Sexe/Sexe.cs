using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Base.Sexe
{
    [Table("sexes")]
    public class Sexe
    {
        public int Id { get; set; }
        public char Codi { get; set; }
        public string? Descripcio { get; set; }
    }
}
