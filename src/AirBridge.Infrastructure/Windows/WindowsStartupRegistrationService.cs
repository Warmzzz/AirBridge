using AirBridge.Domain.Interfaces;
using Microsoft.Win32;

namespace AirBridge.Infrastructure.Windows;

public sealed class WindowsStartupRegistrationService : IStartupRegistrationService
{
    private const string RegisterKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "AirBridge";
    private static readonly string ExecutablePath = $"\"{Environment.ProcessPath}\"";
    
    public bool IsRegistered()
    {
        using var key = OpenRunKey(writable: false);
        return key?.GetValue(AppName) is not null;
    }

    public void Register()
    {
        using var key = OpenRunKey(writable: true);
        key?.SetValue(AppName, ExecutablePath, RegistryValueKind.String);
    }

    public void Unregister()
    {
        using var key = OpenRunKey(writable: true);
        if(key?.GetValue(AppName) is not null)
        {
            key.DeleteValue(AppName);
        }
    }
    
    private static RegistryKey? OpenRunKey(bool writable) => Registry.CurrentUser.OpenSubKey(RegisterKeyPath, writable);
}