using AppAjuntament.Services;

namespace AppAjuntament.Jobs;

public class IngressosExternsImportJob
{
    private readonly IngressosExternsService _service;
    private readonly ILogger<IngressosExternsImportJob> _logger;

    public IngressosExternsImportJob(IngressosExternsService service, ILogger<IngressosExternsImportJob> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task ImportFonsCooperacioAsync()
    {
        try
        {
            var registres = await _service.SincronitzarFonsCooperacioAsync();
            _logger.LogInformation("FCLC sincronitzat: {Registres} registres", registres);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sincronitzant el Fons de Cooperació Local");
            throw;
        }
    }

    public async Task ImportPagamentsPendentsAsync()
    {
        try
        {
            var registres = await _service.SincronitzarPagamentsPendentsAsync();
            _logger.LogInformation("Pagaments pendents Generalitat sincronitzats: {Registres} registres", registres);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sincronitzant els pagaments pendents de la Generalitat");
            throw;
        }
    }
}
