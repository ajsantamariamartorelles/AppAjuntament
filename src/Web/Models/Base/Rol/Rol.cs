using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Base.Rol
{
    [Table("rols")]
    public class Rol
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
    }
}
