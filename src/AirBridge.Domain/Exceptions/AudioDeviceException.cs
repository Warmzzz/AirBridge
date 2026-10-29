namespace AirBridge.Domain.Exceptions;

public sealed class AudioDeviceException : DomainException
{
    public string? DeviceId { get; }
 
    public AudioDeviceException(string message, string? deviceId = null)
        : base(message)
    {
        DeviceId = deviceId;
    }
 
    public AudioDeviceException(string message, string? deviceId, Exception inner)
        : base(message, inner)
    {
        DeviceId = deviceId;
    }
}
