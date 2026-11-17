"""Render snippet specs to WAV files for listening (mono 48 kHz 16-bit).

Usage (from the repo root):
    python3 -m tools.music.render music/library/capitolo_01.yaml OUT_DIR [id ...] [--register grave|acuto|normale]

Specs that depend on the register (calls, stings, the battle loop, the reprise) are rendered once per
register unless --register is given. The game's own files are written by tools/story_placeholder_sfx.py,
which calls the same renderer.
"""
from __future__ import annotations

import os
import struct
import sys
import wave

from .grid import REGISTERS, load_specs, render
from .synth import SR


def write_wav(path: str, x: list[float], gain: float = 1.0) -> None:
    """Writes with a fixed gain (no per-file normalisation), so stems keep their relative balance.
    Raises if the result would clip."""
    peak = max((abs(v) for v in x), default=0.0) * gain
    if peak > 0.98:
        raise ValueError(f"{path}: peak {peak:.2f} would clip; lower the gain")
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(struct.pack(f"<{len(x)}h", *(int(v * gain * 32767) for v in x)))


def depends_on_register(spec: dict) -> bool:
    if spec.get("role") in ("call", "answer"):
        return True
    return any(t.get("buttons") or t.get("register") for t in (spec.get("tracks") or {}).values())


def main(argv: list[str]) -> int:
    args = [a for a in argv if not a.startswith("--")]
    register = None
    if "--register" in argv:
        register = argv[argv.index("--register") + 1]
        args.remove(register)
    path, out, ids = args[0], args[1], set(args[2:])
    os.makedirs(out, exist_ok=True)
    for sid, spec in load_specs(path).items():
        if ids and sid not in ids:
            continue
        regs = [register] if register else (["grave", "acuto"] if depends_on_register(spec) else ["normale"])
        for reg in regs:
            x = render(spec, reg)
            name = sid if reg == "normale" else f"{sid}_{reg}"
            peak = max(abs(v) for v in x) or 1.0
            write_wav(os.path.join(out, name + ".wav"), x, 0.7 / peak)
            print(f"{name}.wav  {len(x) / SR:.2f}s")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
