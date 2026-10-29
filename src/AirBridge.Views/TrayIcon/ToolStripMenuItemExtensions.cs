namespace AirBridge.Views.TrayIcon;

internal static class ToolStripMenuItemExtensions
{
    internal static T Also<T>(this T item, Action<T> action)
    {
        action(item);
        return item;
    }
}