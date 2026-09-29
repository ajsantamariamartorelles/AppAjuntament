namespace AjuntamentSantaMariaMartorelles.Config;

/// <summary>
/// URL base de l'API de Cursets (mòdul dins AppAjuntament).
/// En DEBUG apunta al backend en local (10.0.2.2 és l'àlies que l'emulador
/// Android fa servir per referir-se al "localhost" de l'ordinador amfitrió;
/// el simulador iOS pot fer servir "localhost" directament).
/// Abans de publicar per a producció, actualitza la branca #else amb el
/// domini real on estigui desplegat AppAjuntament.
/// </summary>
public static class ApiConfig
{
#if DEBUG
	public static readonly string BaseUrl =
		DeviceInfo.Platform == DevicePlatform.Android
			? "https://10.0.2.2:7261/"
			: "https://localhost:7261/";
#else
	public static readonly string BaseUrl = "https://app.santamariademartorelles.cat/";
#endif
}
