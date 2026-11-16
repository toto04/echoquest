# Debug UI and demo shell

Last updated: 2026-11-17. Status: **first version of the overlay works in the
Editor** (seven tabs, mouse-driven). The demo shell, the timing strip and the
key-map change are still to do. See [Tasks](#tasks) for the exact state.

## What exists now

- `Scripts/DebugUi/Ui.cs`: buttons, toggles and sliders drawn with IMGUI and
  clicked through the Input System.
- `Scripts/DebugUi/DebugOverlay.cs`: the docked panel. It creates itself in
  every scene at start-up, finds the scene's `StoryPlayer` (also one created
  later), and rebuilds its tabs. F1 or the backquote key hides it; a corner
  button brings it back.
- `Scripts/DebugUi/Panels.cs`: the tabs Story, Input, Audio, Room, Pad, Log,
  Demo. Scenes without a story player show Pad and Demo only.
- `Scripts/Story/StoryPlayer.Control.cs`: the control surface the tabs call.
- `tools/debug_overlay_checks.cs`: 17 Play-mode checks, including a simulated
  mouse click. Run it like the other checks:

```sh
unity command 'script run' --file ../../tools/debug_overlay_checks.cs --entry DebugOverlayChecks.Run --timeout 60
```

Verified on 2026-11-17 in the Editor: compiles with no warnings; the new checks
and all four existing check scripts pass (56 rule checks, 45 interaction
checks, the three Scene 1 routes, the audio pause check); console clean. The
tabs were inspected from Game view captures. **Not yet done:** using it by hand
with a real mouse, and any player build.

Goal: a sighted visitor with a mouse and a keyboard can try everything the
project can do, and can see what the audio engine, the story player and the
controller are doing while it happens.

Read [current-state.md](current-state.md) first.

## Principles

1. **Overlay, not game.** One key and one on-screen button hide all of it. The
   hidden state is the real experience: a dark screen and sound.
2. **Everything reachable by mouse.** A web visitor will not learn fifteen
   keys. Every keyboard action has a button.
3. **The debug UI never changes behaviour by being open.** It calls the same
   public methods and feeds the same input queue as the keyboard and the
   controller.
4. **Code only.** No hand-edited scenes or prefabs. The overlay creates itself
   at start-up, so the scenes did not need rebuilding.
5. **Works in the Web build.** No file access, no editor-only APIs outside
   `#if UNITY_EDITOR`.

## Technology

Decision taken: **IMGUI for drawing, the Input System for clicks.**

Unity's Input System documentation says IMGUI receives no input events in a
player when Active Input Handling is "Input System Package", which is this
project's setting. Drawing still works. So `Ui.cs` draws with IMGUI and does
its own hit testing against `Pointer.current`, which covers mouse, touch and
pen and behaves the same in the Editor and in a build. This has not been tried
in a build yet (task W-01).

Rules that follow from this, for anyone adding a panel:

- Use `Ui.Button`, `Ui.Toggle`, `Ui.Slider`. `GUI.Button` and
  `GUILayout.Button` will not react in a build.
- A click is found during the Repaint pass and delivered on the next Layout
  pass, one frame later. Do not put a value that changes every frame in a
  button's label, because the label is part of the button's identity.
- Scrolling is the overlay's own: mouse wheel, or drag on the bar at the right.

Why IMGUI at all:

- It is pure code, so agents can write and review it without touching assets.
- It works in the Web player and needs no fonts, style sheets or prefabs.
- Its weak points are looks and layout on small screens. A shared scale factor
  (already in `ControllerDebugHud`) covers the second.

UI Toolkit would look better and is the alternative if designers are expected
to judge the debug UI itself. They are not: it is a tool for showing the audio.

Known trap, from [../unity-test-scene.md](../unity-test-scene.md): do not read
Input System controls inside `OnGUI`. Collect state in `Update` or
`LateUpdate` and only draw in `OnGUI`.

## Demo shell

The entry point that does not exist today.

Partly covered already: the overlay's **Demo** tab lists the scenes in the
build and switches between them, and the **Story** tab switches section, so the
unrecorded sections can be read as text today. What is missing is the first
scene and the click-to-start.

- A new first scene, `Assets/EchoQuest/Scenes/Demo.unity`, built by a new
  `DemoSceneBuilder`.
- **Click-to-start screen.** Browsers block audio until the first click or key
  press. The screen says what the demo is, asks for headphones, and starts on
  any input.
- **Menu.** Intro, Scena 1, Controller test, and the unrecorded sections
  (`s02` to `e00`) marked as "text only". One story scene that takes the
  section as a parameter is simpler than one scene per section.
- **Build stamp.** Version, commit, date (task W-13).
- **Back to menu** from every scene, by key and by button.
- **Language.** The story is Italian. The shell and debug UI should be in
  English with Italian content shown as is, or bilingual; the audience is
  Italian tutors and designers, so decide when writing the first screen.

## Refactors the panels need

Before this work `StoryPlayer` exposed `Restart`, `TogglePause`, `Skip`,
`Repeat` and read-only state. The "Done" column says what
`StoryPlayer.Control.cs` now provides.

| Need | Change | Done |
|---|---|---|
| Jump to any item | `JumpTo(string id)`: restart `RunSection` from that index, stop all sources, reset branch state | Yes. Jumping into a branch selects that branch |
| Switch section and take at run time | `SetSection(string)`, `SetTake(string)`; today they are Inspector fields read once | `SetSection` yes. `take` is a public field read on every line, so the tab sets it directly |
| Force a branch | Public setter for `BranchChoice` | Yes, `ForceBranch` |
| On-screen buttons | One input entry point, `PressLogical(int button, double time)`, fed by keyboard, controller and UI. It replaces the direct keyboard reads and feeds `m_InteractionInput` | Partly. `PressLogical(int)` feeds the same queue from the UI. Keyboard and controller keep their own paths, so the existing checks are untouched |
| Show what an interaction heard | Keep the last attempt: tap times, expected times, per-gap error, pass or fail and the reason. `StoryInteractionRules` returns a result object instead of a bool | Partly. `LastAttempt` is a one-line description. The rules still return a bool |
| Force pass or fail | `ResolveInteraction(bool passed)` for demo use | `ForcePass()` only. Failing is done by tapping wrong |
| Event log | A `StoryEvent` callback: item started, clip played, clip missing, input received, branch chosen | Yes, `LogEvent` and the `Log` list (last 300) |
| Live tunables | The pacing fields are already public; the panel edits them. Add a reset to defaults | Yes, in the Audio tab |
| Missing-audio report | `StoryPlayer` already knows which ids have no clip; expose the list | Yes, `HasAudio(item)` |

Keep the pure rules pure. `tools/story_interaction_checks.cs` and the other
check scripts must keep passing after each refactor.

## Panels

A top bar with tabs. Each tab is one small class deriving from `DebugPanel`
(`Title`, `Draw`, and optionally `Tick` for reading input and `DrawScreen` for
drawing outside the dock). New tabs are added in `DebugOverlay.Rebuild`.

The sections below are the plan. Items not built yet are marked.

### Story

- Transport: play/pause, previous item, next item, repeat, restart.
- Section and take selectors.
- **Timeline**: scrollable list of every item in the section with id, speaker,
  first words, kind, branch. Current item highlighted. Click to jump. Items
  with no audio are marked.
- Current line in full, with a **subtitles** toggle that shows the line large,
  for tutors who want to read along.
- Branch: the current choice, and buttons to force A or B.

All built.

### Interaction

Shown when the current item is an interaction.

- Mode, prompt id, which buttons are accepted, attempts used.
- **Virtual buttons**: 1, 2, 3, 4, Yes, No. Mouse or touch.
- For rhythm and missing-note: a time strip with the expected taps and the
  received taps, the error of each gap in milliseconds, and the tolerance band.
  **Not built**: the tab shows the expected times and a one-line description of
  the last attempt instead.
- Pass, skip and hear-again buttons. There is no fail button.

### Audio

- Master volume and mute.
- Active sources: label, clip, volume, gain after ducking, distance, low-pass
  cutoff, looping, time left.
- Toggles: reverb, distance low-pass, ducking. A beds toggle is **not built**.
  On the Web a note says reverb and muffling are expected to do nothing; hiding
  them properly waits for task W-07.
- Pacing sliders: the five pause lengths, duck gains, fade time.
- Record to WAV (Editor and desktop only).

### Listener and room

- The existing top-down view stays as the background of the story scene.
- Head yaw slider and recentre. Option to **drive the head from the
  controller's yaw**, which the story scenes do not do today.
- Staging of the current item: position, movement, volume.

All built. Head tracking from the controller has not been tried with hardware.

### Controller

- The content of `ControllerDebugHud`: device, report rate, raw bytes, buttons,
  hat, angles, device list.
- Mapping of the Yes and No thumb buttons, editable at run time. Today both are
  unassigned, so a controller cannot answer questions.
- Recentre yaw: in the Room tab ("Face forward").
- Haptics test: motor, intensity, duration. Shown as "not implemented" until
  roadmap milestone 2.

### Log and info

- The event log, newest first, with a filter.
- Missing-audio report for the current section.
- Build stamp, platform, browser, frame rate, audio sample rate, audio clock.

The log and the missing-audio list are in the Log tab; there is no filter yet.
The Demo tab has the scene list, the dark-screen toggle, version, platform,
frame rate and audio clock. Commit and build date wait for W-13.

## Keys

- **F1** or **`** toggles the overlay. Also a small corner button, since the
  Web visitor may not read instructions. Built.
- **Esc** goes back to the menu. Not built; there is no menu scene yet.
- Fix the double meaning of **N**, which is "skip" during speech and "No"
  during a question. Proposal: N stays "No" and skip moves to **.** (full
  stop); the arrow keys keep turning the head. Settle it in D-03 and update
  `Story/README.md` and the Input tab's help line. **Not done**: the keys are
  unchanged, because the existing check scripts press them.

## Tasks

### Shell

- [ ] **S-01 `DemoSceneBuilder` and `Demo.unity`**, first in the build list.
  Done when a build opens on it.
- [ ] **S-02 Click-to-start screen.** No audio before the first input.
- [ ] **S-03 Menu and scene switching**, with back-to-menu everywhere. Done
  when all three existing scenes are reachable and leavable without a reload.
  The Demo tab already switches scenes; the menu itself is missing.
- [ ] **S-04 One story scene parameterised by section**, replacing the
  per-section scenes, with unrecorded sections listed as text only. The Story
  tab already switches section inside either story scene.

### Debug UI

- [x] **D-01 Overlay framework**: `DebugOverlay` component, tabs, scale,
  toggle key and corner button, `DebugPanel`. `StoryHud` now only draws the
  names over the sound markers; `ControllerDebugHud` only builds the Pad tab's
  text. Done 2026-11-17.
- [x] **D-02 `StoryPlayer` control surface**: `JumpTo`, `Previous`,
  `SetSection`, `ForceBranch`, `ForcePass`. All existing checks pass. Done
  2026-11-17.
- [ ] **D-03 Single input entry point** and the new key map. Half done: the UI
  goes through `PressLogical`. Still to do: route keyboard and controller
  through it, change the N key, and update the check scripts with it.
- [x] **D-04 Story panel** with timeline and subtitles. Done 2026-11-17.
- [x] **D-05 Interaction panel** (the Input tab): virtual buttons, what the
  story is waiting for, pass, skip, hear again. Done 2026-11-17.
- [ ] **D-06 Interaction timing view**, after the rules return a result
  object. The rule checks are extended to cover the new result.
- [x] **D-07 Audio panel**: sources list, toggles, pacing sliders, reset,
  record. Done 2026-11-17. A beds toggle is left out.
- [x] **D-08 Listener panel** (the Room tab) and controller-yaw head tracking.
  Done 2026-11-17; head tracking not tried with hardware.
- [x] **D-09 Controller panel** (the Pad tab) with run-time Yes/No mapping.
  Done 2026-11-17; not tried with hardware.
- [x] **D-10 Event log and missing-audio report.** Done 2026-11-17, without a
  filter.
- [ ] **D-11 Info panel** with the build stamp (needs W-13). The Demo tab
  exists and shows version, platform, frame rate and audio clock.
- [ ] **D-13 Try it by hand.** Play Scene 1 in the Editor with a real mouse:
  every tab, the sliders, the scroll bar, the timeline jump. The checks only
  simulate one click. Also check the colours, which Game view captures showed
  paler than intended.
- [ ] **D-12 Walkthrough check.** Someone who has not seen the project opens
  the build and, using only the mouse, reaches the end of Scene 1 by both
  routes. Record what confused them.

## Out of scope

- Editing the script or the staging CSV from inside the demo.
- Recording or replacing audio.
- Haptics from Unity (roadmap milestone 2) and the reusable input layer
  (milestone 3). The controller panel leaves room for both.
- Any visual for the child. The game stays audio-only.
