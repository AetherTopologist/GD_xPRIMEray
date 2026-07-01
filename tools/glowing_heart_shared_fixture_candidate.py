#!/usr/bin/env python3
"""Generate the v1.0 shared fixture candidate preview packet.

This is metadata-only bridge work. It reads existing Core and preview Godot
fixture metadata, writes preview reports, and does not execute Godot.
"""

from __future__ import annotations

import argparse
import json
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


DEFAULT_CORE_FIXTURE = Path("Fixtures/grin_radial_smoke.json")
DEFAULT_GODOT_INDEX = Path("reports/glowing_heart_godot_fixture_candidates.preview.json")
DEFAULT_BRIDGE = Path("reports/glowing_heart_bridge.preview.md")
DEFAULT_GALLERY = Path("reports/glowing_heart_gallery.preview.md")
DEFAULT_OUTPUT_JSON = Path("reports/glowing_heart_shared_fixture_candidate.preview.json")
DEFAULT_OUTPUT_MD = Path("reports/glowing_heart_shared_fixture_candidate.preview.md")
PREFERRED_GODOT_CANDIDATE = "Fixtures/fixture_hermetic_observatory_grin.tscn"

SHARED_CONCEPTS = [
    "fixture",
    "grin",
    "field-driven bending",
    "observatory artifact",
]
DIFFERENCES = [
    "Core fixture is simplified JSON; Godot fixture is a .tscn scene.",
    "Core snapshot visualizes bend magnitude; Godot artifacts are renderer/HUD/observatory outputs.",
    "Core smoke fixture has no closure claim; Godot candidate may involve hermetic closure.",
    "Core currently has no geometry, collision, portal, or scene tree.",
]
NORMALIZATION_NEEDED = [
    "Shared fixture schema",
    "Shared observer/camera definition",
    "Shared field parameter mapping",
    "Shared validation vocabulary",
    "Shared snapshot metric naming",
    "Godot scene metadata export path",
]


class PacketError(Exception):
    pass


@dataclass(frozen=True)
class SelectedCandidate:
    entry: dict[str, Any]
    confidence: str
    reason: str


def load_object(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:
        raise PacketError(f"{path}: failed to load JSON: {exc}") from exc
    if not isinstance(value, dict):
        raise PacketError(f"{path}: expected JSON object")
    return value


def load_candidate_index(path: Path) -> list[dict[str, Any]]:
    data = load_object(path)
    candidates = data.get("candidates")
    if not isinstance(candidates, list):
        raise PacketError(f"{path}: expected candidates array")

    result: list[dict[str, Any]] = []
    for index, candidate in enumerate(candidates):
        if not isinstance(candidate, dict):
            raise PacketError(f"{path}: candidate {index} is not an object")
        result.append(candidate)
    return result


def require_inputs(paths: list[Path]) -> None:
    for path in paths:
        if not path.is_file():
            raise PacketError(f"{path}: required input not found")


def string_value(value: Any, fallback: str = "") -> str:
    return value if isinstance(value, str) else fallback


def numeric_value(value: Any) -> int | float | None:
    return value if isinstance(value, (int, float)) and not isinstance(value, bool) else None


def summarize_core_fixture(path: Path, fixture: dict[str, Any]) -> dict[str, Any]:
    ray_grid = fixture.get("rayGrid") if isinstance(fixture.get("rayGrid"), dict) else {}
    transport = fixture.get("transport") if isinstance(fixture.get("transport"), dict) else {}
    fields = fixture.get("fields") if isinstance(fixture.get("fields"), list) else []

    summarized_fields: list[dict[str, Any]] = []
    for field in fields:
        if not isinstance(field, dict):
            continue
        summarized_fields.append(
            {
                "type": string_value(field.get("type"), "unknown"),
                "radiusOuter": numeric_value(field.get("radiusOuter")),
                "amplitude": numeric_value(field.get("amplitude")),
                "curveType": string_value(field.get("curveType"), "unknown"),
                "gamma": numeric_value(field.get("gamma")),
            }
        )

    return {
        "name": string_value(fixture.get("name"), path.stem),
        "path": path.as_posix(),
        "mode": string_value(transport.get("mode"), "unknown"),
        "rayGrid": {
            "width": numeric_value(ray_grid.get("width")),
            "height": numeric_value(ray_grid.get("height")),
        },
        "transport": {
            "maxStepsPerRay": numeric_value(transport.get("maxStepsPerRay")),
            "stepSize": numeric_value(transport.get("stepSize")),
        },
        "fields": summarized_fields,
    }


def candidate_tags(candidate: dict[str, Any]) -> set[str]:
    tags = candidate.get("detected_tags")
    if not isinstance(tags, list):
        return set()
    return {tag for tag in tags if isinstance(tag, str)}


def candidate_score(candidate: dict[str, Any]) -> int:
    tags = candidate_tags(candidate)
    score = 0
    if candidate.get("likely_category") == "READY_CANDIDATE":
        score += 20
    if "grin" in tags:
        score += 30
    if "hermetic" in tags:
        score += 18
    if "observatory" in tags:
        score += 18
    if candidate.get("transport_hint") == "grin":
        score += 15
    if candidate.get("closure_hint") == "likely":
        score += 8
    elif candidate.get("closure_hint") == "possible":
        score += 4
    return score


def selection_confidence(candidate: dict[str, Any], preferred: bool) -> str:
    tags = candidate_tags(candidate)
    if preferred or ("grin" in tags and ("hermetic" in tags or "observatory" in tags)):
        return "HIGH"
    if "grin" in tags:
        return "MEDIUM"
    return "LOW"


def select_godot_candidate(candidates: list[dict[str, Any]]) -> SelectedCandidate:
    for candidate in candidates:
        if candidate.get("path") == PREFERRED_GODOT_CANDIDATE:
            return SelectedCandidate(
                entry=candidate,
                confidence="HIGH",
                reason="Preferred Godot GRIN observatory fixture found in the v0.9 candidate index.",
            )

    strong_candidates = [candidate for candidate in candidates if candidate_score(candidate) > 0]
    if not strong_candidates:
        raise PacketError("no Godot fixture candidate could be identified with LOW confidence")

    selected = max(strong_candidates, key=lambda candidate: (candidate_score(candidate), string_value(candidate.get("path"))))
    confidence = selection_confidence(selected, preferred=False)
    if confidence == "HIGH":
        reason = "Best tag overlap with Core radial GRIN smoke fixture."
    elif confidence == "MEDIUM":
        reason = "Fallback GRIN candidate selected; hermetic/observatory overlap was not available."
    else:
        reason = "Weak fallback candidate selected from static metadata; confidence is low."

    return SelectedCandidate(entry=selected, confidence=confidence, reason=reason)


def build_godot_summary(selected: SelectedCandidate) -> dict[str, Any]:
    entry = selected.entry
    return {
        "name": string_value(entry.get("name"), "UNKNOWN"),
        "path": string_value(entry.get("path"), "UNKNOWN"),
        "likely_category": string_value(entry.get("likely_category"), "UNKNOWN"),
        "transport_hint": string_value(entry.get("transport_hint"), "unknown"),
        "closure_hint": string_value(entry.get("closure_hint"), "unknown"),
        "detected_tags": sorted(candidate_tags(entry)),
        "selectionConfidence": selected.confidence,
        "selectionReason": selected.reason,
    }


def build_packet(core: dict[str, Any], godot: dict[str, Any], generated: datetime) -> dict[str, Any]:
    return {
        "schema": "xprimeray.glowing_heart.shared_fixture_candidate.v1.0",
        "generatedUtc": generated.strftime("%Y-%m-%dT%H:%M:%SZ"),
        "parityClaim": "NONE",
        "runtimeExecuted": False,
        "coreFixture": core,
        "godotCandidate": godot,
        "sharedConcepts": SHARED_CONCEPTS,
        "differences": DIFFERENCES,
        "normalizationNeeded": NORMALIZATION_NEEDED,
        "nextBridgeStep": "Create a static Godot fixture metadata export for the selected candidate without executing Godot.",
    }


def first_field(core: dict[str, Any]) -> dict[str, Any]:
    fields = core.get("fields")
    if isinstance(fields, list) and fields and isinstance(fields[0], dict):
        return fields[0]
    return {}


def build_markdown(packet: dict[str, Any]) -> str:
    core = packet["coreFixture"]
    godot = packet["godotCandidate"]
    field = first_field(core)
    ray_grid = core["rayGrid"]
    transport = core["transport"]

    lines = [
        "# Project Glowing Heart Shared Fixture Candidate (Preview)",
        "",
        f"Generated: {packet['generatedUtc']}",
        "",
        "Parity claim: NONE",
        "",
        "Runtime executed: false",
        "",
        "## Core Fixture",
        "",
        "| Field | Value |",
        "|---|---|",
        f"| Name | {core['name']} |",
        f"| Path | {core['path']} |",
        f"| Mode | {core['mode']} |",
        f"| Ray Grid | {ray_grid['width']}x{ray_grid['height']} |",
        f"| Steps per Ray | {transport['maxStepsPerRay']} |",
        f"| Step Size | {transport['stepSize']} |",
        f"| Field Type | {field.get('type', 'unknown')} |",
        f"| Radius Outer | {field.get('radiusOuter', 'unknown')} |",
        f"| Amplitude | {field.get('amplitude', 'unknown')} |",
        "",
        "## Godot Candidate",
        "",
        "| Field | Value |",
        "|---|---|",
        f"| Name | {godot['name']} |",
        f"| Path | {godot['path']} |",
        f"| Category | {godot['likely_category']} |",
        f"| Transport Hint | {godot['transport_hint']} |",
        f"| Closure Hint | {godot['closure_hint']} |",
        f"| Confidence | {godot['selectionConfidence']} |",
        "",
        "## Why This Candidate",
        "",
        godot["selectionReason"],
        "",
        "## Shared Concepts",
        "",
    ]

    lines.extend(f"- {item}" for item in packet["sharedConcepts"])
    lines.extend(["", "## Differences", ""])
    lines.extend(f"- {item}" for item in packet["differences"])
    lines.extend(["", "## Normalization Needed Before Parity", ""])
    lines.extend(f"- {item}" for item in packet["normalizationNeeded"])
    lines.extend(
        [
            "",
            "## Bridge Status",
            "",
            "Current state:",
            "SHARED FIXTURE CANDIDATE IDENTIFIED",
            "",
            "Verification:",
            "METADATA ONLY",
            "",
            "Parity:",
            "NONE",
            "",
            "Recommendation:",
            "Proceed to static Godot metadata export for the selected candidate.",
            "",
        ]
    )
    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser(description="Generate the Glowing Heart shared fixture candidate preview packet.")
    parser.add_argument("--core-fixture", type=Path, default=DEFAULT_CORE_FIXTURE)
    parser.add_argument("--godot-index", type=Path, default=DEFAULT_GODOT_INDEX)
    parser.add_argument("--bridge", type=Path, default=DEFAULT_BRIDGE)
    parser.add_argument("--gallery", type=Path, default=DEFAULT_GALLERY)
    parser.add_argument("--out-json", type=Path, default=DEFAULT_OUTPUT_JSON)
    parser.add_argument("--out-md", type=Path, default=DEFAULT_OUTPUT_MD)
    args = parser.parse_args()

    require_inputs([args.core_fixture, args.godot_index, args.bridge, args.gallery])
    core = summarize_core_fixture(args.core_fixture, load_object(args.core_fixture))
    selected = select_godot_candidate(load_candidate_index(args.godot_index))
    godot = build_godot_summary(selected)
    packet = build_packet(core, godot, datetime.now(timezone.utc))

    args.out_json.parent.mkdir(parents=True, exist_ok=True)
    args.out_md.parent.mkdir(parents=True, exist_ok=True)
    args.out_json.write_text(json.dumps(packet, indent=2) + "\n", encoding="utf-8")
    args.out_md.write_text(build_markdown(packet), encoding="utf-8")

    print("[glowing-heart-shared-fixture]")
    print(f"core_fixture={core['name']}")
    print(f"godot_candidate={godot['path']}")
    print(f"confidence={godot['selectionConfidence']}")
    print("runtime_executed=false")
    print("parity_claim=NONE")
    print()
    print(f"wrote={args.out_json}")
    print(f"wrote={args.out_md}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except PacketError as exc:
        print(f"[glowing-heart-shared-fixture] ERROR: {exc}")
        raise SystemExit(1)
