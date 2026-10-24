using Microsoft.EntityFrameworkCore;
using AppAjuntament.Models;
using AppAjuntament.Models.Base.AcceptacioTermes;

namespace AppAjuntament.Services
{
    /// <summary>
    /// Acceptació dels termes i condicions i la política de privacitat, comuna al
    /// Web i a l'app de Cursets. Vegeu AcceptacioTermes.cs i AGENTS.md.
    /// </summary>
    public interface IAcceptacioTermesService
    {
        /// <summary>
        /// Versió vigent del text (Termes + Privacitat). Puja-la quan canviï el
        /// contingut legal: qui ja havia acceptat una versió anterior, se li
        /// tornarà a demanar.
        /// </summary>
        const int VersioActual = 1;

        /// <summary>Si el correu ha acceptat la versió vigent del text.</summary>
        Task<bool> HaAcceptatVersioActualAsync(string email);

        /// <summary>Registra (o actualitza) l'acceptació de la versió vigent per a aquest correu.</summary>
        Task AcceptaAsync(string email);
    }

    public class AcceptacioTermesService : IAcceptacioTermesService
    {
        // Un context propi a cada crida (no el del circuit de Blazor): MainLayout comprova
        // l'acceptació alhora que la pàgina carrega les seves dades, i EF no admet dues
        // operacions simultànies sobre el mateix context.
        private readonly IDbContextFactory<GestorSubvencionsContext> _contextFactory;

        public AcceptacioTermesService(IDbContextFactory<GestorSubvencionsContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        private static string Normalitza(string email) => email.Trim().ToLowerInvariant();

        public async Task<bool> HaAcceptatVersioActualAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            var correu = Normalitza(email);
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.AcceptacionsTermes.AsNoTracking()
                .AnyAsync(a => a.Email == correu && a.VersioAcceptada >= IAcceptacioTermesService.VersioActual);
        }

        public async Task AcceptaAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Cal un correu per registrar l'acceptació.", nameof(email));

            var correu = Normalitza(email);
            await using var context = await _contextFactory.CreateDbContextAsync();
            var existent = await context.AcceptacionsTermes.FirstOrDefaultAsync(a => a.Email == correu);
            if (existent is null)
            {
                context.AcceptacionsTermes.Add(new AcceptacioTermes
                {
                    Email = correu,
                    VersioAcceptada = IAcceptacioTermesService.VersioActual,
                    DataAcceptacio = DateTime.Now
                });
            }
            else
            {
                existent.VersioAcceptada = IAcceptacioTermesService.VersioActual;
                existent.DataAcceptacio = DateTime.Now;
            }
            await context.SaveChangesAsync();
        }
    }
}
