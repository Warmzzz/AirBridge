using AirBridge.Domain.Enums;
using AirBridge.Domain.Exceptions;
using AirBridge.Domain.Interfaces;

namespace AirBridge.Application.UseCases;

public sealed class StopReceiverUseCase(IAirPlayReceiver receiver)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (receiver.Status is ReceiverStatus.Stopped or ReceiverStatus.Stopping)
        {
            throw new ReceiverException("Receiver is not running.", receiver.Status);
        }
        await receiver.StopAsync(cancellationToken);
    }
}