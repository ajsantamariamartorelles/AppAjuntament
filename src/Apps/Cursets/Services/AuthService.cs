using System.Net.Http.Json;
using System.Text.Json;
using AjuntamentSantaMariaMartorelles.Models;

namespace AjuntamentSantaMariaMartorelles.Services;

/// <summary>
/// Gestiona el login de la professora (usuari/contrasenya propis, independent
/// de l'app web de l'ajuntament) i l'emmagatzematge segur del token JWT amb
/// SecureStorage (xifrat pel sistema operatiu; no viatja a còpies de
/// seguretat sense xifrar).
/// </summary>
public class AuthService
{
	private const string TokenKey = "cursets_jwt_token";
	private const string ExpiryKey = "cursets_jwt_expiry";
	private const string ProfessoraNomKey = "cursets_professora_nom";
	private const string RolKey = "cursets_rol";
	private const string TeContrasenyaKey = "cursets_te_contrasenya";
	private const string CalAcceptarTermesKey = "cursets_cal_acceptar_termes";

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	private readonly HttpClient _httpClient;
	private readonly MicrosoftAuthService _microsoftAuth;

	public string? Token { get; private set; }
	public string? ProfessoraNom { get; private set; }
	public string Rol { get; private set; } = "Professora";

	/// <summary>Motiu de l'últim intent de login amb Microsoft fallit (per mostrar-lo a l'usuari).</summary>
	public string? UltimErrorMicrosoft { get; private set; }

	/// <summary>
	/// La sessió s'ha iniciat amb correu i contrasenya (i per tant es pot canviar la
	/// contrasenya des de l'app). Amb el compte de Microsoft no n'hi ha.
	/// </summary>
	public bool TeContrasenya { get; private set; } = true;

	/// <summary>Encara no ha acceptat la versió vigent dels termes i la protecció de
	/// dades: cal mostrar-li la pantalla d'acceptació abans del menú.</summary>
	public bool CalAcceptarTermes { get; private set; }

	public bool EsControlador => string.Equals(Rol, "Controlador", StringComparison.OrdinalIgnoreCase);
	public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

	public AuthService(MicrosoftAuthService microsoftAuth)
	{
		_microsoftAuth = microsoftAuth;

		// Client propi i sense el AuthTokenHandler: el login és anònim
		// (encara no hi ha token) i mai s'ha de barrejar amb les crides
		// autenticades de CursetsApiService.
		_httpClient = new HttpClient(HttpClientFactoryHelper.CreateHandler())
		{
			BaseAddress = new Uri(Config.ApiConfig.BaseUrl)
		};
	}

	/// <summary>Recupera una sessió vàlida guardada anteriorment (en obrir l'app).</summary>
	public async Task<bool> RestoreSessionAsync()
	{
		try
		{
			var token = await SecureStorage.Default.GetAsync(TokenKey);
			var expiryText = await SecureStorage.Default.GetAsync(ExpiryKey);

			if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(expiryText))
				return false;

			if (!DateTime.TryParse(expiryText, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expiry)
				|| expiry.ToUniversalTime() <= DateTime.UtcNow)
			{
				Logout();
				return false;
			}

			Token = token;
			ProfessoraNom = await SecureStorage.Default.GetAsync(ProfessoraNomKey);
			Rol = await SecureStorage.Default.GetAsync(RolKey) ?? "Professora";
			// Sessions anteriors a aquest camp: es considera que sí (el servidor ho valida).
			TeContrasenya = (await SecureStorage.Default.GetAsync(TeContrasenyaKey)) != "0";
			// Sessions anteriors a aquest camp: es considera que no cal (ja funcionaven abans).
			CalAcceptarTermes = (await SecureStorage.Default.GetAsync(CalAcceptarTermesKey)) == "1";
			return true;
		}
		catch
		{
			// SecureStorage pot fallar en alguns dispositius/simuladors; en
			// aquest cas simplement es demana login de nou.
			return false;
		}
	}

	public async Task<bool> LoginAsync(string email, string password)
	{
		var request = new LoginRequest { Email = email, Password = password };
		var response = await _httpClient.PostAsJsonAsync("api/cursets/auth/login", request);

		if (!response.IsSuccessStatusCode)
			return false;

		var body = await response.Content.ReadAsStringAsync();
		var login = JsonSerializer.Deserialize<LoginResponse>(body, JsonOptions);
		if (login is null || string.IsNullOrEmpty(login.Token))
			return false;

		await DesaSessioAsync(login, teContrasenya: true);
		return true;
	}

	/// <summary>
	/// Login amb el compte de Microsoft de l'Ajuntament: identifica't a l'AAD i
	/// bescanvia l'id_token per un token de l'app. Retorna false si l'usuari
	/// cancel·la o si el compte no té accés (no és professora ni regidor/a vigent).
	/// </summary>
	public async Task<bool> LoginMicrosoftAsync()
	{
		UltimErrorMicrosoft = null;
		string? idToken;
		try
		{
			idToken = await _microsoftAuth.AcquireIdTokenAsync();
		}
		catch (Microsoft.Identity.Client.MsalClientException ex)
		{
			// Cancel·lació de l'usuari, navegador tancat, redirect no configurat...
			UltimErrorMicrosoft = $"Microsoft: {ex.ErrorCode} · {ex.Message}";
			return false;
		}
		catch (Exception ex)
		{
			UltimErrorMicrosoft = $"Microsoft: {ex.Message}";
			return false;
		}

		if (string.IsNullOrEmpty(idToken))
		{
			UltimErrorMicrosoft = "No s'ha rebut cap identificador de Microsoft.";
			return false;
		}

		try
		{
			using var request = new HttpRequestMessage(HttpMethod.Post, "api/cursets/auth/login-microsoft");
			request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", idToken);
			var response = await _httpClient.SendAsync(request);

			var body = await response.Content.ReadAsStringAsync();
			if (!response.IsSuccessStatusCode)
			{
				var msg = ExtreuMissatge(body);
				UltimErrorMicrosoft = $"Servidor {(int)response.StatusCode}: {msg}";
				return false;
			}

			var login = JsonSerializer.Deserialize<LoginResponse>(body, JsonOptions);
			if (login is null || string.IsNullOrEmpty(login.Token))
			{
				UltimErrorMicrosoft = "Resposta del servidor no vàlida.";
				return false;
			}

			await DesaSessioAsync(login, teContrasenya: false);
			return true;
		}
		catch (Exception ex)
		{
			UltimErrorMicrosoft = $"Connexió: {ex.Message}";
			return false;
		}
	}

	private static string ExtreuMissatge(string body)
	{
		try
		{
			using var doc = JsonDocument.Parse(body);
			if (doc.RootElement.TryGetProperty("message", out var m))
				return m.GetString() ?? body;
		}
		catch { /* body no és JSON */ }
		return string.IsNullOrWhiteSpace(body) ? "(sense detall)" : body;
	}

	/// <summary>
	/// Canvia la contrasenya de qui té la sessió iniciada. Retorna null si ha anat bé o
	/// el missatge d'error per mostrar a l'usuari.
	/// </summary>
	public async Task<string?> CanviaPasswordAsync(string passwordActual, string passwordNova)
	{
		if (string.IsNullOrEmpty(Token))
			return "La sessió ha caducat. Torna a entrar.";

		try
		{
			using var request = new HttpRequestMessage(HttpMethod.Post, "api/cursets/auth/canviar-password")
			{
				Content = JsonContent.Create(new { PasswordActual = passwordActual, PasswordNova = passwordNova })
			};
			request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
			var response = await _httpClient.SendAsync(request);

			if (response.IsSuccessStatusCode)
				return null;

			if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				return "La sessió ha caducat. Torna a entrar.";
			if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
				return "Massa intents. Espera un minut i torna-ho a provar.";

			var body = await response.Content.ReadAsStringAsync();
			return ExtreuMissatge(body);
		}
		catch (Exception ex)
		{
			return $"No s'ha pogut connectar amb el servidor: {ex.Message}";
		}
	}

	private async Task DesaSessioAsync(LoginResponse login, bool teContrasenya)
	{
		TeContrasenya = teContrasenya;
		await SecureStorage.Default.SetAsync(TeContrasenyaKey, teContrasenya ? "1" : "0");

		Token = login.Token;
		ProfessoraNom = login.ProfessoraNom;
		Rol = string.IsNullOrWhiteSpace(login.Rol) ? "Professora" : login.Rol;
		CalAcceptarTermes = login.CalAcceptarTermes;

		await SecureStorage.Default.SetAsync(TokenKey, login.Token);
		await SecureStorage.Default.SetAsync(ExpiryKey, login.ExpiresAt.ToString("o"));
		await SecureStorage.Default.SetAsync(ProfessoraNomKey, login.ProfessoraNom);
		await SecureStorage.Default.SetAsync(RolKey, Rol);
		await SecureStorage.Default.SetAsync(CalAcceptarTermesKey, CalAcceptarTermes ? "1" : "0");
	}

	/// <summary>Registra l'acceptació dels termes i la protecció de dades per a qui
	/// té la sessió iniciada. Retorna false si no s'ha pogut contactar el servidor.</summary>
	public async Task<bool> AcceptaTermesAsync()
	{
		if (string.IsNullOrEmpty(Token))
			return false;

		try
		{
			using var request = new HttpRequestMessage(HttpMethod.Post, "api/cursets/auth/accepta-termes");
			request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
			var response = await _httpClient.SendAsync(request);
			if (!response.IsSuccessStatusCode)
				return false;

			CalAcceptarTermes = false;
			await SecureStorage.Default.SetAsync(CalAcceptarTermesKey, "0");
			return true;
		}
		catch
		{
			return false;
		}
	}

	public void Logout()
	{
		Token = null;
		ProfessoraNom = null;
		Rol = "Professora";
		SecureStorage.Default.Remove(TokenKey);
		SecureStorage.Default.Remove(ExpiryKey);
		SecureStorage.Default.Remove(ProfessoraNomKey);
		SecureStorage.Default.Remove(RolKey);
		SecureStorage.Default.Remove(TeContrasenyaKey);
		SecureStorage.Default.Remove(CalAcceptarTermesKey);
		TeContrasenya = true;
		CalAcceptarTermes = false;
		_ = _microsoftAuth.SignOutAsync();
	}
}
