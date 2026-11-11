#!/usr/bin/env python3
"""Remove words that were cut from the script out of an existing voice recording.

When a line is shortened in the copione (sentences deleted, nothing added), the recording does not
need to be redone: this finds the deleted words in the audio (whisper word timestamps, aligned to the
old text) and cuts each deleted stretch out at the pauses around it, keeping half of the pause on each
side so the result breathes like the original.

Usage:
    python3 tools/voice_trim.py IN.wav OUT.wav --old-text "..." --new-text "..." --whisper-model MODEL

Then check the printed transcript of OUT. Needs ffmpeg and whisper-cli on PATH.
For lines processed by stretch_pauses.py, trim the file in --originals-dir and run stretch_pauses again.
"""

from __future__ import annotations

import argparse
import array
import difflib
import math
import re
import subprocess
import sys
import wave
from pathlib import Path

TS = re.compile(r"\[(\d+):(\d+):([\d.]+) --> (\d+):(\d+):([\d.]+)\]\s*(.*)")


def norm(text: str) -> list[str]:
    text = re.sub(r"\[[^\]]*\]", " ", text.lower())
    return [w for w in re.split(r"[^a-zàèéìòóù]+", text) if w]


def decode(path: Path) -> tuple[array.array, int]:
    rate = int(subprocess.check_output(
        ["ffprobe", "-v", "error", "-select_streams", "a:0", "-show_entries", "stream=sample_rate",
         "-of", "csv=p=0", str(path)], text=True).strip())
    raw = subprocess.check_output(["ffmpeg", "-v", "error", "-i", str(path), "-ac", "1", "-f", "s16le", "-"])
    x = array.array("h")
    x.frombytes(raw)
    if sys.byteorder != "little":
        x.byteswap()
    return x, rate


def write_wav(path: Path, x: array.array, rate: int) -> None:
    data = array.array("h", x)
    if sys.byteorder != "little":
        data.byteswap()
    with wave.open(str(path), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(rate)
        w.writeframes(data.tobytes())


def whisper_words(path: Path, model: str) -> list[tuple[float, float, str]]:
    out = subprocess.run(["whisper-cli", "-m", model, "-l", "it", "-np", "-ml", "1", "-sow", "-t", "8", str(path)],
                         capture_output=True, text=True).stdout
    words = []
    for line in out.splitlines():
        m = TS.match(line.strip())
        if not m:
            continue
        a = int(m.group(1)) * 3600 + int(m.group(2)) * 60 + float(m.group(3))
        b = int(m.group(4)) * 3600 + int(m.group(5)) * 60 + float(m.group(6))
        for w in norm(m.group(7)):
            words.append((a, b, w))
    return words


def silences(x: array.array, rate: int, threshold_db: float = -40.0, min_len: float = 0.06) -> list[tuple[float, float]]:
    frame = max(1, rate // 100)
    limit = 32768 * 10 ** (threshold_db / 20)
    runs, start = [], None
    n = (len(x) + frame - 1) // frame
    for k in range(n + 1):
        seg = x[k * frame:(k + 1) * frame] if k < n else None
        quiet = bool(seg) and math.sqrt(sum(s * s for s in seg) / len(seg)) < limit
        if quiet and start is None:
            start = k
        elif not quiet and start is not None:
            if (k - start) * frame / rate >= min_len:
                runs.append((start * frame / rate, min(len(x), k * frame) / rate))
            start = None
    return runs


def main() -> None:
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("input", type=Path)
    p.add_argument("output", type=Path)
    p.add_argument("--old-text", required=True, help="the text the recording was made from")
    p.add_argument("--new-text", required=True, help="the shortened text (only deletions)")
    p.add_argument("--whisper-model", required=True)
    args = p.parse_args()

    old, new = norm(args.old_text), norm(args.new_text)
    keep = set()
    for blk in difflib.SequenceMatcher(None, old, new, autojunk=False).get_matching_blocks():
        keep.update(range(blk.a, blk.a + blk.size))
    deleted = [i for i in range(len(old)) if i not in keep]
    if not deleted:
        sys.exit("nothing to delete: the new text has every word of the old one")

    x, rate = decode(args.input)
    dur = len(x) / rate
    words = whisper_words(args.input, args.whisper_model)
    # script word index -> transcript word index (the file may hold only part of the line)
    s2t = {}
    for blk in difflib.SequenceMatcher(None, old, [w for _, _, w in words], autojunk=False).get_matching_blocks():
        for k in range(blk.size):
            s2t[blk.a + k] = blk.b + k

    # Contiguous runs of deleted script words that are in this file.
    runs, cur = [], []
    for i in range(len(old)):
        if i in keep:
            if cur:
                runs.append(cur)
                cur = []
        else:
            cur.append(i)
    if cur:
        runs.append(cur)

    # Pauses of at least 0.12 s: shorter quiet spots can be the closure before a consonant, inside a word.
    sil = silences(x, rate, min_len=0.12)
    cuts = []
    for run in runs:
        t_idx = [s2t[i] for i in run if i in s2t]
        if len(t_idx) < max(1, len(run) // 2):
            continue  # not (or barely) in this file
        first, last = words[min(t_idx)], words[max(t_idx)]
        prev_kept = next((s2t[i] for i in range(run[0] - 1, -1, -1) if i in s2t and i in keep), None)
        next_kept = next((s2t[i] for i in range(run[-1] + 1, len(old)) if i in s2t and i in keep), None)
        # whisper's word intervals touch each other and a pause can end up in the word before or after
        # it, so take the real pause closest to each boundary, without crossing a kept word.
        floor = words[prev_kept][0] if prev_kept is not None else 0.0
        ceil = words[next_kept][1] - 0.05 if next_kept is not None else dur
        before = [s for s in sil if s[0] >= floor and s[1] >= first[0] - 0.3 and s[0] <= first[1] + 0.3]
        after = [s for s in sil if s[0] >= last[0] and s[1] >= last[0] + 0.1 and s[0] <= ceil]
        if before and after:
            after = [s for s in after if s[0] > max(before, key=lambda s: s[1])[0]] or after
        if prev_kept is None:
            a = 0.0
        elif before:
            s = min(before, key=lambda s: abs((s[0] + s[1]) / 2 - first[0]))
            a = (s[0] + s[1]) / 2
        else:
            sys.exit(f"no pause before '{' '.join(old[i] for i in run)}': trim by hand")
        if next_kept is None:
            b = dur
            if prev_kept is not None:
                a = min(a, s[0] + 0.25)  # the line now ends here: keep a short tail of the pause
        elif after:
            s2 = min(after, key=lambda s: abs((s[0] + s[1]) / 2 - last[1]))
            b = (s2[0] + s2[1]) / 2 if prev_kept is not None else max(0.0, s2[1] - 0.08)
        else:
            sys.exit(f"no pause after '{' '.join(old[i] for i in run)}': trim by hand")
        cuts.append((a, b, " ".join(old[i] for i in run)))

    out = array.array("h")
    pos = 0
    fade = rate // 200
    for a, b, text in sorted(cuts):
        ia, ib = int(a * rate), int(b * rate)
        seg = x[pos:ia]
        for k in range(min(fade, len(seg))):
            seg[-1 - k] = int(seg[-1 - k] * k / fade)
        out.extend(seg)
        pos = ib
        print(f"cut {a:6.2f}-{b:6.2f}s: {text}")
    seg = x[pos:]
    for k in range(min(fade, len(seg))):
        seg[k] = int(seg[k] * k / fade)
    out.extend(seg)
    write_wav(args.output, out, rate)
    print(f"{dur:.1f}s -> {len(out) / rate:.1f}s")
    print("now says:", " ".join(w for _, _, w in whisper_words(args.output, args.whisper_model)))


if __name__ == "__main__":
    main()
