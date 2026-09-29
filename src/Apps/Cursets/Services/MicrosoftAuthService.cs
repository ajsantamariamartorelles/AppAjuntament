using AjuntamentSantaMariaMartorelles.Config;
using Microsoft.Identity.Client;

namespace AjuntamentSantaMariaMartorelles.Services;

/// <summary>
/// Login amb el compte de Microsoft (Azure AD) de l'Ajuntament, per a personal i
/// regidors. Obté l'id_token d'AAD; qui el bescanvia per un token de l'app és
/// <see cref="AuthService.LoginMicrosoftAsync"/>.
/// </summary>
public class MicrosoftAuthService
{
	private readonly IPublicClientApplication _pca;

	public MicrosoftAuthService()
	{
		var builder = PublicClientApplicationBuilder
			.Create(AzureAdConfig.ClientId)
			.WithAuthority(AzureAdConfig.Authority)
			.WithRedirectUri(AzureAdConfig.RedirectUri);

#if ANDROID
		builder = builder.WithParentActivityOrWindow(() => Platform.CurrentActivity);
#endif
#if IOS
		builder = builder.WithIosKeychainSecurityGroup("com.microsoft.adalcache");
#endif

		_pca = builder.Build();
	}

	/// <summary>
	/// Retorna l'id_token d'Azure AD. Prova primer silenciosament (sessió guardada)
	/// i, si cal, obre el navegador del sistema per identificar-se.
	/// </summary>
	public async Task<string?> AcquireIdTokenAsync()
	{
		AuthenticationResult result;

		var accounts = await _pca.GetAccountsAsync();
		var compte = accounts.FirstOrDefault();

		try
		{
			result = await _pca.AcquireTokenSilent(AzureAdConfig.Scopes, compte).ExecuteAsync();
		}
		catch (MsalUiRequiredException)
		{
			// Navegador del sistema (recomanat per Microsoft: SSO + Conditional Access).
			result = await _pca.AcquireTokenInteractive(AzureAdConfig.Scopes)
				.WithPrompt(Prompt.SelectAccount)
				.ExecuteAsync();
		}

		return result?.IdToken;
	}

	/// <summary>Esborra la sessió d'AAD guardada al dispositiu.</summary>
	public async Task SignOutAsync()
	{
		foreach (var compte in await _pca.GetAccountsAsync())
			await _pca.RemoveAsync(compte);
	}
}
