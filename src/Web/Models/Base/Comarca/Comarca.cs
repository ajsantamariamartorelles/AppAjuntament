using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Base.Comarca
{
    [Table("comarques")]
    public class Comarca
    {
        [Key]
        public int Codi { get; set; }  // INT com a clau primària per coincidir amb la base de dades real
        public required string Nom { get; set; }
        public string? Coordenades { get; set; }
        public string? NomDBPedia { get; set; }
        public string? CCAdrecaCompleta { get; set; }
        public string? CCAdreca { get; set; }
        public string? CCCodiPostal { get; set; }
        public string? CCEmail { get; set; }
        public string? CCFax { get; set; }
        public string? CCWeb { get; set; }
    }
}
