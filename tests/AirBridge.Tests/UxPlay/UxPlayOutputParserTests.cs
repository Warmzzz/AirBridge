using AirBridge.AirPlay.UxPlay;

namespace AirBridge.Tests.UxPlay;

public class UxPlayOutputParserTests
{
    [Fact]
    public void ClientRequest_ExtractsNameAndDeviceId()
    {
        var parsed = UxPlayOutputParser.TryParseClientRequest(
            "connection request from iPhone de Pierre (iPhone15,2) with deviceID = 5A:3B:11:22:33:44",
            out var name, out var deviceId);

        Assert.True(parsed);
        Assert.Equal("iPhone de Pierre", name);
        Assert.Equal("5A:3B:11:22:33:44", deviceId);
    }

    [Fact]
    public void ClientRequest_KeepsParenthesesInsideTheName()
    {
        UxPlayOutputParser.TryParseClientRequest(
            "connection request from Bob (work) (iPhone14,5) with deviceID = AA:BB", out var name, out _);

        Assert.Equal("Bob (work)", name);
    }

    [Theory]
    [InlineData("Accepted IPv4 client on socket 868, port 7000", 868)]
    [InlineData("Accepted IPv6 client on socket 12, port 7000", 12)]
    public void SocketOpened_ExtractsSocket(string line, int expected)
    {
        Assert.True(UxPlayOutputParser.TryParseSocketOpened(line, out var socket));
        Assert.Equal(expected, socket);
    }

    [Fact]
    public void SocketClosed_ExtractsSocket()
    {
        Assert.True(UxPlayOutputParser.TryParseSocketClosed("Connection closed on socket 868", out var socket));
        Assert.Equal(868, socket);
    }

    [Fact]
    public void UnrelatedLine_MatchesNothing()
    {
        const string line = "using network ports UDP 6000 6001 7011 TCP 7000 7001 7100";

        Assert.False(UxPlayOutputParser.TryParseClientRequest(line, out _, out _));
        Assert.False(UxPlayOutputParser.TryParseSocketOpened(line, out _));
        Assert.False(UxPlayOutputParser.TryParseSocketClosed(line, out _));
    }
}
