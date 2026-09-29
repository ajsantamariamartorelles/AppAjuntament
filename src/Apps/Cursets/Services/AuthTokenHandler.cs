using System.Net.Http.Headers;

namespace AjuntamentSantaMariaMartorelles.Services;

/// <summary>
/// DelegatingHandler que afegeix "Authorization: Bearer &lt;token&gt;" a totes
/// les peticions fetes amb el HttpClient de CursetsApiService.
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
	private readonly AuthService _authService;

	public AuthTokenHandler(AuthService authService)
	{
		_authService = authService;
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (!string.IsNullOrEmpty(_authService.Token))
		{
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authService.Token);
		}

		return base.SendAsync(request, cancellationToken);
	}
}
