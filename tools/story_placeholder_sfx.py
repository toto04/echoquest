"""Compose the PROVISIONAL prologue sound cues (p00_sfx_XX.wav, mono 48 kHz 16-bit).

Placeholders until real recordings are found. Sources: the Sonniss GDC 2026 game audio bundle
(parts 1, 2 and 5, royalty-free), plus simple synthesis (piano-like tones, claps, whistle) where the
bundle has nothing suitable. The piano theme is our own three-note motif, not an existing song.

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
`what` can be a section (p00, s01), a cue id (s01_sfx_05) or a prologue cue number (16).
Without it, everything is written.
"""
import math
import random
import re
import struct
import sys
import wave
from array import array

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
    return name in ONLY or name[:3] in ONLY_SECTIONS


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


def triangle(vel=0.5, dur=2.5):
    """A triangle: DIN. Inharmonic metal partials with a long ring."""
    n = int(dur * SR)
    out = [0.0] * n
    for ratio, amp in zip((1.0, 2.76, 5.40, 8.93, 13.3), (1.0, 0.7, 0.5, 0.35, 0.25)):
        w = 2 * math.pi * 1250 * ratio / SR
        tau = 1.6 / ratio ** 0.4
        ph = random.random() * 2 * math.pi
        for i in range(n):
            out[i] += amp * vel * math.exp(-i / (tau * SR)) * math.sin(w * i + ph)
    click = biquad([random.uniform(-1, 1) * math.exp(-i / (0.001 * SR)) for i in range(int(0.006 * SR))], "hp", 4000)
    return mix(out, click, 0, 0.3 * vel)


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


NOTE = {"C3": 130.81, "G3": 196.0, "C4": 261.63, "D#4": 311.13, "E4": 329.63, "G4": 392.0,
        "A4": 440.0, "C5": 523.25, "D5": 587.33, "E5": 659.25, "G5": 783.99}

# The recurring theme of the chapter: three notes, rhythm "TA... TA-TA" (scene 3 asks the child to repeat it).
THEME = [(0.0, "G4"), (1.2, "C5"), (1.5, "E5")]
BAR = 2.4
BEAT = BAR / 4
# The flute's answer to the theme, alternating every bar: (time in bar, note, length).
FLUTE_LINE = [[(0.0, "E5", 0.55), (0.6, "G5", 0.55), (1.2, "E5", 0.55), (1.8, "D5", 0.55)],
              [(0.0, "C5", 1.1), (1.2, "D5", 0.55), (1.8, "E5", 0.55)]]


def theme(notes=3, vel=0.7):
    out = silence(BAR + 1.5)
    for t, n in THEME[:notes]:
        mix(out, cached(piano, NOTE[n], 1.6, vel), t)
    return out


def add_bar(out, at, bar_index=0, piano_notes=3, bass=True, drum=True, tri=True, flt=True, first_beat=0):
    """Mix one bar of the band into `out` at time `at`. `first_beat` skips the drum's first beats."""
    mix(out, theme(piano_notes), at)
    if bass:
        mix(out, cached(piano, NOTE["C3"], 2.4, 0.35), at)
    if drum:
        for k in range(first_beat, 4):
            mix(out, cached(tom, 0.8 if k % 2 == 0 else 0.5), at + k * BEAT, 0.7)
    if tri:
        for t in (BEAT, 3 * BEAT):
            mix(out, cached(triangle, 0.4), at + t, 0.3)
    if flt:
        for t, n, d in FLUTE_LINE[bar_index % 2]:
            mix(out, cached(flute, NOTE[n], d, 0.4), at + t, 0.5)
    return out


def broken_theme(intensity=1.0):
    """The theme after the wind has stolen it: out of tune, sagging, the last note gone.
    Where the third note should be there is only a dull, stuck key."""
    out = silence(BAR + 1.5)
    mix(out, piano(NOTE["G4"], 1.6, 0.6, cents=-35 * intensity, wobble=18 * intensity), 0.0)
    mix(out, piano(NOTE["C5"], 1.6, 0.6, cents=45 * intensity, sag=-90 * intensity, wobble=25 * intensity), 1.25)
    mix(out, piano(NOTE["E5"], 0.5, 0.35, cents=-110 * intensity, damp=0.06), 1.75)  # the note that will not come
    return out


def cue(n):
    return f"p00_sfx_{n:02d}"


def wanted(n):
    return selected(cue(n))



if wanted(1):  # room ambience, someone tries a few piano notes in the distance. MISSING: footsteps, door, murmurs.
    x = fade(load("seat"), 0.3, 0.8)
    for t, n, v in [(1.6, "A4", 0.3), (2.5, "A4", 0.25), (3.4, "E5", 0.3), (4.6, "C4", 0.25)]:
        mix(x, piano(NOTE[n], 1.8, v), t, 3.0)
    save(cue(1), fade(x, 0, 0.5), -6)

if wanted(2):  # three funny, slightly out-of-tune piano notes
    x = silence(2.6)
    for t, f, v in [(0.0, NOTE["E4"] * 0.97, 0.6), (0.35, NOTE["D#4"] * 1.03, 0.6), (0.75, NOTE["G3"] * 0.95, 0.8)]:
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
    save(cue(5), triangle(0.3, 3.0), -12)  # delicate: one soft triangle touch
if wanted(6):
    x = silence(3.2)  # loud: everybody together
    for n in ("C3", "C4", "E4", "G4"):
        mix(x, piano(NOTE[n], 3.0, 1.0), 0.0, 0.5)
    mix(x, tom(1.0), 0.0, 0.9)
    mix(x, triangle(0.6), 0.0, 0.4)
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
    save(cue(10), mix(mix(silence(3.0), triangle(0.5), 0.0), triangle(0.5), 0.6), -6)  # DIN, DIN
if wanted(11):
    x = silence(2.4)  # a whispering flute
    for t, n, d in [(0.0, "E5", 0.5), (0.5, "D5", 0.5), (1.0, "C5", 1.2)]:
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
    #   "e un altro ancora" ~5 s (triangle at 8.4) · "Ogni strumento..." ~7 s (flute and bass at 9.6)
    #   "La musica continua a crescere" ~15 s (more layers at 16.8, louder up to 20.5)
    # Then the full band keeps playing (looping) until the wind ruins it.
    length = 80.0
    x = silence(length + 2)
    bars = int(length / BAR)
    for b in range(bars):
        at = b * BAR
        add_bar(x, at, b,
                bass=at >= 9.6,
                drum=at + 3 * BEAT >= 6.0, first_beat=max(0, math.ceil((6.0 - at) / BEAT - 1e-6)) if at < 6.0 else 0,
                tri=at + BEAT >= 8.4 - 1e-6 and at >= 7.2,
                flt=at >= 9.6)
        if at >= 16.8:  # growing: chords on every bar and extra triangle
            for n in ("C4", "E4", "G4"):
                mix(x, cached(piano, NOTE[n], 2.0, 0.4), at, 0.35)
            for k in (0, 2):
                mix(x, cached(triangle, 0.3), at + k * BEAT, 0.2)
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


NOTE.update({"C2": 65.41, "E3": 164.81, "A3": 220.0, "G3": 196.0, "B4": 493.88, "A5": 880.0, "C6": 1046.5})


def cue1(n):
    return f"s01_sfx_{n:02d}"


def wanted1(n):
    return selected(cue1(n))


NOTE.update({"F4": 349.23})


def stolen_try(intensity=0.6, vel=0.6):
    """The piano trying the stolen song after the wind: the same two out-of-tune notes as in the
    prologue (broken_theme), and then nothing where the third note should be."""
    out = silence(BAR + 0.6)
    mix(out, piano(NOTE["G4"], 1.6, vel, cents=-35 * intensity, wobble=18 * intensity), 0.0)
    mix(out, piano(NOTE["C5"], 1.6, vel, cents=45 * intensity, sag=-90 * intensity, wobble=25 * intensity), 1.25)
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
# then silence) or as song_memory; every other piano sound avoids its notes, so the child can tell them apart.
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
    save(cue1(6), phrase([(0.0, NOTE["E4"] * 1.04, 0.2), (0.35, NOTE["G3"] * 0.96)], 0.7), -4)
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
if wanted1(18):  # the Maestro's rhythm, then the piano imitates it (not the song)
    x = tac_tac_toc() + silence(1.6)
    mix(x, phrase([(0.0, "G4", 0.15), (0.32, "G4", 0.15), (0.95, "C4", 0.35)], 0.6, 0.9), 1.8)  # short, so it doesn't ring under the voice
    save(cue1(18), fade(x, 0, 0.3), -3)
if wanted1(19):  # two cheerful notes (not the song's C-E step)
    save(cue1(19), phrase([(0.0, "D5", 0.18), (0.25, "A4", 0.3)], 0.6), -4)
if wanted1(20):  # a breath of wind far away (the same storm as Sbuffo in the prologue), a door creaks a little
    x = fade(biquad(load("storm", 4.0, 8.0), "lp", 900), 1.0, 1.5)
    mix(x, load("k_creak1"), 1.6, 0.6)
    save(cue1(20), x, -8)
if wanted1(21):  # the wind blows a few more seconds, then calms down
    save(cue1(21), fade(ramp(biquad(load("storm", 9.0, 14.0), "lp", 800), 0.8, 0.1), 0.6, 2.0), -8)
if wanted1(22):  # the Maestro sighs once: a single, slowed-down exhale
    sigh = fade(load("pant", 3.86, 4.62), 0.04, 0.35)  # only the exhale of one breath
    save(cue1(22), biquad(resample(sigh, 0.85), "lp", 3000) + silence(0.2), -12)
if wanted1(23):  # the memory of the complete song, as the orchestra played it in the prologue
    save(cue1(23), song_memory(), -6)
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
    save(cue1(27), phrase([(0.0, "E4"), (0.6, "G4")], 0.4, 2.0), -6)  # two soft, encouraging notes
if wanted1(28):
    save(cue1(28), phrase([(0.0, "A4", 0.2), (0.25, "D5", 0.3)], 0.6), -4)  # two cheerful notes
if wanted1(29):
    save(cue1(29), piano(NOTE["C4"], 1.8, 0.6), -4)  # one note
if wanted1(30):  # a second note joins the first
    save(cue1(30), mix(piano(NOTE["C4"], 2.2, 0.55), piano(NOTE["E4"], 2.0, 0.55), 0.4), -4)
if wanted1(31):  # a drum gently accompanies the two notes (a new example, not the song)
    x = silence(3.6)
    for k in range(3):
        mix(x, piano(NOTE["C4"], 1.2, 0.45), k * 1.2)
        mix(x, piano(NOTE["E4"], 1.2, 0.45), k * 1.2 + 0.6)
        mix(x, tom(0.5), k * 1.2, 0.7)
    save(cue1(31), fade(x, 0, 0.6), -4)
if wanted1(32):  # the sounds stop, a short silence
    save(cue1(32), mix(silence(1.2), phrase([(0.0, "C4", 0.08), (0.0, "E4", 0.08)], 0.4, 0.4)), -12)
if wanted1(33):
    save(cue1(33), phrase([(0.0, "E5"), (0.5, "D5"), (1.0, "C5")], 0.4, 1.6), -6)  # three sweet notes
if wanted1(11):  # four little, well separated neutral signals (not instruments yet)
    x = silence(3.6)
    for k, f in enumerate((660, 740, 830, 990)):
        mix(x, blip(f), k * 0.9)
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
    mix(x, triangle(0.2, 1.2), 3.6, 0.4)
    mix(x, flute(NOTE["A4"], 0.6, 0.3), 5.0, 0.5)
    mix(x, piano(NOTE["D5"], 1.0, 0.3), 6.2, 0.6)
    save(cue1(36), fade(x, 0, 0.8), -4)
if wanted1(37):  # the piano tries its part once more: two notes, then silence
    save(cue1(37), stolen_try(0.6) + silence(0.9), -4)
if wanted1(38):
    save(cue1(38), piano(NOTE["C2"], 5.0, 0.7), -4)  # a single low note that rings on
if wanted1(40):  # soft notes lead into the next scene (not the stolen phrase)
    save(cue1(40), fade(phrase([(0.0, "C4"), (0.5, "E4"), (1.0, "G4"), (1.5, "A4"), (2.2, "G4")], 0.4, 2.0), 0, 1.0), -6)

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
    save("x_s01_pulsante", blip(660) + silence(0.06), -8)
if selected("x_s01_nota_mancante"):
    x = silence(3.2)
    intensity = 0.6
    mix(x, piano(NOTE["G4"], 1.6, 0.6, cents=-35 * intensity, wobble=18 * intensity), THEME[0][0])
    mix(x, piano(NOTE["C5"], 1.6, 0.6, cents=45 * intensity, sag=-90 * intensity, wobble=25 * intensity), THEME[1][0])
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
