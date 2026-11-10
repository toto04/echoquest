#!/usr/bin/env python3
"""Lengthen the pauses between sentences inside voice recordings.

TTS voices (ElevenLabs) often read a paragraph a little too fast for young listeners.
This finds the silent gaps inside each recording and makes the longer ones (sentence ends)
even longer, by repeating a slice of the gap itself, so the background noise stays the same.

The untouched recordings are kept in --originals-dir (moved there the first time), and the
processed copies are always rebuilt from them, so running the script again is safe and
changing --extra simply redoes everything.

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


def stretch(x: array.array, rate: int, pauses: list[tuple[int, int]], extra: float) -> array.array:
    out = array.array("h")
    pos = 0
    add = int(extra * rate)
    for a, b in pauses:
        mid = (a + b) // 2
        out.extend(x[pos:mid])
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
    args = p.parse_args()

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
        pauses = find_pauses(x, rate, args.threshold_db, args.min_pause)
        y = stretch(x, rate, pauses, args.extra)
        write_wav(args.voice_dir / (orig.stem + ".wav"), y, rate)
        total += 1
        print(f"{orig.stem}: {len(pauses)} pauses lengthened, {len(x) / rate:.1f}s -> {len(y) / rate:.1f}s")
    print(f"{total} files written to {args.voice_dir}")


if __name__ == "__main__":
    main()
