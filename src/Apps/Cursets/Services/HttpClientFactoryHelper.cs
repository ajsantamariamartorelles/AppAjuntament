namespace AjuntamentSantaMariaMartorelles.Services;

/// <summary>
/// Crea el <see cref="HttpMessageHandler"/> per a les crides a l'API.
///
/// NOMÉS EN DEBUG accepta qualsevol certificat de servidor: en local
/// AppAjuntament es serveix amb el certificat de desenvolupament d'ASP.NET
/// (CN=localhost), que ni l'emulador Android ni el simulador iOS tenen com a
/// arrel de confiança. En RELEASE es fa servir la validació estàndard.
/// </summary>
public static class HttpClientFactoryHelper
{
	public static HttpMessageHandler CreateHandler()
	{
#if DEBUG
		return new HttpClientHandler
		{
			ServerCertificateCustomValidationCallback =
				HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
		};
#else
		return new HttpClientHandler();
#endif
	}
}
