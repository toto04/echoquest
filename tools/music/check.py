"""Check EchoQuest snippet specs against docs/music (hard errors H1-H8, call warnings W1-W11).

Usage (from the repo root):
    python3 -m tools.music.check music/library/capitolo_01.yaml

Every spec is checked in the three registers (normale, grave, acuto). Prints one line per problem
and a JSON summary; exits with status 1 if any hard error is found.
"""
from __future__ import annotations

import json
import sys

from .grid import REGISTERS, Report, check, load_specs, parse_track


def contour_and_rhythm(spec: dict) -> tuple[str, str]:
    evs = parse_track(spec["bars"], spec.get("steps_per_bar", 8), spec["id"], Report(), True, 0)
    order = "DFAC"
    contour = "*" + "".join(
        "u" if order.index(b.notes[0][0]) > order.index(a.notes[0][0]) else
        "d" if order.index(b.notes[0][0]) < order.index(a.notes[0][0]) else "r"
        for a, b in zip(evs, evs[1:]))
    rhythm = " ".join("x" if t not in "-." else t for bar in spec["bars"] for t in bar.split())
    return contour, rhythm


def main(paths: list[str]) -> int:
    summary = {"specs": 0, "errors": [], "warnings": []}
    for path in paths:
        specs = load_specs(path)
        calls = []
        for sid, spec in specs.items():
            summary["specs"] += 1
            seen: set[str] = set()
            for register in REGISTERS:
                rep = check(spec, register)
                for msg in rep.errors + rep.warnings:
                    key = msg if "register" not in msg else msg + register
                    if key in seen:
                        continue
                    seen.add(key)
                    (summary["errors"] if msg in rep.errors else summary["warnings"]).append(f"[{register}] {msg}")
            if spec.get("role") == "call":
                calls.append((sid, *contour_and_rhythm(spec)))
        # H7: two calls of one encounter may not share both contour and rhythm.
        for i, (a, ca, ra) in enumerate(calls):
            for b, cb, rb in calls[i + 1:]:
                if ca == cb and ra == rb:
                    summary["errors"].append(f"H7 {a} and {b} have the same contour {ca} and rhythm")
                elif ra == rb and sum(x != y for x, y in zip(ca, cb)) == 1:
                    summary["warnings"].append(f"W8 {a} and {b} differ by one contour symbol ({ca} / {cb}); fine inside one battle")
    for e in summary["errors"]:
        print("ERROR  ", e)
    for w in summary["warnings"]:
        print("warning", w)
    print(json.dumps({"specs": summary["specs"], "errors": len(summary["errors"]), "warnings": len(summary["warnings"])}))
    return 1 if summary["errors"] else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
