using AirBridge.Application.UseCases;
using AirBridge.Domain.Enums;
using AirBridge.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AirBridge.Application.Services;

public sealed class ReceiverOrchestrationService : IAsyncDisposable
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);
    // A receiver that stayed up this long is considered healthy again, so the retry budget is refilled.
    private static readonly TimeSpan StableRunDuration = TimeSpan.FromMinutes(1);
    private const int MaxRetries = 3;

    private readonly StartReceiverUseCase _startUseCase;
    private readonly RestartReceiverUseCase _restartUseCase;
    private readonly ILogger<ReceiverOrchestrationService> _logger;

    private readonly IDisposable _statusSubscription;
    private readonly CancellationTokenSource _cts = new();
    private int _attempts;
    private volatile bool _retryPending;
    private DateTimeOffset _lastAttemptAt = DateTimeOffset.MinValue;

    public ReceiverOrchestrationService(
        StartReceiverUseCase startUseCase,
        RestartReceiverUseCase restartUseCase,
        IAirPlayReceiver receiver,
        ILogger<ReceiverOrchestrationService> logger)
    {
        _startUseCase = startUseCase;
        _restartUseCase = restartUseCase;
        _logger = logger;

        _statusSubscription = receiver.StatusChanged.Subscribe(OnStatusChanged);
    }

    public Task StartAsync(CancellationToken cancellationToken = default) => _startUseCase.ExecuteAsync(cancellationToken);

    // Only crashes / failed starts are retried; a user stop or restart goes through Stopped and is left alone.
    private void OnStatusChanged(ReceiverStatus status)
    {
        // A start we didn't trigger (tray Start, settings or device change) gets a fresh retry budget.
        if (status is ReceiverStatus.Starting && !_retryPending)
        {
            _attempts = 0;
        }

        if (status is not ReceiverStatus.Error || _cts.IsCancellationRequested) return;

        var now = DateTimeOffset.Now;
        if (now - _lastAttemptAt > StableRunDuration)
        {
            _attempts = 0;
        }

        if (_attempts >= MaxRetries)
        {
            _logger.LogError("All {Max} retry attempts exhausted. Receiver left in error state.", MaxRetries);
            return;
        }

        _attempts++;
        _lastAttemptAt = now;
        _ = RetryAsync(_attempts);
    }

    private async Task RetryAsync(int attempt)
    {
        _logger.LogWarning("Receiver failed. Retry attempt {Attempt}/{Max} in {Delay}.", attempt, MaxRetries, RetryDelay);
        try
        {
            await Task.Delay(RetryDelay, _cts.Token).ConfigureAwait(false);

            // Marks the Starting status this restart publishes as ours, so it doesn't refill the budget.
            _retryPending = true;
            try
            {
                // A failure here publishes Error again, which schedules the next attempt.
                await _restartUseCase.ExecuteAsync(_cts.Token);
            }
            finally
            {
                _retryPending = false;
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Retry attempt {Attempt} failed.", attempt);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _statusSubscription.Dispose();
        await _cts.CancelAsync();
        _cts.Dispose();
    }
}
