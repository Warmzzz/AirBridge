using System.Windows;
using AirBridge.Presenters;
using AirBridge.Views.TrayIcon;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace AirBridge.App;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += (_, args) =>
        {
            Log.Error(args.Exception, "Unhandled UI exception.");
            args.Handled = true;
        };

        try
        {
            _host = AppHost.Build();
            await _host.StartAsync();

            await _host.Services.GetRequiredService<TrayPresenter>().InitializeAsync();
            await _host.Services.GetRequiredService<AppBootstrapper>().RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "AirBridge failed to start.");
            System.Windows.MessageBox.Show($"AirBridge failed to start:\n{ex.Message}", "AirBridge", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            // The tray icon belongs to the UI thread; remove it before the container disposes it elsewhere.
            _host.Services.GetRequiredService<TrayIconManager>().Dispose();

            // Block (off the UI thread, to avoid dispatcher deadlocks) so the AirPlay process is killed before we exit.
            var host = _host;
            Task.Run(async () =>
            {
                await host.StopAsync();
                if (host is IAsyncDisposable asyncDisposable) await asyncDisposable.DisposeAsync();
                else host.Dispose();
            }).GetAwaiter().GetResult();
        }

        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
