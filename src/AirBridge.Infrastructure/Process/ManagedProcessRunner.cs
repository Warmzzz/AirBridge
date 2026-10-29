using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AirBridge.Domain.Interfaces;

namespace AirBridge.Infrastructure.Process;

public sealed class ManagedProcessRunner : IProcessManager
{
    private static readonly TimeSpan ExitTimeout = TimeSpan.FromSeconds(5);

    private System.Diagnostics.Process? _process;
    private readonly Subject<string> _outputReceived = new();
    private readonly Subject<string> _errorReceived = new();
    private readonly Subject<int> _processExited = new();

    public int? ProcessId => _process?.Id;
    public bool IsRunning => _process is { HasExited: false };

    public IObservable<string> OutputReceived => _outputReceived.AsObservable();
    public IObservable<string> ErrorReceived => _errorReceived.AsObservable();
    public IObservable<int> ProcessExited => _processExited.AsObservable();

    public Task StartAsync(string executable, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
    {
        if (IsRunning)
        {
            throw new InvalidOperationException("Cannot start a process that is already running");
        }

        KillLeftovers(executable);

        ProcessStartInfo startInfo = new()
        {
            FileName = executable,
            WorkingDirectory = Path.GetDirectoryName(executable),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        _process?.Dispose();
        var process = new System.Diagnostics.Process { StartInfo = startInfo, EnableRaisingEvents = true };
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null) _outputReceived.OnNext(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null) _errorReceived.OnNext(e.Data);
        };
        process.Exited += (_, _) => _processExited.OnNext(process.ExitCode);

        _process = process;
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_process is null || _process.HasExited) return;

        // Console processes without a window ignore CloseMainWindow, so kill directly.
        _process.Kill(entireProcessTree: true);

        using CancellationTokenSource timeout = new(ExitTimeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
        try
        {
            await _process.WaitForExitAsync(linked.Token);
        }
        catch (OperationCanceledException)
        {
            // Process is being torn down by the OS; nothing more we can do.
        }
    }

    // A previous AirBridge crash can leave the native process alive, holding its ports and AirPlay name.
    private static void KillLeftovers(string executable)
    {
        foreach (var leftover in System.Diagnostics.Process.GetProcessesByName(Path.GetFileNameWithoutExtension(executable)))
        {
            using (leftover)
            {
                try
                {
                    leftover.Kill(entireProcessTree: true);
                    leftover.WaitForExit(ExitTimeout);
                }
                catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
                {
                    // Already exited or not ours to kill.
                }
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _process?.Dispose();
        _outputReceived.Dispose();
        _errorReceived.Dispose();
        _processExited.Dispose();
    }
}
