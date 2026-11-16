# Debug UI and demo shell

Last updated: 2026-11-16. Status: **planned, nothing implemented.**

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
4. **Code only.** No hand-edited scenes or prefabs; builders add the
   components, as they do today.
5. **Works in the Web build.** No file access, no editor-only APIs outside
   `#if UNITY_EDITOR`.

## Technology

Recommendation: **stay on IMGUI** (`OnGUI`), which both existing HUDs use.

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

`StoryPlayer` exposes almost nothing: `Restart`, `TogglePause`, `Skip`,
`Repeat`, and read-only state. Input is read straight from `Keyboard.current`
in `Update` and from raw events in `CaptureInteractionInput`.

| Need | Change |
|---|---|
| Jump to any item | `JumpTo(string id)`: restart `RunSection` from that index, stop all sources, reset branch state |
| Switch section and take at run time | `SetSection(string)`, `SetTake(string)`; today they are Inspector fields read once |
| Force a branch | Public setter for `BranchChoice` |
| On-screen buttons | One input entry point, `PressLogical(int button, double time)`, fed by keyboard, controller and UI. It replaces the direct keyboard reads and feeds `m_InteractionInput` |
| Show what an interaction heard | Keep the last attempt: tap times, expected times, per-gap error, pass or fail and the reason. `StoryInteractionRules` returns a result object instead of a bool |
| Force pass or fail | `ResolveInteraction(bool passed)` for demo use |
| Event log | A `StoryEvent` callback: item started, clip played, clip missing, input received, branch chosen |
| Live tunables | The pacing fields are already public; the panel edits them. Add a reset to defaults |
| Missing-audio report | `StoryPlayer` already knows which ids have no clip; expose the list |

Keep the pure rules pure. `tools/story_interaction_checks.cs` and the other
check scripts must keep passing after each refactor.

## Panels

A top bar with tabs. Each tab is one small class implementing a common
`IDebugPanel { string Title; void Draw(); }`.

### Story

- Transport: play/pause, previous item, next item, repeat, restart.
- Section and take selectors.
- **Timeline**: scrollable list of every item in the section with id, speaker,
  first words, kind, branch. Current item highlighted. Click to jump. Items
  with no audio are marked.
- Current line in full, with a **subtitles** toggle that shows the line large,
  for tutors who want to read along.
- Branch: the current choice, and buttons to force A or B.

### Interaction

Shown when the current item is an interaction.

- Mode, prompt id, which buttons are accepted, attempts used.
- **Virtual buttons**: 1, 2, 3, 4, Yes, No. Mouse or touch.
- For rhythm and missing-note: a time strip with the expected taps and the
  received taps, the error of each gap in milliseconds, and the tolerance band.
- Pass, fail, skip buttons.

### Audio

- Master volume and mute.
- Active sources: label, clip, volume, gain after ducking, distance, low-pass
  cutoff, looping, time left.
- Toggles: reverb, distance low-pass, ducking, beds. On the Web, the ones that
  have no effect are shown as unavailable (task W-07).
- Pacing sliders: the five pause lengths, duck gains, fade time.
- Record to WAV (Editor and desktop only).

### Listener and room

- The existing top-down view stays as the background of the story scene.
- Head yaw slider and recentre. Option to **drive the head from the
  controller's yaw**, which the story scenes do not do today.
- Staging of the current item: position, movement, volume.

### Controller

- The content of `ControllerDebugHud`: device, report rate, raw bytes, buttons,
  hat, angles, device list.
- Mapping of the Yes and No thumb buttons, editable at run time. Today both are
  unassigned, so a controller cannot answer questions.
- Recentre yaw.
- Haptics test: motor, intensity, duration. Shown as "not implemented" until
  roadmap milestone 2.

### Log and info

- The event log, newest first, with a filter.
- Missing-audio report for the current section.
- Build stamp, platform, browser, frame rate, audio sample rate, audio clock.

## Keys

- **F1** or **`** toggles the overlay. Also a small corner button, since the
  Web visitor may not read instructions.
- **Esc** goes back to the menu.
- Fix the double meaning of **N**, which is "skip" during speech and "No"
  during a question. Proposal: N stays "No" and skip moves to **.** (full
  stop); the arrow keys keep turning the head. Settle it in D-03 and update
  `Story/README.md` and the HUD help line.

## Tasks

### Shell

- [ ] **S-01 `DemoSceneBuilder` and `Demo.unity`**, first in the build list.
  Done when a build opens on it.
- [ ] **S-02 Click-to-start screen.** No audio before the first input.
- [ ] **S-03 Menu and scene switching**, with back-to-menu everywhere. Done
  when all three existing scenes are reachable and leavable without a reload.
- [ ] **S-04 One story scene parameterised by section**, replacing the
  per-section scenes, with unrecorded sections listed as text only.

### Debug UI

- [ ] **D-01 Overlay framework**: `DebugOverlay` component, tabs, scale,
  toggle key and corner button, `IDebugPanel`. Move `ControllerDebugHud` and
  `StoryHud` onto it.
- [ ] **D-02 `StoryPlayer` control surface**: `JumpTo`, `SetSection`,
  `SetTake`, branch setter. All existing checks pass.
- [ ] **D-03 Single input entry point** and the new key map. Keyboard,
  controller and UI all go through `PressLogical`. All existing checks pass.
- [ ] **D-04 Story panel** with timeline and subtitles.
- [ ] **D-05 Interaction panel**: virtual buttons and force pass/fail.
- [ ] **D-06 Interaction timing view**, after the rules return a result
  object. The rule checks are extended to cover the new result.
- [ ] **D-07 Audio panel**: sources list, toggles, pacing sliders.
- [ ] **D-08 Listener panel** and controller-yaw head tracking.
- [ ] **D-09 Controller panel** with run-time Yes/No mapping.
- [ ] **D-10 Event log and missing-audio report.**
- [ ] **D-11 Info panel** with the build stamp (needs W-13).
- [ ] **D-12 Walkthrough check.** Someone who has not seen the project opens
  the build and, using only the mouse, reaches the end of Scene 1 by both
  routes. Record what confused them.

## Out of scope

- Editing the script or the staging CSV from inside the demo.
- Recording or replacing audio.
- Haptics from Unity (roadmap milestone 2) and the reusable input layer
  (milestone 3). The controller panel leaves room for both.
- Any visual for the child. The game stays audio-only.
