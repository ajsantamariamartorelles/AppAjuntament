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
        private readonly GestorSubvencionsContext _context;

        public AcceptacioTermesService(GestorSubvencionsContext context)
        {
            _context = context;
        }

        private static string Normalitza(string email) => email.Trim().ToLowerInvariant();

        public async Task<bool> HaAcceptatVersioActualAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            var correu = Normalitza(email);
            return await _context.AcceptacionsTermes.AsNoTracking()
                .AnyAsync(a => a.Email == correu && a.VersioAcceptada >= IAcceptacioTermesService.VersioActual);
        }

        public async Task AcceptaAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Cal un correu per registrar l'acceptació.", nameof(email));

            var correu = Normalitza(email);
            var existent = await _context.AcceptacionsTermes.FirstOrDefaultAsync(a => a.Email == correu);
            if (existent is null)
            {
                _context.AcceptacionsTermes.Add(new AcceptacioTermes
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
            await _context.SaveChangesAsync();
        }
    }
}
