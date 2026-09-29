using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace AppAjuntament.Controllers.Cursets
{
    /// <summary>Helpers per llegir el subjecte i el rol del token Bearer de Cursets.</summary>
    public static class CursetsClaimsHelper
    {
        public const string RolProfessora = "Professora";
        public const string RolControlador = "Controlador";

        /// <summary>Id de la professora (usuari) autenticada. Vàlid quan el rol és "Professora".</summary>
        public static bool TryGetProfessoraId(this ControllerBase controller, out int professoraId)
        {
            var valor = controller.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(valor, out professoraId);
        }

        /// <summary>Id del regidor quan qui s'ha autenticat és un controlador.</summary>
        public static bool TryGetRegidorId(this ControllerBase controller, out int regidorId)
        {
            regidorId = 0;
            if (!controller.User.IsInRole(RolControlador))
                return false;

            var valor = controller.User.FindFirst("regidorId")?.Value
                ?? controller.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            return int.TryParse(valor, out regidorId);
        }

        public static bool EsControlador(this ControllerBase controller)
            => controller.User.IsInRole(RolControlador);
    }
}
