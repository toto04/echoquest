# 07. Open questions, decisions needed, and further research

These guides rest on adult and sighted-child research, music-theory reasoning and game-industry practice. No study of music perception or rhythm synchronisation in *blind children* was found. Everything here is a starting value for playtesting.

## Decisions the team needs to make

| # | Decision | Options | Recommendation (opinion) |
|---|---|---|---|
| D1 | **What a button means.** The script's notes say each button "should correspond to a known instrument"; the music research assumes each button is a fixed **pitch** | (a) pitch per button, with the timbre set by the current instrument; (b) instrument per button, unpitched; (c) both: a fixed pitch *and* a home timbre per button | **(a)**, with the timbre stable for a whole battle. (b) loses melody, contour memory and the orchestra's song. (c) is worth a playtest as a learning aid (redundant coding), fading to (a) |
| D2 | **Battle centre** | D (minor) everywhere; or F for stage 1–2, D from the drum chapter on | D by default. 5-year-olds read tempo, not mode, so D is safe for the youngest; the worked example uses D |
| D3 | **Exchange format by stage** | long (call · call · echo · reply) vs short (call · echo) | long for stages 1–2, short from stage 3. Measure it |
| D4 | **The orchestra's song as the spine** | yes / no | yes: compose it before the battles ([06](06-worked-example.md)) |
| D5 | **Who plays the call** | the stake instrument's timbre, or the challenger's timbre | the stake timbre at stages 1–2 (the child copies the sound they will produce); the challenger's timbre for taunts and later stages |
| D6 | **Piano register after Scene 2's choice** | keep the absolute set, or shift it an octave | an octave shift of the whole set is allowed; flip the register map ([01](01-palette.md#register-map)) |
| D7 | **The "your turn" cue** | Maestro's *TOC*, a chime, or a voice | *TOC*: it is already his signature and is percussive (precise onset) |
| D8 | **House SoundFont** | GeneralUser GS, FluidR3_GM, MuseScore_General | GeneralUser GS; check its licence caveat before release |
| D9 | **Pipeline ownership** | build `check.py` / `render.py` now, or hand-compose in a DAW | build the checker first: it is small and unlocks agent iteration |

## Playtest questions (with children, ideally blind or visually impaired)

Ordered by how much they would change the music.

1. **Is the missing note felt?** Play the song head `C A F`, then `C A …`. If the gap is not obvious, the story's central musical moment fails.
2. **Can children tell the four buttons apart by ear** at ages 5–7 and 8–12, and which pairs get confused (F–A, the major third, is the widest neighbour gap; D–F and A–C are minor thirds)?
3. **Long versus short exchanges:** does hearing the call twice help 5–7-year-olds as much as the auditory-masking data suggest?
4. **Timbre match:** do children echo better when the call uses the same timbre as their button?
5. **Layers as information:** do children notice the music getting fuller as they succeed, and does that motivate them, without telling them?
6. **Playful-menacing calibration:** does Sbuffo's palette (bassoon, wobble, ♭II) read as funny or as scary at each age? Any child who finds it scary is a hard stop for that ingredient.
7. **Tempo:** is 100 BPM right for the youngest, or do they prefer faster (the sources disagree: 115–150 BPM spontaneous tapping)?
8. **Syncopation:** do children straighten syncopa and tresillo calls when they echo, as the adult recall literature predicts? If so, should the judge accept the straightened version early on?
9. **Rhyme:** do calls that share an ending get remembered better than unrelated calls?
10. **Retention:** how many owned songs or pieces does a child keep across days?

Log raw press times in every session: they would be the first rhythm-echo data for blind children anywhere.

## Desk research still worth doing

Sources the researchers could not open, in order of expected value:

| Source | Why |
|---|---|
| Utrecht **BongoBeats** theses ([record](https://studenttheses.uu.nl/handle/20.500.12932/43216)) and the **Feel the Rhythm** thesis ([record](https://repository.lib.umassd.edu/esploro/outputs/graduate/Feel-the-rhythm-a-blind-accessible-rhythm/9914424803801301)) | the only rhythm games built for or tested with this audience |
| Westerholm, Tuuri & Hassan, *Not just Atmosphere* (Mindtrek 2024) ([record](https://jyx.jyu.fi/jyx/Record/jyx_123456789_98477)) | music composed to carry information for blind players |
| **Kodály Center collection**: scrape the songs with tone set *l, d, m, s* ([collection](https://kodalycollection.org/song.cfm?id=481)) | real children's songs on exactly EchoQuest's solfa set: rhythm sets, ranges and forms to count. A small, high-value project |
| Winifred Phillips, *A Composer's Guide to Game Music*; Michael Sweet, *Writing Interactive Music for Video Games*; Karen Collins, *Game Sound* | the standard texts on adaptive music; only summaries were read |
| David Huron, *Sweet Anticipation*, ch. 5–7 | primary source for the expectation rules (only a review was read) |
| Tim Summers, *The Legend of Zelda: Ocarina of Time: A Game Music Companion* (2021) | rigorous analysis of the five-note songs |
| Frank Lehman, *Hollywood Harmony*; Philip Tagg, *Everyday Tonality* | scholarly basis for "villain" and modal-mood claims |
| AOSA handbook / Frazee, *Discovering Orff* | authoritative bordun definitions (blogs were used) |
| *A Blind Legend*, *The Vale*, *Papa Sangre* developer interviews | music versus information sounds in audio-only games |
| Hi-Fi Rush GDC 2024 ([Vault](https://gdcvault.com/play/1034256/Developing-Hi-Fi-RUSH-Backwards)) | beat-synced action design |

## Engineering experiments

1. **Format bake-off for our own agents.** No study compares text music formats for Claude-class models. Have agents write the same 20 briefs in the grid format and in ABC, and compare hard-error rates and blind ratings.
2. **Agent versus hand-written calls.** Rate 20 agent calls blind against 5 hand-written ones (the tooling research's suggested first step).
3. **Loop seams.** Render a loop with both strategies (overlapping passes; the twice-render cut) and listen for clicks at 100 and 110 BPM.
4. **SoundFont audition.** Render every candidate call instrument at D4 F4 A4 C5 in the house bank and listen for attack precision (the perceived onset), pitch clarity and level match.
5. **Latency budget.** BLE HID latency on each target OS, motor rise time, and any audio output latency. These decide whether "absolute" judging is ever usable (see the prior report).

## How to keep these docs alive

- When a playtest settles a question, record the answer here, and change the rule it affects in 01–05, citing the session.
- When the team makes a decision (D1–D9), move it into the relevant guide and delete the row.
- New research goes into `research_notes/Music composition guidelines/` in the same format (Takeaway, Cited Findings, Inferences, Gaps). Guides link to it.
