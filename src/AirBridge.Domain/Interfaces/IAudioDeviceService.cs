using AirBridge.Domain.Core;

namespace AirBridge.Domain.Interfaces;

public interface IAudioDeviceService
{
    IReadOnlyList<AudioDevice> GetOutputDevices();
    AudioDevice? GetDefaultDevice();
    void SetActiveDevice(AudioDevice device);
    IObservable<AudioDevice> DeviceChanged { get; }
}