using AirBridge.Domain.Interfaces;
using AirBridge.Presenters.Contracts;
using Microsoft.Extensions.Logging;

namespace AirBridge.Presenters;

public class LogsPresenter : IDisposable
{
    private const int InitialLineCount = 200;

    private readonly ILogsView _view;
    private readonly ILogReader _logReader;
    private readonly ILogger<LogsPresenter> _logger;

    private IDisposable? _liveSubscription;

    public LogsPresenter(ILogsView view, ILogReader logReader, ILogger<LogsPresenter> logger)
    {
        _view = view;
        _logReader = logReader;
        _logger = logger;

        _view.OnClosed += OnViewClosed;
    }

    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _liveSubscription?.Dispose();
            var lines = await _logReader.ReadLastLinesAsync(InitialLineCount, cancellationToken);
            _view.Show(lines);

            var dispatcher = System.Windows.Application.Current.Dispatcher;
            _liveSubscription = _logReader.LiveLines.Subscribe(line => dispatcher.BeginInvoke(() => _view.AppendLine(line)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open logs.");
        }
    }

    private void OnViewClosed(object? sender, EventArgs e)
    {
        _liveSubscription?.Dispose();
        _liveSubscription = null;
    }

    public void Dispose()
    {
        _liveSubscription?.Dispose();
    }
}
