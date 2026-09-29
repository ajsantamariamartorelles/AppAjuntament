namespace AjuntamentSantaMariaMartorelles.Config;

/// <summary>
/// Dades del registre d'app d'Azure AD (es reutilitza el mateix del web de
/// l'Ajuntament, amb una plataforma "Mobile and desktop" afegida).
/// </summary>
public static class AzureAdConfig
{
    public const string TenantId = "31aa02c9-6fdf-4998-b3ee-7697b1077120";
    public const string ClientId = "8fa27198-fc80-4593-9912-eb98295e73c7";

    public static string Authority => $"https://login.microsoftonline.com/{TenantId}";

    /// <summary>
    /// URI de redirecció que ha d'estar donada d'alta a l'Azure com a
    /// plataforma "Mobile and desktop applications".
    /// Android: msal{ClientId}://auth · iOS: msauth.{bundleId}://auth
    /// </summary>
    public static string RedirectUri =>
        DeviceInfo.Platform == DevicePlatform.iOS
            ? "msauth.cat.santamariademartorelles.mobil://auth"
            : $"msal{ClientId}://auth";

    /// <summary>Scopes de la petició interactiva. "email" fa que l'id_token porti
    /// el claim "email" (correu corporatiu real, no l'UPN …@…onmicrosoft.com).</summary>
    public static readonly string[] Scopes = { "User.Read", "email" };
}
