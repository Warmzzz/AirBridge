using System.Reactive.Linq;
using System.Reactive.Subjects;
using AirBridge.Domain.Interfaces;

namespace AirBridge.Infrastructure.Logging;

public sealed class FileLogReader : ILogReader, IDisposable
{
    private readonly Subject<string> _liveLines = new();
    private readonly object _gate = new();
    private FileSystemWatcher? _watcher;
    private long _position;

    public FileLogReader(string logFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logFilePath);
        LogFilePath = logFilePath;
        InitializeWatcher();
    }
    
    public string LogFilePath { get; }
    public IObservable<string> LiveLines => _liveLines.AsObservable();
    
    public async Task<IReadOnlyList<string>> ReadLastLinesAsync(int count, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(LogFilePath)) return [];
        
        // Read with FileShare.ReadWrite so Serilog's open handle doesn't block us
        await using FileStream stream = new(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using StreamReader reader = new(stream);

        var content = await reader.ReadToEndAsync(cancellationToken);
        lock (_gate)
        {
            _position = stream.Position;
        }
        var lines = content.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        return lines.TakeLast(count).ToList();
    }

    private void InitializeWatcher()
    {
        var directory = Path.GetDirectoryName(LogFilePath);
        if (directory is null || !Directory.Exists(directory)) return;

        _watcher = new FileSystemWatcher(directory, Path.GetFileName(LogFilePath))
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true,
        };

        _watcher.Changed += OnFileChanged;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Nobody is watching (logs window closed): skip the IO, ReadLastLinesAsync resyncs the position on open.
        if (!_liveLines.HasObservers) return;

        try
        {
            lock (_gate)
            {
                using var stream = new FileStream(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                if (stream.Length < _position) _position = 0;
                stream.Seek(_position, SeekOrigin.Begin);

                using var reader = new StreamReader(stream);
                while (reader.ReadLine() is { } line)
                {
                    _liveLines.OnNext(line);
                }
                _position = stream.Position;
            }
        }
        catch (IOException)
        {
            // File might be locked mid-write; the next change event catches up.
        }
    }
    
    public void Dispose()
    {
        _watcher?.Dispose();
        _liveLines.Dispose();
    }
}