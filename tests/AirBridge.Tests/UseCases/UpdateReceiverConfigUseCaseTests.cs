using AirBridge.Application.UseCases;
using AirBridge.Domain.Core;
using AirBridge.Domain.Enums;
using AirBridge.Domain.Interfaces;
using AirBridge.Domain.ValueObjects;
using NSubstitute;

namespace AirBridge.Tests.UseCases;

public class UpdateReceiverConfigUseCaseTests
{
    private readonly IConfigurationRepository _config = Substitute.For<IConfigurationRepository>();
    private readonly IStartupRegistrationService _startup = Substitute.For<IStartupRegistrationService>();
    private readonly IAirPlayReceiver _receiver = Substitute.For<IAirPlayReceiver>();
    private readonly UpdateReceiverConfigUseCase _sut;

    public UpdateReceiverConfigUseCaseTests()
    {
        _config.LoadAsync(Arg.Any<CancellationToken>()).Returns(new ReceiverConfig(new DeviceName("Old"), "default"));
        _receiver.Status.Returns(ReceiverStatus.Running);
        _sut = new UpdateReceiverConfigUseCase(_config, _startup, _receiver);
    }

    [Fact]
    public async Task Rename_RestartsRunningReceiver()
    {
        await _sut.ExecuteAsync(new UpdateReceiverConfigRequest("New", StartWithWindows: true));

        _startup.Received(1).Register();
        await _receiver.Received(1).RestartAsync(Arg.Is<ReceiverConfig>(c => c.DeviceName.Value == "New"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SameName_DoesNotRestart()
    {
        await _sut.ExecuteAsync(new UpdateReceiverConfigRequest("Old", StartWithWindows: false));

        _startup.Received(1).Unregister();
        await _receiver.DidNotReceiveWithAnyArgs().RestartAsync(default!, default);
    }
}
