using AirBridge.Domain.Interfaces;
using AirBridge.Infrastructure.Audio;
using AirBridge.Infrastructure.Configuration;
using AirBridge.Infrastructure.Logging;
using AirBridge.Infrastructure.Process;
using AirBridge.Infrastructure.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace AirBridge.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string appDataPath)
    {
        var configPath = Path.Combine(appDataPath, "config.json");
        var logDirectory = Path.Combine(appDataPath, "logs");
        var todayLogFile = Path.Combine(logDirectory, $"airbridge-{DateTime.Today:yyyyMMdd}.log");

        services.AddSingleton<IConfigurationRepository>(_ => new JsonConfigurationRepository(configPath));
        services.AddSingleton<IAudioDeviceService, WasapiAudioDeviceService>();
        services.AddSingleton<IProcessManager, ManagedProcessRunner>();
        services.AddSingleton<IStartupRegistrationService, WindowsStartupRegistrationService>();
        services.AddSingleton<ILogReader>(_ => new FileLogReader(todayLogFile));
        
        return services;
    }
}