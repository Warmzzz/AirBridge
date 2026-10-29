namespace AirBridge.Domain.Interfaces;

public interface ILogReader
{
    Task<IReadOnlyList<string>> ReadLastLinesAsync(int count, CancellationToken cancellationToken = default);
    IObservable<string> LiveLines { get; }
    string LogFilePath { get; }
}
