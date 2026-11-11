# EchoQuest music guidelines

How to compose the short musical snippets that EchoQuest's battles are made of: calls the child echoes, backing loops, stingers, fanfares and character motifs. Written for **AI agents and humans iterating quickly on the demo**. These guides are both a blueprint and a research base: every rule says whether it comes from evidence, from music theory, or from opinion, and links its source.

They build on the earlier report, [`reports/Audio rhythm game music design.md`](../../reports/Audio%20rhythm%20game%20music%20design.md), which chose the note set, the battle loop, timing judgement and the age stages. Raw research is in [`research_notes/`](../../research_notes/).

## Files

| File | Read it when you are… |
|---|---|
| [01-palette.md](01-palette.md) | doing anything. The fixed pitches, the five forbidden backing notes, safe chords, the mode wheel, registers, tempi |
| [02-calls.md](02-calls.md) | writing a call, answer or refrain: the ten rules, stage limits, rhythm vocabulary, cadences, development operators, the starter library |
| [03-backing.md](03-backing.md) | writing a loop: progressions by mood, borduns, bass, drums, voicings, texture during the listening turn |
| [04-battle-cues.md](04-battle-cues.md) | planning an encounter: battle phases, the cue sheet, adaptive layers, leitmotifs, the emotional palette |
| [05-format-and-pipeline.md](05-format-and-pipeline.md) | producing anything an agent writes: the YAML/grid spec, hard checks and warnings, quality rubric, prompt templates, rendering, Unity, licences, General MIDI numbers |
| [06-worked-example.md](06-worked-example.md) | looking for a complete reference: a draft of the orchestra's song plus every asset of the first piano challenge |
| [07-open-questions.md](07-open-questions.md) | deciding or researching: team decisions, playtest questions, unread sources, experiments |
| [resources.md](resources.md) | going deeper: an annotated reading list |

## The rules card

If you read nothing else, read this.

**Material**
- Buttons 1–4 = **D4 F4 A4 C5** (la do mi so; a Dm7 = F6 chord). They are the same on every instrument for the whole game; only octave shifts of the whole set are allowed.
- **Playable layer** (calls, echoes, companion, stingers) = those four pitches only.
- **While a button can sound, the backing never uses E, B, F♯, C♯ or G♯.** What remains is B♭ C D E♭ F G A.
- Safe chords: Dm(7), F/F6, B♭(maj7), Gm7(9), **C and G only as sus or 5**, E♭maj7♯11 (rationed). Never plain C, G, A, Am or D major while input is open.
- **D centre = battle**, **F centre = home and victory**. Moving the bass from D to F under the same notes means "tamed".
- The call register C4–D5 is reserved. Bass and pads stay below C4; nothing pitched sits above a call while it plays.

**Calls**
- One four-beat line: **3 beats of notes + a rest**. The rest is where the "your turn" *TOC* goes.
- **Rhythm first.** Hook in the first 2–4 notes. Repeat something. **Exactly one surprise.**
- Neighbour buttons and repeats for the youngest; leap-then-reverse from about age 8.
- Echoes **end on the tonic (D or F), long, on a strong beat**. Questions end on so or mi.
- Develop **between rounds** (repeat → vary the end → sequence → fragment), not inside a call.
- Two calls must differ in **contour or rhythm**. A transposed copy is the same song to a child.
- When in doubt, **remove a note**.

**Battles**
- Pulse **100–130 BPM**. Get urgency from subdivision and drums, never from tempo. Never slow a call down to help.
- **One chord per exchange**, so call and echo share harmony.
- The beat never stops. During the call bar, only drums and bass play.
- Success **adds a layer** at the next bar line; failure removes only the top layer and triggers a *comic* taunt.
- Characters: **instrument + 3-note cell + rhythm + register + tempo word**. Present them before the fight.
- **Playful-menacing, never scary:** low staccato, wobbles, pizzicato, timpani. No slow high glissandi, clusters or jump scares.
- Chromatic villain colour (B♭m slide, A7, D major hit) goes **only in input-locked bars**.

**Process**
- Write the plan (form, motif rhythm, contour, development) **before** the notes, in the grid format.
- **Run the checker. Never trust self-checking.** Make 5–10 candidates, rank them with the rubric, and have a human listen to the top 2–3.
- The LLM writes notes. Code writes velocities, articulation and humanisation. **Calls are never humanised.**

## Confidence legend

Every guide uses these terms:

- **Evidence**: a cited study or primary source; follow the link to check the population (almost always adults or sighted children).
- **Computed**: interval or chord arithmetic, verified by script; this is certain.
- **Opinion** or **proposal**: design judgement by the researchers or the guide's author. Change it freely when a playtest disagrees.

No snippet in these guides has been played to a child yet.
