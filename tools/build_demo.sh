#!/usr/bin/env bash
# Builds the Unity demo for Linux, macOS and Windows from the project's Build
# Profiles and packs each one into a single zip a tutor can download and run:
#
#   Builds/demo/echoquest_demo_<os>_<YYYY-MM-DD>.zip
#
# Each zip holds one folder with the game, a README.txt (how to start it, first
# launch warnings, controls) and BUILD_INFO.txt (commit, date, Editor version).
#
# Usage (from anywhere in the repo, with the Unity Editor closed):
#   tools/build_demo.sh                    # all three
#   tools/build_demo.sh macos windows      # only some
#   tools/build_demo.sh --allow-dirty      # build with uncommitted changes in the Unity project
#   DEMO_DATE=2026-11-20 tools/build_demo.sh   # override the date in the names
#
# Details: docs/demo/desktop-builds.md
set -euo pipefail

REPO="$(git -C "$(dirname "$0")" rev-parse --show-toplevel)"
PROJECT="$REPO/unity/EchoQuestTest"
OUT="$REPO/Builds/demo"
DATE="${DEMO_DATE:-$(date +%Y-%m-%d)}"
ALLOW_DIRTY=0
TARGETS=()

for arg in "$@"; do
  case "$arg" in
    --allow-dirty) ALLOW_DIRTY=1 ;;
    linux|macos|windows) TARGETS+=("$arg") ;;
    -h|--help) sed -n '2,16p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    *) echo "Unknown argument: $arg (expected linux, macos, windows, --allow-dirty)" >&2; exit 2 ;;
  esac
done
[ ${#TARGETS[@]} -eq 0 ] && TARGETS=(linux macos windows)

die() { echo "error: $*" >&2; exit 1; }

# --- Preconditions ----------------------------------------------------------

UNITY="$(command -v unity || true)"
[ -z "$UNITY" ] && [ -x "$HOME/.unity/bin/unity" ] && UNITY="$HOME/.unity/bin/unity"
[ -n "$UNITY" ] || die "Unity CLI not found. Install it: curl -fsSL https://unity.com/install.sh | bash"
command -v zip >/dev/null || die "zip not found"
[[ "$DATE" =~ ^[0-9]{4}-[0-9]{2}-[0-9]{2}$ ]] || die "DEMO_DATE must be YYYY-MM-DD, got '$DATE'"

# A batch build cannot open a project that an Editor already has open.
if pgrep -f -i -- "-projectpath $PROJECT( |$)" >/dev/null 2>&1; then
  die "the Unity Editor has $PROJECT open. Save your work and close it (or run: unity close \"$PROJECT\"), then run this again."
fi

# Untracked files under Assets/ end up in the build too, so they count as dirty.
DIRTY="$(git -C "$REPO" status --porcelain -- unity/EchoQuestTest)"
if [ -n "$DIRTY" ] && [ "$ALLOW_DIRTY" -eq 0 ]; then
  echo "$DIRTY" >&2
  die "the Unity project has uncommitted changes (above). Commit them so the build matches a commit, or pass --allow-dirty."
fi

COMMIT="$(git -C "$REPO" rev-parse --short HEAD)"
[ -n "$DIRTY" ] && COMMIT="$COMMIT (plus uncommitted changes)"
BRANCH="$(git -C "$REPO" rev-parse --abbrev-ref HEAD)"
EDITOR_VERSION="$(sed -n 's/^m_EditorVersion: //p' "$PROJECT/ProjectSettings/ProjectVersion.txt")"

mkdir -p "$OUT/stage" "$OUT/logs"

# --- Per-OS settings --------------------------------------------------------

# Prints: <Build Profile name> <executable path inside the folder>
os_settings() {
  case "$1" in
    linux)   echo "Linux EchoQuest.x86_64" ;;
    macos)   echo "macOS EchoQuest.app" ;;
    windows) echo "Windows EchoQuest.exe" ;;
  esac
}

write_readme() { # <os> <file>
  {
    echo "EchoQuest demo ($1, $DATE)"
    echo "================================"
    echo
    echo "EchoQuest is an audio-only game. Use headphones: sounds come from the"
    echo "left, the right and behind you."
    echo
    echo "HOW TO START"
    echo
    case "$1" in
      windows)
        echo "1. Extract the whole zip (right click > Extract All). Do not run the game"
        echo "   from inside the zip: it needs the files next to it."
        echo "2. Double click EchoQuest.exe."
        echo "3. Windows may say \"Windows protected your PC\". Click \"More info\","
        echo "   then \"Run anyway\". This happens because the game is not signed."
        ;;
      macos)
        echo "1. Double click the zip to extract it, then move EchoQuest.app anywhere"
        echo "   you like (Applications is fine)."
        echo "2. Double click EchoQuest.app. The first time, macOS refuses because the"
        echo "   app is not from the App Store or a registered developer:"
        echo "   - macOS 15 or newer: click \"Done\", open System Settings >"
        echo "     Privacy & Security, scroll down and click \"Open Anyway\", then confirm."
        echo "   - macOS 14 or older: right click EchoQuest.app > Open, then Open."
        echo "   Alternatively, in Terminal:  xattr -dr com.apple.quarantine EchoQuest.app"
        echo "   (run it in the folder that contains the app)."
        echo "3. This build runs on Macs with Apple silicon (M1 or newer)."
        ;;
      linux)
        echo "1. Extract the zip (unzip echoquest_demo_linux_$DATE.zip)."
        echo "2. Run ./EchoQuest.x86_64 from the extracted folder."
        echo "   If it says \"permission denied\": chmod +x EchoQuest.x86_64"
        echo "3. 64-bit x86 Linux only."
        ;;
    esac
    echo
    echo "KEYBOARD"
    echo
    echo "  Space        pause / resume"
    echo "  R            repeat the last line"
    echo "  N            skip (or \"no\" when asked a question)"
    echo "  S or Y       yes"
    echo "  1 to 4       note buttons in the piano battle"
    echo "  Arrows       turn your head; Home recentres it"
    echo "  Backspace    restart the scene"
    echo "  F1 or \`     show / hide the debug overlay (also usable with the mouse)"
    echo
    echo "CONTROLLER"
    echo
    echo "The EchoQuest Bluetooth controller is optional. Pair it in the computer's"
    echo "Bluetooth settings (it shows up as \"EchoQuest ...\") before starting the game."
    echo
    echo "PROBLEMS"
    echo
    echo "Write down what happened and when, and send it with the BUILD_INFO.txt"
    echo "file from this folder."
  } > "$2"
}

# --- Build and pack ---------------------------------------------------------

FAILED=()
for os in "${TARGETS[@]}"; do
  read -r profile exe <<<"$(os_settings "$os")"
  name="echoquest_demo_${os}_${DATE}"
  stage="$OUT/stage/$name"
  log="$OUT/logs/$name.log"
  zipfile="$OUT/$name.zip"

  echo "==> $os: building with Build Profile '$profile' (log: ${log#$REPO/})"
  rm -rf "$stage" "$zipfile"
  mkdir -p "$stage"

  if ! "$UNITY" build "$PROJECT" --profile "$profile" --output-path "$stage/$exe" \
      --log-file "$log" --no-tail --no-provenance --allow-dirty-build \
      --non-interactive --no-banner; then
    echo "!!  $os: build failed. Last lines of the log:" >&2
    tail -n 30 "$log" >&2 || true
    FAILED+=("$os")
    continue
  fi
  [ -e "$stage/$exe" ] || { echo "!!  $os: build reported success but $exe is missing" >&2; FAILED+=("$os"); continue; }

  # Folders Unity writes next to the player that must not be shipped.
  find "$stage" -maxdepth 1 -type d \( -name '*_DoNotShip' -o -name '*_ButDontShipItWithYourGame' \) \
    -exec rm -rf {} +

  # Apple silicon refuses unsigned code. Unity already signs ad hoc; re-signing
  # makes sure the bundle is consistent after the build. No-op on other hosts.
  if [ "$os" = macos ] && command -v codesign >/dev/null; then
    codesign --force --deep --sign - "$stage/$exe" >/dev/null 2>&1 \
      || echo "warning: ad hoc signing of $exe failed; the app may not open" >&2
  fi
  [ "$os" = linux ] && chmod +x "$stage/$exe"

  write_readme "$os" "$stage/README.txt"
  cat > "$stage/BUILD_INFO.txt" <<EOF
EchoQuest demo
OS:      $os
Date:    $DATE
Commit:  $COMMIT
Branch:  $BRANCH
Unity:   $EDITOR_VERSION
Profile: $profile
EOF

  # -y keeps symlinks (macOS frameworks), -X drops extra attributes. Unix
  # permissions are kept, so the Linux binary stays executable.
  (cd "$OUT/stage" && zip -r -y -X -q "$zipfile" "$name")
  echo "    $os: ${zipfile#$REPO/} ($(du -h "$zipfile" | cut -f1 | tr -d ' '))"
done

echo
if [ ${#FAILED[@]} -gt 0 ]; then
  echo "Failed: ${FAILED[*]}. Logs are in ${OUT#$REPO/}/logs/." >&2
  exit 1
fi
echo "Done. Upload the zips in ${OUT#$REPO/}/ (commit $COMMIT)."
