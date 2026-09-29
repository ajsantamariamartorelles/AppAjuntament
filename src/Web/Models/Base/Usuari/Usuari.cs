using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Base.Usuari
{
    [Table("usuaris")]
    public class Usuari
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Cognoms { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public bool Actiu { get; set; }

        // FK & Navigation
        public int? EntitatId { get; set; }
        public virtual AppAjuntament.Models.Base.Entitat.Entitat? Entitat { get; set; }
    }
}
