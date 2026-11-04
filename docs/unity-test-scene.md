# Unity test scene (plan)

Status: **planned, not started.** This document describes what we will build
and how, so the work can be picked up by anyone (or any AI agent).

## Goal

A very small, slightly fun Unity scene that proves the controller works inside
Unity, before any real game code:

- An object (e.g. a cube or a simple "creature") **rotates with the
  controller's yaw, pitch and roll** in real time.
- The **two buttons** (touch A/B on the Waveshare board = HID buttons 1 and 2)
  **change something visible**, e.g. A cycles the object's colour, B makes it
  pulse or jump.
- A small on-screen **debug readout** of the raw values (buttons, angles, device
  name) for troubleshooting.

It is a developer test bench, so it is visual on purpose. The real game is
audio-only (see [vision.md](vision.md)). Haptics are out of scope for this
first scene (see [roadmap.md](roadmap.md#2-haptics-from-unity)).

Equally important as the scene itself: **learning how Unity sees our device**,
because that decides how the game's input layer is written.

## Technical plan

| Item | Choice | Why |
|---|---|---|
| Unity version | **Unity 6.3 LTS** (6000.3.28f1 at time of writing) | Latest long-term-support release |
| Template | Universal 3D (`com.unity.template.urp-blank`) | Default modern render pipeline; Built-in is deprecated |
| Input | **Unity Input System** package | Supports HID devices on Windows and macOS, including generic ones with custom descriptors |
| Location | `unity/EchoQuestTest/` in this repo (proposed) | Keeps the test next to the protocol it mirrors |
| Editor automation | Unity CLI + Pipeline package (`com.unity.pipeline`) | Lets an AI agent build the scene in a live Editor (see below) |

### How the controller is expected to appear in Unity

Our controller uses a non-standard VID/PID (`0x1209:0x0001`), so Unity will not
recognise it as a known gamepad (Xbox, DualSense). The expectation is that the
Input System's **generic HID support** builds a layout from our report
descriptor:

- 16 buttons → button controls
- hat switch → a d-pad-like control
- X / Y / Z (yaw / pitch / roll) and Rx / Ry / Rz (spare) → axis controls,
  probably normalised from the logical range −32767..32767 to −1..1, so
  `degrees ≈ value × 180`

**These are assumptions to verify.** The first step of the work is a small
script that lists every input device Unity sees, with its layout, control paths
and live values. The cube script is then written against the real control
names, not guessed ones. Results get recorded in this document.

Possible obstacles:
- **macOS Input Monitoring permission** may be needed for the Unity Editor to
  read HID devices (as it was for the terminal running `hid_probe.py`).
- If the generic HID layout is unusable (wrong scaling, missing controls), the
  fallback is a **custom Input System device layout** for our VID/PID that
  matches `protocol/hid-report.md` byte for byte.
- Yaw starts at 0 at controller power-on and drifts; the scene should have a
  **recenter** action (e.g. a keyboard key) that zeroes yaw on the Unity side.

### Scene contents (draft)

- Camera, light, a floor or simple backdrop.
- `ControllerTarget`: the object that rotates. Rotation from the controller:
  yaw → rotation around Unity's Y axis, pitch → around X, roll → around Z, with
  signs adjusted so the on-screen object moves like the controller in your hand.
  Light smoothing is optional.
- `ControllerDebugHud`: device name, connected or not, buttons, yaw/pitch/roll in degrees.
- Button 1 (A): cycle colour. Button 2 (B): a short scale "pop" or jump.
- Keyboard fallback (arrow keys and space) so the scene can be checked with no controller.

## Repository setup

The repo-root `.gitignore` was generated for several stacks and **breaks Unity
projects** as it stands:

- `*.meta` (from the Visual Studio section): Unity `.meta` files **must be
  committed**, or asset GUIDs and references break.
- `**/[Pp]ackages/*` (from the NuGet section): would ignore
  `Packages/manifest.json` and `packages-lock.json`, which **must be committed**.
- The Unity rules (`/[Ll]ibrary/`, `/[Tt]emp/`, ...) are anchored to the repo
  root, so they would **not** ignore `unity/EchoQuestTest/Library/`.

Plan: remove those two lines from the root `.gitignore` (nothing else in the
repo needs them) and put Unity's standard `.gitignore` inside the Unity
project folder. Before the first commit of the project, check that
`git ls-files | grep Library/` prints nothing and that `.meta` files are staged.

## How it will be built (AI-assisted)

Built mostly by Claude Code driving the Unity Editor through the **Unity CLI**
(details in [development-workflow.md](development-workflow.md#unity)):

1. **Install the Editor** (`unity install 6000.3.28f1`), or install it through
   Unity Hub. Several GB; needs a go-ahead.
2. **Create the project** with the CLI (`unity projects create ... --template
   com.unity.template.urp-blank`).
3. **Add the Pipeline package before the first open** (`unity pipeline
   install`), plus the Input System package.
4. **Open the Editor** (`unity open`) and wait until the CLI reports it ready.
5. **Device discovery script** first: log what Unity sees, with the controller
   paired, and record the findings here.
6. **Build the scene in the live Editor**: C# scripts are written as files;
   GameObjects, components and references are created with `unity command
   eval` against the running Editor, then the scene is saved. Scene and prefab
   YAML is never hand-edited while an Editor is open.
7. **Verify** by entering Play mode, reading the console and taking
   screenshots. A human checks the feel with the real controller in hand.

What the human does: approve the Editor install, pair the controller, keep the
Editor window open (an Editor in the background may stop updating Play mode),
and try it.

## Done when

- [ ] Unity project created, opens cleanly, committed with correct ignores.
- [ ] Device discovery results recorded in this document (layout name, control paths, value ranges).
- [ ] Turning, tilting and rolling the controller rotates the object the same way, with correct signs.
- [ ] Button A and button B each trigger their effect; holding both works.
- [ ] Recenter-yaw action works.
- [ ] Short "how to run the test scene" section added to the root README.
