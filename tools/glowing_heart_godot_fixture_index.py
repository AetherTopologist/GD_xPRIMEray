#!/usr/bin/env python3
"""Generate a preview static index of Godot fixture candidates.

This tool scans repository files only. It does not execute Godot, modify scenes,
touch Core transport, or update the production Observatory catalog.
"""

from __future__ import annotations

import argparse
import json
import re
from collections import Counter
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


DEFAULT_OUTPUT_JSON = Path("reports/glowing_heart_godot_fixture_candidates.preview.json")
DEFAULT_OUTPUT_MD = Path("reports/glowing_heart_godot_fixture_candidates.preview.md")
SCAN_ROOTS = (Path("Fixtures"), Path("fixtures"), Path("Docs"), Path("reports"))
SOURCE_EXTENSIONS = {".tscn", ".md", ".json"}
TAGS = (
    "fixture",
    "grin",
    "hermetic",
    "curved",
    "blackhole",
    "einstein",
    "boundary",
    "wormhole",
    "observatory",
    "atomic",
    "portal",
    "demo",
    "closure",
    "metric",
    "visual",
    "sequence",
)
PRIORITY_TERMS = {
    "fixture",
    "grin",
    "hermetic",
    "curved",
    "blackhole",
    "einstein",
    "boundary",
    "wormhole",
    "observatory",
}
CATEGORY_PRIORITY = {
    "READY_CANDIDATE": 0,
    "NEEDS_REVIEW": 1,
    "EXPERIMENTAL": 2,
    "UNKNOWN": 3,
}
SOURCE_PRIORITY = {
    "tscn": 0,
    "md": 1,
    "json": 2,
}


@dataclass(frozen=True)
class Candidate:
    name: str
    path: str
    source_type: str
    detected_tags: list[str]
    likely_category: str
    transport_hint: str
    closure_hint: str
    godot_runtime_required: bool
    parity_claim: str
    notes: str


def read_sample(path: Path, limit: int = 32_768) -> str:
    try:
        return path.read_text(encoding="utf-8", errors="ignore")[:limit]
    except OSError:
        return ""


def discover_source_files(roots: tuple[Path, ...]) -> list[Path]:
    files: list[Path] = []
    for root in roots:
        if not root.exists():
            continue
        for path in root.rglob("*"):
            if (
                path.is_file()
                and path.suffix.lower() in SOURCE_EXTENSIONS
                and not (path.parts and path.parts[0] == "reports" and path.name.startswith("glowing_heart_"))
            ):
                files.append(path)
    return sorted(files)


def normalized_words(text: str) -> set[str]:
    return {part for part in re.split(r"[^a-z0-9]+", text.lower()) if part}


def detect_tags(path: Path, sample: str) -> list[str]:
    haystack = f"{path.as_posix()} {sample}".lower()
    words = normalized_words(haystack)
    detected: list[str] = []
    for tag in TAGS:
        if tag in words or tag in haystack:
            detected.append(tag)
    return detected


def is_candidate_file(path: Path, tags: list[str], sample: str) -> bool:
    path_text = path.as_posix().lower()
    stem_text = path.stem.lower()
    tag_set = set(tags)

    if "project_glowing_heart" in stem_text:
        return False

    if path.suffix.lower() == ".tscn":
        return bool(tag_set & PRIORITY_TERMS) or "fixture" in stem_text

    if path.suffix.lower() == ".json" and "observatory_fixtures" in path_text:
        return True

    if "fixture" in stem_text or "observatory_fixture" in path_text:
        return bool(tag_set) or "fixture" in sample.lower()

    if "canonical_fixtures" in path_text:
        return True

    return False


def likely_category(tags: list[str], path: Path) -> str:
    tag_set = set(tags)
    text = path.as_posix().lower()

    if tag_set & {"wormhole", "portal", "demo", "atomic", "visual", "sequence"}:
        if not (tag_set & {"hermetic", "closure", "boundary", "curved", "blackhole", "einstein"}):
            return "EXPERIMENTAL"

    if tag_set & {"hermetic", "closure", "boundary", "curved", "blackhole", "einstein"}:
        return "READY_CANDIDATE"

    if "grin" in tag_set and "observatory" in tag_set:
        return "NEEDS_REVIEW"

    if "fixture" in tag_set or "observatory" in tag_set:
        return "NEEDS_REVIEW"

    if tag_set & {"grin", "metric"}:
        return "NEEDS_REVIEW"

    if text.endswith(".tscn"):
        return "UNKNOWN"

    return "UNKNOWN"


def transport_hint(tags: list[str], path: Path) -> str:
    tag_set = set(tags)
    text = path.as_posix().lower()
    for hint in ("wormhole", "boundary", "blackhole", "einstein", "metric", "grin"):
        if hint in tag_set or hint in text:
            return hint
    return "unknown"


def closure_hint(tags: list[str], path: Path) -> str:
    tag_set = set(tags)
    text = path.as_posix().lower()
    if "hermetic" in tag_set or "closure" in tag_set or "hermetic" in text or "closure" in text:
        return "likely"
    if "boundary" in tag_set or "curved" in tag_set:
        return "possible"
    if tag_set & {"demo", "wormhole", "visual", "portal"}:
        return "unlikely"
    return "unknown"


def candidate_from_file(path: Path) -> Candidate | None:
    sample = read_sample(path)
    tags = detect_tags(path, sample)
    if not is_candidate_file(path, tags, sample):
        return None

    source_type = path.suffix.lower().lstrip(".")
    return Candidate(
        name=path.stem,
        path=path.as_posix(),
        source_type=source_type,
        detected_tags=tags,
        likely_category=likely_category(tags, path),
        transport_hint=transport_hint(tags, path),
        closure_hint=closure_hint(tags, path),
        godot_runtime_required=source_type == "tscn",
        parity_claim="NONE",
        notes="Best-effort static metadata only; scene not executed.",
    )


def candidate_sort_key(candidate: Candidate) -> tuple[int, int, str]:
    return (
        CATEGORY_PRIORITY.get(candidate.likely_category, CATEGORY_PRIORITY["UNKNOWN"]),
        SOURCE_PRIORITY.get(candidate.source_type, 9),
        candidate.path,
    )


def candidate_to_json(candidate: Candidate) -> dict[str, Any]:
    return {
        "name": candidate.name,
        "path": candidate.path,
        "source_type": candidate.source_type,
        "detected_tags": candidate.detected_tags,
        "likely_category": candidate.likely_category,
        "transport_hint": candidate.transport_hint,
        "closure_hint": candidate.closure_hint,
        "godot_runtime_required": candidate.godot_runtime_required,
        "parity_claim": candidate.parity_claim,
        "notes": candidate.notes,
    }


def build_json(candidates: list[Candidate], generated: datetime) -> dict[str, Any]:
    return {
        "schema": "xprimeray.glowing_heart.godot_fixture_candidates.v0.9",
        "generatedUtc": generated.strftime("%Y-%m-%dT%H:%M:%SZ"),
        "source": "static_repo_scan",
        "parityClaim": "NONE",
        "runtimeExecuted": False,
        "candidateCount": len(candidates),
        "candidates": [candidate_to_json(candidate) for candidate in candidates],
    }


def build_markdown(candidates: list[Candidate], generated: datetime) -> str:
    generated_text = generated.strftime("%Y-%m-%dT%H:%M:%SZ")
    counts = Counter(candidate.likely_category for candidate in candidates)
    lines = [
        "# Glowing Heart Godot Fixture Candidates (Preview)",
        "",
        f"Generated: {generated_text}",
        "",
        "Runtime executed: false",
        "",
        "Parity claim: NONE",
        "",
        f"Candidates: {len(candidates)}",
        "",
        "## Summary",
        "",
        "| Category | Count |",
        "|---|---:|",
    ]

    for category in CATEGORY_PRIORITY:
        lines.append(f"| {category} | {counts.get(category, 0)} |")

    lines.extend(
        [
            "",
            "## Candidates",
            "",
            "| Name | Category | Transport | Closure | Tags | Path |",
            "|---|---|---|---|---|---|",
        ]
    )

    for candidate in candidates:
        tags = ", ".join(candidate.detected_tags) if candidate.detected_tags else "none"
        lines.append(
            "| "
            f"{candidate.name} | "
            f"{candidate.likely_category} | "
            f"{candidate.transport_hint} | "
            f"{candidate.closure_hint} | "
            f"{tags} | "
            f"{candidate.path} |"
        )

    return "\n".join(lines) + "\n"


def main() -> int:
    parser = argparse.ArgumentParser(description="Generate a static preview index of Godot fixture candidates.")
    parser.add_argument("--out-json", type=Path, default=DEFAULT_OUTPUT_JSON)
    parser.add_argument("--out-md", type=Path, default=DEFAULT_OUTPUT_MD)
    args = parser.parse_args()

    source_files = discover_source_files(SCAN_ROOTS)
    candidates = [candidate for path in source_files if (candidate := candidate_from_file(path)) is not None]
    candidates.sort(key=candidate_sort_key)
    generated = datetime.now(timezone.utc)

    args.out_json.parent.mkdir(parents=True, exist_ok=True)
    args.out_md.parent.mkdir(parents=True, exist_ok=True)
    args.out_json.write_text(json.dumps(build_json(candidates, generated), indent=2) + "\n", encoding="utf-8")
    args.out_md.write_text(build_markdown(candidates, generated), encoding="utf-8")

    counts = Counter(candidate.likely_category for candidate in candidates)
    print("[glowing-heart-godot-fixtures]")
    print(f"scanned_files={len(source_files)}")
    print(f"candidates={len(candidates)}")
    print(f"ready_candidates={counts.get('READY_CANDIDATE', 0)}")
    print(f"experimental={counts.get('EXPERIMENTAL', 0)}")
    print("runtime_executed=false")
    print("parity_claim=NONE")
    print()
    print(f"wrote={args.out_json}")
    print(f"wrote={args.out_md}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
