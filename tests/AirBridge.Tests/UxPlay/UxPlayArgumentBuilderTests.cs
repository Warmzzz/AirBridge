using AirBridge.AirPlay.UxPlay;
using AirBridge.Domain.Core;
using AirBridge.Domain.ValueObjects;

namespace AirBridge.Tests.UxPlay;

public class UxPlayArgumentBuilderTests
{
    [Fact]
    public void DefaultDevice_LetsGStreamerPickTheSink()
    {
        var arguments = UxPlayArgumentBuilder.Build(new ReceiverConfig(new DeviceName("Salon PC"), "default"));

        Assert.Equal(["-n", "Salon PC", "-nh", "-vs", "0"], arguments);
    }

    [Fact]
    public void SelectedDevice_IsPassedToWasapiSink()
    {
        const string deviceId = "{0.0.0.00000000}.{98e35b2b-8eba-412e-b840-fd2c2492cf44}";

        var arguments = UxPlayArgumentBuilder.Build(new ReceiverConfig(DeviceName.Default, deviceId));

        Assert.Equal(["-as", $"wasapisink device=\"{deviceId}\""], arguments.TakeLast(2));
    }
}
