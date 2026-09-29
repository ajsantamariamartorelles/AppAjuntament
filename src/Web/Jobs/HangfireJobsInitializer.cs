using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace AppAjuntament.Jobs;

public class HangfireJobsInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public HangfireJobsInitializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        // Comarques
        recurringJobs.AddOrUpdate<ComarquesImportJob>(
            "import-comarques",
            job => job.ImportComarquesAsync(),
            Cron.Weekly
        );
        // Municipis
        recurringJobs.AddOrUpdate<MunicipisImportJob>(
            "import-municipis",
            job => job.ImportMunicipisAsync(),
            Cron.Weekly
        );

        // Regidors (import setmanal)
        recurringJobs.AddOrUpdate<RegidorsImportJob>(
            "import-regidors",
            job => job.ImportRegidorsAsync(),
            Cron.Weekly
        );

        // Ingressos externs: les dades es desen a BDD i la pantalla no consulta APIs externes.
        recurringJobs.AddOrUpdate<IngressosExternsImportJob>(
            "import-fons-cooperacio-local",
            job => job.ImportFonsCooperacioAsync(),
            Cron.Weekly
        );
        recurringJobs.AddOrUpdate<IngressosExternsImportJob>(
            "import-pagaments-pendents-generalitat",
            job => job.ImportPagamentsPendentsAsync(),
            Cron.Weekly
        );

        // Execució immediata en arrencar per verificar funcionament (desactiva en producció si no és necessari)
        var backgroundJobClient = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
        backgroundJobClient.Enqueue<ComarquesImportJob>(job => job.ImportComarquesAsync());
        backgroundJobClient.Enqueue<MunicipisImportJob>(job => job.ImportMunicipisAsync());
        // Enqueue regidors import immediat per verificació a l'arrencar
        backgroundJobClient.Enqueue<RegidorsImportJob>(job => job.ImportRegidorsAsync());
        backgroundJobClient.Enqueue<IngressosExternsImportJob>(job => job.ImportFonsCooperacioAsync());
        backgroundJobClient.Enqueue<IngressosExternsImportJob>(job => job.ImportPagamentsPendentsAsync());

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
