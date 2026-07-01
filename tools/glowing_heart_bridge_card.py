#!/usr/bin/env python3
"""Generate a preview bridge card for Glowing Heart Core and Godot evidence.

This is a documentation bridge only. It reads preview artifacts and writes a
preview Markdown card without executing Godot, modifying Core transport, or
touching the production Observatory catalog.
"""

from __future__ import annotations

import argparse
import json
import re
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


DEFAULT_GALLERY = Path("reports/glowing_heart_gallery.preview.md")
DEFAULT_CATALOG = Path("reports/glowing_heart_observatory_catalog.preview.json")
DEFAULT_OUTPUT = Path("reports/glowing_heart_bridge.preview.md")
REFERENCE_ROOTS = (
    Path("Fixtures"),
    Path("fixtures"),
    Path("Docs/Observatory"),
    Path("Docs/xPRIMEray"),
    Path("Docs/observatory"),
)


class BridgeError(Exception):
    pass


@dataclass(frozen=True)
class CoreArtifact:
    run_id: str
    fixture: str
    validation: str
    artifact_type: str
    phase: str
    timestamp: str


@dataclass(frozen=True)
class CandidateReference:
    display: str
    explanation: str


def load_catalog(path: Path) -> list[dict[str, Any]]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:
        raise BridgeError(f"{path}: failed to load preview catalog: {exc}") from exc

    if not isinstance(value, list):
        raise BridgeError(f"{path}: expected a JSON array")

    entries: list[dict[str, Any]] = []
    for index, entry in enumerate(value):
        if not isinstance(entry, dict):
            raise BridgeError(f"{path}: entry {index} is not an object")
        entries.append(entry)
    return entries


def required_str(entry: dict[str, Any], key: str, source: Path) -> str:
    value = entry.get(key)
    if not isinstance(value, str) or not value.strip():
        raise BridgeError(f"{source}: catalog entry missing required string field {key}")
    return value


def timestamp_sort_key(value: str) -> tuple[str, str]:
    if value.endswith("Z"):
        return (value, "")
    return ("", value)


def latest_core_artifact(entries: list[dict[str, Any]], source: Path) -> CoreArtifact:
    if not entries:
        raise BridgeError(f"{source}: preview catalog has no entries")

    validated: list[CoreArtifact] = []
    for entry in entries:
        run_id = required_str(entry, "run_id", source)
        fixture = required_str(entry, "fixture", source)
        artifact_type = required_str(entry, "artifact_type", source)
        timestamp = required_str(entry, "timestamp", source)
        phase = required_str(entry, "phase", source)
        validation = entry.get("coverage")
        if not isinstance(validation, str) or not validation.strip():
            validation = entry.get("validation")
        if not isinstance(validation, str) or not validation.strip():
            validation = entry.get("verdict")
        if not isinstance(validation, str) or not validation.strip():
            validation = "UNKNOWN"

        validated.append(
            CoreArtifact(
                run_id=run_id,
                fixture=fixture,
                validation=validation,
                artifact_type=artifact_type,
                phase=phase,
                timestamp=timestamp,
            )
        )

    return max(validated, key=lambda artifact: (timestamp_sort_key(artifact.timestamp), artifact.run_id))


def count_gallery_runs(path: Path) -> int:
    text = path.read_text(encoding="utf-8")
    match = re.search(r"^Runs discovered:\s*(\d+)\s*$", text, re.MULTILINE)
    if match:
        return int(match.group(1))

    return len(re.findall(r"^##\s+\d{8}T\d{6}Z_", text, re.MULTILINE))


def fixture_tokens(fixture: str) -> set[str]:
    return {token for token in re.split(r"[^a-z0-9]+", fixture.lower()) if len(token) >= 3}


def score_reference(path: Path, tokens: set[str]) -> int:
    text = path.as_posix().lower()
    stem = path.stem.lower()
    score = 0
    if path.suffix == ".tscn":
        score += 25
    if "observatory" in text:
        score += 18
    if "fixture" in text:
        score += 6
    if "hermetic" in text:
        score += 4
    if "godot" in text:
        score += 3

    for token in tokens:
        if token in stem:
            score += 15
        elif token in text:
            score += 8

    if "grin" in tokens and "grin" in text:
        score += 10
    return score


def find_candidate_reference(fixture: str, roots: tuple[Path, ...]) -> CandidateReference:
    tokens = fixture_tokens(fixture)
    candidates: list[tuple[int, Path]] = []
    for root in roots:
        if not root.exists():
            continue
        for path in root.rglob("*"):
            if not path.is_file():
                continue
            if path.suffix.lower() not in {".tscn", ".md", ".json"}:
                continue
            score = score_reference(path, tokens)
            if score > 0:
                candidates.append((score, path))

    if not candidates:
        return CandidateReference(
            display="UNKNOWN",
            explanation="No fixture or observatory document reference matched the latest Core fixture tokens.",
        )

    score, path = max(candidates, key=lambda item: (item[0], item[1].as_posix()))
    return CandidateReference(
        display=path.as_posix(),
        explanation=f"Best-effort filename match for latest Core fixture '{fixture}' (score {score}); not executed.",
    )


def build_bridge_card(
    artifact: CoreArtifact,
    candidate: CandidateReference,
    gallery_runs: int,
    catalog_entries: int,
    generated: datetime,
) -> str:
    generated_text = generated.strftime("%Y-%m-%dT%H:%M:%SZ")
    return "\n".join(
        [
            "# Project Glowing Heart Bridge Card (Preview)",
            "",
            f"Generated: {generated_text}",
            "",
            "Current Phase:",
            "Project Glowing Heart v0.8",
            "",
            "---",
            "",
            "## Core Artifact",
            "",
            "Run:",
            artifact.run_id,
            "",
            "Fixture:",
            artifact.fixture,
            "",
            "Validation:",
            artifact.validation,
            "",
            "Artifact Type:",
            artifact.artifact_type,
            "",
            "Phase:",
            artifact.phase,
            "",
            "Preview Evidence:",
            f"- Gallery runs: {gallery_runs}",
            f"- Catalog entries: {catalog_entries}",
            "",
            "## Godot Observatory",
            "",
            "Status:",
            "Reference System",
            "",
            "Nearest Candidate Fixture:",
            candidate.display,
            "",
            "Candidate Note:",
            candidate.explanation,
            "",
            "Execution:",
            "Godot-based",
            "",
            "Output Type:",
            "Renderer / Observatory artifact",
            "",
            "Transport:",
            "Integrated into GD_xPRIMEray",
            "",
            "Parity:",
            "NOT CLAIMED",
            "",
            "## Shared Concepts",
            "",
            "| Concept | Core | Godot |",
            "| --- | --- | --- |",
            "| Fixture | Yes | Yes |",
            "| Validation | Yes | Yes |",
            "| Observatory Artifact | Yes | Yes |",
            "| Transport | Simplified | Full |",
            "| Snapshot | Metric Snapshot | Renderer Snapshot |",
            "| Closure | Not Yet | Existing |",
            "",
            "## Known Gaps",
            "",
            "- No parity claim",
            "- No shared fixture execution",
            "- No snapshot equivalence",
            "- No closure equivalence",
            "- No shared validation baseline",
            "- No shared transport baseline",
            "",
            "## Suggested Next Milestones",
            "",
            "v0.9",
            "Godot fixture metadata extraction",
            "",
            "v1.0",
            "First shared fixture definition",
            "",
            "v1.1",
            "First shared transport baseline",
            "",
            "v1.2",
            "First parity candidate review",
            "",
            "## Bridge Status",
            "",
            "Current State:",
            "CONNECTED BY DOCUMENTATION",
            "",
            "Current Verification:",
            "NONE",
            "",
            "Parity Claim:",
            "NONE",
            "",
            "Risk:",
            "LOW",
            "",
            "Recommendation:",
            "Continue additive bridge construction.",
            "",
        ]
    )


def main() -> int:
    parser = argparse.ArgumentParser(description="Generate the Glowing Heart bridge preview card.")
    parser.add_argument("--gallery", type=Path, default=DEFAULT_GALLERY)
    parser.add_argument("--catalog", type=Path, default=DEFAULT_CATALOG)
    parser.add_argument("--out", type=Path, default=DEFAULT_OUTPUT)
    args = parser.parse_args()

    if not args.gallery.is_file():
        raise BridgeError(f"{args.gallery}: gallery preview not found")
    if not args.catalog.is_file():
        raise BridgeError(f"{args.catalog}: catalog preview not found")

    gallery_runs = count_gallery_runs(args.gallery)
    catalog_entries = load_catalog(args.catalog)
    artifact = latest_core_artifact(catalog_entries, args.catalog)
    candidate = find_candidate_reference(artifact.fixture, REFERENCE_ROOTS)

    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text(
        build_bridge_card(
            artifact=artifact,
            candidate=candidate,
            gallery_runs=gallery_runs,
            catalog_entries=len(catalog_entries),
            generated=datetime.now(timezone.utc),
        ),
        encoding="utf-8",
    )

    print("[glowing-heart-bridge]")
    print(f"gallery_runs={gallery_runs}")
    print(f"catalog_entries={len(catalog_entries)}")
    print()
    print(f"wrote={args.out}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except BridgeError as exc:
        print(f"[glowing-heart-bridge] ERROR: {exc}")
        raise SystemExit(1)
