using AirBridge.Domain.Enums;
using AirBridge.Domain.Interfaces;
using AirBridge.Domain.ValueObjects;

namespace AirBridge.Application.UseCases;

public sealed class UpdateReceiverConfigUseCase(IConfigurationRepository configRepository, IStartupRegistrationService startupRegistration, IAirPlayReceiver receiver)
{
    public async Task ExecuteAsync(UpdateReceiverConfigRequest request, CancellationToken cancellationToken = default)
    {
        var currentConfig = await configRepository.LoadAsync(cancellationToken);
        var updatedConfig = currentConfig;

        if (request.DeviceName is not null)
        {
            updatedConfig = updatedConfig.WithDeviceName(new DeviceName(request.DeviceName));
        }

        if (request.StartWithWindows is not null)
        {
            updatedConfig = updatedConfig.WithStartWithWindows(request.StartWithWindows.Value);
            SyncStartupRegistration(request.StartWithWindows.Value);
        }

        await configRepository.SaveAsync(updatedConfig, cancellationToken);

        // The AirPlay name is advertised at start, so the iPhone only sees a rename after a restart.
        if (!updatedConfig.DeviceName.Equals(currentConfig.DeviceName) && receiver.Status is ReceiverStatus.Running)
        {
            await receiver.RestartAsync(updatedConfig, cancellationToken);
        }
    }

    private void SyncStartupRegistration(bool enable)
    {
        if (enable)
        {
            startupRegistration.Register();
        }
        else
        {
            startupRegistration.Unregister();
        }
    }
}
