namespace AirBridge.Domain.ValueObjects;

public sealed class DeviceName
{
    private const int MaxLength = 64;

    public static DeviceName Default => new("AirBridge");
    
    public DeviceName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if(value.Length > MaxLength)
        {
            throw new ArgumentException($"Device name must be less than {MaxLength} characters.");
        }
        Value = value.Trim();
    }
    
    public string Value { get; }
    
    public bool Equals(DeviceName? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is DeviceName other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
    
    public static implicit operator string(DeviceName deviceName) => deviceName.Value;
    public static implicit operator DeviceName(string value) => new(value);
}