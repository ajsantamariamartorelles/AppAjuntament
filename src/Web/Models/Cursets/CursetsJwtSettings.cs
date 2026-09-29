namespace AppAjuntament.Models.Cursets
{
    /// <summary>
    /// Configuració del JWT Bearer exclusiu per a l'API de Cursets (app MAUI).
    /// Aquest esquema és totalment independent del Cookie+OIDC (Azure AD) que fa
    /// servir la web: només l'accepten els controllers de Controllers/Cursets.
    /// </summary>
    public class CursetsJwtSettings
    {
        public const string SectionName = "CursetsJwt";

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;

        /// <summary>Clau simètrica per signar els tokens. Mínim 32 caràcters. No es commiteja mai amb valor real.</summary>
        public string SigningKey { get; set; } = string.Empty;

        /// <summary>Durada del token. L'app MAUI no té refresh token: es vol una sessió
        /// llarga (com WhatsApp) i es re-demana login només quan caduca. 43200 min = 30 dies.</summary>
        public int ExpirationMinutes { get; set; } = 43200;
    }
}
