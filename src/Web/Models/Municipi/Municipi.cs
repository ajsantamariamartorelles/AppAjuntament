using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppAjuntament.Models.Base.Comarca;

namespace AppAjuntament.Models.Municipi
{
    [Table("municipis")]
    public class Municipi
    {
        [Key]
        [Column("ine")]
        [MaxLength(10)]
        public string Ine { get; set; } = string.Empty;

        [Column("municipi_nom")]
        [MaxLength(255)]
        public string MunicipiNom { get; set; } = string.Empty;

        [Column("municipi_nom_curt")]
        [MaxLength(255)]
        public string? MunicipiNomCurt { get; set; }

        [Column("municipi_article")]
        [MaxLength(50)]
        public string? MunicipiArticle { get; set; }

        [Column("municipi_transliterat")]
        [MaxLength(255)]
        public string? MunicipiTransliterat { get; set; }

        [Column("municipi_curt_transliterat")]
        [MaxLength(255)]
        public string? MunicipiCurtTransliterat { get; set; }

        [Column("centre_municipal")]
        [MaxLength(50)]
        public string? CentreMunicipal { get; set; }

        [Column("comarca_codi")]
        public int? ComarcaCodi { get; set; }

        [ForeignKey("ComarcaCodi")]
        public Comarca? Comarca { get; set; }

        [Column("provincia_codi")]
        [MaxLength(10)]
        public string? ProvinciaCodi { get; set; }

        //[ForeignKey("ProvinciaCodi")]
        //public Provincia? Provincia { get; set; }

        [Column("adreca_completa")]
        [MaxLength(255)]
        public string? AdrecaCompleta { get; set; }

        [Column("adreca")]
        [MaxLength(255)]
        public string? Adreca { get; set; }

        [Column("codi_postal")]
        [MaxLength(10)]
        public string? CodiPostal { get; set; }

        [Column("localitzacio")]
        [MaxLength(50)]
        public string? Localitzacio { get; set; }

        [Column("telefon_contacte")]
        [MaxLength(20)]
        public string? TelefonContacte { get; set; }

        [Column("fax")]
        [MaxLength(20)]
        public string? Fax { get; set; }

        [Column("email")]
        [MaxLength(255)]
        public string? Email { get; set; }

        [Column("url_general")]
        [MaxLength(255)]
        public string? UrlGeneral { get; set; }

        [Column("cif")]
        [MaxLength(20)]
        public string? CIF { get; set; }

        [Column("municipi_escut")]
        [MaxLength(255)]
        public string? MunicipiEscut { get; set; }

        [Column("municipi_bandera")]
        [MaxLength(255)]
        public string? MunicipiBandera { get; set; }

        [Column("municipi_vista")]
        [MaxLength(255)]
        public string? MunicipiVista { get; set; }

        [Column("ine6")]
        [MaxLength(10)]
        public string? Ine6 { get; set; }

        [Column("nom_dbpedia")]
        [MaxLength(255)]
        public string? NomDBPedia { get; set; }

        [Column("nombre_habitants")]
        public int? NombreHabitants { get; set; }

        [Column("extensio")]
        public decimal? Extensio { get; set; }

        [Column("altitud")]
        public int? Altitud { get; set; }
    }
}
