"""Compose the PROVISIONAL prologue sound cues (p00_sfx_XX.wav, mono 48 kHz 16-bit).

Placeholders until real recordings are found. Sources: the Sonniss GDC 2026 game audio bundle
(parts 1, 2 and 5, royalty-free), plus simple synthesis (piano-like tones, claps, whistle) where the
bundle has nothing suitable. Every pitched note follows docs/music: the four button pitches
D4 F4 A4 C5 (01-palette.md), and the orchestra's song is the draft in 06-worked-example.md, whose
head C5 A4 F4 loses its third note to Sbuffo.

<mono_dir> must contain these Sonniss files converted to mono 48 kHz 16-bit WAV, e.g.
`ffmpeg -i "<file>.wav" -ac 1 -ar 48000 -sample_fmt s16 mono/stick.wav`:
    stick   WEAPBlnt_Spear And Stick Impact, Wooden MKH 2
    whoosh  WINDDsgn_Wind, Rush, Whoosh, Long x5 01
    storm   WINDDsgn_EXT, Eye Of The Storm (from 30 s, 20 s long)
    pages   PAPRMisc_Antique Books Flicking Through Pages 11
    rummage OBJLug_Rumaging Through Suitcase, Antique Suitcase
    drop    COMTelph_Antique Telephone Ear Piece Falling Down
    seat    FOLYProp_Sitting Down, Creaky Seat, Squeaks 3
    bells   MAGMisc_Magic Christmas Bells 2
    woodhit30  WOODImpt_Wooden Hit, Dark, Heavy Hit, Vampire's Prison (first 30 s)
    drag    METLFric_Large Metal Box, Drag, Geofon (first 12 s)
    pant    HMNBrth_Panting Male 02 04 (SoundBits Vox Hominis, bundle part 5)
    laugh   VOXLaff_Police Officer Laugh Vocal Male 1 (Epic Stock Media, bundle part 2)
and these Kenney CC0 sounds (kenney.nl: RPG Audio, Impact Sounds, Interface Sounds), converted the
same way and prefixed with k_: footstep_wood_000-004, creak1-3, confirmation_001, bookFlip1-3,
cloth1-4, impactWood_light_000-004, impactSoft_medium_000-004.

Usage: python3 tools/story_placeholder_sfx.py <mono_dir> <out_dir> [what ...]
`what` can be a section (p00, s01 ... s05, e00, or t00-t99 and c00-c99 for chapters 2 and 3), a cue id (s01_sfx_05) or a prologue cue number (16).
Without it, everything is written.

Scenes 2 and 3 are music first: their piano phrases, Sbuffo's motif and the whole first battle are
grid specs in music/library/capitolo_01.yaml, checked by `python3 -m tools.music.check` and rendered by
tools/music. Sounds that depend on the register the child chose in Scene 2 are written twice, with
the suffix _grave (buttons D3-C4) or _acuto (D5-C6); Unity picks the right one.
"""
import math
import random
import re
import struct
import sys
import wave
from array import array

sys.path.insert(0, __file__.rsplit("/tools/", 1)[0])
from tools.music import grid as music  # noqa: E402

SR = 48000
MONO, OUT = sys.argv[1], sys.argv[2]
ONLY, ONLY_SECTIONS = set(), set()
for _arg in sys.argv[3:]:
    if re.fullmatch(r"[a-z]\d\d", _arg):
        ONLY_SECTIONS.add(_arg)
    elif re.fullmatch(r"[a-z]\d\d_sfx_\d\d|x_\w+", _arg):
        ONLY.add(_arg)
    else:
        ONLY.add(f"p00_sfx_{int(_arg):02d}")
random.seed(4)


def load(name, start=0.0, end=None):
    with wave.open(f"{MONO}/{name}.wav") as w:
        assert w.getframerate() == SR and w.getnchannels() == 1 and w.getsampwidth() == 2
        data = array("h", w.readframes(w.getnframes()))
    a, b = int(start * SR), len(data) if end is None else int(end * SR)
    return [s / 32768 for s in data[a:b]]


def silence(sec):
    return [0.0] * int(sec * SR)


def mix(dst, src, at=0.0, gain=1.0):
    off = int(at * SR)
    if off + len(src) > len(dst):
        dst.extend([0.0] * (off + len(src) - len(dst)))
    for i, s in enumerate(src):
        dst[off + i] += s * gain
    return dst


def fade(x, fin=0.0, fout=0.0):
    n_in, n_out = int(fin * SR), int(fout * SR)
    for i in range(min(n_in, len(x))):
        x[i] *= i / n_in
    for i in range(min(n_out, len(x))):
        x[-1 - i] *= i / n_out
    return x


def ramp(x, g0, g1):
    n = len(x)
    return [s * (g0 + (g1 - g0) * i / n) for i, s in enumerate(x)]


def biquad(x, kind, f0, q=0.707):
    w0 = 2 * math.pi * f0 / SR
    al, c = math.sin(w0) / (2 * q), math.cos(w0)
    if kind == "lp":
        b = [(1 - c) / 2, 1 - c, (1 - c) / 2]
    elif kind == "hp":
        b = [(1 + c) / 2, -(1 + c), (1 + c) / 2]
    else:  # bandpass, constant peak gain
        b = [al, 0.0, -al]
    a = [1 + al, -2 * c, 1 - al]
    b = [v / a[0] for v in b]
    a1, a2 = a[1] / a[0], a[2] / a[0]
    y, x1, x2, y1, y2 = [], 0.0, 0.0, 0.0, 0.0
    for s in x:
        o = b[0] * s + b[1] * x1 + b[2] * x2 - a1 * y1 - a2 * y2
        x2, x1, y2, y1 = x1, s, y1, o
        y.append(o)
    return y


def resample(x, rate):
    """Play x at `rate` times the speed (rate < 1 = lower and longer)."""
    n = int(len(x) / rate)
    return [x[min(int(i * rate), len(x) - 1)] for i in range(n)]


def piano(freq, dur=2.0, vel=0.7, damp=None, cents=0.0, sag=0.0, wobble=0.0):
    """A simple piano-like tone: inharmonic partials with exponential decay and a hammer click.

    Out-of-tune options (in cents, 100 = one semitone): `cents` detunes the whole note,
    `sag` bends it by that much over the first second, `wobble` adds a slow, seasick vibrato.
    """
    n = int(dur * SR)
    out = [0.0] * n
    inharm = 0.0004
    bent = cents != 0.0 or sag != 0.0 or wobble != 0.0
    for k in range(1, 9):
        fk = k * freq * math.sqrt(1 + inharm * k * k)
        if fk > SR / 2.2:
            break
        amp = vel / k ** 1.3 * (1.0 if k == 1 else 0.8)
        decay = (1.2 + 0.6 * k) * (freq / 261.6) ** 0.5
        ph = random.random() * 2 * math.pi
        w = 2 * math.pi * fk / SR
        if not bent:
            for i in range(n):
                out[i] += amp * math.exp(-decay * i / SR) * math.sin(w * i + ph)
        else:
            for i in range(n):
                t = i / SR
                c = cents + sag * min(1.0, t) + wobble * math.sin(2 * math.pi * 3.2 * t)
                ph += w * 2 ** (c / 1200)
                out[i] += amp * math.exp(-decay * t) * math.sin(ph)
    for i in range(int(0.004 * SR)):  # attack
        out[i] *= i / (0.004 * SR)
    click = biquad([random.uniform(-1, 1) * math.exp(-i / (0.003 * SR)) for i in range(int(0.02 * SR))], "bp", 2500, 1.0)
    mix(out, click, 0, 0.15 * vel)
    if damp:  # key released: fast decay after `damp` seconds
        d0 = int(damp * SR)
        for i in range(d0, n):
            out[i] *= math.exp(-(i - d0) / (0.04 * SR))
    return out


def clap():
    burst = []
    for rep in range(3):  # a clap is a few very close bursts
        burst += [random.uniform(-1, 1) * math.exp(-i / (0.004 * SR)) for i in range(int(0.006 * SR))]
    burst += [random.uniform(-1, 1) * math.exp(-i / (0.03 * SR)) for i in range(int(0.12 * SR))]
    return biquad(biquad(burst, "bp", 1400, 0.8), "hp", 400)


def whistle(dur=0.9, f0=1700, f1=800):
    n = int(dur * SR)
    out, ph = [], 0.0
    for i in range(n):
        t = i / n
        f = f0 + (f1 - f0) * t + 25 * math.sin(2 * math.pi * 6 * i / SR)
        ph += 2 * math.pi * f / SR
        env = math.sin(math.pi * t) ** 1.5
        out.append(env * (0.6 * math.sin(ph) + 0.08 * random.uniform(-1, 1)))
    return out


def normalize(x, peak_db=-3.0):
    p = max(abs(s) for s in x) or 1.0
    g = 10 ** (peak_db / 20) / p
    return [s * g for s in x]


def selected(name):
    if not ONLY and not ONLY_SECTIONS:
        return True
    # interaction clips (x_s02_..., x_e00_..., x_piano_...) belong to their section
    section = name[2:5] if re.match(r"x_[a-z]\d\d_", name) else "s02" if name.startswith("x_piano") else name[:3]
    return name in ONLY or section in ONLY_SECTIONS


def save(name, x, peak_db=-3.0):
    if not selected(name):
        return
    x = normalize(x, peak_db)
    with wave.open(f"{OUT}/{name}.wav", "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(struct.pack(f"<{len(x)}h", *(max(-32767, min(32767, int(s * 32767))) for s in x)))
    print(f"{name}.wav  {len(x) / SR:.1f}s")


def tom(vel=0.8):
    """A floor tom: BUM."""
    n = int(0.6 * SR)
    out, ph = [], 0.0
    for i in range(n):
        t = i / SR
        f = 70 + 60 * math.exp(-t / 0.05)
        ph += 2 * math.pi * f / SR
        out.append(vel * math.exp(-t / 0.22) * math.sin(ph))
    hit = biquad([random.uniform(-1, 1) * math.exp(-i / (0.02 * SR)) for i in range(int(0.08 * SR))], "lp", 900)
    return mix(out, hit, 0, 0.5 * vel)


def campanella(note="A5", vel=0.5, dur=2.5):
    """The campanella (04-battle-cues.md: a celesta, clear pitch, an octave above the buttons). It
    replaces the prologue's triangle, so the bell the child recovers is in the orchestra from the start."""
    return music.synth.celesta(NOTE[note], dur, vel)


# Bell notes on beats 2 and 4 of each song chord: chord tones among the button letters, D5-C6.
BELL_NOTES = {"F": ("A5", "C6"), "Dm7": ("A5", "C6"), "Bb": ("F5", "D5"), "Csus4": ("F5", "C6"), "Gm7": ("F5", "D5")}


def flute(freq, dur, vel=0.5, breath=0.08):
    """A flute-like tone: soft attack, gentle vibrato, a little breath noise."""
    n = int(dur * SR)
    out, ph = [], 0.0
    attack, release = 0.07, min(0.15, dur / 3)
    env = []
    for i in range(n):
        t = i / SR
        vib = 12 * math.sin(2 * math.pi * 5.2 * t) * min(1.0, max(0.0, (t - 0.25) / 0.3))
        ph += 2 * math.pi * freq * 2 ** (vib / 1200) / SR
        e = min(1.0, t / attack) * min(1.0, max(0.0, (dur - t) / release))
        env.append(e)
        out.append(vel * e * (math.sin(ph) + 0.22 * math.sin(2 * ph) + 0.1 * math.sin(3 * ph) + 0.04 * math.sin(4 * ph)))
    air = biquad([random.uniform(-1, 1) for _ in range(n)], "bp", min(freq * 2, 8000), 2.0)
    for i in range(n):
        out[i] += breath * vel * env[i] * air[i]
    return out


_CACHE = {}


def cached(fn, *args, **kw):
    """Instruments are expensive in pure Python: reuse identical notes."""
    key = (fn.__name__, args, tuple(sorted(kw.items())))
    if key not in _CACHE:
        _CACHE[key] = fn(*args, **kw)
    return _CACHE[key]


class _Notes(dict):
    """Note name ("D4", "Bb2", "F#4") -> equal-tempered frequency in Hz, A4 = 440."""

    def __missing__(self, name):
        m = re.fullmatch(r"([A-G])(#|b)?(\d)", name)
        pc = "C D EF G A B".index(m[1]) + {"#": 1, "b": -1, None: 0}[m[2]]
        return 440.0 * 2 ** ((pc + 12 * (int(m[3]) + 1) - 69) / 12)


NOTE = _Notes()

# docs/music/01-palette.md: buttons 1-4 (index, middle, ring, little) are always D4 F4 A4 C5.
# Every pitched note a child could take for "their" note uses only these four.
BUTTONS = ("D4", "F4", "A4", "C5")

# docs/music/06-worked-example.md: "La canzone del concerto" (draft), centre F, 100 BPM, 4/4,
# eighth-note grid. Letters are buttons (D F A C), "-" holds, "." rests.
BAR = 2.4  # 100 BPM
BEAT = BAR / 4
EIGHTH = BAR / 8
SONG_BARS = ["C - A - F - - .",   # 1 head: so-mi-do; the F is the note Sbuffo takes
             "D - . . F D - .",   # 2 TA... TA-TA
             "A A C - A A F -",   # 3 the drum's gallop
             "C - - - A - - .",   # 4
             "C - A - F - - .",   # 5 head again (the flute's piece)
             "D - . . F D - .",   # 6
             "C A C A C - A -",   # 7 so-mi figure
             "A - C - F - - -"]   # 8 home to do
LETTER = {"D": "D4", "F": "F4", "A": "A4", "C": "C5"}
# One chord per bar. World layer only (01-palette.md register map): bass D1-Bb2, comping Bb2-Bb3,
# nothing sustained in the call register C4-D5, no E B F# C# G#.
SONG_CHORDS = ["F", "Dm7", "Bb", "Csus4", "F", "Dm7", "Gm7", "F"]
CHORD = {"F": ("F2", ("C3", "F3", "A3")),
         "Dm7": ("D2", ("C3", "F3", "A3")),
         "Bb": ("Bb1", ("Bb2", "D3", "F3")),
         "Csus4": ("C2", ("C3", "F3", "G3")),
         "Gm7": ("G2", ("Bb2", "D3", "F3"))}


def bar_notes(bar):
    """[(start in s, note, length in s)] for one grid bar."""
    toks = bar.split()
    out = []
    for i, t in enumerate(toks):
        if t in LETTER:
            n = 1
            while i + n < len(toks) and toks[i + n] == "-":
                n += 1
            out.append((i * EIGHTH, LETTER[t], n * EIGHTH))
    return out


# The recurring theme of the chapter: the song's head, C5 A4 F4 at 0, 0.6 and 1.2 s.
THEME = [(t, n) for t, n, _ in bar_notes(SONG_BARS[0])]


def song_piano(bar_index, vel=0.7):
    out = silence(BAR + 1.5)
    for t, n, d in bar_notes(SONG_BARS[bar_index % len(SONG_BARS)]):
        mix(out, cached(piano, NOTE[n], max(1.0, d + 0.6), vel, damp=d + 0.25), t)
    return out


def add_bar(out, at, bar_index=0, bass=True, drum=True, tri=True, flt=True, first_beat=0):
    """Mix one bar of the song, as the band plays it, into `out` at time `at`.
    `first_beat` skips the drum's first beats."""
    k_bar = bar_index % len(SONG_BARS)
    mix(out, song_piano(k_bar), at)
    root, _ = CHORD[SONG_CHORDS[k_bar]]
    if bass:
        mix(out, cached(piano, NOTE[root], 2.4, 0.45), at)
    if drum:
        if k_bar == 2:  # the drum plays its own piece of the song: ti-ti ta, ti-ti ta
            for k in (0, 1, 2, 4, 5, 6):
                if k >= 2 * first_beat:
                    mix(out, cached(tom, 0.8 if k in (0, 4) else 0.55), at + k * EIGHTH, 0.7)
        else:
            for k in range(first_beat, 4):
                mix(out, cached(tom, 0.8 if k % 2 == 0 else 0.5), at + k * BEAT, 0.7)
    if tri:  # the campanella (it was a triangle): chord tones on beats 2 and 4
        for t, n in zip((BEAT, 3 * BEAT), BELL_NOTES[SONG_CHORDS[k_bar]]):
            mix(out, cached(campanella, n, 0.4, 1.2), at + t, 0.3)
    if flt:  # the flute doubles the melody an octave up, softly
        for t, n, d in bar_notes(SONG_BARS[k_bar]):
            up = n[:-1] + str(int(n[-1]) + 1)
            mix(out, cached(flute, NOTE[up], max(0.25, d - 0.05), 0.4), at + t, 0.35)
    return out


def broken_theme(intensity=1.0):
    """The theme after the wind has stolen it: out of tune, sagging, the last note gone.
    Where the third note (F4) should be there is only a dull, stuck key."""
    out = silence(BAR + 1.5)
    mix(out, piano(NOTE["C5"], 1.6, 0.6, cents=-35 * intensity, wobble=18 * intensity), 0.0)
    mix(out, piano(NOTE["A4"], 1.6, 0.6, cents=45 * intensity, sag=-90 * intensity, wobble=25 * intensity), 0.65)
    mix(out, piano(NOTE["F4"], 0.5, 0.35, cents=-110 * intensity, damp=0.06), 1.45)  # the note that will not come
    return out


def cue(n):
    return f"p00_sfx_{n:02d}"


def wanted(n):
    return selected(cue(n))



if wanted(1):  # room ambience, someone tries a few piano notes in the distance. MISSING: footsteps, door, murmurs.
    x = fade(load("seat"), 0.3, 0.8)
    for t, n, v in [(1.6, "A4", 0.3), (2.5, "A4", 0.25), (3.4, "C5", 0.3), (4.6, "D4", 0.25)]:
        mix(x, piano(NOTE[n], 1.8, v), t, 3.0)
    save(cue(1), fade(x, 0, 0.5), -6)

if wanted(2):  # three funny, slightly out-of-tune piano notes
    x = silence(2.6)
    for t, f, v in [(0.0, NOTE["A4"] * 0.97, 0.6), (0.35, NOTE["F4"] * 1.03, 0.6), (0.75, NOTE["D3"] * 0.95, 0.8)]:
        mix(x, piano(f, 1.6, v, damp=0.25 if t < 0.7 else None), t)
    save(cue(2), x)

# "Un suono può essere lungo / corto / delicato / forte": a different instrument for each.
if wanted(3):
    save(cue(3), mix(silence(3.8), flute(NOTE["C5"], 3.5, 0.5)), -6)  # long: flute
if wanted(4):
    x = silence(1.4)  # short: three drum hits
    for t in (0.0, 0.3, 0.6):
        mix(x, tom(0.7), t)
    save(cue(4), x, -3)
if wanted(5):
    save(cue(5), campanella("A5", 0.25, 3.0), -12)  # delicate: one soft touch of the campanella
if wanted(6):
    x = silence(3.2)  # loud: everybody together
    for n in ("F2", "C3", "F3", "A3", "C4"):  # F major, home
        mix(x, piano(NOTE[n], 3.0, 1.0), 0.0, 0.45)
    mix(x, tom(1.0), 0.0, 0.9)
    mix(x, campanella("C6", 0.6), 0.0, 0.4)
    mix(x, flute(NOTE["C5"], 1.6, 0.6), 0.0, 0.6)
    save(cue(6), x, -3)

if wanted(7):  # one clap in a big room (the reverb comes from Unity). MISSING: footsteps.
    save(cue(7), mix(silence(1.0), clap(), 0.05), -4)
if wanted(8):  # two claps
    save(cue(8), mix(mix(silence(1.4), clap(), 0.05), clap(), 0.6), -4)

# "C'è chi fa bum bum / din din / suoni delicati / tutti insieme".
if wanted(9):
    save(cue(9), mix(mix(silence(1.4), tom(0.9), 0.0), tom(0.9), 0.5), -3)  # BUM, BUM
if wanted(10):
    save(cue(10), mix(mix(silence(3.0), campanella("C6", 0.5), 0.0), campanella("A5", 0.5), 0.6), -6)  # DIN, DIN (campanella)
if wanted(11):
    x = silence(2.4)  # a whispering flute
    for t, n, d in [(0.0, "F4", 0.5), (0.5, "A4", 0.5), (1.0, "F4", 1.2)]:
        mix(x, flute(NOTE[n], d, 0.25, breath=0.2), t)
    save(cue(11), x, -10)
if wanted(12):
    save(cue(12), fade(add_bar(silence(0), 0.0), 0, 1.0), -4)  # all together for a moment

if wanted(13):  # a small object falls. MISSING: throat clearing.
    save(cue(13), mix(silence(0.6), load("drop")), -6)
if wanted(14):  # rummaging through objects
    save(cue(14), fade(load("rummage", 0.0, 4.5), 0.05, 0.8), -6)
if wanted(15):  # someone laughs in the distance (Sonniss: Police Officer Laugh Vocal Male 1)
    save(cue(15), fade(load("laugh"), 0.0, 0.3), -8)
if wanted(16):  # TAC, TAC... TOC! (two wooden hits, then a lower one)
    hit = fade(load("stick", 0.60, 0.90), 0.002, 0.05)
    toc = resample(fade(load("stick", 1.28, 1.58), 0.002, 0.05), 0.7)
    save(cue(16), mix(mix(mix(silence(1.6), hit, 0.0), hit, 0.32, 0.9), toc, 0.95, 1.1), -3)

if wanted(17):
    # The orchestra's song, built up under the narrator, who starts 3 s in (regia: attesa 3).
    # Narrator times (p00_narr_05) -> song time = narrator time + 3:
    #   "Prima senti un suono" ~1 s (piano alone) · "poi ne arriva un altro" ~3 s (drum at 6.0)
    #   "e un altro ancora" ~5 s (campanella at 8.4) · "Ogni strumento..." ~7 s (flute and bass at 9.6)
    #   "La musica continua a crescere" ~15 s (more layers at 16.8, louder up to 20.5)
    # Then the full band keeps playing (looping) until the wind ruins it.
    length = 4 * len(SONG_BARS) * BAR  # the 8-bar song four times (76.8 s)
    x = silence(length + 2)
    bars = int(length / BAR)
    for b in range(bars):
        at = b * BAR
        add_bar(x, at, b,
                bass=at >= 9.6,
                drum=at + 3 * BEAT >= 6.0, first_beat=max(0, math.ceil((6.0 - at) / BEAT - 1e-6)) if at < 6.0 else 0,
                tri=at + BEAT >= 8.4 - 1e-6 and at >= 7.2,
                flt=at >= 9.6)
        if at >= 16.8:  # growing: chords on every bar and extra bell notes
            for n in CHORD[SONG_CHORDS[b % len(SONG_CHORDS)]][1]:
                mix(x, cached(piano, NOTE[n], 2.0, 0.4), at, 0.35)
            for k, n in zip((0, 2), BELL_NOTES[SONG_CHORDS[b % len(SONG_CHORDS)]]):
                mix(x, cached(campanella, n, 0.3, 1.2), at + k * BEAT, 0.2)
    for i in range(len(x)):  # crescendo: 0.45 until 9.6 s, full at 20.5 s
        t = i / SR
        x[i] *= 0.45 + 0.55 * min(1.0, max(0.0, (t - 9.6) / (20.5 - 9.6)))
    save(cue(17), fade(x[:int(length * SR)], 0, 2.0), -3)

if wanted(18):  # a gust of wind crosses the room, light at first, then more insistent (never deafening)
    storm = biquad(biquad(load("storm", 0.0, 8.0), "lp", 1200), "hp", 120)
    save(cue(18), fade(ramp(storm, 0.15, 0.7), 2.5, 1.2), -6)
if wanted(19):  # FFFF... a door bangs softly, papers flutter
    x = mix(silence(2.6), load("whoosh"), 0.0)
    mix(x, fade(load("woodhit30", 1.9, 3.4), 0.003, 0.6), 0.55, 0.35)
    mix(x, load("pages"), 0.8, 1.6)
    save(cue(19), x, -4)
if wanted(20):  # wind again; the band loses instruments one by one, then the piano falls apart
    x = silence(0)
    add_bar(x, 0.0, 0)
    add_bar(x, BAR, 1, tri=False, flt=False)
    mix(x, broken_theme(0.6), 2 * BAR)
    mix(x, load("whoosh"), 0.9 * BAR, 0.8)
    mix(x, load("whoosh"), 1.9 * BAR, 0.8)
    save(cue(20), fade(x, 0, 0.8), -3)
if wanted(21):  # the piano tries the theme: two wrong notes, a stuck key, a pause that is too long
    # Also loops under the narrator ("la canzone continua, ma non è più la stessa").
    save(cue(21), mix(silence(BAR + 2.8), broken_theme(1.0)), -6)
if wanted(22):  # musical silence, only a light wind (loopable bed)
    bed = biquad(biquad(load("storm", 8.0, 16.0), "lp", 600), "hp", 100)
    save(cue(22), fade(bed, 1.0, 1.0), -14)
if wanted(23):  # the wind leaves quickly, a last whistle
    x = mix(silence(2.0), load("whoosh"), 0.0)
    mix(x, whistle(), 0.45, 0.25)
    save(cue(23), fade(x, 0, 0.6), -4)




def steps(times, variants=5, vel=1.0):
    """Footsteps on a wooden stage at the given times (Kenney footstep_wood)."""
    end = (times[-1] if times else 0) + 0.6
    out = silence(end)
    for k, t in enumerate(times):
        mix(out, load(f"k_footstep_wood_{k % variants:03d}"), t, vel)
    return out


def tac_tac_toc():
    hit = fade(load("stick", 0.60, 0.90), 0.002, 0.05)
    toc = resample(fade(load("stick", 1.28, 1.58), 0.002, 0.05), 0.7)
    return mix(mix(mix(silence(1.6), hit, 0.0), hit, 0.32, 0.9), toc, 0.95, 1.1)


def stick_drop(variant=0):
    """The baton falls: a light wooden hit and a small bounce."""
    out = silence(1.0)
    mix(out, load(f"k_impactWood_light_{variant:03d}"), 0.0)
    mix(out, load(f"k_impactWood_light_{(variant + 2) % 5:03d}"), 0.18, 0.35)
    return out


def memory(x):
    """Turns a phrase into a memory: muffled, with soft echoes, slightly wavering."""
    y = biquad(x, "lp", 2200)
    out = y + silence(1.6)
    for delay, g in ((0.23, 0.35), (0.47, 0.2), (0.71, 0.1)):
        mix(out, y, delay, g)
    return out


def phrase(notes, vel=0.6, dur=1.4):
    """Piano notes [(time, note or freq, optional damp)]."""
    end = max(t for t, *_ in notes) + dur + 0.3
    out = silence(end)
    for t, n, *rest in notes:
        f = NOTE[n] if isinstance(n, str) else n
        mix(out, piano(f, dur, vel, damp=rest[0] if rest else None), t)
    return out




def cue1(n):
    return f"s01_sfx_{n:02d}"


def wanted1(n):
    return selected(cue1(n))




def stolen_try(intensity=0.6, vel=0.6):
    """The piano trying the stolen song after the wind: the same two out-of-tune notes as in the
    prologue (broken_theme), and then nothing where the third note should be."""
    out = silence(BAR + 0.6)
    mix(out, piano(NOTE["C5"], 1.6, vel, cents=-35 * intensity, wobble=18 * intensity), 0.0)
    mix(out, piano(NOTE["A4"], 1.6, vel, cents=45 * intensity, sag=-90 * intensity, wobble=25 * intensity), 0.65)
    return out


def song_memory():
    """The memory of the song as the whole orchestra played it in the prologue (one bar), muffled."""
    return fade(memory(add_bar(silence(0), 0.0, 0)), 0, 1.2)


def one_breath(k=2, speed=1.0, gain=1.0):
    """One breath from the panting recording (k = which breath), slowed down by `speed` < 1."""
    cuts = [0.0, 1.05, 2.35, 3.70, 5.20, 6.60, 8.05]
    x = fade(load("pant", cuts[k], cuts[k + 1]), 0.03, 0.25)
    if speed != 1.0:
        x = resample(x, speed)
    return [v * gain for v in x]


def blip(freq, dur=0.22, vel=0.5):
    """A neutral, soft signal tone (not an instrument)."""
    n = int(dur * SR)
    return [vel * min(1.0, i / 240) * math.exp(-i / (0.06 * SR)) * math.sin(2 * math.pi * freq * i / SR) for i in range(n)]


# Numbers follow capitolo_01_suoni.csv. The stolen song only ever appears as stolen_try (two notes,
# then silence) or as song_memory; every other piano sound uses the button notes in a different order,
# so the child can tell them apart.
if wanted1(1):  # pacing steps, the piano trying the song in the distance, Sbuffo's breath fading away
    x = steps([0.0, 0.55, 1.1, 1.65, 2.2, 3.4, 3.95, 4.5, 5.05])
    mix(x, biquad(stolen_try(0.6, 0.4), "lp", 1800), 2.6, 0.5)
    mix(x, fade(biquad(load("storm", 2.0, 5.0), "lp", 700), 0.05, 2.5), 0.0, 0.35)
    save(cue1(1), fade(x, 0, 0.5), -4)
if wanted1(2):
    save(cue1(2), stick_drop(0), -4)  # TAC! the baton falls
if wanted1(3):  # bending down to pick it up, a little effort breath
    x = mix(silence(1.6), load("k_cloth1"), 0.0, 0.8)
    mix(x, one_breath(1, 0.95, 0.6), 0.3)
    mix(x, load("k_impactWood_light_004"), 1.1, 0.3)
    save(cue1(3), x, -6)
if wanted1(4):  # "three notes try to make a melody": the prologue's broken theme, the third key stuck
    save(cue1(4), broken_theme(0.8), -4)
if wanted1(5):  # the Maestro's steps slow down and stop
    save(cue1(5), steps([0.0, 0.6, 1.4], vel=0.9), -6)
if wanted1(6):  # two funny piano notes
    save(cue1(6), phrase([(0.0, NOTE["F4"] * 1.04, 0.2), (0.35, NOTE["D3"] * 0.96)], 0.7), -4)
if wanted1(7):
    save(cue1(7), stick_drop(1), -4)  # the baton falls again
if wanted1(8):  # picks up the baton (the "ehm, ehm" is in the voice line)
    x = mix(silence(1.2), load("k_cloth2"), 0.0, 0.8)
    mix(x, load("k_impactWood_light_003"), 0.6, 0.4)
    save(cue1(8), x, -6)
for n in (9, 10, 17, 39):
    if wanted1(n):
        save(cue1(n), tac_tac_toc(), -3)
if wanted1(12):  # the piano tries a short phrase (the song) and breaks off
    save(cue1(12), stolen_try(0.6), -4)
if wanted1(13):  # the baton brushes the music stand
    x = mix(silence(0.8), load("k_impactWood_light_002"), 0.0, 0.4)
    mix(x, biquad([random.uniform(-1, 1) * math.sin(math.pi * i / 9600) for i in range(9600)], "bp", 3000, 1.0), 0.05, 0.15)
    save(cue1(13), x, -10)
if wanted1(14):  # TA... TA... TA: three slow, regular claps
    x = silence(2.6)
    for t in (0.0, 1.0, 2.0):
        mix(x, clap(), t)
    save(cue1(14), x, -4)
if wanted1(15):
    save(cue1(15), steps([0.0, 0.9, 1.8]), -4)  # TUM... TUM... TUM: three slow steps
if wanted1(16):
    save(cue1(16), steps([0.24 * k for k in range(9)]), -4)  # running: faster steps
if wanted1(18):  # the Maestro's rhythm, then the piano imitates it on the index note D4, as the child just did
    x = tac_tac_toc() + silence(1.6)
    mix(x, phrase([(0.0, "D4", 0.15), (0.32, "D4", 0.15), (0.95, "D4", 0.35)], 0.6, 0.9), 1.8)  # short, so it doesn't ring under the voice
    save(cue1(18), fade(x, 0, 0.3), -3)
if wanted1(19):  # two cheerful notes (rising, unlike the song's falling C-A)
    save(cue1(19), phrase([(0.0, "A4", 0.18), (0.25, "C5", 0.3)], 0.6), -4)
if wanted1(20):  # a breath of wind far away (the same storm as Sbuffo in the prologue), a door creaks a little
    x = fade(biquad(load("storm", 4.0, 8.0), "lp", 900), 1.0, 1.5)
    mix(x, load("k_creak1"), 1.6, 0.6)
    save(cue1(20), x, -8)
if wanted1(21):  # the wind blows a few more seconds, then calms down
    save(cue1(21), fade(ramp(biquad(load("storm", 9.0, 14.0), "lp", 800), 0.8, 0.1), 0.6, 2.0), -8)
if wanted1(22):  # the Maestro sighs once: a single, slowed-down exhale
    sigh = fade(load("pant", 3.86, 4.62), 0.04, 0.35)  # only the exhale of one breath
    save(cue1(22), biquad(resample(sigh, 0.85), "lp", 3000) + silence(0.2), -12)
if wanted1(23):  # the memory of the complete song, a pause, then today's piano without the last note
    # (the child is asked "Ti sembrano uguali?": no timed press in the silence, too hard)
    save(cue1(23), song_memory() + silence(0.8) + stolen_try(0.6) + silence(0.6), -5)
if wanted1(24):  # the piano tries again and again, hesitating more each time; the third note never comes
    x = silence(0)
    for k, (at, vel) in enumerate(((0.0, 0.6), (3.0, 0.5), (6.2, 0.4))):
        mix(x, stolen_try(0.6 + 0.2 * k, vel), at)
    save(cue1(24), x + silence(0.4), -4)
if wanted1(25):  # two quick steps towards the door, then a stop
    save(cue1(25), steps([0.0, 0.3], vel=1.0), -4)
if wanted1(26):  # starts running, stumbles after two steps and stops, out of breath
    x = steps([0.0, 0.22, 0.44])
    mix(x, load("k_cloth3"), 0.5, 0.8)
    mix(x, load("k_impactSoft_medium_000"), 0.6, 0.8)
    mix(x, load("k_footstep_wood_003"), 0.95, 0.6)
    mix(x, one_breath(4, 1.0, 0.5), 1.2)
    save(cue1(26), x + silence(0.2), -4)
if wanted1(27):
    save(cue1(27), phrase([(0.0, "F4"), (0.6, "A4")], 0.4, 2.0), -6)  # two soft, encouraging notes
if wanted1(28):
    save(cue1(28), phrase([(0.0, "D4", 0.2), (0.25, "A4", 0.3)], 0.6), -4)  # two cheerful notes
if wanted1(29):
    save(cue1(29), piano(NOTE["D4"], 1.8, 0.6), -4)  # one note: the index button
if wanted1(30):  # a second note joins the first
    save(cue1(30), mix(piano(NOTE["D4"], 2.2, 0.55), piano(NOTE["F4"], 2.0, 0.55), 0.4), -4)
if wanted1(31):  # a drum gently accompanies the two notes (a new example, not the song)
    x = silence(3.6)
    for k in range(3):
        mix(x, piano(NOTE["D4"], 1.2, 0.45), k * 1.2)
        mix(x, piano(NOTE["F4"], 1.2, 0.45), k * 1.2 + 0.6)
        mix(x, tom(0.5), k * 1.2, 0.7)
    save(cue1(31), fade(x, 0, 0.6), -4)
if wanted1(32):  # the sounds stop, a short silence
    save(cue1(32), mix(silence(1.2), phrase([(0.0, "D4", 0.08), (0.0, "F4", 0.08)], 0.4, 0.4)), -12)
if wanted1(33):
    save(cue1(33), phrase([(0.0, "F4"), (0.5, "A4"), (1.0, "C5")], 0.4, 1.6), -6)  # three sweet notes
if wanted1(11):  # four little, well separated neutral signals (not instruments yet), on the four button pitches
    x = silence(3.6)
    for k, n in enumerate(BUTTONS):
        mix(x, blip(NOTE[n]), k * 0.9)
    save(cue1(11), x, -8)
if wanted1(34):  # the piano in the distance tries the song and stops before the last note
    save(cue1(34), biquad(stolen_try(0.6, 0.45), "lp", 2000) + silence(0.4), -8)
if wanted1(35):  # memory of the full song, a pause, then the live piano without the last note
    x = song_memory() + silence(0.8)
    x += stolen_try(0.6) + silence(0.6)
    save(cue1(35), x, -5)
if wanted1(36):  # walking across the room; here and there an instrument tries a single sound
    x = steps([0.6 * k for k in range(12)], vel=0.9)
    mix(x, tom(0.3), 1.5, 0.5)
    mix(x, campanella("A5", 0.2, 1.2), 3.6, 0.4)
    mix(x, flute(NOTE["A4"], 0.6, 0.3), 5.0, 0.5)
    mix(x, piano(NOTE["F4"], 1.0, 0.3), 6.2, 0.6)
    save(cue1(36), fade(x, 0, 0.8), -4)
if wanted1(37):  # the piano tries its part once more: two notes, then silence
    save(cue1(37), stolen_try(0.6) + silence(0.9), -4)
if wanted1(38):
    save(cue1(38), piano(NOTE["D2"], 5.0, 0.7), -4)  # a single low note that rings on (the index note, two octaves down)
if wanted1(40):  # soft notes lead into the next scene (not the stolen phrase)
    save(cue1(40), fade(phrase([(0.0, "D4"), (0.5, "F4"), (1.0, "A4"), (1.5, "C5"), (2.2, "A4")], 0.4, 2.0), 0, 1.0), -6)

# Practice examples have stable names because they are replayed inside interactions.
if selected("x_s01_regolare"):
    x = silence(2.22)
    for t in (0.0, 1.0, 2.0):
        mix(x, clap(), t)
    save("x_s01_regolare", x, -4)
if selected("x_s01_confronto"):
    x = silence(2.0)
    for t in (0.0, 0.3, 1.2):
        mix(x, clap(), t)
    save("x_s01_confronto", x, -4)
if selected("x_s01_maestro"):
    save("x_s01_maestro", tac_tac_toc(), -3)
if selected("x_s01_pulsante"):
    # the index button: the piano note the child plays (docs/music: the child hears the stake instrument)
    save("x_s01_pulsante", piano(NOTE["D4"], 0.42, 0.6, damp=0.2), -8)
if selected("x_s01_ricordo"):  # the first example of the comparison: the song as it was
    save("x_s01_ricordo", song_memory(), -5)
if selected("x_s01_nota_mancante"):
    x = silence(3.2)
    intensity = 0.6
    mix(x, piano(NOTE[THEME[0][1]], 1.6, 0.6, cents=-35 * intensity, wobble=18 * intensity), THEME[0][0])
    mix(x, piano(NOTE[THEME[1][1]], 1.6, 0.6, cents=45 * intensity, sag=-90 * intensity, wobble=25 * intensity), THEME[1][0])
    save("x_s01_nota_mancante", x, -4)

# Extra beds for the regia (not cues in the script).
if selected("x_vento_alberi"):  # wind among the trees, under the Maestro talking about Sbuffo
    rustle = [random.uniform(-1, 1) for _ in range(12 * SR)]
    rustle = biquad(biquad(rustle, "bp", 2500, 0.5), "lp", 4000)
    slow = biquad([random.uniform(-1, 1) for _ in range(12 * SR)], "lp", 0.4)  # very slow, gentle swell
    peak = max(abs(v) for v in slow) or 1.0
    rustle = [r * (0.75 + 0.25 * v / peak) for r, v in zip(rustle, slow)]
    save("x_vento_alberi", fade(rustle, 1.5, 1.5), -24)
if selected("x_fogli_vento"):  # papers flying in the wind
    x = fade(biquad(load("storm", 12.0, 18.0), "lp", 1100), 0.8, 0.8)
    for t, k in ((0.4, 1), (1.3, 2), (2.1, 3), (3.4, 1), (4.6, 2)):
        mix(x, load(f"k_bookFlip{k}"), t, 1.4)
    mix(x, load("pages"), 2.6, 1.2)
    save("x_fogli_vento", x, -8)


# ---------------------------------------------------------------------------- Scenes 2 and 3
# Music comes from music/library/capitolo_01.yaml through tools/music (checked against docs/music).

SPECS = music.load_specs(__file__.rsplit("/tools/", 1)[0] + "/music/library/capitolo_01.yaml")
CHOSEN = ("grave", "acuto")
SHIFT = {"normale": 0, "grave": -12, "acuto": 12}
VOICE_DIR = __file__.rsplit("/tools/", 1)[0] + "/unity/EchoQuestTest/Assets/EchoQuest/Story/Audio/Interazioni"


def reg_note(name, register):
    """A button note in the chosen register: reg_note("D4", "grave") -> "D3"."""
    return music.shift_note(name, SHIFT[register])


def loudness(x):
    """A rough loudness proxy (RMS above 150 Hz), to match low and high notes by ear, not by peak."""
    y = biquad(x, "hp", 150)
    return math.sqrt(sum(v * v for v in y) / max(1, len(y)))


def same_loudness(sounds, target=None):
    target = target or loudness(sounds[0])
    gains = [target / (loudness(s) or 1) for s in sounds]
    return [[v * g for v in s] for s, g in zip(sounds, gains)]


def save_fixed(name, x, gain):
    """Writes without normalising, so battle stems keep the balance they were mixed with."""
    if not selected(name):
        return
    peak = max(abs(v) for v in x) * gain
    assert peak < 0.98, f"{name} would clip ({peak:.2f})"
    with wave.open(f"{OUT}/{name}.wav", "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(struct.pack(f"<{len(x)}h", *(int(s * gain * 32767) for s in x)))
    print(f"{name}.wav  {len(x) / SR:.2f}s  (fixed gain)")


def baton_kit():
    """The Maestro's real baton for TAC (claves) and TOC (block_lo): the same sound as his signature."""
    hit = fade(load("stick", 0.60, 0.90), 0.002, 0.05)
    toc = resample(fade(load("stick", 1.28, 1.58), 0.002, 0.05), 0.7)
    return {"claves": lambda vel=1.0: [v * vel for v in hit], "block_lo": lambda vel=1.0: [v * vel * 1.1 for v in toc]}


def voice_48k(name):
    """A recorded Maestro line from Audio/Interazioni, as mono 48 kHz samples."""
    import subprocess
    raw = subprocess.run(["ffmpeg", "-v", "error", "-i", f"{VOICE_DIR}/{name}.wav", "-f", "s16le", "-ac", "1", "-ar", str(SR), "-"],
                         check=True, capture_output=True).stdout
    data = array("h", raw)
    return [s / 32768 for s in data]


def key_note(register, letter="D", dur=0.45, vel=0.6):
    """What a button sounds like: a short piano note (the stake instrument, 06 §2)."""
    return piano(NOTE[reg_note(music.BUTTON_NOTES[letter], register)], dur, vel, damp=dur * 0.5)


def cue2(n):
    return f"s02_sfx_{n:02d}"


def cue3(n):
    return f"s03_sfx_{n:02d}"


# Button sounds: one per button and register (normale = before Scene 2's choice).
for letter, nome in (("D", "re"), ("F", "fa"), ("A", "la"), ("C", "do")):
    for register in ("normale",) + CHOSEN:
        name = f"x_piano_{nome}" + ("" if register == "normale" else f"_{register}")
        if selected(name):
            save(name, key_note(register, letter), -8)

if selected(cue2(1)):  # the last soft notes of Scene 1; one of them stays in the air and fades
    x = phrase([(0.0, "D4", 0.5), (0.5, "F4", 0.6)], 0.35, 1.2)
    mix(x, piano(NOTE["A4"], 5.0, 0.4), 1.0)
    save(cue2(1), fade(x, 0, 2.5), -8)
if selected(cue2(2)):  # two playful notes, F4 A4: hello (spec s2_saluto)
    save(cue2(2), music.render(SPECS["s2_saluto"], tail=1.0)[: int(1.6 * SR)], -6)
if selected(cue2(3)):  # the small mechanical noise of a key going down, very discreet
    x = mix(silence(0.45), load("k_impactWood_light_002"), 0.0, 0.25)
    mix(x, biquad(load("k_impactSoft_medium_001"), "lp", 1500), 0.02, 0.35)
    save(cue2(3), fade(x, 0, 0.1), -18)
if selected(cue2(4)):  # one piano note, D4 (the index button), left ringing on its own
    save(cue2(4), piano(NOTE["D4"], 4.5, 0.6), -6)
if selected(cue2(5)) or selected(cue2(10)) or selected(cue2(11)) or selected(cue2(12)) or selected(cue2(13)):
    # Low and high: D3 and D5, the two registers the child will choose between. Same length and the
    # same loudness by ear (not by peak), so only the pitch differs (the script's requirement).
    low, high = same_loudness([piano(NOTE["D3"], 1.8, 0.6, damp=1.3), piano(NOTE["D5"], 1.8, 0.6, damp=1.3)])
    save(cue2(5), mix(low + silence(0.9), high, 2.7), -6)
    save(cue2(10), low, -8)
    save(cue2(11), high, -8)
    confirm = [v * 0.35 for v in blip(NOTE["A5"], 0.25)]
    save(cue2(12), mix(mix(silence(0.3), confirm), piano(NOTE["D3"], 2.4, 0.6), 0.35), -6)
    save(cue2(13), mix(mix(silence(0.3), confirm), piano(NOTE["D5"], 2.4, 0.6), 0.35), -6)
if selected(cue2(6)):  # five notes up D3 A3 D4 A4 D5, then down; quarter notes at 100 BPM, same loudness
    notes = ["D3", "A3", "D4", "A4", "D5"]
    tones = same_loudness([piano(NOTE[n], 1.0, 0.55, damp=0.5) for n in notes])
    x = silence(0)
    for k, tone in enumerate(tones):
        mix(x, tone, k * BEAT)
    for k, tone in enumerate(reversed(tones)):
        mix(x, tone, (len(notes) + 1 + k) * BEAT)
    save(cue2(6), x + silence(0.3), -6)
if selected(cue2(7)) or selected("x_s02_piano") or selected("x_s02_forte"):
    # piano and forte: the same three notes D4 F4 A4, soft and then strong (about +9 dB, a brighter
    # touch, no sudden peaks)
    def trio(vel, bright):
        x = silence(0)
        for k, n in enumerate(("D4", "F4", "A4")):
            mix(x, music.synth.piano(NOTE[n], 1.0, vel, damp=0.5, bright=bright), k * BEAT)
        return x
    soft, strong = trio(0.25, 0.0), trio(0.75, 0.5)
    scale = 1 / max(abs(v) for v in strong)
    soft, strong = [v * scale for v in soft], [v * scale for v in strong]
    save_fixed("x_s02_piano", soft, 0.55)
    save_fixed("x_s02_forte", strong, 0.55)
    save_fixed(cue2(7), mix(soft + silence(1.2), strong, len(soft) / SR + 1.2), 0.55)
if selected(cue2(8)):  # a free cheerful phrase (spec s2_allegra), then the piano tries the stolen head alone: C5, A4... nothing
    x = music.render(SPECS["s2_allegra"], tail=0.6)
    mix(x, stolen_try(0.6), len(x) / SR + 0.3)
    save(cue2(8), x + silence(0.6), -5)
if selected(cue2(9)):
    save(cue2(9), tac_tac_toc(), -3)  # TAC, TAC... TOC!
for register in CHOSEN:
    # The Maestro's rhythm on the chosen piano note, with the exact times of s01_int_04 (0, 0.32, 0.95 s)
    ritmo = phrase([(0.0, reg_note("D4", register), 0.15), (0.32, reg_note("D4", register), 0.15),
                    (0.95, reg_note("D4", register), 0.35)], 0.6, 0.9)
    if selected(f"{cue2(14)}_{register}"):
        save(f"{cue2(14)}_{register}", ritmo, -4)
    if selected(f"x_s02_ritmo_{register}"):
        save(f"x_s02_ritmo_{register}", ritmo, -4)
    if selected(f"{cue2(16)}_{register}"):  # the chosen note once, then a soft timpani roll: the challenge is coming
        x = piano(NOTE[reg_note("D4", register)], 2.4, 0.6) + silence(0.6)
        roll = silence(0)
        for k in range(14):
            mix(roll, music.synth.timpani(NOTE["D2"], 0.15 + 0.5 * k / 14, 0.6), k * 0.075)
        mix(roll, music.synth.timpani(NOTE["D2"], 0.8), 14 * 0.075)
        mix(x, roll, 2.6)
        save(f"{cue2(16)}_{register}", x, -5)
if selected(cue2(15)):  # two comic notes (spec s2_comiche), slightly out of tune like the Maestro's joke
    x = phrase([(0.0, NOTE["C5"] * 1.03, 0.12), (0.3, NOTE["D4"] * 0.97, 0.3)], 0.6, 0.9)
    save(cue2(15), x, -5)

# ---- Scene 3: the first battle (docs/music/06-worked-example.md)
if selected(cue3(1)):  # the wind comes back, far away at first (the same storm as Sbuffo in the prologue)
    x = fade(biquad(load("storm", 3.0, 9.0), "lp", 700), 2.5, 1.5)
    save(cue3(1), ramp(x, 0.4, 1.0), -8)
if selected(cue3(2)):  # Sbuffo's signature: intro_sbuffo (pan flute C A D, villain slide), plus a gust
    x = music.render(SPECS["intro_sbuffo"], tail=1.5)
    mix(x, fade(biquad(load("whoosh", 0.0, 2.4), "hp", 300), 0.3, 0.8), 2.4, 0.5)
    mix(x, music.synth.slide_whistle(700, 1600, 0.9, 0.25), 2.5)
    save(cue3(2), x, -4)
kit = baton_kit() if any(selected(n) for n in (cue3(4), "x_s03_toc")) else None
if selected(cue3(4)):
    # the count-in: TAC, TAC... TOC, then "uno, due, tre... via!" on beats 1-4 of the second bar
    # (the recorded words fall at 0.04, 0.65, 1.2 and 1.8 s: on the beat at 100 BPM). Exactly two
    # bars long, so the battle loop starts on the next downbeat.
    x = music.render(SPECS["count_in"], kit=kit, tail=0.0)[: int(2 * BAR * SR)]
    mix(x, voice_48k("eq_s03_extra_02_conteggio"), BAR, 0.9)
    x = x[: int(2 * BAR * SR)]
    save_fixed(cue3(4), x, 0.8 / max(abs(v) for v in x))
if selected("x_s03_toc"):  # the turn cue: one TOC on beat 4
    save_fixed("x_s03_toc", music.render(SPECS["turn_cue"], kit=kit, tail=0.3), 0.7)
if selected("x_s03_taunt"):
    save_fixed("x_s03_taunt", music.render(SPECS["taunt_sbuffo"], tail=1.0), 0.5)

# The battle stems, mixed together: L0 groove, L1 pad, L3 hits, the calls on top. The calls must
# be clearly the loudest thing (03-backing.md: nothing competes with the call).
CALL_GAIN, STEM_GAIN = 0.55, 0.42
for register in CHOSEN:
    loop = SPECS["loop_sfida_piano_A"]
    stems = {f"x_s03_base_l0_{register}": music.render(loop, register, only_layers={"L0"}),
             f"x_s03_base_l1_{register}": music.render(loop, register, only_layers={"L1"}),
             f"x_s03_base_l3_{register}": music.render(loop, register, only_layers={"L3"})}
    if any(selected(n) for n in stems):
        full = [a + b + c for a, b, c in zip(*stems.values())]
        g = STEM_GAIN * 0.95 / max(abs(v) for v in full)
        for name, x in stems.items():
            save_fixed(name, x, g)
    for sid, name in (("sfida_piano_r1", "r1"), ("sfida_piano_r2", "r2"), ("sfida_piano_r3", "r3")):
        if selected(f"x_s03_{name}_{register}"):
            x = music.render(SPECS[sid], register, tail=1.2)
            save_fixed(f"x_s03_{name}_{register}", x, CALL_GAIN / max(abs(v) for v in x))
    if selected(f"x_s03_ok_{register}"):
        x = music.render(SPECS["sting_ok"], register, tail=1.0)
        save_fixed(f"x_s03_ok_{register}", x, 0.45 / max(abs(v) for v in x))
    if selected(f"x_s03_notyet_{register}"):
        x = music.render(SPECS["sting_notyet"], register, tail=0.8)
        save_fixed(f"x_s03_notyet_{register}", x, 0.3 / max(abs(v) for v in x))
    if selected(f"{cue3(3)}_{register}"):  # the first phrase alone, without the backing: TA... TA-TA
        save(f"{cue3(3)}_{register}", music.render(SPECS["sfida_piano_r1"], register, tail=1.2), -4)
    if selected(f"{cue3(5)}_{register}"):  # victory: the fanfare, then the reprise with the F back (centre F)
        x = music.render(SPECS["fanfare_win_piano"], tail=0.0)[: int(2 * BAR * SR)]
        mix(x, music.render(SPECS["reprise_piano"], register, tail=2.5), 2 * BAR)
        save(f"{cue3(5)}_{register}", fade(x, 0, 1.5), -3)
if selected(cue3(6)):  # Sbuffo goes away; his three notes come back softly, almost kind (C6 A5 D5)
    x = fade(biquad(load("whoosh", 0.0, 3.0), "hp", 250), 0.1, 1.5)
    tune = silence(0)
    for t, n, d in ((0.0, "C6", 0.3), (0.3, "A5", 0.3), (0.6, "D5", 0.9)):
        mix(tune, music.synth.pan_flute(NOTE[n], d, 0.35), t)
    mix(x, biquad(tune, "lp", 5000), 1.6, 0.9)
    save(cue3(6), fade(x, 0, 0.8), -6)


# ---- Scene 4: the first note is back
def cue4(n):
    return f"s04_sfx_{n:02d}"


for register in CHOSEN:
    if selected(f"{cue4(1)}_{register}"):  # the recovered melody, complete: the reprise without Sbuffo
        x = music.render(SPECS["reprise_piano"], register, tail=2.0, only_layers={"L0", "L1", "L2"})
        save(f"{cue4(1)}_{register}", fade(x, 0, 1.5), -4)
    if selected(f"x_s04_libero_{register}"):  # free play: piano accompaniment, bell, soft pulse; the child adds notes
        x = music.render(SPECS["s4_libero"], register, tail=2.0)
        save_fixed(f"x_s04_libero_{register}", fade(x, 0, 1.0), 0.42 / max(abs(v) for v in x))
if selected(cue4(2)):  # a light bell joins the piano for a moment
    save(cue4(2), music.render(SPECS["s4_campanella"], tail=1.5), -8)
if selected(cue4(3)):  # a small comic piano fanfare
    save(cue4(3), music.render(SPECS["s4_fanfara_comica"], tail=1.2), -4)
if selected(cue4(4)):  # applause: regular, then too fast, then out of time and it stops
    times, t, gap = [], 0.0, 0.32
    for k in range(18):
        times.append(t)
        gap = gap * (0.86 if k > 5 else 1.0)
        t += max(0.07, gap) + (random.uniform(-0.04, 0.06) if k > 11 else 0)
    x = silence(t + 0.6)
    for k, tt in enumerate(times):
        mix(x, clap(), tt, 0.9 if k < 12 else 0.7)
    save(cue4(4), x, -6)
if selected(cue4(5)):  # a light breath of wind moves in the room again
    save(cue4(5), fade(biquad(load("storm", 6.0, 10.0), "lp", 900), 1.2, 1.5), -10)
if selected(cue4(6)):  # a long blow, then a small funny puff of air
    x = fade(biquad(load("whoosh", 0.0, 2.6), "hp", 200), 0.2, 0.6)
    puff = [random.uniform(-1, 1) * math.exp(-i / (0.04 * SR)) for i in range(int(0.25 * SR))]
    mix(x, biquad(puff, "bp", 700, 1.2), 2.7, 1.2)
    mix(x, music.synth.slide_whistle(900, 450, 0.35, 0.25), 2.75)
    save(cue4(6), x + silence(0.3), -6)
if selected(cue4(7)):  # the wind goes away, leaving a whistle that slowly fades
    x = fade(biquad(load("whoosh", 0.0, 3.0), "hp", 250), 0.1, 1.5)
    mix(x, fade(whistle(2.4, 1500, 900), 0.2, 1.6), 0.8, 0.25)
    save(cue4(7), x + silence(0.4), -7)
if selected(cue4(8)):
    save(cue4(8), tac_tac_toc(), -3)  # TAC, TAC... TOC!


# ---- Scene 5: two sounds, two roads (the tamburo on the left, the campanella on the right)
def cue5(n):
    return f"s05_sfx_{n:02d}"


for register in CHOSEN:
    if selected(f"{cue5(1)}_{register}"):  # the piano goes on playing softly, then fades: the Maestro stops to listen
        x = music.render(SPECS["reprise_piano"], register, tail=1.5, only_layers={"L1", "L2"})[: int(2 * BAR * SR)]
        save(f"{cue5(1)}_{register}", fade(x + silence(1.0), 0, 2.4), -10)
if selected(cue5(2)) or selected(cue5(4)):  # Bum... Bum...: two deep strokes (s05_sfx_04 repeats it as the index's sound)
    drum = music.render(SPECS["s5_tamburo"], tail=1.8)
    save(cue5(2), drum, -4)
    save(cue5(4), drum, -4)
if selected(cue5(3)) or selected(cue5(5)):  # a clear bell that keeps ringing (s05_sfx_05 repeats it as the middle's sound)
    bell = music.render(SPECS["s4_campanella"], tail=2.5)
    save(cue5(3), bell, -6)
    save(cue5(5), bell, -6)
if selected(cue5(6)):  # branch A: the drum calls from further and further away (the regia also moves it)
    x = music.render(SPECS["s5_tamburo_lontano"], tail=1.5)
    far = biquad(x, "lp", 500)
    n = len(x)
    x = [(a * (1 - i / n) + b * (i / n)) * (1.0 - 0.8 * i / n) for i, (a, b) in enumerate(zip(x, far))]
    save(cue5(6), x, -4)
if selected(cue5(7)):  # branch B: three tinkles, the campanella's A-C-A
    save(cue5(7), music.render(SPECS["s5_tintinnii"], tail=2.0), -6)


# ---- Epilogue
def cue_e(n):
    return f"e00_sfx_{n:02d}"


kit = baton_kit() if any(selected(f"{n}_{r}") for n in (cue_e(1), "x_e00_eco") for r in CHOSEN) else None
for register in CHOSEN:
    if selected(f"{cue_e(1)}_{register}"):  # the recovered melody, slow, the Maestro tapping the beat on his baton
        x = music.render(SPECS["e0_ripresa_lenta"], register, kit=kit, tail=2.5)
        save(f"{cue_e(1)}_{register}", fade(x, 0, 2.0), -5)
    if selected(f"x_e00_eco_{register}"):
        # one exchange, nothing counted: groove, call, call + TOC, the child's bar, the piano again.
        # Kept below the button notes (the child's presses must be the clearest thing in bar 4).
        x = music.render(SPECS["e0_eco"], register, kit=kit, tail=1.5)
        save_fixed(f"x_e00_eco_{register}", fade(x, 0, 1.0), 0.5 / max(abs(v) for v in x))
    if selected(f"{cue_e(5)}_{register}"):  # the Maestro's little laugh, then the whole song on the piano, fading out
        laugh = voice_48k("eq_e00_extra_01_risata")
        x = laugh + silence(0.4)
        song = music.render(SPECS["e0_tema_finale"], register, tail=2.0)
        start, n = int(4 * BAR * SR), len(song)
        song = [v * (1.0 if i < start else max(0.0, 1 - (i - start) / (n - start)) ** 1.5) for i, v in enumerate(song)]
        mix(x, song, len(x) / SR)
        save(f"{cue_e(5)}_{register}", x, -4)
if selected(cue_e(2)):  # a small far whistle of the wind, almost musical: Sbuffo's C6 A5, very soft
    x = fade(biquad(load("whoosh", 0.0, 2.6), "hp", 400), 0.4, 1.2)
    x = [v * 0.5 for v in x]
    mix(x, music.synth.slide_whistle(1100, NOTE["C6"], 0.5, 0.18), 0.6)
    mix(x, biquad(music.synth.pan_flute(NOTE["C6"], 0.35, 0.3), "lp", 4000), 1.1)
    mix(x, biquad(music.synth.pan_flute(NOTE["A5"], 0.9, 0.3), "lp", 4000), 1.45)
    save(cue_e(2), fade(x, 0, 0.8), -9)
if selected(cue_e(3)):
    save(cue_e(3), tac_tac_toc(), -3)  # TAC, TAC... TOC!
if selected(cue_e(4)):  # the baton falls again
    save(cue_e(4), stick_drop(1), -6)


# ============================================================================ Chapters 2 and 3
# The tamburo and the campanella (docs/capitoli-2-3.md). Their instruments are real recordings from
# the Versilian Community Sample Library (VCSL, CC0, github.com/sgossner/VCSL), converted like the
# Sonniss files and prefixed v_ in <mono_dir>:
#   timpani    Membranophones/Struck Membranophones/Timpani 1/Hit/Timpani{1..5}_Hit_v3_*_Sum.wav
#   hand bells Idiophones/Struck Idiophones/Hand Chimes/sus_{D4,F#4,A4,C5}_r01_main.wav (they sound an
#              octave above their name: sus_D4 is D5)
# Each sample is tuned to the button pitch by resampling: the tamburo plays the buttons an octave down
# (D3-C4), the campanella an octave up (D5-C6), as 04-battle-cues.md asks for these characters.
SPECS_T = music.load_specs(__file__.rsplit("/tools/", 1)[0] + "/music/library/capitolo_02_tamburo.yaml")
SPECS_C = music.load_specs(__file__.rsplit("/tools/", 1)[0] + "/music/library/capitolo_03_campanella.yaml")
REG_T, REG_C = "grave", "acuto"
# sample, principal frequency measured from its spectrum (Hz)
TIMPANI = [("v_Timpani1_Hit_v3_rr3_Sum", 90.0), ("v_Timpani2_Hit_v3_rr1_Sum", 122.0), ("v_Timpani3_Hit_v3_rr1_Sum", 142.0),
           ("v_Timpani4_Hit_v3_rr1_Sum", 169.0), ("v_Timpani5_Hit_v3_rr1_Sum", 191.0)]
CHIMES = {"D5": ("v_sus_D4_r01_main", NOTE["D5"]), "F5": ("v_sus_Fs4_r01_main", NOTE["F#5"]),
          "A5": ("v_sus_A4_r01_main", NOTE["A5"]), "C6": ("v_sus_C5_r01_main", NOTE["C6"])}


def onset_trimmed(x, threshold=0.02):
    """Drops the silence before the attack, so a note starts exactly where it is placed."""
    peak = max(abs(v) for v in x) or 1.0
    for i, v in enumerate(x):
        if abs(v) > threshold * peak:
            return x[max(0, i - 48):]
    return x


def pitched(x, ratio):
    """Linear-interpolation resampling: ratio > 1 plays higher and shorter."""
    n = int((len(x) - 1) / ratio)
    out = []
    for i in range(n):
        p = i * ratio
        k = int(p)
        out.append(x[k] + (x[k + 1] - x[k]) * (p - k))
    return out


def tamburo(note, dur=1.6, vel=0.8):
    """A real timpani stroke tuned to `note`, from the sample whose pitch is closest."""
    f = NOTE[note]
    name, base = min(TIMPANI, key=lambda s: abs(math.log(f / s[1])))
    x = pitched(onset_trimmed(cached(load, name)), f / base)
    x = x[: int((dur + 0.8) * SR)]
    return [v * vel for v in fade(x, 0.0, 0.8)]


def campana(note, dur=3.0, vel=0.7, damp=None):
    """A real hand bell tuned to `note`. `damp` stops it with the hand after that many seconds."""
    name, base = CHIMES[note]
    x = pitched(onset_trimmed(cached(load, name)), NOTE[note] / base)
    stop = damp if damp is not None else dur
    x = x[: int((stop + (0.06 if damp is not None else 0.6)) * SR)]
    return [v * vel for v in fade(x, 0.0, 0.06 if damp is not None else 0.6)]


def call_with(spec, register, instrument, tail=1.5, vel=0.8):
    """Plays a call spec (its button onsets) on a sampled instrument, in the given register."""
    bar_len = 60.0 / spec.get("tempo", 100) * (int(spec.get("meter", "4/4").split("/")[0]))
    length = len(spec["bars"]) * bar_len
    onsets = music.call_onsets(spec, register)
    out = silence(length + tail)
    for k, (t, button) in enumerate(onsets):
        end = onsets[k + 1][0] if k + 1 < len(onsets) else length
        note = reg_note(music.BUTTON_NOTES["DFAC"[button - 1]], register)
        mix(out, instrument(note, max(0.3, end - t), vel), t)
    return out


def hits(times, note="D3", vel=0.8, dur=1.0):
    out = silence(max(times) + dur + 1.0)
    for t in times:
        mix(out, tamburo(note, dur, vel), t)
    return out


SONG_PIECES = ["C - A - F - - .", "D - . . F D - .", "A A C - A A F -", "C - - - A - - .",
               "C - A - F - - .", "D - . . F D - .", "C A C A C - A -", "A - C - F - - -"]
SONG_BASS = ["F2 - - - C2 - - -", "D2 - - - A1 - - -", "Bb1 - - - F2 - - -", "C2 - - - G1 - - -",
             "F2 - - - C2 - - -", "D2 - - - A1 - - -", "G1 - - - D2 - - -", "F2 - - - - - - -"]
SONG_PAD = ["F2+C3+A3 - - - - - - -", "C3+F3+A3 - - - - - - -", "Bb2+D3+F3 - - - - - - -", "C3+F3+G3 - - - - - - -",
            "F2+C3+A3 - - - - - - -", "C3+F3+A3 - - - - - - -", "G2+Bb2+D3+F3 - - - - - - -", "F2+C3+A3 - - - - - - -"]
SONG_PAD_GRAVE = ["F2+A2 - - - - - - -", "D2+A2 - - - - - - -", "Bb1+F2 - - - - - - -", "C2+G2 - - - - - - -",
                  "F2+A2 - - - - - - -", "D2+A2 - - - - - - -", "G1+D2 - - - - - - -", "F2+A2 - - - - - - -"]


def song(bars, register, layers=("L0", "L1", "L2"), tail=2.0):
    """Some bars of the orchestra's song (0-based indices), piano melody in the chosen register with
    bass and pad (the reprise: "the song up to the pieces recovered so far", 04)."""
    pick = lambda rows: [rows[b] for b in bars]
    spec = {"id": "song_" + "_".join(map(str, bars)), "role": "loop", "loop": False, "input": "locked", "centre": "F",
            "tempo": 100, "steps_per_bar": 8,
            "tracks": {"piano": {"instrument": "piano", "layer": "L2", "buttons": True, "bars": pick(SONG_PIECES)},
                       "bass": {"instrument": "acoustic_bass", "layer": "L0", "bars": pick(SONG_BASS)},
                       "pad": {"instrument": "strings", "layer": "L1", "register": ["normale", "acuto"], "bars": pick(SONG_PAD)},
                       "pad_grave": {"instrument": "strings", "layer": "L1", "register": "grave", "bars": pick(SONG_PAD_GRAVE)}}}
    return music.render(spec, register, tail=tail, only_layers=set(layers))


def gallop_on_drums():
    """The gallop (song bar 3) on the tamburo, as a layer to mix over song()."""
    return call_with(SPECS_T["t_galoppo"], REG_T, tamburo, tail=1.0, vel=0.6)


def gust(length=0.7, gain=1.0):
    x = fade(biquad(load("whoosh", 0.2, 0.2 + length), "hp", 250), 0.05, 0.25)
    return [v * gain for v in x]


def stems(spec, register, layers, prefix):
    """Battle/practice loop stems, mixed together with one gain like Scene 3's."""
    parts = {f"{prefix}_{l.lower()}": music.render(spec, register, only_layers={l}) for l in layers}
    if any(selected(n) for n in parts):
        full = [sum(v) for v in zip(*parts.values())]
        g = 0.42 * 0.95 / max(abs(v) for v in full)
        for name, x in parts.items():
            save_fixed(name, x, g)


def save_call(name, x, gain=0.55):
    save_fixed(name, x, gain / max(abs(v) for v in x))


# ---- Chapter 2: the tamburo
def ct(sec, n):
    return f"t{sec:02d}_sfx_{n:02d}"


for letter, nome in (("D", "re"), ("F", "fa"), ("A", "la"), ("C", "do")):  # the drums under the four fingers
    if selected(f"x_t01_tamburo_{nome}"):
        save(f"x_t01_tamburo_{nome}", tamburo(reg_note(music.BUTTON_NOTES[letter], REG_T), 0.9, 0.8), -8)
if selected("x_t01_marcia"):  # free play: six bars of the practice march (bordun and soft marcia) under the child's drums
    loop = music.render(SPECS_T["loop_allenamento_tamburo"], REG_T, only_layers={"L0"})
    x = (loop + loop)[: int(6 * BAR * SR)] + silence(0.8)
    save_fixed("x_t01_marcia", fade(x, 0, 1.6), 0.42 / max(abs(v) for v in x))
for register in CHOSEN:
    if selected(f"{ct(0, 1)}_{register}"):  # the piano's recovered piece, then a drum that cannot keep its step
        x = song([0, 1], register, layers=("L2",), tail=0.5)
        x = [v * 0.6 for v in x]
        mix(x, hits([0.0, 0.9, 1.15, 2.9, 3.35], "D3", 0.7), len(x) / SR + 0.3)
        save(f"{ct(0, 1)}_{register}", x, -6)
    if selected(f"{ct(5, 1)}_{register}"):  # fanfare, then the song up to the gallop, Sbuffo's cell softly at the end
        x = music.render(SPECS["fanfare_win_piano"], tail=0.0)[: int(2 * BAR * SR)]
        rep = song([0, 1, 2, 3], register, tail=2.5)
        mix(rep, gallop_on_drums(), 2 * BAR)
        for t, n, d in ((3 * BAR + 4 * EIGHTH, "C6", 0.3), (3 * BAR + 5 * EIGHTH, "A5", 0.3), (3 * BAR + 6 * EIGHTH, "D5", 0.9)):
            mix(rep, music.synth.pan_flute(NOTE[n], d, 0.3), t)
        mix(x, rep, 2 * BAR)
        save(f"{ct(5, 1)}_{register}", fade(x, 0, 1.5), -3)
    if selected(f"t99_sfx_01_{register}"):  # a calm, regular drum under the song up to the gallop
        x = song([0, 1, 2, 3], register, tail=2.5)
        mix(x, hits([k * BEAT for k in range(16)], "D3", 0.35, 0.5), 0.0)
        mix(x, gallop_on_drums(), 2 * BAR)
        save(f"t99_sfx_01_{register}", fade(x, 0, 2.0), -5)
if selected(ct(0, 2)):  # footsteps to the left, the drum getting closer
    x = steps([0.55 * k for k in range(8)], vel=0.9)
    mix(x, hits([1.6, 3.1], "D3", 0.25), 0.0)
    save(ct(0, 2), fade(x, 0, 0.5), -5)
if selected(ct(1, 1)):  # one deep stroke, left to ring
    save(ct(1, 1), tamburo("D3", 3.5, 0.9), -4)
regular = hits([k * BEAT for k in range(4)], "D3", 0.7)
stumbling = hits([0.0, 0.45, 1.5, 1.9], "D3", 0.7)  # the second stroke early, the third late
if selected(ct(1, 2)):
    save(ct(1, 2), hits([k * BEAT for k in range(4)], "D3", 0.5), -6)  # four soft strokes, a heartbeat
if selected("x_t01_regolare"):
    save("x_t01_regolare", regular, -5)
if selected("x_t01_inciampa"):
    save("x_t01_inciampa", stumbling, -5)
if selected(ct(1, 3)):
    save(ct(1, 3), regular + silence(1.0) + stumbling, -5)
if selected(ct(2, 1)):
    save(ct(2, 1), hits([k * BEAT for k in range(4)], "D3", 0.7), -5)
tempo_hits = {bpm: hits([k * 60.0 / bpm for k in range(4)], "D3", 0.7) for bpm in (80, 100, 130)}
for bpm, x in tempo_hits.items():
    if selected(f"x_t02_tempo_{bpm}"):
        save(f"x_t02_tempo_{bpm}", x, -5)
if selected(ct(2, 2)):
    save(ct(2, 2), tempo_hits[100] + silence(1.0) + tempo_hits[130], -5)
if selected(ct(2, 3)):
    save(ct(2, 3), tempo_hits[100] + silence(1.0) + tempo_hits[80], -5)
if selected("x_t02_battito"):  # keep the beat: the count-in bar, then loop_battito (bars 3-4 thin to the shaker)
    kit = baton_kit()
    x = music.render(SPECS["count_in"], kit=kit, tail=0.0)[: int(BAR * SR)]
    x += music.render(SPECS_T["loop_battito"], REG_T, tail=0.5)
    save_fixed("x_t02_battito", x, 0.5 / max(abs(v) for v in x))
def counted_words():
    """The Maestro's "Uno. Due. Tre. Quattro." cut word by word, so each word can sit on a beat. Words are
    found by their loudness; each one starts on its onset."""
    v = voice_48k("eq_t02_extra_01_conta")
    frame = int(0.01 * SR)
    env = [max(abs(s) for s in v[i:i + frame]) for i in range(0, len(v), frame)]
    loud = [e > max(env) * 0.06 for e in env]
    words, start, quiet, end = [], None, 0, 0
    for i, on in enumerate(loud + [False] * 12):
        if on:
            start, quiet, end = (i if start is None else start), 0, i
        elif start is not None:
            quiet += 1
            if quiet >= 12:
                words.append((start * frame, (end + 1) * frame))
                start = None
    assert len(words) == 4, f"expected four counted words, found {len(words)}"
    return [fade(v[a:b + int(0.08 * SR)], 0.005, 0.06) for a, b in words]


if any(selected(f"x_t02_conta_{k}") for k in range(1, 5)):  # the help after a missed "keep the beat": one word per beat in Unity
    for k, word in enumerate(counted_words(), 1):
        save_fixed(f"x_t02_conta_{k}", word, 1.0)
if selected(ct(3, 1)):  # memory of the gallop (song bars 3-4, drum and piano), then today's drum stumbling
    x = song([2, 3], "normale", tail=1.0)
    mix(x, gallop_on_drums(), 0.0)
    x = memory(x) + silence(0.8)
    mix(x, hits([0.0, 0.3], "A3", 0.7) + silence(0.6), len(x) / SR)
    mix(x, tamburo("C4", 0.4, 0.4), len(x) / SR - 0.5)
    save(ct(3, 1), x + silence(0.5), -5)
for sid in ("t_mela", "t_colibri", "t_tartaruga", "t_due_tamburi", "t_colibri_doppio", "t_galoppo", "t_habanera"):
    name = "x_t03_" + sid[2:] if sid in ("t_mela", "t_colibri", "t_tartaruga", "t_due_tamburi") else "x_t04_" + sid[2:]
    if selected(name):
        save_call(name, call_with(SPECS_T[sid], REG_T, tamburo))
if selected("x_t04_galoppo_sbuffo") or selected(ct(4, 2)) or selected(ct(4, 3)):
    backwards = music.render(SPECS_T["t_galoppo_sbuffo"], "normale", tail=1.0)
    save_call("x_t04_galoppo_sbuffo", backwards)
    save(ct(4, 2), backwards, -4)
    x = call_with(SPECS_T["t_galoppo"], REG_T, tamburo) + silence(0.8)
    save(ct(4, 3), mix(x, backwards, len(x) / SR) + silence(0.3), -4)
if selected(ct(4, 1)):  # the wind comes back from far away, then Sbuffo's signature
    x = fade(biquad(load("storm", 3.0, 7.0), "lp", 700), 2.0, 1.0)
    sig = music.render(SPECS["intro_sbuffo"], tail=1.5)
    mix(sig, fade(biquad(load("whoosh", 0.0, 2.4), "hp", 300), 0.3, 0.8), 2.4, 0.5)
    mix(x, sig, 3.0)
    save(ct(4, 1), x, -4)
if selected(ct(4, 4)):  # the count-in, two bars exactly, as in Scene 3
    kit = baton_kit()
    x = music.render(SPECS["count_in"], kit=kit, tail=0.0)[: int(2 * BAR * SR)]
    mix(x, voice_48k("eq_s03_extra_02_conteggio"), BAR, 0.9)
    x = x[: int(2 * BAR * SR)]
    save_fixed(ct(4, 4), x, 0.8 / max(abs(v) for v in x))
if selected(ct(4, 5)):  # the bass walks from D down to F on the exchange boundary: the music comes home
    x = silence(BAR + 1.0)
    for k, n in enumerate(("D2", "C2", "A1", "F1")):
        mix(x, music.synth.pizz(NOTE[n], 0.5, 0.8), k * BEAT)
    mix(x, tamburo("F3", 1.2, 0.8), 0.0)
    save(ct(4, 5), x, -5)
stems(SPECS_T["loop_allenamento_tamburo"], REG_T, ("L0",), "x_t03_allenamento")
stems(SPECS_T["loop_sfida_tamburo_A"], REG_T, ("L0", "L1", "L3"), "x_t04_base_a")
stems(SPECS_T["loop_sfida_tamburo_B"], REG_T, ("L0", "L1", "L3"), "x_t04_base_b")
stems(SPECS_T["loop_sfida_tamburo_casa"], REG_T, ("L0", "L1"), "x_t04_base_casa")
if selected("x_t05_colpo"):  # the final stroke: the biggest drum, with a low timpani under it
    x = mix(tamburo("D3", 3.0, 1.0), tamburo("A3", 2.0, 0.5), 0.0)
    mix(x, music.synth.timpani(NOTE["D2"], 1.0), 0.0, 0.8)
    save("x_t05_colpo", x, -2)
if selected("x_t05_soffio"):  # one of Sbuffo's gusts, for the scene where he blows in time with the child
    save("x_t05_soffio", gust(0.7), -8)
if selected("x_t05_bacchetta"):  # the final stroke's count: the baton and "uno, due, tre" on beats 1-3; beat 4 is the child's
    kit, words = baton_kit(), counted_words()
    x = silence(2.6)
    for k in range(3):
        mix(x, kit["claves"](0.9), k * BEAT)
        mix(x, words[k], k * BEAT)
    save_fixed("x_t05_bacchetta", x, 0.8 / max(abs(v) for v in x))
if selected("x_t05_libero"):  # the bed of "Sbuffo blows in time": a soft, distant wind for sixteen seconds
    x = fade(biquad(load("storm", 3.0, 19.0), "lp", 500), 1.5, 2.5)
    save("x_t05_libero", x, -20)
if selected(ct(5, 2)):  # the wind goes away in gusts, never on a beat
    x = silence(6.5)
    for t, g in ((0.0, 1.0), (1.3, 0.8), (1.75, 0.8), (3.9, 0.5)):
        mix(x, gust(0.9, g), t)
    save(ct(5, 2), fade(x, 0, 1.0), -7)


# ---- Chapter 3: the campanella
def cc(sec, n):
    return f"c{sec:02d}_sfx_{n:02d}"


for letter, nome in (("D", "re"), ("F", "fa"), ("A", "la"), ("C", "do")):  # the bells under the four fingers
    if selected(f"x_c01_campanella_{nome}"):
        save(f"x_c01_campanella_{nome}", campana(reg_note(music.BUTTON_NOTES[letter], REG_C), 1.6, 0.8), -10)
question = call_with(SPECS_C["c_domanda_rubata"], REG_C, campana, tail=2.0) if any(
    selected(n) for n in (cc(0, 1) + "_grave", cc(0, 1) + "_acuto", cc(0, 2))) else None
for register in CHOSEN:
    if selected(f"{cc(0, 1)}_{register}"):  # the piano's piece, then the bell's question twice, and no answer
        x = [v * 0.6 for v in song([0, 1], register, layers=("L2",), tail=0.5)]
        q = question + silence(1.2)
        x = x + silence(0.4) + q + question
        save(f"{cc(0, 1)}_{register}", x, -6)
    if selected(f"{cc(5, 3)}_{register}"):  # fanfare, then the song with the bell's question and, at last, its answer
        x = music.render(SPECS["fanfare_win_piano"], tail=0.0)[: int(2 * BAR * SR)]
        rep = song([0, 1, 6, 7], register, tail=2.5)
        mix(rep, call_with(SPECS_C["c_domanda_rubata"], REG_C, campana, vel=0.5), 2 * BAR)
        mix(rep, call_with(SPECS_C["c_risposta_rubata"], REG_C, campana, vel=0.5), 3 * BAR)
        mix(x, rep, 2 * BAR)
        save(f"{cc(5, 3)}_{register}", fade(x, 0, 1.5), -3)
    if selected(f"c99_sfx_01_{register}"):
        x = song([0, 1, 6, 7], register, tail=2.5)
        mix(x, call_with(SPECS_C["c_domanda_rubata"], REG_C, campana, vel=0.5), 2 * BAR)
        mix(x, call_with(SPECS_C["c_risposta_rubata"], REG_C, campana, vel=0.5), 3 * BAR)
        save(f"c99_sfx_01_{register}", fade(x, 0, 2.0), -5)
if selected(cc(0, 2)):  # the question once more, softer, almost sad
    save(cc(0, 2), [v * 0.6 for v in question], -10)
if selected(cc(0, 3)):  # footsteps to the right, a light tinkle getting closer
    x = steps([0.55 * k for k in range(8)], vel=0.9)
    mix(x, campana("A5", 1.5, 0.25), 1.4)
    mix(x, campana("C6", 1.5, 0.35), 3.2)
    save(cc(0, 3), fade(x, 0, 0.6), -5)
long_ring, short_ring = campana("A5", 4.0, 0.8), campana("A5", 4.0, 0.8, damp=0.25)
if selected(cc(1, 1)):
    save(cc(1, 1), long_ring, -5)
if selected("x_c01_lungo"):
    save("x_c01_lungo", long_ring, -5)
if selected("x_c01_corto"):
    save("x_c01_corto", short_ring + silence(0.5), -5)
if selected(cc(1, 2)):
    save(cc(1, 2), long_ring + silence(0.8) + short_ring + silence(0.5), -5)
if selected("x_c02_rintocco"):  # one ring, placed left or right by the regia
    save("x_c02_rintocco", campana("C6", 2.0, 0.8), -5)
if selected("x_c02_movimento"):  # three rings while the bell moves (the regia moves it and stops it on the left)
    x = silence(3.5)
    for t in (0.0, 0.9, 1.8):
        mix(x, campana("A5", 1.4, 0.7), t)
    save("x_c02_movimento", x, -5)
for count, name in ((2, "x_c02_due"), (3, "x_c02_tre")):  # rings to count, well apart
    if selected(name):
        x = silence(0.9 * count + 1.6)
        for k in range(count):
            mix(x, campana("C6", 1.2, 0.7, damp=0.5), 0.9 * k)
        save(name, x, -5)
for sid in ("c_ciao", "c_motto", "c_altalena", "c_domanda", "c_risposta", "c_r1", "c_r2", "c_domanda_rubata",
            "c_risposta_rubata", "c_domanda_libera", "c_ninna_nanna"):
    scene = 3 if sid in ("c_ciao", "c_motto", "c_altalena") else 4 if sid in ("c_domanda", "c_risposta") else 5
    name = f"x_c{scene:02d}_{sid[2:]}"
    if selected(name):
        save_call(name, call_with(SPECS_C[sid], REG_C, campana))
if selected(cc(3, 1)):  # the bell's motto (open), a pause, then the swing (closed)
    x = call_with(SPECS_C["c_motto"], REG_C, campana) + silence(0.6)
    save(cc(3, 1), x + call_with(SPECS_C["c_altalena"], REG_C, campana), -5)
if selected(cc(4, 1)):  # memory of song bars 7-8: the question and its answer, bell and piano
    x = song([6, 7], "normale", tail=1.0)
    mix(x, call_with(SPECS_C["c_domanda_rubata"], REG_C, campana, vel=0.6), 0.0)
    mix(x, call_with(SPECS_C["c_risposta_rubata"], REG_C, campana, vel=0.6), BAR)
    save(cc(4, 1), memory(x), -5)
if selected(cc(5, 1)):  # the wind arrives from the right and goes round the listener, then Sbuffo's signature
    x = fade(biquad(load("whoosh", 0.0, 3.0), "hp", 200), 0.4, 1.0)
    sig = music.render(SPECS["intro_sbuffo"], tail=1.5)
    mix(x, sig, 2.6)
    save(cc(5, 1), x, -4)
if selected(cc(5, 2)):  # the count-in, two bars exactly
    kit = baton_kit()
    x = music.render(SPECS["count_in"], kit=kit, tail=0.0)[: int(2 * BAR * SR)]
    mix(x, voice_48k("eq_s03_extra_02_conteggio"), BAR, 0.9)
    x = x[: int(2 * BAR * SR)]
    save_fixed(cc(5, 2), x, 0.8 / max(abs(v) for v in x))
if selected(cc(5, 4)) or selected("x_c05_domanda_sbuffo"):  # Sbuffo's small question on his pan flute: C A D
    x = music.render(SPECS_C["c_domanda_sbuffo"], "normale", tail=1.5)
    save(cc(5, 4), [v * 0.7 for v in x], -8)
    save_call("x_c05_domanda_sbuffo", x)
if selected(cc(5, 5)):  # the wind leaves softly; its last whistle is almost a note (C6)
    x = fade(biquad(load("whoosh", 0.0, 3.0), "hp", 250), 0.1, 1.5)
    mix(x, music.synth.slide_whistle(1300, NOTE["C6"], 0.4, 0.15), 1.6)
    mix(x, biquad(music.synth.pan_flute(NOTE["C6"], 0.8, 0.25), "lp", 4000), 2.0)
    save(cc(5, 5), fade(x, 0, 0.8), -8)
if selected("x_c05_rintocco_finale"):  # the big final ring: two bells an octave apart, very long
    x = mix(campana("C6", 5.0, 1.0), campana("F5", 5.0, 0.6), 0.0)
    save("x_c05_rintocco_finale", x, -3)
stems(SPECS_C["loop_allenamento_campanella"], REG_C, ("L0",), "x_c03_allenamento")
stems(SPECS_C["loop_domande_campanella"], REG_C, ("L0", "L1"), "x_c05_base")
if selected("x_c05_rintocco"):  # the finisher bed: three counted bars on the baton, the bell struck on bar 4
    kit = baton_kit()
    save_fixed("x_c05_rintocco", (x := music.render(SPECS_C["loop_rintocco"], REG_C, kit=kit, tail=0.5)),
               0.5 / max(abs(v) for v in x))
if selected("x_c01_accordo"):  # free play on the bells: a soft F chord (strings) over the practice bordun, 6 bars
    bed = {"id": "c01_accordo", "role": "loop", "loop": False, "input": "open", "centre": "F", "tempo": 100, "steps_per_bar": 8,
           "tracks": {"pad": {"instrument": "strings", "layer": "L1", "bars": ["F3+A3+C4 - - - - - - -"] * 6},
                      "bordun": {"instrument": "pizzicato_strings", "layer": "L0", "gain": 0.5,
                                 "bars": ["F2+C3 - - - F2+C3 - - -"] * 5 + ["F2+C3 - - - - - - -"]}}}
    save("x_c01_accordo", fade(music.render(bed, REG_C, tail=2.5), 0.8, 2.5), -12)
for name in ("x_c02_sinistra", "x_c02_destra"):  # the same ring, placed left or right by the regia
    if selected(name):
        save(name, campana("C6", 2.0, 0.8), -5)
if selected("x_c05_bacchetta"):  # the final ring's count, as the drum's: baton and "uno, due, tre" on beats 1-3, the bass under it
    kit, words = baton_kit(), counted_words()
    x = silence(2.6)
    for k, n in enumerate(("F2", "C3", "F2")):
        mix(x, kit["claves"](0.9), k * BEAT)
        mix(x, words[k], k * BEAT)
        mix(x, music.synth.pizz(NOTE[n], 0.5, 0.5), k * BEAT)
    save_fixed("x_c05_bacchetta", x, 0.8 / max(abs(v) for v in x))
if selected("x_c05_silenzio_l0"):  # the "loop" of the answer to Sbuffo: four silent bars at 100 bpm, the grid only
    save_fixed("x_c05_silenzio_l0", silence(4 * BAR), 1.0)
if selected("x_c05_rintocco_casa"):  # the bell that joins an answer ending at home: a soft F5 with C6
    save("x_c05_rintocco_casa", mix(campana("F5", 3.0, 0.7), campana("C6", 3.0, 0.3), 0.0), -8)
for half, bars in (("a", slice(0, 2)), ("b", slice(2, 4))):  # the lullaby in two pieces of two bars (3/4)
    name = f"x_c05_ninna_{half}"
    if selected(name):
        spec = dict(SPECS_C["c_ninna_nanna"], id=f"c_ninna_nanna_{half}", bars=SPECS_C["c_ninna_nanna"]["bars"][bars])
        save_call(name, call_with(spec, REG_C, campana))
stems(SPECS_C["loop_ninna_nanna"], REG_C, ("L0",), "x_c05_ninna_base")
if selected("x_c05_applauso"):  # a very soft applause, "so as not to wake anyone"
    x = silence(2.2)
    for k in range(9):
        mix(x, clap(), 0.12 + k * 0.19 + random.uniform(-0.03, 0.03), 0.5 + 0.5 * math.sin(math.pi * k / 8))
    save("x_c05_applauso", fade(biquad(x, "lp", 2500), 0.0, 0.4), -20)
