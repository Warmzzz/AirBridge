using AirBridge.Domain.Core;

namespace AirBridge.Domain.Interfaces;

public interface IConfigurationRepository
{
    Task<ReceiverConfig> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(ReceiverConfig config, CancellationToken cancellationToken = default);
}