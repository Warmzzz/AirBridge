using AirBridge.Domain.Enums;
using AirBridge.Domain.Exceptions;
using AirBridge.Domain.Interfaces;

namespace AirBridge.Application.UseCases;

public sealed class StartReceiverUseCase(IAirPlayReceiver receiver, IConfigurationRepository configRepository)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (receiver.Status is ReceiverStatus.Running or ReceiverStatus.Starting)
        {
            throw new ReceiverException("The receiver is already running.", receiver.Status);
        }

        var config = await configRepository.LoadAsync(cancellationToken);
        await receiver.StartAsync(config, cancellationToken);
    }
}