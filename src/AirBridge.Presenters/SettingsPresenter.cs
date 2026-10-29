using AirBridge.Application.UseCases;
using AirBridge.Domain.Interfaces;
using AirBridge.Presenters.Contracts;
using AirBridge.Presenters.Models;
using Microsoft.Extensions.Logging;

namespace AirBridge.Presenters;

public sealed class SettingsPresenter
{
    private readonly ISettingsView _view;
    private readonly IConfigurationRepository _configRepository;
    private readonly UpdateReceiverConfigUseCase _updateConfig;
    private readonly ILogger<SettingsPresenter> _logger;
 
    public SettingsPresenter(ISettingsView view, IConfigurationRepository configRepository, UpdateReceiverConfigUseCase updateConfig, ILogger<SettingsPresenter> logger)
    {
        _view = view;
        _configRepository = configRepository;
        _updateConfig = updateConfig;
        _logger = logger;
 
        _view.OnSaveRequested += async (_, model) => await OnSaveAsync(model);
    }

    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        var config = await _configRepository.LoadAsync(cancellationToken);
        SettingsModel model = new(
            DeviceName: config.DeviceName.Value,
            StartWithWindows: config.StartWithWindows
        );
        _view.Show(model);
    }

    private async Task OnSaveAsync(SettingsModel model)
    {
        try
        {
            await _updateConfig.ExecuteAsync(new UpdateReceiverConfigRequest(model.DeviceName, model.StartWithWindows));
            _view.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings.");
            _view.ShowError(ex.Message);
        }
    }
}