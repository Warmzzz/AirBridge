using System.Reactive.Linq;
using System.Reactive.Subjects;
using AirBridge.Domain.Core;
using AirBridge.Domain.Interfaces;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace AirBridge.Infrastructure.Audio;

public sealed class WasapiAudioDeviceService : IAudioDeviceService, IMMNotificationClient, IDisposable
{
    private readonly MMDeviceEnumerator _enumerator;
    private readonly Subject<AudioDevice> _deviceChanged = new();

    public WasapiAudioDeviceService()
    {
        _enumerator = new MMDeviceEnumerator();
        _enumerator.RegisterEndpointNotificationCallback(this);
    }

    public IObservable<AudioDevice> DeviceChanged => _deviceChanged.AsObservable();
    
    public IReadOnlyList<AudioDevice> GetOutputDevices()
    {
        var devices = _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
        var defaultDevice = GetDefaultMmDevice();
        return devices
            .Select(d => ToAudioDevice(d, defaultDevice?.ID))
            .ToList();
    }

    public AudioDevice? GetDefaultDevice()
    {
        var device = GetDefaultMmDevice();
        return (device is null) ? null : ToAudioDevice(device, device.ID);
    }

    public void SetActiveDevice(AudioDevice device)
    {
        // WASAPI doesn't support programmatic default device changes via public API.
        // This is handled by the AirPlay layer passing the device ID to the backend process.
        // The active device selection is persisted in config and applied on next receiver start.
    }
    
    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
    {
        if (flow != DataFlow.Render || role != Role.Multimedia) return;
        PublishDeviceChange(defaultDeviceId);
    }

    // Plugging / unplugging a headset changes the endpoint state, so the tray list must be refreshed.
    public void OnDeviceStateChanged(string deviceId, DeviceState newState) => PublishDeviceChange(deviceId);

    public void OnDeviceAdded(string pwstrDeviceId) { }
    public void OnDeviceRemoved(string deviceId) { }
    public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }

    private void PublishDeviceChange(string deviceId)
    {
        try
        {
            var device = _enumerator.GetDevice(deviceId);
            if (device.DataFlow != DataFlow.Render) return;
            _deviceChanged.OnNext(ToAudioDevice(device, GetDefaultMmDevice()?.ID));
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or ArgumentException)
        {
            // Device may have been removed; ignore
        }
    }

    private MMDevice? GetDefaultMmDevice()
    {
        try
        {
            return _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }
        catch
        {
            return null;
        }
    }
    
    private static AudioDevice ToAudioDevice(MMDevice device, string? defaultDeviceId) => new(device.ID, device.FriendlyName, device.ID == defaultDeviceId);

    public void Dispose()
    {
        _enumerator.UnregisterEndpointNotificationCallback(this);
        _enumerator.Dispose();
        _deviceChanged.Dispose();
    }
}