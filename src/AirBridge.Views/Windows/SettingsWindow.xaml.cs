using System.ComponentModel;
using System.Windows;
using AirBridge.Presenters.Contracts;
using AirBridge.Presenters.Models;
namespace AirBridge.Views.Windows;

public partial class SettingsWindow : Window, ISettingsView
{
    public event EventHandler<SettingsModel>? OnSaveRequested;
    public new event EventHandler? OnClosed;

    public SettingsWindow()
    {
        InitializeComponent();
    }

    public void Show(SettingsModel model)
    {
        DeviceNameBox.Text = model.DeviceName;
        StartWithWindowsBox.IsChecked = model.StartWithWindows;
        Show();
        Activate();
    }

    public void ShowError(string message) =>
        System.Windows.MessageBox.Show(this, message, "AirBridge", MessageBoxButton.OK, MessageBoxImage.Error);

    public new void Close()
    {
        base.Close();
    }

    // Single window instance for the app lifetime: hide it so the tray can show it again.
    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
        OnClosed?.Invoke(this, EventArgs.Empty);
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        SettingsModel model = new(
            DeviceName: DeviceNameBox.Text.Trim(),
            StartWithWindows: StartWithWindowsBox.IsChecked ?? false
        );

        OnSaveRequested?.Invoke(this, model);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => Close();
}
