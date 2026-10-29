using AirBridge.Domain.Enums;
using AirBridge.Domain.Exceptions;
using AirBridge.Domain.Interfaces;

namespace AirBridge.Application.UseCases;

public sealed class SelectAudioDeviceUseCase(IAudioDeviceService audioDeviceService, IConfigurationRepository configRepository, IAirPlayReceiver receiver)
{
    public async Task ExecuteAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);

        var devices = audioDeviceService.GetOutputDevices();
        var device = devices.FirstOrDefault(d => d.Id == deviceId) ??
                     throw new AudioDeviceException($"Device '{deviceId}' not found.", deviceId);

        audioDeviceService.SetActiveDevice(device);
        var config = (await configRepository.LoadAsync(cancellationToken)).WithAudioDevice(deviceId);
        await configRepository.SaveAsync(config, cancellationToken);

        // The audio sink is chosen when the native receiver starts, so a running one must be restarted.
        if (receiver.Status is ReceiverStatus.Running)
        {
            await receiver.RestartAsync(config, cancellationToken);
        }
    }
}
