using AirBridge.Presenters.Models;

namespace AirBridge.Presenters.Contracts;

public interface ITrayView
{
    event EventHandler OnStartRequested;
    event EventHandler OnStopRequested;
    event EventHandler OnRestartRequested;
    event EventHandler<string> OnAudioDeviceSelected;
    event EventHandler OnOpenLogsRequested;
    event EventHandler OnOpenSettingsRequested;
    event EventHandler OnQuitRequested;
 
    void UpdateMenu(TrayMenuModel model);
    void ShowNotification(string title, string message);
}