using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Base.Contacte
{
    [Table("SUBV_contactes")]
    public class Contacte
    {
        public int Id { get; set; }
        public string? Adreca { get; set; }
        public string? CodiPostal { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public string? Web { get; set; }
        public string? TelefonContacte { get; set; }
        public string? Fax { get; set; }
    }
}
