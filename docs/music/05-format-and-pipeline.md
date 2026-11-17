# 05. Format, checks and pipeline for AI-composed snippets

This file is the contract for agents. It covers how to write a snippet, how it is checked, how it becomes audio in Unity, and how to iterate. Evidence and alternatives considered: [ai_composition_tooling.md](../../research_notes/Music%20composition%20guidelines/ai_composition_tooling.md).

**Status.** The format below is in use. `tools/music/check.py` (H1-H8 and the call warnings W1-W8, W11) and `tools/music/render.py` exist, and Scene 3's battle runs in Unity (`StoryPlayer.Battle.cs`) from the specs in [`music/library/capitolo_01.yaml`](../../music/library/capitolo_01.yaml). The renderer uses small pure-Python placeholder instruments (`tools/music/synth.py`); FluidSynth with the house bank, the preview page and a text-driven `CallPlayer` are still to build (see [Tooling to build](#tooling-to-build)).

## Why this format

- LLMs make **arithmetic errors with durations**. In ByteComposer, GPT-4's ABC parsed 94.6% of the time, yet had time-signature errors 68.8% of the time ([ChatMusician](https://arxiv.org/html/2402.16153v1)). A **fixed-width step grid** makes wrong bar lengths a visible, countable error, and a **closed vocabulary** makes illegal call pitches impossible to write without the check noticing.
- **Plan first, then realise.** Generating form and motif before notes improves structure ([Bhandari & Colton review](https://ar5iv.labs.arxiv.org/html/2403.07995); [ComposerX](https://arxiv.org/html/2404.18081v1); [ByteComposer](https://arxiv.org/html/2402.17785v1)). The spec therefore has `form`, `motif` and `development` fields *above* the notes.
- **The LLM writes notes and structure; code writes the performance.** Velocities, accents, articulation and humanisation are applied by the renderer from fixed rules. They are never improvised per snippet.
- The same grid string is read by the **game** (expected onsets for judging, haptic cues) and by the **renderer**, so audio and gameplay can't drift apart.

## The snippet spec (YAML)

One spec per snippet. A file may also hold a YAML list of specs.

```yaml
id: sfida_piano_call_01      # unique, snake_case
role: call                   # call | answer | refrain | theme | loop | stinger | fanfare | intro | taunt | transition
stage: 2                     # 0-4, calls and answers only (see 02-calls.md)
centre: D                    # D (battle) | F (home) | G | Bb | Eb | C (see the mode wheel in 01-palette.md)
meter: 4/4                   # 4/4 (default) | 3/4 | 6/8
steps_per_bar: 8             # grid resolution of `bars`; 8 = eighths in 4/4, 6 = eighths in 3/4
tempo: 100                   # BPM; prefer 90, 96, 100, 120, 125, 128
input: open                  # open (default) | locked: whether buttons can sound during this snippet
instrument: piano            # name from the instrument table below
chords: [Dm]                 # optional for calls; one per bar; safe list only while input is open
form: "a"                    # optional: phrase letters, e.g. "a a b" (one per bar or half-bar)
motif:                       # optional plan, written BEFORE the bars
  rhythm: "x - x - x - - -"
  contour: "*dd"             # Parsons code: * start, u up, d down, r repeat
development:                 # optional: how later bars derive from earlier ones
  - {bar: 2, op: repeat, of: 1}
open_ending: false           # true for questions / taunts that deliberately do not end on the tonic
intent: "mi-do-la, walking down to the minor home; the surprise is the long last note"   # what the snippet is for, its hook and its one surprise
bars:                        # the realisation (playable layer: tokens D F A C - . only)
  - "A - F - D - - -"
```

Loops, fanfares and other backing snippets use `tracks` instead of (or as well as) `bars`:

```yaml
id: example_loop_fragment
role: loop
centre: D
tempo: 100
steps_per_bar: 8
chords: [Dm, Dm]
tracks:
  bass:
    instrument: pizzicato_strings
    layer: L0                       # L0 groove | L1 harmony | L2 companion | L3 hits | L4 fever
    bars: ["D2 . D2 A2 D2 . D2 A2", "=1"]
  pad:
    instrument: strings
    layer: L1
    bars: ["D3+F3+A3 - - - - - - -", "=1"]
  drums:
    layer: L0
    steps_per_bar: 16               # a track may use its own resolution
    lanes:
      kick:   ["x . . . . . . . x . x . . . . .", "=1"]
      snare:  [". . . . x . . . . . . . x . . .", "=1"]
      shaker: ["x x x x x x x x x x x x x x x x", "=1"]
```

### Grid tokens

| Token | Meaning | Where |
|---|---|---|
| `D` `F` `A` `C` | onset of a button pitch (D4 F4 A4 C5, or the octave-shifted set if the snippet declares `register_shift: ±12`) | `bars` of call, answer, refrain, theme, and any track marked `buttons: true` |
| `D2`, `Bb1`, `Eb3`, … | onset of an absolute pitch (scientific pitch notation, C4 = MIDI 60) | backing tracks |
| `D2+A2` | several notes at once (a chord or bordun) | backing tracks |
| `x` | percussion hit | drum lanes |
| `-` | hold the previous onset | everywhere |
| `.` | silence | everywhere |
| `"=N"` | the whole bar is a copy of bar N (1-based, an earlier bar of the same track or lane) | everywhere: makes repetition explicit and cheap |

Tokens are separated by single spaces. A bar string has **exactly `steps_per_bar` tokens**. A bar may start with `-` only to tie over from the previous bar.

## Checks

### Hard errors (the snippet is rejected)

| # | Check |
|---|---|
| H1 | every bar has exactly `steps_per_bar` tokens; every token is legal; a `-` never follows silence |
| H2 | playable-layer bars use only `D F A C - .` |
| H3 | all tracks and lanes have the same number of bars; `chords` has one entry per bar; `=N` references point backwards |
| H4 | **while input is open, no backing note has pitch class E, B, F♯, C♯ or G♯** |
| H5 | while input is open, every chord is on the safe list ([01](01-palette.md#safe-chords-computed)); a slash bass must not be a forbidden pitch class |
| H6 | while input is open, L0 and L1 notes stay outside MIDI 60–74 (C4–D5), the call register |
| H7 | two calls in the same encounter, or in the library, may not have identical contour *and* identical rhythm |
| H8 | loops are exactly N bars; no note in a loop starts after its last bar |
| H9 | a declared `development` op is really applied (e.g. `repeat` means bar 2 equals bar 1) |

### Warnings (fix, or justify in `intent`)

| # | Check | Default threshold |
|---|---|---|
| W1 | note count above the stage limit | 3 / 4 / 6 / 11 for stages 1–4 |
| W2 | shortest gap between onsets | < 250 ms through stage 3 |
| W3 | only one note length and no rest | stage ≥ 2 |
| W4 | stage 1 uses notes shorter than a quarter | — |
| W5 | the last note is neither on a strong beat (1 or 3) nor the longest | — |
| W6 | an echo call does not end on the centre's tonic (D or F) | unless `open_ending: true` |
| W7 | the aimless archetype: four different pitches, all the same length | — |
| W8 | two calls differ by only one contour or rhythm symbol | normal in stage 1 |
| W9 | backing: empty-beat rate (beats with no onset in any L0 lane) | > 0.1 |
| W10 | backing: groove consistency (1 − mean Hamming distance between bars' onset vectors, MusPy definition) | < 0.8 for L0 |
| W11 | backing: anything pitched above the call register during a call bar | — |
| W12 | backing: B♭ or E♭ sustained in the same octave as a button note | — |

These thresholds are starting values. Tune them against snippets the team likes. Metric definitions (MusPy, mgeval, MuseGAN) are in [ai_composition_tooling.md §4](../../research_notes/Music%20composition%20guidelines/ai_composition_tooling.md).

## Quality rubric

The checks catch *invalid* music. The rubric ranks *valid* candidates. Score each item 0–2, after the checks pass. The agent scores its own candidates; a human scores the shortlist.

| Item | 2 = | Applies to |
|---|---|---|
| **Hook** | the identity is clear in the first 2–4 notes; you could name it | calls |
| **One surprise** | exactly one deliberate violation, named in `intent` | calls |
| **Repetition** | a repeated note, cell or rhythm the ear can grab | calls, loops |
| **Closure** | an echo ends home and long; a question clearly *asks* | calls |
| **Sayable** | the rhythm can be spoken as a word or syllables in one breath | calls |
| **Character fit** | instrument, register, rhythm and tempo word match the character sheet | calls, intros |
| **Distinct** | far from the other calls of the same encounter (contour and rhythm) | calls |
| **Groove** | the beat is obvious without the call; the kick on 1 never moves | loops |
| **Space** | the call register is empty during call bars; the arrangement is sparse there | loops |
| **Arc** | it sounds better with each added layer, and fine with L0 alone | loops |

Ship candidates scoring ≥ 15 / 20 on their applicable items. Keep the score in the registry.

## Agent workflow

```
brief → plan (form, motif, development) → realise bars → check → repair (≤3 tries) or regenerate
      → 5–10 valid candidates → rubric ranking → top 2–3 rendered → human listens → registry
```

1. **One composer agent with tools beats a committee.** In CoComposer, a single-agent baseline did about as well as the five-agent version ([CoComposer](https://arxiv.org/html/2509.00132v1)). Use one agent plus the deterministic checker. A critic prompt, if used, receives the **metrics report**, not the raw notes.
2. **Never trust self-checking of bar lengths or pitches. Run the checker.** Feed its JSON errors back verbatim ("bar 3 has 9 steps, expected 8").
3. **Generate many, keep few.** Ask for 5–10 candidates per brief, drop the invalid ones, rank the rest.
4. **Few-shot from the library.** Short, clean, approved examples in exactly this format work better than long realistic ones. With LilyPond, simple hand-written examples raised compile rates to about 98%, while corpus examples lowered them.
5. **Humans listen.** No metric measures catchiness for children. A human must hear the top candidates rendered before they enter the registry, and ultimately children must hear them in playtests.

### Prompt template: a call

> You are composing a call for EchoQuest. Read `docs/music/01-palette.md` and `docs/music/02-calls.md` first.
> **Brief:** opponent `<name>` (character sheet: instrument, cell, rhythm, register, tempo word); battle round `<n>` of `<m>`; stage `<s>`; centre `<D|F>`; tempo `<bpm>`; form `<echo|answer|refrain>`.
> **Library:** the calls already used in this encounter are `<ids + bars>`.
> For each of 6 candidates write, in order: (1) the rhythm cell and a word that speaks it; (2) the Parsons contour; (3) the one surprise; (4) the YAML spec. Then run `tools/music/check.py` on all six, fix hard errors, and report the rubric scores of the valid ones, best first.

### Prompt template: a loop

> Compose `loop_<opp>_A` for EchoQuest. Read `01-palette.md`, `03-backing.md` and `04-battle-cues.md`.
> Exchange format `<long|short>`; centre D; tempo `<bpm>`; mood `<row of the progression table>`.
> Write in order: (1) the chord per exchange; (2) the L0 bass pattern and drum lanes; (3) the L1 voicings below C4; (4) the L3 hits; then the YAML. Every layer must sound complete over L0 alone. Run the checker, fix hard errors, and report W9 and W10.

## Library registry

All approved snippets live in one place (proposed: `music/library/*.yaml`, one file per encounter or theme), each with `id`, the spec, its rubric score, its status (`draft | approved | playtested`) and a playtest notes field. The checker runs **H7 across the whole registry** for calls, so a new call is never a disguised copy of an old one. The starter calls in [02](02-calls.md#starter-library-validated) are its seed.

## From spec to sound

### Rendering (offline, Python)

1. **Spec → MIDI** with `pretty_midi` (MIT). One MIDI file per stem.
2. **Performance rules**, applied by the renderer, never by the LLM:
   - **Call tracks (copied by the child): no humanisation at all.** Onsets exactly on the grid. Velocity accents by position: beat 1 = 100, other beats = 88, off-beats = 76, final note +6. Fixed durations: percussive instruments at 90% of the written length.
   - **Beat anchor** (kick, bass downbeats): quantised, velocity ±3 at most.
   - **Free layers** (pads, countermelody, shakers, fills): seeded humanisation. Deterministic offsets per role (hats −5 to −10 ms, snare backbeat +5 to +10 ms), plus a *correlated* random walk of ±5 ms (each offset = 0.8 × previous + small noise, not white noise; listeners preferred correlated deviations and even judged them "more precise", [Hennig et al. 2011](https://pmc.ncbi.nlm.nih.gov/articles/PMC3202537)), and velocity ±6. Notes of an eighth or shorter at 50–70% length (light staccato), long notes at 95–100%. These are practitioner starting values, not measured ones.
3. **Pad the MIDI** 2–3 s past the last note-off. FluidSynth's file render stops at the last event and cuts the tails.
4. **MIDI → WAV** with FluidSynth, as dry stems, 48 kHz, 24-bit:
   ```sh
   fluidsynth -ni -q -R 0 -C 0 -g 0.6 -r 48000 -O s24 -T wav \
     -F out/<id>_<stem>.wav sf/GeneralUser-GS.sf2 midi/<id>_<stem>.mid
   ```
   Add reverb in Unity's mixer, so every snippet shares one room and the call stays dry. Pin FluidSynth ≥ 2.4, because effect defaults changed between 2.3 and 2.4.
5. **Loops:** render "N bars + tail" and schedule each pass every N bars so the tails overlap (preferred). Alternatively render the loop twice and cut exactly `[N·bar, 2N·bar)` samples. **Never use MP3 for loops**: encoder delay and frame padding break the seam. Check automatically: exact sample length, peak ≤ −1 dBFS, no DC offset.
6. **Loudness:** master each cue to about **−18 LUFS integrated, −1 dBTP**, which is the portable convention and suits tablets and laptops. Keep cue-to-cue variation within ±2 LU.

### SoundFonts and licences

| Asset | Licence | Use |
|---|---|---|
| **GeneralUser GS** v2.0.x | its own permissive licence (commercial use OK; sample origins "not 100% certain") | **house bank** |
| FluidR3_GM | MIT | fallback with the cleanest licence |
| MuseScore_General | MIT (credits required) | alternative |
| TimGM6mb | GPL-2 (and pretty_midi's *default* bank) | **never for game assets**: always pass a SoundFont explicitly |
| Arachno | personal use only | **never** |
| FluidSynth (LGPL-2.1), abc2midi (GPL), Strudel (AGPL) | — | offline tools only, never shipped |
| mido, pretty_midi, MIDIUtil, MusPy, MeltySynth, DryWetMIDI, abcjs | MIT | fine |
| music21 | BSD-3 | fine |
| mgeval | **no licence** | reimplement its metrics; don't copy the code |

Record whatever ships, or was used for shipped renders, in a third-party notices file. Licences were checked by the researcher on the repos and sites; re-check them before release.

### In Unity (runtime)

- **Calls stay as text.** A `CallPlayer` reads the grid and schedules **one-shot samples** (one WAV per button pitch per instrument, rendered from the house bank) with `AudioSource.PlayScheduled` on `AudioSettings.dspTime`, using a pool of AudioSources, one per note. The same grid gives the judge its expected onsets and gives the motors their cue times.
- **Backing loops, stingers and fanfares** are pre-rendered stems on the same beat clock. Schedule 100–200 ms ahead ([Unity 6000.6 PlayScheduled](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/audiosource/playscheduled)). Use double precision from `dspTime`; never accumulate frame time.
- **Runtime synthesis** (MeltySynth in `OnAudioFilterRead`) only if a feature truly needs procedural backing. Test the house bank in it first: GeneralUser's docs rate TinySoundFont, the synth MeltySynth descends from, as "Poor". DryWetMIDI's playback runs on a timer, not the audio clock, so use it for editor tooling only.
- **Import settings:** one-shots as 48 kHz mono PCM, Decompress On Load, Preload; loops as Vorbis (high quality). Check `AudioSettings.outputSampleRate` on the target machines.

## General MIDI reference

0-based program numbers, as sent in code (add 1 for the printed GM tables). The same program sounds very different between banks, so **audition each one in the house bank** before locking a character's instrument.

| Role | Instrument (program) |
|---|---|
| **Call / button timbres** (fast attack, clear pitch) | Acoustic Grand Piano (0), Marimba (12), Xylophone (13), Kalimba (108), Celesta (8), Music Box (10), Glockenspiel (9, sounds high: check the bank's transposition), Vibraphone (11), Steel Drums (114), Pizzicato Strings (45), Orchestral Harp (46) |
| Soft-attack calls (add a transient) | Pan Flute (75), Ocarina (79), Flute (73), Recorder (74), Choir Aahs (52), Voice Oohs (53) |
| Bass | Acoustic Bass (32), Pizzicato Strings (45), Synth Bass 1 (38), Tuba (58) |
| Pads / harmony | String Ensemble 1 (48), Pad 2 warm (89), Choir Aahs (52) |
| Characterful backing | Bassoon (70), Clarinet (71), Tuba (58), French Horn (60), Brass Section (61), Trumpet (56), Timpani (47), Taiko (116), Melodic Tom (117), Woodblock (115), Tubular Bells (14: pitch ambiguous, effects only) |

Drum lanes (channel 10, i.e. index 9 in code): `kick` 36, `kick2` 35, `snare` 38, `rim` 37, `clap` 39, `hat` 42, `hat_open` 46, `crash` 49, `ride` 51, `tamb` 54, `cowbell` 56, `cabasa` 69, `shaker` 70 (maracas), `claves` 75, `block_hi` 76, `block_lo` 77, `tri_mute` 80, `tri` 81, `tom_low` 45, `tom_mid` 47, `tom_hi` 50. Keep the lane vocabulary to these names. Maestro Battuta's baton is `claves` (*TAC*) plus `block_lo` (*TOC*).

## Tooling to build

In priority order. Each is small.

1. **`tools/music/check.py`**: parses specs (YAML, or ```yaml blocks in Markdown), runs H1–H9 and W1–W12, and prints JSON. A draft of the hard checks plus W1–W8 was used to validate every example in these guides; it can be the starting point.
2. **`tools/music/render.py`**: spec → MIDI (pretty_midi) with the performance rules → FluidSynth stems → length, peak and loudness checks.
3. **`tools/music/preview.html`**: a local page (abcjs or SpessaSynth with the house bank) that plays candidates side by side with their metrics, for fast human review without a DAW.
4. **Unity `CallPlayer` and `LoopPlayer`**: grid → `PlayScheduled`; loops and stems by bar; the layer state machine from [04](04-battle-cues.md#adaptive-layers-vertical-layering).

Suggested first experiment (from the tooling research): build 1 and 2 for calls only, have an agent generate 20 calls from briefs, and have the team rate them blind against 5 hand-written calls.
