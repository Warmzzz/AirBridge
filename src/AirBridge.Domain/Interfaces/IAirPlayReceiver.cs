using AirBridge.Domain.Core;
using AirBridge.Domain.Enums;

namespace AirBridge.Domain.Interfaces;

public interface IAirPlayReceiver
{
    ReceiverStatus Status { get; }
    IObservable<ReceiverStatus> StatusChanged { get; }
    IObservable<AirPlaySession> SessionStarted { get; }
    IObservable<AirPlaySession> SessionEnded { get; }
 
    Task StartAsync(ReceiverConfig config, CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    Task RestartAsync(ReceiverConfig config, CancellationToken cancellationToken = default);
}