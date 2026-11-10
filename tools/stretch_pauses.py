#!/usr/bin/env python3
"""Lengthen the pauses between sentences inside voice recordings.

TTS voices (ElevenLabs) often read a paragraph a little too fast for young listeners.
This finds the silent gaps inside each recording and makes the longer ones (sentence ends)
even longer, by repeating a slice of the gap itself, so the background noise stays the same.

The untouched recordings are kept in --originals-dir (moved there the first time), and the
processed copies are always rebuilt from them, so running the script again is safe and
changing --extra simply redoes everything.

Questions: with --question-extra (and --whisper-model) the gap after every "?" gets that much
extra silence instead, so a listener has time to think about the question.

Usage (from the repo root):
    V=unity/EchoQuestTest/Assets/EchoQuest/Story/Audio/Voice
    python3 tools/stretch_pauses.py --voice-dir $V --originals-dir $V/_originali --chars narr

Needs ffmpeg on PATH. Output files are 16-bit WAV, mono, at the original sample rate.
"""

from __future__ import annotations

import argparse
import array
import math
import re
import shutil
import subprocess
import sys
import wave
from pathlib import Path

AUDIO_EXTS = {".mp3", ".wav", ".ogg", ".flac"}
VOICE_RE = re.compile(r"^[a-z]\d\d_([a-z]+)_\d\d")


def decode(path: Path) -> tuple[array.array, int]:
    rate = int(subprocess.check_output(
        ["ffprobe", "-v", "error", "-select_streams", "a:0", "-show_entries", "stream=sample_rate",
         "-of", "csv=p=0", str(path)], text=True).strip())
    raw = subprocess.check_output(
        ["ffmpeg", "-v", "error", "-i", str(path), "-ac", "1", "-f", "s16le", "-acodec", "pcm_s16le", "-"])
    samples = array.array("h")
    samples.frombytes(raw)
    if sys.byteorder != "little":
        samples.byteswap()
    return samples, rate


def find_pauses(x: array.array, rate: int, threshold_db: float, min_pause: float) -> list[tuple[int, int]]:
    """Silent runs (start, end) in samples, excluding leading and trailing silence."""
    frame = max(1, rate // 100)  # 10 ms
    limit = 32768 * 10 ** (threshold_db / 20)
    quiet = []
    for i in range(0, len(x), frame):
        seg = x[i:i + frame]
        rms = math.sqrt(sum(s * s for s in seg) / len(seg)) if seg else 0.0
        quiet.append(rms < limit)
    runs, start = [], None
    for k, q in enumerate(quiet + [False]):
        if q and start is None:
            start = k
        elif not q and start is not None:
            runs.append((start * frame, min(len(x), k * frame)))
            start = None
    if runs and runs[0][0] == 0:
        runs = runs[1:]
    if runs and runs[-1][1] >= len(x) - frame:
        runs = runs[:-1]
    return [(a, b) for a, b in runs if (b - a) / rate >= min_pause]


def question_ends(path: Path, model: str) -> list[float]:
    """Times (s) where a word ending with '?' ends, from whisper's word timestamps."""
    res = subprocess.run(["whisper-cli", "-m", model, "-l", "it", "-np", "-ml", "1", "-sow", "-t", "8", str(path)],
                         capture_output=True, text=True)
    ends = []
    for line in res.stdout.splitlines():
        m = re.match(r"\[(\d+):(\d+):([\d.]+) --> (\d+):(\d+):([\d.]+)\]\s*(.*)", line.strip())
        if m and "?" in m.group(7):
            start = int(m.group(1)) * 3600 + int(m.group(2)) * 60 + float(m.group(3))
            end = int(m.group(4)) * 3600 + int(m.group(5)) * 60 + float(m.group(6))
            ends.append((start, end))
    return ends


def quietest(x: array.array, rate: int, a: float, b: float) -> int:
    """Sample index of the quietest 20 ms between a and b seconds."""
    lo, hi = max(0, int(a * rate)), min(len(x), int(b * rate))
    win, hop = max(1, rate // 50), max(1, rate // 200)
    if hi - lo <= win:
        return (lo + hi) // 2
    best = min(range(lo, hi - win, hop), key=lambda i: sum(abs(v) for v in x[i:i + win]))
    return best + win // 2


def stretch(x: array.array, rate: int, pauses: list[tuple[int, int, float]]) -> array.array:
    """pauses: (start, end, seconds to add), start/end in samples."""
    out = array.array("h")
    pos = 0
    for a, b, extra in pauses:
        add = int(extra * rate)
        mid = (a + b) // 2
        out.extend(x[pos:mid])
        if b - a < rate // 20:
            # No real gap (the voice ran on): insert plain silence, with tiny fades so it doesn't click.
            fade = min(len(out), rate // 200)
            for k in range(fade):
                out[-1 - k] = int(out[-1 - k] * k / fade)
            out.extend(array.array("h", [0]) * add)
            tail = x[mid:mid + fade]
            out.extend(array.array("h", (int(v * k / max(1, fade)) for k, v in enumerate(tail))))
            pos = mid + len(tail)
            continue
        # Repeat a slice of the pause itself (centred), as many times as needed.
        half = max(1, (b - a) // 4)
        fill = x[mid - half:mid + half] or array.array("h", [0])
        while add > 0 and len(fill) < add:
            fill = fill + fill
        out.extend(fill[:add])
        pos = mid
    out.extend(x[pos:])
    return out


def write_wav(path: Path, x: array.array, rate: int) -> None:
    data = array.array("h", x)
    if sys.byteorder != "little":
        data.byteswap()
    with wave.open(str(path), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(rate)
        w.writeframes(data.tobytes())


def main() -> None:
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("--voice-dir", type=Path, required=True)
    p.add_argument("--originals-dir", type=Path, required=True)
    p.add_argument("--chars", default="narr", help="comma-separated character codes to process (default: narr)")
    p.add_argument("--min-pause", type=float, default=0.28, help="only gaps at least this long (s) are lengthened")
    p.add_argument("--extra", type=float, default=0.35, help="seconds added to each such gap")
    p.add_argument("--threshold-db", type=float, default=-38.0, help="level below which audio counts as silence")
    p.add_argument("--question-extra", type=float, default=0.0,
                   help="seconds added after a question inside a line (needs whisper-cli and --whisper-model)")
    p.add_argument("--whisper-model", help="path to a whisper.cpp ggml model, for --question-extra")
    args = p.parse_args()
    if args.question_extra > 0 and not args.whisper_model:
        p.error("--question-extra needs --whisper-model")

    chars = {c.strip() for c in args.chars.split(",") if c.strip()}
    args.originals_dir.mkdir(parents=True, exist_ok=True)

    # 1. Move new recordings of these characters into the originals folder.
    for f in sorted(args.voice_dir.iterdir()):
        m = VOICE_RE.match(f.stem)
        if f.is_file() and f.suffix.lower() in AUDIO_EXTS and m and m.group(1) in chars:
            target = args.originals_dir / f.name
            if target.exists():
                continue  # already an original: f is a processed copy from a previous run
            shutil.move(str(f), target)
            meta = f.with_name(f.name + ".meta")
            if meta.exists():
                meta.unlink()  # Unity makes a new one for the processed file

    # 2. Rebuild every processed copy from its original.
    total = 0
    for orig in sorted(args.originals_dir.iterdir()):
        m = VOICE_RE.match(orig.stem)
        if not (orig.is_file() and orig.suffix.lower() in AUDIO_EXTS and m and m.group(1) in chars):
            continue
        x, rate = decode(orig)
        plan = [(a, b, args.extra) for a, b in find_pauses(x, rate, args.threshold_db, args.min_pause)]
        questions = 0
        if args.question_extra > 0:
            # Any gap (even a short one) right after a question gets the longer pause.
            gaps = find_pauses(x, rate, args.threshold_db, 0.08)
            for qs, qe in question_ends(orig, args.whisper_model):
                # whisper often stretches the last word's end into the pause: look from the
                # second half of the word to a little after it.
                lo_t = qs + 0.5 * (qe - qs)
                near = [g for g in gaps if lo_t <= g[0] / rate <= qe + 0.6]
                if near:
                    g = min(near, key=lambda g: g[0])
                else:
                    if qe >= len(x) / rate - 0.3:
                        continue  # the question ends the file: the player pauses after it
                    p0 = quietest(x, rate, qe - 0.4, qe + 0.3)
                    g = (p0, p0)
                plan = [p for p in plan if (p[0], p[1]) != g] + [(g[0], g[1], max(args.extra, args.question_extra))]
                questions += 1
            plan.sort()
        y = stretch(x, rate, plan)
        write_wav(args.voice_dir / (orig.stem + ".wav"), y, rate)
        total += 1
        print(f"{orig.stem}: {len(plan)} pauses lengthened ({questions} after questions), "
              f"{len(x) / rate:.1f}s -> {len(y) / rate:.1f}s")
    print(f"{total} files written to {args.voice_dir}")


if __name__ == "__main__":
    main()
