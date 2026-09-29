using Microsoft.EntityFrameworkCore;
using AppAjuntament.Models;

namespace AppAjuntament.Services;

/// <summary>
/// Servei per gestionar catàlegs (tipus patrimoni, estats conservació, ubicacions)
/// </summary>
public interface ICatalogService
{
    // Tipus Patrimoni
    Task<IEnumerable<TipusPatrimoni>> GetTipusPatrimoniAsync();
    Task<TipusPatrimoni?> GetTipusPatrimoniByIdAsync(int id);

    // Subtipus Patrimoni
    /// <summary>
    /// Returns the list of subtypes (names) for a given TipusPatrimoniId.
    /// </summary>
    Task<List<string>> GetSubtipusPatrimoniAsync(int tipusPatrimoniId);

    // Estats Conservació
    Task<IEnumerable<EstatConservacio>> GetEstatsConservacioAsync();
    Task<EstatConservacio?> GetEstatConservacioByIdAsync(int id);

    // Ubicacions
    Task<IEnumerable<Ubicacio>> GetUbicacionsAsync();
    Task<IEnumerable<Ubicacio>> GetDistrictesAsync();
    Task<IEnumerable<Ubicacio>> GetBarrisByDistricteAsync(int districteId);
    // Tipus Arxiu
    Task<IEnumerable<TipusArxiu>> GetTipusArxiuAsync();
}

public class CatalogService : ICatalogService
{
    private readonly GestorSubvencionsContext _context;

    public CatalogService(GestorSubvencionsContext context)
    {
        _context = context;
    }

    // Tipus Patrimoni
    public async Task<IEnumerable<TipusPatrimoni>> GetTipusPatrimoniAsync()
    {
        return await _context.TipusPatrimonis
            .Where(t => t.Actiu)
            .OrderBy(t => t.Nom)
            .ToListAsync();
    }

    // Subtipus Patrimoni
    public async Task<List<string>> GetSubtipusPatrimoniAsync(int tipusPatrimoniId)
    {
        return await _context.SubtipusPatrimonis
            .Where(s => s.TipusPatrimoniId == tipusPatrimoniId && s.Actiu)
            .OrderBy(s => s.Nom)
            .Select(s => s.Nom)
            .ToListAsync();
    }

    public async Task<TipusPatrimoni?> GetTipusPatrimoniByIdAsync(int id)
    {
        return await _context.TipusPatrimonis
            .FirstOrDefaultAsync(t => t.Id == id && t.Actiu);
    }

    // Estats Conservació
    public async Task<IEnumerable<EstatConservacio>> GetEstatsConservacioAsync()
    {
        return await _context.EstatsConservacio
            .Where(e => e.Actiu)
            .OrderBy(e => e.PrioritatIntervencio)
            .ToListAsync();
    }

    public async Task<EstatConservacio?> GetEstatConservacioByIdAsync(int id)
    {
        return await _context.EstatsConservacio
            .FirstOrDefaultAsync(e => e.Id == id && e.Actiu);
    }

    // Ubicacions
    public async Task<IEnumerable<Ubicacio>> GetUbicacionsAsync()
    {
        return await _context.Ubicacions
            .Include(u => u.Parent)
            .Where(u => u.Actiu)
            .OrderBy(u => u.Parent != null ? u.Parent.Nom : u.Nom)
            .ThenBy(u => u.Nom)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ubicacio>> GetDistrictesAsync()
    {
        return await _context.Ubicacions
            .Where(u => u.Actiu && u.Tipus == TipusUbicacio.Districte)
            .OrderBy(u => u.Nom)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ubicacio>> GetBarrisByDistricteAsync(int districteId)
    {
        return await _context.Ubicacions
            .Where(u => u.Actiu && u.ParentId == districteId)
            .OrderBy(u => u.Nom)
            .ToListAsync();
    }

    public async Task<IEnumerable<TipusArxiu>> GetTipusArxiuAsync()
    {
        return await _context.TipusArxius
            .Where(t => t.Actiu)
            .OrderBy(t => t.Nom)
            .ToListAsync();
    }
}
