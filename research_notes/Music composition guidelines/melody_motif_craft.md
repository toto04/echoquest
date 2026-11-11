# Melody and motif craft for a four-note set: writing short, catchy enemy calls (EchoQuest)

Reading notes for the report writer:

- Scope: how to *compose* calls and phrases. Things the earlier notes already cover (Ocarina of Time note set, Kodály/Orff basics, contour-over-interval memory, the earworm study, children's span and timing data) are not repeated; see `research_notes/Audio rhythm game music design/` and `reports/Audio rhythm game music design.md`.
- "Cited Findings" are facts from a page or PDF I opened in this session, unless marked **(search extract only)**, which means the claim came from a search-result digest and I did not read the page. **(secondary)** means a later source reporting an earlier study.
- "Inferences" are my reasoning and example cells. None of the cells has been played to a child. Treat them as starting drafts.
- Several primary texts could not be opened (Huron's book, Caplin, Schoenberg, Margulis's book, Orff-Keetman *Music for Children*, several AOSA and Open Music Theory pages returned HTTP 403). Where I rely on a summary, I say so.

**Notation used in every Inferences section.**

| Button | Pitch | Solfa (F = do) | Role if the centre is F | Role if the centre is D |
|---|---|---|---|---|
| 1 | D4 | la | 6th | tonic (1) |
| 2 | F4 | do | tonic (1) | minor 3rd |
| 3 | A4 | mi | 3rd | 5th |
| 4 | C5 | so | 5th | minor 7th |

Durations: `s` sixteenth, `e` eighth, `q` quarter, `q.` dotted quarter, `h` half, `h.` dotted half, `r` plus a duration is a rest (for example `rq` is a quarter rest), and `|` is a bar line. A cell is written as pitch plus duration, for example `F4 e, A4 e, C5 q`. 4/4 time unless stated. Example tempo is 100 to 120 BPM.

Interval facts I use throughout (my arithmetic): neighbouring buttons are 3, 4 and 3 semitones apart (D-F minor 3rd, F-A major 3rd, A-C minor 3rd). Skipping one button gives a perfect 5th (D-A, F-C, 7 semitones). Button 1 to button 4 is a minor 7th (10 semitones), the only wide leap. There is no semitone and no tritone. The set is a D minor 7th chord, which is the same thing as an F major 6th chord. The total range is 1000 cents, which matters for section 6.

---

## 1. Huron's expectation theory: what listeners predict, and how to use it

### Takeaway
Huron's ITPRA theory says that a correct prediction is rewarding in itself, and that listeners misattribute that reward to the music. That is why predictable, schema-fitting material is liked. Surprise is enjoyed when it contrasts with a safe outcome. The melodic regularities listeners learn are pitch proximity (small intervals), step declination (descending steps tend to continue down), step inertia (a step tends to continue in its direction), melodic regression (extreme pitches return toward the middle) and the melodic arch (phrases rise then fall). Listeners approximate regression with a cruder rule, post-skip reversal: after a leap, turn back. For calls a child must reproduce, the practical rule is to write calls that are mostly predictable, with exactly one deliberate violation.

### Cited Findings
- The ITPRA theory has five components. Two act before the outcome: **imagination** (imagining and evaluating possible outcomes) and **tension** (tuning arousal and attention to the uncertainty and imminence of the outcome). Three act after it: **prediction** (did the outcome confirm the expectation), **reaction** (a fast, automatic, pessimistic response), and **appraisal** (slower, conscious evaluation). Predictive success gives "positive limbic reward", and failure gives "a negative limbic penalty in the form of surprise". — [Pearce & Müllensiefen, review of *Sweet Anticipation*](https://www.marcus-pearce.com/assets/papers/huron06-review.pdf)
- The **prediction effect**: "positive emotions resulting (via the prediction response) from anticipatory success are misattributed to the stimulus itself, leading to a preference for predictable events." Surprise can still be pleasant through **contrastive valence**: an unexpected but harmless or welcome event makes the later reaction or appraisal more positive. Delaying an expected event builds tension, and that tension enhances the reward when the event arrives. — [Pearce & Müllensiefen](https://www.marcus-pearce.com/assets/papers/huron06-review.pdf)
- Huron's chapters 5 and 6 discuss "five statistical regularities in music and listeners' sensitivity to each one". Pitch proximity: "successive pitches in melodies tend to be proximal", and listeners expect this. Melodic regression to the mean is "the tendency for pitches in the extremes of the pitch range to be followed by more central pitches". Listener expectations show post-skip reversal instead, "an approximation to this regularity". Huron argues that post-skip reversal wins in the mind because it needs only the last two events, whereas regression needs a running estimate of the mean pitch. — [Pearce & Müllensiefen](https://www.marcus-pearce.com/assets/papers/huron06-review.pdf)
- The five regularities are named as pitch proximity, step declination, step inertia, melodic regression and melodic arch. The digest attributes these glossary definitions to the book: step declination is the tendency "for a descending melodic step to be followed by a subsequent lower pitch"; the melodic arch is the tendency "for melodic phrases to exhibit a rising then falling pitch contour". Real melodies show step inertia mainly for *descending* steps, but listeners expect it in both directions. **(search extract only; I could not open a legitimate copy of the book.)** — [MIT Press book page](https://mitpressbookstore.mit.edu/book/9780262582780)
- An experiment with 28 music students and 12 non-musicians, using random 12-tone fragments, found that musicians expected **skip reversal** (after a skip of 3 to 11 semitones they predicted a change of direction more than 50% of the time, more strongly after larger skips) and **step momentum** (after a 1 to 2 semitone step they predicted continuation). Non-musicians did not show these rules clearly, so von Hippel argues the rules are learned heuristics. In real melodies, "skip reversal is just an artifact of regression toward the mean": "An upward skip usually lands on a high note, and from there the melody usually turns downward." — [von Hippel 2002, ICMPC 7 proceedings](https://lbj.utexas.edu/sites/default/files/file/faculty/Expectations%20as%20learned%20heuristics.pdf)
- Pitch proximity is described as "quite possibly innate" and tied to the physics of sound sources, whereas scale-degree expectations are learned. — [von Hippel 2002](https://lbj.utexas.edu/sites/default/files/file/faculty/Expectations%20as%20learned%20heuristics.pdf)
- Leaps fall into four types by their position relative to the median pitch. Over more than 3,000 leaps, melodies changed direction after median-departing and median-crossing leaps, were about 50/50 after median-landing leaps, and tended to *continue* after median-approaching leaps. This fits regression to the mean, not post-skip reversal (von Hippel & Huron 2000). In one folksong, 13 leaps were followed by 11 reversals. — [Shanahan, "Regression to the Mean in Music" (course page)](https://shanahdt.github.io/mus-345-445/emp_methods_workshop/regression_in_music.html)
- Step inertia shows strongly in European folk song, classical themes and hymns, but not in modern Western popular music. **(search extract only)** — [Music & Science 2024, via DOAJ](https://doaj.org/article/43b320953f444935a677d6cec2cb74f8)
- Huron's 1996 contour typology compares the mean pitch of the first, middle and last parts of a phrase, giving nine types. Four of them (ascending, descending, convex/arch, concave) are very frequent, and the other five are each at most 1.2% of the data. A 2026 re-analysis argues that the skew is partly an artefact of Huron's tolerance parameter and that contours do not form discrete clusters. — [Cornelissen, Burgoyne, Honing & Zuidema 2026, arXiv](https://arxiv.org/html/2604.13119v1)

### Inferences
- **The "mean" of this set is between F4 and A4.** Regression and post-skip reversal give the same advice here. After you reach C5 (button 4), come down. After you reach D4 (button 1), go up or stop. The safe leaps are median-crossing: D4 to A4, or F4 to C5, then reverse.
  - Leap and turn: `D4 q, A4 q, F4 q, D4 q` (1 3 2 1). An upward perfect 5th, then a stepwise descent home to la. This satisfies post-skip reversal *and* step declination, so it is very "expected" and good for a first minor-colour call.
  - Leap that continues (an intentional surprise): `D4 q, A4 q, C5 h` (1 3 4). The leap crosses the middle, and both regression and post-skip reversal predict a turn back down, but the line keeps rising to the top of the set. This is the one violation; use it as the hook of a "rising threat" enemy.
- **"Steps" in this set are thirds.** Every set-neighbour move (3 or 4 semitones) counts as a "small" interval in Schellenberg's sense (below 6 semitones; section 2). Every skip of a button (7 or 10 semitones) counts as "large". So the expectation rules work on button adjacency: moving to a neighbouring button is a step, skipping a button is a leap.
- **Step declination makes descending runs feel natural and ascending runs feel like questions.**
  - Closing descent ("Fall"): `C5 q, A4 q, F4 q, D4 q` (4 3 2 1). This is the most predictable four-note line in the set; use it as a cadence or a "defeated enemy" figure.
  - Rising question: `D4 e, F4 e, A4 e, C5 e, C5 h` (1 2 3 4 4). Less expected; ends high and open.
- **Arch as the default phrase shape.** `F4 e, A4 e, C5 q, A4 q, F4 q` (2 3 4 3 2). It rises, peaks once, and falls back to the start note, so it satisfies the arch, regression and declination at once.
- **One violation per call, made with tension and release (the ITPRA recipe).** Make the call predictable everywhere except one moment: a leap that does not reverse, a delayed final note (a rest before the tonic), or a repeated note where a step was expected. The prediction effect says the predictable remainder is what makes the call pleasant and easy to echo. The violation is what makes it distinctive. Delaying the expected tonic by one beat (`... A4 q, rq, F4 h`) is a cheap tension device that keeps the pitch content trivial.
- **For echoing, predictability is a feature, not a flaw.** A call that follows proximity, reversal and declination is easier to predict *while it is heard*, so it is easier to rehearse in memory before the child's turn. Save the "unexpected" calls for bosses.

### Gaps
- I did not read *Sweet Anticipation* itself. The step declination and arch definitions and the step-inertia asymmetry come from a search digest. Chapter-level quotes should come from the book (MIT Press 2006, ch. 5 to 7).
- Huron's 1996 *Computing in Musicology* paper on the melodic arch was not retrieved, so no percentages for the arch in folk song are given.
- None of these corpus findings were measured on children's songs or on four-note sets.

---

## 2. Narmour's implication-realization model, Schellenberg's two-factor model, and children

### Takeaway
Narmour's model says that a small interval implies continuation in the same direction with a similar small interval, and that a large interval implies a reversal with a smaller interval. Schellenberg reduced the model to two factors, **pitch proximity** and **pitch reversal**, without losing accuracy. In children, proximity is a strong expectation from age 5, while expectations of reversal and of symmetric patterns (X-Y-X) appear only in older listeners. For the youngest players, write calls that move between neighbouring buttons. Use X-Y-X returns and leap-then-reverse figures from about age 8.

### Cited Findings
- Narmour's archetypes: **P (process)** is the same direction with similar intervals. **D (duplication)** is identical intervals with lateral (repeated-pitch) motion. **R (reversal)** is a large interval followed by a smaller one in the opposite direction. Derived types are IP, VP, IR, VR and ID. **Registral return** is the "aba" archetype, an exact or near return to a pitch. **Closure** (a felt group boundary) occurs when direction changes, or when a large interval is followed by a smaller one. — [Wikipedia, "Implication-Realization"](https://en.wikipedia.org/wiki/Implication-Realization)
- Schellenberg's 1997 restatement as five principles: registral direction (small intervals imply continuation, large intervals imply a change); intervallic difference (small intervals imply a similar-sized next interval, within ±2 semitones if direction changes and ±3 if not; large intervals imply a smaller one); registral return (the next tone lands within 2 semitones of the earlier pitch); proximity (small realized intervals are more strongly implied); closure. — [Wikipedia, "Implication-Realization"](https://en.wikipedia.org/wiki/Implication-Realization)
- Schellenberg, Adachi, Purdy & McKinnon (2002) tested adults and 11- and 8-year-olds rating continuation tones (Experiment 1), and 11-, 8- and 5-year-olds *singing* continuations of two-tone stimuli (Experiment 2). The two-factor model (Schellenberg 1997) "equaled or surpassed" the I-R model. "Listeners of all ages expected the next tone in a melody to be proximate in pitch to the tone heard most recently. Older listeners also expected reversals of pitch direction, specifically for tones that changed direction after a disruption of proximity and for tones that formed symmetric patterns." — [Schellenberg et al. 2002, *J. Exp. Psych.: General* 131(4)](https://sites.utm.utoronto.ca/sites/sites.utm.utoronto.ca.glenn_website/files/download/JEPGeneral.pdf)
- In the same paper, PITCH PROXIMITY was a robust predictor in every age group, and PITCH REVERSAL was significant for adults but not for either child group in Experiment 1. "In both experiments, PITCH PROXIMITY was a better predictor of response patterns for the youngest age group than PITCH REVERSAL was for the oldest group." — [Schellenberg et al. 2002](https://sites.utm.utoronto.ca/sites/sites.utm.utoronto.ca.glenn_website/files/download/JEPGeneral.pdf)
- The authors note that their test melodies "did not have the repeated, short motives that characterize many nursery tunes and popular songs (e.g., 'Three Blind Mice,' 'Billie Jean')". — [Schellenberg et al. 2002](https://sites.utm.utoronto.ca/sites/sites.utm.utoronto.ca.glenn_website/files/download/JEPGeneral.pdf)

### Inferences
- **Age ladder for interval content** (my mapping of the age findings onto the set):
  - Ages 5 to 7: mostly neighbour moves and repeated notes, with at most one skip. For example "Swing": `F4 q, A4 q, F4 h` (2 3 2), or "Knock": `A4 e, A4 e, F4 q, A4 e, A4 e, F4 q` (3 3 2, 3 3 2).
  - Ages 8 and up: leap-then-reverse (`D4 q, A4 q, F4 h`, 1 3 2) and symmetric X-Y-X patterns over a wider span (`D4 q, C5 q, D4 h`, 1 4 1). The second is the widest symmetric pattern possible; it reads as a deliberate "jump".
  - Ages 10 and up: unreversed leaps and the minor 7th used melodically (`D4 q, C5 q, A4 q, C5 q`).
- **Registral return is the natural "rhyme" inside a cell.** X-Y-X cells (2 3 2, 3 4 3, 1 2 1) have built-in closure (direction change plus return), so they make good one-beat or two-beat basic ideas. In Narmour's terms, a run of identical pitches (D, duplication) is the most predictable of all, which is why repeated-note openings are easy entry points.
- **Use closure to mark the end of the call.** End on a direction change or on a large-to-small interval pair, and add a longer duration (see section 9). Example ending: `... C5 q, F4 q, A4 q, F4 h`. The leap down F-C is followed by the small step back up to A and then home to F; the closure is clear.
- **Avoid "process" chains of four or more in one direction at early ages,** except the stock descending run (section 1). A long same-direction chain implies continuation beyond the set's edges, so the child expects a note the controller does not have.

### Gaps
- I did not open Narmour (1990, 1992) or Schellenberg (1997) themselves. The principle list comes from Wikipedia, cross-checked against the 2002 paper's framing.
- The 2002 study used Western tonal stimuli with semitones. Whether a set made only of thirds and fifths changes children's expectations is untested.

---

## 3. Motif development when only four pitches exist

### Takeaway
The standard toolkit (repetition, varied repetition, sequence, inversion, retrograde, augmentation, diminution, fragmentation, liquidation, extension, displacement) works inside four pitches if "transposition" means **moving each note to the neighbouring button** (a tonal sequence, in which the interval qualities change from major to minor 3rd). Because pitch options are so few, rhythm-side transformations (augmentation, displacement, changing a single duration) carry most of the variety. They also train the skill the game tests.

### Cited Findings
- Definitions from an open textbook: **inversion** means "an idea is exactly upside-down or 'mirrored'", either real (every interval keeps its exact quality) or tonal (intervals "abide by the scale or key"). **Augmentation** "usually refers to an exact doubling of the duration of every rhythmic value"; **diminution** is the exact halving. **Rhythmic change**: "some but not all rhythmic values of the motive are varied". **Intervallic change**: the rhythm stays, "some of the intervals are different". **Extension**: "additional material is added to the end of a motive upon its repetition". **Retrograde**: "an exact reversing of the order of notes". — [Hutchinson, *Music Theory for the 21st-Century Classroom*, "Melodic Alteration"](https://musictheory.pugetsound.edu/mt21c/MelodicAlteration.html)
- Open Music Theory's continuation features: **fragmentation**, "Breaking the melody into smaller chunks"; **liquidation**, "Gradually replacing the characteristic or unique parts of a melody with conventional or common elements"; **sequential repetition**, "Repeating the same melodic or harmonic element two or three times, transposed to different pitch levels"; acceleration of melodic rhythm (moving from quarters and eighths toward mostly eighths); acceleration of harmonic rhythm. — [Open Music Theory (v1 mirror), "Classical theme functions"](https://hybrid-pedagogy.github.io/openmusictheory/themeFunctions.html)
- In the Open Music Theory 2e chapter, "fragmentation refers only to the length of the units", not their content. Varied repetition of a basic idea commonly uses rhythmic or melodic embellishment, transposition, a change of harmonization, or a change of interval quality or size. Keeping the contour is what makes a variant heard as a repetition rather than as contrast. — [Open Music Theory 2e, "The Phrase Archetypes and Unique Forms" (LibreTexts)](https://human.libretexts.org/Bookshelves/Music/Music_Theory/Open_Music_Theory_2e_(Gotham_et_al.)/03%3A_Form/3.02%3A_The_Phrase_Archetypes_and_Unique_Forms)
- Fragmentation is the "division of a musical idea (gesture, motive, theme, etc.) into segments". Schoenberg's **liquidation** is the "reduction of a large-scale musical idea to its essential form" (citing *Fundamentals of Musical Composition*, p. 58). The two are related but separate processes. — [Wikipedia, "Fragmentation (music)"](https://en.wikipedia.org/wiki/Fragmentation_(music))
- Open Music Theory (Nebraska edition) lists enlargement, contraction, inversion, displacement and retrograde as common motive transformations. **(search extract only; page returned 403)** — [Open Music Theory, Nebraska Pressbooks](https://pressbooks.nebraska.edu/openmusictheory/?p=282)
- A Berklee classroom method for pentatonic composition: write 4- or 8-bar rhythmic patterns first, then add pentatonic pitches. Restrict some patterns to two, three or four pitches. Then try "elongation" (longer notes, same pitches) and "truncation" (shorter notes, same pitches). — [Berklee PULSE, "Exploring Form Using Minor and Major Pentatonic Scales" (PDF)](https://pulse.berklee.edu/content/public/pub_unit_assets/lessons/lesson2_banded/Exploring%20Form.pdf)

### Inferences
Base cell **X** = `F4 e, A4 e, C5 q` (buttons 2 3 4, rhythm ti-ti ta, two beats). Every transformation below fits in two beats unless noted.

| Technique | Result | Buttons | Comment |
|---|---|---|---|
| Exact repetition | X X | 2 3 4, 2 3 4 | The basic presentation; the memory cost is one cell |
| Varied repetition (a a') | X, then `F4 e, A4 e, F4 q` | 2 3 4, 2 3 2 | Same rhythm and head, new ending. The cheapest "rhyme" |
| Set-step sequence down | X, then `D4 e, F4 e, A4 q` | 2 3 4, 1 2 3 | Every note moves one button down. Intervals change from M3+m3 to m3+M3 (a tonal sequence). Going up is impossible from 2 3 4, so sequence down from high cells and up from low cells |
| Sequence of a 2-note cell, three times | `A4 e, C5 e, F4 e, A4 e, D4 e, F4 e, D4 q` | 3 4, 2 3, 1 2, 1 | The classic falling sequence by step; ends on la. Good continuation material |
| Contour inversion (mirror the button positions 1↔4, 2↔3) | `A4 e, F4 e, D4 q` | 3 2 1 | Up-up becomes down-down. A "tonal" inversion within the set |
| Exact inversion about the start note | not playable: from F4 the mirror of "up M3, up m3" needs two notes below F4, and only D4 exists | — | Exact inversions run out of room quickly; prefer the positional mirror |
| Retrograde (pitch and rhythm) | `C5 q, A4 e, F4 e` | 4 3 2 | Also reverses the rhythm (ta ti-ti). A good "answer" to X |
| Augmentation | `F4 q, A4 q, C5 h` | 2 3 4 | Same identity at half speed. Teaches the cell to the youngest; use for the tutorial pass, then the real-speed call |
| Diminution | `F4 s, A4 s, C5 e` | 2 3 4 | Sixteenths at 110 BPM are about 136 ms apart (my arithmetic). Probably too fast for button input below about age 9 (unverified) |
| Fragmentation | `F4 e, A4 e, F4 e, A4 e, F4 e, A4 e, C5 q` | 2 3 ×3, 4 | Repeats the 2-note head, then releases. Builds excitement before a boss attack |
| Liquidation | X X, then `A4 e, F4 e, F4 h` | …, 3 2 2 | The distinctive top note C5 disappears and the line settles on the tonic F4 |
| Extension | X, then X + `A4 q, F4 q` | 2 3 4, 2 3 4 3 2 | The repeated cell grows a tail (the arch) |
| Rhythmic displacement | `rq, F4 e, A4 e, C5 q, rq` | 2 3 4 | The same cell starts one beat late, so C5 lands on beat 3 (strong) instead of beat 2. The emphasised note changes, and so does the feel |
| Rhythmic change only | `F4 q, A4 e, C5 e` | 2 3 4 | ta ti-ti instead of ti-ti ta. A different song to a child, with the same pitches (see the prior report's "rhythm alone" point) |
| Intervallic change only | `F4 e, A4 e, D4 q` | 2 3 1 | Same rhythm and same first two notes; the ending drops instead of rising |

- **Rhythm first, pitches second** (the Berklee method) suits this set especially well. Compose the rhythm cell, then try it on two pitches, three, and four. A call such as `A4 e, A4 e, F4 q, A4 e, A4 e, F4 q` (two pitches) is the same rhythmic hook as `A4 e, C5 e, F4 q, A4 e, D4 e, F4 q` (four pitches), so the rhythm can be taught first and the pitch layer added at a later level. That gives a built-in difficulty ladder.
- **Development across a battle, not inside a call.** A call is only 1 to 2 bars, so most development should happen *between* calls. Round 1 plays X X. Round 2 plays X X'. Round 3 plays the sequence. The final round plays the fragmentation and liquidation. The child keeps hearing the same identity while the task grows. This also suits the earlier finding that chunkable, regular sequences are remembered better (Mathy et al. 2016, cited in the prior report).
- **Keep the contour when you vary.** Open Music Theory notes that contour decides whether a variant is heard as "the same idea". So for an "easy variant" round, change the rhythm or one interval but keep the up/down pattern. For a "new enemy" signal, change the contour.

### Gaps
- No source discusses motivic development under a hard four-pitch limit. The table is my application of standard definitions.
- The Open Music Theory chapter on motive (Nebraska and VIVA editions) returned 403. Its exact wording on displacement and on enlargement and contraction is unverified.
- Schoenberg's *Fundamentals* (p. 58) was not opened.

---

## 4. Sentence and period at the scale of a 1 to 2 bar call

### Takeaway
Classical phrase archetypes shrink well. A **sentence** is a basic idea, its repetition, then a continuation that fragments, speeds up and drives to a cadence, in roughly 1 : 1 : 2 proportion. A **period** is an antecedent (basic idea plus contrasting idea, weak cadence) followed by a consequent that restates the basic idea and ends with a stronger cadence. In a two-bar call, the sentence is the natural shape for an enemy's *call*, and the period is the natural shape for a *call-and-answer* exchange between enemy and child.

### Cited Findings
- "A sentence consists of two subphrases: the presentation and the continuation." The presentation is "a subphrase comprised of a basic idea (b.i.) and its repetition". Basic ideas are often two measures long, though one-measure and four-measure ideas also occur. The continuation "typically feels less stable than the presentation", is usually as long as the presentation or longer, and always drives toward a cadence. — [Open Music Theory 2e (LibreTexts)](https://human.libretexts.org/Bookshelves/Music/Music_Theory/Open_Music_Theory_2e_(Gotham_et_al.)/03%3A_Form/3.02%3A_The_Phrase_Archetypes_and_Unique_Forms)
- A period has two phrases, each a basic idea followed by a contrasting idea. The antecedent "ends with a weaker cadence, most often a half cadence (HC)". The consequent "ends with a stronger cadence than the antecedent, most often a perfect authentic cadence (PAC)". The consequent's basic idea is usually similar or identical to the antecedent's, and its contrasting idea is altered. — [Open Music Theory 2e (LibreTexts)](https://human.libretexts.org/Bookshelves/Music/Music_Theory/Open_Music_Theory_2e_(Gotham_et_al.)/03%3A_Form/3.02%3A_The_Phrase_Archetypes_and_Unique_Forms)
- The prototypical sentence "is eight measures long and contains two four-measure phrases"; "Continuation phrases acquire momentum that leads to the cadence". — [Open Music Theory (v1 mirror), "The sentence"](https://hybrid-pedagogy.github.io/openmusictheory/sentence.html)
- In the v1 period page, the antecedent "ends with a weak cadence, either a HC or an IAC"; the consequent "closes with a strong cadence ... (and almost always) a PAC". The repeated basic idea at the start of the consequent is treated as essential. — [Open Music Theory (v1 mirror), "Period"](https://hybrid-pedagogy.github.io/openmusictheory/period.html)
- Cadential function typically uses "a conventional, descending melodic pattern". — [Open Music Theory (v1), "Classical theme functions"](https://hybrid-pedagogy.github.io/openmusictheory/themeFunctions.html)
- The Ocarina of Time songs open with two short phrases on a three-note motif and resolve with a longer phrase, which is a sentence shape (already in the prior notes). — see `research_notes/Audio rhythm game music design/music_theory_note_sets.md` §1

### Inferences
There is no dominant harmony in a drone-backed pentatonic set, so I treat "weak cadence" as ending on so or mi (C5 or A4) and "strong cadence" as ending on the tonic (F4, or D4 for the minor centre) with a long note on a strong beat. This is the folk/pentatonic analogue, not Caplin's harmonic definition.

- **One-bar sentence (ages 5 to 7, 7 notes):**
  `F4 e, A4 e` + `F4 e, A4 e` + `C5 e, A4 e, F4 q`
  The basic idea (2 3) takes one beat, its repetition one beat, and the continuation plus cadence two beats (4 3 2), in 1 : 1 : 2 proportion. Memory load is one chunk plus a three-note descent.
- **Two-bar sentence (ages 8 and up, 11 notes):**
  Bar 1: `F4 e, A4 e, C5 q, F4 e, A4 e, C5 q` (b.i. X and its repetition)
  Bar 2: `C5 e, A4 e, C5 e, A4 e, F4 h` (fragmentation into 2-note units, all eighths for acceleration, liquidation to a long tonic)
  This is longer than the 4 to 6 item span of young children, but it is only three chunks (X, X, "C-A ×2, F").
- **Minor sentence on D:** `D4 e, F4 e, A4 q, D4 e, F4 e, A4 q | A4 e, F4 e, A4 e, F4 e, D4 h`.
- **Two-bar parallel period, major centre:**
  Antecedent (weak, ends on so): `F4 e, A4 e, C5 q, A4 q, C5 q`
  Consequent (strong, ends on do): `F4 e, A4 e, C5 q, A4 q, F4 q`
  Only the last note differs. That makes it a perfect first "answer the enemy" exercise: the child hears the antecedent and must change one note to close it.
- **Two-bar parallel period, minor centre:**
  `D4 e, F4 e, A4 q, F4 q, A4 q` (weak, ends on the 5th) / `D4 e, F4 e, A4 q, F4 q, D4 q` (strong, ends on the tonic D).
- **Game mapping.** Echo levels: the enemy plays a sentence and the child copies it. Answer levels (later): the enemy plays an antecedent and the child plays the consequent, which differs only in the cadence note. This teaches "home" versus "away" by ear and gives the child a creative-feeling move with almost no new memory load.
- **The continuation is where the difficulty lives.** Keep the presentation identical across difficulty tiers and make the continuation longer, faster or more fragmented for harder enemies. Children recognise the enemy from the presentation (its "face") while the challenge rises.

### Gaps
- Caplin's *Classical Form* (1998) and *Analyzing Classical Form* (2013) were not opened. The definitions above are Open Music Theory's restatements.
- No study tests whether children perceive sentence or period structure in one- or two-bar units. Reducing the archetypes to this scale is my design move.

---

## 5. What makes a hook: recognisability, repetition and typicality

### Takeaway
Large-sample studies of song recognition and of folk-song transmission agree on the same few properties. Segments are more recognisable and more stable when they are **repetitive**, use **typical (conventional) motives**, have **small intervals**, are **short**, are **expected** in the statistical sense, and come **early** in the song. In audio, a **prominent vocal-like lead** was the strongest predictor. Repetition does more than help memory: it makes sound be heard *as music* and pulls attention to contour. For calls: put the hook first, repeat it, make it conventional with one twist, and make the call voice stand out from the backing.

### Cited Findings
- *Hooked* defines catchiness as "long-term musical salience, the degree to which a musical fragment remains memorable after a period of time". The pilot found significant differences in time to recall across segments, and players were "not as reliable as one might expect at predicting their own recall performance". **(search extract only)** — [Burgoyne, Bountouridis, Van Balen & Honing, ISMIR 2013](https://archives.ismir.net/ismir2013/paper/000101.pdf)
- In the Hooked! recognition data (1,715 segments, 321 songs, 973 participants; a symbolic subset of 99 songs and 536 segments), most significant audio predictors were "conventionality" components: "more recognizable sections have a more typical, expected sound". Timbral recurrence also mattered: "sections that are more representative of a song are more recognizable". The strongest effect was **Vocal Prominence** (β = 0.14 in the full audio model). In the symbolic model, **Melodic Repetitivity** (β = 0.12) and **melody/bass conventionality** (β = 0.07) were significant: "recognizable melodies are more repetitive" and "contain more typical motives". Marginal R² was only about .06 to .10, so most variance is unexplained. — [Van Balen, Burgoyne, Bountouridis, Müllensiefen & Veltkamp, ISMIR 2015](https://archives.ismir.net/ismir2015/paper/000148.pdf)
- In 9,639 phrases from 147 Dutch folk tune families, phrases that survived oral transmission with less variation were shorter (β −0.30), more expected under IDyOM (lower surprisal, β −0.24), earlier in the melody (β −0.10), had smaller intervals (β +0.10), were repeated more within the melody (β +0.09), had more repeating motifs (β +0.08), and fulfilled pitch-reversal expectations (β +0.05). The model's fit was low (marginal R² 0.05). — [Janssen, Burgoyne & Honing 2017, *Frontiers in Psychology*](https://www.frontiersin.org/journals/psychology/articles/10.3389/fpsyg.2017.00621/full)
- Margulis cites Huron's estimate that for "more than 90 per cent of the time spent listening to music" people hear passages they have heard before. She argues that "the exact same sequence of sounds can seem either like speech or like music, depending only on whether it has been repeated" (the speech-to-song illusion), and that musicalisation "shifts your attention from the meaning of the words to the contour of the passage". — [Margulis, "Why repetition can turn almost anything into music", *Aeon*](https://aeon.co/essays/why-repetition-can-turn-almost-anything-into-music)
- Margulis's book *On Repeat: How Music Plays the Mind* (OUP, 2014) is the full treatment. **(search extract only; I read the Aeon essay, not the book)** — [TAMU library record](https://libcat.library.tamu.edu/Record/in00005709729/Description)
- Melody recognition (Dalla Bella, Peretz & Aronoff 2003, gating paradigm): the more familiar the melody, the fewer notes needed. One citing paper reports about 4 notes (2.3 s) to feel familiar. **(search extract only, secondary)** — [Springer PDF citing the study](https://link.springer.com/content/pdf/10.3758/PBR.17.3.335.pdf)
- Boltz & Jones (1986), as summarised by Chiappe & Schmuckler (1997): memory was helped by tonic-triad notes marking phrase endings, but only when a matching pattern of temporal accents highlighted them. **(search extract only, secondary)** — [Chiappe & Schmuckler 1997 (Scribd copy)](https://www.scribd.com/document/433978938/Chiappe-Schmuckler-1997)

### Inferences
- **Hook placement.** Put the identity of the call in its first 2 to 4 notes (the early phrases were the most stable in Janssen et al.; recognition can start after about 4 notes). The rest of the call can be stock material (a descent home).
- **Typical with one twist.** "Conventional" motives were *more* recognisable. Build calls from stock children's-song gestures (the falling minor 3rd, X-Y-X, the descending run, repeated-note openings; section 6) and add one distinctive element per call (section 1). This matches the prior notes' earworm finding: common global contour, unusual local gradient.
- **Repeat inside the call.** At least one of: a repeated note, a repeated cell (X X), or a repeated rhythm under new pitches. Melodic repetitivity was the top symbolic predictor of recognition, and within-melody phrase repetition predicted stability.
- **Vocal prominence.** The call should be in the most voice-like, foreground timbre in the mix, ideally a sung or voice-like enemy "voice" in the D4 to C5 register (a typical singing register). The backing should be lower and darker. This is an inference from "vocal prominence" in pop songs; it is consistent with the stream-separation advice in the earlier accessibility notes.
- **Accent the tonic at the end.** Following Boltz & Jones, finish on F4 (or D4) placed on a metrically strong beat and lengthened. A tonic ending without a temporal accent may not help memory.
- **Musical rhyme (inference; no direct study found).** Give two calls in the same battle the same ending and different beginnings: `F4 e, A4 e, C5 q, A4 q, F4 q` and `D4 e, D4 e, C5 q, A4 q, F4 q`. The shared tail `C5 q, A4 q, F4 q` acts like a rhyme. The child learns one cadence and two "faces".

### Gaps
- No study isolates "musical rhyme" (shared phrase endings) as a memory factor. The closest evidence is phrase-repetition and tonic-ending effects.
- Both large studies (Hooked!, Dutch folk song) use adult listeners or adult transmission. No equivalent exists for children's recognition of short game calls.
- The Hooked 2013 paper's detailed results were not read.

---

## 6. Children's songs and playground chants: corpus numbers and stock patterns

### Takeaway
Songs that children sing span about a minor 6th on average (about 941 cents) and use about 4.5 scale degrees. EchoQuest's set (4 notes, 1000 cents) sits almost exactly at that average, which is a strong argument that the set's *size* is child-typical. Children's verse is built on four-beat lines grouped in fours, and short lines are filled out by a silent beat. The descending minor-third taunt ("nyah nyah") is widespread across cultures. Its "universality" rests mostly on Bernstein and anecdote.

### Cited Findings
- In 100 globally sampled songs (50 sung by children aged about 8 to 16, including "nursery rhythms [sic] and gaming songs", excluding lullabies; 50 adult songs), the mean melodic range of children's songs was **941 cents** ("more than a minor 6th") against 1,362 for adult songs (p = 1.2×10⁻⁷). Children's songs used on average **4.5 scale degrees** against 5.8 for adults. Pentatonic scales were the most common (30% of the children's sample, 36% of the adult). Children's songs "tended to fall within one octave". The analysis was automated (Tarsos/YIN), and the authors warn that it may overestimate range. — [Sato, Six, Pfordresher, Fujii & Savage 2019, Folk Music Analysis workshop](https://0110.be/files/attachments/471/FMA2019_paper_13.pdf)
- Burling (1966, *American Anthropologist* 68:1418-1441) used children's verse across languages "as a means of getting at the unmarked" metrical structure. He found a binary hierarchy: 1 quatrain, 2 couplets, 4 lines, 8 hemistichs, 16 feet (beats). Short lines are rendered with "empty beats that come after short lines", so "Mary had a little lamb" scans as 4-3-4-3 beats, with the missing beat as a rest. Common line-beat patterns in his data: 4444, 4343, 4443, 3333, 3343. — [Hayes, UCLA Linguistics 251 handout "Quatrains I"](https://www.brucehayes.org/251HayesSchuhMetrics/handouts/12QuatrainI.pdf)
- Brailoiu (1956) and Burling (1966) independently found strikingly similar metrical patterns in children's rhymes worldwide. **(search extract only)** — [Arleo, "Fascinating Rhythm" (HAL)](https://halshs.archives-ouvertes.fr/halshs-00630404/file/Arleo_Fascinating_Rhythm_version_1.pdf)
- The "nyah nyah" taunt "features a descending minor third" and is usually six notes (sometimes five, with the first two notes merged into one long note). Variants include Korean la-so-la-so mi-re-mi-re, Japanese so-so-mi-mi so-mi-mi and Mexican so-la-so-mi (all flagged "citation needed"). Bernstein, in his 1973 Harvard lecture "Musical Phonology" (*The Unanswered Question*), called the figure "a clear case of a musical-linguistic universal" (quoted via a 2016 Slate article). — [Wikipedia, "Nyah nyah nyah nyah nyah nyah"](https://en.wikipedia.org/wiki/Nyah_nyah_nyah_nyah_nyah_nyah)
- A 2005 Auditory list thread questions whether the minor-third call is universal. **(search extract only)** — [auditory.org posting](https://www.auditory.org/postings/2005/57.html)
- In a corpus of French children's songs used to train a melody model, repeated-note intervals were "by far" the most frequent two-interval pattern, and tritones and minor 6ths were absent. The corpus was small (reported as 10 training songs, 437 intervals). **(search extract only; the arXiv abstract page I opened does not give these numbers)** — [Defays, French & Tillmann 2023, arXiv](https://arxiv.org/abs/2311.12431)
- The Kodály Center's American Folk Song Collection tags each song with tone set, rhythm set, range, form, game type and grade. For example, "Do, Do, Pity My Case" is tone set d r m s l, rhythm set ta, ti-ti, tiri-ti, eighth rest, anacrusis, range M6, form A(ab)B(cd), grades K-2. — [Kodály Center, song 481](https://kodalycollection.org/song.cfm?id=481)
- Pentatonic songs are the norm in Kodály's early sequence: so-mi first, then la, then the do and la pentatonics (already in the prior notes). — [SUNY *Music and the Child*, ch. 4 (Lumen)](https://courses.lumenlearning.com/suny-music-and-the-child/chapter/chapter-4-approaches-to-music-education-2/)

### Inferences
- **The set's range is child-typical.** D4 to C5 (1000 cents) against a children's-song mean of 941 cents, and 4 pitch classes against a mean of 4.5. Use the whole range, but not constantly: most calls should span a 5th (three buttons), saving the full minor 7th for climaxes.
- **The four-beat line is the call's container.** One call equals one four-beat line. A "3 + rest" line (three beats of notes, then a silent beat) is the most natural children's-verse ending, and it gives the child a clean silent beat before their echo starts. A 4343 pattern maps naturally onto *call (4) / answer (3 + rest) / call (4) / answer (3 + rest)* across a two-bar exchange.
- **The stock chants, mapped onto the set.** The so-mi taunt as C5-A4 (buttons 4 and 3):
  - "Nyah nyah" shape: the rhythm of the real taunt varies by country, so a safe version is `C5 q, A4 q, C5 e, C5 e, A4 q` (4 3 4 4 3; the prior report's "Hello" motif).
  - so-la-so-mi (the Mexican and "Rain, rain" family): needs la *above* so, which the set lacks (D5 is not available). The nearest substitute is so-so-mi with the D4 la *below*: `C5 q, C5 q, A4 q, D4 q` (4 4 3 1). Its character changes. Flag this as a design limitation: the classic so-la-so-mi playground figure cannot be played on this set. (If a tilt or octave modifier is added later, D5 would enable it.)
  - mi-re-do and other stepwise children's figures are also absent (no re), so "Mary had a little lamb"-type tunes cannot be quoted. The set's idiom is **arpeggiated, bugle-like**, closer to calls and chants than to stepwise nursery songs. Composers should lean into that, not fight it.
- **Repeated notes are free difficulty-zero material.** If repeated notes really are the most common interval in children's songs (low confidence, small corpus), calls should use plenty of them: `A4 e, A4 e, A4 q, F4 h`.

### Gaps
- The Sato et al. children's songs are songs *sung by* children in small-scale societies, recorded by ethnomusicologists, and analysed automatically. That is not the same population as Western nursery songs sung by adults to children.
- I found no Western nursery-song corpus with published statistics on phrase length in notes, rhythm-cell frequencies or interval distributions. The Kodály Center collection has the fields (tone set, rhythm set, range) and could be scraped and counted; that would be a valuable small project.
- The minor-third "universal" is not established by any study I retrieved. Bernstein's lecture was not watched.

---

## 7. Call-and-response traditions

### Takeaway
Call and response is "a succession of two distinct phrases that works like a conversation". Its main forms are leader/chorus (a leader states, the group answers), question/answer (one part poses an unfinished phrase and another finishes it) and antiphony (spatially separated groups). In African drumming the responder may reply with "either the same or a different pattern", which is the same echo/answer distinction EchoQuest needs.

### Cited Findings
- Definition, origins in many African regions, and the leader/chorus, question/answer and antiphony types. In African drumming, "The master drummer sets a pattern, and the accompanying drummer responds with either the same or a different pattern"; responders add "their own creative touches". — [Wikipedia, "Call and response (music)"](https://en.wikipedia.org/wiki/Call_and_response_(music))
- Kondo (GDC 2007): "If the music doesn't reflect the rhythm of the game, it becomes background music". The Hyrule Field day music was built by randomly choosing among 12 phrases to form an eight-phrase melody. — [Game Developer, "GDC: Koji Kondo's Interactive Musical Landscapes"](https://www.gamedeveloper.com/game-platforms/gdc-koji-kondo-s-interactive-musical-landscapes)
- A Chinese write-up of the same talk says Kondo built melodies on a captured rhythm and wanted each song's identity to register within 2 to 3 seconds. **(search extract only)** — [Bilibili write-up](https://www.bilibili.com/opus/905066127452798982)

### Inferences
- **Three response modes, in order of difficulty:** (1) *echo*, the same pattern (the current game); (2) *fixed response*, where the child always answers with the same short refrain whatever the enemy plays (leader/chorus; good for the youngest, because the child's part never changes while the enemy's call varies); (3) *answer*, where the child completes the enemy's unfinished phrase (the period in section 4).
- **Fixed-response refrain idea:** the "hero's motto" `F4 q, A4 q, F4 h` (2 3 2), which the child plays after every enemy call in the tutorial area. It anchors the tonic and gives a success experience between harder echoes.
- **Hyrule-Field-style variety:** an enemy can have a pool of 3 to 4 calls that share a head (its identity) and differ in the tail, chosen at random per round. The child learns the enemy's "face" (head) and listens for the tail. This uses Kondo's phrase-pool technique inside a single enemy.
- **2 to 3 second identity window:** at 110 BPM a bar lasts about 2.2 s (my arithmetic). Each enemy's character should be audible within its first bar, which is another reason to put the hook first.

### Gaps
- No ethnomusicological source on children's call-and-response games (for example clapping games, ring shouts) was retrieved. Their typical call lengths and response rules would be directly useful.
- The original Kondo GDC slides or video were not retrieved.

---

## 8. Orff bordun and ostinato types as backing patterns

### Takeaway
A bordun is an open 5th on scale degrees 1 and 5 with no third. Its types differ only in *how* the two pitches are struck: together (chord), alternating (broken), alternating registers (level), crossed hands up to the upper tonic (arpeggiated/crossover), or with one or both pitches stepping to a neighbour and back (single or double moving). Teachers introduce chord and broken borduns first, then level, then crossover. On this set both natural borduns (F-C and D-A) use only button pitches, so any button press is consonant with the backing.

### Cited Findings
- Chord bordun: "The two pitches (scale degrees 1 and 5) are played together at the same time"; "the lowest barrier to entry ... often the first drone we use". Broken bordun: "The two pitches are played one at a time with alternating hands". Level bordun: "the two pitches are played together, but they alternate between different registers", either octaves on one instrument or different instruments. Arpeggiated (crossover) bordun: the left hand crosses over the right to play the higher tonic; "the most involved in terms of physical coordination". Single-moving: 1 and 5 are played while "one mallet moves to an upper neighbor and back". Double-moving: both mallets move to the upper neighbour and back. Simple bordun (the umbrella category): "Students always play scale degrees 1 and 5 throughout the whole piece". Students may be ready for borduns around 1st grade. — [Boler, "Types of Borduns in Elementary General Music" (teacher blog)](https://victoria-boler-857o.squarespace.com/blog/types-of-borduns-elementary-general-music)
- A community-college guide defines the bordun as "An open 5th, containing the 1st and 5th degrees of the chord. The 3rd is not played." The broken bordun example is C-G-C-G in quarters. A moving/broken chord bordun goes C-G to C-A and back. The level bordun "plays in different octaves". Crossover: "The mallets cross over to play the pattern" (D-A-D, then an octave higher). A drone is "A note or chord continuously sounding without change"; an ostinato is a "Motif or phrase that repeats". The example ostinato is C-G-A-G in quarters. — [Hostos CC, EDU 107 guide 2.7](https://guides.hostos.cuny.edu/edu107/2-7)
- An AOSA handbook defines the moving bordun as one in which one tone, almost always the upper voice, moves, and notes that a moving bordun can be hard to tell from a melodic ostinato. **(search extract only; PDF returned 403)** — [AOSA supporting materials, "Melody, Rhythm, Accompaniment"](https://member.aosa.org/storage/files/shares/CMS%20Pages/TE%20Content/Curriculum%20Docs/sm-supportingmaterials-melody%20rhythm%20accompaniment(3).pdf)
- SUNY *Music and the Child*: voice and speech come first ("singing songs and speaking and creating poems should be mastered before playing an instrument"); the next step is "to say (or sing) a rhyme along with an ostinato". Body percussion uses stamp, patsch, clap and snap. — [SUNY *Music and the Child*, ch. 4 (Lumen)](https://courses.lumenlearning.com/suny-music-and-the-child/chapter/chapter-4-approaches-to-music-education-2/)
- When children are not ready to sing a song, it can be chanted as a speech piece with one or more speech ostinati layered over it. **(search extract only)** — [SUNY *Music and the Child* (search digest)](https://milnepublishing.geneseo.edu/music-and-the-child/chapter/chapter-4/)
- *Music for Children* vol. 1 (Orff and Keetman, the pentatonic book) contains "speech and rhythmic exercises, nursery rhymes, songs, instrumental pieces and melodic exercises using only the five note scale". **(search extract only; retailer description)** — [Heid Music listing](https://www.heidmusic.com/music-for-children-volume-1-pentatonic/)

### Inferences
Backing patterns for each tonal centre. They are placed an octave or more *below* the buttons (D4 to C5) so the child's notes stay in the foreground. One bar of 4/4 each.

| Type | Centre F (major feel) | Centre D (minor feel) | Use |
|---|---|---|---|
| Chord bordun | `F2+C3 q` on every beat | `D2+A2 q` on every beat | Tutorial, youngest players; most stable |
| Broken bordun | `F2 q, C3 q, F2 q, C3 q` | `D2 q, A2 q, D2 q, A2 q` | Default "walking" backing; the pulse is very clear |
| Level bordun | `F2+C3 h, F3+C4 h` | `D2+A2 h, D3+A3 h` | Adds lift without new pitches; avoid an upper pair that reaches the button octave |
| Arpeggiated / crossover | `F2 e, C3 e, F3 e, C3 e` ×2 | `D2 e, A2 e, D3 e, A2 e` ×2 | Energetic battles; eighth motion under quarter-note calls |
| Single moving | `F2` held, upper `C3 q, D3 q, C3 h` | `D2` held, upper `A2 q, C3 q, A2 h` | D3 and C3 are button pitch classes, so no clash. Keep the moving voice slow so it does not compete with the call (the AOSA warning) |
| Double moving | `F2+C3 h, G2+D3 q, F2+C3 q` | `D2+A2 h, F2+C3 q, D2+A2 q` | The F version's G2 is outside the set but forms no semitone with any button. For D, the scale-step neighbours E and B would put B2 a minor 9th from a C5 press, so the D version moves by a set-step (to F+C) instead |
| Melodic ostinato | `F2 q, C3 q, D3 q, C3 q` (1 5 6 5, Hostos's C-G-A-G transposed) | `D2 q, A2 q, C3 q, A2 q` | Mid-game; gives harmonic colour while staying inside the set |

- **Bordun pitches are a subset of the buttons.** F-C (buttons 2 and 4) and D-A (buttons 1 and 3) are both perfect 5ths inside the set. Every button note over either bordun is a chord tone or a 6th, 7th or 9th colour, never a semitone clash (my arithmetic: over F, D is the 6th; over D, C is the minor 7th and F the minor 3rd).
- **Switching bordun switches the centre.** The same call, `D4 e, F4 e, A4 q, F4 q, D4 q`, ends "at home" over a D-A bordun but sounds like it ends on the 6th, unresolved, over F-C. Use this deliberately: an enemy weakened by the child can have its bordun shift from D to F (minor to major) under the *same* call, a clean "you've tamed it" signal. Do not shift the bordun *during* a call the child must reproduce, because it changes the perceived ending.
- **Speech ostinati for percussion layers** (untested; check word stress in Italian and English):
  - "co-li-BRÌ" (colibrì, hummingbird) or English "kan-ga-ROO" as `e, e, q` = ti-ti ta, with the stress on the long note. Avoid words like "pa-TA-ta", whose stress falls on a short note.
  - "ba-NA-na" (the same stress in Italian and English) as `e, q, e` = syn-co-pa, with the long middle note on the stressed syllable. A natural mnemonic for syncopation (section 10).
  - "tar-ta-RU-ga" (tartaruga, turtle) as `e, e, q, q`.
  - "coc-co-DRIL-lo" (coccodrillo, crocodile) as `e, e, q, q`.
  Layer order in a battle, following Orff practice: the speech or body-percussion ostinato first, then the bordun, then the call.

### Gaps
- The two bordun definitions come from a teacher blog and a community-college guide. The AOSA handbook, the authoritative source, returned 403. Before quoting these as Orff canon, check the AOSA PDF or Frazee, *Discovering Orff* (1987).
- The teaching order (chord, broken, level, crossover) is the blog's implied sequence, not a documented curriculum.
- *Music for Children* itself was not opened; its actual first exercises (often said to be so-mi calls and rhymes) are unverified.

---

## 9. Writing in pentatonic without sounding aimless

### Takeaway
Pentatonic material has no leading tone and no semitone, so nothing in the pitches pulls toward a tonic. The centre has to be made by **duration, metric position, repetition and the bass**. Perception research says differences in note *duration* are what make a tonal hierarchy perceptible. Pedagogy says to begin and end on the chosen tonic. On this set, two centres are available, F (do, major) and D (la, minor). A call sounds aimless when all four notes get equal time and no note is clearly "home".

### Cited Findings
- Smith & Schmuckler (2004): "the perception of tonality depended on a minimal degree of differentiation in the distribution of the duration—but not frequency of occurrence—of pitches"; "the mere differentiation of the tonic from nontonic pitches was not sufficient to induce tonal percepts" (the distribution had to be hierarchically organised). A later study found duration weighted more heavily than frequency of occurrence. **(search extract only; abstract and secondary citations)** — [Smith & Schmuckler 2004, *JEP: HPP* 30(2) (catalogue record)](https://bvbvf1.bib-bvb.de/vufind/PrimoRecord/cdi_proquest_miscellaneous_71791320)
- Orff improvisation guidance: to play major pentatonic, use C D E G A "beginning and ending on C"; for minor, "keep the same five pitches and begin and end on A". — [SUNY *Music and the Child*, ch. 4 (Lumen)](https://courses.lumenlearning.com/suny-music-and-the-child/chapter/chapter-4-approaches-to-music-education-2/)
- Relative major and minor pentatonic "contain the same notes. The difference is in which note is functioning as the root." — [Berklee PULSE lesson (PDF)](https://pulse.berklee.edu/content/public/pub_unit_assets/lessons/lesson2_banded/Exploring%20Form.pdf)
- Lendvai's analysis of Bartók and Hungarian folk music describes a plagal pentatonic cadence (descending do-la minor 3rd and la-mi 4th) as the basic pentatonic close, a mirror of the classical so-do cadence. The page's solfa conventions are hard to map onto Western do-based solfa. **(low confidence; the scanned text is garbled and the author/title are not shown on the page)** — [Lendvai, ch. 4 (VisMath mirror)](https://www.mi.sanu.ac.rs/vismath/lends/ch4.htm)
- The bordun "enforces a non-functional harmony" (the prior notes), so the bass sets the centre in Orff-style pieces. — [Wikipedia, "Orff Schulwerk"](https://en.wikipedia.org/wiki/Orff_Schulwerk) (cited in the prior notes)
- Consonant pentatonic notes "always sound pleasant" together, and the only advice one introductory guide gives for structure is to pick a form such as ABA. That shows how little pentatonic guides say about direction. — [MyMusicTheory, "Writing a pentatonic composition"](https://mymusictheory.com/composition/writing-a-pentatonic-composition/)

### Inferences
Rules for anchoring the centre (mine, derived from the above):

1. **Decide the centre per song, and put it in the bass.** F centre: bordun F-C. D centre: bordun D-A. Never leave the centre to the melody alone.
2. **The tonic gets the longest note and the strongest final beat.** End on F4 (or D4) as a half note or longer, on beat 1 or 3. Example (F): `A4 e, C5 e, A4 q, F4 h`. Example (D): `F4 e, A4 e, F4 q, D4 h`.
3. **Begin on the tonic or its 5th.** The F-centre calls start on F4 or C5. The D-centre calls start on D4 or A4. The first and last notes together frame the key.
4. **Repeat the tonic inside the call** at least once besides the ending, ideally on a strong beat: `F4 q, A4 q, F4 q, C5 q | A4 q, F4 h.` (2 3 2 4 | 3 2).
5. **Open versus closed endings.** In F: open on C5 (so) or A4 (mi), closed on F4. In D: open on A4 (the 5th) or C5 (the minor 7th, very open), closed on D4. A call that will be *answered* ends open; a call that will be *echoed* ends closed (an echo is a complete thought).
6. **Cadence gestures available without a leading tone** (my menu):
   - mi-do, falling major 3rd: `A4 q, F4 h` (F centre; the softest close).
   - so-do, falling perfect 5th: `C5 q, F4 h` (F centre; the strongest, bugle-like).
   - do-la, falling minor 3rd: `F4 q, D4 h` (D centre; the minor close).
   - mi-la, falling perfect 5th: `A4 q, D4 h` (D centre; the strongest minor close; it is the "dominant-to-tonic" of the D-centred set).
   - The descending run: `C5 e, A4 e, F4 e, D4 e` into a held D4 or F4 (step declination plus closure).
7. **Cures for aimlessness:** fewer distinct pitches per call (2 to 3 is usually enough; Berklee's restricted-pitch exercise), a rhythmic hook repeated under the pitches, a single clear peak (the arch), and an ending on a held tonic. A call using all four notes in equal quarters with no repeats, such as `D4 q, A4 q, F4 q, C5 q`, is the aimless archetype: no repeated note, no long note, an ambiguous ending.
8. **Mood from the same notes.** F centre: bright, "friend" or "victory". D centre: minor, "enemy". Same buttons, so no new learning, only a new bass. This is the OoT recolouring idea from the prior notes, done with two centres instead of modes.

### Gaps
- No game composer's advice specifically on making pentatonic calls feel directed was found. Kondo's statements retrieved are about rhythm and interactivity, not pentatonic melody.
- The Smith & Schmuckler result was taken from an abstract-level summary. Its applicability to four-note sets with a drone is my extrapolation.
- No retrieved source compares how listeners perceive la-final and do-final endings in pentatonic melodies.

---

## 10. Rhythm vocabulary: cells, catchiness, syncopation and the tresillo

### Takeaway
The Kodály rhythm syllables give a ready vocabulary: ta (quarter), ti-ti (two eighths), ta-a (half), rest, tam-ti (dotted quarter plus eighth), syn-co-pa (eighth, quarter, eighth), and later tika-tika and tim-ka. On-beat ("strong-meter") rhythms with simple 1:2 ratios are easier to perceive, remember and reproduce. People recode syncopated rhythms toward on-beat forms when they recall them, so syncopation in a call the child must *echo* raises the difficulty sharply and should be introduced late and one at a time. The tresillo (3+3+2 eighths) is a maximally even, globally familiar later-stage hook. It is best introduced in the backing before it appears in calls.

### Cited Findings
- Kodály/Chevé-derived syllables: quarter "ta", paired eighths "ti ti", rests "(no sound)", half "ta-ah or too", whole "ta-a-a-ah", sixteenths "tiri tiri or tika tika", eighth-quarter-eighth "syn-co-pa", dotted quarter plus eighth "tam-ti", dotted eighth plus sixteenth "tim-ka". — [Dynamic Music Room, "Kodály counting system"](https://dynamicmusicroom.com/kodaly-counting-system/)
- Rhythm syllables "are taught sequentially," starting from basic note values. — [SUNY *Music and the Child*, ch. 4 (Lumen)](https://courses.lumenlearning.com/suny-music-and-the-child/chapter/chapter-4-approaches-to-music-education-2/)
- A Wichita State Kodály sequence lists "ti-tam", "Ti Rest" and "Tam-ti" among early-level items; one teacher introduces ta and ti-ti in 1st grade. **(search extract only; the PDF returned 404)** — [Wichita State Kodály sequence](https://wichita.edu/academics/fine_arts/music/kodaly/documents/G_2009_Wichita_A_KODALY_SEQUENCE_FINAL.pdf)
- "Anacrusis" is a tagged rhythm element in the Kodály Center collection; for example, the K-2 song "Do, Do, Pity My Case" includes one. — [Kodály Center, song 481](https://kodalycollection.org/song.cfm?id=481)
- "Sequences organized around a beat are easier to perceive and reproduce." Strong-meter rhythms have small integer ratios (1:2, 1:3, 1:4). Weak-meter rhythms "are encoded as a chain of independent, unrelated values and ... are cognitively demanding to maintain in working memory". "When instructed to recall a weak-meter or syncopated rhythm, normal individuals recode it toward a strong-meter or less syncopated form", a process "automatic and obligatory". "Strong meters are easier than weak meters to discriminate, remember, and reproduce." This is a literature review inside a clinical paper. — [Hopyan, Schellenberg & Dennis 2009, *J. Int. Neuropsych. Soc.* 15:521-528](https://sites.utm.utoronto.ca/sites/sites.utm.utoronto.ca.glenn_website/files/download/HopyanSchellenbergDennis2009.pdf)
- The Euclidean rhythm E(3,8) = [x . . x . . x .] (inter-onset intervals 3-3-2) "is none other than one of the most famous on the planet. In Cuba it goes by the name of the tresillo and in the USA is often called the Habanera rhythm". It is in the bass of "Hound Dog", on the atoke bell in the Ewe Sohu dance, and forms the first bar of the son clave. Euclidean rhythms distribute onsets "as evenly as possible". — [Toussaint, "The Euclidean Algorithm Generates Traditional Musical Rhythms" (BRIDGES 2005)](https://cgm.cs.mcgill.ca/~godfried/publications/banff.pdf)
- Children's verse lines are four beats, and short lines end with a rendered empty beat (section 6). — [Hayes handout on Burling 1966](https://www.brucehayes.org/251HayesSchuhMetrics/handouts/12QuatrainI.pdf)

### Inferences
**Cell inventory, with a proposed introduction order** (my ordering, which follows the general Kodály and Gordon direction from the prior notes; the stage boundaries are untested):

| Stage | Cell | Syllables | Durations | Example on the set |
|---|---|---|---|---|
| 1 | Quarter | ta | `q` | `F4 q, A4 q, F4 q, rq` |
| 1 | Eighth pair | ti-ti | `e e` | `A4 e, A4 e, F4 q` |
| 1 | Quarter rest | (silent) | `rq` | ends every stage-1 call on beat 4 |
| 1 | Half | ta-a | `h` | `C5 q, A4 q, F4 h` |
| 2 | Dotted quarter + eighth | tam-ti | `q. e` | `F4 q., A4 e, C5 h` |
| 2 | Eighth + dotted quarter | ti-tam | `e q.` | `F4 e, A4 q., F4 h` (an accent on the off-beat long note; harder) |
| 2 | Single-eighth anacrusis | (pickup) | `e |` | `C5 e | F4 q, A4 q, F4 h` |
| 3 | Syncopa | syn-co-pa | `e q e` | `F4 e, A4 q, F4 e, D4 h` ("ba-NA-na") |
| 3 | Displaced cell | — | starts on an off-beat | `re, F4 e, A4 e, C5 e, F4 h` |
| 4 | Tresillo | 3+3+2 | `q. q. q` | `D4 q., D4 q., F4 q` or `F4 q., A4 q., C5 q` |
| 4+ | Sixteenths | tika-tika | `s s s s` | only at slow tempos (sixteenths at 100 BPM are 150 ms apart, my arithmetic) |

Rules for catchy rhythm:
- **Four beats per call line; three beats of notes plus a rest is the default.** This follows Burling's "4 3" lines and gives the child a silent beat to prepare. Two-bar calls use a 4 + (3 + rest) shape.
- **End on a strong beat with a long note.** Beat 1 or 3, half note or longer, on the tonic (section 9). The ending is where memory and closure meet (Boltz & Jones; section 5).
- **One rhythm idea per call, repeated.** The repeated rhythm is the hook; pitches can change under it (section 3, "rhythm first").
- **Anacrusis:** a pickup gives forward momentum ("and-ONE"), but in an echo game it adds a second hard problem, because the child has to start *before* the bar. Use calls that start on the downbeat until stage 2. When pickups arrive, make the pickup the 5th falling or rising to the tonic on the downbeat (`C5 e | F4 q`), the most expected two-note move.
- **Syncopation: introduce it in the backing first, then in calls.** Because recall recodes syncopation toward on-beat forms, the judging window for a syncopated call needs to be at least as forgiving as for on-beat calls (inference). A child who "straightens" a syncopa is showing a normal memory effect, not failing to listen. A good early-stage game response is "nice, now make the middle note *longer*" rather than a miss.
- **Tresillo staging:** levels A to C have the tresillo only in the percussion or bordun (`F2 q., C3 q., F2 q`). Level D has an enemy call on a repeated pitch in tresillo (`A4 q., A4 q., A4 q`, a pure rhythm test). Level E adds pitch (`D4 q., F4 q., A4 q`). Its 3-3-2 grouping against the steady 4-beat bordun is the hook. Because it is maximally even, it is the most "on-beat-like" of the syncopated rhythms, so it is a good first syncopation for the call itself, plausibly easier than the syncopa (untested).
- **The beat must never stop under the call** (already a game rule). Syncopation is only perceptible against a clear pulse, so the bordun should keep hitting beats 1 to 4 even when the call is off-beat.

### Gaps
- No study was found on children's ability to *reproduce* specific syncopated cells (syncopa, tresillo) by age, or on anacrusis. The Hopyan review is about adults and a clinical sample, and the strong/weak-meter work is by Povel & Essens (1985), which I did not open.
- Kodály sequences differ between programmes. No authoritative grade-by-grade sequence was retrieved (the Wichita State PDF was a 404).
- The claim that the tresillo is easier to echo than the syncopa is my conjecture.

---

## 11. Synthesis: a checklist for one EchoQuest call (all inference)

1. **Length:** one four-beat line (ages 5 to 7: 3 to 5 notes) or two lines (8 and up: up to about 8 to 11 notes in three chunks).
2. **Shape:** a sentence (idea, idea, continuation to the tonic) for echo calls; a period's antecedent (ending open) for answer calls.
3. **Pitch:** mostly neighbouring buttons and repeated notes. At most one skip for the youngest. Reverse after a skip toward the middle (F4/A4). Default contour is an arch or a descent.
4. **One surprise:** a non-reversing leap, a delayed tonic, a displaced cell, or a syncopation. Only one.
5. **Centre:** the bordun sets F or D. The tonic is the longest note, on a strong beat, at the end. Start on the tonic or its 5th.
6. **Rhythm:** a single repeated rhythm cell from the current stage; the call starts on the downbeat (until stage 2); it ends with a held note or a rest on beat 4.
7. **Hook first:** the enemy's identity is in the first 2 to 4 notes. The tail may be shared between enemies (a rhyme).
8. **Development across rounds, not within:** X X → X X' → sequence → fragmentation and liquidation.
9. **Mix:** the call is a prominent, voice-like lead in D4 to C5. The bordun stays an octave or more below. The beat never stops.
10. **Avoid:** four equal notes using all four buttons with no repetition (aimless); four or more notes in one ascending direction at early ages; borduns or ostinati in the D4 to C5 octave.

---

## Recommended further reading

1. **Open Music Theory, 2nd ed. (Gotham et al.), free online textbook.** The chapters "The Phrase Archetypes and Unique Forms" (sentence, period, hybrids) and the motive and melody chapters are the most practical free introduction to the techniques in sections 3 and 4. [LibreTexts edition](https://human.libretexts.org/Bookshelves/Music/Music_Theory/Open_Music_Theory_2e_(Gotham_et_al.)/03%3A_Form/3.02%3A_The_Phrase_Archetypes_and_Unique_Forms); the original is at viva.pressbooks.pub/openmusictheory, which blocked automated access.
2. **Open Music Theory v1: "Classical theme functions", "The sentence", "Period".** Short pages with the clearest one-line definitions of fragmentation, liquidation and sequence. [themeFunctions](https://hybrid-pedagogy.github.io/openmusictheory/themeFunctions.html)
3. **David Huron, *Sweet Anticipation: Music and the Psychology of Expectation* (MIT Press, 2006).** The source for ITPRA, the prediction effect and the five melodic regularities. Chapters 5 to 7 matter most for melody writing. [MIT Press page](https://mitpressbookstore.mit.edu/book/9780262582780); read it with the [Pearce & Müllensiefen review](https://www.marcus-pearce.com/assets/papers/huron06-review.pdf) for a critical summary.
4. **Elizabeth Hellmuth Margulis, *On Repeat: How Music Plays the Mind* (OUP, 2014).** Why repetition makes music feel musical and participatory. The [Aeon essay](https://aeon.co/essays/why-repetition-can-turn-almost-anything-into-music) is a 15-minute version.
5. **Schellenberg, Adachi, Purdy & McKinnon (2002), "Expectancy in melody: Tests of children and adults."** The key paper for what 5- to 11-year-olds expect next in a melody. [Author PDF](https://sites.utm.utoronto.ca/sites/sites.utm.utoronto.ca.glenn_website/files/download/JEPGeneral.pdf)
6. **Janssen, Burgoyne & Honing (2017), "Predicting variation of folk songs: a corpus analysis study on the memorability of melodies."** Open access; which phrase features survive oral transmission. [Frontiers](https://www.frontiersin.org/journals/psychology/articles/10.3389/fpsyg.2017.00621/full)
7. **Van Balen et al. (2015), "Corpus analysis tools for computational hook discovery," and Burgoyne et al. (2013), "Hooked".** Data on what makes song sections recognisable. [ISMIR 2015 PDF](https://archives.ismir.net/ismir2015/paper/000148.pdf); [ISMIR 2013 PDF](https://archives.ismir.net/ismir2013/paper/000101.pdf) (not read in this session).
8. **Godfried Toussaint, "The Euclidean Algorithm Generates Traditional Musical Rhythms" (2005), and his book *The Geometry of Musical Rhythm* (CRC Press).** The tresillo, clave and maximally even rhythms; a recipe generator for backing timelines. [Paper PDF](https://cgm.cs.mcgill.ca/~godfried/publications/banff.pdf) (book not opened).
9. **Natalie Sarrazin, *Music and the Child* (SUNY Open Textbooks), ch. 4 "Approaches to Music Education".** A free summary of the Orff (bordun, ostinato, layering) and Kodály (tone sets, syllables) practices. [Lumen edition](https://courses.lumenlearning.com/suny-music-and-the-child/chapter/chapter-4-approaches-to-music-education-2/)
10. **Kodály Center, American Folk Song Collection.** A searchable database of children's songs tagged by tone set, rhythm set, range and game type. Search it for the tone set "l, d, m, s" to find real songs on EchoQuest's exact solfa set. [kodalycollection.org](https://kodalycollection.org/song.cfm?id=481)
11. **Orff and Keetman, *Music for Children*, vol. 1: Pentatonic (Schott; Murray English edition).** The primary repertoire of speech pieces, borduns and ostinati over pentatonic melody. [Schott listing](https://www.schott-music.com/en/music-for-children-noc35165.html) (not opened; a purchase, not free).
12. **Arnold Schoenberg, *Fundamentals of Musical Composition* (Faber, 1967), and William Caplin, *Analyzing Classical Form* (OUP, 2013).** The original treatments of motive, sentence, period and liquidation. Dense but the definitive sources for sections 3 and 4 (neither opened in this session).
13. **Hutchinson, *Music Theory for the 21st-Century Classroom*, "Melodic Alteration".** Free textbook; precise definitions of inversion, augmentation, extension and retrograde. [Puget Sound](https://musictheory.pugetsound.edu/mt21c/MelodicAlteration.html)
14. **Berklee PULSE, "Exploring Form Using Minor and Major Pentatonic Scales".** A two-page rhythm-first, restricted-pitch pentatonic composing method that translates directly into a call-writing workflow. [PDF](https://pulse.berklee.edu/content/public/pub_unit_assets/lessons/lesson2_banded/Exploring%20Form.pdf)
15. **Leonard Bernstein, *The Unanswered Question*, lecture 1 "Musical Phonology" (Harvard, 1973; filmed).** The famous (contested) argument for the so-mi(-la) children's chant as a universal. Watch it for inspiration, not as evidence. (Not watched; no link verified in this session.)
