namespace AirBridge.Domain.Core;

public sealed class AudioDevice
{
    public AudioDevice(string id, string name, bool isDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
        IsDefault = isDefault;
    }
    
    public string Id { get; }
    public string Name { get; }
    public bool IsDefault { get; }

    public override bool Equals(object? obj)
        => obj is AudioDevice other && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();
    
    public override string ToString() => Name;
}