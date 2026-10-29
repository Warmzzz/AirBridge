using System.Text.Json;
using AirBridge.Domain.Core;
using AirBridge.Domain.Interfaces;
using AirBridge.Domain.ValueObjects;

namespace AirBridge.Infrastructure.Configuration;

public sealed class JsonConfigurationRepository : IConfigurationRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly string _filePath;

    public JsonConfigurationRepository(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = filePath;
    }

    public async Task<ReceiverConfig> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return CreateDefault();
        }

        await using var stream = File.OpenRead(_filePath);
        var dto = await JsonSerializer.DeserializeAsync<ReceiverConfigDto>(stream, JsonOptions, cancellationToken) ?? new ReceiverConfigDto();
        return ToEntity(dto);
    }

    public async Task SaveAsync(ReceiverConfig config, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }
        var dto = ToDto(config);

        await using var stream = File.Open(_filePath, FileMode.Create, FileAccess.Write);
        await JsonSerializer.SerializeAsync(stream, dto, JsonOptions, cancellationToken);
    }

    private static ReceiverConfig CreateDefault()
    {
        var defaultDevice = new AudioDevice("default", "Default Device", isDefault: true);
        return new ReceiverConfig(DeviceName.Default, defaultDevice.Id);
    }

    private static ReceiverConfig ToEntity(ReceiverConfigDto dto) => new(
        new DeviceName(string.IsNullOrWhiteSpace(dto.DeviceName) ? "AirBridge" : dto.DeviceName),
        string.IsNullOrWhiteSpace(dto.AudioDeviceId) ? "default" : dto.AudioDeviceId,
        dto.Port,
        dto.StartWithWindows,
        dto.LogDirectory
    );

    private static ReceiverConfigDto ToDto(ReceiverConfig config) => new()
    {
        DeviceName = config.DeviceName.Value,
        AudioDeviceId = config.AudioDeviceId,
        Port = config.Port,
        StartWithWindows = config.StartWithWindows,
        LogDirectory = config.LogDirectory
    };

}