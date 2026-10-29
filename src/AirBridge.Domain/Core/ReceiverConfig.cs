using AirBridge.Domain.ValueObjects;

namespace AirBridge.Domain.Core;

public sealed class ReceiverConfig
{
    public ReceiverConfig(DeviceName deviceName, string audioDeviceId, int port = 5000, bool startWithWindows = false, string? logDirectory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(audioDeviceId);
        if (port is < 1024 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1024 and 65535.");
        }
        
        DeviceName = deviceName;
        AudioDeviceId = audioDeviceId;
        Port = port;
        StartWithWindows = startWithWindows;
        LogDirectory = logDirectory 
                       ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AirBridge", "Logs");
    }
    
    public DeviceName DeviceName { get; }
    public string AudioDeviceId { get; }
    public int Port { get; }
    public bool StartWithWindows { get; }
    public string LogDirectory { get; }
    
    public ReceiverConfig WithDeviceName(DeviceName name) => new(name, AudioDeviceId, Port, StartWithWindows, LogDirectory);
    public ReceiverConfig WithAudioDevice(string deviceId) => new(DeviceName, deviceId, Port, StartWithWindows, LogDirectory);
    public ReceiverConfig WithStartWithWindows(bool value) => new(DeviceName, AudioDeviceId, Port, value, LogDirectory);
}