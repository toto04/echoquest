"""Small pure-Python instruments for EchoQuest music snippets (mono, 48 kHz, lists of floats).

No dependencies: docs/music/05-format-and-pipeline.md proposes FluidSynth and a GeneralUser GS bank
for final renders; until those are installed, these placeholder instruments render the same grid
specs. The piano is the same model as tools/story_placeholder_sfx.py, so the button timbre the child
hears in the battle matches the piano of the prologue and Scene 1.
"""
from __future__ import annotations

import math
import random
import re

SR = 48000
_rng = random.Random(7)


def silence(sec: float) -> list[float]:
    return [0.0] * int(round(sec * SR))


def mix(dst: list[float], src: list[float], at: float = 0.0, gain: float = 1.0) -> list[float]:
    off = int(round(at * SR))
    if off < 0:
        src, off = src[-off:], 0
    if off + len(src) > len(dst):
        dst.extend([0.0] * (off + len(src) - len(dst)))
    for i, s in enumerate(src):
        dst[off + i] += s * gain
    return dst


def fade(x: list[float], fin: float = 0.0, fout: float = 0.0) -> list[float]:
    n_in, n_out = int(fin * SR), int(fout * SR)
    for i in range(min(n_in, len(x))):
        x[i] *= i / n_in
    for i in range(min(n_out, len(x))):
        x[-1 - i] *= i / n_out
    return x


def biquad(x: list[float], kind: str, f0: float, q: float = 0.707) -> list[float]:
    w0 = 2 * math.pi * f0 / SR
    al, c = math.sin(w0) / (2 * q), math.cos(w0)
    if kind == "lp":
        b = [(1 - c) / 2, 1 - c, (1 - c) / 2]
    elif kind == "hp":
        b = [(1 + c) / 2, -(1 + c), (1 + c) / 2]
    else:  # band-pass, constant peak gain
        b = [al, 0.0, -al]
    a0 = 1 + al
    b = [v / a0 for v in b]
    a1, a2 = -2 * c / a0, (1 - al) / a0
    y, x1, x2, y1, y2 = [], 0.0, 0.0, 0.0, 0.0
    for s in x:
        o = b[0] * s + b[1] * x1 + b[2] * x2 - a1 * y1 - a2 * y2
        x2, x1, y2, y1 = x1, s, y1, o
        y.append(o)
    return y


def noise(n: int) -> list[float]:
    return [_rng.uniform(-1, 1) for _ in range(n)]


class _Notes(dict):
    """Note name ("D4", "Bb2", "F#4") -> equal-tempered frequency in Hz, A4 = 440."""

    def __missing__(self, name: str) -> float:
        return 440.0 * 2 ** ((midi(name) - 69) / 12)


def midi(name: str) -> int:
    m = re.fullmatch(r"([A-G])(#|b)?(-?\d)", name)
    if not m:
        raise ValueError(f"not a note: {name}")
    pc = "C D EF G A B".index(m[1]) + {"#": 1, "b": -1, None: 0}[m[2]]
    return pc + 12 * (int(m[3]) + 1)


NOTE = _Notes()

# One band-limited cycle per waveform, read with a phase accumulator (fast enough in pure Python).
_TABLE = 2048


def _table(harmonics) -> list[float]:
    t = [0.0] * _TABLE
    for k, amp in harmonics:
        for i in range(_TABLE):
            t[i] += amp * math.sin(2 * math.pi * k * i / _TABLE)
    peak = max(abs(v) for v in t)
    return [v / peak for v in t]


_SAW = _table([(k, 1 / k) for k in range(1, 24)])
_REED = _table([(1, 1.0), (2, 0.55), (3, 0.7), (4, 0.25), (5, 0.35), (6, 0.12), (7, 0.15), (9, 0.06)])
_BRASS = _table([(k, 1 / k ** 0.8) for k in range(1, 16)])
_FLUTE = _table([(1, 1.0), (2, 0.22), (3, 0.1), (4, 0.04)])


def _osc(table, freqs, env) -> list[float]:
    """Wavetable oscillator. `freqs` is a constant or a per-sample list; `env` is the amplitude list."""
    out, ph = [0.0] * len(env), _rng.random() * _TABLE
    const = not isinstance(freqs, list)
    step = freqs * _TABLE / SR if const else 0.0
    for i, e in enumerate(env):
        if not const:
            step = freqs[i] * _TABLE / SR
        ph += step
        if ph >= _TABLE:
            ph -= _TABLE * int(ph / _TABLE)
        out[i] = e * table[int(ph)]
    return out


def _adsr(dur: float, a: float, d: float, s: float, r: float, total: float | None = None) -> list[float]:
    total = dur + r if total is None else total
    n = int(total * SR)
    env = [0.0] * n
    for i in range(n):
        t = i / SR
        if t < a:
            v = t / a
        elif t < a + d:
            v = 1 - (1 - s) * (t - a) / d
        elif t < dur:
            v = s
        else:
            v = s * max(0.0, 1 - (t - dur) / r)
        env[i] = v
    return env


# ---------------------------------------------------------------- pitched instruments

def piano(freq: float, dur: float = 2.0, vel: float = 0.7, damp: float | None = None, bright: float = 0.0) -> list[float]:
    """Inharmonic partials with exponential decay and a hammer click (as story_placeholder_sfx.piano).
    `bright` > 0 lifts the upper partials, the way a harder touch does."""
    n = int(dur * SR)
    out = [0.0] * n
    inharm = 0.0004
    for k in range(1, 9):
        fk = k * freq * math.sqrt(1 + inharm * k * k)
        if fk > SR / 2.2:
            break
        amp = vel / k ** (1.3 - 0.5 * bright) * (1.0 if k == 1 else 0.8)
        decay = (1.2 + 0.6 * k) * (freq / 261.6) ** 0.5
        ph = _rng.random() * 2 * math.pi
        w = 2 * math.pi * fk / SR
        g = math.exp(-decay / SR)
        e = amp
        for i in range(n):
            out[i] += e * math.sin(w * i + ph)
            e *= g
    for i in range(int(0.004 * SR)):
        out[i] *= i / (0.004 * SR)
    click = biquad([_rng.uniform(-1, 1) * math.exp(-i / (0.003 * SR)) for i in range(int(0.02 * SR))], "bp", 2500, 1.0)
    mix(out, click, 0, (0.15 + 0.15 * bright) * vel)
    if damp:
        d0 = int(damp * SR)
        for i in range(d0, n):
            out[i] *= math.exp(-(i - d0) / (0.04 * SR))
    return out


def pizz(freq: float, dur: float = 0.6, vel: float = 0.7) -> list[float]:
    """Plucked string (Karplus-Strong): pizzicato bass with audible upper harmonics."""
    period = max(2, int(round(SR / freq)))
    buf = [_rng.uniform(-1, 1) for _ in range(period)]
    n = int((dur + 0.25) * SR)
    out = [0.0] * n
    damping = 0.996 if freq < 150 else 0.993
    for i in range(n):
        j = i % period
        v = buf[j]
        out[i] = v
        buf[j] = damping * 0.5 * (v + buf[(j + 1) % period])
    env_n = int(dur * SR)
    for i in range(env_n, n):
        out[i] *= max(0.0, 1 - (i - env_n) / (0.25 * SR))
    out = biquad(out, "lp", min(3500, freq * 14))
    return [v * vel for v in out]


def strings(freqs: list[float], dur: float, vel: float = 0.5) -> list[float]:
    """A soft string pad: three detuned saws per note, slow attack, low-passed (a 'world' sound)."""
    env = _adsr(dur, 0.35, 0.3, 0.85, 0.6)
    out = [0.0] * len(env)
    for f in freqs:
        for det in (-6, 0, 7):
            mix(out, _osc(_SAW, f * 2 ** (det / 1200), env), 0, vel / (3 * len(freqs)) ** 0.5 / 1.7)
    return biquad(out, "lp", 1800)


def timpani(freq: float, vel: float = 0.8, dur: float = 1.6) -> list[float]:
    n = int(dur * SR)
    out = [0.0] * n
    for ratio, amp, tau in ((1.0, 1.0, 0.55), (1.5, 0.45, 0.35), (1.99, 0.25, 0.25), (2.44, 0.12, 0.2)):
        ph, w = _rng.random() * 6.283, 2 * math.pi * freq * ratio / SR
        for i in range(n):
            t = i / SR
            out[i] += vel * amp * math.exp(-t / tau) * math.sin(w * i * (1 + 0.02 * math.exp(-t / 0.05)) + ph)
    thump = biquad(noise(int(0.06 * SR)), "lp", 400)
    return mix(out, [s * math.exp(-i / (0.015 * SR)) for i, s in enumerate(thump)], 0, 0.6 * vel)


def pan_flute(freq: float, dur: float, vel: float = 0.6) -> list[float]:
    """Breathy pan flute with a short wooden transient (Sbuffo's instrument)."""
    a = 0.03
    env = _adsr(dur, a, 0.05, 0.85, 0.12)
    vib = [freq * 2 ** (10 * math.sin(2 * math.pi * 5.5 * i / SR) * min(1, i / (0.2 * SR)) / 1200) for i in range(len(env))]
    out = _osc(_FLUTE, vib, [e * vel for e in env])
    air = biquad(noise(len(env)), "bp", min(freq * 2.2, 9000), 1.4)
    for i in range(len(env)):
        out[i] += 0.32 * vel * env[i] * air[i]
    chiff = biquad([_rng.uniform(-1, 1) * math.exp(-i / (0.004 * SR)) for i in range(int(0.03 * SR))], "bp", 1800, 2.0)
    return mix(out, chiff, 0, 0.5 * vel)


def flute(freq: float, dur: float, vel: float = 0.5) -> list[float]:
    env = _adsr(dur, 0.07, 0.05, 0.9, 0.15)
    vib = [freq * 2 ** (12 * math.sin(2 * math.pi * 5.2 * i / SR) * min(1, i / (0.3 * SR)) / 1200) for i in range(len(env))]
    out = _osc(_FLUTE, vib, [e * vel for e in env])
    air = biquad(noise(len(env)), "bp", min(freq * 2, 8000), 2.0)
    return [o + 0.08 * vel * env[i] * air[i] for i, o in enumerate(out)]


def bassoon(freq: float, dur: float, vel: float = 0.6) -> list[float]:
    """Staccato reed: the comic 'ha-ha-ha'."""
    env = _adsr(dur, 0.02, 0.06, 0.7, 0.06)
    out = _osc(_REED, freq, [e * vel for e in env])
    return biquad(biquad(out, "bp", 520, 0.9), "lp", 2500) if freq < 260 else biquad(out, "lp", 2500)


def trombone(f0: float, f1: float, dur: float, vel: float = 0.6, wobble: float = 7.0) -> list[float]:
    """A falling, wobbling trombone: the cartoon 'wah-wah' (pitch bend from f0 to f1)."""
    env = _adsr(dur, 0.04, 0.1, 0.85, 0.12)
    n = len(env)
    freqs = []
    for i in range(n):
        t = min(1.0, i / (dur * SR))
        f = f0 * (f1 / f0) ** (t ** 1.4)
        freqs.append(f * 2 ** (40 * math.sin(2 * math.pi * wobble * i / SR) * t / 1200))
    return biquad(_osc(_BRASS, freqs, [e * vel for e in env]), "lp", 1600)


def brass(freq: float, dur: float, vel: float = 0.7) -> list[float]:
    env = _adsr(dur, 0.025, 0.08, 0.8, 0.15)
    out = _osc(_BRASS, freq, [e * vel for e in env])
    # brighter at the attack, like a real brass onset
    bright = biquad(out, "hp", 1200)
    for i in range(min(len(out), int(0.12 * SR))):
        out[i] += 0.5 * bright[i] * (1 - i / (0.12 * SR))
    return biquad(out, "lp", 3500)


def celesta(freq: float, dur: float = 1.0, vel: float = 0.5) -> list[float]:
    n = int(dur * SR)
    out = [0.0] * n
    for ratio, amp, tau in ((1.0, 1.0, 0.5), (4.0, 0.35, 0.12), (8.2, 0.12, 0.05)):
        w, ph = 2 * math.pi * freq * ratio / SR, _rng.random() * 6.283
        for i in range(n):
            out[i] += vel * amp * math.exp(-i / (tau * SR)) * math.sin(w * i + ph)
    return out


def marimba(freq: float, dur: float = 0.8, vel: float = 0.5) -> list[float]:
    n = int(dur * SR)
    out = [0.0] * n
    for ratio, amp, tau in ((1.0, 1.0, 0.35), (3.93, 0.25, 0.06), (9.2, 0.06, 0.02)):
        w, ph = 2 * math.pi * freq * ratio / SR, _rng.random() * 6.283
        for i in range(n):
            out[i] += vel * amp * math.exp(-i / (tau * SR)) * math.sin(w * i + ph)
    for i in range(int(0.003 * SR)):
        out[i] *= i / (0.003 * SR)
    return out


def slide_whistle(f0: float, f1: float, dur: float, vel: float = 0.4) -> list[float]:
    n = int(dur * SR)
    out, ph = [], 0.0
    for i in range(n):
        t = i / n
        f = f0 * (f1 / f0) ** t
        ph += 2 * math.pi * f / SR
        out.append(vel * math.sin(math.pi * t) ** 0.8 * (math.sin(ph) + 0.04 * _rng.uniform(-1, 1)))
    return out


# ---------------------------------------------------------------- drums (unpitched, free in every register)

def kick(vel: float = 0.9) -> list[float]:
    n = int(0.35 * SR)
    out, ph = [], 0.0
    for i in range(n):
        t = i / SR
        f = 48 + 90 * math.exp(-t / 0.03)
        ph += 2 * math.pi * f / SR
        out.append(vel * math.exp(-t / 0.12) * math.sin(ph))
    click = biquad([_rng.uniform(-1, 1) * math.exp(-i / (0.0015 * SR)) for i in range(int(0.01 * SR))], "lp", 3000)
    return mix(out, click, 0, 0.35 * vel)


def woodblock(freq: float = 1000.0, vel: float = 0.6) -> list[float]:
    n = int(0.12 * SR)
    w = 2 * math.pi * freq / SR
    tone = [vel * math.exp(-i / (0.022 * SR)) * math.sin(w * i) for i in range(n)]
    hit = biquad([_rng.uniform(-1, 1) * math.exp(-i / (0.002 * SR)) for i in range(n)], "bp", freq * 1.6, 3.0)
    return mix(tone, hit, 0, 0.6 * vel)


def claves(vel: float = 0.6) -> list[float]:
    return woodblock(2500.0, vel)


def shaker(vel: float = 0.25) -> list[float]:
    n = int(0.07 * SR)
    env = [min(1.0, i / (0.01 * SR)) * math.exp(-i / (0.02 * SR)) for i in range(n)]
    return [vel * e * s for e, s in zip(env, biquad(noise(n), "bp", 6500, 1.2))]


def hat(vel: float = 0.3) -> list[float]:
    """Closed hi-hat: a very short burst of bright noise (sharper and higher than the shaker)."""
    n = int(0.05 * SR)
    env = [math.exp(-i / (0.008 * SR)) for i in range(n)]
    return [vel * e * s for e, s in zip(env, biquad(noise(n), "hp", 7000))]


def snare(vel: float = 0.6) -> list[float]:
    n = int(0.2 * SR)
    body = [vel * 0.6 * math.exp(-i / (0.03 * SR)) * math.sin(2 * math.pi * 190 * i / SR) for i in range(n)]
    rattle = biquad(noise(n), "hp", 1800)
    return mix(body, [vel * 0.7 * math.exp(-i / (0.06 * SR)) * s for i, s in enumerate(rattle)])


DRUMS = {
    "kick": kick,
    "snare": snare,
    "shaker": shaker,
    "hat": hat,
    "claves": claves,
    "block_hi": lambda vel=0.6: woodblock(1050.0, vel),
    "block_lo": lambda vel=0.6: woodblock(680.0, vel),
    "timpani_d": lambda vel=0.8: timpani(NOTE["D2"], vel),
}
