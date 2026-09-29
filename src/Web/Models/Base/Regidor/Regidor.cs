using System.ComponentModel.DataAnnotations.Schema;

using EnsModel = AppAjuntament.Models.Base.Ens.Ens;
using SexeModel = AppAjuntament.Models.Base.Sexe.Sexe;
using AppAjuntament.Models.Subvencions;

namespace AppAjuntament.Models.Base.Regidor
{
    [Table("regidors")]
    public class Regidor
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Carrec { get; set; }
        public string? Partit { get; set; }
            public string? Sigles { get; set; }
        public string? Area { get; set; }
        public DateTime? DataNomenament { get; set; }
        public string? Email { get; set; }
        public int? Orde { get; set; }
        public string? CodiEns { get; set; }
        public string? NomEns { get; set; }
        public int? EnsId { get; set; }
        public EnsModel? Ens { get; set; }
        public int? SexeId { get; set; }
        public SexeModel? Sexe { get; set; }
        public string? ExternId { get; set; }
        public string? NomComplet { get; set; }
        public string? Cognom1 { get; set; }
        public string? Cognom2 { get; set; }

        /// <summary>
        /// Hash de la contrasenya per entrar a l'app de Cursets com a controlador
        /// (regidor responsable, només lectura). NULL = el regidor no té accés a l'app.
        /// </summary>
        public string? PasswordHash { get; set; }

        public virtual ICollection<RegidorsSubvencions> RegidorsSubvencions { get; set; } = new List<RegidorsSubvencions>();
    }
}
