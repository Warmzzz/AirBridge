using System.Text.RegularExpressions;

namespace AirBridge.AirPlay.UxPlay;

internal static partial class UxPlayOutputParser
{
    // uxplay stdout examples (LOGGER_INFO, printed without -d):
    //   "connection request from iPhone de Pierre (iPhone15,2) with deviceID = 5A:3B:..."
    //   "Accepted IPv4 client on socket 868, port 7000"
    //   "Connection closed on socket 868"
    [GeneratedRegex(@"connection request from (?<name>.+) \((?<model>[^)]*)\) with deviceID = (?<id>\S+)")]
    private static partial Regex ClientRequestRegex();

    [GeneratedRegex(@"Accepted \S+ client on socket (?<socket>\d+)")]
    private static partial Regex SocketOpenedRegex();

    [GeneratedRegex(@"Connection closed on socket (?<socket>\d+)")]
    private static partial Regex SocketClosedRegex();

    public static bool TryParseClientRequest(string line, out string clientName, out string deviceId)
    {
        var match = ClientRequestRegex().Match(line);
        clientName = match.Success ? match.Groups["name"].Value.Trim() : string.Empty;
        deviceId = match.Success ? match.Groups["id"].Value : string.Empty;
        return match.Success;
    }

    public static bool TryParseSocketOpened(string line, out int socket) => TryParseSocket(SocketOpenedRegex(), line, out socket);

    public static bool TryParseSocketClosed(string line, out int socket) => TryParseSocket(SocketClosedRegex(), line, out socket);

    private static bool TryParseSocket(Regex regex, string line, out int socket)
    {
        var match = regex.Match(line);
        socket = match.Success ? int.Parse(match.Groups["socket"].Value) : 0;
        return match.Success;
    }
}
