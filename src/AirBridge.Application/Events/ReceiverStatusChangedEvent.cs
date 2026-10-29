using AirBridge.Domain.Enums;

namespace AirBridge.Application.Events;

public sealed record ReceiverStatusChangedEvent(ReceiverStatus Previous, ReceiverStatus Current, DateTimeOffset OccurredAt);