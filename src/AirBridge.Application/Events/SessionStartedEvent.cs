using AirBridge.Domain.Core;

namespace AirBridge.Application.Events;

public sealed record SessionStartedEvent(AirPlaySession Session, DateTimeOffset OccurredAt);