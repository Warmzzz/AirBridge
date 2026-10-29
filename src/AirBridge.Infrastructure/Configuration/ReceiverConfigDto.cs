namespace AirBridge.Infrastructure.Configuration;

internal sealed class ReceiverConfigDto
{
    public string DeviceName { get; set; } = "AirBridge";
    public string AudioDeviceId { get; set; } = string.Empty;
    public int Port { get; set; } = 5000;
    public bool StartWithWindows { get; set; } = false;
    public string? LogDirectory { get; set; }
}