using AjuntamentSantaMariaMartorelles.Services;
using AjuntamentSantaMariaMartorelles.ViewModels;
using AjuntamentSantaMariaMartorelles.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AjuntamentSantaMariaMartorelles;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseMauiApp<App>();

		// Login amb Microsoft (Azure AD) + sessió/token de l'app: singletons per a tota l'app.
		builder.Services.AddSingleton<MicrosoftAuthService>();
		builder.Services.AddSingleton<AuthService>();

		// Afegeix automàticament "Authorization: Bearer <token>" a cada crida
		// feta amb el HttpClient de CursetsApiService.
		builder.Services.AddTransient<AuthTokenHandler>();

		builder.Services.AddHttpClient<CursetsApiService>(client =>
		{
			client.BaseAddress = new Uri(Config.ApiConfig.BaseUrl);
			client.Timeout = TimeSpan.FromSeconds(30);
		})
		.ConfigurePrimaryHttpMessageHandler(HttpClientFactoryHelper.CreateHandler)
		.AddHttpMessageHandler<AuthTokenHandler>();

		builder.Services.AddTransient<LoadingPage>();

		builder.Services.AddSingleton<LoginViewModel>();
		builder.Services.AddSingleton<LoginPage>();

		builder.Services.AddSingleton<AcceptaTermesViewModel>();
		builder.Services.AddSingleton<AcceptaTermesPage>();

		builder.Services.AddSingleton<MenuViewModel>();
		builder.Services.AddSingleton<MenuPage>();

		builder.Services.AddTransient<CompteViewModel>();
		builder.Services.AddTransient<ComptePage>();

		builder.Services.AddSingleton<CursetsListViewModel>();
		builder.Services.AddSingleton<CursetsListPage>();

		builder.Services.AddTransient<SessioViewModel>();
		builder.Services.AddTransient<SessioPage>();

		// Controlador (regidor): seguiment de només lectura.
		builder.Services.AddTransient<CursetSeguimentViewModel>();
		builder.Services.AddTransient<CursetSeguimentPage>();

		builder.Services.AddTransient<PersonaViewModel>();
		builder.Services.AddTransient<PersonaPage>();

		builder.Services.AddTransient<CobramentsViewModel>();
		builder.Services.AddTransient<CobramentsPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
