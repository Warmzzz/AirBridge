using AirBridge.Domain.Enums;

namespace AirBridge.Domain.Core;

public sealed class AirPlaySession
{
    public AirPlaySession(string clientName, string clientDeviceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientName);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientDeviceId);
        
        Id = Guid.NewGuid();
        ClientName = clientName;
        ClientDeviceId = clientDeviceId;
        StartedAt = DateTimeOffset.Now;
        State = ConnectionState.Connecting;
    }
    
    public Guid Id { get; }
    public string ClientName { get; }
    public string ClientDeviceId { get; }
    public DateTimeOffset StartedAt { get; }
    public ConnectionState State { get; private set; }

    public void MarkStreaming() => Transition(ConnectionState.Streaming);
    public void MarkConnected() => Transition(ConnectionState.Connected);
    public void MarkDisconnected() => Transition(ConnectionState.Disconnected);
    
    private void Transition(ConnectionState nextState) => State = nextState;
}