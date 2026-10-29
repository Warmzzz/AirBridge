#!/usr/bin/env bash
# Copies uxplay.exe plus only the DLLs and GStreamer plugins it needs for audio into <output-dir>/uxplay,
# laid out like the MSYS2 prefix (bin/, lib/gstreamer-1.0/) so GStreamer finds its plugins relative to its DLLs.
# Run from an MSYS2 UCRT64 shell after tools/build-uxplay.sh:  bash tools/bundle-uxplay.sh <output-dir>
set -euo pipefail

out="$1/uxplay"
src="${UXPLAY_SRC:-$HOME/UxPlay}"
prefix=/ucrt64
# Elements of UxPlay's audio pipeline plus the Windows audio sinks, and the plugins its startup check
# (check_plugins in renderers/audio_renderer.c) refuses to run without: playback, videoparsersbad.
plugins=(coreelements app audioconvert audioresample volume level autodetect wasapi wasapi2 directsound libav
         playback videoparsersbad)

rm -rf "$out"
mkdir -p "$out/bin" "$out/lib/gstreamer-1.0" "$out/libexec/gstreamer-1.0" "$out/licenses"

binaries=("$prefix/bin/uxplay.exe" "$prefix/libexec/gstreamer-1.0/gst-plugin-scanner.exe")
for plugin in "${plugins[@]}"; do
  binaries+=("$prefix/lib/gstreamer-1.0/libgst$plugin.dll")
done

cp "$prefix/bin/uxplay.exe" "$out/bin/"
cp "$prefix/libexec/gstreamer-1.0/gst-plugin-scanner.exe" "$out/libexec/gstreamer-1.0/"
for plugin in "${plugins[@]}"; do
  cp "$prefix/lib/gstreamer-1.0/libgst$plugin.dll" "$out/lib/gstreamer-1.0/"
done

# ldd walks the whole dependency tree; keep the MSYS2 DLLs (Windows system DLLs are already on every PC).
mapfile -t dlls < <(ldd "${binaries[@]}" | awk '$3 ~ "^/ucrt64/bin/" { print $3 }' | sort -u)
cp "${dlls[@]}" "$out/bin/"

# Third-party notices: UxPlay itself, then every MSYS2 package a bundled file comes from.
mapfile -t packages < <(pacman -Qqo "${binaries[@]:1}" "${dlls[@]}" | sort -u)
{
  echo "AirBridge bundles the following third-party software, each under its own license (texts in licenses/)."
  echo "Binaries come from MSYS2 packages; their exact sources: https://github.com/msys2/MINGW-packages"
  echo
  echo "uxplay $("$prefix/bin/uxplay.exe" -v 2>/dev/null | sed -nE 's/.*version ([^;]+).*/\1/p' | head -n 1)"
  echo "  License: GPL-3.0   Source: https://github.com/FDH2/UxPlay (commit $(git -C "$src" rev-parse HEAD 2>/dev/null || echo unknown))"
  for package in "${packages[@]}"; do
    # LC_ALL=C: the field names below are the English ones, whatever the machine's locale.
    LC_ALL=C pacman -Qi "$package" | awk -F' *: ' '
      /^Name/     { name = $2 }
      /^Version/  { version = $2 }
      /^URL/      { url = $2 }
      /^Licenses/ { licenses = $2 }
      END { printf "%s %s\n  License: %s   Upstream: %s\n", name, version, licenses, url }'
  done
} > "$out/THIRD-PARTY-NOTICES.txt"

cp "$src/LICENSE" "$out/licenses/uxplay-LICENSE" 2>/dev/null || cp "$prefix/share/doc/uxplay/LICENSE" "$out/licenses/uxplay-LICENSE"
for package in "${packages[@]}"; do
  short="${package#mingw-w64-ucrt-x86_64-}"
  if [ -d "$prefix/share/licenses/$short" ]; then
    cp -r "$prefix/share/licenses/$short" "$out/licenses/$short"
  fi
done

echo "Bundled uxplay: ${#dlls[@]} DLLs, ${#plugins[@]} GStreamer plugins, $(du -sh "$out" | cut -f1) in $out"
