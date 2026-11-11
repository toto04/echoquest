# 02. Writing calls: the phrases the child echoes

A **call** is the 1–2 bar phrase an opponent (or Maestro Battuta, or an instrument) plays using only the four button pitches, which the child then reproduces. It is the most important musical object in the game. It is the notation, the challenge and the hook at once.

Notation used here: one token per eighth note, 8 per 4/4 bar. `D F A C` = a button onset, `-` = hold, `.` = silence. So `C - A - F - . .` is so–mi–do in quarters with a rest on beat 4. The full format is in [05-format-and-pipeline.md](05-format-and-pipeline.md).

## The ten rules (use as a checklist)

These are condensed from [melody_motif_craft.md](../../research_notes/Music%20composition%20guidelines/melody_motif_craft.md) and the [prior report](../../reports/Audio%20rhythm%20game%20music%20design.md#proposal-composition-rules-for-a-four-note-songbook). The evidence behind each is noted. Where a rule is a design opinion, it says so.

1. **One call = one four-beat line, usually 3 beats of notes plus a rest.** Children's verse is built from four-beat lines, and short lines end on a silent beat (Burling 1966). The rest on beat 4 is also where the "your turn" cue goes. Two-bar calls use a 4 + (3 + rest) shape.
2. **Rhythm first, pitches second.** Write the rhythm cell, then try it on 2, then 3, then 4 pitches. That gives a difficulty ladder for free (Berklee pentatonic method). Rhythm is also the skill being trained.
3. **Hook in the first 2–4 notes.** Early and short phrases are the ones that survive memory best (Janssen et al. 2017). Recognition can start after about 4 notes, and Kondo aims for a 2–3 s identity window. At 100–120 BPM a bar is 2–2.4 s.
4. **Repeat something inside the call:** a repeated note, a repeated cell (X X), or a repeated rhythm under new pitches. Melodic repetitiveness was the strongest symbolic predictor of recognisability (Van Balen et al. 2015).
5. **Mostly predictable, exactly one surprise.** Correct predictions are rewarding, and listeners credit that reward to the music (Huron's ITPRA). The surprise is what makes the call distinctive. Pick one: a leap that keeps going, a delayed tonic (a rest before the last note), a repeated note where a step was expected, one off-beat. *Never two.*
6. **Step, then leap later.** From age 5 children expect the next note to be *close* (neighbouring button, or the same note). Expecting a leap to reverse, or an X-Y-X return, appears only in older listeners (Schellenberg et al. 2002). For ages 5–7, use neighbour moves and repeats with at most one skip. Add leap-then-reverse and 1-4-1 figures from about age 8.
7. **Reverse toward the middle.** The middle of the set is F4/A4. After reaching C5, come down; after reaching D4, go up or stop. Descending runs feel natural, like a close. Rising runs feel like questions (step declination, regression to the mean).
8. **End on the tonic: the longest note, on a strong beat.** Pentatonic has no leading tone, so the centre comes from **duration, metric position and the bass**. Duration differences are what make a key perceptible (Smith & Schmuckler 2004). End echo calls on **D4** (D centre) or **F4** (F centre), on beat 1 or 3, as the longest note. Start on the tonic or its 5th.
9. **Keep the bass under the call constant.** The bordun (D–A or F–C) decides what "home" is. Changing the chord *during* a call changes how its ending sounds, so call and echo share one chord ([03-backing.md](03-backing.md)).
10. **Fewer notes beats more notes.** When in doubt, cut a note. Two or three distinct pitches per call are usually enough.

## The aimless call, and how to fix it

The archetype to avoid is **four different buttons in equal quarters with no repetition**, for example `D - A - F - C -`. It has no repeat, no long note, no clear ending and no hook. Fixes, in order of preference:

| Problem | Fix | Before → after |
|---|---|---|
| No repetition | repeat a note or the head | `D - A - F - C -` → `D - D - A - - .` |
| No home | end on the tonic, longest | `F A C - A - C -` → `F A C - A - F -` |
| All equal lengths | make the last note twice as long | `C - A - F - D -` → `C A F - D - - -` |
| Too many notes for the stage | delete the passing note | `D F A F D F A -` → `D F A - D - - .` |
| Long run in one direction | turn back at the edge | `D F A C C - - -` → `D F A C A - - -` |

## Stage ladder

These are the difficulty stages from the prior report, translated into composition limits. The validator in [05](05-format-and-pipeline.md) enforces the note count and the minimum gap between onsets. The ages are entry points, not rules.

| Stage (age) | Buttons | Max notes | Chunks | Rhythm cells allowed | Pitch moves | Typical judging |
|---|---|---|---|---|---|---|
| **0** (<5) | any | — | — | free play, no calls | — | none |
| **1** (5–6) | C, A, then F (so, mi, do) | 3 | 1 | quarters only, rest on beat 4 | neighbours, repeats | order only |
| **2** (6–8) | add D | 4 | 1 | + ti-ti, ta-a, tam-ti (q. e) | one skip allowed | relative rhythm |
| **3** (8–10) | all 4 | 6 | 2 (cell twice) | + one rest inside, then one off-beat | leap-then-reverse, X-Y-X | relative, then ±100 ms |
| **4** (10–12) | all 4 (+ octave tilt later) | 11 over 2 bars | 3 (cell, cell, answer) | + syncopa, tresillo, pickup, 3/4 | unreversed leaps, the 1–4 seventh | absolute |

Keep the minimum gap between onsets at or above **250 ms** through stage 3. That is an eighth note at 120 BPM.

## Rhythm vocabulary

Kodály syllables (ta, ti-ti, …) are the shared language. Say them aloud while writing: if a call can't be *spoken* as a rhythm, it is too complex.

| Stage | Cell | Syllables | Grid (one beat = 2 steps) | Example call |
|---|---|---|---|---|
| 1 | quarter | ta | `X -` | `C - A - C - . .` |
| 1 | quarter rest | (silent) | `. .` | ends most stage-1 calls |
| 2 | two eighths | ti-ti | `X X` | `A A F - . . . .` |
| 2 | half | ta-a | `X - - -` | `D - A - F - - -` |
| 2 | dotted quarter + eighth | tam-ti | `X - - X` | `A - - F D - - -` |
| 3 | inner rest / off-beat | — | `X - . X` | `D - . F D - - .` |
| 4 | syncopa | syn-co-pa | `X X - X` | `F A - F D - - -` |
| 4 | tresillo 3+3+2 | — | `X - - X - - X -` | `F - - F - - D -` |
| 4 | pickup (anacrusis) | — | last step of previous bar | `. . . . . . . C` → `F - A - F - - .` |
| 4 | triple metre | — | 6 steps per 3/4 bar | `A - - - F -` |

Rules:

- **Introduce syncopation in the backing before it appears in a call.** People recall syncopated rhythms as if they were on the beat, and this recoding is automatic (Hopyan et al. 2009 review). A child who straightens a syncopa is showing normal memory, not careless listening. Coach the child ("make the middle note longer"); don't count it as a miss.
- The **tresillo** is the friendliest first syncopation for a call: it is maximally even (Toussaint's Euclidean E(3,8)). Put it in percussion first, then a call on one repeated pitch, then with pitches. Whether children echo it more easily than the syncopa is untested; that's my conjecture.
- **Pickups** add a second hard problem, because the child has to start *before* the bar. Avoid them until stage 4. When they come, use the 5th falling or rising to the tonic (`C` on the last step of the previous bar, `F` on the downbeat).

### Speech-rhythm mnemonics

Saying a word in the call's rhythm before playing is the "say, then play" step. Stress must fall on the **long** note. These are untested; check them aloud with a native speaker of each language.

| Rhythm | Grid | Italian | English |
|---|---|---|---|
| ta ta | `X - X -` | *me-la* | *ap-ple* |
| ti-ti ta | `X X X -` | *co-li-brì* | *kan-ga-roo* |
| ti-ti ti-ti | `X X X X` | *co-co-me-ro* | *wa-ter-mel-on* |
| syn-co-pa | `X X - X` | *ba-na-na* | *ba-na-na* |
| ti-ti ta ta | `X X X - X -` | *tar-ta-ru-ga* | *ca-ter-pil-lar* |

Avoid words whose stress lands on a short note (*pa-TA-ta* over ti-ti ta).

## Cadence menu (no leading tone needed)

| Close | Grid | Centre | Character |
|---|---|---|---|
| mi → do (falling major 3rd) | `A - F - - -` | F | softest close |
| so → do (falling 5th) | `C - F - - -` | F | strongest, bugle-like |
| do → la (falling minor 3rd) | `F - D - - -` | D | the minor close |
| mi → la (falling 5th) | `A - D - - -` | D | strongest minor close, the "dominant to tonic" of the set |
| descending run | `C A F D - - - -` | D | step declination plus closure |
| open (question) | ends on C5 or A4 | either | for calls that will be *answered*, not echoed |

**Echoes end closed, questions end open.** A call the child copies is a complete thought. A call the child must *answer* ends on so or mi.

## Three call forms (and the response modes they create)

| Form | Shape | Response mode | Use |
|---|---|---|---|
| **Sentence** | idea, idea again, continuation to the tonic, in 1 : 1 : 2 proportion | **echo**: the child copies it | the default battle call |
| **Refrain** | the child always plays the same short motto, whatever the opponent plays | **fixed response** (leader/chorus) | youngest players: their part never changes |
| **Period** | antecedent ends open; consequent restates it and ends closed. Often only the last note differs | **answer**: the child completes the phrase | later stages; feels creative, costs almost no memory |

Sentence in one bar (stage 3): `F A F A C - F -` (idea `F A`, again `F A`, continuation `C - F -`). In two bars (stage 4): `D F A - D F A -` | `A F A F D - - -`.

Refrain (the "hero's motto", opinion): `F - A - F - - .` (do–mi–do). It anchors home and gives a guaranteed success between harder echoes.

Period (stage 3): antecedent `F A C - A - C -` (ends on so, open); consequent `F A C - A - F -` (ends on do, closed).

## Development: vary *between* rounds, not inside a call

A call is too short to develop internally. Development happens **across the rounds of a battle**, so the child keeps hearing the same identity while the task grows: **X X → X X′ → sequence → fragmentation/liquidation**. This also follows the finding that regular, chunkable sequences are retained better at every age (Mathy et al. 2016).

With four pitches, "transpose" means **move every note one button** (a sequence along the set). Base cell X = `F A C -` (2 3 4, ti-ti ta):

| Operator | Result | Buttons | Comment |
|---|---|---|---|
| `repeat` | `F A C - F A C -` | 234 234 | memory cost: one cell |
| `vary-end` (a a′) | `F A C - F A F -` | 234 232 | same head, new ending. The cheapest "rhyme" |
| `sequence-down` | `F A C -` → `D F A -` | 234 → 123 | every note one button down. Sequence down from high cells, up from low ones |
| `invert` (mirror 1↔4, 2↔3) | `A F D -` | 321 | up-up becomes down-down |
| `retrograde` | `C - A F` | 432 | reverses pitch *and* rhythm. A natural "answer" to X; "Sbuffo plays it backwards" |
| `augment` | `F - A - C - - -` | 234 | half speed. The teaching pass for the youngest |
| `diminish` | sixteenths | 234 | too fast for buttons before stage 4; avoid |
| `fragment` | `F A F A F A C -` | 23 23 23 4 | repeats the head, then releases. Builds excitement before an attack |
| `liquidate` | `F A C - A F F -` | …, 3 2 2 | the distinctive top note disappears and the line settles on the tonic |
| `displace` | `. . F A C - . .` | 234 | same cell one beat late. The accent moves, and so does the feel |
| `rerhythm` | `F - A C - - . .` | 234 | ta ti-ti instead of ti-ti ta: same pitches, a different song to a child |
| `repitch` | `F A D -` | 231 | same rhythm, new pitches. The "rhythm first" ladder: teach the rhythm on 1–2 buttons, then repitch it |

**Important refinement of the prior report's library rule.** A *transposed copy*, with the same contour and the same rhythm on different buttons, **is the same song to a young child**, because children encode contour and rhythm before exact pitch. Use sequences only as a deliberate development step *inside one battle*. Never use them to make two different opponents' calls. Two calls in the library must differ in **contour or rhythm**. Sharing pitches alone doesn't make them distinct. The validator fails pairs whose contour and rhythm are both identical, and warns when they differ by a single symbol. In stage 1, where every call is in quarters, that warning is expected: there, the first note and the contour carry the identity.

**Story verbs map onto operators** (opinion): Sbuffo *scrambles* the song, so his versions are `retrograde`, `invert` or `rerhythm` of the true piece. The child *restores* the original. That turns music-theory operators into the plot.

## Identity: how to make every opponent's calls recognisable

A character is **one instrument + one 3-note cell + one rhythm + one register + one tempo word** (the Peter and the Wolf recipe; see [04-battle-cues.md](04-battle-cues.md#leitmotifs)). Any two opponents must differ in at least two of those five.

- **Keep the head fixed and vary the tail.** An opponent can have a pool of 3–4 calls that share its first 2–3 notes and differ in the ending, chosen per round (Kondo's phrase pool, used inside one character). The head is the opponent's "face"; the tail is the challenge.
- **Musical rhyme:** two calls in one battle can share their ending and differ in their beginning, so the child learns one cadence and two faces. Example: `F A C - A - F -` and `D D C - A - F -` share `C - A - F -`. No study tests rhyme directly; it's an extrapolation from phrase-repetition effects.
- **Keep a registry.** Every approved call goes into the library (see [05](05-format-and-pipeline.md#library-registry)). New calls are checked against it before review.

## Starter library (validated)

These calls pass the format checks and the pairwise distinctness check. None has been played to a child yet, so treat them as drafts to audition, not finished material. Grids are 8 steps per 4/4 bar; tempo 100 unless noted.

```yaml
- id: s1_ciao
  role: call
  stage: 1
  centre: F
  open_ending: true
  intent: "so–mi, the playground falling third; first two-button call"
  bars: ["C - A - . . . ."]
- id: s1_casa
  role: call
  stage: 1
  centre: F
  intent: "so–mi–do, walking home; also the head of the orchestra's song (see 06)"
  bars: ["C - A - F - . ."]
- id: s1_sveglia
  role: call
  stage: 1
  centre: F
  open_ending: true
  intent: "do–mi–so rising, 'wake up!'; a question"
  bars: ["F - A - C - . ."]
- id: s1_altalena
  role: call
  stage: 1
  centre: F
  intent: "do–mi–do, the swing; doubles as the hero's refrain"
  bars: ["F - A - F - . ."]
- id: s2_scaletta
  role: call
  stage: 2
  centre: D
  intent: "all the way down, short-short-long-longer; the closing run"
  bars: ["C A F - D - - -"]
- id: s2_bussa
  role: call
  stage: 2
  centre: F
  intent: "knock-knock: repeated note then home; ti-ti ta"
  bars: ["A A F - . . . ."]
- id: s2_salto
  role: call
  stage: 2
  centre: F
  intent: "one leap up, step back down (la–mi–do); the only skip"
  bars: ["D - A - F - - -"]
- id: s2_dondolo
  role: call
  stage: 2
  centre: D
  intent: "tam-ti ta-a: long-short-long, rocking down to la"
  bars: ["A - - F D - - -"]
- id: s3_galoppo
  role: call
  stage: 3
  centre: F
  intent: "a a′: same rhythm cell twice, the ending changes; trains repeated presses"
  bars: ["A A C - A A F -"]
- id: s3_inciampo
  role: call
  stage: 3
  centre: D
  intent: "the stumble: one off-beat note after an inner rest"
  bars: ["D - . F D - - ."]
- id: s3_domanda
  role: call
  stage: 3
  centre: F
  open_ending: true
  intent: "period antecedent: climbs and stops on so; the child answers with s3_risposta"
  bars: ["F A C - A - C -"]
- id: s3_risposta
  role: answer
  stage: 3
  centre: F
  intent: "period consequent: identical except the last note comes home"
  bars: ["F A C - A - F -"]
- id: s3_marameo
  role: call
  stage: 3
  centre: F
  open_ending: true
  intent: "Sbuffo's taunt, so–mi–so-so–mi (the prior report's 'Hello' shape)"
  bars: ["C - A - C C A -"]
- id: s4_habanera
  role: call
  stage: 4
  centre: D
  intent: "tresillo 3+3+2 on one repeated note (a pure rhythm test), then land home on the next downbeat"
  bars: ["F - - F - - F -", "D - - - . . . ."]
- id: s4_banana
  role: call
  stage: 4
  centre: D
  intent: "syn-co-pa then a held la: 'ba-NA-na, la!'"
  bars: ["F A - F D - - -"]
- id: s4_frase
  role: call
  stage: 4
  centre: D
  intent: "two-bar sentence: idea, idea, fragmented continuation, long tonic"
  bars: ["D F A - D F A -", "A F A F D - - -"]
- id: s4_ninna_nanna
  role: call
  stage: 4
  centre: D
  meter: 3/4
  steps_per_bar: 6
  intent: "rocking thirds in 3/4, the prior report's Lullaby; first triple-metre call"
  bars: ["A - - - F -", "A - - - C -", "A - - - F -", "D - - - - -"]
```

## Writing a new call: procedure for an agent

1. **Read the brief:** role (echo, answer or refrain), character, stage, centre, tempo.
2. **Pick the rhythm** from the stage's cells and say it aloud with a mnemonic word.
3. **Pick a contour** (Parsons code such as `*ud`) that no library call with the same rhythm already uses.
4. **Choose the first note** (the tonic or its 5th) and place pitches so the line moves between neighbours, with at most one skip, reversing toward F4/A4.
5. **Choose the one surprise** and write down which it is.
6. **Write the ending:** the tonic, longest, on a strong beat; a rest on beat 4 if the call fits in 3 beats.
7. **Run the checks** ([05](05-format-and-pipeline.md)). Fix hard errors. Read the warnings and either fix them or justify them in `intent`.
8. **Produce 5–10 candidates**, rank them with the rubric in [05](05-format-and-pipeline.md#quality-rubric), and hand the best 2–3 to a human to listen to.
