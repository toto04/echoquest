# Development workflow

EchoQuest is developed by a small team working closely with AI coding agents
(currently **Claude Code**). The repo is organised so that an agent can make a
change, verify it and explain it, while a human keeps control of hardware,
pairing, commits and anything that leaves the machine.

## Principles

1. **Small, verifiable steps.** Every change has a check that doesn't need a
   human: a build, unit tests, a self-test or a probe command. Hardware checks
   are written down as manual steps in the [README](../README.md#manual-test-steps).
2. **One source of truth per concern.** Pins and board settings live only in
   `firmware/include/config.h`; the report layout only in
   `protocol/hid-report.md` (code mirrors it); agent instructions in `CLAUDE.md`.
3. **Pure logic is testable on the host.** Anything that doesn't need hardware
   goes in header-only files under `firmware/include/` with native tests.
4. **Verify before building.** When a choice depends on external facts (library
   capabilities, platform versions, board pinouts), check the source or docs
   first and report back before writing code. Example: we checked
   ESP32-BLE-Gamepad's output-report support before choosing NimBLE directly.
5. **The human decides on irreversible or outward actions.** Agents commit or
   push only when asked, ask before flashing or installing large software, and
   never re-pair or erase bonds on someone's behalf.

## Firmware

Everything runs from `firmware/` with PlatformIO. Full command list: [`CLAUDE.md`](../CLAUDE.md#commands-run-from-firmware).

The loop an agent follows for every firmware change:

```sh
pio run -e waveshare-c6-lcd147 -e esp32-c6-devkitc-1 -e release   # all build, no warnings
pio test -e native                                                 # unit tests pass
python tools/hid_probe.py selftest                                 # from repo root
```

Then, on hardware (human, or agent with a board connected and permission):

```sh
pio run -t upload -t monitor          # flash the default (Waveshare) env and watch the log
python tools/hid_probe.py monitor     # decoded input as the host sees it
python tools/hid_probe.py haptic 0 200 500
```

### Changing the protocol

Any change to the HID descriptor or report layout is the most disruptive change
in the project, because hosts cache descriptors. Follow the procedure in
[`CLAUDE.md`](../CLAUDE.md#changing-the-hid-descriptor-or-report-layout-re-pair-required):
doc first, then `protocol.h`, then `hid_probe.py` (and later the Unity layout);
bump PID and version; re-pair on every host.

### Adding a board

Add a `BOARD_*` profile in `config.h` and an env in `platformio.ini`. Boards
must produce identical HID reports. The Waveshare profile is the worked example.

## Unity

Unity work is driven through the **Unity CLI** (`unity`, installed at
`~/.unity/bin/unity`) and its Claude Code skill (`unity-cli`, installed in
`~/.claude/skills/unity-cli`).

What it lets an agent do:

- Install Editors and create projects from the command line.
- Add the **Pipeline package** (`com.unity.pipeline`) to a project. With it, a
  running Editor can be controlled from the terminal: `unity status` to check
  it's ready, `unity command` to list what it exposes, and `unity command eval
  '<C#>'` to run C# inside the Editor (create GameObjects, add components, set
  references, save scenes).
- Recompile and read errors, enter Play mode, read the console and take
  screenshots to check results.
- Run tests and builds headless (`unity test`, `unity build`) when no Editor is open.

Working rules for Unity:

- **Drive the live Editor; don't hand-edit `.unity`, `.prefab` or `.asset`
  YAML** while an Editor is open. File edits are invisible to the running
  Editor and GUIDs are easy to get wrong.
- **C# scripts are normal files** written by the agent; the Editor recompiles them.
- **Compile errors put the Editor in Safe Mode**, where the Pipeline package
  doesn't load and the CLI can't connect. Fix the errors in the files, then restart the Editor.
- **Play mode needs the Editor window to stay alive.** An unfocused Editor can
  freeze at the first frame while still reporting that it's playing. Verify with
  fresh screenshots or console output, not just the Play state.
- Installing an Editor, updating the CLI (`unity self-update`) and filing Unity
  bug reports (`unity bug`, which uploads logs) need the human's go-ahead.

Commands that proved useful (Editor commands are addressed as `<group>.<name>`
or `<group> <name>`; `unity command --query <word>` finds them):

```sh
export UNITY_PROJECT_PATH=$PWD/unity/EchoQuestTest    # target this project's Editor
unity status --until-ready                            # wait for the Editor
unity command recompile; unity command recompile_status --result-only
unity command eval --code 'return UnityEngine.InputSystem.InputSystem.devices.Count;' --result-only
unity command eval --code 'EchoQuest.Editor.TestSceneBuilder.Build();'
unity command editor.playmode set_run_in_background   # needed again after every recompile, or Play mode freezes at frame 1
unity command editor.playmode start                   # and editor.playmode stop
unity command capture game_view --output-path /tmp/game.png
unity command console get --tail 20
```

To confirm Play mode is really running (not frozen), read `Time.frameCount`
twice with `eval` and check it increased.

Environment as of 2026-11-04: Unity CLI 1.0.0-beta.13 (beta.16 available),
Unity Personal license, Unity Hub, Unity Editor **6000.6.8f1** (arm64).

## Git

- `main` is the only branch for now. Commits use conventional prefixes (`feat:`, `docs:`, `fix:`).
- Agents commit and push only when asked.
- Generated folders (`.pio/`, Unity `Library/`, `Temp/`, Python venvs) are never committed.
- Unity projects keep their own `.gitignore`; see
  [unity-test-scene.md](unity-test-scene.md#repository-setup) for what to check before committing.
