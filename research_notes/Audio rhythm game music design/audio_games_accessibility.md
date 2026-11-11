# Audio-only games, auditory display design and haptics for blind players (EchoQuest)

Reading notes for the report writer:

- **[primary]** = I opened the page and the figure is on it. **[summary]** = the figure comes from a search-result digest or a secondary review page; the primary full text was NOT opened in this session and the number should be checked before being quoted as fact. **[anecdotal]** = forum post, devlog, vendor or press text.
- Almost all quantitative evidence found is from **adults**. Each item says so. Evidence from blind children specifically is thin and is flagged wherever it exists.
- Several primary sources could not be opened (HTTP 403 or binary PDF): the Utrecht BongoBeats thesis, the ACM "Feel the Rhythm" page, the Finger Dance poster. Their details are therefore limited to abstracts.
- Many of the named titles and authors in the brief returned nothing usable; they are listed under Gaps rather than filled from memory.

## 1. Existing audio games and accessible rhythm games: what worked

### Takeaway
The closest precedents to EchoQuest are research prototypes (Blind Hero, Finger Dance, Feel the Rhythm, BongoBeats, a non-visual Simon game), not commercial hits; they consistently show that non-visual rhythm play is learnable and enjoyed but markedly less accurate than visual play, and that players shift from following cues to playing from memory within about half an hour. Only two sources found involve blind children in a rhythm or motion game (BongoBeats, VI-Tennis), and neither gave detailed numbers in what I could access.

### Cited Findings
- **Blind Hero** (Yuan and Folmer, ASSETS 2008): Guitar Hero made playable through a glove with pager motors on the fingertips of one hand, one motor per button, 4 of the 5 buttons (the fifth dropped because of thumb placement). Built on the open-source clone Frets on Fire. Motors: 7 mm diameter, 12.5 mm long, 1.3 V DC, 70 mA. [summary, secondary review page] — [review of Blind Hero](https://a11y-paradise.onrender.com/reviews/69dc0c11502ad1a83c7ff006); paper DOI [10.1145/1414471.1414503](https://www.doi.org/10.1145/1414471.1414503)
- Blind Hero results: sighted players of the original visual game reached 60 to 90% accuracy; the Blind Hero (haptic) groups ranged 15 to 50%. Blind participants did better than blindfolded sighted newcomers. All groups improved across eight plays of a song. All participants rated it fun. Sample sizes were not on the page I read. [summary; adults] — [review of Blind Hero](https://a11y-paradise.onrender.com/reviews/69dc0c11502ad1a83c7ff006)
- Blind Hero design compromises that matter for EchoQuest: because pager motors take time to spin up and reaction to touch is slower than to vision, the glove **started vibrating about a quarter of a second before the note was due**; **sustained (long) notes were removed** because duration was hard to judge by touch. [summary] — [review of Blind Hero](https://a11y-paradise.onrender.com/reviews/69dc0c11502ad1a83c7ff006)
- Blind Hero learning curve: a "learning zone" (first minutes, adapting to the glove), a "memory zone" (players recognise musical patterns and play from memory rather than from the cues), and a "stabilising zone" where performance plateaus after about 30 minutes. [summary] — [review of Blind Hero](https://a11y-paradise.onrender.com/reviews/69dc0c11502ad1a83c7ff006)
- Blind Hero cost: the two prototype gloves were reported at 1,500 USD each. [summary, press] — [Wired Game|Life episode listing](https://rosetta.to/u/wired/game-life-episode-19)
- **Rock Vibe** extended the Blind Hero idea to Rock Band drumming with five inputs. [summary] — [review of Rock Vibe](https://a11y-paradise.onrender.com/reviews/69db911889f909175db44aef)
- **Finger Dance** (University of Oregon, about 2007): an audio-only rhythm-action game designed from scratch for sound, explicitly rejecting "sensory substitution" of an existing visual game. Early tests with sighted participants showed a learning curve similar to other rhythm-action games. Prototypes were announced on the audiogames.net forum. [summary; adults; I could not read the poster text] — [review of Finger Dance](https://a11y-paradise.onrender.com/reviews/69e1038385fefedddb62b3ad); [poster PDF](https://www.cs.uoregon.edu/Activities/Poster_Contest/2007/AaronPoster.pdf)
- **Feel the Rhythm** (Matt Mrazik, UMass Dartmouth master's thesis, 2022): a rhythm game using only audio and controller vibration, stated goal "blind accessibility without sacrificing core gameplay". Prototype with two songs and a tutorial; requires an Xbox controller and "headphones or dual speakers" (so stereo output is needed). The itch page gives no cue-mapping details or playtest results. [primary for these facts] — [itch.io page](https://mrazikmj.itch.io/feel-the-rhythm); [thesis record](https://repository.lib.umassd.edu/esploro/outputs/graduate/Feel-the-rhythm-a-blind-accessible-rhythm/9914424803801301)
- **BongoBeats: Tap with me** (Utrecht University master's thesis): an inclusive multiplayer rhythm game for visually impaired children. Reported result: the children "generally enjoyed" it and "collaborated very well, in teams of two". Recommendations: "provide enough and discrete audio feedback" and offer a variety of levels and songs or genres. Participant count and ages not available to me. [summary of abstract; **children with visual impairment**] — [thesis record](https://studenttheses.uu.nl/handle/20.500.12932/43216); follow-up thesis on player experience: [record](https://studenttheses.uu.nl/handle/20.500.12932/48232)
- **Non-visual Simon game** (Kuber, Tretter and Murphy, INTERACT 2011): sequences presented with speech, non-speech audio and/or haptic cues, to be replicated in order. Users replicated more sequences when haptics were combined with other modalities than with haptics alone. Authors recommend speech feedback alongside haptic cues when the input device is unfamiliar. Not children. [summary of abstract; adults] — [record](https://mdsoar.org/handle/11603/19922)
- **Blindfold Simon / Follow.Me** (Blindfold Games, iOS): commercial audio Simon-style game; each completed sequence earns coins and the next sequence is one step longer. [anecdotal, user comments] — [AppleVis comment thread](https://applevis.com/comment/44095)
- **Blindfold Games** (Marty Schultz): started with Blindfold Racer in 2014 with no prior accessibility knowledge; the line grew on demand from blind players. Schultz's stated conclusion is that **sound and physicality** are the two most important design priorities. [anecdotal, developer account] — [AFB AccessWorld](https://afb.org/aw/16/12/15494)
- Teachers reported that children progressed on a skill while playing without realising they were practising it; over 500,000 downloads reported in 2019. [anecdotal] — [NFB Future Reflections](https://nfb.org/images/nfb/publications/fr/fr38/1/fr380112.htm)
- **ObjectiveEd**: Schultz's education product; games intended to be playable by blind, low-vision and sighted children together. [press] — [TechCrunch](https://techcrunch.com/2019/05/16/objectiveed-is-building-a-better-digital-curriculum-for-vision-impaired-kids)
- **Rhythm Doctor**: a March 2026 Audiogames Digest entry covers an accessibility mod for the Rhythm Doctor level editor (keyboard-only interface with screen reader support). A third-party site claims screen reader support and voice cues in a beta "accessibility mode"; its "fully accessible" wording is unverified. [anecdotal] — [Audiogames Digest March 2026](https://buttondown.com/audiogames_digest/archive/audiogamesnet-monthly-digest-march-2026/); [third-party page](https://inviocean.com/?p=5163)
- **Blind End Dance Hall**: a Windows game listed as blind-accessible in which the player repeats drum patterns with the arrow keys (the same call-and-response loop EchoQuest intends). [primary listing, no evaluation] — [itch.io page](https://magesinstitute.itch.io/blind-end-dance-hall)
- **Papa Sangre** (Somethin' Else): first-person navigation by sound only, binaural audio, headphones required. The developer reported that in playtests **some blind players breezed through while other players found even the opening areas very difficult** (large individual spread). [press interview] — [Gamezebo summary of Ars Technica interview](https://www.gamezebo.com/news/ars-technica-talks-to-papa-sangre-creator-about-developing-games-for-the-blind/); critical reading: [Sounding Out!](https://soundstudiesblog.com/2013/10/28/papa-sangre-immersion-and-audio-games/)
- **The Vale: Shadow of the Crown** (Falling Squirrel): developed in consultation with the Canadian National Institute for the Blind; reviewers praise the sound direction and 3D soundscapes; one mainstream review called it "so-so". Developer David Evans named the main playtesting challenge as making it work both for sighted players and for the diversity within the blind and low-vision community. [reviews and press; attribution of the developer quote is from a search digest] — [RPGFan review](https://www.rpgfan.com/review/the-vale-shadow-of-the-crown/); [Upside Down Shark review](https://upsidedownshark.com/video-games/video-game-reviews/the-vale-shadow-of-the-crown-review/); [Escapist review](https://www.escapistmagazine.com/the-vale-shadow-of-the-crown-review-in-3-minutes-a-so-so-audio-based-adventure/); [Steam page](https://store.steampowered.com/app/989790/The_Vale_Shadow_of_the_Crown/)
- **Blind Drive** (Lo-Fi People): I found only a pointer to a sound-designer interview about communicating gameplay "through clear, logical design"; no specifics retrieved. — [A Sound Effect](https://www.asoundeffect.com/audio-only-video-games/)
- A usability study of ten mobile audio games rated only three "good"; the main shortfalls were **no audio tutorial** and **no level/difficulty configuration**. [summary] — [Springer chapter](https://link.springer.com/chapter/10.1007/978-3-319-58703-5_18)

### Inferences
- Expect a large accuracy gap versus sighted rhythm games (Blind Hero: 15 to 50% versus 60 to 90%). Timing windows and scoring should be much more forgiving than mainstream rhythm games, and success criteria should be tuned on blind children, not copied.
- The "memory zone" finding supports EchoQuest's call-and-response design: players naturally move from reacting to cues to reproducing remembered patterns, which is exactly the intended loop.
- Avoid sustained notes at first, or make duration irrelevant to scoring; duration was the first thing Blind Hero cut.
- Wide individual spread (Papa Sangre, The Vale) plus the audio-game usability study argue for adjustable difficulty and a replayable spoken tutorial from day one.
- Two thesis recommendations for children map directly onto EchoQuest: discrete, unambiguous feedback sounds, and song variety.

### Gaps
- No usable information retrieved on: The Nightjar, A Blind Legend (only a Vice link surfaced, not read), BlindSide, Audio Defence, Frequency Missing, Alt-Frequencies, Lost and Hound, Sequence Storm, Pitch Black, Beat Saber accessibility mods, Rhythm Heaven played blind, Rhythm Rage, DDR clones for the blind, Sonokids Ballyland, AudioGame Hub, Ear Monsters.
- No reviews by Ross Minor, SightlessKombat or Brandon Cole were retrieved; no audiogames.net forum threads on rhythm games were read directly.
- No developer postmortem of The Vale, Blind Drive or Papa Sangre was found with concrete design rules.
- No published playtest with blind children of an audio rhythm game with numbers (sample, ages, accuracy). The BongoBeats theses are the best lead; their PDFs returned HTTP 403.
- Blind Hero sample sizes and the exact wording of the accuracy figures should be checked against the ACM paper.

## 2. Accessibility guidelines and research on games for blind players

### Takeaway
Guidelines converge on a few rules: every important object or event needs its own clearly distinct sound, stereo carries useful information but a mono option should exist, and a spoken tutorial that can be replayed at any time is essential. Research systems (RAD, VI-Tennis, Blind Hero) show that a small number of well-designed cues that let the player plan ahead works better than a system that acts for the player.

### Cited Findings
- Game Accessibility Guidelines has a guideline titled "Ensure sound / music choices for each key objects / events are distinct from each other", noting that some games become playable with little or no vision if key sounds are all distinct. [title and digest only] — [guideline page](https://gameaccessibilityguidelines.com/ensure-sound-music-choices-for-each-key-objects-events-are-distinct-from-each-other/)
- Xbox Accessibility Guidelines list the same idea ("use distinct sounds / music design for all objects and events") under audio accessibility. — [XAG 105](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/105); [XAG 103](https://devdocs.xbox.com/gaming/accessibility/xbox-accessibility-guidelines/103)
- Microsoft training material: represent key information through multiple sensory channels; cues should each have their own distinct sound. — [Microsoft Learn](https://learn.microsoft.com/en-us/training/modules/games-and-platforms1/02-key-information)
- RNIB developer guidance: audio cues must be easy to tell apart and still fit the game's atmosphere; stereo audio "can provide a lot of information to blind gamers". — [RNIB Design for Every Gamer](https://www.rnib.org.uk/living-with-sight-loss/assistive-aids-and-technology/everyday-tech/design-for-every-gamer/)
- AFB: games should let players choose mono instead of stereo. — [AFB AccessWorld](https://afb.org/aw/winter2024/accessibility-gaming-deaf-blind)
- Can I Play That: the tutorial should be playable at any time as a reminder. — [Can I Play That guide](https://caniplaythat.com/basic-accessibility-options-for-blind-and-low-vision-players/)
- Audiokinetic blind-accessibility guide: identify the mechanics the player uses most and design their audio first; build "accessibility dialogue" into the game by default so the same lines work for sighted and blind players. — [Audiokinetic](https://www.audiokinetic.com/en/introduction-to-blind-accessibility)
- **RAD, Racing Auditory Display** (Smith and Nayar, CHI 2018): two sonifications only. (1) A "sound slider": the engine sound moves left or right as the car is more at risk of hitting that track edge, and steering moves the sound back to centre, condensing position, heading, speed, track width and upcoming turns into one measure. (2) A turn indicator: **four beeps** at four equally spaced distance markers before a turn; the beep rhythm lets players anticipate when the turn starts, and each turn's number is spoken so the track can be learned. [primary project page; adults] — [RAD project page](https://www.cs.columbia.edu/~brian/projects/rad.html)
- RAD result: a blind participant raced about as well as casual sighted players, with "similar lap times and driving paths", and drove more smoothly than with a driver-assistance system. The page gives no sample size or lap-time numbers. Design stance: give players information to make their own plan rather than telling them what to do. Players preferred the RAD to the blind-accessible racing game Mach 1. [primary page and press] — [RAD project page](https://www.cs.columbia.edu/~brian/projects/rad.html); [TechCrunch](https://techcrunch.com/2018/03/08/rad-is-a-new-system-to-help-the-visually-impaired-play-racing-games/)
- **VI-Tennis** (Folmer, Morelli and colleagues, VI Fit project): Wii Sports tennis adapted with audio and vibrotactile cues that say when to serve and when to return. Tested with **13 children who were blind**; active energy expenditure was high enough to be considered healthy. [press release; **blind children**] — [University of Nevada, Reno](https://www.unr.edu/nevada-today/news/2010/video-game-accessibility-project-to-help-blind-children-exercise)
- **VI-Bowling**: vibrotactile feedback lets the player find the direction in which to throw; evaluated with six adults; energy expenditure comparable to walking. [press release; adults] — [EurekAlert](https://www.eurekalert.org/news-releases/890956)
- **TiM project** (Archambault, EU-funded): developed and adapted computer games for blind children and for partially sighted children who need specific display settings; first step was describing user needs. Only abstracts were retrieved. — [ICCHP 2004 overview](https://doi.org/10.1007/978-3-540-27817-7_38); [2002 paper](https://link.springer.com/chapter/10.1007/3-540-45491-8_38); survey: [Computer Games and Visually Impaired People](https://cedric.cnam.fr/fichiers/RC1204.pdf)
- A later co-design project built mini-games for children aged 3 to 8 with visual impairment and ran a usability pilot (results not retrieved). — [record](https://ouci.dntb.gov.ua/works/4Yd5aRV7)
- A 2023 DiGRA paper describes co-designing an audio-only game with sight-loss charities, using stereo audio on generic low-end hardware and introducing concepts gradually over the first levels. — [Lancaster eprint](https://eprints.lancs.ac.uk/224966/1/Digra_2023.pdf)

### Inferences
- The RAD's four-beep turn indicator is a directly reusable pattern: a short, evenly spaced count-in whose rhythm itself predicts the moment of action.
- The RAD's "inform, do not automate" stance and Blind Hero's "memory zone" both suggest cues should fade as the child learns a pattern, rather than prompting every press forever.
- "Distinct sound per event" plus "multiple sensory channels" supports redundant coding of the four buttons (see section 3).

### Gaps
- AbleGamers APX patterns were not retrieved.
- No work by Thomas Westin was retrieved.
- "Kevin Allain's audio rhythm game for blind players" could not be confirmed. The only "Allain, 2015" hit is listed under navigation games in a review, not rhythm games. Treat the attribution in the brief as unverified.
- TiM evaluation results (what blind children could and could not do) were not accessible.
- VI-Tennis paper numbers (audio-only versus audio plus haptic comparison) were not retrieved; a slide deck exists: [VI-Tennis slides](https://de.slideshare.net/eelkefolmer/vitennis-a-vibrotactileaudio-exergame-for-players-who-are-visually-impaired).

## 3. Telling the player WHICH button with sound alone

### Takeaway
Abstract musical cues (earcons) are the hardest kind of sound to learn, while speech, sped-up speech (spearcons), sung labels (lyricons) and meaningful real-world sounds (auditory icons) are learned much faster; so four buttons should not be distinguished by pitch alone. Young children do not reliably map pitch to space, and their left/right hearing over headphones is close to but not quite adult-like before about 8 to 10, which argues for redundant coding (timbre or character plus pitch plus pan plus a spoken or sung name during learning).

### Cited Findings
- Earcons were "much more difficult to learn" than spearcons, and spearcons were as learnable as speech, in a study of sound cues for environmental features. [summary of abstract; adults] — [Georgia Tech repository](https://repository.gatech.edu/entities/publication/8cf42420-1aaf-4a59-9ccb-7c7458346a9d)
- ICAD 2018 comparison of music, earcons, spearcons and lyricons: lyricons (sung or musical speech) and spearcons were learned and recalled best; having a meaning related to the referent was an important factor for every sound type. [summary of abstract; adults] — [Virginia Tech repository](https://vtechworks.lib.vt.edu/items/b228d90f-5d4c-4d8b-ad4b-0cfab60e4c90)
- Auditory icons were matched to their targets faster and more accurately than earcons. [summary; adults] — [Georgia Tech repository](https://repository.gatech.edu/entities/publication/e30c1603-3a99-49df-8dd0-d43e63c69e12)
- Spearcons improved menu navigation efficiency more than text-to-speech or no sound in a study with 127 undergraduates. [summary; adults] — [Michigan Tech record](https://digitalcommons.mtu.edu/michigantech-p/1088)
- Foundational earcon evaluation (Brewster, Wright and Edwards, CHI 1993) is available here; I did not extract its numbers. — [CHI 93 paper PDF](https://www.dcs.gla.ac.uk/~stephen/papers/CHI93.PDF)
- Pitch-to-space mapping in children is unreliable: one study found 5-year-old Dutch children understood height-pitch metaphors in a **reversed** fashion (high pitch = low) and showed no consistent space-pitch association in non-linguistic tasks. [summary; sighted children] — [eScholarship paper](https://escholarship.org/content/qt78j7948x/qt78j7948x.pdf?t=sgiixx)
- Other work finds that children and adults both map pitch continuously onto space, but the effect is stronger in older than in younger children, and that **verbal labels helped** 3 to 6-year-olds make the mapping in most conditions. [summary; sighted children] — [Starr et al., 2017](https://lcdlab.berkeley.edu/wp-content/uploads/2019/04/Starr_SpaceOddity_CDS_2017.pdf); [eScholarship item](https://escholarship.org/uc/item/1v08v2x9)
- In adults, the left/right pitch association (low = left, high = right, the SMARC effect) with horizontally arranged keys appears reliably only in musicians when pitch is not the task focus. [summary; adults] — [PubMed: Rusconi et al.](https://pubmed.ncbi.nlm.nih.gov/15925355/)
- Left/right hearing in children: headphone lateralization based on interaural time difference was measured in 49 children aged 4 to 9 and 10 adults; children were slightly but significantly worse than adults, with no age trend inside 4 to 9. In free-field localization with loudspeakers, 5-year-olds were not significantly different from adults; 4-year-olds had larger errors. [summary; sighted children] — [PubMed 19194296](https://pubmed.ncbi.nlm.nih.gov/19194296/)
- Minimum audible angle is about 4 to 6 degrees at 18 months and adult-like (1 to 2 degrees) by 5 years. By 8 to 10 years, just-noticeable differences for interaural level and envelope time differences do not differ from adults, but individual variability is large and decision strategies are immature. [summary; sighted children] — [Frontiers in Systems Neuroscience 2020](https://www.frontiersin.org/articles/10.3389/fnsys.2020.00039/pdf)
- Non-visual Simon study: speech feedback is recommended alongside haptic cues when the device is unfamiliar; multimodal cues beat haptic-only. [summary; adults] — [record](https://mdsoar.org/handle/11603/19922)
- Headphone safety for children: the WHO standard sets 80 dB(A) for adults and **75 dB(A) for "sensitive users" such as children**, both for 40 hours per week; "85 dB" limits on children's headphones are a marketing convention and frequently not met (Wirecutter: 15 of 30 child headphones did not actually limit volume; one analysis measured peaks of 93 dB). — [PMC6928536](https://pmc.ncbi.nlm.nih.gov/articles/PMC6928536); [third-party analysis](https://www.sleepphones.com/sites/default/files/AnalysisOfHeadphonesAndChildrensSafety-WSL-20180822.pdf)

### Inferences
- Do not rely on "low note = left button, high note = right button" as the only code for 5 to 7-year-olds. Use it as one redundant layer.
- A strong design consistent with the evidence: each button is a **character or instrument with a meaningful, nameable sound** (auditory-icon or lyricon style, for example four creatures or four drums with distinct timbre), fixed pitch register, fixed stereo position, and a spoken or sung name during learning that fades later. This stacks the three best-learned cue types and keeps pitch and pan as extra help.
- Hard left/right panning between two groups is likely reliable from age 5; four positions across the stereo field (far left, mid left, mid right, far right) is a finer discrimination and is not supported by any child data I found. Pair pan with the physical layout (left pair of buttons, right pair).
- If stereo is used as a cue, the game needs a start-of-session "which ear?" check, since reversed headphones silently invert the mapping, and a mono fallback in which timbre and pitch alone still work (AFB guideline).
- Speakers avoid headphone volume risk and let a helper adult hear the game; headphones give cleaner left/right. Whichever is chosen, design so that timbre carries identity and pan is redundant.

### Gaps
- No study of earcon, auditory icon or spearcon learnability in **children**, blind or sighted, was found. All learnability figures are adult.
- No data found on how many distinct sound categories young children can learn and retain, or on retention across days.
- No data found on whether blind children have superior auditory memory span or pitch discrimination compared with sighted peers (searched; nothing relevant returned).
- No data on whether children aged 5 to 12 tolerate headphones for a play session.
- No direct test of stereo panning (as opposed to psychoacoustic lateralization tasks) as a button code in children.
- Brewster's earcon design guidelines (timbre as the most salient differentiator, pitch alone unreliable, intervals between earcons) are well known but were not opened in this session; the CHI 93 PDF above is the place to verify.

## 4. Telling the player WHEN (tempo, count-ins, turn-taking, stream separation)

### Takeaway
Children under about 6 synchronise poorly and tap best near their own spontaneous tempo (inter-tap interval around 400 to 500 ms, roughly 120 to 150 beats per minute); adaptation to an external tempo becomes evident from 5 and is near adult level at 6 to 7. A fixed-length count-in of evenly spaced beeps is a proven way to let blind players predict the moment of action.

### Cited Findings
- Spontaneous tapping interval in children aged 2 to 7 is about 400 to 500 ms and slows through childhood to about 600 ms in early adulthood. [summary; sighted children; exact source among the two links] — [Frontiers in Psychology 2014](https://www.frontiersin.org/journals/psychology/articles/10.3389/fpsyg.2014.01346/full); [PMC4256987](https://pmc.ncbi.nlm.nih.gov/articles/PMC4256987)
- In a large study of 3 to 11-year-olds, the youngest tapped at about 2 Hz regardless of the music's tempo; increasing adaptation to tempo was evident from 5 years; synchronisation accuracy improved most between 3 and 7. [summary; sighted children] — [Scientific Reports 2023](https://www.nature.com/articles/s41598-023-29367-5.pdf)
- A lifespan dataset found 4 and 5-year-olds did not synchronise well, if at all, whereas 6 and 7-year-olds performed almost at adult level. [summary; sighted children] — [Psychonomic Bulletin and Review](https://link.springer.com/article/10.3758/s13423-012-0371-2)
- Children under 6 succeed best near their spontaneous motor tempo, cited as about 150 to 200 bpm, which drops around ages 6 to 8. [summary; note this is faster than the 400 to 500 ms figure above, so the sources are not fully consistent] — [Scientific Reports 2023](https://www.nature.com/articles/s41598-023-29367-5.pdf)
- Ages 5 to 8: tapping regularity and the ability to slow down improve with age; age differences are larger when continuing a beat without the pacing sound than when synchronising with it. Five-year-olds in one study never produced intervals longer than about 1.5 s. [summary; sighted children] — [Scientific Reports 2023 review context](https://www.nature.com/articles/s41598-023-29367-5.pdf); [Frontiers 2014](https://www.frontiersin.org/journals/psychology/articles/10.3389/fpsyg.2014.01346/full)
- RAD turn indicator: four evenly spaced beeps before the event; players used the rhythm of the beeps to anticipate the exact moment and even to act slightly early by choice. [primary; adults] — [RAD project page](https://www.cs.columbia.edu/~brian/projects/rad.html)
- Blind Hero: cues were sent about 250 ms early to compensate for slow perception and actuation in the haptic channel. [summary] — [review of Blind Hero](https://a11y-paradise.onrender.com/reviews/69dc0c11502ad1a83c7ff006)
- VI-Tennis used audio and vibrotactile cues specifically to mark "when to serve and when to return", and blind children could play it. [press; blind children] — [University of Nevada, Reno](https://www.unr.edu/nevada-today/news/2010/video-game-accessibility-project-to-help-blind-children-exercise)

### Inferences
- Tempo: start near 120 bpm (500 ms beat) for the youngest and avoid slow tempi; slow is harder, not easier, for 5 to 6-year-olds (they cannot hold long intervals and struggle to slow down). Offer tempo as a difficulty setting rather than lowering it by default.
- Keep the backing beat playing during the child's turn: continuation without a pacing signal is where young children are weakest.
- For 5-year-olds, score order (which buttons) generously and timing loosely or not at all; treat timing as a skill introduced from about 6 to 7.
- Use a fixed four-beat count-in or a consistent "your turn" sound on the beat before the response window, always the same length, so its rhythm predicts the start.
- Stream separation (my inference from general auditory scene analysis principles, not from a source read this session): keep the cue melody in a different register and timbre from the backing track, pan or centre it distinctly, duck the backing during the cue phrase, and never play cue notes under narration.

### Gaps
- No source was retrieved on auditory stream segregation (Bregman) with numbers for register or timing separation; no source on speech masking or ducking levels. The rules above are inference only.
- No study of turn-taking earcons or count-in length in audio games.
- No tapping or synchronisation data for **blind** children; all developmental data are from sighted children.
- The two tempo figures for young children (400 to 500 ms intervals versus 150 to 200 bpm) come from different digests and should be reconciled against the primary papers.

## 5. Tutorials and onboarding without visuals; physical controller

### Takeaway
The best-supported rules are: provide a spoken tutorial that can be replayed at any time, introduce concepts gradually over the first levels, pair speech with other cues when the device is unfamiliar, and allow difficulty configuration. I found no empirical source on tactile button differentiation for blind children.

### Cited Findings
- In a usability study of ten mobile audio games, the absence of an audio tutorial and of level configuration were the main reasons games scored poorly; only three were rated good. [summary] — [Springer chapter](https://link.springer.com/chapter/10.1007/978-3-319-58703-5_18)
- The tutorial should be replayable at any time as a reminder. — [Can I Play That guide](https://caniplaythat.com/basic-accessibility-options-for-blind-and-low-vision-players/)
- Speech-based feedback is recommended alongside haptic cues when users meet an unfamiliar input device. [adults] — [Kuber et al. record](https://mdsoar.org/handle/11603/19922)
- Audio-only game co-designed with sight-loss charities introduces concepts across the first few levels. — [Lancaster eprint](https://eprints.lancs.ac.uk/224966/1/Digra_2023.pdf)
- Feel the Rhythm ships a tutorial alongside its two songs. — [itch.io page](https://mrazikmj.itch.io/feel-the-rhythm)
- Blind Hero players needed the first minutes just to adapt to the device before performance climbed ("learning zone"). [summary] — [review of Blind Hero](https://a11y-paradise.onrender.com/reviews/69dc0c11502ad1a83c7ff006)
- Schultz (Blindfold Games) names physicality, alongside sound, as a top design priority for blind players. [anecdotal] — [AFB AccessWorld](https://afb.org/aw/16/12/15494)
- In a study where visually impaired children defined their own gestures for a wearable device, they preferred buttons and touch gestures; only one device-motion gesture (shaking) emerged, tentatively. [summary; **visually impaired children**] — [paper DOI](https://doi.org/10.1145%2F3490149.3501327)
- The Vale's developers consulted the blind community on language, including whether words like "see" and "look" are used in conversation. [press, via digest] — [Steam page for the game](https://store.steampowered.com/app/989790/The_Vale_Shadow_of_the_Crown/)

### Inferences
- Start with a no-fail free-play mode where each button immediately plays its sound and (at first) its spoken name, before any sequence is asked for. This matches the device-adaptation phase and the speech-plus-cue recommendation.
- Let the narrator character re-explain on request (a dedicated gesture or a long press), since replayable tutorials are a recurring demand.
- Give each button a distinct tactile identity and a layout that matches the audio layout (left pair and right pair, or a row ordered low to high); this is a reasoned recommendation, not a sourced one.

### Gaps
- No empirical source found on tactile differentiation of buttons (shape, texture, spacing, diamond versus row) for blind children, or on controller ergonomics for small hands.
- No description retrieved of how specific audio games (A Blind Legend, The Vale, Blind Drive) structure their tutorials.
- No source on narrator design or instruction length for children aged 5 to 12.

## 6. Haptics

### Takeaway
Eccentric rotating mass (ERM) motors need roughly 50 to 100 ms or more to start and to stop, so they cannot render sharp rhythm at game tempi and must be triggered early; rhythm is nonetheless the most reliably recognised vibration parameter (93% in adult tests). Vibration works best as a redundant layer with sound, not as the only cue.

### Cited Findings
- ERM start (time to 90% of target) and stop times are on the order of 50 to 100 ms. [vendor technical article] — [Power Electronic Tips](https://www.powerelectronictips.com/haptics-components-pt-1-lra-erm-and-piezo-actuators/)
- Coin ERM makers quote start and stop times from 80 ms to over 120 ms for typical 0720 or 1020 coin motors. [vendor press release, anecdotal] — [Leader Motor release](https://www.einpresswire.com/article/920162383?promo=4400)
- Linear resonant actuator (LRA) start-up time is quoted inconsistently: 40 to 60 ms in one Texas Instruments document, about 10 ms in another, under 20 to 30 ms on a vendor page. An unbraked LRA can take up to 300 ms to stop; an out-of-phase braking signal shortens this. — [TI SLYT483](https://ti.com/lit/pdf/slyt483); [TI SSZTAP9](https://ti.com/lit/pdf/ssztap9); [Boreas comparison](https://pages.boreas.ca/blog/piezo-haptics/haptic-actuators-how-lra-and-erm-stack-up-with-piezo-actuators)
- Piezo actuators respond on the order of a millisecond. [vendor] — [Boreas comparison](https://pages.boreas.ca/blog/piezo-haptics/haptic-actuators-how-lra-and-erm-stack-up-with-piezo-actuators)
- Tactons (Brown, Brewster and Purchase): with three rhythms and three "roughness" values, overall recognition was 71%; **rhythm was recognised 93%** of the time and roughness 80%. [summary; adults; high-quality voice-coil actuator, not an ERM] — [Tacton paper PDF](https://core.ac.uk/download/1393758.pdf); [follow-up PDF](https://core.ac.uk/download/1393792.pdf)
- General vibrotactile temporal resolution: people can distinguish pulses separated by as little as 8 to 12 ms, less well at low intensity. [summary; adults; laboratory actuators] — [McGill WHC 2019 paper](https://srl.mcgill.ca/publications/2019-WHC-Numeric.pdf)
- Blind Hero: pager (ERM) motors needed about 250 ms of lead time; duration of vibration was hard to judge so long notes were removed; accuracy with haptic-only cueing was 15 to 50%. [summary; adults] — [review of Blind Hero](https://a11y-paradise.onrender.com/reviews/69dc0c11502ad1a83c7ff006)
- Non-visual Simon: haptic-only sequences were replicated less well than haptic combined with speech or non-speech audio. [summary; adults] — [record](https://mdsoar.org/handle/11603/19922)
- VI-Tennis: vibrotactile plus audio cues for timing worked for 13 blind children; the team noted the Wii remote's limited ability to provide tactile feedback and listed "how to encode motion instructions through touch" as an open question. [press and slides; blind children] — [University of Nevada, Reno](https://www.unr.edu/nevada-today/news/2010/video-game-accessibility-project-to-help-blind-children-exercise); [VI-Tennis slides](https://de.slideshare.net/eelkefolmer/vitennis-a-vibrotactileaudio-exergame-for-players-who-are-visually-impaired)
- VI-Bowling: vibrotactile feedback successfully guided players to a throwing direction (a continuous "find the target" use of vibration). [press; adults] — [EurekAlert](https://www.eurekalert.org/news-releases/890956)

### Inferences
- If EchoQuest's motors are ERMs, the shortest clean event is on the order of 100 to 150 ms on plus a similar gap, so vibration can mark beats at 120 bpm (500 ms) but cannot articulate fast subdivisions. Measure the real motors' rise and fall and pre-trigger by that amount.
- With two motors, the safest roles are (a) left versus right as a redundant button-group cue and (b) confirmation of a correct or wrong press; using vibration as the sole indicator of which of four buttons to press is not supported (Blind Hero needed one motor per finger and still reached only 15 to 50%).
- Encode meaning in rhythm and in which motor, not in intensity or duration: rhythm was the best-recognised parameter and duration the worst-judged.
- Keep feedback vibration short and distinct from any cue vibration so the two never overlap in time.
- The project rule that haptics stop on disconnect is consistent with keeping vibration strictly event-bound.

### Gaps
- No source found on a haptic metronome's effect on timing accuracy, or on whether vibration distracts children.
- No source on vibrotactile pattern design or perception thresholds specifically for children.
- No measured minimum pulse and gap for ERM motors in a handheld controller; the 8 to 12 ms figure is for laboratory actuators and does not transfer to ERMs.
- LRA start-time figures conflict between sources; the motor type in the EchoQuest controller is not stated in the brief.
- Tacton figures should be verified in the PDFs; I did not open them.

## 7. Latency and timing constraints

### Takeaway
The radio link is not the problem (7.5 ms connection interval), but the host can be: one report measured about 80 ms average latency for BLE HID on Windows with a 7.5 ms interval. Bluetooth headphones add roughly 110 to 250 ms and should be excluded or calibrated. Non-musician adults only notice a key-to-sound delay at around 180 ms on average, but timing discrimination is as fine as 10 to 50 ms, so immediate button sounds are feasible on wired audio with a small buffer, and a tap-along calibration by ear is the standard remedy for the rest.

### Cited Findings
- The HID-over-GATT profile allows connection intervals from 7.5 to 50 ms; 7.5 ms gives the fastest response; input reports are sent as notifications that can only go out at a connection event. — [TI SWRA715](https://www.ti.com/document-viewer/lit/html/SWRA715/GUID-3E3206EB-BACF-4F54-AEFE-0C98C5B25389)
- A developer measured about **80 ms average latency** for a BLE HID mouse on Windows despite a confirmed 7.5 ms interval, attributed to how Windows handles HID over BLE, not to the radio; the same hardware against a non-Windows central measured about 1.5 ms. A high-end commercial BLE gaming mouse behaved similarly. [anecdotal, single forum report] — [Nordic DevZone thread](https://devzone.nordicsemi.com/f/nordic-q-a/119571/unusually-high-ble-connection-latency/526410)
- Nordic's nRF Desktop reference design has a dedicated module for managing BLE connection latency for HID devices. — [Nordic documentation](https://nrfconnectdocs.nordicsemi.com/ncs/latest/nrf/applications/nrf_desktop/doc/ble_latency.html)
- Bluetooth Core 6.2 introduces shorter connection intervals below the former 7.5 ms minimum; benefit depends on host support. [trade press] — [WIoT article](https://wiot-group.com/think/en/news/bluetooth-sci-reduces-le-latency/)
- Bluetooth headphone latency (RTings, SBC codec): Sony WH-1000XM5 241 ms; Razer Barracuda Pro 113 ms; Plantronics BackBeat Pro 2: wired 7 ms, SBC 173 ms, aptX 166 ms. AAC latency varies widely by operating system and could not be measured reliably. — [RTings latency article](https://www.rtings.com/headphones/learn/latency); [RTings codec article](https://www.rtings.com/headphones/learn/sbc-aptx-which-bluetooth-codec-is-the-best)
- Conflicting measurement for the same headset from another lab: 248 ms SBC, 34 ms aptX Low Latency. — [SoundStage Solo](https://www.soundstagesolo.com/index.php/features/185-latency-a-new-concern-for-audiophiles)
- Unity: default DSP buffer described as 1024 samples; developers report "Best Latency" fixes rhythm-game timing but can distort audio on Windows (a Unity issue closed as will-not-fix); "Good Latency" is cited as a middle ground. Song position should be driven from `AudioSettings.dspTime`, not frame time. [anecdotal, forums] — [Unity Discussions](https://discussions.unity.com/t/rhythm-game-dsp-buffer-file-best-latency-or-best-performance/651238); [Unity issue tracker](https://issuetracker.unity3d.com/issues/audio-gets-distorted-when-dsp-buffer-size-is-changed-to-best-latency-in-play-mode); [interpolating playback time](https://itch.io/blog/744936/the-right-way-to-interpolate-the-audio-playback-time-unity)
- Calibration (Rhythm Quest devlog): tapping along to an audio signal measures **audio latency plus input latency** together, which is exactly the offset an audio-only game needs. A typical tap test uses about 10 taps and discards the minimum and maximum. **Taps should not trigger sounds during calibration**, because those sounds are themselves delayed and throw the player off. Calibration should be quick. [primary devlog; anecdotal expertise] — [Rhythm Quest devlog 10](https://rhythmquestgame.com/devlog/10.html)
- Rhythm Doctor uses a two-phase calibration, with a tap test that mirrors actual gameplay ("seventh beat"); another game uses a test scene simulating gameplay for immediate feedback on the calibration. [primary devlog] — [Rhythm Quest devlog 10](https://rhythmquestgame.com/devlog/10.html)
- Key-press-to-sound delay detection threshold: **180 ± 104 ms in non-musicians**, 102 ± 65 ms in musicians (pianists and brass players alike). [summary; adults] — [van Vugt and Tillmann, PMC3911931](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC3911931/)
- Just-noticeable difference in audio latency: mean 49 ms at 0 ms base latency, 27 ms at 64 ms base, 77 ms at 512 ms base; strongly task-dependent. [summary; adults] — [JND Audio Latency paper](https://epub.uni-regensburg.de/59003/1/JND_Audio_Latency.pdf)
- Musicians can perceive latency differences as small as about 10 ms in some tasks; 30 ms and above was reliably perceived in a theremin task. [summary; adult musicians] — [Jack et al., action-sound latency](https://qmro.qmul.ac.uk/xmlui/bitstream/handle/123456789/44614/Jack%20Action-sound%20Latency%202018%20Accepted.pdf?sequence=1)
- Ensemble playing: synchronisation is best with 8 to 25 ms delay and deteriorates significantly at 55 to 66 ms. Delayed auditory feedback disrupts tapping most when the delay is about equal to the tapping interval. [summary; adult musicians] — [Frontiers in Psychology 2021](https://www.frontiersin.org/articles/10.3389/fpsyg.2021.707090/pdf)

### Inferences
- Rough budget for an immediate button sound on wired audio: BLE up to one connection interval (7.5 to 15 ms) plus host stack (small on macOS/Linux by the one report, possibly tens of ms on Windows) plus Unity mixing (1024 samples at 48 kHz is about 21 ms per buffer; 256 samples is about 5 ms) plus device output. On a good path this stays under about 50 ms, well below the 180 ms at which non-musician adults notice, so **playing the instrument sound immediately on press is reasonable on wired audio**. The arithmetic for buffer durations is mine.
- With Bluetooth headphones (110 to 250 ms) the immediate sound would arrive a quarter to half a beat late at 120 bpm and approaches the regime where delayed feedback disrupts tapping. Recommend wired headphones or speakers; if Bluetooth audio is detected or calibration shows a large offset, consider suppressing or simplifying the press sound and judging timing against the compensated clock.
- Because the device radio is already Bluetooth, a Bluetooth headset on the same computer also shares the 2.4 GHz adapter; this is a further practical reason to prefer wired audio (inference, not sourced).
- Judge hits against `dspTime` minus a calibrated offset; timestamp presses as early as possible. Measure the Windows BLE HID path specifically before fixing timing windows, since the 80 ms report, if typical, also implies jitter.
- A non-visual calibration fits the story: "tap along with the drum eight to ten times", silent taps, drop the extremes, average. It can be re-run inside gameplay-like conditions, as Rhythm Doctor does. An adult helper could do it once per setup.
- Timing windows for children should be wide relative to these numbers (child variability dominates), so residual latency of 20 to 30 ms is tolerable once the mean offset is calibrated out.

### Gaps
- No controlled end-to-end measurement of BLE HID gamepad latency and jitter across Windows, macOS and Linux; the 80 ms Windows figure is a single forum report.
- No authoritative Unity documentation figures were retrieved for output latency per DSP buffer setting per platform.
- No data on delay-detection thresholds in children.
- No source found for a rhythm game that performs calibration with no visual element at all for blind players specifically.

## 8. Motion input for blind children

### Takeaway
Motion-controlled games are feasible and enjoyed by blind children when timing is cued by sound and vibration (VI-Tennis, 13 children), and vibration can guide an arm to a target direction (VI-Bowling, adults), but visually impaired children asked to invent their own controls preferred buttons and proposed only shaking as a motion gesture.

### Cited Findings
- VI-Tennis: motion-sensing tennis with audio and vibrotactile cues; 13 blind children; healthy levels of active energy expenditure. [press; blind children] — [University of Nevada, Reno](https://www.unr.edu/nevada-today/news/2010/video-game-accessibility-project-to-help-blind-children-exercise)
- VI-Bowling: players found the throwing direction through vibrotactile feedback while sweeping the arm; six adults. [press; adults] — [EurekAlert](https://www.eurekalert.org/news-releases/890956)
- Visually impaired children defining gestures for a wearable preferred buttons and touch gestures, with a single tentative device-motion gesture (shaking). [summary; visually impaired children] — [paper DOI](https://doi.org/10.1145%2F3490149.3501327)
- RAD: continuous closed-loop sonification (sound moves off-centre, player steers to bring it back) let a blind player control a fast continuous variable precisely. [primary; adult] — [RAD project page](https://www.cs.columbia.edu/~brian/projects/rad.html)
- Children's involuntary movements need to be segmented out of inertial sensor data (noted in work with children with cerebral palsy, 50 Hz accelerometer and gyroscope). [summary; different population] — [IIT Madras preprint](https://publications.iitm.ac.in/pdf/preprint-an-inertial-sensor-based-system-to-develop-motor-capacity)

### Inferences
- Prefer coarse, unmistakable motion events (a shake, a big tilt, pointing up versus down) over fine angles, and always give continuous closed-loop sound while the motion is being made (RAD and VI-Bowling pattern: the sound or vibration changes as you approach the target).
- Use motion for story moments or power moves rather than for note-by-note rhythm input; buttons are what the children themselves preferred, and timing a motion precisely is harder to judge and to sense.
- Use thresholds with hysteresis and ignore small unintentional movement.

### Gaps
- No studies found on proprioception or pointing accuracy in blind children, on sonification of tilt (pitch follows tilt) with children, or on fatigue and safety of motion input for this group.
- Morelli and Folmer's sensory-substitution work on whole-body gestures and the "Audio Bracelet" pilot with early-blind children surfaced only as references; findings not retrieved.
- VI-Tennis audio-only versus audio-plus-haptic comparison numbers were not retrieved.

## 9. Spatial audio in Unity, as far as relevant to a music game

### Takeaway
Head-related transfer function (HRTF) spatialisation with a generic head model adds front/back confusion and requires headphones; for separating four sound sources in a music game, plain left/right stereo placement is the more robust choice, and nothing found shows a clear winner among Unity spatialiser plugins.

### Cited Findings
- Mismatched (non-individual) HRTFs cause front/back errors and can make frontal sounds seem inside the head. [technical article] — [Fora Soft](https://www.forasoft.com/learn/audio-for-video/articles-audio/ambisonics-hrtf-binaural-rendering); [VOCAL](https://vocal.com/audio/errors-in-localization-using-non-individualized-head-related-transfer-functions/)
- Head movement helps resolve front/back ambiguity (relevant only with head tracking). — [Meta spatial audio design guide](https://developers.meta.com/horizon/design/spatial_audio/)
- A 2018 comparison of Steam Audio (2.0 beta) and Resonance Audio (1.2.0) found neither clearly better; Steam Audio showed sudden timbre shifts during a 360-degree rotation. Old versions; may no longer hold. [practitioner test] — [Designing Sound](https://designingsound.org/2018/03/29/lets-test-3d-audio-spatialization-plugins/)
- Papa Sangre and similar binaural games state that headphones are required for the effect. — [Sounding Out!](https://soundstudiesblog.com/2013/10/28/papa-sangre-immersion-and-audio-games/)
- Feel the Rhythm requires "headphones or dual speakers", that is, only stereo, not binaural. — [itch.io page](https://mrazikmj.itch.io/feel-the-rhythm)
- Children's left/right discrimination is adult-like or close to it from about 5 (free field) and 8 to 10 (headphone cues). [summary] — [PubMed 19194296](https://pubmed.ncbi.nlm.nih.gov/19194296/); [Frontiers 2020](https://www.frontiersin.org/articles/10.3389/fnsys.2020.00039/pdf)

### Inferences
- For the core battle loop, use ordinary stereo panning (works on speakers and headphones, no front/back ambiguity, no plugin). Reserve HRTF spatialisation for narrative scenes, if at all, and never make front/back or elevation carry gameplay information.
- The controller has no head tracking, so the main remedy for front/back confusion is unavailable.

### Gaps
- No current (2025 to 2026) comparison of Steam Audio, Meta XR Audio, Microsoft Spatializer and Resonance Audio in Unity 6 was retrieved; maintenance status of each was not verified.
- No study on HRTF localisation accuracy in children or blind children was found.
