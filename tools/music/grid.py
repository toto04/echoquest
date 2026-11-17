"""Read, check and render EchoQuest snippet specs (docs/music/05-format-and-pipeline.md).

A spec is a dict (one YAML document entry) with `bars` (the playable layer: D F A C - .) and/or
`tracks` (backing: absolute notes like D2, chords like D2+A2, or drum lanes with x). The same grid
strings give the game its expected onsets, so audio and judging can't drift apart.

Registers: the four buttons are D4 F4 A4 C5; Scene 2's choice shifts the whole set one octave
(01-palette.md): "grave" = D3-C4, "acuto" = D5-C6, "normale" = no shift.
"""
from __future__ import annotations

import math
import random
import re
from dataclasses import dataclass, field

from . import synth
from .synth import NOTE, SR, midi

REGISTERS = {"normale": 0, "grave": -12, "acuto": 12}
BUTTON_NOTES = {"D": "D4", "F": "F4", "A": "A4", "C": "C5"}
BUTTON_NUMBER = {"D": 1, "F": 2, "A": 3, "C": 4}
FORBIDDEN_PC = {4: "E", 11: "B", 1: "C#/Db", 8: "G#/Ab", 6: "F#/Gb"}
SAFE_CHORDS = {
    "Dm", "Dm7", "D5", "Dsus4", "D7sus4",
    "F", "F6", "F5", "Fsus2", "Fadd9",
    "Gm7", "Gm9", "G5", "Gsus2", "Gsus4", "G7sus4",
    "Bb", "Bbmaj7", "Bb6", "Bb5", "Bbsus2", "Bbadd9",
    "C5", "Csus4", "Csus2", "C7sus4",
    "Ebmaj7#11", "Eb6", "Eb",
}
STAGE_MAX_NOTES = {1: 3, 2: 4, 3: 6, 4: 11}
PLAYABLE_ROLES = {"call", "answer", "refrain", "theme"}


def shift_note(name: str, semitones: int) -> str:
    if semitones == 0:
        return name
    m = midi(name) + semitones
    names = ["C", "C#", "D", "Eb", "E", "F", "F#", "G", "Ab", "A", "Bb", "B"]
    return f"{names[m % 12]}{m // 12 - 1}"


@dataclass
class Event:
    bar: int            # 0-based
    step: int           # 0-based within the bar
    notes: list[str]    # note names, or ["x"] for a drum hit
    steps: int          # length in steps (holds included)
    buttons: list[int] = field(default_factory=list)  # 1-4 when the event is a button pitch


@dataclass
class Report:
    errors: list[str] = field(default_factory=list)
    warnings: list[str] = field(default_factory=list)


def expand_bars(bars: list[str], where: str, rep: Report) -> list[list[str]]:
    out: list[list[str]] = []
    for i, bar in enumerate(bars):
        m = re.fullmatch(r"=(\d+)", bar.strip())
        if m:
            k = int(m[1])
            if not 1 <= k <= i:
                rep.errors.append(f"H3 {where} bar {i + 1}: '={k}' must point to an earlier bar")
                out.append([])
            else:
                out.append(list(out[k - 1]))
        else:
            out.append(bar.split())
    return out


def parse_track(bars: list[str], steps_per_bar: int, where: str, rep: Report, buttons: bool, register: int,
                drums: bool = False) -> list[Event]:
    grid = expand_bars(bars, where, rep)
    events: list[Event] = []
    last: Event | None = None
    for b, toks in enumerate(grid):
        if len(toks) != steps_per_bar:
            rep.errors.append(f"H1 {where} bar {b + 1} has {len(toks)} steps, expected {steps_per_bar}")
        for s, t in enumerate(toks):
            if t == "-":
                if last is None:
                    rep.errors.append(f"H1 {where} bar {b + 1} step {s + 1}: '-' after silence")
                else:
                    last.steps += 1
                continue
            if t == ".":
                last = None
                continue
            if drums:
                if t != "x":
                    rep.errors.append(f"H1 {where} bar {b + 1}: drum lanes use only x - .")
                    continue
                last = Event(b, s, ["x"], 1)
                events.append(last)
                continue
            if buttons:
                if t not in BUTTON_NOTES:
                    rep.errors.append(f"H2 {where} bar {b + 1}: '{t}' is not a button (D F A C)")
                    continue
                last = Event(b, s, [shift_note(BUTTON_NOTES[t], register)], 1, [BUTTON_NUMBER[t]])
            else:
                notes = t.split("+")
                try:
                    for n in notes:
                        midi(n)
                except ValueError:
                    rep.errors.append(f"H1 {where} bar {b + 1}: illegal token '{t}'")
                    continue
                last = Event(b, s, notes, 1)
            events.append(last)
    return events


def bar_count(bars: list[str]) -> int:
    return len(bars)


def _timing(spec) -> tuple[float, float]:
    tempo = spec.get("tempo", 100)
    beats = int(str(spec.get("meter", "4/4")).split("/")[0])
    bar = 60.0 / tempo * beats
    return bar, tempo


def check(spec: dict, register: str = "normale") -> Report:
    """Hard checks H1-H6 and H8 and the call warnings of 05-format-and-pipeline.md."""
    rep = Report()
    sid = spec.get("id", "?")
    shift = REGISTERS[register]
    open_input = spec.get("input", "open") == "open"
    spb = spec.get("steps_per_bar", 8)
    role = spec.get("role", "")
    n_bars = None
    bar_len, tempo = _timing(spec)
    reserved = range(60 + shift, 75 + shift)
    call_bars = {b - 1 for b in spec.get("call_bars", [])}

    if "bars" in spec:
        evs = parse_track(spec["bars"], spb, f"{sid}", rep, role in PLAYABLE_ROLES, shift)
        n_bars = len(spec["bars"])
        if role in ("call", "answer"):
            _check_call(spec, evs, spb, bar_len, rep)
    for name, tr in (spec.get("tracks") or {}).items():
        if not _track_in_register(tr, register):
            continue
        tspb = tr.get("steps_per_bar", spb)
        if "lanes" in tr:
            for lane, bars in tr["lanes"].items():
                parse_track(bars, tspb, f"{sid}.{name}.{lane}", rep, False, 0, drums=True)
                n_bars = _same(n_bars, len(bars), f"{sid}.{name}.{lane}", rep)
            continue
        buttons = bool(tr.get("buttons"))
        evs = parse_track(tr["bars"], tspb, f"{sid}.{name}", rep, buttons, shift)
        n_bars = _same(n_bars, len(tr["bars"]), f"{sid}.{name}", rep)
        if not open_input or buttons:
            continue
        layer = tr.get("layer", "")
        for e in evs:
            locked_bar = (e.bar + 1) in spec.get("locked_bars", [])
            if locked_bar:
                continue
            for n in e.notes:
                pc = midi(n) % 12
                if pc in FORBIDDEN_PC:
                    rep.errors.append(f"H4 {sid}.{name} bar {e.bar + 1}: {n} is a forbidden pitch class ({FORBIDDEN_PC[pc]}) while input is open")
                if layer in ("L0", "L1") and midi(n) in reserved and not tr.get("muted_in_call_bars"):
                    rep.errors.append(f"H6 {sid}.{name} bar {e.bar + 1}: {n} sits in the call register ({register})")
                if call_bars and e.bar in call_bars and midi(n) >= reserved.start and not tr.get("muted_in_call_bars"):
                    rep.warnings.append(f"W11 {sid}.{name} bar {e.bar + 1}: {n} is at or above the call during a call bar")
    if open_input:
        for i, ch in enumerate(spec.get("chords") or []):
            for part in str(ch).split():
                if part not in SAFE_CHORDS and (i + 1) not in spec.get("locked_bars", []):
                    rep.errors.append(f"H5 {sid} bar {i + 1}: chord {part} is not on the safe list while input is open")
    if spec.get("chords") and n_bars is not None and len(spec["chords"]) != n_bars:
        rep.errors.append(f"H3 {sid}: {len(spec['chords'])} chords for {n_bars} bars")
    if role == "loop" and spec.get("loop_bars") and n_bars != spec["loop_bars"]:
        rep.errors.append(f"H8 {sid}: {n_bars} bars, expected {spec['loop_bars']}")
    return rep


def _same(n, m, where, rep):
    if n is not None and n != m:
        rep.errors.append(f"H3 {where}: {m} bars, the other tracks have {n}")
    return m if n is None else n


def _track_in_register(tr: dict, register: str) -> bool:
    regs = tr.get("register")
    return regs is None or register in (regs if isinstance(regs, list) else [regs])


def _check_call(spec, evs, spb, bar_len, rep):
    sid = spec["id"]
    stage = spec.get("stage", 2)
    step_s = bar_len / spb
    if len(evs) > STAGE_MAX_NOTES.get(stage, 99):
        rep.warnings.append(f"W1 {sid}: {len(evs)} notes, stage {stage} allows {STAGE_MAX_NOTES[stage]}")
    onsets = [e.bar * spb + e.step for e in evs]
    gaps = [(b - a) * step_s for a, b in zip(onsets, onsets[1:])]
    if gaps and min(gaps) < 0.25 - 1e-9 and stage <= 3:
        rep.warnings.append(f"W2 {sid}: onsets {min(gaps) * 1000:.0f} ms apart (< 250 ms)")
    if evs:
        last = evs[-1]
        strong = last.step in (0, spb // 2)
        longest = last.steps >= max(e.steps for e in evs)
        if not (strong or longest):
            rep.warnings.append(f"W5 {sid}: the last note is neither on beat 1/3 nor the longest")
        tonic = {"D": "D", "F": "F"}.get(spec.get("centre", "D"))
        if not spec.get("open_ending") and last.notes[0][0] != tonic:
            rep.warnings.append(f"W6 {sid}: an echo should end on the tonic {tonic}")
        if len({e.notes[0] for e in evs}) == 4 and len({e.steps for e in evs}) == 1:
            rep.warnings.append(f"W7 {sid}: the aimless archetype")


def call_onsets(spec: dict, register: str = "normale") -> list[tuple[float, int]]:
    """[(seconds from the start, button 1-4)] for a call: what the child must reproduce."""
    rep = Report()
    bar_len, _ = _timing(spec)
    spb = spec.get("steps_per_bar", 8)
    evs = parse_track(spec["bars"], spb, spec["id"], rep, True, REGISTERS[register])
    if rep.errors:
        raise ValueError("; ".join(rep.errors))
    return [((e.bar * spb + e.step) * bar_len / spb, e.buttons[0]) for e in evs]


# ---------------------------------------------------------------- rendering

def _velocity_call(step: int, spb: int, final: bool) -> float:
    beat = spb // 4
    v = 100 if step == 0 else 88 if step % beat == 0 else 76
    return min(127, v + (6 if final else 0)) / 127


def _instrument(name: str):
    return {
        "piano": "piano", "pizzicato_strings": "pizz", "acoustic_bass": "pizz", "strings": "strings",
        "timpani": "timpani", "pan_flute": "pan_flute", "flute": "flute", "bassoon": "bassoon",
        "trombone": "trombone", "brass_section": "brass", "celesta": "celesta", "marimba": "marimba",
    }[name]


TRACK_GAIN = {"piano": 0.9, "pizz": 0.55, "strings": 0.35, "timpani": 0.55, "pan_flute": 0.6, "flute": 0.4,
              "bassoon": 0.6, "trombone": 0.5, "brass": 0.45, "celesta": 0.5, "marimba": 0.6}
DRUM_GAIN = {"kick": 0.75, "snare": 0.45, "shaker": 0.5, "claves": 0.6, "block_hi": 0.4, "block_lo": 0.6,
             "timpani_d": 0.6, "hat": 0.3}


def render(spec: dict, register: str = "normale", kit: dict | None = None, tail: float = 2.0,
           only_layers: set[str] | None = None, call_tracks_only: bool = False) -> list[float]:
    """Render a spec to mono samples. Loops (role: loop with loop: true) wrap their tail around,
    so the file repeats seamlessly; other snippets keep `tail` seconds of ring after the last bar.
    `kit` maps a drum lane to a function returning a sample list (e.g. the Maestro's real baton)."""
    rep = check(spec, register)
    if rep.errors:
        raise ValueError(f"{spec.get('id')}: " + "; ".join(rep.errors))
    rng = random.Random(hash(spec.get("id")) & 0xFFFF)
    shift = REGISTERS[register]
    bar_len, _ = _timing(spec)
    spb = spec.get("steps_per_bar", 8)
    tracks = dict(spec.get("tracks") or {})
    if "bars" in spec:
        tracks = {"_call": {"instrument": spec.get("instrument", "piano"), "bars": spec["bars"], "buttons": spec.get("role") in PLAYABLE_ROLES,
                            "layer": "call"}, **({} if call_tracks_only else tracks)}
    n_bars = max(len(t["bars"]) if "bars" in t else len(next(iter(t["lanes"].values()))) for t in tracks.values())
    length = n_bars * bar_len
    out = synth.silence(length + tail)
    for name, tr in tracks.items():
        if not _track_in_register(tr, register):
            continue
        if only_layers is not None and tr.get("layer") not in only_layers:
            continue
        tspb = tr.get("steps_per_bar", spb)
        step_s = bar_len / tspb
        if "lanes" in tr:
            for lane, bars in tr["lanes"].items():
                evs = parse_track(bars, tspb, lane, Report(), False, 0, drums=True)
                fn = (kit or {}).get(lane) or synth.DRUMS[lane]
                anchor = lane in ("kick", "claves", "block_lo", "timpani_d")
                walk = 0.0
                for e in evs:
                    t = (e.bar * tspb + e.step) * step_s
                    vel = 1.0 if e.step == 0 else 0.85 if e.step % (tspb // 4) == 0 else 0.7
                    if not anchor:  # correlated humanisation, +-5 ms, never on the beat anchors
                        walk = 0.8 * walk + rng.uniform(-2.0, 2.0)
                        t += max(-0.005, min(0.005, walk / 1000))
                        vel *= rng.uniform(0.92, 1.0)
                    mix_gain = DRUM_GAIN.get(lane, 0.5) * tr.get("gain", 1.0)
                    synth.mix(out, fn(vel) if fn.__code__.co_argcount else fn(), max(0.0, t), mix_gain)
            continue
        inst = _instrument(tr.get("instrument", "piano"))
        buttons = bool(tr.get("buttons"))
        evs = parse_track(tr["bars"], tspb, name, Report(), buttons, shift)
        is_call = tr.get("layer") in ("call", "L2") or buttons
        gain = TRACK_GAIN[inst] * tr.get("gain", 1.0)
        for k, e in enumerate(evs):
            t = (e.bar * tspb + e.step) * step_s
            dur = e.steps * step_s
            vel = _velocity_call(e.step, tspb, k == len(evs) - 1) if is_call else 0.8
            for n in e.notes:
                f = NOTE[n]
                if inst == "piano":
                    snd = piano(f, dur + 0.9, vel, damp=0.9 * dur + 0.12)
                elif inst == "pizz":
                    snd = synth.pizz(f, dur * 0.9, vel)
                elif inst == "strings":
                    snd = synth.strings([NOTE[x] for x in e.notes], dur, vel)
                    synth.mix(out, snd, t, gain)
                    break
                elif inst == "timpani":
                    snd = synth.timpani(f, vel)
                elif inst == "pan_flute":
                    snd = synth.pan_flute(f, dur * 0.9, vel)
                elif inst == "flute":
                    snd = synth.flute(f, dur * 0.95, vel)
                elif inst == "bassoon":
                    snd = synth.bassoon(f, dur * 0.6, vel)
                elif inst == "trombone":
                    snd = synth.trombone(f * 1.06, f, dur * 0.95, vel)
                elif inst == "brass":
                    snd = synth.brass(f, dur * 0.9, vel)
                elif inst == "celesta":
                    snd = synth.celesta(f, max(0.6, dur + 0.5), vel)
                else:
                    snd = synth.marimba(f, max(0.5, dur + 0.3), vel)
                synth.mix(out, snd, t, gain)
    if spec.get("role") == "loop" and spec.get("loop", True):
        n = int(round(length * SR))
        loop = out[:n] + [0.0] * max(0, n - len(out))
        for i in range(n, len(out)):  # the ring past the last bar continues at the start of the next pass
            loop[i - n] += out[i]
        return loop
    return out


_piano_cache: dict = {}


def piano(f, dur, vel, damp=None):
    key = (round(f, 3), round(dur, 3), round(vel, 3), None if damp is None else round(damp, 3))
    if key not in _piano_cache:
        _piano_cache[key] = synth.piano(f, dur, vel, damp=damp)
    return _piano_cache[key]


def load_specs(path: str) -> dict[str, dict]:
    import yaml
    with open(path, encoding="utf-8") as fh:
        docs = yaml.safe_load(fh)
    return {d["id"]: d for d in docs}
