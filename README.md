# AirBridge

AirBridge turns a Windows PC into an **AirPlay audio receiver**. Pick it from the AirPlay menu on an iPhone and audio from Spotify, YouTube, TikTok, Safari, Apple Music and other apps plays on the PC's speakers.

AirBridge is a lightweight .NET 8 tray app. It does not implement AirPlay itself: it starts, watches and configures [UxPlay](https://github.com/FDH2/UxPlay), an open-source native AirPlay server.

## Install (ready to use)

1. Download `AirBridge-vX.Y.Z-win-x64.zip` from the [Releases](../../releases) page.
2. Extract it anywhere (for example `C:\Program Files\AirBridge` or `%LOCALAPPDATA%\AirBridge`).
3. Run `AirBridge.exe`. Nothing else is needed: the .NET runtime, UxPlay and GStreamer are included.
4. When Windows Firewall asks, **allow `uxplay.exe`** on the network you use.
5. On the iPhone: **Control Center → AirPlay → AirBridge**.

The iPhone and the PC must be on the same Wi-Fi network, and guest networks with client isolation will not work.

## Usage

The AirBridge icon sits in the system tray. A left or right click opens the menu:

- the status (waiting for a device / connected: *iPhone name*)
- Start / Stop / Restart
- Output Device: the Windows audio output to play on
- Open Logs, Settings (receiver name, start with Windows), Quit

Config and logs are stored in `%APPDATA%\AirBridge`.

## Build from source

Requirements: Windows 10/11 x64, the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and [MSYS2](https://www.msys2.org/) installed in `C:\msys64`.

1. Build UxPlay. Open an **MSYS2 UCRT64** shell and run:

   ```bash
   bash tools/build-uxplay.sh
   ```

   This installs the dependencies (GStreamer, libplist, OpenSSL, etc.), clones UxPlay, builds it and installs it to `C:\msys64\ucrt64\bin\uxplay.exe`. UxPlay 1.74 and later handles mDNS on its own, so Bonjour is not needed.

2. Build, test and run AirBridge:

   ```bash
   dotnet build AirBridge.sln
   dotnet test AirBridge.sln
   dotnet run --project src/AirBridge.App
   ```

AirBridge looks for `uxplay.exe` in this order: `uxplay\bin\` next to `AirBridge.exe` (release layout), then `C:\msys64\ucrt64\bin`, then `PATH`.

### Build the release zip locally

```bash
dotnet publish src/AirBridge.App -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=none -o dist/AirBridge
```

Then, in the MSYS2 UCRT64 shell:

```bash
bash tools/bundle-uxplay.sh dist/AirBridge
```

`bundle-uxplay.sh` copies `uxplay.exe` with only the GStreamer plugins it needs for audio and every DLL they load (resolved with `ldd`). It also writes `THIRD-PARTY-NOTICES.txt` and the license texts.

## Releases (GitHub Actions)

- **CI** (`.github/workflows/ci.yml`) builds and tests every push and pull request.
- **Release** (`.github/workflows/release.yml`) runs on every `v*` tag. It builds UxPlay in MSYS2, publishes AirBridge, bundles everything into `AirBridge-<tag>-win-x64.zip` and attaches the zip to a GitHub Release:

  ```bash
  git tag v1.0.0
  git push origin v1.0.0
  ```

  "Run workflow" on the Actions tab builds the same zip as an artifact, without publishing a release.

## Architecture

| Project | Role |
|---|---|
| `AirBridge.Domain` | Entities (`ReceiverConfig`, `AirPlaySession`, `AudioDevice`), value objects, exceptions, interfaces |
| `AirBridge.Application` | Use cases (start/stop/restart, select device, update config) and crash recovery (`ReceiverOrchestrationService`) |
| `AirBridge.Infrastructure` | JSON config, Serilog, WASAPI device list (NAudio), process runner, Windows startup registration |
| `AirBridge.AirPlay` | UxPlay backend: finds the exe, builds its arguments, parses its output into sessions |
| `AirBridge.Presenters` | Presenters (MVP) and view contracts |
| `AirBridge.Views` | Tray icon (WinForms `NotifyIcon`), WPF Logs/Settings windows |
| `AirBridge.App` | Composition root: DI, hosting, single instance, startup/shutdown |
| `AirBridge.Tests` | xUnit + NSubstitute unit tests |

Dependencies point inward: `Domain` depends on nothing. Another AirPlay core (for example Shairport Sync in WSL, or video support) can be added by implementing `IAirPlayReceiver`.

## Troubleshooting

- **The PC does not show up on the iPhone:** check that Windows Firewall allows `uxplay.exe` for the current network profile (`Get-NetConnectionProfile`). Also check that the Wi-Fi network has no client isolation.
- **Exit code `0xC0000135` in the logs:** `uxplay.exe` cannot find its DLLs. Keep the `uxplay` folder next to `AirBridge.exe`, with its contents intact.
- **"Required gstreamer plugin … not found" in the logs:** the GStreamer plugin set is incomplete. Rebuild the bundle with `tools/bundle-uxplay.sh`.
- Details are in *Open Logs* (lines starting with `[uxplay]`).

## Licenses

AirBridge itself is released under the [MIT License](LICENSE).

The release zip bundles UxPlay (GPL-3.0), GStreamer (LGPL) and FFmpeg plus other MSYS2 libraries, each under its own license. `uxplay/THIRD-PARTY-NOTICES.txt` lists every package with its exact version and license, and `uxplay/licenses/` holds the license texts. Sources are available at [FDH2/UxPlay](https://github.com/FDH2/UxPlay) (commit recorded in the notices) and [msys2/MINGW-packages](https://github.com/msys2/MINGW-packages). AirBridge only starts UxPlay as a separate process.
