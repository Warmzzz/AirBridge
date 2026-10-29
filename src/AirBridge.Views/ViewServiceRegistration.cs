using AirBridge.Presenters.Contracts;
using AirBridge.Views.TrayIcon;
using AirBridge.Views.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace AirBridge.Views;

public static class ViewServiceRegistration
{
    public static IServiceCollection AddViews(this IServiceCollection services)
    {
        services.AddSingleton<TrayContextMenu>();
        services.AddSingleton<TrayIconManager>();
        services.AddSingleton<ITrayView>(provider => provider.GetRequiredService<TrayIconManager>());

        // One instance each, hidden instead of closed (see OnClosing).
        services.AddSingleton<SettingsWindow>();
        services.AddSingleton<ISettingsView>(provider => provider.GetRequiredService<SettingsWindow>());

        services.AddSingleton<LogsWindow>();
        services.AddSingleton<ILogsView>(provider => provider.GetRequiredService<LogsWindow>());

        return services;
    }
}
