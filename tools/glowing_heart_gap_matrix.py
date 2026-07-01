#!/usr/bin/env python3
"""Generate the v1.2 Core-to-Godot metadata gap matrix.

This is a preview comparison over existing metadata artifacts only. It does not
execute Godot, modify fixtures, modify Core transport, or touch the production
Observatory catalog.
"""

from __future__ import annotations

import argparse
import json
from collections import Counter
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


DEFAULT_SHARED = Path("reports/glowing_heart_shared_fixture_candidate.preview.json")
DEFAULT_GODOT_EXPORT = Path("reports/glowing_heart_godot_fixture_export.preview.json")
DEFAULT_CORE_FIXTURE = Path("Fixtures/grin_radial_smoke.json")
DEFAULT_OUTPUT_JSON = Path("reports/glowing_heart_gap_matrix.preview.json")
DEFAULT_OUTPUT_MD = Path("reports/glowing_heart_gap_matrix.preview.md")
STATUSES = ("MATCH", "PARTIAL", "MISSING_IN_CORE", "MISSING_IN_GODOT_EXPORT", "UNKNOWN")
CATEGORIES = (
    "Fixture Identity",
    "Observer / Camera",
    "Field Definition",
    "Transport Concept",
    "Validation",
    "Closure Concept",
    "Receiver Concept",
    "Snapshot Output",
    "Observatory Artifact",
    "Runtime Dependency",
    "Scene Graph",
    "Geometry",
    "Portal / Wormhole",
    "Boundary Modeling",
)
NORMALIZATION_TARGETS = (
    "Shared fixture schema",
    "Shared observer/camera definition",
    "Shared field parameter vocabulary",
    "Shared transport baseline vocabulary",
    "Shared validation and closure vocabulary",
)


class MatrixError(Exception):
    pass


def load_object(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:
        raise MatrixError(f"{path}: failed to load JSON: {exc}") from exc
    if not isinstance(value, dict):
        raise MatrixError(f"{path}: expected JSON object")
    return value


def require_inputs(paths: list[Path]) -> None:
    for path in paths:
        if not path.is_file():
            raise MatrixError(f"{path}: required input not found")


def has_core_observer(core_fixture: dict[str, Any]) -> bool:
    observer = core_fixture.get("observer")
    return isinstance(observer, dict) and bool(observer)


def has_core_grin_field(core_fixture: dict[str, Any]) -> bool:
    fields = core_fixture.get("fields")
    if not isinstance(fields, list):
        return False
    return any(isinstance(field, dict) and field.get("type") == "grin_radial" for field in fields)


def has_core_transport(core_fixture: dict[str, Any]) -> bool:
    transport = core_fixture.get("transport")
    return isinstance(transport, dict) and bool(transport.get("mode"))


def has_core_validation(core_fixture: dict[str, Any]) -> bool:
    validation = core_fixture.get("validation")
    return isinstance(validation, dict) and bool(validation)


def core_requires_closure(core_fixture: dict[str, Any]) -> bool:
    validation = core_fixture.get("validation")
    return isinstance(validation, dict) and validation.get("requireHermeticClosure") is True


def godot_hint(godot_export: dict[str, Any], key: str) -> bool:
    hints = godot_export.get("classifiedHints")
    return isinstance(hints, dict) and hints.get(key) is True


def godot_has_node(godot_export: dict[str, Any], node_type: str | None = None, name_contains: str | None = None) -> bool:
    nodes = godot_export.get("nodes")
    if not isinstance(nodes, list):
        return False
    for node in nodes:
        if not isinstance(node, dict):
            continue
        if node_type and node.get("type") == node_type:
            return True
        if name_contains and name_contains.lower() in str(node.get("name", "")).lower():
            return True
    return False


def godot_reference_count(godot_export: dict[str, Any], term: str) -> int:
    refs = godot_export.get("interestingReferences")
    if not isinstance(refs, dict):
        return 0
    entry = refs.get(term)
    if not isinstance(entry, dict):
        return 0
    count = entry.get("count")
    return count if isinstance(count, int) else 0


def row(category: str, status: str, reason: str) -> dict[str, str]:
    if category not in CATEGORIES:
        raise MatrixError(f"unexpected category: {category}")
    if status not in STATUSES:
        raise MatrixError(f"unexpected status for {category}: {status}")
    return {"category": category, "status": status, "reason": reason}


def build_rows(shared: dict[str, Any], godot_export: dict[str, Any], core_fixture: dict[str, Any]) -> list[dict[str, str]]:
    core_fixture_path = shared.get("coreFixture", {}).get("path") if isinstance(shared.get("coreFixture"), dict) else None
    godot_fixture_path = godot_export.get("fixture", {}).get("path") if isinstance(godot_export.get("fixture"), dict) else None
    shared_concepts = shared.get("sharedConcepts") if isinstance(shared.get("sharedConcepts"), list) else []

    rows = [
        row(
            "Fixture Identity",
            "MATCH" if core_fixture_path and godot_fixture_path else "UNKNOWN",
            f"Core fixture exists at {core_fixture_path}; Godot fixture export exists at {godot_fixture_path}.",
        ),
        row(
            "Observer / Camera",
            "PARTIAL" if has_core_observer(core_fixture) and godot_hint(godot_export, "hasCamera") else "UNKNOWN",
            "Core observer exists; Godot Camera3D detected, but no shared observer/camera schema exists.",
        ),
        row(
            "Field Definition",
            "PARTIAL" if has_core_grin_field(core_fixture) and godot_hint(godot_export, "hasGrinSignal") else "UNKNOWN",
            "Core has a grin_radial field; Godot export has GRIN/FieldSource signals, but field parameters are not normalized.",
        ),
        row(
            "Transport Concept",
            "PARTIAL" if has_core_transport(core_fixture) and (godot_reference_count(godot_export, "Ray") or godot_reference_count(godot_export, "Renderer") or godot_reference_count(godot_export, "Transport")) else "UNKNOWN",
            "Core has radial_grin_smoke transport settings; Godot export has Ray/Renderer/Transport references, but no shared transport baseline.",
        ),
        row(
            "Validation",
            "PARTIAL" if has_core_validation(core_fixture) and godot_hint(godot_export, "hasClosureSignal") else "UNKNOWN",
            "Core validation metadata exists; Godot export has static closure/contract signals, but validation vocabulary is not shared.",
        ),
        row(
            "Closure Concept",
            "MISSING_IN_CORE" if godot_hint(godot_export, "hasClosureSignal") and not core_requires_closure(core_fixture) else "PARTIAL",
            "Godot export has closure-style signals; Core smoke fixture explicitly does not require hermetic closure.",
        ),
        row(
            "Receiver Concept",
            "MISSING_IN_CORE" if godot_hint(godot_export, "hasReceiverSignal") else "UNKNOWN",
            "Godot export includes receiver nodes/groups; Core smoke fixture has no receiver or collision target concept.",
        ),
        row(
            "Snapshot Output",
            "PARTIAL",
            "Core packets include metric snapshots; Godot side is represented as renderer/observatory output metadata, not a shared snapshot type.",
        ),
        row(
            "Observatory Artifact",
            "MATCH" if "observatory artifact" in shared_concepts and godot_hint(godot_export, "hasObservatorySignal") else "PARTIAL",
            "Both sides are represented in the Glowing Heart observatory artifact chain.",
        ),
        row(
            "Runtime Dependency",
            "PARTIAL",
            "Core fixture can run outside Godot; Godot fixture is a scene that requires Godot for runtime behavior.",
        ),
        row(
            "Scene Graph",
            "MISSING_IN_CORE" if godot_export.get("nodes") else "UNKNOWN",
            "Godot export has scene nodes; Core fixture is JSON metadata plus transport settings with no scene graph.",
        ),
        row(
            "Geometry",
            "MISSING_IN_CORE" if godot_has_node(godot_export, node_type="StaticBody3D") else "UNKNOWN",
            "Godot export includes StaticBody3D receiver geometry; Core smoke fixture has no geometry or collision model.",
        ),
        row(
            "Portal / Wormhole",
            "UNKNOWN" if not godot_hint(godot_export, "hasWormholeSignal") else "MISSING_IN_CORE",
            "No portal or wormhole signal is present in the Core fixture or static Godot export.",
        ),
        row(
            "Boundary Modeling",
            "UNKNOWN" if not godot_hint(godot_export, "hasBoundarySignal") else "MISSING_IN_CORE",
            "No explicit boundary modeling signal is present in the static Godot export; Core fixture has no boundary model.",
        ),
    ]
    return rows


def summarize(rows: list[dict[str, str]]) -> dict[str, int]:
    counts = Counter(row["status"] for row in rows)
    return {status: counts.get(status, 0) for status in STATUSES}


def readiness_score(rows: list[dict[str, str]]) -> int:
    score = 0
    for item in rows:
        if item["status"] == "MATCH":
            score += 10
        elif item["status"] == "PARTIAL":
            score += 5
    return min(score, 100)


def build_packet(shared: dict[str, Any], godot_export: dict[str, Any], core_fixture: dict[str, Any], generated: datetime) -> dict[str, Any]:
    rows = build_rows(shared, godot_export, core_fixture)
    core_path = shared.get("coreFixture", {}).get("path") if isinstance(shared.get("coreFixture"), dict) else DEFAULT_CORE_FIXTURE.as_posix()
    godot_path = godot_export.get("fixture", {}).get("path") if isinstance(godot_export.get("fixture"), dict) else "UNKNOWN"
    return {
        "schema": "xprimeray.glowing_heart.gap_matrix.v1.2",
        "generatedUtc": generated.strftime("%Y-%m-%dT%H:%M:%SZ"),
        "runtimeExecuted": False,
        "parityClaim": "NONE",
        "coreFixture": core_path,
        "godotFixture": godot_path,
        "rows": rows,
        "summary": summarize(rows),
        "readinessScore": readiness_score(rows),
        "readinessMethod": "MATCH = 10 points; PARTIAL = 5 points; everything else = 0; capped at 100.",
        "recommendedNormalizationTargets": list(NORMALIZATION_TARGETS),
    }


def build_markdown(packet: dict[str, Any]) -> str:
    lines = [
        "# Project Glowing Heart Gap Matrix (Preview)",
        "",
        f"Generated: {packet['generatedUtc']}",
        "",
        "Runtime executed: false",
        "",
        "Parity claim: NONE",
        "",
        "## Summary",
        "",
        "| Status | Count |",
        "|---|---:|",
    ]
    for status in STATUSES:
        lines.append(f"| {status} | {packet['summary'][status]} |")

    lines.extend(
        [
            "",
            "## Gap Matrix",
            "",
            "| Category | Status | Reason |",
            "|---|---|---|",
        ]
    )
    for item in packet["rows"]:
        lines.append(f"| {item['category']} | {item['status']} | {item['reason']} |")

    lines.extend(
        [
            "",
            "## Bridge Readiness",
            "",
            "Readiness Score:",
            f"{packet['readinessScore']} / 100",
            "",
            "Method:",
            "MATCH = 10 points",
            "PARTIAL = 5 points",
            "everything else = 0",
            "",
            "This score is informational only.",
            "",
            "It is NOT parity.",
            "",
            "It is NOT scientific validation.",
            "",
            "It is only a planning aid.",
            "",
            "## Recommended Normalization Targets",
            "",
        ]
    )
    lines.extend(f"- {target}" for target in packet["recommendedNormalizationTargets"])
    return "\n".join(lines) + "\n"


def main() -> int:
    parser = argparse.ArgumentParser(description="Generate the Glowing Heart Core-to-Godot gap matrix preview.")
    parser.add_argument("--shared", type=Path, default=DEFAULT_SHARED)
    parser.add_argument("--godot-export", type=Path, default=DEFAULT_GODOT_EXPORT)
    parser.add_argument("--core-fixture", type=Path, default=DEFAULT_CORE_FIXTURE)
    parser.add_argument("--out-json", type=Path, default=DEFAULT_OUTPUT_JSON)
    parser.add_argument("--out-md", type=Path, default=DEFAULT_OUTPUT_MD)
    args = parser.parse_args()

    require_inputs([args.shared, args.godot_export, args.core_fixture])
    shared = load_object(args.shared)
    godot_export = load_object(args.godot_export)
    core_fixture = load_object(args.core_fixture)
    packet = build_packet(shared, godot_export, core_fixture, datetime.now(timezone.utc))

    args.out_json.parent.mkdir(parents=True, exist_ok=True)
    args.out_md.parent.mkdir(parents=True, exist_ok=True)
    args.out_json.write_text(json.dumps(packet, indent=2) + "\n", encoding="utf-8")
    args.out_md.write_text(build_markdown(packet), encoding="utf-8")

    core_name = core_fixture.get("name", "UNKNOWN")
    godot_name = Path(str(packet["godotFixture"])).stem
    print("[glowing-heart-gap-matrix]")
    print()
    print(f"core_fixture={core_name}")
    print()
    print(f"godot_fixture={godot_name}")
    print()
    print("runtime_executed=false")
    print()
    print("parity_claim=NONE")
    print()
    print(f"readiness_score={packet['readinessScore']}")
    print()
    print(f"wrote={args.out_json}")
    print()
    print(f"wrote={args.out_md}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except MatrixError as exc:
        print(f"[glowing-heart-gap-matrix] ERROR: {exc}")
        raise SystemExit(1)
