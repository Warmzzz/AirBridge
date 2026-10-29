using AirBridge.Domain.Core;
using AirBridge.Domain.Enums;

namespace AirBridge.Presenters.Models;

public sealed record TrayMenuModel(ReceiverStatus ReceiverStatus, IReadOnlyList<AudioDevice> AudioDevices, string? ActiveAudioDeviceId, string? ConnectedClient)
{
    public bool CanStart => ReceiverStatus is ReceiverStatus.Stopped or ReceiverStatus.Error;
    public bool CanStop => ReceiverStatus is ReceiverStatus.Running;
    public bool CanRestart => ReceiverStatus is ReceiverStatus.Running or ReceiverStatus.Error;

    public string StatusLabel => ReceiverStatus switch
    {
        ReceiverStatus.Running when ConnectedClient is not null => $"Connected: {ConnectedClient}",
        ReceiverStatus.Stopped => "Stopped",
        ReceiverStatus.Starting => "Starting…",
        ReceiverStatus.Running => "Waiting for a device",
        ReceiverStatus.Stopping => "Stopping…",
        ReceiverStatus.Error => "Error",
        _ => "Unknown"
    };
}
