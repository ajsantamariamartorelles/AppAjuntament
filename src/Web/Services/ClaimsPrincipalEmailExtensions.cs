using System.Security.Claims;

namespace AppAjuntament.Services
{
    /// <summary>Extreu el correu d'un usuari autenticat via Azure AD (cookie del Web).</summary>
    public static class ClaimsPrincipalEmailExtensions
    {
        /// <summary>
        /// L'UPN d'Azure AD sol diferir del correu corporatiu segons com s'hagi
        /// configurat el tenant; es proven diversos claims candidats, com fa
        /// CursetsAuthController per al login de la MAUI.
        /// </summary>
        public static string? GetEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value
                ?? user.FindFirst("preferred_username")?.Value
                ?? user.FindFirst(ClaimTypes.Upn)?.Value
                ?? user.FindFirst("upn")?.Value
                ?? user.FindFirst("email")?.Value;
        }
    }
}
