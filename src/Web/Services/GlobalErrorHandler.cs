using Microsoft.AspNetCore.Components.Server.Circuits;
using AppAjuntament.Services;

namespace AppAjuntament.Services
{
    public class GlobalErrorHandler : CircuitHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GlobalErrorHandler> _logger;

        public GlobalErrorHandler(IServiceProvider serviceProvider, ILogger<GlobalErrorHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Circuit opened: {CircuitId}", circuit.Id);
            return base.OnCircuitOpenedAsync(circuit, cancellationToken);
        }

        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Circuit closed: {CircuitId}", circuit.Id);
            return base.OnCircuitClosedAsync(circuit, cancellationToken);
        }

        public override async Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            await base.OnConnectionUpAsync(circuit, cancellationToken);
        }

        public override async Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            await base.OnConnectionDownAsync(circuit, cancellationToken);
        }
    }
}
