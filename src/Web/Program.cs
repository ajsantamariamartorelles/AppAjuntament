using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using System;
using AppAjuntament.Models;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Amazon.S3;
using System.Security.Claims;
using AppAjuntament.Services;
using AppAjuntament.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;

using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.InMemory;
using AppAjuntament.Jobs;
using AppAjuntament.Models.Cursets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;

#if RELEASE
    System.Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
#endif

// NOTA: la llicència de QuestPDF s'estableix de forma diferida a
// LiquidacioPdfDocument (constructor estàtic), NO aquí. Així, si QuestPDF.dll no
// arriba al servidor en un desplegament incremental, només falla la generació de
// PDFs de liquidacions i no l'arrencada de tota l'aplicació.

var builder = WebApplication.CreateBuilder(args);

// Secrets locals (contrasenyes, claus, connection strings reals): fora del
// repositori (AGENTS.md, regla 3). appsettings.secrets.sample.json en mostra
// l'estructura sense valors; aquí es carrega la versió real de cadascú,
// ignorada pel git.
builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.secrets.json",
    optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToPage("/_Host");
});
builder.Services.AddServerSideBlazor();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources"); // Configura la carpeta de recursos

// Afegir HttpContextAccessor per accedir al context HTTP dels components Blazor
builder.Services.AddHttpContextAccessor();

// Configurar autenticació Azure AD (Cookies per autenticar peticions, OIDC per fer challenge)
var authenticationBuilder = builder.Services
    .AddAuthentication(options =>
    {
        // Assegurem explícitament que el Hub de Blazor autentica amb Cookies
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    });

// AddMicrosoftIdentityWebApp retorna un builder especialitzat que no permet
// encadenar AddJwtBearer directament, per això es crida per separat sobre
// l'AuthenticationBuilder original (mateixa IServiceCollection per sota).
authenticationBuilder.AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

// Esquema JWT Bearer independent, exclusiu per a l'API de Cursets (app MAUI).
// Mai és el DefaultScheme/DefaultChallengeScheme: només l'utilitzen els
// controllers que ho demanen explícitament amb [Authorize(AuthenticationSchemes = "CursetsBearer")].
authenticationBuilder.AddJwtBearer("CursetsBearer", options =>
{
    var jwtSettings = builder.Configuration.GetSection(CursetsJwtSettings.SectionName).Get<CursetsJwtSettings>()
        ?? new CursetsJwtSettings();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

// Esquema JWT Bearer per validar id_tokens d'Azure AD que ENVIA l'app MAUI al
// endpoint /api/cursets/auth/login-microsoft (bescanvi per un token CursetsBearer).
// Reutilitza el registre d'app del web (secció AzureAd). Mai és scheme per defecte.
authenticationBuilder.AddJwtBearer("CursetsAzureBearer", options =>
{
    var aad = builder.Configuration.GetSection("AzureAd");
    var tenantId = aad["TenantId"];
    var clientId = aad["ClientId"];
    options.Authority = $"{aad["Instance"]}{tenantId}/v2.0";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = true,
        ValidAudiences = new[] { clientId, $"api://{clientId}" },
        ValidateIssuer = true,
        ValidIssuers = new[]
        {
            $"https://login.microsoftonline.com/{tenantId}/v2.0",
            $"https://sts.windows.net/{tenantId}/"
        },
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

// En alguns entorns, AddMicrosoftIdentityWebApp pot canviar valors per defecte. Forcem-les després.
builder.Services.PostConfigure<Microsoft.AspNetCore.Authentication.AuthenticationOptions>(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
});

// Configuració del JWT de Cursets + hashing de contrasenyes (usuari/contrasenya propi, sense Azure AD)
builder.Services.Configure<CursetsJwtSettings>(builder.Configuration.GetSection(CursetsJwtSettings.SectionName));
builder.Services.AddScoped<IPasswordHasher<AppAjuntament.Models.Base.Usuari.Usuari>, PasswordHasher<AppAjuntament.Models.Base.Usuari.Usuari>>();
builder.Services.AddScoped<IPasswordHasher<AppAjuntament.Models.Base.Regidor.Regidor>, PasswordHasher<AppAjuntament.Models.Base.Regidor.Regidor>>();

// Configurar autorització
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // Polítiques per als grups especificats
    var groupAccess1 = builder.Configuration["Authorization:Groups:GroupAccess1"]!;
    var groupAccess2 = builder.Configuration["Authorization:Groups:GroupAccess2"]!;
    options.AddPolicy("GroupAccess1", policy => policy.RequireClaim("groups", groupAccess1));
    options.AddPolicy("GroupAccess2", policy => policy.RequireClaim("groups", groupAccess2));

    // Polítiques per rols personalitzats
    options.AddPolicy("AdminOnly", policy => policy.RequireAssertion(context =>
        context.User.HasClaim(c => (c.Type == "roles" || c.Type == ClaimTypes.Role) && c.Value == "Admin")));
    options.AddPolicy("ViewerOnly", policy => policy.RequireAssertion(context =>
        context.User.HasClaim(c => (c.Type == "roles" || c.Type == ClaimTypes.Role) && c.Value == "Viewer")));
    options.AddPolicy("EditorOnly", policy => policy.RequireAssertion(context =>
        context.User.HasClaim(c => (c.Type == "roles" || c.Type == ClaimTypes.Role) && c.Value == "Editor")));
    options.AddPolicy("EditorOrAdmin", policy => policy.RequireAssertion(context =>
        context.User.HasClaim(c => (c.Type == "roles" || c.Type == ClaimTypes.Role) && (c.Value == "Editor" || c.Value == "Admin"))));
});

// Afegir rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;  // 5 intents per minut
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });

    // Formulari públic d'inscripció a cursos: limita l'spam.
    options.AddFixedWindowLimiter("inscripcio-publica", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configuració de l'Ajuntament
builder.Services.Configure<AjuntamentSettings>(
    builder.Configuration.GetSection(AjuntamentSettings.SectionName));

builder.Services.AddDbContextFactory<GestorSubvencionsContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Afegir HttpClient per a les crides a l'API amb configuració de timeout i retry policies
builder.Services.AddHttpClient<ComarcaApiService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5); // Timeout de 5 minuts
    client.DefaultRequestHeaders.Add("User-Agent", "GestorSubvencions/1.0");
});

builder.Services.AddHttpClient<MunicipiApiService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5); // Timeout de 5 minuts
    client.DefaultRequestHeaders.Add("User-Agent", "GestorSubvencions/1.0");
});

builder.Services.AddHttpClient<OrdenancesApiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("User-Agent", "GestorSubvencions/1.0");
});

builder.Services.AddHttpClient<EstablimentsApiService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5); // Timeout de 5 minuts
    client.DefaultRequestHeaders.Add("User-Agent", "GestorSubvencions/1.0");
});

builder.Services.AddHttpClient<ElectoralApiService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5); // Timeout de 5 minuts
    client.DefaultRequestHeaders.Add("User-Agent", "GestorSubvencions/1.0");
});

builder.Services.AddHttpClient<IdescatEmexService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5); // Timeout de 5 minuts
    client.DefaultRequestHeaders.Add("User-Agent", "GestorSubvencions/1.0");
});

// Punts d'aigua (DIBA)
builder.Services.AddHttpClient<PuntsAiguaService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(2);
    client.DefaultRequestHeaders.Add("User-Agent", "GestorSubvencions/1.0");
});
// També registrar com a servei scoped per assegurar injecció a components Blazor
builder.Services.AddScoped<PuntsAiguaService>();

// Fonts naturals (DIBA)
builder.Services.AddHttpClient<FontsNaturalsService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(2);
    client.DefaultRequestHeaders.Add("User-Agent", "GestorSubvencions/1.0");
});
builder.Services.AddScoped<FontsNaturalsService>();

// Equipaments esportius (DIBA)
builder.Services.AddHttpClient<EquipamentsEsportiusService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(2);
    client.DefaultRequestHeaders.Add("User-Agent", "GestorSubvencions/1.0");
});
builder.Services.AddScoped<EquipamentsEsportiusService>();

// Tècnics municipals (Transparència)
builder.Services.AddHttpClient<TecnicsMunicipalsService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(2);
    client.DefaultRequestHeaders.Add("User-Agent", "GestorSubvencions/1.0");
});
builder.Services.AddScoped<TecnicsMunicipalsService>();

// Carrecs electes (Transparència)
builder.Services.AddHttpClient<CarrecsElectesService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(2);
    client.DefaultRequestHeaders.Add("User-Agent", "GestorSubvencions/1.0");
});
builder.Services.AddScoped<CarrecsElectesService>();

// Seu-e (fallback for regidors)
builder.Services.AddHttpClient<SeuEcService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(1);
    client.DefaultRequestHeaders.Add("User-Agent", "GestorSubvencions/1.0");
});
builder.Services.AddScoped<SeuEcService>();

// Transparència (servici agregat)
builder.Services.AddHttpClient<TransparenciaService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(2);
    client.DefaultRequestHeaders.Add("User-Agent", "GestorSubvencions/1.0");
});
builder.Services.AddScoped<TransparenciaService>();

builder.Services.AddHttpClient<IngressosExternsService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(3);
    client.DefaultRequestHeaders.Add("User-Agent", "GestorSubvencions/1.0");
});

// Afegir HttpClient per al controlador Proxy
builder.Services.AddHttpClient<ProxyController>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(2); // Timeout més curt per a imatges
    client.DefaultRequestHeaders.Add("User-Agent", "GestorSubvencions/1.0");
});

builder.Services.AddHttpClient<EntitatDigitalApiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "GestorSubvencions/1.0");
});

// Registrar els serveis de sincronització de comarques i municipis
builder.Services.AddScoped<IComarcaApiService, ComarcaApiService>();
builder.Services.AddScoped<IMunicipiApiService, MunicipiApiService>();
builder.Services.AddScoped<ICachedMunicipiService, CachedMunicipiService>();
builder.Services.AddScoped<IEstablimentsApiService, EstablimentsApiService>();
builder.Services.AddScoped<ConvenisApiService>();
builder.Services.AddScoped<ConvenisLocalsService>();
// EntitatDigitalApiService registrat via AddHttpClient (veure més amunt)
builder.Services.AddScoped<IElectoralApiService, ElectoralApiService>();
builder.Services.AddScoped<IIdescatEmexService, IdescatEmexService>();
builder.Services.AddScoped<ImportadorSubvencions>();
builder.Services.AddScoped<PdfService>();
builder.Services.AddHttpClient<CkanApiService>();

// Registrar serveis de Patrimoni

// Registrar el servei MinioStorageService per accés a MinIO
builder.Services.AddScoped<MinioStorageService>();
builder.Services.AddScoped<IPatrimoniService, PatrimoniService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddAWSService<IAmazonS3>(); // Per MinIO (S3-compatible)

// Configurar HttpClient per Blazor
var appBaseUrl = builder.Configuration["AppBaseUrl"]!;
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(appBaseUrl) });

// Registrar el servei de logging a base de dades
builder.Services.AddScoped<DatabaseLoggerService>();

// Enviament de correus (SMTP). Els paràmetres de connexió no viuen a appsettings:
// es guarden a BD (curs_email_absencies_config) i s'editen des de la web.
builder.Services.AddScoped<AppAjuntament.Services.IEmailService, AppAjuntament.Services.SmtpEmailService>();
builder.Services.AddScoped<AppAjuntament.Services.IAcceptacioTermesService, AppAjuntament.Services.AcceptacioTermesService>();

// Serveis del mòdul Cursets
builder.Services.AddScoped<AppAjuntament.Services.Cursets.ICursetsAuthService, AppAjuntament.Services.Cursets.CursetsAuthService>();
builder.Services.AddScoped<AppAjuntament.Services.Cursets.ICursetsCatalogService, AppAjuntament.Services.Cursets.CursetsCatalogService>();
builder.Services.AddScoped<AppAjuntament.Services.Cursets.ICursetsSessionsService, AppAjuntament.Services.Cursets.CursetsSessionsService>();
builder.Services.AddScoped<AppAjuntament.Services.Cursets.ICursetsLiquidacioService, AppAjuntament.Services.Cursets.CursetsLiquidacioService>();
builder.Services.AddScoped<AppAjuntament.Services.Cursets.ICursetsSeguimentService, AppAjuntament.Services.Cursets.CursetsSeguimentService>();

// Configurar CircuitHandler per capturar errors globals
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Server.Circuits.CircuitHandler, GlobalErrorHandler>();

// Afegir Memory Cache
builder.Services.AddMemoryCache();

// Hangfire InMemory storage (MySQL 5.6 no suporta release_all_locks() requerida pels paquets Hangfire MySQL)
builder.Services.AddHangfire(config =>
    config.UseInMemoryStorage()
);
builder.Services.AddHangfireServer();
// El dashboard s'afegeix després de Build amb app.UseHangfireDashboard();



// Registrar el servei ComarquesImportJob i el HostedService per inicialitzar jobs Hangfire
builder.Services.AddTransient<ComarquesImportJob>();
builder.Services.AddTransient<MunicipisImportJob>();
builder.Services.AddHostedService<HangfireJobsInitializer>();


var app = builder.Build();


// (La programació del job es fa ara via HostedService: HangfireJobsInitializer)

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GestorSubvencionsContext>();
    db.Database.EnsureCreated();
}

// NOMÉS EN DESENVOLUPAMENT: professora de prova per poder provar el login de
// l'app MAUI i la gestió de professores sense dependre d'Azure AD.
// Email: professora.test@santamariademartorelles.cat  ·  Contrasenya: Prova1234
if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var authService = scope.ServiceProvider
            .GetRequiredService<AppAjuntament.Services.Cursets.ICursetsAuthService>();
        var catalogService = scope.ServiceProvider
            .GetRequiredService<AppAjuntament.Services.Cursets.ICursetsCatalogService>();

        var professora = await authService.CrearOActualitzarProfessoraAsync(
            "Professora de Prova",
            "professora.test@santamariademartorelles.cat",
            "Prova1234");

        var cursets = await catalogService.GetCursetsAdminAsync();
        if (cursets.Count == 0)
        {
            var tipus = await catalogService.GetTipusCursetsAsync(nomesActius: false);
            var tipusId = tipus.FirstOrDefault()?.Id
                ?? (await catalogService.CrearTipusCursetAsync(
                        new AppAjuntament.Models.Cursets.Dto.CrearTipusCursetRequest { Nom = "Pilates" })).Id;

            await catalogService.CrearCursetAsync(new AppAjuntament.Models.Cursets.Dto.CrearCursetRequest
            {
                Nom = "Dilluns matí",
                TipusCursetId = tipusId,
                ProfessoraId = professora.Id,
                DiaSetmana = DayOfWeek.Monday,
                HoraInici = new TimeSpan(9, 30, 0),
                HoraFi = new TimeSpan(10, 30, 0),
                PreuPerSessioEmpadronat = 3.5m,
                PreuPerSessioNoEmpadronat = 5m
            });
        }

        // Curset petit amb inscripció oberta i sense enllaç extern: prova del
        // formulari públic d'inscripció + del sorteig (2 places, 3 sol·licituds).
        var totsCursets = await catalogService.GetCursetsAdminAsync();
        if (totsCursets.All(c => c.Nom != "Dijous tarda"))
        {
            var tipusId = (await catalogService.GetTipusCursetsAsync(nomesActius: false)).FirstOrDefault()?.Id
                ?? (await catalogService.CrearTipusCursetAsync(
                        new AppAjuntament.Models.Cursets.Dto.CrearTipusCursetRequest { Nom = "Pilates" })).Id;
            await catalogService.CrearCursetAsync(new AppAjuntament.Models.Cursets.Dto.CrearCursetRequest
            {
                Nom = "Dijous tarda",
                TipusCursetId = tipusId,
                ProfessoraId = professora.Id,
                DiaSetmana = DayOfWeek.Thursday,
                HoraInici = new TimeSpan(18, 0, 0),
                HoraFi = new TimeSpan(19, 0, 0),
                PreuPerSessioEmpadronat = 3.5m,
                PreuPerSessioNoEmpadronat = 5m,
                MaxPlaces = 2,
                InscripcioInici = DateTime.Today.AddDays(-3),
                InscripcioFi = DateTime.Today.AddDays(14)
            });
        }

        var alumnes = await catalogService.GetAlumnesAsync();
        if (alumnes.Count == 0)
        {
            var cursetId = (await catalogService.GetCursetsAsync()).FirstOrDefault()?.Id;
            await catalogService.CrearAlumneAsync(new AppAjuntament.Models.Cursets.Dto.CrearAlumneRequest
            {
                Nom = "Alumna",
                Cognoms = "de Prova",
                Dni = "00000000T",
                Telefon = "600000000",
                Adreca = "Carrer de Prova, 1",
                CodiPostal = "08115",
                Poblacio = "Santa Maria de Martorelles",
                Empadronat = true,
                CursetIds = cursetId.HasValue ? new List<int> { cursetId.Value } : new List<int>()
            });
        }

        // Sol·licituds d'inscripció de prova al curset petit (per provar la fitxa
        // d'alumne, la llista d'espera i el sorteig sense passar pel formulari web).
        {
            var db2 = scope.ServiceProvider.GetRequiredService<GestorSubvencionsContext>();
            var cursetPetit = await db2.Cursets
                .Where(c => c.MaxPlaces != null && c.MaxPlaces <= 3)
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();
            if (cursetPetit is not null
                && !await db2.AlumnesCursets.AnyAsync(ac => ac.CursetId == cursetPetit.Id
                        && ac.Estat == AppAjuntament.Models.Cursets.EstatInscripcio.Sollicitada))
            {
                foreach (var (nom, cognoms, dni, emp) in new[]
                {
                    ("Marta", "Vidal Roca", "11111111H", true),
                    ("Jordi", "Soler Camps", "22222222J", false),
                    ("Aina", "Prat Ferrer", "33333333P", true),
                })
                {
                    try
                    {
                        await catalogService.CrearSollicitudPublicaAsync(
                            new AppAjuntament.Models.Cursets.Dto.SollicitudInscripcioPublicaRequest
                            {
                                CursetId = cursetPetit.Id,
                                Nom = nom,
                                Cognoms = cognoms,
                                Dni = dni,
                                Telefon = "600000000",
                                Email = $"{nom.ToLowerInvariant()}@exemple.cat",
                                Adreca = "Carrer de Prova, 2",
                                CodiPostal = "08115",
                                Poblacio = "Santa Maria de Martorelles",
                                DeclaraEmpadronat = emp,
                                ConsentimentRgpd = true
                            });
                    }
                    catch (ArgumentException) { /* ja existeix */ }
                }
            }
        }

        // Un parell de sessions passades amb assistència, per poder provar la
        // pàgina d'assistències sense obrir classes des de l'app.
        var sessionsService = scope.ServiceProvider
            .GetRequiredService<AppAjuntament.Services.Cursets.ICursetsSessionsService>();
        var cursetAmbAlumnes = (await catalogService.GetCursetsAsync())
            .FirstOrDefault(c => c.NumAlumnes > 0);
        if (cursetAmbAlumnes is not null
            && (await sessionsService.GetAssistenciesPerCursetAsync(cursetAmbAlumnes.Id)).Count == 0)
        {
            foreach (var diesEnrere in new[] { 14, 7 })
            {
                var data = DateTime.Today.AddDays(-diesEnrere);
                var oberta = await sessionsService.ObrirSessioAsync(
                    cursetAmbAlumnes.Id, cursetAmbAlumnes.ProfessoraId, data);
                // La primera alumna consta absent a la sessió més antiga.
                var assistencia = oberta.Alumnes
                    .Select((a, i) => new AppAjuntament.Models.Cursets.Dto.AssistenciaAlumneDto
                    {
                        AlumneId = a.AlumneId,
                        Present = !(diesEnrere == 14 && i == 0),
                        Nota = (diesEnrere == 14 && i == 0) ? "Avisada" : null
                    })
                    .ToList();
                await sessionsService.TancarSessioAsync(oberta.SessioId,
                    new AppAjuntament.Models.Cursets.Dto.GuardarAssistenciaRequest { Alumnes = assistencia });
            }
        }

        // Codis de liquidació de prova (per poder emetre remeses des de la web).
        {
            var db = scope.ServiceProvider.GetRequiredService<GestorSubvencionsContext>();

            // Retrodata les inscripcions de prova perquè les sessions passades ja
            // seedejades siguin facturables (altrament DataAlta = avui les exclou).
            foreach (var ac in await db.AlumnesCursets.ToListAsync())
            {
                var primeraSessio = await db.CursetsSessions
                    .Where(s => s.CursetId == ac.CursetId && s.Estat == EstatSessio.Tancada)
                    .OrderBy(s => s.Data)
                    .Select(s => (DateTime?)s.Data)
                    .FirstOrDefaultAsync();
                if (primeraSessio is DateTime d && ac.DataAlta.Date > d.Date)
                {
                    ac.DataAlta = d.Date.AddDays(-7);
                    await db.SaveChangesAsync();
                }
            }

            var tipusPilates = await db.TipusCursets.FirstOrDefaultAsync(t => t.Nom == "Pilates");
            if (tipusPilates is not null && string.IsNullOrWhiteSpace(tipusPilates.CodiLiquidacio))
            {
                tipusPilates.CodiLiquidacio = "PTES";
                await db.SaveChangesAsync();
            }
            var cursetSeed = await db.Cursets.OrderBy(c => c.Id).FirstOrDefaultAsync();
            if (cursetSeed is not null)
            {
                if (string.IsNullOrWhiteSpace(cursetSeed.CodiLiquidacio))
                {
                    cursetSeed.CodiLiquidacio = "DLM";
                    cursetSeed.CadenciaLiquidacio = "Trimestral";
                }
                if (string.IsNullOrWhiteSpace(cursetSeed.OrdenancaLiquidacio))
                    cursetSeed.OrdenancaLiquidacio = "Ordenança fiscal reguladora de la taxa per la prestació de serveis esportius municipals.";
                if (cursetSeed.MaxPlaces is null)
                    cursetSeed.MaxPlaces = 12;
                if (cursetSeed.InscripcioInici is null && cursetSeed.InscripcioFi is null)
                {
                    cursetSeed.InscripcioInici = DateTime.Today.AddDays(-7);
                    cursetSeed.InscripcioFi = DateTime.Today.AddDays(21);
                }
                await db.SaveChangesAsync();
            }
        }

        // Controlador de prova: assigna un regidor vigent al curset amb sessions
        // i li dona accés a l'app. Login controlador: regidor.test@... / Control1234
        var regidorVigent = (await authService.GetRegidorsVigentsAsync()).FirstOrDefault();
        var cursetPerControlar = (await catalogService.GetCursetsAsync())
            .FirstOrDefault(c => c.NumAlumnes > 0)
            ?? (await catalogService.GetCursetsAdminAsync()).FirstOrDefault();
        if (regidorVigent is not null && cursetPerControlar is not null)
        {
            if (cursetPerControlar.RegidorId is null)
            {
                var db = scope.ServiceProvider.GetRequiredService<GestorSubvencionsContext>();
                var c = await db.Cursets.FindAsync(cursetPerControlar.Id);
                if (c is not null) { c.RegidorId = regidorVigent.Id; await db.SaveChangesAsync(); }
            }
            if (!regidorVigent.TeAcces)
                await authService.DonarOActualitzarAccesAsync(
                    regidorVigent.Id, "regidor.test@santamariademartorelles.cat", "Control1234");
        }

        Console.WriteLine($"[SeedCursetsDev] OK · professora={professora.Id} · cursets={(await catalogService.GetCursetsAdminAsync()).Count} · alumnes={(await catalogService.GetAlumnesAsync()).Count} · regidor-control={regidorVigent?.Id}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[SeedCursetsDev] No s'han pogut crear les dades de prova: {ex.Message}");
    }
}

// Middleware per capturar errors globals i guardar-los a la base de dades
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        // Intentar guardar l'error a la base de dades
        try
        {
            using var scope = app.Services.CreateScope();
            var dbLogger = scope.ServiceProvider.GetService<DatabaseLoggerService>();
            if (dbLogger != null)
            {
                await dbLogger.LogFatalAsync($"Error no capturat: {ex.Message}", ex, "GlobalErrorMiddleware");
            }
        }
        catch
        {
            // Si falla el logging a BD, almenys log a consola
        }

        // Re-throw per permetre que altres handlers processin l'error
        throw;
    }
});

// Configurar idiomes suportats
var supportedCultures = new[]
{
    new CultureInfo("ca-ES"), // Català
    new CultureInfo("es-ES")  // Castellà
};
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("ca-ES"), // Idioma per defecte
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();  // Aplicar rate limiting

app.MapControllers(); // Afegir aquesta línia per habilitar els controladors API
app.MapRazorPages(); // Afegir aquesta línia per habilitar les pàgines Razor (necessàries per a l'autenticació)
app.MapBlazorHub().AllowAnonymous();
app.MapFallbackToPage("/_Host").AllowAnonymous();

app.UseHangfireDashboard("/hangfire");

app.Run();
