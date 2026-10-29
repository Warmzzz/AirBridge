using AirBridge.Application.UseCases;
using AirBridge.Domain.Core;
using AirBridge.Domain.Enums;
using AirBridge.Domain.Exceptions;
using AirBridge.Domain.Interfaces;
using AirBridge.Domain.ValueObjects;
using NSubstitute;

namespace AirBridge.Tests.UseCases;

public class SelectAudioDeviceUseCaseTests
{
    private readonly IAudioDeviceService _devices = Substitute.For<IAudioDeviceService>();
    private readonly IConfigurationRepository _config = Substitute.For<IConfigurationRepository>();
    private readonly IAirPlayReceiver _receiver = Substitute.For<IAirPlayReceiver>();
    private readonly SelectAudioDeviceUseCase _sut;

    public SelectAudioDeviceUseCaseTests()
    {
        _devices.GetOutputDevices().Returns([new AudioDevice("speakers", "Speakers"), new AudioDevice("headset", "Headset")]);
        _config.LoadAsync(Arg.Any<CancellationToken>()).Returns(new ReceiverConfig(DeviceName.Default, "speakers"));
        _sut = new SelectAudioDeviceUseCase(_devices, _config, _receiver);
    }

    [Fact]
    public async Task RunningReceiver_IsRestartedOnTheNewDevice()
    {
        _receiver.Status.Returns(ReceiverStatus.Running);

        await _sut.ExecuteAsync("headset");

        await _config.Received(1).SaveAsync(Arg.Is<ReceiverConfig>(c => c.AudioDeviceId == "headset"), Arg.Any<CancellationToken>());
        await _receiver.Received(1).RestartAsync(Arg.Is<ReceiverConfig>(c => c.AudioDeviceId == "headset"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StoppedReceiver_IsNotStarted()
    {
        _receiver.Status.Returns(ReceiverStatus.Stopped);

        await _sut.ExecuteAsync("headset");

        await _receiver.DidNotReceiveWithAnyArgs().RestartAsync(default!, default);
    }

    [Fact]
    public async Task UnknownDevice_Throws()
    {
        await Assert.ThrowsAsync<AudioDeviceException>(() => _sut.ExecuteAsync("missing"));
    }
}
