#!/usr/bin/env bash
# Builds UxPlay (the AirPlay core AirBridge drives) and installs it to C:\msys64\ucrt64\bin\uxplay.exe,
# where AirBridge looks for it. UxPlay >= 1.74 ships its own mDNS, so Bonjour is not required.
# Run from an MSYS2 UCRT64 shell:  bash tools/build-uxplay.sh
set -euo pipefail

pacman -S --needed --noconfirm git \
  mingw-w64-ucrt-x86_64-{cmake,gcc,ninja,pkgconf,openssl,libplist} \
  mingw-w64-ucrt-x86_64-{gstreamer,gst-plugins-base,gst-plugins-good,gst-plugins-bad,gst-libav}

src="${UXPLAY_SRC:-$HOME/UxPlay}"
if [ -d "$src/.git" ]; then
  git -C "$src" pull --ff-only
else
  git clone --depth 1 https://github.com/FDH2/UxPlay "$src"
fi

# NO_MARCH_NATIVE: UxPlay otherwise compiles with -march=native, so a binary built on the CI runner
# crashes with STATUS_ILLEGAL_INSTRUCTION (0xC000001D) on CPUs lacking the runner's instruction sets.
cmake -S "$src" -B "$src/build" -G Ninja -DCMAKE_BUILD_TYPE=Release -DNO_MARCH_NATIVE=ON
ninja -C "$src/build"
cmake --install "$src/build" --prefix /ucrt64

/ucrt64/bin/uxplay -h | head -n 1
