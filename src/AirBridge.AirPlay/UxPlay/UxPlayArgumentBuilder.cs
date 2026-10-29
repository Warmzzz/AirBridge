using AirBridge.Domain.Core;

namespace AirBridge.AirPlay.UxPlay;

internal static class UxPlayArgumentBuilder
{
    // Matches the id JsonConfigurationRepository uses when no device was picked: let GStreamer use the Windows default.
    private const string SystemDefaultDevice = "default";

    public static IReadOnlyList<string> Build(ReceiverConfig config)
    {
        List<string> arguments =
        [
            "-n", config.DeviceName.Value,
            "-nh",        // don't append "@hostname" to the AirPlay name
            "-vs", "0"    // audio only, no video window
        ];

        if (config.AudioDeviceId != SystemDefaultDevice)
        {
            // WASAPI endpoint ids (e.g. "{0.0.0.00000000}.{guid}") are what wasapisink expects.
            arguments.AddRange(["-as", $"wasapisink device=\"{config.AudioDeviceId}\""]);
        }

        return arguments;
    }
}
