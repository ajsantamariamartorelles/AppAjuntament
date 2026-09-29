using Microsoft.EntityFrameworkCore;
using AppAjuntament.Models;
using AppAjuntament.Models.Base.Log;

namespace AppAjuntament.Services;

/// <summary>
/// Servei per gestionar el patrimoni
/// </summary>
public interface IPatrimoniService
{
    Task<IEnumerable<Patrimoni>> GetAllAsync();
    Task<Patrimoni?> GetByIdAsync(int id);
    Task<Patrimoni?> GetByCodiInventariAsync(string codi);
    Task<IEnumerable<Patrimoni>> GetByTipusAsync(int tipusId);
    Task<IEnumerable<Patrimoni>> GetByUbicacioAsync(int ubicacioId);
    Task<IEnumerable<Patrimoni>> SearchAsync(string searchTerm);
    Task<Patrimoni> CreateAsync(Patrimoni patrimoni);
    Task<Patrimoni?> UpdateAsync(int id, Patrimoni patrimoni);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsCodiInventariAsync(string codi, int? excludeId = null);
}

public class PatrimoniService : IPatrimoniService
{
    private readonly GestorSubvencionsContext _context;

    public PatrimoniService(GestorSubvencionsContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Patrimoni>> GetAllAsync()
    {
        try
        {
            return await _context.Patrimonis
                .Include(p => p.TipusPatrimoni)
                .Include(p => p.EstatConservacio)
                .Include(p => p.Ubicacio)
                .Include(p => p.Arxius.Where(a => a.Actiu))
                    .ThenInclude(a => a.TipusArxiu)
                .Where(p => p.Actiu)
                .OrderBy(p => p.Nom)
                .ToListAsync();
        }
        catch (Exception ex) when (ContainsTableNotFound(ex))
        {
            Console.WriteLine($"[PatrimoniService] GetAllAsync - Alguna taula no existeix ({ex.InnerException?.Message ?? ex.Message})");
            
            // Intentar sense includes si alguna taula falta
            return await _context.Patrimonis
                .Where(p => p.Actiu)
                .OrderBy(p => p.Nom)
                .ToListAsync();
        }
    }

    public async Task<Patrimoni?> GetByIdAsync(int id)
    {
        Console.WriteLine($"[PatrimoniService] Buscant patrimoni amb Id: {id}");
        
        try
        {
            // Intentar carregar amb tots els includes
            var result = await _context.Patrimonis
                .Include(p => p.TipusPatrimoni)
                .Include(p => p.EstatConservacio)
                .Include(p => p.Ubicacio)
                .Include(p => p.Arxius)
                    .ThenInclude(a => a.TipusArxiu)
                .Include(p => p.Arxius)
                    .ThenInclude(a => a.Colleccio)
                .Include(p => p.Inspeccions)
                .Include(p => p.Intervencions)
                .Include(p => p.NotesPatrimoni)
                    .ThenInclude(n => n.TipusNota)
                .Include(p => p.Identificadors)
                .FirstOrDefaultAsync(p => p.Id == id && p.Actiu);
                
            Console.WriteLine($"[PatrimoniService] Element trobat: {result != null}");
            return result;
        }
        catch (Exception ex) when (ContainsTableNotFound(ex))
        {
            Console.WriteLine($"[PatrimoniService] Alguna taula no existeix, intentant sense includes opcionals... ({ex.InnerException?.Message ?? ex.Message})");

            // Si falla, intentar sense arxius, inspeccions i intervencions
            var result = await _context.Patrimonis
                .Include(p => p.TipusPatrimoni)
                .Include(p => p.EstatConservacio)
                .Include(p => p.Ubicacio)
                .FirstOrDefaultAsync(p => p.Id == id && p.Actiu);

            Console.WriteLine($"[PatrimoniService] Element trobat (sense arxius/inspeccions): {result != null}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PatrimoniService] Error carregant patrimoni: {ex.Message}");
            throw;
        }
    }

    public async Task<Patrimoni?> GetByCodiInventariAsync(string codi)
    {
        return await _context.Patrimonis
            .Include(p => p.TipusPatrimoni)
            .Include(p => p.EstatConservacio)
            .Include(p => p.Ubicacio)
            .FirstOrDefaultAsync(p => p.CodiInventari == codi && p.Actiu);
    }

    public async Task<IEnumerable<Patrimoni>> GetByTipusAsync(int tipusId)
    {
        return await _context.Patrimonis
            .Include(p => p.TipusPatrimoni)
            .Include(p => p.EstatConservacio)
            .Include(p => p.Ubicacio)
            .Where(p => p.TipusPatrimoniId == tipusId && p.Actiu)
            .OrderBy(p => p.Nom)
            .ToListAsync();
    }

    public async Task<IEnumerable<Patrimoni>> GetByUbicacioAsync(int ubicacioId)
    {
        return await _context.Patrimonis
            .Include(p => p.TipusPatrimoni)
            .Include(p => p.EstatConservacio)
            .Include(p => p.Ubicacio)
            .Where(p => p.UbicacioId == ubicacioId && p.Actiu)
            .OrderBy(p => p.Nom)
            .ToListAsync();
    }

    public async Task<IEnumerable<Patrimoni>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        var term = $"%{searchTerm.ToLower()}%";

        return await _context.Patrimonis
            .Include(p => p.TipusPatrimoni)
            .Include(p => p.EstatConservacio)
            .Include(p => p.Ubicacio)
            .Where(p => p.Actiu && (
                EF.Functions.Like(p.Nom.ToLower(), term) ||
                EF.Functions.Like(p.CodiInventari.ToLower(), term) ||
                (p.NomAlternatiu != null && EF.Functions.Like(p.NomAlternatiu.ToLower(), term)) ||
                (p.DescripcioBreu != null && EF.Functions.Like(p.DescripcioBreu.ToLower(), term)) ||
                (p.AdrecaCompleta != null && EF.Functions.Like(p.AdrecaCompleta.ToLower(), term))
            ))
            .OrderBy(p => p.Nom)
            .ToListAsync();
    }

    public async Task<Patrimoni> CreateAsync(Patrimoni patrimoni)
    {
        patrimoni.CreatedAt = DateTime.Now;
        patrimoni.UpdatedAt = DateTime.Now;
        
        // Calcular valor total
        patrimoni.ValorTotal = (patrimoni.ValorHistoric + patrimoni.ValorArtistic + 
                               patrimoni.ValorArquitectonic + patrimoni.ValorSocial) / 4.0m;

        _context.Patrimonis.Add(patrimoni);
        await _context.SaveChangesAsync();
        return patrimoni;
    }

    public async Task<Patrimoni?> UpdateAsync(int id, Patrimoni patrimoni)
    {
        var existing = await _context.Patrimonis.FindAsync(id);
        if (existing == null || !existing.Actiu)
            return null;

        // Actualització automàtica de l'entity
        _context.Entry(existing).CurrentValues.SetValues(patrimoni);
        existing.UpdatedAt = DateTime.Now;
        
        // Calcular valor total
        existing.ValorTotal = (existing.ValorHistoric + existing.ValorArtistic + 
                               existing.ValorArquitectonic + existing.ValorSocial) / 4.0m;

        await _context.SaveChangesAsync();
        
        // Retornar amb navegació carregada
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var patrimoni = await _context.Patrimonis.FindAsync(id);
        if (patrimoni == null) return false;

        // Soft delete
        patrimoni.Actiu = false;
        patrimoni.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Patrimonis.AnyAsync(p => p.Id == id && p.Actiu);
    }

    public async Task<bool> ExistsCodiInventariAsync(string codi, int? excludeId = null)
    {
        var query = _context.Patrimonis.Where(p => p.CodiInventari == codi && p.Actiu);

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    private static bool ContainsTableNotFound(Exception ex)
    {
        var msg = ex.Message;
        if (msg.Contains("doesn't exist") || msg.Contains("Unknown column") || msg.Contains("Table"))
            return true;
        if (ex.InnerException != null)
            return ContainsTableNotFound(ex.InnerException);
        return false;
    }
}
