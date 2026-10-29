using System.Reflection;
using AirBridge.Domain.Enums;
using AirBridge.Presenters.Contracts;
using AirBridge.Presenters.Models;

namespace AirBridge.Views.TrayIcon;

public sealed class TrayIconManager : ITrayView, IDisposable
{
    private static readonly Lazy<Icon> AppIcon = new(() =>
        (Environment.ProcessPath is { } path ? Icon.ExtractAssociatedIcon(path) : null) ?? SystemIcons.Application);

    private readonly NotifyIcon _notifyIcon;
    private readonly TrayContextMenu _contextMenu;
    
    public TrayIconManager(TrayContextMenu contextMenu)
    {
        _contextMenu = contextMenu;
        _notifyIcon = new NotifyIcon
        {
            Visible = true,
            Text = "AirBridge",
            Icon = LoadIcon(ReceiverStatus.Stopped)
        };
 
        // Like AudioPlaybackConnector: a left click opens the menu too. NotifyIcon only exposes this privately.
        _notifyIcon.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
                typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(_notifyIcon, null);
        };

        WireContextMenuEvents();
    }
    
    public event EventHandler? OnStartRequested;
    public event EventHandler? OnStopRequested;
    public event EventHandler? OnRestartRequested;
    public event EventHandler<string>? OnAudioDeviceSelected;
    public event EventHandler? OnOpenLogsRequested;
    public event EventHandler? OnOpenSettingsRequested;
    public event EventHandler? OnQuitRequested;
    
    public void UpdateMenu(TrayMenuModel model)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var previousMenu = _notifyIcon.ContextMenuStrip;
            _notifyIcon.Icon = LoadIcon(model.ReceiverStatus);
            _notifyIcon.ContextMenuStrip = _contextMenu.Build(model);
            previousMenu?.Dispose();

            // NotifyIcon.Text is limited to 127 characters.
            var text = $"AirBridge — {model.StatusLabel}";
            _notifyIcon.Text = text.Length > 127 ? text[..127] : text;
        });
    }

    public void ShowNotification(string title, string message)
    {
        _notifyIcon.ShowBalloonTip(
            timeout: 3000,
            tipTitle: title,
            tipText: message,
            tipIcon: ToolTipIcon.Info
        );
    }
    
    private void WireContextMenuEvents()
    {
        _contextMenu.StartClicked += (s, e) => OnStartRequested?.Invoke(s, e);
        _contextMenu.StopClicked += (s, e) => OnStopRequested?.Invoke(s, e);
        _contextMenu.RestartClicked += (s, e) => OnRestartRequested?.Invoke(s, e);
        _contextMenu.AudioDeviceSelected += (s, id) => OnAudioDeviceSelected?.Invoke(s, id);
        _contextMenu.LogsClicked += (s, e) => OnOpenLogsRequested?.Invoke(s, e);
        _contextMenu.SettingsClicked += (s, e) => OnOpenSettingsRequested?.Invoke(s, e);
        _contextMenu.QuitClicked += (s, e) => OnQuitRequested?.Invoke(s, e);
    }

    private static Icon LoadIcon(ReceiverStatus status)
    {
        var resourceName = status switch
        {
            ReceiverStatus.Running => "AirBridge.Views.Resources.Icons.tray_running.ico",
            ReceiverStatus.Error => "AirBridge.Views.Resources.Icons.tray_error.ico",
            _ => "AirBridge.Views.Resources.Icons.tray_stopped.ico"
        };
        
        using var stream = typeof(TrayIconManager).Assembly.GetManifestResourceStream(resourceName);
 
        // Per-status icons are optional; fall back to the exe's own icon (ApplicationIcon in AirBridge.App).
        return stream is not null
            ? new Icon(stream)
            : AppIcon.Value;
    }

    // Idempotent: App disposes it on the UI thread before the container does.
    public void Dispose() => _notifyIcon.Dispose();
}