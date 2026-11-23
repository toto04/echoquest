# Current state of the Unity demo

Surveyed on 2026-11-16 at commit `e5902b5`, by reading the code and counting
the assets. Nothing was run in the Editor for this survey.

Since then (2026-11-17) the debug overlay was added; it closes gaps 2, 3 and 4
below in the Editor. See [debug-ui.md](debug-ui.md#what-exists-now). The rest
of this page still describes the project as surveyed.

## Summary

The project `unity/EchoQuestTest` (Unity 6000.6.8f1, URP, Input System 1.20.1)
has **three scenes that work in the Editor** and **no build pipeline**. It has
never been built as a player, for any platform.

| Scene | What it is | State |
|---|---|---|
| `Assets/EchoQuest/Scenes/ControllerTest.unity` | A critter that mirrors the controller's orientation; A changes colour, B hops | Works in the Editor with the real controller on macOS. Signs not confirmed by hand |
| `Assets/EchoQuest/Story/Scenes/Capitolo1_Pianoforte/C1_00_Intro.unity` | Prologue (`p00`) of chapter 1, linear, about 11 minutes of audio | Plays. One voice line missing (`p00_vent_01`), sounds are placeholders |
| `Assets/EchoQuest/Story/Scenes/Capitolo1_Pianoforte/C1_01_Scena1.unity` | Scene 1 (`s01`), 93 items, 8 interactions, two endings | Plays, all audio present. Checked by scripts; not yet listened through with headphones and controller |

## Code

About 2,300 lines of C# in `Assets/EchoQuest/`.

| File | Role |
|---|---|
| `Scripts/EchoQuestController.cs` | Input System device for the HID report. Matches on interface `HID` + VID + PID |
| `Scripts/CritterController.cs`, `ControllerDebugHud.cs` | Controller test scene and its IMGUI readout |
| `Scripts/Story/StoryPlayer.cs` | Plays one section of the story: voices, sounds, looping beds, ducking, pauses, branches. One coroutine, `RunSection()` |
| `Scripts/Story/StoryPlayer.Interactions.cs` | The five interaction modes and their input capture |
| `Scripts/Story/StoryInteractionRules.cs` | Pure rules: rhythm matching, missing-note timing, staging validation |
| `Scripts/Story/StoryData.cs` | CSV parsing for the sequence and the staging |
| `Scripts/Story/SpatialSource.cs` | One playing sound: position, movement, distance low-pass, ducking, fades |
| `Scripts/Story/StoryHud.cs` | IMGUI overlay: current line, status, key help. Read-only |
| `Scripts/Story/ListenerRecorder.cs` | F9 records the mixed output to a WAV |
| `Editor/TestSceneBuilder.cs`, `Editor/StorySceneBuilder.cs` | Generate the three scenes from code |

Checks live outside the project, in `tools/story_*_checks.cs`. They run inside
a live Editor through `unity command 'script run'`; the commands are in
[../scena1-interattiva.md](../scena1-interattiva.md). There are no Unity Test
Framework tests.

## Story content

The sequence CSV covers the whole chapter (322 items). Only the first two
sections have audio.

| Section | Voice lines recorded | Sounds present | Interactions | Scene |
|---|---|---|---|---|
| `p00` Prologue | 20 of 21 | 23 of 23 (placeholders) | 0 | `C1_00_Intro.unity` |
| `s01` Scene 1 | 45 of 45 | 40 of 40 (placeholders) | 8 | `C1_01_Scena1.unity` |
| `s02` | 0 of 33 | 0 of 43 | 1 | none |
| `s03` | 0 of 25 | 0 of 15 | 4 | none |
| `s04` | 0 of 24 | 0 of 12 | 0 | none |
| `s05` | 0 of 7 | 0 of 8 | 1 | none |
| `e00` Epilogue | 0 of 4 | 0 of 5 | 0 | none |

Without a recording, `StoryPlayer` keeps the line's text on screen for a reading
time and moves on. So sections `s02` to `e00` can already be stepped through as
text, which is useful for designers reading the script in context.

Interaction modes implemented, all used in Scene 1: `conferma` (press to
confirm), `ritmo` (repeat three taps), `confronto` (are two rhythms the same),
`nota` (press at the missing third note), `scelta` (yes or no picks a branch).
The interactions of `s02`, `s03` and `s05` have no staging yet, so they fall
back to the generic "press 1 to 4 to pick a branch".

## Audio assets

- 198 WAV clips are used by the scenes: 105 voice, 70 sound, 23 interaction.
  About **25 minutes** and **132 MB** of mono 16-bit PCM.
- Another 94 MB in folders starting with `_` (`_originali`, `_da_dividere`,
  `_ricevuti`) is excluded from the scenes by `StorySceneBuilder.FindClips()`,
  so it will not be in a build.
- Every clip uses the default import settings: Vorbis, quality 100%, Decompress
  On Load, **Preload Audio Data off**, no per-platform override.
- Each story scene references the full clip array, both sections' clips
  included.
- Audio files are in plain git, not LFS. The repository pack is about 198 MB.

## Input

| Source | What works today |
|---|---|
| Keyboard | Everything. Space pause, R repeat, N skip (or No during a question), Backspace restart, arrows turn the head, Home recentre, 1 to 4 note buttons, S or Y yes, N no, F9 record |
| EchoQuest controller | Buttons 1 to 4 as note buttons. Yes and No need `yesThumbButton` and `noThumbButton` set on the `Storia` object; both are 0 (unassigned), so **a controller alone cannot answer yes/no questions** |
| Controller orientation | Used only by the critter scene. The story scenes do not read yaw, pitch or roll; the listener's head turns with the arrow keys only |
| Mouse, touch, standard gamepads | Nothing |
| Haptics from Unity | Not implemented (roadmap milestone 2) |

Hardware note: the Waveshare board has two touch buttons (HID buttons 1 and 2).
The DevKitC-1 profile has eight buttons and a hat.

## Gaps that matter for a demo

1. **No entry point.** Build scene 0 is `ControllerTest`. There is no menu and
   no way to go from one scene to another at run time.
2. **No way to jump around.** Starting from a given line needs the `Start From`
   field in the Inspector. At run time there is only restart, skip one line,
   repeat one line.
3. **Keyboard only.** A visitor on a tablet, or one who does not read the key
   list, cannot play. The key list is one line of small text.
4. **The HUD is read-only and in Italian.** No tunables, no log, no view of the
   interaction's expected timing against what was tapped.
5. **`N` means both "skip" and "No".** It is resolved by context, which is
   confusing for a newcomer.
6. **Nothing has been built.** `productName` is `EchoQuestTest`, `companyName`
   is `DefaultCompany`, `runInBackground` is off, and only the macOS build
   module is installed. No build script, no CI, no `.github/` folder.
7. **The audio engine uses features the Web platform lacks.** See
   [web-build.md](web-build.md#what-is-expected-to-break-on-the-web).
8. **The controller layout cannot match in a browser.** It matches on interface
   `HID`, which the Web player never reports.

## Environment

- Unity CLI 1.0.0-beta.13 at `~/.unity/bin/unity`. Editor 6000.6.8f1 (arm64)
  with the macOS module only.
- Repository `toto04/echoquest`, private, branch `main` only. GitHub Pages is
  not enabled.
- Project settings already hold Web defaults: Brotli compression, no
  decompression fallback, default template, 32 MB initial memory.
