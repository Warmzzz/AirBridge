using AirBridge.Domain.Core;

namespace AirBridge.Application.Events;

public sealed record AudioDeviceChangedEvent(AudioDevice? Previous, AudioDevice Current, DateTimeOffset OccurredAt);