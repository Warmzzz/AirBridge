namespace AirBridge.Application.UseCases;

public record UpdateReceiverConfigRequest(string? DeviceName, bool? StartWithWindows);