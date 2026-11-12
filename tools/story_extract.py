#!/usr/bin/env python3
"""Extract voice lines, sound cues and interactions from an EchoQuest story script.

The script is a Markdown file written by the story team:

    ## Prologo — ...            section headings (Prologo, Scena N, Epilogo)
    ### Ramo A — ...            branch headings (### or ####)
    **NARRATORE:**              a speaker block starts
    plain paragraphs            spoken text
    *[AUDIO: ...]*              stage directions: AUDIO, PAUSA, INTERAZIONE, CONVERGENZA

Outputs four CSV files (UTF-8) next to each other:

    <name>_sequenza.csv     the whole chapter in order (voices, sounds, interactions)
    <name>_voci.csv         one row per voice line, with clean text ready for TTS
    <name>_suoni.csv        one row per [AUDIO] cue, to find or produce
    <name>_interazioni.csv  [INTERAZIONE] and [CONVERGENZA] cues for gameplay

IDs look like p00_narr_01: section (p00 prologue, s01..s05 scenes, e00 epilogue),
then the character code, then a counter per section and character.
Sounds are p00_sfx_01, interactions p00_int_01.

Recorded voice files are matched from --voice-dir by name:
    p00_narr_01_a.mp3        covers p00_narr_01 (take a)
    p00_narr_02_03_a.mp3     covers p00_narr_02 and p00_narr_03 in one take
    p00_narr_02_p1_a.wav     part 1 of p00_narr_02: a voice line with N sounds inside it is
                             split into N+1 parts, played as part 1, sound 1, part 2, ...
Sound files are matched from --sfx-dir: p00_sfx_01.wav (or p00_sfx_01_<variant>.wav).

Usage:
    python tools/story_extract.py SCRIPT.md --out-dir DIR [--name NAME] [--voice-dir DIR]
"""

from __future__ import annotations

import argparse
import csv
import re
import sys
from dataclasses import dataclass, field
from pathlib import Path

CHARACTER_CODES = {
    "NARRATORE": "narr",
    "MAESTRO BATTUTA": "maes",
    "VENTO": "vent",
    "VOCE LONTANA": "lont",
}

# "## Scena 1 — Titolo", "## 1. Titolo" or "# EchoQuest — Scena 2: Titolo"
SECTION_RE = re.compile(r"^##?\s+(?:[^#\n]*?\b)?(Prologo|(?:Scena\s+)?(\d+)(?=[\s.—:-])|Epilogo)")
STOP_RE = re.compile(r"^##\s+Note di sviluppo")
SUBHEADING_RE = re.compile(r"^(#{3,4})\s+(.*)$")
# "### Ramo A — ..." names the branch; "### Se il bambino sceglie ..." gets the next letter.
BRANCH_RE = re.compile(r"\bRamo\s+([A-Z])\b")
UNNAMED_BRANCH_RE = re.compile(r"^Se\s+il\s+bambino\b", re.I)
SPEAKER_RE = re.compile(r"^\*\*([A-ZÀ-Ü][A-ZÀ-Ü ]*):\*\*\s*$")
CUE_RE = re.compile(r"^\*\[(.+)\]\*\s*$")
CUE_KIND_RE = re.compile(r"^([A-ZÀ-Ü]+(?:\s+[A-ZÀ-Ü]+)*)\s*(?:[:—-]\s*(.*))?$", re.S)
VOICE_FILE_RE = re.compile(r"^([a-z]\d\d)_([a-z]+)_(\d\d)(?:_(\d\d))?(?:_p(\d+))?_([a-z])$")
SFX_FILE_RE = re.compile(r"^([a-z]\d\d_sfx_\d\d)(?:_[a-z0-9]+)?$")
AUDIO_EXTS = {".mp3", ".wav", ".ogg", ".flac"}


@dataclass
class Item:
    kind: str  # voce | suono | interazione | convergenza
    id: str
    section: str
    branch: str
    line: int
    character: str = ""
    text: str = ""  # voice: text with inline markers; others: description
    tts: str = ""
    inside: str = ""  # sound/pause played inside this voice line
    files: list[str] = field(default_factory=list)


def clean_markdown(text: str) -> str:
    text = re.sub(r"\*\*(.+?)\*\*", r"\1", text)
    text = re.sub(r"\*(.+?)\*", r"\1", text)
    return text.strip()


def section_code(match: re.Match) -> str:
    if match.group(1) == "Prologo":
        return "p00"
    if match.group(1) == "Epilogo":
        return "e00"
    return f"s{int(match.group(2)):02d}"


class Extractor:
    def __init__(self) -> None:
        self.items: list[Item] = []
        self.section = ""
        self.branch = ""
        self.branch_level = 0
        self.counters: dict[str, int] = {}
        # open speaker block
        self.speaker = ""
        self.block_parts: list[tuple[str, object]] = []  # ("text", str) | ("cue", Item|str)
        self.block_line = 0

    def next_id(self, code: str) -> str:
        key = f"{self.section}_{code}"
        self.counters[key] = self.counters.get(key, 0) + 1
        return f"{key}_{self.counters[key]:02d}"

    def close_block(self) -> None:
        if not self.speaker:
            return
        parts = self.block_parts
        last_text = max((i for i, (k, _) in enumerate(parts) if k == "text"), default=-1)
        body, trailing = parts[: last_text + 1], parts[last_text + 1 :]
        if last_text >= 0:
            code = CHARACTER_CODES.get(self.speaker)
            if code is None:
                sys.exit(f"Unknown character {self.speaker!r} at line {self.block_line}: add it to CHARACTER_CODES")
            voice = Item("voce", self.next_id(code), self.section, self.branch, self.block_line, self.speaker)
            shown, spoken, inline = [], [], []
            for kind, value in body:
                if kind == "text":
                    shown.append(value)
                    spoken.append(value)
                elif isinstance(value, Item):  # inline sound
                    value.inside = voice.id
                    shown.append(f"[{value.id}]")
                    spoken.append("...")
                    inline.append(value)
                else:  # inline pause or other direction
                    shown.append(f"[{value}]")
                    spoken.append("...")
            voice.text = " ".join(shown)
            voice.tts = re.sub(r"(\.\.\.\s*){2,}", "... ", " ".join(spoken)).strip()
            self.items.append(voice)
            self.items.extend(inline)
        for _, value in trailing:
            if isinstance(value, Item):
                self.items.append(value)
        self.speaker = ""
        self.block_parts = []

    def make_cue(self, raw: str, line: int) -> Item | str | None:
        raw = clean_markdown(raw)
        m = CUE_KIND_RE.match(raw)
        kind, desc = (m.group(1), (m.group(2) or "").strip()) if m else (raw, "")
        if kind == "AUDIO":
            return Item("suono", self.next_id("sfx"), self.section, self.branch, line, text=desc)
        if kind.startswith("INTERAZIONE"):
            mode = kind.replace("INTERAZIONE", "").strip()
            text = f"{mode}: {desc}" if mode else desc
            return Item("interazione", self.next_id("int"), self.section, self.branch, line, text=text)
        if kind == "CONVERGENZA":  # the point where branches join again: belongs to no branch
            return Item("convergenza", self.next_id("int"), self.section, "", line, text=desc)
        return raw  # PAUSA, PAUSA BREVE, PAUSA DRAMMATICA, ...

    def add_cue(self, raw: str, line: int) -> None:
        cue = self.make_cue(raw, line)
        if self.speaker:
            self.block_parts.append(("cue", cue))
        elif isinstance(cue, Item):
            self.items.append(cue)
        if isinstance(cue, Item) and cue.kind == "convergenza":
            self.close_block()
            self.branch, self.branch_level = "", 0

    def run(self, lines: list[str]) -> list[Item]:
        for n, raw in enumerate(lines, start=1):
            line = raw.strip()
            if not line or line == "---":
                continue
            if STOP_RE.match(line):
                break
            if m := SECTION_RE.match(line):
                self.close_block()
                self.section = section_code(m)
                self.branch, self.branch_level = "", 0
                continue
            if not self.section:
                continue
            if m := SUBHEADING_RE.match(line):
                self.close_block()
                level = len(m.group(1))
                if b := BRANCH_RE.search(m.group(2)):
                    self.branch, self.branch_level = b.group(1), level
                elif UNNAMED_BRANCH_RE.match(m.group(2)):
                    following = self.branch and level == self.branch_level
                    self.branch = chr(ord(self.branch) + 1) if following else "A"
                    self.branch_level = level
                elif self.branch and level <= self.branch_level:
                    self.branch, self.branch_level = "", 0
                continue
            if m := SPEAKER_RE.match(line):
                self.close_block()
                self.speaker, self.block_line = m.group(1).strip(), n
                continue
            if m := CUE_RE.match(line):
                self.add_cue(m.group(1), n)
                continue
            if self.speaker:
                self.block_parts.append(("text", clean_markdown(line)))
            else:
                print(f"warning: line {n} is outside any speaker block, skipped: {line[:60]}", file=sys.stderr)
        self.close_block()
        return self.items


def attach_voice_files(items: list[Item], voice_dir: Path | None) -> None:
    if not voice_dir or not voice_dir.is_dir():
        return
    by_id = {it.id: it for it in items if it.kind == "voce"}
    for path in sorted(voice_dir.iterdir()):
        if path.suffix.lower() not in AUDIO_EXTS:
            continue
        m = VOICE_FILE_RE.match(path.stem)
        if not m:
            print(f"warning: voice file with unexpected name, ignored: {path.name}", file=sys.stderr)
            continue
        section, code, first, last, _part, _take = m.groups()
        for k in range(int(first), int(last or first) + 1):
            voice_id = f"{section}_{code}_{k:02d}"
            if voice_id in by_id:
                by_id[voice_id].files.append(path.name)
            else:
                print(f"warning: {path.name} refers to {voice_id}, which is not in the script", file=sys.stderr)


def attach_sfx_files(items: list[Item], sfx_dir: Path | None) -> None:
    if not sfx_dir or not sfx_dir.is_dir():
        return
    by_id = {it.id: it for it in items if it.kind == "suono"}
    for path in sorted(sfx_dir.iterdir()):
        if path.suffix.lower() not in AUDIO_EXTS:
            continue
        if path.stem.startswith("x_"):
            continue  # extra beds used only by the staging (e.g. x_vento_alberi)
        m = SFX_FILE_RE.match(path.stem)
        if m and m.group(1) in by_id:
            by_id[m.group(1)].files.append(path.name)
        else:
            print(f"warning: sound file not matching any cue, ignored: {path.name}", file=sys.stderr)


def write_csv(path: Path, header: list[str], rows: list[list[object]]) -> None:
    with path.open("w", encoding="utf-8", newline="") as f:
        writer = csv.writer(f)
        writer.writerow(header)
        writer.writerows(rows)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("script", type=Path)
    parser.add_argument("--out-dir", type=Path, required=True)
    parser.add_argument("--name", help="output file prefix (default: script file name)")
    parser.add_argument("--voice-dir", type=Path, help="folder with recorded voice files")
    parser.add_argument("--sfx-dir", type=Path, help="folder with sound files named after their cue id")
    args = parser.parse_args()

    items = Extractor().run(args.script.read_text(encoding="utf-8").splitlines())
    attach_voice_files(items, args.voice_dir)
    attach_sfx_files(items, args.sfx_dir)
    name = args.name or args.script.stem
    out = args.out_dir
    out.mkdir(parents=True, exist_ok=True)

    write_csv(
        out / f"{name}_sequenza.csv",
        ["ordine", "sezione", "ramo", "tipo", "id", "personaggio", "testo", "dentro_battuta", "file_audio", "riga_copione"],
        [
            [i, it.section, it.branch, it.kind, it.id, it.character, it.text, it.inside, " ".join(it.files), it.line]
            for i, it in enumerate(items, start=1)
        ],
    )
    voices = [it for it in items if it.kind == "voce"]
    write_csv(
        out / f"{name}_voci.csv",
        ["id", "sezione", "ramo", "personaggio", "testo_tts", "caratteri", "file_audio", "stato", "riga_copione"],
        [
            [it.id, it.section, it.branch, it.character, it.tts, len(it.tts), " ".join(it.files),
             "registrata" if it.files else "da registrare", it.line]
            for it in voices
        ],
    )
    sounds = [it for it in items if it.kind == "suono"]
    write_csv(
        out / f"{name}_suoni.csv",
        ["id", "sezione", "ramo", "descrizione", "dentro_battuta", "file_audio", "stato", "riga_copione"],
        [[it.id, it.section, it.branch, it.text, it.inside, " ".join(it.files),
          "presente" if it.files else "da trovare", it.line] for it in sounds],
    )
    actions = [it for it in items if it.kind in ("interazione", "convergenza")]
    write_csv(
        out / f"{name}_interazioni.csv",
        ["id", "sezione", "ramo", "tipo", "descrizione", "riga_copione"],
        [[it.id, it.section, it.branch, it.kind, it.text, it.line] for it in actions],
    )

    chars = sum(len(v.tts) for v in voices)
    recorded = sum(1 for v in voices if v.files)
    print(f"{len(voices)} voice lines ({chars} characters, {recorded} recorded), "
          f"{len(sounds)} sounds, {len(actions)} interactions -> {out}")


if __name__ == "__main__":
    main()
