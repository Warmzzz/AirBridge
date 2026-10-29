namespace AirBridge.AirPlay.UxPlay;

internal static class UxPlayLocator
{
    private const string ExecutableName = "uxplay.exe";
    private const string Msys2Ucrt64Bin = @"C:\msys64\ucrt64\bin";

    // Release zips ship uxplay next to AirBridge.exe (see tools/bundle-uxplay.sh).
    private static readonly string BundledBin = Path.Combine(AppContext.BaseDirectory, "uxplay", "bin");

    public static string Locate()
        => Candidates().FirstOrDefault(File.Exists)
           ?? throw new FileNotFoundException(
               $"{ExecutableName} not found in {BundledBin}, {Msys2Ucrt64Bin} or PATH. Build it with tools/build-uxplay.sh (MSYS2 UCRT64 shell).");

    private static IEnumerable<string> Candidates()
    {
        yield return Path.Combine(BundledBin, ExecutableName);
        yield return Path.Combine(Msys2Ucrt64Bin, ExecutableName);

        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            yield return Path.Combine(directory, ExecutableName);
        }
    }
}
