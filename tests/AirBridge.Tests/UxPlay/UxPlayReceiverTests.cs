using System.Reactive.Subjects;
using AirBridge.AirPlay.UxPlay;
using AirBridge.Domain.Core;
using AirBridge.Domain.Enums;
using AirBridge.Domain.Interfaces;
using AirBridge.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace AirBridge.Tests.UxPlay;

public sealed class UxPlayReceiverTests : IDisposable
{
    // UxPlayLocator checks the release layout first; a placeholder file there makes Locate() succeed anywhere.
    private static readonly string BundledExe = Path.Combine(AppContext.BaseDirectory, "uxplay", "bin", "uxplay.exe");

    private readonly Subject<string> _output = new();
    private readonly Subject<int> _exited = new();
    private readonly IProcessManager _process = Substitute.For<IProcessManager>();
    private readonly UxPlayReceiver _sut;
    private readonly ReceiverConfig _config = new(DeviceName.Default, "default");

    public UxPlayReceiverTests()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(BundledExe)!);
        File.WriteAllBytes(BundledExe, []);

        _process.OutputReceived.Returns(_output);
        _process.ErrorReceived.Returns(new Subject<string>());
        _process.ProcessExited.Returns(_exited);
        _sut = new UxPlayReceiver(_process, NullLogger<UxPlayReceiver>.Instance);
    }

    [Fact]
    public async Task StartAfterCrash_DoesNotDuplicateProcessHandlers()
    {
        var errors = 0;
        var sessions = new List<AirPlaySession>();
        using var statusSub = _sut.StatusChanged.Subscribe(s => { if (s is ReceiverStatus.Error) errors++; });
        using var sessionSub = _sut.SessionStarted.Subscribe(sessions.Add);

        await _sut.StartAsync(_config);
        _exited.OnNext(-1);      // crash: receiver goes to Error with its handlers still attached
        await _sut.StartAsync(_config);
        _exited.OnNext(-1);

        Assert.Equal(2, errors);

        await _sut.StartAsync(_config);
        _output.OnNext("connection request from iPhone (iPhone17,3) with deviceID = AA:BB");
        Assert.Single(sessions);
    }

    [Fact]
    public async Task SessionEnds_WhenLastSocketCloses()
    {
        var ended = 0;
        using var sub = _sut.SessionEnded.Subscribe(_ => ended++);
        await _sut.StartAsync(_config);

        _output.OnNext("Accepted IPv6 client on socket 5684, port 53625");
        _output.OnNext("Accepted IPv4 client on socket 5700, port 7000");
        _output.OnNext("connection request from iPhone de Raffaele (iPhone17,3) with deviceID = 1E:EE:EB:BA:90:E8");
        _output.OnNext("Connection closed on socket 5684");
        Assert.Equal(0, ended);

        _output.OnNext("Connection closed on socket 5700");
        Assert.Equal(1, ended);
    }

    public void Dispose()
    {
        File.Delete(BundledExe);
    }
}
