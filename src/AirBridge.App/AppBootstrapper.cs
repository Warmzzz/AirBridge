using AirBridge.Application.Services;
using Microsoft.Extensions.Logging;

namespace AirBridge.App;

internal sealed class AppBootstrapper
{
    private readonly ReceiverOrchestrationService _orchestration;
    private readonly ILogger<AppBootstrapper> _logger;

    public AppBootstrapper(ReceiverOrchestrationService orchestration, ILogger<AppBootstrapper> logger)
    {
        _orchestration = orchestration;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AirBridge starting.");

        try
        {
            await _orchestration.StartAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to auto-start receiver on launch.");
        }
    }
}
