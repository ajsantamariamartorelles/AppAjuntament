using System.Security.Claims;
using AppAjuntament.Controllers.Cursets;
using AppAjuntament.Services;
using AppAjuntament.Services.Cursets;
using AppAjuntament.Tests.Infraestructura;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AppAjuntament.Tests;

/// <summary>
/// Quan falla alguna cosa inesperada en iniciar la classe (p. ex. la BD no està
/// al dia), la professora ha de rebre un missatge entenedor, no un "500" tècnic.
/// </summary>
public class CursetsSessionsControllerTests : IDisposable
{
    private sealed class EmailNoop : IEmailService
    {
        public Task<bool> EnviarAsync(EmailEnviamentParams parametres, string destinatari, string assumpte, string cosHtml)
            => Task.FromResult(true);
    }

    private readonly BdProves _bd = new();

    public void Dispose() => _bd.Dispose();

    [Fact]
    public async Task Obrir_sessio_amb_error_inesperat_torna_missatge_clar_per_a_la_professora()
    {
        // Un context ja alliberat fa fallar la consulta amb una excepció no controlada.
        var context = _bd.NouContext();
        context.Dispose();
        var servei = new CursetsSessionsService(context, new EmailNoop());

        var controller = new CursetsSessionsController(servei, NullLogger<CursetsSessionsController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "test"))
                }
            }
        };

        var resultat = await controller.ObrirSessio(1);

        var objecte = Assert.IsType<ObjectResult>(resultat);
        Assert.Equal(StatusCodes.Status500InternalServerError, objecte.StatusCode);
        var missatge = objecte.Value!.GetType().GetProperty("message")!.GetValue(objecte.Value) as string;
        Assert.Equal(CursetsSessionsController.MissatgeErrorInesperat, missatge);
    }
}
