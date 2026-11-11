#!/usr/bin/env python3
"""Lengthen the pauses between sentences inside voice recordings.

TTS voices (ElevenLabs) often read a paragraph a little too fast for young listeners.
This finds the silent gaps inside each recording and makes the longer ones (sentence ends)
even longer, by adding silence at the quietest point of each gap.

The untouched recordings are kept in --originals-dir (moved there the first time), and the
processed copies are always rebuilt from them, so running the script again is safe and
changing --extra simply redoes everything.

Questions: with --question-extra (and --whisper-model) the pause after every "?" gets that much
extra silence instead, so a listener has time to think about the question. Only a real pause
(--question-min-pause) is lengthened: when the voice runs straight on, nothing is inserted, because
the quietest spot near a word boundary is often the closure before a consonant ("p", "t") and
silence put there splits the word.

Usage (from the repo root):
    V=unity/EchoQuestTest/Assets/EchoQuest/Story/Audio/Voice
    python3 tools/stretch_pauses.py --voice-dir $V --originals-dir $V/_originali --chars narr

Needs ffmpeg on PATH. Output files are 16-bit WAV, mono, at the original sample rate.
"""

from __future__ import annotations

import argparse
import array
import csv
import fnmatch
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


def lines_with_inner_question(sequence: Path) -> set[str]:
    """Ids of voice lines with a "?" followed by more words (only these need whisper)."""
    ids = set()
    for row in csv.DictReader(open(sequence, encoding="utf-8")):
        text = re.sub(r"\[[^\]]*\]", " ", row.get("testo", ""))
        if re.search(r"\?[^?]*[A-Za-zÀ-ÿ]", text):
            ids.add(row["id"])
    return ids


def line_ids(stem: str) -> list[str]:
    """p00_narr_02_03_p1_a -> [p00_narr_02, p00_narr_03]"""
    m = re.match(r"^([a-z]\d\d_[a-z]+_)(\d\d)(?:_(\d\d))?", stem)
    first, last = int(m.group(2)), int(m.group(3) or m.group(2))
    return [f"{m.group(1)}{n:02d}" for n in range(first, last + 1)]


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


def quietest(x: array.array, a: int, b: int, win: int) -> int:
    """Sample index at the centre of the quietest `win` samples between a and b."""
    if b - a <= win:
        return (a + b) // 2
    hop = max(1, win // 4)
    best = min(range(a, b - win, hop), key=lambda i: sum(abs(v) for v in x[i:i + win]))
    return best + win // 2


def stretch(x: array.array, rate: int, pauses: list[tuple[int, int, float]]) -> array.array:
    """pauses: (start, end, seconds to add), start/end in samples.

    The extra time is plain silence put at the quietest point of each pause, with short fades.
    (Repeating a slice of the pause instead would also repeat any breath or word tail in it,
    which sounds like the speaker puffing several times.)"""
    out = array.array("h")
    pos = 0
    fade = max(1, rate // 100)  # 10 ms
    for a, b, extra in pauses:
        p = max(pos + fade, quietest(x, a, b, max(1, rate // 50)))
        seg = x[pos:p]
        for k in range(min(fade, len(seg))):
            seg[-1 - k] = int(seg[-1 - k] * k / fade)
        out.extend(seg)
        out.extend(array.array("h", [0]) * int(extra * rate))
        tail = x[p:p + fade]
        out.extend(array.array("h", (int(v * k / fade) for k, v in enumerate(tail))))
        pos = p + len(tail)
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
    p.add_argument("--question-min-pause", type=float, default=0.15,
                   help="a pause after a question must be at least this long (s) to be lengthened")
    p.add_argument("--whisper-model", help="path to a whisper.cpp ggml model, for --question-extra")
    p.add_argument("--sequence", type=Path,
                   help="the sequenza CSV: whisper then runs only on lines with a question before their end")
    p.add_argument("--only", action="append", default=[],
                   help="only rebuild files whose name matches this pattern (e.g. 's01_maes_1*'); repeatable")
    args = p.parse_args()
    if args.question_extra > 0 and not args.whisper_model:
        p.error("--question-extra needs --whisper-model")

    chars = {c.strip() for c in args.chars.split(",") if c.strip()}
    inner_q = lines_with_inner_question(args.sequence) if args.sequence else None
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
        if args.only and not any(fnmatch.fnmatch(orig.stem, pat) for pat in args.only):
            continue
        x, rate = decode(orig)
        plan = [(a, b, args.extra) for a, b in find_pauses(x, rate, args.threshold_db, args.min_pause)]
        questions = 0
        if args.question_extra > 0 and (inner_q is None or any(i in inner_q for i in line_ids(orig.stem))):
            # A real pause right after a question gets the longer pause.
            gaps = find_pauses(x, rate, args.threshold_db, args.question_min_pause)
            for qs, qe in question_ends(orig, args.whisper_model):
                # whisper often stretches the last word's end into the pause: look from the
                # second half of the word to a little after it.
                lo_t = qs + 0.5 * (qe - qs)
                near = [g for g in gaps if lo_t <= g[0] / rate <= qe + 0.6]
                if not near:
                    continue  # no pause (or the question ends the file: the player pauses after it)
                g = min(near, key=lambda g: g[0])
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
