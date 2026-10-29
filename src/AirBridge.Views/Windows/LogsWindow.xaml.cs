using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using AirBridge.Domain.Interfaces;
using AirBridge.Presenters.Contracts;

namespace AirBridge.Views.Windows;

public partial class LogsWindow : Window, ILogsView
{
    private readonly ILogReader _logReader;

    public new event EventHandler? OnClosed;

    public LogsWindow(ILogReader logReader)
    {
        _logReader = logReader;
        InitializeComponent();
    }

    public void Show(IReadOnlyList<string> initialLines)
    {
        LogContent.Text = string.Join(Environment.NewLine, initialLines);
        Show();
        Activate();
        ScrollToBottom();
    }

    public void AppendLine(string line)
    {
        LogContent.Text += Environment.NewLine + line;
        ScrollToBottom();
    }

    public new void Close() => base.Close();

    // Single window instance for the app lifetime: hide it so the tray can show it again.
    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
        OnClosed?.Invoke(this, EventArgs.Empty);
    }

    private void ScrollToBottom() =>
        Scroller.ScrollToBottom();

    private void OnClearClick(object sender, RoutedEventArgs e) =>
        LogContent.Text = string.Empty;

    private void OnOpenFileClick(object sender, RoutedEventArgs e)
    {
        if (File.Exists(_logReader.LogFilePath))
            Process.Start(new ProcessStartInfo(_logReader.LogFilePath) { UseShellExecute = true });
    }
}
