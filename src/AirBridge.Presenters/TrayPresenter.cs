using System.Reactive.Linq;
using AirBridge.Application.UseCases;
using AirBridge.Domain.Core;
using AirBridge.Domain.Enums;
using AirBridge.Presenters.Contracts;
using AirBridge.Presenters.Models;
using Microsoft.Extensions.Logging;

namespace AirBridge.Presenters;

public sealed class TrayPresenter(
    ITrayView view,
    StartReceiverUseCase start,
    StopReceiverUseCase stop,
    RestartReceiverUseCase restart,
    GetReceiverStatusUseCase getStatus,
    GetAudioDeviceUseCase getDevices,
    SelectAudioDeviceUseCase selectDevice,
    LogsPresenter logsPresenter,
    SettingsPresenter settingsPresenter,
    ILogger<TrayPresenter> logger) : IAsyncDisposable
{
    // Matches the id stored in config when the user never picked a device.
    private const string SystemDefaultDeviceId = "default";

    private readonly List<IDisposable> _subscriptions = [];
    private string? _activeDeviceId;
    private string? _connectedClient;

    /// <summary>Must be called on the UI thread: receiver events are marshalled back to it.</summary>
    public async Task InitializeAsync()
    {
        var uiContext = SynchronizationContext.Current
                        ?? throw new InvalidOperationException("TrayPresenter must be initialized on the UI thread.");

        _activeDeviceId = await getDevices.GetSelectedDeviceIdAsync();

        BindViewEvents();
        _subscriptions.Add(getStatus.ObserveStatus().ObserveOn(uiContext).Subscribe(OnStatusChanged));
        _subscriptions.Add(getStatus.ObserveActiveSession().ObserveOn(uiContext).Subscribe(OnActiveSessionChanged));
        _subscriptions.Add(getDevices.ObserveDevices().ObserveOn(uiContext).Subscribe(_ => RefreshMenu()));
        RefreshMenu();
    }

    private void BindViewEvents()
    {
        view.OnStartRequested        += async (_, _) => await ExecuteSafeAsync(start.ExecuteAsync);
        view.OnStopRequested         += async (_, _) => await ExecuteSafeAsync(stop.ExecuteAsync);
        view.OnRestartRequested      += async (_, _) => await ExecuteSafeAsync(restart.ExecuteAsync);
        view.OnAudioDeviceSelected   += async (_, id) => await ExecuteSafeAsync(ct => SelectDeviceAsync(id, ct));
        view.OnOpenLogsRequested     += async (_, _) => await ExecuteSafeAsync(logsPresenter.OpenAsync);
        view.OnOpenSettingsRequested += async (_, _) => await ExecuteSafeAsync(settingsPresenter.OpenAsync);
        view.OnQuitRequested         += (_, _) => OnQuitRequested();
    }

    private async Task SelectDeviceAsync(string deviceId, CancellationToken cancellationToken)
    {
        await selectDevice.ExecuteAsync(deviceId, cancellationToken);
        _activeDeviceId = deviceId;
        RefreshMenu();
    }

    private void OnStatusChanged(ReceiverStatus status)
    {
        RefreshMenu();
        var message = status switch
        {
            ReceiverStatus.Running => "Ready to receive AirPlay audio.",
            ReceiverStatus.Error => "The AirPlay receiver failed. See logs for details.",
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(message))
        {
            view.ShowNotification("AirBridge", message);
        }
    }

    private void OnActiveSessionChanged(AirPlaySession? session)
    {
        var previousClient = _connectedClient;
        _connectedClient = session?.ClientName;
        RefreshMenu();

        if (session is not null)
        {
            view.ShowNotification("AirBridge", $"{session.ClientName} connected.");
        }
        else if (previousClient is not null)
        {
            view.ShowNotification("AirBridge", $"{previousClient} disconnected.");
        }
    }

    private void RefreshMenu()
    {
        var devices = getDevices.Execute();
        var activeDeviceId = _activeDeviceId == SystemDefaultDeviceId
            ? devices.FirstOrDefault(d => d.IsDefault)?.Id
            : _activeDeviceId;

        TrayMenuModel model = new(
            ReceiverStatus: getStatus.Execute(),
            AudioDevices: devices,
            ActiveAudioDeviceId: activeDeviceId,
            ConnectedClient: _connectedClient
        );
        view.UpdateMenu(model);
    }

    private static void OnQuitRequested()
    {
        System.Windows.Application.Current.Shutdown();
    }

    private async Task ExecuteSafeAsync(Func<CancellationToken, Task> action)
    {
        try
        {
            await action(CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Tray action failed.");
            view.ShowNotification("AirBridge — Error", ex.Message);
        }
    }

    public ValueTask DisposeAsync()
    {
        foreach (var sub in _subscriptions)
        {
            sub.Dispose();
        }
        _subscriptions.Clear();
        return ValueTask.CompletedTask;
    }
}
