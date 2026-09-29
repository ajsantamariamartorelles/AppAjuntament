using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Base.Entitat
{
    [Table("entitats")]
    public class Entitat
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? LogoURL { get; set; }
        public string? ColorCorporatiu { get; set; }
        public bool PermetAutenticacioExterna { get; set; }
    }
}
