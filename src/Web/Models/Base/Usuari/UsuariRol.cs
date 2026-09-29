using System.ComponentModel.DataAnnotations.Schema;
using AppAjuntament.Models.Base.Usuari;
using AppAjuntament.Models.Base.Rol;

namespace AppAjuntament.Models.Base.Usuari
{
    [Table("usuarisrols")]
    public class UsuariRol
    {
        public int UsuariId { get; set; }
        public Usuari? Usuari { get; set; }
        public int RolId { get; set; }
        public AppAjuntament.Models.Base.Rol.Rol? Rol { get; set; }
    }
}
