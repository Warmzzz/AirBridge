using AirBridge.Domain.Core;

namespace AirBridge.Application.Events;

public sealed record SessionEndedEvent(AirPlaySession Session, DateTimeOffset OccurredAt);