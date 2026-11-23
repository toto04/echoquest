# Roadmap and status

Last updated: 2026-11-17.

## Where we are

### Done

| Area | What exists | How it's verified |
|---|---|---|
| Firmware: BLE HID gamepad | Bonded HID-over-GATT gamepad on NimBLE, input report (buttons, hat, yaw/pitch/roll), vendor haptic output report, haptics stop on disconnect, bond clearing | Builds with no warnings; native unit tests; **confirmed working on hardware** (pairing, `hid_probe.py monitor` and `haptic`) |
| Protocol v1.0 | [`protocol/hid-report.md`](../protocol/hid-report.md), 98-byte descriptor, PID `0x0001` | Native tests check that descriptor bit counts match the report sizes; probe self-test |
| Host probe tool | `tools/hid_probe.py`: list, monitor, haptic, descriptor, selftest | Self-test passes; used against real hardware |
| DevKitC-1 profile | 8 GPIO buttons, 4-way hat, `FakeImu`, RGB LED haptic stand-in | Hardware-tested |
| Unity test scene | `unity/EchoQuestTest` (Unity 6000.6.8f1): custom Input System layout for the controller, a critter that mirrors orientation, A = colour, B = hop | Runs in the Editor with the real controller on macOS; decoded values checked against raw report bytes |
| Waveshare ESP32-C6-Touch-LCD-1.47 profile | Real QMI8658A IMU with Mahony fusion, LCD status screen, 2 touch buttons, default env | Builds; orientation signs unit-tested. Hardware checklist in the [README](../README.md#imu-axes-check-once-per-board-design) |

### Known gaps and things to check on hardware

- **Waveshare board checklist** (from the README): IMU axis signs
  (`IMU_AXIS_MAP`), touch-to-screen mapping, LCD colours and orientation, and
  whether the BOOT button is GPIO 9 or 8.
- **Yaw can drift during motion** (no magnetometer). At rest it is held and the
  gyro bias re-learned (fixed 2026-11-04: before that, yaw crept steadily even at
  rest). Revisit when the game's use of yaw is known.
- **Haptics are a stub.** No motor driver yet; the stub logs, toggles a GPIO and
  shows a bar on the LCD. Pattern IDs 1–255 are reserved but play as constant.
- **VID/PID are in the pid.codes test range.** A real PID is needed before any
  distribution.
- **Unity test scene: signs and feel not yet confirmed by hand** (see
  [unity-test-scene.md](unity-test-scene.md#done-when)).

## Next milestones

### 1. Unity test scene (done, pending hand check)

A small, playful Unity scene that proves the controller works end to end
inside Unity: an object that rotates with the controller's orientation and
reacts to the two buttons. Details: [unity-test-scene.md](unity-test-scene.md).

Exit criteria:
- [x] The controller is detected by the Unity Input System on macOS. Windows not tried yet.
- [x] Buttons and yaw/pitch/roll decode with the correct scale.
- [ ] Signs confirmed by hand (turning right turns the critter right, and so on).
- [x] Findings about how Unity exposes the device are written down
  ([unity-test-scene.md](unity-test-scene.md#how-unity-sees-the-controller-findings-2026-11-04-macos)).

### 2. Haptics from Unity

Send output report ID 2 from Unity (through the Input System if it can send
HID output reports on our platforms, otherwise through a small native or
hidapi-based plugin). Unity reports `outputReportSize 9` for the device, as
documented, which is a good sign. Exit: a Unity button press makes the controller's haptic
stub fire, as `hid_probe.py haptic` does today.

### 3. Unity input layer for the game

A reusable C# module that mirrors `protocol/hid-report.md`, growing out of the
test scene's `EchoQuestController` layout: device detection,
decoded state (buttons, orientation in degrees), yaw recentering, haptic
commands, and connection/disconnection events that the game can turn into sound.

### 4. Real haptics hardware

Choose motors and a driver, implement a `HapticDriver` for them, and define
the first vibration patterns (pattern IDs 1+). Keep the disconnect failsafe.

### 5. Game prototype

Once the game design questions in [vision.md](vision.md#open-questions) are
answered: a first playable audio-only prototype.

### Scene 1 participation (implemented 2026-11-11; listening check pending)

The user approved the five Scene 1 changes: teach Tocco earlier, repeat two
rhythms, answer the rhythm comparison, press at the missing note, and answer
the Maestro's request for help. The optional Intro interaction is outside this scope.
The player now supports confirmation, rhythm comparison, rhythm reproduction
and a scheduled missing-note exercise. These are wired into the 93-item Scene 1; Yes/No use the thumb controls, separate from notes.

- [x] Inspect the current sequence and identify reusable recordings.
- [x] Prepare the implementation plan and 11 new lines, all with Antonio Farina.
- [x] Import the [received lines](audio-da-completare-scena1.md) and shorten repeated button instructions.
- [x] Separate note keys 1–4 from Yes/No (Y/N); leave physical thumb mapping unassigned until hardware testing.
- [x] Implement interaction timing, retries and input handling, with pure-rule and Play-mode checks.
- [x] Edit script/audio, regenerate CSVs/scenes and complete both final branches in Play tests.
- [ ] Verify the complete flow in headphones with the controller.

See [scena1-interattiva.md](scena1-interattiva.md) for behavior, audio texts,
implementation decisions and acceptance checks. The current story has 93 items and has passed both route checks. Full listening and physical controller checks remain.

### Scenes 2 and 3: the piano and the first battle (implemented 2026-11-17; listening check pending)

Scenes 2-5 of the script were rewritten for more frequent interaction, following
`docs/music/`. Scenes 2 and 3 are now playable in Unity with all their voices,
sounds and music; the chapter's scenes load one after another.

- [x] Voices for Scenes 2-3 (ElevenLabs, same voices and settings as before).
- [x] Music as grid specs in `music/library/capitolo_01.yaml`, checked by
  `tools/music/check.py` (palette, forbidden notes, call register, call warnings) and
  rendered by `tools/music`; Scene 2-3 sounds from `tools/story_placeholder_sfx.py`.
- [x] Scene 2: the index plays the piano, piano/forte (answer Yes), the register choice
  with index/middle (whole button set one octave down or up), the Maestro's rhythm on the piano.
- [x] Scene 3: the first battle against Sbuffo (06-worked-example.md): count-in, a loop that
  never stops, long exchanges with the TOC cue, layers that grow with success, comic taunt
  on failure, an answer round where the child plays the stolen F, fanfare on the bar line.
- [x] Rule checks (80) and Play-mode checks of both scenes; Scene 1 checks still pass.
- [ ] Listen in headphones; tune levels and the synthesised placeholder instruments
  (FluidSynth and a GeneralUser GS bank would replace them, 05-format-and-pipeline.md).
- [x] Scene 4 playable: voices, sounds, free play (`libero`), the Sbuffo question (2026-11-18).
- [x] No more timed presses in a silence (too hard): Scene 1 compares memory and today's piano,
  Scene 3's round 3 is the documented echo La... Fa-Re, the epilogue repeats TA... TA-TA (2026-11-18).
- [x] Intro and Scene 1 voices: pauses no longer split words; two lines re-recorded (2026-11-18).
- [x] Scene 5 and the epilogue playable (2026-11-19): voices (items 56-70), the tamburo on the
  left and the campanella on the right (index / middle choice, the register is kept), the
  epilogue's last TA... TA-TA exchange as free play (nothing counted, silence is fine), the
  Maestro's laugh and the whole song fading out. The chapter now plays from the Intro to the epilogue.
- [x] The chapter plays without breaks: sections follow one another in the same Unity scene (no scene
  load); the debug overlay picks a scene and can stop at its end (2026-11-19).
- [x] Four instruments, one per finger: the campanella replaces the prologue's triangle, the flauto
  is announced in the epilogue as the last stage (2026-11-19).

See [scena2-3-interattive.md](scena2-3-interattive.md).

### Chapters 2 and 3: the tamburo and the campanella (scripts written 2026-11-19; to review)

One chapter per instrument; the Scene 5 choice decides which comes first, and each works in either
order. Both climb from stage 0 to stage 3 with an optional stage-4 extra level, with more interactions
than all of chapter 1. Plan, interaction ladder, menu and what to build: [capitoli-2-3.md](capitoli-2-3.md).

- [x] Scripts (`capitolo_02_il_tamburo.md`, `capitolo_03_la_campanella.md`) and music specs
  (`music/library/capitolo_02_tamburo.yaml`, `capitolo_03_campanella.yaml`, 0 checker errors).
- [ ] Team review of the scripts and the open questions in capitoli-2-3.md.
- [ ] Chapter ids in the extractor, audio menu, saved progress, the songbook.
- [ ] New interaction modes (battito, riconosci, conta, risposta libera, colpo) and battle features
  (short exchanges, help loop, loop changes, attack round, recovered-instrument layers).
- [x] Voices (115 lines with ElevenLabs, `docs/registrazioni-capitoli-2-3.txt`) and sounds: the tamburo and
  the campanella are real CC0 recordings (VCSL timpani and hand bells) tuned to the button pitches;
  ids carry the chapter letter (`tools/story_extract.py --section-letter`) (2026-11-19).
- [x] Chapter 2 (tamburo) playable in Unity from the Prologo to the Epilogo (2026-11-23): regia, scenes in
  `Story/Scenes/Capitolo2_Tamburo`, new interaction modes (`battito`, `eco`, `colpo`, attack round, short exchanges,
  loop changes), Play-mode checks in `tools/story_tamburo_runtime_checks.cs`; chapter 1 goes on into it
  when the child picks the drum.
- [ ] Regia, Unity scenes and Play-mode checks for chapter 3 (campanella).

### Shareable web demo (planned 2026-11-16; debug overlay started 2026-11-17)

A demo that tutors and designers can open from a link: an automated Web build
of the Unity project and a debug UI for trying everything by mouse. The survey,
the plan, the open decisions and the task lists are in
[demo/](demo/README.md).

- [x] Survey the Unity project and write the plan.
- [ ] Decide hosting, audio visibility, where the build runs, and how reverb is handled on the Web.
- [ ] Web spike: build once, test audio and input in a browser (W-01 to W-04).
- [ ] Demo shell: first scene, click-to-start, menu (S-01 to S-04).
- [ ] Web-ready runtime and automated build and deploy (W-05 to W-14).
- [ ] Debug UI (D-01 to D-13). First version works in the Editor: seven tabs,
  mouse-driven, 17 new Play-mode checks, all older checks still pass. Open:
  timing strip, key map, build stamp, a walkthrough by hand.

### Repository cleanup (completed 2026-11-12)

Keep runtime behavior, HID reports, story IDs, audio sources and existing local
changes intact. Remove files only after checking references and build inputs.

- [x] Remove the empty root Unity project and generated solution. The project
  remains in `unity/EchoQuestTest`; ignore rules prevent accidental root imports.
- [x] Remove the template scene and build entry. Preserve referenced render/input
  assets, audio originals and the local recovery scene.
- [x] Remove two unread fields, two unused synthesizers, duplicated WAV helpers
  and redundant comments. Later-section choices and hardware profiles remain.
- [x] Remove nine unloadable test subassets from the volume profile and seven
  obsolete renderer references through Unity. Preserve the 19 active volume
  components, renderer masks, render modes, stencil and SSAO settings.
- [x] Verify Unity compilation, 50 rule/configuration checks, 34 interaction
  checks, paused-audio regression and all three Scene 1 routes. Verify 22 native
  firmware tests, probe self-test and three Python audio tests. The 45 generated
  Scene 1 WAVs and four generated CSVs remain byte-identical.
- [x] Resolve all remaining scene/material/render-pipeline references and
  preserve the 93-item Scene 1 with 194 audio clips.

No dependency, firmware behavior or architecture changes. ESP32 target builds
were not rerun: the changed firmware lines are comments only; native tests pass.

### Later

- Controller form factor, battery and power management, and how battery state
  is shown to a blind player.
- A custom PCB as a new `BOARD_*` profile.
- Possibly a magnetometer for absolute heading, if the game needs it.
- A real USB VID/PID allocation.
