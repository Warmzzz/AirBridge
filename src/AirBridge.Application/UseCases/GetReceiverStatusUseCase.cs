using System.Reactive.Linq;
using AirBridge.Domain.Core;
using AirBridge.Domain.Enums;
using AirBridge.Domain.Interfaces;

namespace AirBridge.Application.UseCases;

public sealed class GetReceiverStatusUseCase(IAirPlayReceiver receiver)
{
    public ReceiverStatus Execute() => receiver.Status;

    public IObservable<ReceiverStatus> ObserveStatus() => receiver.StatusChanged;

    /// <summary>Emits the connected session, or null when the client disconnects.</summary>
    public IObservable<AirPlaySession?> ObserveActiveSession()
        => receiver.SessionStarted.Select(session => (AirPlaySession?)session)
            .Merge(receiver.SessionEnded.Select(_ => (AirPlaySession?)null));
}
