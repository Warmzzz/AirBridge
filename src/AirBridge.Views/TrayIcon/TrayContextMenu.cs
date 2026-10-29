using AirBridge.Presenters.Models;

namespace AirBridge.Views.TrayIcon;

public sealed class TrayContextMenu
{
    public event EventHandler? StartClicked;
    public event EventHandler? StopClicked;
    public event EventHandler? RestartClicked;
    public event EventHandler<string>? AudioDeviceSelected;
    public event EventHandler? LogsClicked;
    public event EventHandler? SettingsClicked;
    public event EventHandler? QuitClicked;
 
    public ContextMenuStrip Build(TrayMenuModel model)
    {
        ContextMenuStrip menu = new();
 
        menu.Items.Add(BuildStatusItem(model.StatusLabel));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(BuildItem("Start",   enabled: model.CanStart,   handler: (_, _) => StartClicked?.Invoke(this, EventArgs.Empty)));
        menu.Items.Add(BuildItem("Stop",    enabled: model.CanStop,    handler: (_, _) => StopClicked?.Invoke(this, EventArgs.Empty)));
        menu.Items.Add(BuildItem("Restart", enabled: model.CanRestart, handler: (_, _) => RestartClicked?.Invoke(this, EventArgs.Empty)));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(BuildAudioDevicesMenu(model));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(BuildItem("Open Logs",  enabled: true, handler: (_, _) => LogsClicked?.Invoke(this, EventArgs.Empty)));
        menu.Items.Add(BuildItem("Settings",   enabled: true, handler: (_, _) => SettingsClicked?.Invoke(this, EventArgs.Empty)));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(BuildItem("Quit",       enabled: true, handler: (_, _) => QuitClicked?.Invoke(this, EventArgs.Empty)));
 
        return menu;
    }
 
    private static ToolStripMenuItem BuildStatusItem(string label) =>
        new($"● {label}") { Enabled = false };

    private static ToolStripMenuItem BuildItem(string text, bool enabled, EventHandler handler)
        => new ToolStripMenuItem(text)
        {
            Enabled = enabled,
            Tag = handler
        }.Also(item => item.Click += handler);
 
    private ToolStripMenuItem BuildAudioDevicesMenu(TrayMenuModel model)
    {
        ToolStripMenuItem parent = new("Output Device");
        if (model.AudioDevices.Count == 0)
        {
            parent.DropDownItems.Add(new ToolStripMenuItem("No devices found") { Enabled = false });
            return parent;
        }
 
        foreach (var device in model.AudioDevices)
        {
            var isActive = device.Id == model.ActiveAudioDeviceId;
            var label = isActive ? $"✓ {device.Name}" : device.Name;
            var capturedId = device.Id;
 
            parent.DropDownItems.Add(BuildItem(label, enabled: !isActive,
                handler: (_, _) => AudioDeviceSelected?.Invoke(this, capturedId)));
        }
 
        return parent;
    }
}