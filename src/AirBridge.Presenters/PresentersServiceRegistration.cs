using Microsoft.Extensions.DependencyInjection;

namespace AirBridge.Presenters;

public static class PresentersServiceRegistration
{
    public static IServiceCollection AddPresenters(this IServiceCollection services)
    {
        services.AddSingleton<TrayPresenter>();
        services.AddSingleton<SettingsPresenter>();
        services.AddSingleton<LogsPresenter>();

        return services;
    }
}