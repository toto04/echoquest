# 04. Battle music: anatomy, cue sheet, adaptive layers and characters

This guide turns the battle loop from the [prior report](../../reports/Audio%20rhythm%20game%20music%20design.md#the-battle-loop-alternates-listen-and-echo-over-a-beat-that-never-stops) into the list of musical assets to compose, and the rules for how they fit together. Evidence: [battle_and_adaptive_music.md](../../research_notes/Music%20composition%20guidelines/battle_and_adaptive_music.md). Where a rule is a design opinion, it says so.

## Battle anatomy

What to copy from video-game battle themes is **drive and immediacy**, not speed. Normal battle themes state their identity at once, are "peppy, driving, sometimes even cheerful", and loop quickly. Boss themes keep the drive and darken the colour. Final-boss music is built from fragments of earlier themes. Published JRPG battle themes run about 150–190 BPM, which is far faster than a young child can echo. Keep the quarter-note pulse at **100–130 BPM**, and get the felt speed from **eighth-note bass, sixteenth-note shaker, accents and a full kit**.

**Keep it friendly by design.** Masuda keeps Pokémon battle music from sounding "too scary or dangerous" because the opponent may become a friend. EchoQuest's Sbuffo is likewise meant to be understood and included, not destroyed (see the script's development notes). Menace is **comic**.

### Exchange formats

An **exchange** is the unit of play. Its length decides the loop math:

| Format | Bars | Content | Stage | Loop |
|---|---|---|---|---|
| **Long exchange** | 4 | call · call again · child's echo · reply (on success the companion plays it back; on failure the call returns) | 1–2 | 8-bar loop = 2 exchanges, 2 chords |
| **Short exchange** | 2 | call · echo | 3+ | 8-bar loop = 4 exchanges, 4 chords |

Playing a new call twice for ages 5–7 follows the finding that repetition helps children detect a target in a mix far more than it helps adults: 15.6 dB of improvement at ages 5–7 versus 9.5 dB for adults, going from 2 to 4 repetitions ([Leibold & Bonino 2009](https://pmc.ncbi.nlm.nih.gov/articles/PMC2736737)).

**Beat 4 is a signal slot.** Calls leave beat 4 empty (the "3 beats + rest" rule in [02](02-calls.md)). This gives two fixed slots:

- **beat 4 of the last call bar:** the **"your turn" cue**. It is identical everywhere in the game. Opinion: use Maestro Battuta's baton *TOC*, a low woodblock, already his signature in the script.
- **beat 4 of the echo bar:** the **verdict sting**. It is short and consonant. Success = a rising button-pitch sparkle; "not yet" = one soft low note.

### Phases of one battle

| Phase | Music | Input |
|---|---|---|
| **Introduction** | Maestro presents the opponent's instrument and motif *in isolation* before the fight (Prokofiev's own instruction for Peter and the Wolf: show the instruments and themes first) | locked |
| **Intro sting** | 1–2 bars: the opponent's signature with a low hit. Locked colour allowed here | locked |
| **Count-in** | 1 bar: the same for the whole game, e.g. percussion plus "uno, due, tre, quattro" or the baton *TAC TAC TAC TOC* | open (wrong presses are harmless) |
| **Defence rounds** ×3–4 | loop A + calls. Each success adds a layer; the song grows cumulatively (cell → cell twice → cell, cell, answer) | open |
| **Attack round** | the child plays an owned song or answers a question (period form); fewer memory demands, more agency | open |
| **Finisher** | one big gesture on the beat (a bell flick, or one held note) | open |
| **Victory** | fanfare (2 bars + a hit), then the reprise (8 bars, F centre) | fanfare locked, reprise open |

Timing at 100 BPM: one bar = 2.4 s, a short exchange = 4.8 s, a long one = 9.6 s. A 4-round battle plus intro and victory lasts about **45–90 s**. Opinion: keep battles under 2 minutes at stages 1–2.

## Cue sheet

Every battle needs these assets. Each one is a separate spec ([05](05-format-and-pipeline.md)).

| Cue | Length | Input | Loops? | Content and rules |
|---|---|---|---|---|
| `intro_<opp>` | 1–2 bars | locked | no | signature motif in its own instrument; a villain chord or glide allowed; ends with the downbeat into the count-in |
| `count_in` | 1 bar | open | no | the game-wide constant. Never vary it |
| `loop_<opp>_A` | 8 bars | open | yes | stems L0–L3 (below); one chord per exchange |
| `loop_<opp>_B` "help" | 8 bars | open | yes | after 2 failures: same chords, sparser, louder beat anchor, sung note names allowed (support, not punishment) |
| `loop_<opp>_C` "almost" | 8 bars | open | yes | near victory: same chords, fuller drums, the orchestra theme fragment in echo-free bars |
| `call_<opp>_<n>` | 1–2 bars | open | no | playable layer only; from the opponent's pool (shared head, different tails) |
| `turn_cue` | 1 beat | — | no | game-wide constant, on beat 4 |
| `sting_ok` / `sting_notyet` | ≤ 1 beat | — | no | button pitches only, so it fits any chord: `D F A C` arpeggio up for success; one soft `D` or `F` for "not yet" |
| `taunt_<opp>` | 1–2 bars | locked | no | after a failed phrase: comic laugh on bassoon or trombone with a fast falling wobble, over `Gm7 → B♭maj7`, then a snare pickup back to bar 1. Under 4 s |
| `transition_<x>_<y>` | 1 bar | open | no | between loops A, B and C, on an exchange boundary |
| `fanfare_win` | 2 bars + hit | locked | no | ≤ 4 s; triplet brass. Options: `B♭ C5 → D5` (Mario-like whole-step lift into an open fifth), `B♭ Csus4 → F`, or `E♭maj7♯11 → F` (the Pokémon-like Mixolydian ♭VII–I). Only the final hit may be off-list (D major, or D♭–E♭–F) |
| `reprise_<piece>` | 8 bars | open | no | the recovered piece of the orchestra's song, fully harmonised in F; the opponent's motif returns as a countermelody in bars 5–8 ("redemption") |

## Adaptive layers (vertical layering)

Success **adds** a layer at the next bar line. Failure removes **only the top** layer. Every layer must sound complete over L0 alone, so every subset is a valid mix (the LittleBigPlanet rule from Winifred Phillips's [GDC write-ups](https://www.gamedeveloper.com/blogs/pure-vertical-layering-for-game-music-composers-from-spyder-to-sackboy-gdc-2021-)).

| Layer | Content | Register | During the call bar |
|---|---|---|---|
| **L0 groove** | kick, snare/woodblock, shaker, bass ostinato | bass + unpitched | **always on**: the only layer under a call |
| **L1 harmony** | pad or comping, the chord of the exchange | below C4 | ducked ~9 dB or muted |
| **L2 companion** | an instrument that plays the child's phrase back (the "reply" bar of the long exchange) | the call register, *only in reply bars* | silent |
| **L3 hits** | brass stabs and timpani on exchange downbeats | low | silent |
| **L4 fever** | the orchestra theme fragment, a countermelody | above C6 or low tenor | silent; plays only in echo-free bars (attack round, loop C) |
| **Recovered instruments** | each instrument the child has won back becomes a permanent extra stem in later battles | per instrument | silent |

The layer count *is information*: fuller means winning. Name that once, in story form ("listen: the orchestra is coming back!"). Blind players in one study did not use game music as information *unless it was composed to carry it* ([Westerholm et al. 2024](https://jyx.jyu.fi/jyx/Record/jyx_123456789_98477)).

Transition rules:

- **Layer changes** fade in over one beat, starting at the next bar line after the verdict. Never in the middle of a call or echo bar.
- **Loop changes** (A → B → C → victory) happen only at an **exchange boundary**, through a 1-bar transition or a sting, scheduled on the `dspTime` grid (Unity `PlayScheduled`, 100–200 ms ahead).
- **Stingers** built only from button pitches and the current chord's root and 5th can fire on any beat without harmonic logic. That's the payoff of the fixed-pitch design.
- **Victory** waits for the end of the echo bar, then fires the fanfare on the next downbeat.

## Leitmotifs

Peter and the Wolf works for children because **each character is one instrument and one tune with its own tempo and character marking**, and because the children hear the instruments and tunes *before* the story ([Boosey & Hawkes introduction](https://www.boosey.com/downloads/peterwolf/introduction.pdf)). Game leitmotifs work by **repetition plus transformation**: a short core stays fixed while tempo, metre, orchestration and harmonisation change ([Murphy, DiGRA 2019](https://dl.digra.org/index.php/dl/article/download/1820/1820)).

**Recipe (opinion):** character = **instrument + 3-note cell + rhythm + register + tempo word**. Any two characters must differ in at least two of these five.

### Proposed cast motifs (drafts; story names are provisional in the script)

| Character | Instrument | Cell (buttons) | Rhythm | Notes |
|---|---|---|---|---|
| **Sbuffo** (the wind) | pan flute or ocarina with breathy flutter + a short wooden transient; slide-whistle "whoosh" in locked bars only | `C A D` (4 3 1): a falling sweep that skips | short-short-long `X X X -` | his taunt is `s3_marameo` (so–mi–so-so–mi). His locked-bar colour: glides and the `Dm ↔ B♭m` slide |
| **Maestro Battuta** | claves + low woodblock (baton), brass for pitched moments | baton rhythm *TAC TAC… TOC* (`x . x . . . x -`); pitched form `D - A -` (open 5th, like a raised baton) | as in the script | the *TOC* is the game-wide "your turn" cue |
| **Pianoforte** | piano (fast attack, ideal) | the head of the orchestra's song: `C A F` | ta ta ta-a | the first instrument recovered; the child's first button timbre |
| **Tamburo** | timpani / tuned toms on the button pitches (an octave down) | rhythm-led: 1–2 pitches | tresillo or galoppo | its chapter is the *rhythm* chapter: calls on one pitch first |
| **Campanella** | glockenspiel, celesta or music box (clear pitch; avoid tubular bells, whose pitch is ambiguous) | `A C A` | ringing quarters | an octave-up register shift (D5–C6); flip the register map |

### Transforming one motif across the story (same pitches, different setting)

| State | Harmony under the cell | Register / articulation | Tempo / metre | Instrument |
|---|---|---|---|---|
| **Menace** (battle) | Dm, or E♭maj7♯11 for bosses | low octave, staccato | loop tempo, 4/4 | own instrument + low-brass doubling |
| **Taunt** (child failed) | Gm7 | fast wobble ending | same | bassoon / trombone |
| **Defeated** | B♭maj7 | high octave, legato | half tempo | own instrument, alone, soft |
| **Redeemed** (victory) | F, B♭ | middle, as a countermelody | 6/8 or swung | joins the orchestra |
| **Hint** (overworld) | F6 or Dm7 | distant: reverb, low level | slow | music box or celesta |

Changing the chord under fixed notes is the cheapest emotional flip. `D F A` over Dm is the enemy; over B♭maj7 (as 3rd, 5th, maj7) it turns tender. In the finale, layer every redeemed motif as a countermelody, one per 2 bars, so the child hears the whole cast return (Undertale's motif-blending function).

## The orchestra's song as the spine (strong recommendation)

The script's premise is that Sbuffo stole *pieces of one song*, and every recovered instrument brings back its piece. **Compose that song first and derive the battle calls from it.** Each battle's calls are cells, variations and Sbuffo-scrambled versions (`retrograde`, `invert`, `rerhythm`) of that chapter's piece. Each victory reprise plays the song *up to the pieces recovered so far*. At the end of the game the child can play the whole song on the four buttons.

This is the Ocarina of Time and Patapon idea (songs as owned tools, reprised as area music) made literal. Repeated material is also what memory and recognition research rewards. A draft song and a full worked battle are in [06-worked-example.md](06-worked-example.md).

## Emotional palette: playful-menacing, never scary

Children read musical emotion first through **tempo**, and only later through **mode**. Five-year-olds use tempo only; 6- to 8-year-olds use tempo and mode; recognition is adult-like by about 11. Children of every age *prefer high-arousal music* ([Dalla Bella et al. 2001](https://www.ebi.ac.uk/europepmc/webservices/rest/search?query=EXT_ID:11274986%20AND%20SRC:MED&resultType=core&format=json); [Hunter et al. 2011](https://sites.utm.utoronto.ca/sites/sites.utm.utoronto.ca.glenn_website/files/download/HunterEtAl2011JECP.pdf)). The target is **high arousal, positive valence**.

| Use (playful-menacing) | Avoid (horror) |
|---|---|
| low staccato woodwinds and brass (bassoon, tuba), pizzicato | slowly rising high-string glissandi |
| fast low-brass and trombone wobbles, slide whistle (the cartoon-humour glissando) | clusters, drones on a semitone |
| timpani, woodblock, hand drums | sudden loud hits, jump-scare stingers |
| a comic one-beat stop before the opponent's call (the tiptoe) | breathy whispers, screams |
| the ♭II chord, rationed to bosses | sub-bass rumble with no beat |
| minor mode at ≥ 100 BPM | slow + minor + low + dissonant, all at once |

The glissando categories come from [Smith, MTO 2026](https://www.mtosmt.org/issues/mto.26.32.1/mto.26.32.1.smith.pdf).

- **Direction means status, everywhere:** rising glides and arpeggios mean success, a layer gained, an instrument recovered. A falling wobble means "not yet". Never use a slow descending minor line for a failure: that reads as sad, not funny.
- **Any one or two "dark" ingredients are fine; all of them together is horror.**
- **Scale the menace by stage.** Stages 1–2 get mischief progressions and comic opponents. The ♭II boss colour and dense percussion come from stage 2 or 3, when mode starts to matter. Every encounter resolves into the F-centred major reprise within the same session.

## Failure, musically

Failure is staged and never silent (the game rules are in the prior report). Musically:

1. **First failed phrase:** `taunt_<opp>` (a funny laugh), then the same call again. The groove never stops.
2. **Second:** switch to `loop_B` at the next exchange. It is sparser, with a louder beat anchor; the sung note names return; the motors pulse the beat.
3. **Third consecutive:** the call is reduced to its first cell (`fragment`). The tempo is **not** slowed.
4. **Only the top music layer is lost on a failed phrase.** The child never hears the music collapse.
