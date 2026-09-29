using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Base.AcceptacioTermes
{
    /// <summary>
    /// Acceptació dels termes i condicions i la política de privacitat.
    /// S'identifica per correu, perquè és l'únic identificador comú entre el
    /// personal municipal del Web (via Azure AD, sense fila pròpia necessàriament
    /// a <see cref="Usuari.Usuari"/>) i les professores/controladors de l'app de
    /// Cursets (<see cref="Usuari.Usuari"/> / <see cref="Regidor.Regidor"/>).
    /// Una fila per correu: s'actualitza (no es duplica) en tornar a acceptar.
    /// </summary>
    [Table("acceptacions_termes")]
    public class AcceptacioTermes
    {
        public int Id { get; set; }

        /// <summary>Correu normalitzat (minúscules, sense espais).</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Versió del text (Termes + Privacitat) que es va acceptar.
        /// Vegeu <see cref="AppAjuntament.Services.IAcceptacioTermesService.VersioActual"/>.</summary>
        public int VersioAcceptada { get; set; }

        public DateTime DataAcceptacio { get; set; } = DateTime.Now;
    }
}
