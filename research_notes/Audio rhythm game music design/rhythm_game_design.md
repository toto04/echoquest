# Game design of rhythm, call-and-response and musical memory games (for an audio-only, 4-button + IMU + vibration game for blind children aged 5 to 12)

Reading notes for the report writer:

- "Snippet only" means the fact came from a search-result summary and the underlying page was not opened; treat as medium confidence.
- "Researcher's conversion" means arithmetic I did on a sourced number (for example frames to ms).
- Anything I remember from general knowledge but could not source in this session is in the Gaps sections, labelled "unverified recollection". Do not present those as cited facts.
- Several target sources were unreachable (Fandom wikis returned HTTP 402, Zelda Wiki and the ACM "Feel the Rhythm" page returned 403, a Finger Dance PDF was unreadable). Coverage of Um Jammer Lammy, Gitaroo Man, Bop It, Donkey Konga, Elite Beat Agents, Rhythm Doctor (own sources), Sayonara Wild Hearts, Wandersong, Metal: Hellsinger, BPM, Everhood, Undertale, Theatrhythm, Melody of Memory, Paper Mario / Mario and Luigi, LeapFrog, Toca Band, Loopimal, Sago Mini and Just Dance Kids is therefore missing or thin, and is listed under Gaps rather than filled from memory.

## 1. Call-and-response and listen-then-repeat games: how the loop is structured

### Takeaway
The proven pattern is a strict alternation of equal-length phrases over a backing track that never stops: a teacher or enemy performs (with voice cues naming the inputs), then the player answers in the same number of beats, and every player input makes a sound. Simon and Loom show the non-metric variant (order only, no timing), and Ocarina-style songs show that short fixed melodies on very few notes are enough to be memorable.

### Cited Findings

PaRappa the Rapper
- The teacher raps a line, then the player presses buttons with the right timing to make PaRappa rap the line back; this is the whole loop. — [Wikipedia: PaRappa the Rapper](https://en.wikipedia.org/wiki/PaRappa_the_Rapper)
- Four coarse rating states only: Awful, Bad, Good, Cool. Staying on beat keeps the player on Good. — [Wikipedia: PaRappa the Rapper](https://en.wikipedia.org/wiki/PaRappa_the_Rapper)
- Hysteresis in judgement: one bad line only makes the lower rank flash as a warning; it takes two bad lines in a row to drop a rank, and two good lines in a row to rise one rank. Rank changes are only evaluated on the first line of each two-line pair. — [Wikipedia: PaRappa the Rapper](https://en.wikipedia.org/wiki/PaRappa_the_Rapper)
- The stage is passed if the player is on Good at the end of the song; it fails only on ending in Bad/Awful or dropping below Awful. — [Wikipedia: PaRappa the Rapper](https://en.wikipedia.org/wiki/PaRappa_the_Rapper)
- Improvisation is the top tier: after clearing the game once, freestyling instead of copying can trigger "Cool" mode, in which the teacher leaves and the player raps freely; two unimpressive freestyles bring the teacher back at Good. — [Wikipedia: PaRappa the Rapper](https://en.wikipedia.org/wiki/PaRappa_the_Rapper)

Space Channel 5
- Ulala copies the opponents' moves and "chu" vocalisations in time with the music; critics compared it to Simon Says. Inputs are 4 directions plus 2 action buttons, and the opponents' vocal calls name the inputs. — [Wikipedia: Space Channel 5](https://en.wikipedia.org/wiki/Space_Channel_5)
- Two feedback economies: dance sections raise a "ratings" meter, boss fights use hearts with one heart lost per mistake. Running out of hearts or ratings means game over and restart. — [Wikipedia: Space Channel 5](https://en.wikipedia.org/wiki/Space_Channel_5)
- Mizuguchi said early builds were "very cool, but not so fun" because the player only pressed buttons to music over non-interactive video; the call-and-response idea came partly from the audience-clapping segment of the troupe Stomp. He also stripped out extra effects early so the team could focus on matching button presses to music and character action, and considered it key to approachability that Ulala never kills anyone. — [Wikipedia: Space Channel 5](https://en.wikipedia.org/wiki/Space_Channel_5)
- Reviewers (EGM, IGN) criticised sync problems between music, gameplay and graphics, and unresponsive controls on the GBA port: timing integrity is the thing players notice. — [Wikipedia: Space Channel 5](https://en.wikipedia.org/wiki/Space_Channel_5)

Patapon (four drums, strict 4 + 4 structure)
- Four face buttons map to four drum syllables: Square = "pata", Circle = "pon", Triangle = "chaka", Cross = "don". A command is a four-beat combination, for example march = pata-pata-pata-pon, attack = pon-pon-pata-pon, defend = chaka-chaka-pata-pon. — snippet only, from reviews such as [Pocket Gamer hands-on](https://www.pocketgamer.com/patapon/hands-on-with-patapon-on-psp/) and [GamesRadar review](https://www.gamesradar.com/patapon-review/)
- The loop is: the player inputs a command for four beats, the Patapons perform (and sing it back) for four beats, the player inputs the next command for four beats, and so on to the end of the stage. — snippet only, [GamesRadar review](https://www.gamesradar.com/patapon-review/) / [Pocket Gamer](https://www.pocketgamer.com/patapon/review/)
- The idea of having the Patapons sing the command back to the player after it is sent came from developer Pyramid. — [Wikipedia: Patapon](https://en.wikipedia.org/wiki/Patapon)
- Fever: reached by a combo of 10 correct commands, or earlier with a combo of 3 or more when the command is played in perfect sync. Fever raises attack and defence bonuses. Stopping, or playing off the rhythm, loses combo and Fever. — [Wikipedia: Patapon](https://en.wikipedia.org/wiki/Patapon)
- Missing a beat or sitting out a measure resets the combo. — snippet only, [Pocket Gamer review](https://www.pocketgamer.com/patapon/review/)
- Kotani's stated aim was a game that could be played either simply or in a complex way, depending on the player. — [Wikipedia: Patapon](https://en.wikipedia.org/wiki/Patapon)

Simon (pure sequence memory, no metre)
- Four buttons, each with its own tone. The tones were designed by Ralph Baer after a bugle so that they are "always harmonic... no matter the sequence": an A major triad in second inversion (E, C sharp, A, and E an octave lower). — [Wikipedia: Simon (game)](https://en.wikipedia.org/wiki/Simon_(game))
- Simon was a reaction to Atari's Touch Me, which Baer judged: "Nice gameplay. Terrible execution. Visually boring. Miserable, rasping sounds." Pleasant, consonant tones were the fix. — [Wikipedia: Simon (game)](https://en.wikipedia.org/wiki/Simon_(game))
- The sequence grows as the game progresses; the game ends on a wrong press or when the response time limit runs out. Sequence caps listed for Simon are 8, 14, 20 and 31 (the article does not say outright that these are the four skill levels). — [Wikipedia: Simon (game)](https://en.wikipedia.org/wiki/Simon_(game))
- Simon Trickster (2005) variants: Classic up to 35 tones; Bounce (colours move); Surprise (all one colour, remember positions); Rewind (repeat the sequence backwards). It gives spoken compliments at 5 and 11 tones and a victory melody at 18 and 35. — [Wikipedia: Simon (game)](https://en.wikipedia.org/wiki/Simon_(game))

Loom (four-note spells)
- Spells ("drafts") are four-note tunes played on a distaff. The player starts with only three notes (C, D, E); more notes unlock as the story progresses, which gates which drafts can be played. — [Wikipedia: Loom](https://en.wikipedia.org/wiki/Loom_(video_game))
- Drafts are learned by observing something in the world that has that quality (watching a blade being sharpened teaches "Sharpening"). — [Wikipedia: Loom](https://en.wikipedia.org/wiki/Loom_(video_game))
- Playing a draft backwards gives the opposite effect (Dye reversed is Bleach); palindromes such as C-E-E-C cannot be reversed. — [Wikipedia: Loom](https://en.wikipedia.org/wiki/Loom_(video_game))
- Many drafts are randomised per playthrough, so the player has to learn them rather than look them up. — [Wikipedia: Loom](https://en.wikipedia.org/wiki/Loom_(video_game))
- Difficulty is expressed as the amount of non-audio help: Standard shows the notes on a scale, Expert shows nothing and must be played by ear. — [Wikipedia: Loom](https://en.wikipedia.org/wiki/Loom_(video_game))

Zelda: Ocarina of Time
- Songs use the five notes an ocarina can produce; the analog stick bends pitch for extra tones; Link learns 12 melodies, which solve music puzzles and teleport him to visited places. — [Wikipedia: Ocarina of Time](https://en.wikipedia.org/wiki/The_Legend_of_Zelda:_Ocarina_of_Time)
- Koji Kondo called writing distinct themes on a five-note scale a "major challenge"; in some areas the background music is a variation of the ocarina tune learned there, so the learned song is heard again as environment music. — [Wikipedia: Ocarina of Time](https://en.wikipedia.org/wiki/The_Legend_of_Zelda:_Ocarina_of_Time)

Rock Band (play-along, for contrast)
- Play-along games trigger the recorded track rather than synthesise the player's note: the Expert drum window is described as "quite a bit more generous than real life", and the trick is called "a sleight of hand that is unbelievably elegant" because players feel they are hearing their own playing. The author scored about 99% in-game on a chart that he could not hold together on a real kit. — [Dubious Quality: Design Brilliance and the Timing Window](https://dubiousquality.blogspot.com/2009/05/design-brilliance-and-timing-window.html)

### Inferences
- A reusable loop template for EchoQuest: N beats "call" (enemy/teacher instrument plus optional spoken or sung syllables), then N beats "response", with N fixed inside an encounter (Patapon uses 4 + 4). The backing rhythm keeps running through both halves so the handover is felt as the next downbeat, not as a pause.
- Give each of the 4 buttons a fixed, nameable syllable or timbre (Patapon's pata/pon/chaka/don, Space Channel 5's "up, down, chu"). Spoken or sung syllables in the call tell a blind child which button without any display, and children can chant them.
- Choose the four button pitches so that any order sounds consonant (Simon's triad). Then even a wrong answer is not ugly, which is itself a non-punishing miss signal.
- Evaluate per phrase, not per note, and require two bad phrases before any penalty (PaRappa's flash-then-drop). This suits children better than per-note fail states.
- Two distinct modes already exist in the canon: metric call-and-response (PaRappa, Space Channel 5, Patapon) and non-metric order recall (Simon, Loom, Ocarina song entry). The second is an obvious easy mode for the youngest players: accept the right order at any tempo.
- Loom's "learn by witnessing" and "reverse to invert" give cheap narrative variety from the same four-note vocabulary (for example an enemy's attack phrase played backwards as the counter).

### Gaps
- Not sourced in this session: exact phrase lengths in beats for PaRappa/Um Jammer Lammy/Space Channel 5, and whether the backing track continues during the player's turn. Unverified recollection: in all three the backing continues uninterrupted, and in PaRappa/Lammy the player's button presses trigger the vocal or guitar samples directly, so bad timing is heard as a mangled line.
- No sources retrieved for Um Jammer Lammy, Gitaroo Man (attack/charge/guard phases), Bop It, Donkey Konga, Taiko's loop, Elite Beat Agents, Rhythm Doctor, Sayonara Wild Hearts, Wandersong.
- Patapon tempo (BPM) and exact per-beat windows were not found. Unverified recollection: around 120 BPM, giving a 2 s command and 2 s response.
- How Ocarina songs are taught (NPC plays, player repeats, then a note-by-note prompt) and the song lengths (6 notes for the short songs, 8 for the warp songs) are unverified recollection; the Zelda Wiki page returned 403. Wikipedia only confirms 5 notes, 12 songs, pitch bend.
- Wind Waker baton details (3, 4 and 6 beat metres chosen by a stick, with a metronome) are unverified recollection; only qualitative reception was found (see section 7).

## 2. Rhythm Heaven / Rhythm Tengoku: cueing, judgement and Tsunku's philosophy

### Takeaway
Rhythm Heaven is the closest mainstream precedent for an eyes-free rhythm game: it has no note display at all, relies mainly on audio cues, lets the player practise each pattern first, judges with three hidden-score ranks, and offers a skip after three failures. Its designers treat sharp, immediate input sounds and "bounce" in every voice cue as non-negotiable, and deliberately make hard stages with fewer notes or slower tempo rather than more notes.

### Cited Findings
- Tsunku proposed to Nintendo in 2004 a rhythm game that did not rely on visual indicators. — [Wikipedia: Rhythm Heaven](https://en.wikipedia.org/wiki/Rhythm_Heaven) (citing Wired, 2009)
- The games "rely mainly on audio cues"; visual cues exist but sometimes mislead on purpose. Most minigames last 1 to 2 minutes. — [Wikipedia: Rhythm Heaven](https://en.wikipedia.org/wiki/Rhythm_Heaven)
- Osawa (chief designer/programmer) on the first game: "Since nothing like a music score appears on the screen, it was a new type of rhythm game, and I thought only certain people would be able to get it." — [Iwata Asks: Rhythm Heaven, p.1](https://iwataasks.nintendo.com/interviews/ds/rhythm-heaven/0/0)
- Structure: sets of 4 to 6 games, i.e. 4 or 5 stages plus a "remix" that combines the set's games under a new song. — [Wikipedia: Rhythm Heaven](https://en.wikipedia.org/wiki/Rhythm_Heaven)
- Practice: most stages let the player practise before the scored attempt; remixes do not. — [Wikipedia: Rhythm Heaven](https://en.wikipedia.org/wiki/Rhythm_Heaven)
- Judgement: three end-of-stage ranks only: Try Again (repeat), OK (pass), Superb (medal, unlocks extras). No running score is shown. — [Wikipedia: Rhythm Heaven](https://en.wikipedia.org/wiki/Rhythm_Heaven)
- Anti-frustration: in most stages, three consecutive "Try Again" results let the player skip the level. — [Wikipedia: Rhythm Heaven](https://en.wikipedia.org/wiki/Rhythm_Heaven)
- Mastery layer kept optional: a "Perfect Campaign" is offered at random on a stage already rated Superb; any miss costs one of three chances. — [Wikipedia: Rhythm Heaven](https://en.wikipedia.org/wiki/Rhythm_Heaven)
- Input sound is treated as the core of game feel. Sound director Yone: "we really put a lot of effort into a means of clearly expressing to players an immediate response so they would think, 'I just pressed a button!' Specifically, we paid attention to making sure the sounds for each sound effect would stand out sharply and clearly." — [Iwata Asks: Rhythm Heaven, p.3](https://iwataasks.nintendo.com/interviews/ds/rhythm-heaven/0/2)
- Sound rescues an imprecise gesture: the DS flick input was only kept once "we realized that if we combined sound with the flicking action, you can get a good sense of the timing". Researching the flick took two to three months and integrating it six months. — [Iwata Asks: Rhythm Heaven, p.3](https://iwataasks.nintendo.com/interviews/ds/rhythm-heaven/0/2); [Wikipedia: Rhythm Heaven](https://en.wikipedia.org/wiki/Rhythm_Heaven)
- Difficulty philosophy. Iwata: music games "have tried to be difficult by ramping up the tempo, increasing the number of notes, and introducing unnatural timing". Tsunku: "In this series, sometimes the difficult games have fewer notes or a slower tempo. It feels good to skilled players. And when less skilled players become able to do it, they feel an incredible sense of achievement." — [Iwata Asks: Rhythm Heaven Fever, p.1](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/0)
- Iwata, summarising Tsunku's complaint about other music games: they are frustrating "because you think, 'Am I supposed to press a button here?'"; Rhythm Heaven feels good "by placing the rhythms where they make sense". — [Iwata Asks: Rhythm Heaven Fever, p.1](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/0)
- Fast improvement is a design target. Tsunku: "once you do it three or four times, you'll definitely improve. That's an important point." He also says the series "took hold among everyone from children to adults" because it can be played for five minutes and rhythm is universal. — [Iwata Asks: Rhythm Heaven Fever, p.1](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/0)
- Motion controls were prototyped and rejected for the Wii game: "getting all excited and swinging the Wii Remote controller around made our hands tired"; the series needs "an overwhelming amount of input from the player per time of gameplay", so the team shipped with buttons only (A, and A+B). 2D art was kept because it responded to presses faster and "snappier" than 3D. — [Iwata Asks: Rhythm Heaven Fever, p.1](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/0)
- Voice cues must swing. Tsunku rejected staff-recorded voice cues with "It doesn't have bounce!" and said: "if even one 'In to you!' doesn't have rhythm, the game would lose what's good about it." All takes were re-recorded. — [Iwata Asks: Rhythm Heaven Fever, p.5](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/4)
- Sound effects were designed to "have an impact and get stuck in your head"; music should "spin around in your head after you play". — [Iwata Asks: Rhythm Heaven Fever, p.2](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/1) and [p.4](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/3)
- Practice and performance sound different: some voices are heard "only ... in the main rhythm game. You can't hear them when you're practicing." — [Iwata Asks: Rhythm Heaven Fever, p.3](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/2)
- Whole-body rhythm. Tsunku: "You don't just play the game with your hands. Rather, it's better to keep the rhythm with your body, with your chest", plus knees and feet; he recommends standing. The development staff took dance lessons at Tsunku's suggestion to share a sense of rhythm. — [Iwata Asks: Rhythm Heaven Fever, p.5](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/4); [Iwata Asks: Rhythm Heaven, p.1](https://iwataasks.nintendo.com/interviews/ds/rhythm-heaven/0/0)
- Remixes are meant to be passable once the four constituent games are learned ("If you do all four of the games up to the remix, you can pick up on it"). — [Iwata Asks: Rhythm Heaven Fever, p.5](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/4)
- Many prototype minigames were cut: "Even if one was well-made, the theme was rhythm, so if it didn't feel right, the whole thing had to go." — [Iwata Asks: Rhythm Heaven, p.5](https://iwataasks.nintendo.com/interviews/ds/rhythm-heaven/0/4)
- Tsunku said the hardest decision was the difficulty criteria: whether to start hard for veterans or let beginners in. — [Iwata Asks: Rhythm Heaven Fever, p.5](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/4)
- "Barely" inputs are a middle state between hit and miss; a trope wiki states they only count as passes in Perfect runs. — snippet only, [TV Tropes: Rhythm Heaven YMMV](https://tvtropes.org/pmwiki/pmwiki.php/YMMV/RhythmHeaven)

### Inferences
- Rhythm Heaven's transferable cueing rules: (1) one minigame teaches one cue-to-action mapping; (2) a distinct cue sound pre-announces each pattern; (3) the pattern is practised with a tutor before the scored song; (4) patterns are recombined later in a "remix", which is a natural model for boss battles that mix earlier enemies' patterns.
- Because the score is hidden and shown once as one of three verdicts, the child hears only the music and character reactions while playing. For EchoQuest the verdict can be narrative (enemy flees, enemy laughs it off, enemy joins the band).
- The skip-after-3-fails rule should be copied directly, expressed as story (a companion steps in).
- Rhythm Heaven's evidence argues against using shaking or swinging as the high-frequency input: Nintendo's own prototype was tiring and less precise. Keep motion for low-frequency, expressive actions (see section 7).
- Every call recorded by voice actors must itself be rhythmically exact ("bounce"), since in an audio-only game the call is the notation.

### Gaps
- The specific claim that designers said the game should be playable "with eyes closed" was not found in the Iwata Asks pages read (DS pages 1 to 5, Fever pages 1 to 5); the nearest sourced statements are Tsunku's "no visual indicators" proposal and Osawa's "nothing like a music score appears on the screen". Fever page 6 and the Megamix "Iwata Asks in writing" were not read.
- Rhythm Heaven's timing windows in frames or ms were not found in a reliable source (a fan site listing roughly ±35 to ±40 ms looked unreliable). The Rhythm Heaven fan wiki returned HTTP 402.
- The count-in conventions (for example "one, two, three, four" voice or cowbell count-ins) and the Skill Star are unverified recollection.

## 3. Rhythm-combat hybrids: framing music as battle

### Takeaway
Successful rhythm-combat games reward on-beat play instead of punishing off-beat play, always make a button press do something audible, and give enemies learnable, beat-locked patterns. Only fragments were sourced on how attack and defence phases differ, so that part rests mostly on inference.

### Cited Findings
- Hi-Fi Rush: off-beat inputs still register and the game syncs the resulting action to the music; players are rewarded for on-beat timing but not penalised for off-beat timing. Director John Johanas: "the worst thing you can do in an action game is to press the button and nothing happens", and "even if you're not good at rhythm games, you can enjoy the experience of being good at rhythm games." — [Tacoma Ledger: The ingenious accessibility of Hi-Fi Rush](https://thetacomaledger.com/2023/02/27/the-ingenious-accessibility-of-hi-fi-rush/)
- Hi-Fi Rush: off-beat attacks still happen but are more powerful when played in rhythm. — snippet only, [Vice interview with Johanas](https://vice.com/en/article/88qyw5/how-a-designer-known-for-horror-made-the-musical-gaming-triumph-hi-fi-rush)
- Hi-Fi Rush: everything in the world (backgrounds, characters, attacks) is synced to the beat, giving redundant beat cues; a later update added an assist that widens the success window in its rhythm-game sections. — [Tacoma Ledger](https://thetacomaledger.com/2023/02/27/the-ingenious-accessibility-of-hi-fi-rush/); assist: snippet only
- Crypt of the NecroDancer: actions only work on the beat; a missed beat mostly just resets the coin multiplier rather than hurting the player. Enemies have fixed, learnable beat patterns (skeletons move every other beat). The player's action takes priority over monster movement. — [Wikipedia: Crypt of the NecroDancer](https://en.wikipedia.org/wiki/Crypt_of_the_NecroDancer)
- NecroDancer: Ryan Clark's goal was "a game that is really hard but you can still improve. If you die, you still know it was your fault." The initial timing window was too strict and caused stress-driven mistakes, so he widened it and added auto-calibration that adapts to whether the player tends to act early or late. — [Wikipedia: Crypt of the NecroDancer](https://en.wikipedia.org/wiki/Crypt_of_the_NecroDancer)
- NecroDancer: tempo rises across zones as the difficulty ramp; the Bard character and a "No Beat" mode (added 2022) remove the rhythm requirement entirely. — [Wikipedia: Crypt of the NecroDancer](https://en.wikipedia.org/wiki/Crypt_of_the_NecroDancer)
- Mother 3 "sound battles": after a normal attack the player can keep pressing A on the beat of the battle music for up to 16 hits; a missed beat ends the combo; extra hits do much less damage each. The music is the enemy's identity, and some songs change tempo unexpectedly to trick the player. — snippet only, [Wikipedia: Mother 3](https://en.wikipedia.org/wiki/Mother_3)
- Mother 3: when an enemy is put to sleep, a heartbeat sound makes the hidden beat audible so the player can learn where to press. — snippet only, [Wikipedia: Mother 3](https://en.wikipedia.org/wiki/Mother_3) / [EarthBound Wiki](https://earthbound.fandom.com/wiki/Enemies)
- Mother 3 windows are per battle theme, stored in 60 fps frames, symmetric around the beat. The default entry gives ±1 frame for "Great" and ±3 frames for "OK"; the system only supports one press per beat (24 sequencer ticks). — [Data Crystal: Mother 3 sound battle timing windows](https://datacrystal.tcrf.net/wiki/Mother_3/Sound_battle_timing_windows). Researcher's conversion: about ±17 ms and ±50 ms.
- Patapon as combat: commands are stances (march, attack, defend) chosen by drum phrase; Fever (combo of 10, or 3 or more with perfect timing) raises attack and defence. — [Wikipedia: Patapon](https://en.wikipedia.org/wiki/Patapon)
- Space Channel 5 frames the two phase types differently: dance sections fill a ratings meter, boss battles use hearts lost per mistake; the second action button rescues hostages instead of shooting. — [Wikipedia: Space Channel 5](https://en.wikipedia.org/wiki/Space_Channel_5)
- Thumper: each obstacle type has its own call-out sound that plays a fraction of a second before the input is due; a successful input plays a sound mirroring the call-out. A "good enough" turn gives pleasant but muted feedback; a perfectly timed one adds a higher-pitched ping and a controller shake. On a hit, the percussion and the player's rhythm drop to silence and a grating machine noise plays, which the author says often causes a second mistake. — [SUPERJUMP: The Helpful, Harmful Sounds of Thumper](https://www.superjumpmagazine.com/the-helpful-harmful-sounds-of-thumper/)
- Thumper's developers say the audio cues are a fundamental part of the soundtrack, not a layer on top of it; Marc Flury describes the aim as "rhythm hell" rather than "rhythm heaven". — snippet only, [Game Developer: Road to the IGF, Thumper](https://gamedeveloper.com/audio/road-to-the-igf-drool-s-i-thumper-i-); [SUPERJUMP](https://www.superjumpmagazine.com/the-helpful-harmful-sounds-of-thumper/)

### Inferences
- Combat framing that fits call-and-response: the enemy's "attack" is the call (a musical phrase in the enemy's own timbre), and the child's echo is the block, counter or spell. Success is rewarded with the party or the enemy musically joining in; failure costs a small, recoverable resource.
- Attack versus defence can differ by who leads. Defence: the enemy calls, the child echoes (memory plus timing). Attack: the child plays a known "owned" song or free on-beat hits (Mother 3 combo, Hi-Fi Rush always-lands hits), which is lower in memory load and higher in agency. Alternating the two also alternates cognitive load.
- Telegraph by sound, as Thumper and Mother 3 do: give each enemy type one signature cue and one signature rhythm, and expose the beat explicitly when the player is struggling (Mother 3's heartbeat is a model for an assist that can be delivered as a vibration pulse).
- Never let a press be silent. Off-beat presses should still sound, quantised or softened, as in Hi-Fi Rush.
- Thumper's failure feedback (music cut plus harsh noise) is documented to induce follow-up errors; for children the opposite should apply: keep the groove running through mistakes.

### Gaps
- No sources retrieved for Metal: Hellsinger, BPM: Bullets Per Minute, Everhood, Undertale, Theatrhythm, Kingdom Hearts: Melody of Memory, Mario and Luigi timed hits, Paper Mario action commands, Wandersong, The Bard's Tale, Gitaroo Man.
- No sourced description of Hi-Fi Rush's enemy parry sequences (unverified recollection: enemies play a rhythmic pattern of attack cues which the player then repeats as parries, a direct call-and-response used as the defence phase).
- Thumper boss structure (unverified recollection: repeating phrases where hitting every note in the phrase fires a shot back at the boss) not sourced.

## 4. Timing judgement: windows, relative versus absolute timing, quantisation, latency

### Takeaway
Sourced windows span from about ±17 ms (Mother 3 "Great") to ±120 to 135 ms (osu!taiko's most lenient setting), and the most forgiving mainstream designs use two tiers plus miss. For EchoQuest the dominant problem is not window size but latency: Bluetooth headphones add roughly 100 to 300 ms and a tap test only measures audio plus input latency combined, so audio must be scheduled on the DSP clock and judged against a calibrated offset, or judged tempo-relatively.

### Cited Findings

Windows
- osu!taiko (half-window, i.e. maximum error either side): GREAT = 50 - 3 x OD ms, so ±50 ms at OD 0, ±35 ms at OD 5, ±20 ms at OD 10. OK = ±120 ms at OD 0, ±80 ms at OD 5, ±50 ms at OD 10. MISS boundary = ±135 ms at OD 0, ±95 ms at OD 5, ±70 ms at OD 10. Two judgement tiers plus miss. — [osu! wiki: osu!taiko judgement](https://osu.ppy.sh/wiki/en/Gameplay/Judgement/osu%21taiko)
- Taiko no Tatsujin (Switch) has an "easier hit timing" option that roughly triples the Good window and enlarges the OK window by about 25%. — snippet only, community FAQ on [r/taikonotatsujin](https://lr.sudovanilla.org/r/taikonotatsujin/comments/mvz94m/frequently_asked_questions_edition_v4)
- Mother 3 default: Great ±1 frame, OK ±3 frames at 60 fps, tuned per song. — [Data Crystal](https://datacrystal.tcrf.net/wiki/Mother_3/Sound_battle_timing_windows)
- StepMania default windows reported as 22.5 ms (W1) up to 180 ms (W5); osu!mania's top window reported as ±22.4 ms shrinking to ±13.9 ms. Both from a secondary blog and not checked against source code. — snippet only, [voxbooster blog](https://voxbooster.com/blog/rhythm-game-audio-latency-calibration-statistics-2026/) (low reliability)
- Guitar Hero and Rock Band use a single hit-or-miss window, with Rock Band's the stricter of the two. — snippet only, [TV Tropes: Rhythm Game](https://www.tvtropes.org/pmwiki/pmwiki.php/Main/RhythmGame). No official ms values were found.
- Clone Hero offers a modifier ("Drunk Mode") that widens the hit window for players who find the game too hard. — snippet only, [Clone Hero wiki: How to Play](https://wiki.clonehero.net/books/clone-hero-manual/page/how-to-play)
- NecroDancer widened its window after playtests because a strict one caused stress errors, and auto-calibrates to a player's early or late bias. — [Wikipedia: Crypt of the NecroDancer](https://en.wikipedia.org/wiki/Crypt_of_the_NecroDancer)

Children's timing ability (bounds how tight a window can be)
- Children's spontaneous tapping interval is reported as roughly fixed around 400 ms between ages 5 and 8 (about 150 taps per minute), with variability decreasing with age. — snippet only, [Frontiers in Psychology 2014 review](https://www.frontiersin.org/articles/10.3389/fpsyg.2014.01346/full)
- A review reports that 4- and 5-year-olds did not synchronise well, if at all, whereas 6- and 7-year-olds performed much better; a study of 600 children aged 3 to 11 found synchronisation improved throughout that range, most between 3 and 7. — snippet only, [Repp review, Psychonomic Bulletin and Review](https://link.springer.com/article/10.3758/s13423-012-0371-2); [Scientific Reports 2024](https://link.springer.com/article/10.1038/s41598-024-69807-4)
- Between ages 6 and 11, tapping variability improves with age, but mean asynchrony does not show a clear developmental effect; the anticipatory "negative asynchrony" typical of adults is rarely found in young children. — snippet only, [Scientific Reports 2023](https://link.springer.com/10.1038/s41598-023-29367-5)
- A study of 64 children aged 5 to 7 used clapping in time at 0.9 Hz (one clap about every 1.1 s) for one minute with no practice. — snippet only, [Bonacina et al. 2018](https://brainvolts.northwestern.edu/wp-content/uploads/boxtrx/Bonacina_et_al-2018-Annals_of_the_New_York_Academy_of_Sciences.pdf)

Quantisation and auto-correction
- Wii Music: the track responds to any player input so that the notes stay consistent with the song even when off the original melody; the player does not choose pitch, only timing and gesture; there is no score and no penalty for "wrong" notes in Jam mode. — [Wikipedia: Wii Music](https://en.wikipedia.org/wiki/Wii_Music)
- Hi-Fi Rush syncs off-beat inputs to the music so that actions land musically regardless of press time. — [Tacoma Ledger](https://thetacomaledger.com/2023/02/27/the-ingenious-accessibility-of-hi-fi-rush/)
- Rock Band triggers the recorded stem inside a generous window, so the player hears a correct performance. — [Dubious Quality](https://dubiousquality.blogspot.com/2009/05/design-brilliance-and-timing-window.html)
- Rhythm Quest pre-schedules its hit sounds so that they play exactly on the beat even with high audio latency, and they play even if the player misses. — [Rhythm Quest devlog 10: Latency calibration](https://rhythmquestgame.com/devlog/10.html)

Latency and calibration
- Three latencies exist: audio (usually largest; buffering, mixing, Bluetooth transmission; Android is especially bad), visual, and input (debouncing, frame-based polling, wireless controller transmission). — [Rhythm Quest devlog 10](https://rhythmquestgame.com/devlog/10.html)
- A tap test to an audio signal measures audio latency plus input latency; it cannot separate them. — [Rhythm Quest devlog 10](https://rhythmquestgame.com/devlog/10.html)
- Rhythm Doctor calibrates in two phases, the second being a tap test that mirrors real gameplay (tap on the seventh beat). Typical tap tests use about 10 taps and discard the minimum and maximum; Rhythm Quest uses 4 and aims to keep calibration quick. — [Rhythm Quest devlog 10](https://rhythmquestgame.com/devlog/10.html)
- During calibration, taps should be silent, because a tap sound would itself be played late and would mislead the player. — [Rhythm Quest devlog 10](https://rhythmquestgame.com/devlog/10.html)
- Bluetooth headphone latency, RTINGS measurements quoted in search results: one headset 7 ms wired, 173 ms SBC, 166 ms aptX, 34 ms aptX Low Latency. Averages attributed to RTINGS: about 260 ms for true-wireless earbuds on SBC and about 170 ms for gaming headsets over Bluetooth. Individual models from 113 ms to 241 ms on SBC. — snippet only, [RTINGS: headphone latency](https://www.rtings.com/headphones/learn/latency) (the averages reached me via a secondary summary; verify before quoting)
- Unity: `AudioSource.PlayScheduled(double time)` starts a clip at an absolute time on the `AudioSettings.dspTime` timeline, independent of frame rate. Unity recommends scheduling "slightly in the future (~100-200ms) so the audio system can prepare playback". If the time is already past, playback starts as soon as possible. The documented pattern uses two AudioSources alternately and advances `nextEventTime` by `60 / bpm * beatsPerSegment`. — [Unity Scripting API: AudioSource.PlayScheduled](https://docs.unity.com/en-us/engine/current/script-reference/unityengine/audiosource/playscheduled)

### Inferences
- Suggested starting windows for 5 to 12 year olds (inference, to be playtested): "perfect" about ±50 to 60 ms and "good" about ±120 to 150 ms, i.e. at or beyond osu!taiko's most lenient setting, widening further for the youngest. With eighth notes at 100 BPM (300 ms apart) a ±150 ms window already tiles the whole timeline, so beyond that point judge order and count rather than time.
- Because children show larger variability and no stable anticipation, and because 4 to 5 year olds may not synchronise at all, the youngest tier should not be judged on absolute synchronisation. Judge relative rhythm instead: compare the child's inter-onset intervals to the model's after normalising for total duration (tempo-invariant), with a tolerance per interval. This also cancels constant latency completely, since a fixed delay shifts all onsets equally.
- Since the response phase in a call-and-response loop starts after a clear handover, the first press of the answer can define the child's own "beat one" (phase-free judging), with later presses judged relative to it. This removes the need for calibration in the default mode.
- Where absolute sync matters (playing along with a backing track), do a one-time, story-framed tap test (for example "clap along with the drum 8 times") using silent taps, discard outliers, take the median as the combined audio plus input offset, and keep adapting it slowly from in-game hit errors as NecroDancer does.
- Two feedback strategies under Bluetooth latency: (a) play the note immediately on press (it will be heard late by the headphone latency, possibly 150 to 250 ms, which sounds like a flam against the backing); (b) pre-schedule the correct note on the grid with PlayScheduled and mute or soften it if no press arrives inside the window (Rock Band/Rhythm Quest style, always sounds musical). For the echo phase with no dense backing, (a) is acceptable; for play-along phases (b) is safer. Vibration from the controller has its own, separate BLE latency and should not be used as a precise timing reference without measuring it.
- Schedule all calls, count-ins and backing loops with `PlayScheduled` at least 100 to 200 ms ahead and compute beat times from `dspTime`, never from frame time.

### Gaps
- Official or datamined windows for Guitar Hero, Rock Band, DDR (arcade), Taiko no Tatsujin (arcade/console, per difficulty), Rhythm Heaven and Beat Saber were not found. Unverified recollection: DDR Marvelous about ±16.7 ms, Perfect ±33 ms, Great ±92 ms, Good ±142 ms; Taiko Good about ±25 ms on Oni/Hard and ±42 ms on Easy/Normal, OK about ±75 ms and ±108 ms; Clone Hero/Guitar Hero-style total window about 140 ms (±70 ms); Rock Band about ±100 ms. Beat Saber has no timing judgement at all (score comes from swing angle and cut accuracy). All of these need verification.
- No source found on judging by inter-onset intervals in commercial games, or on quantisation in Rez, Child of Eden, Lumines, Tetris Effect, Sound Shapes, Ape Out or the Rayman Legends music levels. Unverified recollection: Rez and Child of Eden quantise the sound of player shots to the beat (commonly described as sixteenth-note quantisation) so any input sounds musical.
- No measured figures for BLE HID controller input latency (connection interval dependent) or for Unity's own output latency per platform were retrieved.
- No measured asynchrony or variability values in ms per age band were extracted from the developmental papers (only snippets read).

## 5. Difficulty and progression

### Takeaway
The best-documented levers are sequence length (Simon), number of available notes (Loom's 3 notes growing over the story), tempo (NecroDancer), and rhythmic placement rather than density (Rhythm Heaven). Forgiving structures that are well documented are hysteresis in rank changes, skip after repeated failure, wider-window assist toggles, and modes that remove timing entirely; evidence that automatic dynamic difficulty improves enjoyment is weak.

### Cited Findings
- Simon grows the sequence over the game and caps it by skill level (8, 14, 20, 31; Touch Me handheld 8, 16, 32, 99). Praise is given at fixed milestones (5 and 11 tones in Simon Trickster; victory melody at 18 and 35). — [Wikipedia: Simon (game)](https://en.wikipedia.org/wiki/Simon_(game))
- Loom ramps the note vocabulary rather than speed: 3 notes at the start, more unlocked through the story, with phrase length fixed at 4 notes. — [Wikipedia: Loom](https://en.wikipedia.org/wiki/Loom_(video_game))
- Rhythm Heaven: hard stages sometimes have fewer notes or slower tempo; the designers reject difficulty by tempo, note count and "unnatural timing". — [Iwata Asks: Rhythm Heaven Fever, p.1](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/0)
- Rhythm Heaven: practice before each stage; skip offered after three consecutive failures; mastery challenges are optional and appear only on stages already done well. — [Wikipedia: Rhythm Heaven](https://en.wikipedia.org/wiki/Rhythm_Heaven)
- PaRappa: rank moves only after two consecutive good or bad lines; pass requires only "Good" at the end. — [Wikipedia: PaRappa the Rapper](https://en.wikipedia.org/wiki/PaRappa_the_Rapper)
- NecroDancer: tempo rises zone by zone; Bard character / No Beat mode removes the beat requirement; auto-calibration absorbs personal early/late bias. — [Wikipedia: Crypt of the NecroDancer](https://en.wikipedia.org/wiki/Crypt_of_the_NecroDancer)
- Assist toggles in the wild: Taiko's easier hit timing (Good window about x3), Clone Hero's widened window, Hi-Fi Rush's wider-window rhythm assist. — snippets only, see section 4 sources
- Wii Music's Jam mode has no score and no fail state; reviewers were split on its appeal: IGN thought children might enjoy it more than adults, who could "grow bored of the experience in a matter of hours, if not minutes"; GameSpy thought it suited young children and families; GameSpot thought older children and adults would get only a few hours from it. — [Wikipedia: Wii Music](https://en.wikipedia.org/wiki/Wii_Music)
- Patapon was designed to be playable simply or deeply depending on the player. — [Wikipedia: Patapon](https://en.wikipedia.org/wiki/Patapon)
- Dynamic difficulty adjustment evidence (none of it from rhythm games): a controlled experiment with 221 players comparing DDA, constant and incremental difficulty found that the DDA group performed better but showed no significant difference in enjoyment or flow. — snippet only, [Acta Ludologica 2022](https://actaludologica.com/performance-based-dynamic-difficulty-adjustment-and-player-experience-in-a-2d-digital-game-a-controlled-experiment/)
- An exergame study comparing DDA methods found the richest-data method produced the highest enjoyment, competence and flow, but the enjoyment and flow differences were not significant. — snippet only, [JMIR Serious Games 2021](https://games.jmir.org/2021/2/e25771/XML)
- Children 5 to 7 can be asked to clap in time at about 0.9 Hz, and children's preferred tapping interval is around 400 ms. — snippets only, [Bonacina et al. 2018](https://brainvolts.northwestern.edu/wp-content/uploads/boxtrx/Bonacina_et_al-2018-Annals_of_the_New_York_Academy_of_Sciences.pdf); [Frontiers 2014](https://www.frontiersin.org/articles/10.3389/fpsyg.2014.01346/full)

### Inferences
- Order in which to add difficulty for EchoQuest (inference from the above): (1) number of distinct buttons (1, then 2, then 3, then 4, as Loom gates notes); (2) phrase length (start at 2 to 3 notes, standard 4, long 6 to 8); (3) rhythmic variety (all on the beat, then one long/short pair, then off-beats and rests); (4) tempo last. Rhythm Heaven's point is that rests and off-beats are the real difficulty, so introduce one at a time with its own practice.
- Tempo guidance (inference): a 400 ms preferred interval suggests children are comfortable pressing at up to about 150 presses per minute, so eighth notes at 75 BPM or quarter notes at 100 to 120 BPM (500 to 600 ms apart) are a sensible default; slower than about 1 s between notes makes timing harder to hold, not easier.
- Use coarse, slow-moving adaptation rather than opaque per-note DDA: after two failed phrases shorten or repeat the call (PaRappa hysteresis), after three failed attempts offer a narrative skip or helper (Rhythm Heaven), and store a per-child window multiplier. The DDA literature found suggests the gain is in success rate rather than measured enjoyment, so do not over-engineer it.
- Offer a parent/teacher-settable "free rhythm" mode (order only, no timing) equivalent to NecroDancer's Bard, and keep Wii Music's lesson in mind: no-fail play needs goals (collection, story progress) or older children get bored.

### Gaps
- No rhythm-game-specific DDA study was found.
- No sources retrieved on how LeapFrog, Toca Band, Loopimal, Sago Mini, Taiko's kids/easy modes or Just Dance Kids handle failure. Unverified recollection: Toca Band and Loopimal have no fail state at all and loop quantised patterns so anything the child does stays in time; Just Dance has no fail state, only star ratings; Guitar Hero/Rock Band offer no-fail modes.
- No source on Simon's response time limit in seconds, or on its tempo increase with sequence length.
- No sourced data on children's working-memory span for tone sequences by age, which would set the maximum phrase length. Unverified recollection: digit span rises from roughly 4 items at age 5 to roughly 6 at age 12; this needs a developmental psychology source (probably covered by another researcher).

## 6. Feedback and juice with audio and vibration only

### Takeaway
The strongest documented pattern is diegetic: the player's presses are the music, good timing makes it brighter or adds a layer and a haptic kick, and weak timing sounds muted rather than wrong. Harsh error sounds and stopping the music are documented to make players fail again.

### Cited Findings
- Thumper's tiered feedback: adequate timing gives a pleasant but muted sound; exact timing adds a higher-pitched ping plus controller vibration. Each successful input sounds like the cue that announced it (call and answer in miniature). — [SUPERJUMP](https://www.superjumpmagazine.com/the-helpful-harmful-sounds-of-thumper/)
- Thumper's miss feedback removes the percussion and plays grating machinery noise; the writer reports that this disruption often leads to a second hit soon after. — [SUPERJUMP](https://www.superjumpmagazine.com/the-helpful-harmful-sounds-of-thumper/)
- Rhythm Heaven: input sounds are made to "stand out sharply and clearly" so the player feels "I just pressed a button!". — [Iwata Asks: Rhythm Heaven, p.3](https://iwataasks.nintendo.com/interviews/ds/rhythm-heaven/0/2)
- Rhythm Heaven: mistakes are meant to be funny for onlookers ("Unexpected mistakes will please your family"; in Ringside "the visuals are amusing when you make a mistake"). — [Iwata Asks: Rhythm Heaven Fever, p.1](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/0) and [p.3](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/2)
- Rhythm Heaven adds reward voices only in the real performance, not in practice. — [Iwata Asks: Rhythm Heaven Fever, p.3](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/2)
- Simon replaced Touch Me's "miserable, rasping sounds" with consonant tones that are harmonic in any order. — [Wikipedia: Simon (game)](https://en.wikipedia.org/wiki/Simon_(game))
- Simon Trickster uses spoken praise at milestones ("Awesome!", "Nice!", "Sweet!", "Respect!") and a victory melody; its failure signal is a crash sound and the word "Later!". — [Wikipedia: Simon (game)](https://en.wikipedia.org/wiki/Simon_(game))
- Patapon: the tribe sings the command back after a correct phrase (success is confirmed by a chorus echo), and Fever is a sustained audible reward state after a 10 combo. — [Wikipedia: Patapon](https://en.wikipedia.org/wiki/Patapon)
- PaRappa warns before it punishes: the lower rank flashes after one bad line and only drops after two. — [Wikipedia: PaRappa the Rapper](https://en.wikipedia.org/wiki/PaRappa_the_Rapper)
- Hi-Fi Rush: a press must never produce nothing; on-beat play is rewarded, off-beat play is not punished. — [Tacoma Ledger](https://thetacomaledger.com/2023/02/27/the-ingenious-accessibility-of-hi-fi-rush/)
- Rock Band-style stem triggering makes the player hear a correct performance for approximately-right input. — [Dubious Quality](https://dubiousquality.blogspot.com/2009/05/design-brilliance-and-timing-window.html)
- Mother 3 exposes the beat as a heartbeat sound when help is warranted. — snippet only, [Wikipedia: Mother 3](https://en.wikipedia.org/wiki/Mother_3)
- Blind-accessible precedents exist: "Feel the Rhythm" is a rhythm game using only audio and controller vibration, built for blind accessibility (requires an Xbox controller and headphones or two speakers), and "Finger Dance" (University of Oregon, 2007) is an audio-based rhythm-action game designed for visually impaired players, inspired by DDR and Guitar Hero. — snippet only, [Feel the Rhythm on itch.io](https://mrazikmj.itch.io/feel-the-rhythm); [UMass Dartmouth repository record](https://repository.lib.umassd.edu/esploro/outputs/journalArticle/Feel-the-Rhythm-A-Blind-Accessible-Rhythm/9914443326501301); [Finger Dance poster](https://www.cs.uoregon.edu/Activities/Poster_Contest/2007/AaronPoster.pdf)

### Inferences
A complete audio and haptic feedback vocabulary can be assembled from the above (all inference):
- Hit (good): the note sounds in the instrument's normal timbre. Hit (perfect): same note plus a bright overtone or sparkle and a short sharp vibration (Thumper).
- Early or late but accepted: the note still sounds, slightly muted or detuned, with no vibration; optionally a soft low or high "nudge" tick to hint direction. Keep early/late coaching for practice mode, not battles.
- Wrong button: the pressed button's own (consonant) tone plays, but the enemy's line is not cancelled; a soft dull thud rather than a buzzer (Simon versus Touch Me).
- Miss (no press): the scheduled note is simply absent or played as a ghost note by the teacher; the backing never stops (opposite of Thumper).
- Combo: add accompaniment layers every few successful phrases; lose only the top layer on a failed phrase (hysteresis as in PaRappa).
- Phrase success: chorus or companion echo of the phrase (Patapon sing-back), plus a long rolling vibration.
- Encounter success: the enemy's theme resolves and joins the player's theme; milestone spoken praise (Simon Trickster).
- Beat assist: a low vibration pulse on each beat during the response phase, switched on automatically after failures (Mother 3 heartbeat).
- Humour over shame: make failed phrases produce funny character reactions (Rhythm Heaven), and give a warning state before any loss.

### Gaps
- The Feel the Rhythm paper and the Finger Dance poster could not be read (403 and unreadable PDF), so their concrete cueing schemes and user-test results with blind players are missing. These are the two most directly relevant prior works and are worth a follow-up read.
- No sources retrieved on Guitar Hero/Rock Band's dropping the instrument stem on a miss, Rez/Tetris Effect layered music, Sayonara Wild Hearts' no-fail rewind, or crowd reactions. Unverified recollection: in Guitar Hero/Rock Band a missed note mutes the player's stem and plays a short clank; Rock Band 4's and Rhythm Heaven's crowd/character reactions carry most of the score feedback.
- No research found on haptic timing perception in children or on latency of BLE-triggered vibration.

## 7. Variety across instruments and input schemes (4 buttons + IMU + vibration)

### Takeaway
Evidence favours buttons for precise, frequent rhythm input and motion for expressive, low-frequency control: Nintendo abandoned swinging in Rhythm Heaven Fever because it was tiring, Skyward Sword's waggle harp and Spirit Tracks' microphone flute were criticised, while Wii Music shows that motion mapped to instrument families is enjoyable when pitch is automated and nothing can fail.

### Cited Findings
- Rhythm Heaven Fever prototyped swinging the Wii Remote and dropped it: it "made our hands tired", and the series needs very many inputs per minute; the final game uses buttons only. — [Iwata Asks: Rhythm Heaven Fever, p.1](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/0)
- Rhythm Heaven DS: the flick gesture needed months of tuning and only worked once paired with sound ("if we combined sound with the flicking action, you can get a good sense of the timing"); touching the screen edge was tried and judged too difficult. — [Iwata Asks: Rhythm Heaven, p.3](https://iwataasks.nintendo.com/interviews/ds/rhythm-heaven/0/2)
- Rhythm Heaven gets variety from re-skinning the same one or two inputs with new sounds and rhythms per minigame: "despite the relatively few controls, you're surprised by the wide variety of effects". The Wii entry deliberately concentrated on "actions reminiscent of percussion, such as pushing and pounding". — [Iwata Asks: Rhythm Heaven, p.3](https://iwataasks.nintendo.com/interviews/ds/rhythm-heaven/0/2); [Fever, p.1](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/0)
- Wii Music groups 66 instruments into a few gesture families: keyboards and percussion are played by swinging the controllers as if striking; bowed and strummed strings by moving the Remote like a bow or strumming hand while holding the Nunchuk like a neck. Slower movement gives softer notes; holding buttons adds damping, muting and tremolo. Pitch is chosen by the game. — [Wikipedia: Wii Music](https://en.wikipedia.org/wiki/Wii_Music)
- Wii Music's scored minigames show two more input ideas: Handbell Harmony (shake the left or right controller to ring one of two bells on cue) and Pitch Perfect (ear-training quiz: higher/lower, major/minor, reorder scrambled notes). — [Wikipedia: Wii Music](https://en.wikipedia.org/wiki/Wii_Music)
- Miyamoto wanted Wii Music to be about creative freedom rather than beat matching. — [Wikipedia: Wii Music](https://en.wikipedia.org/wiki/Wii_Music)
- Ocarina of Time: five notes on buttons, with the analog stick bending pitch. — [Wikipedia: Ocarina of Time](https://en.wikipedia.org/wiki/The_Legend_of_Zelda:_Ocarina_of_Time)
- Spirit Tracks' Spirit Flute (blow into the microphone) drew complaints: "terribly unresponsive" for one reviewer, and the long duet songs learned with the Lokomos were the most disliked part. — snippet only, [Zelda Dungeon: Best and Worst of Zelda instruments](https://www.zeldadungeon.net/?p=81763)
- Skyward Sword's harp (wave the controller back and forth) was criticised as "insultingly easy and hardly engaging", requiring "no real input from the player except... waggling", though it had narrative weight. — snippet only, [Zelda Dungeon](https://www.zeldadungeon.net/?p=81763)
- Wind Waker's baton and Twilight Princess's howling required specific timing; some players found the baton controls "a little odd". — snippet only, [Zelda Dungeon](https://www.zeldadungeon.net/?p=81763)
- Loom: reversing a phrase is an input-scheme variation with no new controls. — [Wikipedia: Loom](https://en.wikipedia.org/wiki/Loom_(video_game))
- Patapon: four buttons become four drums whose phrases are verbs (march, attack, defend). — [Wikipedia: Patapon](https://en.wikipedia.org/wiki/Patapon)
- Thumper pairs perfect inputs with controller shake as a distinct sensation from the sound. — [SUPERJUMP](https://www.superjumpmagazine.com/the-helpful-harmful-sounds-of-thumper/)

### Inferences
Candidate instrument "feels" using only 4 buttons, IMU and vibration (design proposals, not evidence):
- Drum/percussion (default): tap on the beat; 4 buttons = 4 drums; short sharp vibration per hit. Highest precision, use for most battles (supported by the Rhythm Heaven button decision).
- Flute/ocarina: 4 buttons = 4 pitches, hold for sustained notes; pitch (tilt up/down) bends the note or shifts octave, like the Ocarina's stick bend. Continuous soft vibration while a note is held.
- Maracas/shaker: shaking produces the sound; judge only the number of shakes or "shake during the response window", not per-shake timing, and keep bursts short to avoid fatigue (Rhythm Heaven Fever) and trivial waggle (Skyward Sword).
- Harp/strings: hold a button to choose the chord, sweep the controller (roll or yaw) to strum; the sweep should produce discrete string plucks under the player's control so it is not empty waggle.
- Conductor/baton: tilt direction selects one of 4 beats or sets tempo, used for slow, non-combat "spell" moments, like the Wind Waker.
- Bell: one firm downward flick rings a bell on cue (Wii Music Handbell Harmony), a low-frequency, high-payoff gesture suited to finishing moves.
- Rules that follow from the evidence: (1) motion for expression and low-frequency actions, buttons for dense rhythm; (2) every gesture needs an immediate, distinctive sound to make its timing perceivable (DS flick lesson); (3) inputs that feel unresponsive (Spirit Flute) or that ask nothing (Skyward Sword harp) are the two failure modes; (4) since "boards must produce identical HID reports" and IMU orientation is already in the report, instrument variety can be implemented entirely host-side.

### Gaps
- No empirical study was retrieved on the reliability or enjoyment of motion controls for children (5 to 12), or for blind children specifically; the evidence above is developer testimony and critical reception only.
- No sources retrieved for Rock Band instrument differences, Donkey Konga bongos plus clap sensor, Taiko's don/ka mapping, or Wind Waker's metre selection (unverified recollection for the last: the left stick selects 3/4, 4/4 or 6/4 and the right stick gives one direction per beat in time with a metronome).
- Latency and jitter of IMU gesture detection over BLE (versus button events) is unknown and should be measured on the actual controller before choosing which gestures are timing-judged.

## 8. Memory-game design: avoiding frustration, repetition and song ownership

### Takeaway
Simon-style games stay pleasant by using consonant tones, building sequences cumulatively, capping length by level, and praising milestones; adventure games turn memorised phrases into owned tools (Ocarina's 12 songs teleport and solve puzzles, Loom's drafts are verbs), which is what makes repetition feel like growing power rather than drilling.

### Cited Findings
- Simon's cumulative structure means every round replays everything learned so far and adds a small increment; level caps of 8, 14, 20 and 31 bound the session. — [Wikipedia: Simon (game)](https://en.wikipedia.org/wiki/Simon_(game))
- Simon's tones are consonant in any order (designed so by Baer), and milestone praise at 5 and 11 tones marks progress before the cap. — [Wikipedia: Simon (game)](https://en.wikipedia.org/wiki/Simon_(game))
- Simon variants show cheap replay value from the same content: positions only, moving colours, and backwards recall. — [Wikipedia: Simon (game)](https://en.wikipedia.org/wiki/Simon_(game))
- Ocarina of Time: 12 learned melodies function as tools (music puzzles, teleport to visited places); the instrument itself is a plot object (opens the Temple of Time chamber, sends Link back in time); area music reprises the learned tune. The game is credited with raising real-world ocarina sales. — [Wikipedia: Ocarina of Time](https://en.wikipedia.org/wiki/The_Legend_of_Zelda:_Ocarina_of_Time)
- Loom: every spell is a 4-note phrase; phrases are acquired by observing the world, are reusable anywhere, invert when reversed, are randomised per playthrough so they must be memorised or written down (the manual's "Book of Patterns" tells players to write in pencil), and capability grows as new notes are unlocked. — [Wikipedia: Loom](https://en.wikipedia.org/wiki/Loom_(video_game))
- Loom criticism: Scorpia found the game "just too lightweight" and linear as an adventure. — [Wikipedia: Loom](https://en.wikipedia.org/wiki/Loom_(video_game))
- Patapon: a small set of memorised phrases is the entire command vocabulary and is reused in every battle. — [Wikipedia: Patapon](https://en.wikipedia.org/wiki/Patapon)
- Rhythm Heaven: remixes re-test earlier games in new combinations; Superb medals unlock endless games and extras; improvement within "three or four" tries is a stated goal. — [Wikipedia: Rhythm Heaven](https://en.wikipedia.org/wiki/Rhythm_Heaven); [Iwata Asks: Fever, p.1](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/0)
- PaRappa: after mastery the player is invited to improvise rather than repeat (Cool mode), which gives replay value to learned songs. — [Wikipedia: PaRappa the Rapper](https://en.wikipedia.org/wiki/PaRappa_the_Rapper)
- Rhythm Heaven's composers aimed for music that "repeat[s] like a refrain in your head" after play. — [Iwata Asks: Rhythm Heaven Fever, p.4](https://iwataasks.nintendo.com/interviews/wii/rhythmheavenfever/0/3)

### Inferences
- Two kinds of memory should be separated in EchoQuest: short-term echo (the enemy's fresh phrase, forgotten after the battle) and long-term repertoire (a handful of fixed "spell songs" the child owns, as in Ocarina/Loom/Patapon). The repertoire is what creates ownership: a song learned in chapter 1 opens a door in chapter 3 and is hummable away from the game.
- Keep owned songs short and few: Loom uses 4 notes per spell and Ocarina gets 12 songs out of 5 pitches. With 4 buttons, 4 to 6 note songs with distinctive rhythms are distinguishable.
- Use Simon's cumulative build for teaching a long song: add one note per round rather than presenting the full phrase.
- An audio "songbook" (replay any learned song on request, at slow tempo) replaces Loom's pencil-and-paper Book of Patterns for blind players and doubles as a collection/replay menu.
- Reversal, transposition (tilt to change octave) and playing a known song on a different instrument are cheap variations that reuse memory already built.
- Reprising a learned song inside the background score of the place it belongs to (Ocarina) rehearses it passively.
- Do not randomise songs per playthrough as Loom does: for young children the benefit (preventing lookup) is irrelevant and it destroys cross-session ownership.

### Gaps
- No source retrieved that analyses why Simon frustrates (single-error game over, full restart) or documents fixes; the anti-frustration points above are inferred from its feature list.
- Ocarina song functions beyond teleport and puzzles (changing day to night, summoning rain, calling the horse, opening the Door of Time) and Majora's Mask's reuse of songs are unverified recollection; not confirmed by the pages read.
- No academic sources were retrieved (Kiri Miller's "Playing Along", Karen Collins's "Game Sound" / "Playing with Sound", Michael Austin's "Music Video Games"); their arguments about performance, ownership and "schizophonic" play-along are therefore not represented and would need a library pass.
- No data found on collection/replay as a motivator specifically for children in music games.
