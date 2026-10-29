using AirBridge.Presenters.Models;

namespace AirBridge.Presenters.Contracts;

public interface ISettingsView
{
    event EventHandler<SettingsModel> OnSaveRequested;
    event EventHandler OnClosed;
 
    void Show(SettingsModel model);
    void ShowError(string message);
    void Close();
}
