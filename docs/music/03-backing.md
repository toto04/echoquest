# 03. Backing: harmony, bass, drums and texture under the calls

The backing is the "world" layer ([01-palette.md](01-palette.md#two-layers-playable-and-world)). Its jobs, in priority order:

1. keep a **pulse that never stops**, because continuing a beat without a pacing sound is where young children are weakest;
2. set the **tonal centre** (D = battle, F = home);
3. give **mood and energy**;
4. **reward** progress by getting fuller.

It must never compete with the call. Sources for this file: [battle_and_adaptive_music.md](../../research_notes/Music%20composition%20guidelines/battle_and_adaptive_music.md), [melody_motif_craft.md §8](../../research_notes/Music%20composition%20guidelines/melody_motif_craft.md), and the chord computation in [01](01-palette.md#safe-chords-computed).

## Three levels of harmonic safety

| Level | What the backing does | Every button consonant? | When |
|---|---|---|---|
| **1. Bordun / drone** | open 5th D–A (or F–C), no third | always | stage 0–1, tutorials, the first battles |
| **2. Safe progression** | chords from the safe list, one per exchange | always, by construction | the default for battles |
| **3. Locked colour** | anything, including chromatic "villain" chords | n/a: buttons can't sound | intros, taunts, transitions, the final hit |

## Harmonic rhythm: one chord per exchange

An **exchange** is one call bar plus one echo bar. **The call and its echo sit on the same chord**, so the child's ending sounds exactly like the enemy's. An 8-bar loop therefore holds **4 chords**, each lasting 2 bars. That is slow enough to stay stable and fast enough to move. A chord may change *inside* an exchange only in locked bars or in a non-echo section (attack round, victory).

## Progressions by mood (all safe while input is open)

Each cell is one 2-bar exchange. `/D` means a D pedal in the bass.

| Mood | 8-bar loop (4 exchanges) | Why it works | Stage |
|---|---|---|---|
| **Tutorial drone** | `Dm` `Dm` `Dm` `Dm` | Orff bordun: nothing can go wrong | 0–1 |
| **Two-chord ostinato** | `Dm` `C5` `Dm` `C5` | i–♭VII; the battle≈dungeon stasis typical of JRPG battles | 1–2 |
| **Heroic drive** (default battle) | `Dm` `B♭` `C5` `Dm` | Aeolian i–♭VI–♭VII–i, the minor "rock cadence" | 2+ |
| **Mischief, sneaky** | `Dm7` `Gm7` `Dm7` `Gm7` | i–iv rocking; pizzicato bass, staccato bassoon | 1+ |
| **Mischief, cheeky** | `Dm7` `G5` `Dm7` `G5` | a Dorian-like lift without the forbidden B | 2+ |
| **Boss menace** | `Dm` `E♭maj7♯11` `Dm` `Gm7` | the Phrygian ♭II as the boss signature. Use only for bosses so it keeps its meaning | 2+ |
| **Final boss** | `Dm` `B♭/D` `Gm7/D` `Csus2/D` | upper chords move over a timpani/low-string D pedal: the anchor never moves | 3+ |
| **Wonder / magic** | `E♭maj7♯11` `F/E♭` `E♭maj7♯11` `F` | Lydian float that lands on F | any |
| **Tender** (Sbuffo's sadness) | `Gm7` `E♭maj7♯11` `B♭` `F` | G Aeolian, warm, descending bass | story |
| **Victory reprise** (F) | `F` `B♭` `Dm7` `Csus4` / `F` `E♭maj7♯11` `B♭` `F` | relative major; ♭VII–IV–I Mixolydian close | after a win |
| **"Try again"** | `Gm7` → `B♭maj7` (1 bar each, then a snare pickup into `Dm`) | unresolved and warm, never tragic | after a failed phrase |

Two rules for **E♭maj7♯11**: put E♭ in the bass, keep the voicing low (E♭2 B♭2 G3), and let the buttons supply D and A on top. Never stack a low D against the E♭.

### Colour that only goes in locked bars

These are the strongest dramatic devices, and each clashes with a button. Use them only where the game ignores input, and tag those bars `input: locked`:

- **The minor-third "villain" slide**, `Dm ↔ B♭m`: Lehman's sinister minor-mediant motion (the Imperial-March family). Use it for Sbuffo's entrance.
- **The true dominant**, `A7 → Dm`: a hard "here it comes!" right before the count-in.
- **The Picardy hit**, `D major`, as the very last chord of a victory fanfare.
- **The real Mario cadence in F**, `D♭–E♭–F`, as the alternative last hit.
- **Whole-tone or chromatic glides, slide whistle**: Sbuffo's gusts.
- **Lament bass**, `D C♯ C B B♭ A`: one dramatic moment per chapter at most.

## Borduns and ostinati (Orff patterns)

A bordun is the open 5th on degrees 1 and 5 with no third. On this set both natural borduns, **D–A** and **F–C**, are button pitch classes, so every press is consonant. Types differ only in *how* the two notes are struck. Teach-order and definitions: [melody_motif_craft.md §8](../../research_notes/Music%20composition%20guidelines/melody_motif_craft.md). Grids are 8 steps per bar, an octave or more below the buttons:

| Type | D centre | F centre | Use |
|---|---|---|---|
| Chord bordun | `D2+A2 - D2+A2 - D2+A2 - D2+A2 -` | `F2+C3 - …` | stage 0–1; most stable |
| Broken bordun | `D2 - A2 - D2 - A2 -` | `F2 - C3 - F2 - C3 -` | default walking pulse |
| Level bordun | `D2+A2 - - - D3+A3 - - -` | `F1+C2 - - - F2+C3 - - -` (F3+C4 would enter the call register) | lift without new pitches |
| Arpeggiated / crossover | `D2 A2 D3 A2 D2 A2 D3 A2` | `F2 C3 F3 C3 …` | energetic battles: eighth motion under quarter-note calls |
| Moving bordun | `D2+A2 - - - D2+C3 - D2+A2 -` | `F2+C3 - - - F2+D3 - F2+C3 -` | colour inside the set; keep the moving voice slow so it doesn't compete with the call |

**Switching the bordun switches the centre.** The same call `D F A - D - - .` sounds at home over D–A, and like it ends on the 6th, unresolved, over F–C. Use the switch *between* exchanges as the "you tamed it" signal, never during a call.

## Bass lines

The bass is L0 and always on. It carries the drive: urgency comes from **subdivision, not tempo** (see [04](04-battle-cues.md#battle-anatomy)).

| Pattern | Grid over Dm (adapt the root per chord) | Feel |
|---|---|---|
| Pedal quarters | `D2 - D2 - D2 - D2 -` | calm, tutorial |
| Pedal eighths | `D2 D2 D2 D2 D2 D2 D2 D2` | driving; the renderer accents beats 1 and 3 |
| Root–fifth | `D2 . D2 A2 D2 . D2 A2` | the classic battle gallop |
| Octave bounce | `D2 . D3 . D2 . D3 .` | 8-bit, playful |
| Tresillo (from stage 3) | `D2 - - D2 - - A2 -` | 3+3+2: teaches the syncopation before calls use it |
| Pickup into bar 1 | last bar: `D2 . A2 . F2 G2 A2 C3` | a walk-up that leads into the loop restart |

Roots for the other safe chords: B♭ → `B♭1/F2`, C5 → `C2/G2`, Gm7 → `G1/D2`, E♭ → `E♭2/B♭2`, F → `F1/C2`. **Double low roots an octave up** (B♭1 is 58 Hz, below what tablet speakers reproduce), or give the bass sound a bright, clicky attack.

## Pads and comping (L1)

Keep voicings **below C4** while input is open (MIDI < 60), or above C6. Close, low voicings that the validator accepts:

| Chord | Voicing | MIDI |
|---|---|---|
| Dm | D3 F3 A3 | 50 53 57 |
| Dm7 | C3 F3 A3 over a D2 bass (D3 F3 A3 C4 would put C4 in the call register) | 48 53 57 |
| B♭ | B♭2 D3 F3 | 46 50 53 |
| C5 / Csus4 | C3 G3 / C3 F3 G3 | 48 55 / 48 53 55 |
| Gm7 | G2 D3 F3 B♭3 | 43 50 53 58 |
| E♭maj7♯11 | E♭2 B♭2 G3 | 39 46 55 |
| F / F6 | F2 C3 A3 / F2 D3 A3 | 41 48 57 / 41 50 57 |

Pads should be soft-attack sounds (strings, choir, organ), so they read as "world", not "voice".

## Drums and percussion

Unpitched percussion is free in every register, and it is the best way to add excitement without masking the call. General MIDI keys are in [05](05-format-and-pipeline.md#general-midi-reference). Grids here are **16 steps per bar** (sixteenths), split into beats:

```
                   1 . . . 2 . . . 3 . . . 4 . . .
Marcia (stage 0-1)
  kick             x . . . . . . . x . . . . . . .
  woodblock        . . . . x . . . . . . . x . . .
Battaglia (default)
  kick             x . . . . . . . x . x . . . . .
  snare            . . . . x . . . . . . . x . . .
  shaker           x x x x x x x x x x x x x x x x
Galoppo (driving, stage 2+)
  kick             x . . x x . . . x . . x x . . .
  snare            . . . . x . . . . . . . x . . x
  hat              x . x . x . x . x . x . x . x .
Boss
  taiko            x . . . . . x . x . . . . . . .
  snare            . . . . x . . . . . . . x . x x
  timpani(D2/A2)   bar-level hits on beat 1 of each exchange
```

Rules:

- **The kick on beat 1 is sacred.** It is the beat anchor for the child and the judge. Never humanise it, never drop it in a battle.
- **Shaker and hat sixteenths make 100 BPM feel like 170.** Use them instead of raising the tempo.
- **Child-friendly timbres:** woodblock, claves, triangle, shaker, tambourine, hand drum, timpani. Avoid harsh crash cymbals except on section starts.
- **Fills** only in the last 2 beats of bar 8 (the loop turnaround), and only in locked or echo-free bars. A fill in the middle of a call bar destroys the count.
- The **"your turn" cue** sits on beat 4 of the call bar (the rest that calls leave free). See [04](04-battle-cues.md#cue-sheet).

## Texture during the listening turn

Children separate streams worse than adults until about 10–11, gain less from stereo at 5, and track the **highest** voice ([§6 of the battle notes](../../research_notes/Music%20composition%20guidelines/battle_and_adaptive_music.md)). So:

- **Call bar:** L0 only (drums + bass). Pads, hits and leads drop out or fall about 9 dB. **Nothing pitched sits above the call.**
- **Echo bar:** layers return. The companion (L2) may double the *previous* successful echo, never the current one.
- Keep the call, the echo and the voice **centred**. Stereo width belongs to pads and percussion only. Check every mix summed to mono on one small speaker.
- Reverb on the call is short and dry. Long tails smear the next onset.

## Variation without new learning

Repetition is good. Monotony over a 1–2 minute battle is not. Ways to vary that never change what the child must remember:

- **Pools of 2-bar variants** for L1 and L3 per chord pair, stitched at random at exchange boundaries (Kondo's Hyrule Field method). They all fit, because the chord grid and the button set are fixed.
- **Re-voice instead of re-harmonise:** same chord, new register or instrument.
- **Swap the bass pattern** at the loop restart (pedal → root–fifth → octave bounce).
- **Add or remove percussion lanes** as intensity, the cheapest layer.

## A minimal valid loop (tutorial)

```yaml
id: loop_tutorial_drone
role: loop
centre: D
tempo: 100
meter: 4/4
steps_per_bar: 8
chords: [D5, D5, D5, D5]
intent: "stage 0-1 backing: chord bordun plus marcia percussion; any press is consonant"
tracks:
  bordun:
    instrument: pizzicato_strings
    layer: L0
    bars: ["D2+A2 - D2+A2 - D2+A2 - D2+A2 -", "=1", "=1", "D2+A2 - D2+A2 - D2 - A2 -"]
  drums:
    layer: L0
    steps_per_bar: 16
    lanes:
      kick:      ["x . . . . . . . x . . . . . . .", "=1", "=1", "=1"]
      woodblock: [". . . . x . . . . . . . x . . .", "=1", "=1", ". . . . x . . . . . . . x . x x"]
```
