using System.ComponentModel.DataAnnotations.Schema;

using ComarcaModel = AppAjuntament.Models.Base.Comarca.Comarca;
using ProvinciaModel = AppAjuntament.Models.Base.Provincia.Provincia;
using ContacteModel = AppAjuntament.Models.Base.Contacte.Contacte;
using ImatgeModel = AppAjuntament.Models.Base.Imatge.Imatge;
using DadesGeografiquesModel = AppAjuntament.Models.Base.Geo.DadesGeografiques;

namespace AppAjuntament.Models.Base.Ens
{
    [Table("ens")]
    public class Ens
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? CodiEns { get; set; }
        public string? CIF { get; set; }
        public string? NomCurt { get; set; }
        public string? Article { get; set; }
        public string? Transliterat { get; set; }
        public string? CurtTransliterat { get; set; }
        public string? AdrecaCompleta { get; set; }
        public string? INE6 { get; set; }
        public string? NomDBPedia { get; set; }
        public int? ComarcaId { get; set; }  // INT per coincidir amb la base de dades real
        public ComarcaModel? Comarca { get; set; }
        public string? ProvinciaId { get; set; }
        public ProvinciaModel? Provincia { get; set; }
        public int? ContacteId { get; set; }
        public ContacteModel? Contacte { get; set; }
        public int? ImatgeId { get; set; }
        public ImatgeModel? Imatge { get; set; }
        public int? DadesGeografiquesId { get; set; }
        public DadesGeografiquesModel? DadesGeografiques { get; set; }
    }
}
