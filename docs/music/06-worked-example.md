# 06. Worked example: the orchestra's song and the piano challenge

This example shows the whole system on one encounter: a draft of the **orchestra's song** (the spine, see [04](04-battle-cues.md#the-orchestras-song-as-the-spine-strong-recommendation)) and a complete asset set for the **first piano challenge** (script Scenes 3–4). Every spec passes the draft checker ([05](05-format-and-pipeline.md#checks)). **None of it has been rendered or played to a child.** It shows the method; it is not a finished score. Agents iterating on the demo should treat it as the reference shape and replace the notes freely.

## 1. "La canzone del concerto" (draft)

Requirements, derived from the script and the guides:

- The **melody uses only the four buttons**, so by the end of the game the child can play the whole song.
- It is centred on **F**: it is the orchestra's happy song, home.
- It opens with **three recognisable notes** (prologue: "il pianoforte apre con tre note riconoscibili"). If the *last* of those three goes missing, the gap must be obvious. So the head falls to the tonic, `C A F` (so–mi–do), and losing the F leaves the phrase hanging on mi.
- It contains the script's first rhythm challenge, *TA… TA-TA*, so the battle trains a piece of the real song.
- Its form is a a′ b a / c: four 2-bar pieces, one per recovered instrument. The mapping is a proposal for the story team.

| Bars | Piece | Instrument that recovers it | Content |
|---|---|---|---|
| 1–2 | the head + *TA… TA-TA* | pianoforte | so–mi–do, then la… do-la |
| 3–4 | the galloping rhythm | tamburo | ti-ti ta, ti-ti ta |
| 5–6 | the head returns | flauto (proposal) | repetition = memorability |
| 7–8 | the so–mi call and the close | campanella | the playground third reclaimed, then home |

```yaml
id: theme_canzone_del_concerto
role: theme
centre: F
tempo: 100
meter: 4/4
steps_per_bar: 8
chords: [F, Dm7, Bb, Csus4, F, Dm7, Gm7, F]
form: "a b c d a b e f"
intent: "the stolen song; every battle call in chapter 1 derives from one of its pieces"
bars:
  - "C - A - F - - ."   # 1 head: so-mi-do (the piano's third note is the one Sbuffo took)
  - "D - . . F D - ."   # 2 TA... TA-TA: la ... do-la, ends open on la
  - "A A C - A A F -"   # 3 the drum's gallop (s3_galoppo)
  - "C - - - A - - ."   # 4 held so, falls to mi: half-cadence feel
  - "=1"                # 5 head again
  - "=2"                # 6
  - "C A C A C - A -"   # 7 the bell's so-mi figure
  - "A - C - F - - -"   # 8 mi-so, then the falling fifth home to do
```

The harmony is one chord per bar here, because the theme is not an echo exercise. Over these chords every bar is consonant by construction. Bar 2 over Dm7 makes the *TA… TA-TA* land on the minor tonic: a small shadow inside the happy song, and the colour the battle version will live in.

## 2. Character sheet for this encounter

| | Sbuffo (challenger) | Pianoforte (the stake) |
|---|---|---|
| Instrument | pan flute with a breathy flutter and a short wooden transient; slide whistle in locked bars | piano (GM 0) |
| Cell | `C A D` (4 3 1), short-short-long | the head `C A F` |
| Register | the call register for calls; glides above it only in locked bars | the call register (or the octave the child chose in Scene 2) |
| Tempo word | *dispettoso* (cheeky): staccato, comic stops | *cantabile* |
| Calls in this battle | played **on the piano timbre**: at stage 1–2 the child copies exactly the sound they will produce (opinion: matching timbre removes one mapping step); Sbuffo's own timbre is used for the intro and taunt | — |

Battle settings: **stage 1→2**, **long exchanges** (call · call · echo · reply), **centre D** (Sbuffo's world), **100 BPM**, two-chord ostinato `Dm | C5`.

## 3. The calls: one rhythm, pitches added round by round

The script's first challenge is a **one-button rhythm echo** on the index finger, which is D4 ("un suono… una pausa… e due suoni!"). Following the "rhythm first, pitches second" rule, the battle keeps that rhythm and adds a button per round, ending on bar 2 of the real song.

```yaml
- id: sfida_piano_r1
  role: call
  stage: 2
  centre: D
  tempo: 100
  instrument: piano
  intent: "script Scene 3, TA... TA-TA on the index button only; judged by structure (order + relative rhythm)"
  motif: {rhythm: "x - . . x x - .", contour: "*rr"}
  bars: ["D - . . D D - ."]
- id: sfida_piano_r2
  role: call
  stage: 2
  centre: D
  tempo: 100
  instrument: piano
  intent: "same rhythm, the middle note moves up to the middle button: la ... do-la (= song bar 2)"
  development: [{op: repitch, of: sfida_piano_r1}]
  bars: ["D - . . F D - ."]
- id: sfida_piano_r3
  role: call
  stage: 2
  centre: D
  tempo: 100
  instrument: piano
  intent: "Sbuffo's scrambled version starts from the top: mi ... do-la; the falling third answer"
  bars: ["A - . . F D - ."]
```

The checker reports r2 and r3 as *close*: they differ in one contour symbol, with the same rhythm. That is intended within one battle, where the sequence of rounds *is* the development. It would not be acceptable as calls for two different opponents.

## 4. Backing loop A

The 8-bar loop holds 2 long exchanges with one chord each (`Dm` for bars 1–4, `C5` for bars 5–8). Bar layout per exchange: 1 = call, 2 = call again, 3 = echo, 4 = reply. During call bars only L0 plays; L1 and L3 are written here and muted or ducked by the layer state machine.

```yaml
id: loop_sfida_piano_A
role: loop
centre: D
tempo: 100
meter: 4/4
steps_per_bar: 8
chords: [Dm, Dm, Dm, Dm, C5, C5, C5, C5]
intent: "two-chord ostinato, broken bordun, marcia percussion; drive from the woodblock eighths, not the tempo"
tracks:
  bass:
    instrument: pizzicato_strings
    layer: L0
    bars:
      - "D2 - A2 - D2 - A2 -"
      - "=1"
      - "=1"
      - "D2 - A2 - D2 A2 D3 A2"
      - "C2 - G2 - C2 - G2 -"
      - "=5"
      - "=5"
      - "C2 - G2 - A2 - C3 -"     # walk-up back to D2 at the loop restart
  pad:
    instrument: strings
    layer: L1
    bars:
      - "D3+F3+A3 - - - - - - -"
      - "=1"
      - "=1"
      - "=1"
      - "C3+G3 - - - - - - -"
      - "=5"
      - "=5"
      - "=5"
  hits:
    instrument: timpani
    layer: L3
    bars:
      - ". . . . . . . ."
      - "=1"
      - "D2 . . . . . . ."         # beat 1 of the echo bar: "go!"
      - "=1"
      - "=1"
      - "=1"
      - "C2 . . . . . . ."
      - "=1"
  drums:
    layer: L0
    steps_per_bar: 16
    lanes:
      kick:      ["x . . . . . . . x . . . . . . .", "=1", "=1", "=1", "=1", "=1", "=1", "=1"]
      block_hi:  [". . . . x . . . . . . . x . . .", "=1", "=1", "=1", "=1", "=1", "=1", ". . . . x . . . . . . . x . x x"]
      shaker:    ["x . x . x . x . x . x . x . x .", "=1", "=1", "=1", "=1", "=1", "=1", "=1"]
```

## 5. The one-off cues

```yaml
- id: intro_sbuffo
  role: intro
  input: locked
  tempo: 100
  steps_per_bar: 8
  chords: [Dm, Bbm]
  intent: "Sbuffo arrives: his cell on pan flute, then the minor-third villain slide; locked, so Bbm's Db is allowed"
  tracks:
    sbuffo:
      instrument: pan_flute
      layer: L3
      bars: ["C5 C5 A4 - D4 - - -", ". . . . . . . ."]
    strings:
      instrument: strings
      layer: L1
      bars: ["D3+F3+A3 - - - - - - -", "Db3+F3+Bb3 - - - - - . ."]
    whoosh:
      layer: L3
      steps_per_bar: 8
      lanes:
        slide_whistle_up: ["x - - - . . . .", ". . . . . . . ."]
- id: count_in
  role: intro
  tempo: 100
  steps_per_bar: 8
  intent: "game-wide constant: Maestro's baton TAC, TAC... TOC (two close taps, a rest, one low tap, as in the script), plus his voice 'uno, due, tre, VIA'"
  tracks:
    baton:
      layer: L0
      lanes:
        claves:   ["x - x - . . . ."]
        block_lo: [". . . . . . x -"]
- id: turn_cue
  role: stinger
  meter: 1/4
  steps_per_bar: 2
  tempo: 100
  intent: "the TOC on beat 4 of the last call bar; identical in every battle of the game"
  tracks:
    baton:
      layer: L0
      lanes:
        block_lo: ["x -"]
- id: sting_ok
  role: stinger
  meter: 1/4
  steps_per_bar: 4
  tempo: 100
  intent: "verdict on beat 4 of the echo bar: rising button arpeggio, fits any safe chord"
  tracks:
    sparkle:
      instrument: celesta
      layer: L2
      buttons: true
      bars: ["D F A C"]
- id: sting_notyet
  role: stinger
  meter: 1/4
  steps_per_bar: 2
  tempo: 100
  intent: "one soft low note, not a buzzer"
  tracks:
    soft:
      instrument: marimba
      layer: L2
      buttons: true
      bars: ["D -"]
- id: taunt_sbuffo
  role: taunt
  input: locked
  tempo: 100
  steps_per_bar: 8
  chords: [Gm7, Bbmaj7]
  intent: "Sbuffo laughs: staccato bassoon 'ha-ha-ha' falling, then a fast trombone wobble (pitch-bend in the renderer); snare pickup back to bar 1"
  tracks:
    bassoon:
      instrument: bassoon
      layer: L3
      bars: ["D3 . D3 . C3 . A2 -", ". . . . . . . ."]
    trombone:
      instrument: trombone
      layer: L3
      bars: [". . . . . . . .", "F2 - D2 - Bb1 - - ."]
    drums:
      layer: L0
      steps_per_bar: 16
      lanes:
        snare: [". . . . . . . . . . . . . . . .", ". . . . . . . . . . . . x . x x"]
- id: fanfare_win_piano
  role: fanfare
  input: locked
  tempo: 100
  steps_per_bar: 12
  chords: ["Bb C5", "D"]
  intent: "Mario-like whole-step lift Bb-C-D in triplets, parallel fifths; the final D major (Picardy) hit is allowed because input is locked; ~2.4 s + ring"
  tracks:
    brass_top:
      instrument: brass_section
      layer: L3
      bars: ["F4 F4 F4 F4 - - G4 G4 G4 G4 - -", "A4 - - - - - - - - - - ."]
    brass_low:
      instrument: brass_section
      layer: L3
      bars: ["Bb3 Bb3 Bb3 Bb3 - - C4 C4 C4 C4 - -", "D4+F#4 - - - - - - - - - - ."]
    timpani:
      instrument: timpani
      layer: L3
      bars: ["Bb1 . . . . . C2 . . . . .", "D2 - - - - - - - - - - ."]
- id: reprise_piano
  role: loop
  centre: F
  tempo: 100
  steps_per_bar: 8
  chords: [F, Dm7, F, F]
  intent: "victory: the piano plays song bars 1-2 complete (the stolen F is back), twice, closing with the song's final cadence; Sbuffo's cell returns softly as a redeemed countermelody in the last bar"
  tracks:
    piano:
      instrument: piano
      layer: L2
      buttons: true
      bars: ["C - A - F - - .", "D - . . F D - .", "=1", "A - C - F - - -"]
    bass:
      instrument: acoustic_bass
      layer: L0
      bars: ["F2 - C3 - F2 - C3 -", "D2 - A2 - D2 - A2 -", "=1", "C2 - G2 - F2 - - -"]
    pad:
      instrument: strings
      layer: L1
      bars: ["F2+C3+A3 - - - - - - -", "C3+F3+A3 - - - - - - -", "=1", "F2+C3+A3 - - - - - - -"]
    sbuffo_redeemed:
      instrument: pan_flute
      layer: L4
      bars: [". . . . . . . .", "=1", "=1", ". . . . C6 A5 D5 -"]
```

The fanfare's `D4+F#4` is the only forbidden pitch class anywhere in the example, and it sits in a locked bar. Allowing that exception is exactly what `input: locked` is for. In bar 4 of the reprise, Sbuffo's `C A D` an octave up *answers* the song's cadence. The challenger joins the music instead of being destroyed, as the script's development notes suggest for the ending.

## 6. Running order (what the game schedules)

| Time (bars) | Cue | Notes |
|---|---|---|
| — | narration: Maestro presents the piano and plays `sfida_piano_r1` alone | input waits; no timer |
| 1–2 | `intro_sbuffo` | locked |
| 3 | `count_in` | |
| 4… | `loop_sfida_piano_A` from its bar 1 | |
| exchange 1 | `sfida_piano_r1` on call bars 1–2, `turn_cue` on beat 4 of bar 2, the child echoes in bar 3, `sting_ok` on beat 4 of bar 3, reply in bar 4 | success adds L1 at the next bar line |
| exchange 2 | `sfida_piano_r2` | success adds L3 |
| exchange 3 | `sfida_piano_r3` | success: "almost" (`loop_C` in a fuller battle) |
| failure, any round | `taunt_sbuffo` at the next exchange boundary, then the same call | second failure: `loop_B` plus sung note names plus haptic pulse |
| win | `fanfare_win_piano` on the downbeat after the last echo bar, then `reprise_piano` | Scene 4: "la prima nota è tornata" |

## 7. What to iterate first

The cheapest experiments to run with this material, before writing more:

1. **Render it** and listen to the r1–r3 sequence over loop A at 100 and 120 BPM. Is the *TA… TA-TA* gap clear? Does the walk-up in bar 8 lead back convincingly?
2. **The head test.** Play `C A F` and then `C A (silence)` to adults and, when possible, children. Is the missing note *felt*? If not, try `C A F` with F as a long note on beat 3.
3. **Timbre test.** Calls on the piano versus on Sbuffo's pan flute, for a 6-year-old echoing on a piano button.
4. **Loop B and C variants** for this battle, generated by an agent from the templates in [05](05-format-and-pipeline.md#prompt-template-a-loop).
