using System.IO;
using AirBridge.AirPlay;
using AirBridge.Infrastructure;
using AirBridge.Infrastructure.Logging;
using AirBridge.Presenters;
using AirBridge.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace AirBridge.App;

internal static class AppHost
{
    private static readonly string AppDataPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AirBridge");

    public static IHost Build()
    {
        ConfigureSerilog();
        return Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((_, services) =>
            {
                services.AddInfrastructure(AppDataPath);
                services.AddAirPlay();
                services.AddApplicationUseCases();
                services.AddViews();
                services.AddPresenters();
                services.AddSingleton<AppBootstrapper>();
            })
            .Build();
    }

    private static void ConfigureSerilog()
    {
        var logDirectory = Path.Combine(AppDataPath, "logs");
        Directory.CreateDirectory(logDirectory);

        Log.Logger = SerilogSetup
            .CreateDefault(logDirectory)
            .CreateLogger();
    }
}