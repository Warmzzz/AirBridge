using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AirBridge.Domain.Core;
using AirBridge.Domain.Enums;
using AirBridge.Domain.Exceptions;
using AirBridge.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AirBridge.AirPlay.UxPlay;

public sealed class UxPlayReceiver(IProcessManager processManager, ILogger<UxPlayReceiver> logger) : IAirPlayReceiver, IAsyncDisposable
{
    // Windows NTSTATUS STATUS_DLL_NOT_FOUND: uxplay.exe was started outside MSYS2 without its DLLs next to it.
    private const int DllNotFoundExitCode = unchecked((int)0xC0000135);

    private readonly BehaviorSubject<ReceiverStatus> _status = new(ReceiverStatus.Stopped);
    private readonly Subject<AirPlaySession> _sessionStarted = new();
    private readonly Subject<AirPlaySession> _sessionEnded = new();

    // stdout, stderr and exit notifications arrive on different threads.
    private readonly object _gate = new();
    private readonly HashSet<int> _openSockets = [];
    private AirPlaySession? _session;
    private IDisposable? _processSubscriptions;

    public ReceiverStatus Status => _status.Value;
    public IObservable<ReceiverStatus> StatusChanged => _status.AsObservable();
    public IObservable<AirPlaySession> SessionStarted => _sessionStarted.AsObservable();
    public IObservable<AirPlaySession> SessionEnded => _sessionEnded.AsObservable();

    public async Task StartAsync(ReceiverConfig config, CancellationToken cancellationToken = default)
    {
        if (_status.Value is ReceiverStatus.Running or ReceiverStatus.Starting)
        {
            throw new ReceiverException("Receiver is already running.", _status.Value);
        }

        _status.OnNext(ReceiverStatus.Starting);
        logger.LogInformation("Starting UxPlay receiver as '{Name}' on audio device '{Device}'.", config.DeviceName.Value, config.AudioDeviceId);

        try
        {
            var executablePath = UxPlayLocator.Locate();
            SubscribeToProcessEvents();
            await processManager.StartAsync(executablePath, UxPlayArgumentBuilder.Build(config), cancellationToken);

            _status.OnNext(ReceiverStatus.Running);
            logger.LogInformation("UxPlay receiver started ({Path}).", executablePath);
        }
        catch (Exception ex)
        {
            DisposeSubscriptions();
            _status.OnNext(ReceiverStatus.Error);
            logger.LogError(ex, "Failed to start UxPlay receiver.");
            throw new ReceiverException($"Failed to start UxPlay receiver: {ex.Message}", ReceiverStatus.Starting, ex);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_status.Value is ReceiverStatus.Stopped or ReceiverStatus.Stopping) return;

        _status.OnNext(ReceiverStatus.Stopping);
        logger.LogInformation("Stopping UxPlay receiver.");

        DisposeSubscriptions();
        await processManager.StopAsync(cancellationToken);

        lock (_gate)
        {
            _openSockets.Clear();
            EndSession();
        }
        _status.OnNext(ReceiverStatus.Stopped);
        logger.LogInformation("UxPlay receiver stopped.");
    }

    public async Task RestartAsync(ReceiverConfig config, CancellationToken cancellationToken = default)
    {
        await StopAsync(cancellationToken);
        await StartAsync(config, cancellationToken);
    }

    private void SubscribeToProcessEvents()
    {
        // After a crash the old subscriptions are still live (only StopAsync clears them); never stack a second set.
        DisposeSubscriptions();
        _processSubscriptions = new CompositeDisposable(
            processManager.OutputReceived.Merge(processManager.ErrorReceived).Subscribe(OnOutputLine),
            processManager.ProcessExited.Subscribe(OnProcessExited));
    }

    private void OnOutputLine(string line)
    {
        logger.LogDebug("[uxplay] {Line}", line);

        lock (_gate)
        {
            if (UxPlayOutputParser.TryParseSocketOpened(line, out var opened))
            {
                _openSockets.Add(opened);
            }
            else if (UxPlayOutputParser.TryParseSocketClosed(line, out var closed))
            {
                // The client keeps several sockets open while streaming; the session ends with the last one.
                if (_openSockets.Remove(closed) && _openSockets.Count == 0)
                {
                    EndSession();
                }
            }
            else if (UxPlayOutputParser.TryParseClientRequest(line, out var clientName, out var deviceId))
            {
                StartSession(clientName, deviceId);
            }
        }
    }

    private void StartSession(string clientName, string deviceId)
    {
        if (_session?.ClientDeviceId == deviceId) return;
        EndSession();

        _session = new AirPlaySession(clientName, deviceId);
        _session.MarkStreaming();

        logger.LogInformation("AirPlay session started from '{Client}' ({DeviceId}).", clientName, deviceId);
        _sessionStarted.OnNext(_session);
    }

    private void EndSession()
    {
        if (_session is null) return;

        var session = _session;
        _session = null;
        session.MarkDisconnected();

        logger.LogInformation("AirPlay session ended from '{Client}'.", session.ClientName);
        _sessionEnded.OnNext(session);
    }

    private void OnProcessExited(int exitCode)
    {
        if (exitCode == DllNotFoundExitCode)
        {
            logger.LogError("uxplay.exe could not load its DLLs (exit code 0xC0000135). Run it from C:\\msys64\\ucrt64\\bin.");
        }
        else
        {
            logger.LogWarning("UxPlay process exited with code {ExitCode}.", exitCode);
        }

        lock (_gate)
        {
            _openSockets.Clear();
            EndSession();
        }
        if (_status.Value is not ReceiverStatus.Stopping and not ReceiverStatus.Stopped)
        {
            _status.OnNext(ReceiverStatus.Error);
        }
    }

    private void DisposeSubscriptions()
    {
        _processSubscriptions?.Dispose();
        _processSubscriptions = null;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _status.Dispose();
        _sessionStarted.Dispose();
        _sessionEnded.Dispose();
    }
}
