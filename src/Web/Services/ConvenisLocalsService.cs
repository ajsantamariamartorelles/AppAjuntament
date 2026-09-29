using AppAjuntament.Models;
using AppAjuntament.Models.Conveni;
using Microsoft.EntityFrameworkCore;
using EnsModel = AppAjuntament.Models.Base.Ens.Ens;

namespace AppAjuntament.Services
{
    public class ConvenisLocalsService
    {
        private readonly GestorSubvencionsContext _context;
        private readonly DatabaseLoggerService _dbLogger;

        public ConvenisLocalsService(GestorSubvencionsContext context, DatabaseLoggerService dbLogger)
        {
            _context = context;
            _dbLogger = dbLogger;
        }

        public async Task<List<ConvenisLocals>> GetAllConvenisLocalsAsync()
        {
            try
            {
                var convenis = await _context.ConvenisLocals
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                await _dbLogger.LogInformationAsync($"S'han obtingut {convenis.Count} convenis locals", "GetAllConvenisLocalsAsync");
                return convenis;
            }
            catch (Exception ex)
            {
                await _dbLogger.LogErrorAsync($"Error en obtenir convenis locals: {ex.Message}", ex, "GetAllConvenisLocalsAsync");
                throw;
            }
        }

        public async Task<List<EnsModel>> GetAllEnsAsync()
        {
            try
            {
                var ens = await _context.Ens.ToListAsync();
                await _dbLogger.LogInformationAsync($"S'han obtingut {ens.Count} ens", "GetAllEnsAsync");
                return ens;
            }
            catch (Exception ex)
            {
                await _dbLogger.LogErrorAsync($"Error en obtenir ens: {ex.Message}", ex, "GetAllEnsAsync");
                throw;
            }
        }

        public async Task<ConvenisLocals?> GetConveniLocalByIdAsync(int id)
        {
            try
            {
                var conveni = await _context.ConvenisLocals
                    .Include(c => _context.ConvenisAuxiliars.Where(a => a.ConveniId == c._Id))
                    .FirstOrDefaultAsync(c => c._Id == id);

                if (conveni != null)
                {
                    await _dbLogger.LogInformationAsync($"S'ha obtingut el conveni local amb ID {id}", "GetConveniLocalByIdAsync");
                }
                else
                {
                    await _dbLogger.LogWarningAsync($"No s'ha trobat el conveni local amb ID {id}", "GetConveniLocalByIdAsync");
                }

                return conveni;
            }
            catch (Exception ex)
            {
                await _dbLogger.LogErrorAsync($"Error en obtenir conveni local per ID {id}: {ex.Message}", ex, "GetConveniLocalByIdAsync");
                throw;
            }
        }

        public async Task<ConvenisLocals> CreateConveniLocalAsync(ConvenisLocals conveni)
        {
            try
            {
                conveni.CreatedAt = DateTime.Now;
                conveni.UpdatedAt = DateTime.Now;

                _context.ConvenisLocals.Add(conveni);
                await _context.SaveChangesAsync();

                await _dbLogger.LogInformationAsync($"S'ha creat un nou conveni local amb ID {conveni._Id}", "CreateConveniLocalAsync");
                return conveni;
            }
            catch (Exception ex)
            {
                await _dbLogger.LogErrorAsync($"Error en crear conveni local: {ex.Message}", ex, "CreateConveniLocalAsync");
                throw;
            }
        }

        public async Task<ConvenisLocals> UpdateConveniLocalAsync(ConvenisLocals conveni)
        {
            try
            {
                conveni.UpdatedAt = DateTime.Now;

                _context.ConvenisLocals.Update(conveni);
                await _context.SaveChangesAsync();

                await _dbLogger.LogInformationAsync($"S'ha actualitzat el conveni local amb ID {conveni._Id}", "UpdateConveniLocalAsync");
                return conveni;
            }
            catch (Exception ex)
            {
                await _dbLogger.LogErrorAsync($"Error en actualitzar conveni local amb ID {conveni._Id}: {ex.Message}", ex, "UpdateConveniLocalAsync");
                throw;
            }
        }

        public async Task<bool> DeleteConveniLocalAsync(int id)
        {
            try
            {
                var conveni = await _context.ConvenisLocals.FindAsync(id);
                if (conveni == null)
                {
                    await _dbLogger.LogWarningAsync($"No s'ha trobat el conveni local amb ID {id} per eliminar", "DeleteConveniLocalAsync");
                    return false;
                }

                // Eliminar auxiliars relacionats
                var auxiliars = await _context.ConvenisAuxiliars.Where(a => a.ConveniId == id).ToListAsync();
                _context.ConvenisAuxiliars.RemoveRange(auxiliars);

                _context.ConvenisLocals.Remove(conveni);
                await _context.SaveChangesAsync();

                await _dbLogger.LogInformationAsync($"S'ha eliminat el conveni local amb ID {id}", "DeleteConveniLocalAsync");
                return true;
            }
            catch (Exception ex)
            {
                await _dbLogger.LogErrorAsync($"Error en eliminar conveni local amb ID {id}: {ex.Message}", ex, "DeleteConveniLocalAsync");
                throw;
            }
        }

        public async Task<List<ConvenisAuxiliars>> GetAuxiliarsByConveniIdAsync(int conveniId)
        {
            try
            {
                var auxiliars = await _context.ConvenisAuxiliars
                    .Where(a => a.ConveniId == conveniId)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                return auxiliars;
            }
            catch (Exception ex)
            {
                await _dbLogger.LogErrorAsync($"Error en obtenir auxiliars per conveni ID {conveniId}: {ex.Message}", ex, "GetAuxiliarsByConveniIdAsync");
                throw;
            }
        }

        public async Task<ConvenisAuxiliars> AddAuxiliarAsync(ConvenisAuxiliars auxiliar)
        {
            try
            {
                auxiliar.CreatedAt = DateTime.Now;
                auxiliar.UpdatedAt = DateTime.Now;

                _context.ConvenisAuxiliars.Add(auxiliar);
                await _context.SaveChangesAsync();

                await _dbLogger.LogInformationAsync($"S'ha afegit un auxiliar al conveni {auxiliar.ConveniId}", "AddAuxiliarAsync");
                return auxiliar;
            }
            catch (Exception ex)
            {
                await _dbLogger.LogErrorAsync($"Error en afegir auxiliar: {ex.Message}", ex, "AddAuxiliarAsync");
                throw;
            }
        }

        public async Task<ConvenisLocals> AddConveniLocalAsync(ConvenisLocals conveni)
        {
            try
            {
                conveni.IsLocal = true;
                conveni.CreatedAt = DateTime.Now;
                conveni.UpdatedAt = DateTime.Now;

                _context.ConvenisLocals.Add(conveni);
                await _context.SaveChangesAsync();

                await _dbLogger.LogInformationAsync($"S'ha afegit un conveni local: {conveni.TITOL_CONVENI}", "AddConveniLocalAsync");
                return conveni;
            }
            catch (Exception ex)
            {
                await _dbLogger.LogErrorAsync($"Error en afegir conveni local: {ex.Message}", ex, "AddConveniLocalAsync");
                throw;
            }
        }
    }
}