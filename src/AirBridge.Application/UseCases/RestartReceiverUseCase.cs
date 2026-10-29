using AirBridge.Domain.Interfaces;

namespace AirBridge.Application.UseCases;

public sealed class RestartReceiverUseCase(IAirPlayReceiver receiver, IConfigurationRepository configRepository)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var config = await configRepository.LoadAsync(cancellationToken);
        await receiver.RestartAsync(config, cancellationToken);
    }
}