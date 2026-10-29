namespace AirBridge.Domain.Interfaces;

public interface IProcessManager : IAsyncDisposable
{
    int? ProcessId { get; }
    bool IsRunning { get; }
 
    IObservable<string> OutputReceived { get; }
    IObservable<string> ErrorReceived { get; }
    IObservable<int> ProcessExited { get; }
 
    Task StartAsync(string executable, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
