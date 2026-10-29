using AirBridge.Domain.Core;
using AirBridge.Domain.Interfaces;

namespace AirBridge.Application.UseCases;

public sealed class GetAudioDeviceUseCase(IAudioDeviceService audioDeviceService, IConfigurationRepository configRepository)
{
    public IReadOnlyList<AudioDevice> Execute() => audioDeviceService.GetOutputDevices();

    public IObservable<AudioDevice> ObserveDevices() => audioDeviceService.DeviceChanged;

    public async Task<string> GetSelectedDeviceIdAsync(CancellationToken cancellationToken = default)
        => (await configRepository.LoadAsync(cancellationToken)).AudioDeviceId;
}
