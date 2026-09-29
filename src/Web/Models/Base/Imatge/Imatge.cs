using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Base.Imatge
{
    [Table("imatges")]
    public class Imatge
    {
        public int Id { get; set; }
        public string? EscutURL { get; set; }
        public string? BanderaURL { get; set; }
        public string? VistaURL { get; set; }
    }
}
