# Desktop builds for tutors

Last updated: 2026-11-18. Status: **script ready, not yet run on real builds.**

Until the [web build](web-build.md) exists, tutors get a desktop build for each
weekly review: one zip per OS that they download, extract and run, with no Unity
install.

## One command

From the repository root, with the Unity Editor **closed** (a headless build
cannot open a project that an Editor already has open):

```sh
tools/build_demo.sh
```

It builds the three Build Profiles in
`unity/EchoQuestTest/Assets/Settings/Build Profiles/` one after another and
writes:

```
Builds/demo/echoquest_demo_linux_<YYYY-MM-DD>.zip
Builds/demo/echoquest_demo_macos_<YYYY-MM-DD>.zip
Builds/demo/echoquest_demo_windows_<YYYY-MM-DD>.zip
```

`Builds/` is ignored by git. Upload the zips wherever the tutors download from.

Each zip contains a single folder named like the zip, holding:

- the game (`EchoQuest.exe` + `EchoQuest_Data/`, `EchoQuest.app`, or
  `EchoQuest.x86_64` + `EchoQuest_Data/`);
- `README.txt`: how to start it on that OS, how to get past the "unsigned app"
  warning, keyboard controls, controller pairing;
- `BUILD_INFO.txt`: date, commit, branch, Unity version, profile. Tutors send it
  back with bug reports, so every report maps to a commit.

## Options

| Command | Effect |
|---|---|
| `tools/build_demo.sh macos windows` | Build only these OSes |
| `tools/build_demo.sh --allow-dirty` | Build even if `unity/EchoQuestTest` has uncommitted changes. `BUILD_INFO.txt` then says so |
| `DEMO_DATE=2026-11-20 tools/build_demo.sh` | Use another date in the names |

Build logs go to `Builds/demo/logs/`. If one OS fails, the others are still
built and the script exits with an error naming the failed ones.

## Why it is repeatable

- The builds come from the committed Build Profiles and the committed scene list
  (`EditorBuildSettings`); nothing is set by hand in the Editor.
- Each build runs in a fresh headless Editor (`unity build --profile`), not in
  whatever state the open Editor is in.
- By default the script refuses to build when the Unity project has uncommitted
  or untracked changes, so a zip always matches a commit.
- Names come only from the OS and the date. Rebuilding on the same day replaces
  that day's zips.

The zips are not byte-identical between runs: Unity players are not
reproducible builds, and the zip stores file times.

## What the script does after each build

- Removes the `*_BurstDebugInformation_DoNotShip` and
  `*_BackUpThisFolder_ButDontShipItWithYourGame` folders Unity writes next to
  the player.
- macOS: signs the app ad hoc again (`codesign --sign -`), because Apple silicon
  refuses to run unsigned code.
- Linux: makes sure the binary is executable. `zip` keeps Unix permissions and
  symlinks, so they survive the download.

## Known limits

- **macOS Gatekeeper.** The app is not notarised (that needs a paid Apple
  Developer account), so the first launch is blocked until the tutor clicks
  "Open Anyway" in System Settings > Privacy & Security. The README explains it.
- **Windows SmartScreen.** The exe is not signed: "More info" > "Run anyway".
- **macOS architecture.** The macOS profile builds for Apple silicon only.
  Switch it to "Intel 64-bit + Apple silicon" in the Build Profile if a tutor has
  an Intel Mac.
- **Controller on Linux.** The controller layout matches on the `HID`
  interface; untested on Linux, where the Input System's HID support is limited.
  The keyboard works everywhere.

## Possible next step

Run the same script from CI (GameCI on a Linux runner can build Linux and
Windows; macOS players need a macOS runner) and attach the zips to a GitHub
release per review. This needs the Unity licence as a repository secret: see
open decision 3 in [README.md](README.md).
