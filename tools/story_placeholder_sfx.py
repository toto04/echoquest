"""Compose the PROVISIONAL prologue sound cues (p00_sfx_XX.wav, mono 48 kHz 16-bit).

Placeholders until real recordings are found. Sources: the Sonniss GDC 2026 game audio bundle,
part 1 (royalty-free), plus simple synthesis (piano-like tones, claps, kick, whistle) where the
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

Usage: python3 tools/story_placeholder_sfx.py <mono_dir> <out_dir> [cue ...]
With cue numbers (e.g. 16 17) only those files are written.
"""
import math
import random
import struct
import sys
import wave
from array import array

SR = 48000
MONO, OUT = sys.argv[1], sys.argv[2]
ONLY = {f"p00_sfx_{int(c):02d}" for c in sys.argv[3:]}
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


def kick(vel=0.8):
    n = int(0.35 * SR)
    out, ph = [], 0.0
    for i in range(n):
        t = i / SR
        f = 50 + 70 * math.exp(-t / 0.03)
        ph += 2 * math.pi * f / SR
        out.append(vel * math.exp(-t / 0.12) * math.sin(ph))
    return out


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


def save(name, x, peak_db=-3.0):
    if ONLY and name not in ONLY:
        return
    x = normalize(x, peak_db)
    with wave.open(f"{OUT}/{name}.wav", "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(struct.pack(f"<{len(x)}h", *(max(-32767, min(32767, int(s * 32767))) for s in x)))
    print(f"{name}.wav  {len(x) / SR:.1f}s")


NOTE = {"C3": 130.81, "G3": 196.0, "C4": 261.63, "D#4": 311.13, "E4": 329.63, "G4": 392.0,
        "A4": 440.0, "C5": 523.25, "E5": 659.25, "G5": 783.99}

# The recurring theme of the chapter: three notes, rhythm "TA... TA-TA" (scene 3 asks the child to repeat it).
THEME = [(0.0, "G4"), (1.2, "C5"), (1.5, "E5")]
BAR = 2.4


def theme(notes=3, vel=0.7):
    out = silence(BAR + 1.5)
    for t, n in THEME[:notes]:
        mix(out, piano(NOTE[n], 1.6, vel), t)
    return out


def band_bar(piano_notes=3, with_kick=True, with_bells=True):
    out = theme(piano_notes)
    mix(out, piano(NOTE["C3"], 2.4, 0.35), 0.0)
    if with_kick:
        for k in range(4):
            mix(out, kick(0.7), k * 0.6, 0.8)
    if with_bells:
        mix(out, fade(load("bells", 0.83, 2.0), 0.01, 0.4), 1.2, 0.35)
    return out


# p00_sfx_01: room ambience, someone tries a few piano notes in the distance. MISSING: footsteps, door, murmurs.
x = fade(load("seat"), 0.3, 0.8)
for t, n, v in [(1.6, "A4", 0.3), (2.5, "A4", 0.25), (3.4, "E5", 0.3), (4.6, "C4", 0.25)]:
    mix(x, piano(NOTE[n], 1.8, v), t, 3.0)
save("p00_sfx_01", fade(x, 0, 0.5), -6)

# p00_sfx_02: three funny, slightly out-of-tune notes.
x = silence(2.6)
for t, f, v in [(0.0, NOTE["E4"] * 0.97, 0.6), (0.35, NOTE["D#4"] * 1.03, 0.6), (0.75, NOTE["G3"] * 0.95, 0.8)]:
    mix(x, piano(f, 1.6, v, damp=0.25 if t < 0.7 else None), t)
save("p00_sfx_02", x)

save("p00_sfx_03", piano(NOTE["C4"], 4.5, 0.45), -6)  # one soft long note
x = silence(1.4)  # three short notes
for t in (0.0, 0.3, 0.6):
    mix(x, piano(NOTE["C5"], 0.8, 0.6, damp=0.15), t)
save("p00_sfx_04", x)
save("p00_sfx_05", piano(NOTE["G5"], 3.0, 0.2), -12)  # very delicate
x = silence(3.0)  # energetic chord (level is set in Unity, not here)
for n in ("C3", "C4", "E4", "G4"):
    mix(x, piano(NOTE[n], 3.0, 1.0), 0.0, 0.5)
save("p00_sfx_06", x, -3)

# p00_sfx_07: one clap in a big room (the reverb comes from Unity). MISSING: footsteps.
save("p00_sfx_07", mix(silence(1.0), clap(), 0.05), -4)
save("p00_sfx_08", mix(mix(silence(1.4), clap(), 0.05), clap(), 0.6), -4)  # two claps

# p00_sfx_09: a small object falls. MISSING: throat clearing.
save("p00_sfx_09", mix(silence(0.6), load("drop")), -6)
save("p00_sfx_10", fade(load("rummage", 0.0, 4.5), 0.05, 0.8), -6)  # rummaging through objects

# p00_sfx_12: TAC, TAC... TOC! (two wooden hits, then a lower one)
hit = fade(load("stick", 0.60, 0.90), 0.002, 0.05)
toc = resample(fade(load("stick", 1.28, 1.58), 0.002, 0.05), 0.7)
save("p00_sfx_12", mix(mix(mix(silence(1.6), hit, 0.0), hit, 0.32, 0.9), toc, 0.95, 1.1), -3)

# p00_sfx_13: the orchestra plays the theme twice: piano, a deep beat and a tinkle.
x = silence(0)
mix(x, band_bar(with_bells=False), 0.0)
mix(x, band_bar(), BAR)
save("p00_sfx_13", fade(x, 0, 1.2), -3)

# p00_sfx_14: a gust of wind crosses the room, light at first, then more insistent (never deafening).
storm = biquad(biquad(load("storm", 0.0, 8.0), "lp", 1200), "hp", 120)
save("p00_sfx_14", fade(ramp(storm, 0.15, 0.7), 2.5, 1.2), -6)

# p00_sfx_15: FFFF... a door bangs softly, papers flutter.
x = mix(silence(2.6), load("whoosh"), 0.0)
mix(x, fade(load("woodhit30", 1.9, 3.4), 0.003, 0.6), 0.55, 0.35)
mix(x, load("pages"), 0.8, 1.6)
save("p00_sfx_15", x, -4)

def broken_theme(intensity=1.0):
    """The theme after the wind has stolen it: out of tune, sagging, the last note gone.
    Where the third note should be there is only a dull, stuck key."""
    out = silence(BAR + 1.5)
    mix(out, piano(NOTE["G4"], 1.6, 0.6, cents=-35 * intensity, wobble=18 * intensity), 0.0)
    mix(out, piano(NOTE["C5"], 1.6, 0.6, cents=45 * intensity, sag=-90 * intensity, wobble=25 * intensity), 1.25)
    mix(out, piano(NOTE["E5"], 0.5, 0.35, cents=-110 * intensity, damp=0.06), 1.75)  # the note that will not come
    return out


# p00_sfx_16: wind again; the band loses instruments one by one, then the piano falls apart.
x = silence(0)
mix(x, band_bar(), 0.0)
mix(x, band_bar(with_bells=False), BAR)
mix(x, broken_theme(0.6), 2 * BAR)
mix(x, load("whoosh"), 0.9 * BAR, 0.8)
mix(x, load("whoosh"), 1.9 * BAR, 0.8)
save("p00_sfx_16", fade(x, 0, 0.8), -3)

# p00_sfx_17: the piano tries the theme: two wrong notes, a stuck key, then a pause that is too long.
# Also loops under the narrator ("la canzone continua, ma non è più la stessa").
save("p00_sfx_17", mix(silence(BAR + 2.8), broken_theme(1.0)), -6)

# p00_sfx_18: musical silence, only a light wind (loopable bed).
bed = biquad(biquad(load("storm", 8.0, 16.0), "lp", 600), "hp", 100)
save("p00_sfx_18", fade(bed, 1.0, 1.0), -14)

# p00_sfx_19: the wind leaves quickly, a last whistle.
x = mix(silence(2.0), load("whoosh"), 0.0)
mix(x, whistle(), 0.45, 0.25)
save("p00_sfx_19", fade(x, 0, 0.6), -4)
