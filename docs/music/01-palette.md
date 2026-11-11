# 01. The palette: what is fixed, and what the backing may use

Everything in this file is a **hard constraint** unless it says otherwise. The other guides build on it.

## The four buttons

| Button | Finger (story) | Pitch | MIDI | Hz | Solfa (F = do) | Role when centred on D | Role when centred on F |
|---|---|---|---|---|---|---|---|
| 1 | index (*indice*) | **D4** | 62 | 293.7 | la | tonic (1) | 6th |
| 2 | middle (*medio*) | **F4** | 65 | 349.2 | do | minor 3rd | tonic (1) |
| 3 | ring (*anulare*) | **A4** | 69 | 440.0 | mi | 5th | 3rd |
| 4 | little (*mignolo*) | **C5** | 72 | 523.3 | so | minor 7th | 5th |

Facts that shape every composition (interval arithmetic, not opinion):

- Neighbouring buttons are **3, 4 and 3 semitones** apart. In this set, a move to the next button counts as a *step*, and skipping a button (D–A or F–C, a perfect 5th; D–C, a minor 7th) counts as a *leap*. Listeners treat intervals under about 6 semitones as "small" ([Schellenberg et al. 2002](https://sites.utm.utoronto.ca/sites/sites.utm.utoronto.ca.glenn_website/files/download/JEPGeneral.pdf)).
- There is no semitone and no tritone. The set is a **Dm7 chord, which is the same thing as F6**. Any order and any simultaneous combination of buttons sounds consonant.
- The range is 1000 cents across 4 pitch classes. Songs sung by children average 941 cents and 4.5 scale degrees ([Sato et al. 2019](https://0110.be/files/attachments/471/FMA2019_paper_13.pdf)), so the set is child-sized.
- The idiom is **arpeggiated and bugle-like**, closer to calls, chants and fanfares than to stepwise nursery tunes. There is no re, fa or ti, so "Mary had a little lamb"-type lines and the so-la-so-mi playground chant are impossible. Lean into fanfares and calls; don't fight the set.

**Fixed for the whole game:** the same absolute pitches stay on the same buttons for every instrument and every session. Octave shifts of the whole set are the only allowed change. The story's "low or high register" choice for the piano (Scene 2) is such a shift, to D3–C4 or D5–C6. When the call register moves, the register map below moves with it. Why this rule exists, with sources, is in the [prior report](../../reports/Audio%20rhythm%20game%20music%20design.md#choose-la-do-mi-so-on-fast-attack-timbres-and-code-every-button-redundantly).

## Two layers: "playable" and "world"

| Layer | Who plays it | Allowed pitches |
|---|---|---|
| **Playable**: calls, echoes, the companion replaying the child, success stingers | enemy, child, companion | **only D4 F4 A4 C5** (or one octave-shifted copy of the set) |
| **World**: bass, chords, pads, countermelodies, drums, fanfares | the backing | the safe collection below, in the registers below |

## The one harmony rule: avoid five pitch classes while a button can sound

A button note clashes when it sits a **semitone above** a backing note, a minor 9th or minor 2nd. F♯ is added to the list because F against F♯ is the major/minor clash. This leaves exactly five forbidden pitch classes:

| Forbidden in the backing while input is open | Because it clashes with |
|---|---|
| **E** | F4 (button 2) |
| **B** | C5 (button 4) |
| **C♯ / D♭** | D4 (button 1) |
| **G♯ / A♭** | A4 (button 3) |
| **F♯ / G♭** | F4 (button 2) |

What remains is **B♭ C D E♭ F G A**, the B♭-major collection. Every note of it is safe under every button:

- **Free:** D F A C (the buttons themselves) and **G**.
- **Colour, use as chord tones:** **B♭** and **E♭**. Each puts a button a major 7th below it (A under B♭, D under E♭), which is a soft rub, not a clash.

**"Input open"** is the default for every bar of a battle, *including the enemy's call bars*, because wrong presses always sound (a game rule from the prior report). A bar is **input locked** only when the game ignores the buttons: an intro sting, a taunt, a transition, the final fanfare hit, narration. Tag locked bars explicitly in the spec. Locked bars may use any pitch, and that is where chromatic "villain" colour lives.

## The mode wheel: one collection, six moods

The B♭-major collection has a mode on each of its notes, so the *same four buttons* can sit inside six different tonal worlds just by changing the bass. This is the Ocarina of Time recolouring trick ([prior notes §1](../../research_notes/Audio%20rhythm%20game%20music%20design/music_theory_note_sets.md)), made systematic.

| Centre (bass/bordun) | Mode | Tonic chord | What the buttons become | Suggested use (opinion) |
|---|---|---|---|---|
| **D** | Aeolian / Phrygian (no E) | Dm, Dm7, D5 | 1, ♭3, 5, ♭7 | **Battle, mischief, Sbuffo.** The default "enemy" world |
| **F** | Mixolydian (E♭, no E) | F, F6, Fmaj7* | 6, 1, 3, 5 | **Home, friends, victory, the orchestra's song** |
| **G** | Aeolian | Gm7, Gm9 | 5, ♭7, 9, 11 | Tender, sad, "Sbuffo is lonely", the failed-attempt colour |
| **B♭** | Ionian | B♭, B♭maj7 | 3, 5, maj7, 9 | Warm, lullaby, "defeated and calm" |
| **E♭** | Lydian | E♭maj7(♯11) | maj7, 9, ♯11, 13 | Wonder, magic, **the boss signature** (as ♭II under D). Ration it |
| **C** | Dorian-ish (use C5, Csus4) | C5, Csus4 | 9, 11, 13, 1 | Open, floating; transitions and "not yet resolved" |

\*Fmaj7's E is a chord tone that clashes with nothing *in the chord*, but it is on the forbidden list when held under button 2. Voice F as F, F5, F6 or Fadd9 when input is open.

**Default pairing:** D centre for battles and F centre for victory. Moving the bass from D to F under the *same* call is the cheapest, clearest "you tamed it" signal in the game.

## Safe chords (computed)

Every button is a chord tone or a mild extension (9, 11, 13, ♭7, 6), with no semitone clash. This was checked by script over all 12 roots and 14 chord qualities.

| Root | Safe while input is open |
|---|---|
| D | **Dm, Dm7**, D5, Dsus4, D7sus4 |
| F | **F, F6**, F5, Fsus2, Fadd9 (avoid holding Fmaj7's E) |
| G | **Gm7, Gm9** (A is the 9th), G5, Gsus2, Gsus4, G7sus4 |
| B♭ | **B♭, B♭maj7**, B♭6, B♭5, B♭sus2, B♭add9 |
| C | **C5, Csus4, Csus2**, C7sus4. *Never plain C major* |
| E♭ | **E♭maj7(♯11)**, E♭6, E♭. A soft rub; voice E♭ in the bass with the buttons on top |

**Clash with one button. Use only in locked bars, or in bars where that button provably cannot sound:**

| Chord | Clashing button | Fix |
|---|---|---|
| C, Cmaj7, C6 | F (over E) | use Csus4 or C5 |
| G, G7 | C (over B) | use G5, Gsus4 or Gm |
| Am, Am7, A5, A, A7 (the real dominant) | F (over E); A and A7 also clash with D (over C♯) | use Dm/A, or C5 |
| D major (Picardy) | F (against F♯) | use D5 |
| Dm6, Dsus2 | C (over B), F (over E) | Dm7, Dsus4 |
| B♭7, Fm, B♭m | A (over A♭) | locked bars only |

This means the classic minor-key battle moves are available: **i–♭VI–♭VII (Dm–B♭–C5)**, **i–♭VII (Dm–C5)**, **i–iv (Dm7–Gm7)**, the **Phrygian ♭II (Dm–E♭)**, **pedal points**, and **power chords**. Three moves are lost while input is open: the true dominant (A7), the Dorian major IV (G), and the Picardy ending (D major). The guide [03-backing.md](03-backing.md) has the progressions.

## Register map

The call register must stay clear. Children need larger acoustic differences than adults to separate a melody from a mix, and until about age 10 they suffer more masking from competing sounds ([Frontiers 2024 review](https://www.frontiersin.org/journals/human-neuroscience/articles/10.3389/fnhum.2024.1352247/full); details in [battle_and_adaptive_music.md §6](../../research_notes/Music%20composition%20guidelines/battle_and_adaptive_music.md)).

| Zone | MIDI | What goes there |
|---|---|---|
| Sub/bass, D1–B♭2 (37–117 Hz) | 26–46 | bass ostinato, kick, timpani. Give the bass audible upper harmonics so it survives tablet speakers |
| Low-mid, B♭2–B♭3 | 46–58 | pads, chord comping, low brass stabs, bordun upper voice |
| **Reserved, C4–D5 (262–587 Hz)** | **60–74** | **the call, the child's notes, the companion. Nothing else sustains here while input is open** |
| High, above C6 | 84+ | sparkle and glockenspiel, only in bars with no call (children track the *highest* voice) |
| Unpitched | — | shaker, hats, woodblock, claves, triangle: free everywhere |

During the bar in which the enemy plays a call, the backing **thins to drums plus bass** (layer L0, see [04-battle-cues.md](04-battle-cues.md)).

## Tempo and grid

- **Pulse range:** 100–130 BPM for battles (the children's echo range, from the prior report). Never slow a call down to make it easier. Make it *structurally* easier instead (fewer notes, more repetition).
- **Loop-friendly tempos.** At these tempos one eighth note is a whole number of samples at 48 kHz, so bars never drift: **90, 96, 100, 120, 125, 128** (also 80 and 144). Prefer 100 and 120. 110 is fine musically but leaves a fractional sample per bar; `dspTime` scheduling in double precision absorbs that, so treat it as a preference, not a rule.
- **Meter:** 4/4 by default. 3/4 and 6/8 from stage 4 (the "Lullaby"-type songs).
- **Grid:** calls are written on an **eighth-note grid** (8 steps per 4/4 bar). Backing may use 16 steps. Calls never use sixteenths before stage 4: at 100 BPM sixteenths are 150 ms apart, too dense to echo on buttons.

## Story constants worth keeping in mind

The story, from the script [`capitolo_01_la_canzone_rubata_dal_vento.md`](../../unity/EchoQuestTest/Assets/EchoQuest/Story/Script/capitolo_01_la_canzone_rubata_dal_vento.md):

- **Sbuffo**, the mischievous wind, stole and *scrambled* pieces of the orchestra's song.
- **Maestro Battuta** guides the child. His sound signature is the baton: *TAC, TAC… TOC!*
- Instruments are recovered one by one: **pianoforte** first, then **tamburo** and **campanella**. The prologue's orchestra also includes **triangolo** and **flauto**.
- The script's first challenge (Scene 3) is a rhythm echo on the piano button ("TA… TA-TA!"), judged on structure, not precision.

The music should make these characters and that arc audible. [04-battle-cues.md](04-battle-cues.md) proposes how.
