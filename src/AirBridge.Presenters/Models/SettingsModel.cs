namespace AirBridge.Presenters.Models;

public sealed record SettingsModel(
    string DeviceName,
    bool StartWithWindows
);
