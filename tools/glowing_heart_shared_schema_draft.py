#!/usr/bin/env python3
"""Generate the v1.3 shared fixture schema preview draft.

This creates a neutral bridge vocabulary for metadata only. It does not execute
Godot, convert fixtures, modify Core transport, or claim parity.
"""

from __future__ import annotations

import argparse
import json
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


DEFAULT_CORE_FIXTURE = Path("Fixtures/grin_radial_smoke.json")
DEFAULT_GODOT_EXPORT = Path("reports/glowing_heart_godot_fixture_export.preview.json")
DEFAULT_GAP_MATRIX = Path("reports/glowing_heart_gap_matrix.preview.json")
DEFAULT_CANDIDATE_PACKET = Path("reports/glowing_heart_shared_fixture_candidate.preview.json")
DEFAULT_SCHEMA = Path("schemas/glowing_heart/shared_fixture_schema.v0.preview.json")
DEFAULT_REPORT = Path("reports/glowing_heart_shared_fixture_schema.preview.md")
SOURCE_LABELS = [
    "core",
    "godot",
    "shared",
    "unknown",
    "godot_static_export",
    "mixed_metadata",
    "core_artifact",
    "godot_artifact",
]


class SchemaDraftError(Exception):
    pass


def load_object(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:
        raise SchemaDraftError(f"{path}: failed to load JSON: {exc}") from exc
    if not isinstance(value, dict):
        raise SchemaDraftError(f"{path}: expected JSON object")
    return value


def require_inputs(paths: list[Path]) -> None:
    for path in paths:
        if not path.is_file():
            raise SchemaDraftError(f"{path}: required input not found")


def vector_schema(description: str) -> dict[str, Any]:
    return {
        "description": description,
        "type": "array",
        "prefixItems": [
            {"type": "number"},
            {"type": "number"},
            {"type": "number"},
        ],
        "minItems": 3,
        "maxItems": 3,
    }


def nullable_number() -> dict[str, Any]:
    return {"type": ["number", "null"]}


def source_enum() -> dict[str, Any]:
    return {"$ref": "#/$defs/sourceLabel"}


def build_schema() -> dict[str, Any]:
    return {
        "$schema": "https://json-schema.org/draft/2020-12/schema",
        "$id": "https://xprimeray.dev/schemas/glowing-heart/shared-fixture/v0.preview.json",
        "title": "Project Glowing Heart Shared Fixture Schema Preview",
        "description": "Preview draft for a neutral fixture vocabulary between xPRIMEray-Core and GD_xPRIMEray. This is not a parity guarantee.",
        "type": "object",
        "$defs": {
            "sourceLabel": {
                "type": "string",
                "enum": SOURCE_LABELS,
            }
        },
        "required": [
            "identity",
            "observer",
            "rayGrid",
            "fields",
            "transport",
            "validation",
            "runtimeHints",
            "limitations",
        ],
        "additionalProperties": False,
        "properties": {
            "identity": {
                "type": "object",
                "required": ["name", "version", "description", "parityClaim", "runtimeExecuted"],
                "additionalProperties": False,
                "properties": {
                    "name": {"type": "string", "minLength": 1},
                    "version": {"type": "string", "minLength": 1},
                    "description": {"type": "string"},
                    "parityClaim": {"type": "string", "enum": ["NONE", "CANDIDATE", "UNDER_REVIEW", "SUPPORTED"]},
                    "runtimeExecuted": {"type": "boolean"},
                },
            },
            "sourceLinks": {
                "type": "object",
                "additionalProperties": False,
                "properties": {
                    "coreFixturePath": {"type": "string"},
                    "godotFixturePath": {"type": "string"},
                    "godotExportPath": {"type": "string"},
                    "gapMatrixPath": {"type": "string"},
                    "candidatePacketPath": {"type": "string"},
                },
            },
            "observer": {
                "type": "object",
                "required": ["source", "origin", "forward", "up", "fovDegrees"],
                "additionalProperties": False,
                "properties": {
                    "source": source_enum(),
                    "origin": vector_schema("Observer origin."),
                    "forward": vector_schema("Observer forward direction."),
                    "up": vector_schema("Observer up direction."),
                    "fovDegrees": {"type": ["number", "null"]},
                },
            },
            "rayGrid": {
                "type": "object",
                "required": ["width", "height"],
                "additionalProperties": False,
                "properties": {
                    "width": {"type": "integer", "minimum": 1},
                    "height": {"type": "integer", "minimum": 1},
                },
            },
            "fields": {
                "type": "array",
                "minItems": 1,
                "items": {
                    "type": "object",
                    "required": ["source", "type"],
                    "additionalProperties": False,
                    "properties": {
                        "source": source_enum(),
                        "type": {"type": "string", "enum": ["grin_radial", "unknown"]},
                        "center": vector_schema("Field center."),
                        "radiusOuter": nullable_number(),
                        "amplitude": nullable_number(),
                        "curveType": {"type": ["string", "null"]},
                        "gamma": nullable_number(),
                    },
                },
            },
            "transport": {
                "type": "object",
                "required": ["source", "mode", "maxStepsPerRay", "stepSize", "integrator"],
                "additionalProperties": False,
                "properties": {
                    "source": source_enum(),
                    "mode": {"type": ["string", "null"]},
                    "maxStepsPerRay": {"type": ["integer", "null"], "minimum": 1},
                    "stepSize": nullable_number(),
                    "integrator": {"type": ["string", "null"]},
                },
            },
            "geometry": {
                "type": "object",
                "additionalProperties": False,
                "properties": {
                    "present": {"type": "boolean"},
                    "source": source_enum(),
                    "notes": {"type": "string"},
                },
            },
            "receivers": {
                "type": "object",
                "additionalProperties": False,
                "properties": {
                    "present": {"type": "boolean"},
                    "count": {"type": ["integer", "null"], "minimum": 0},
                    "source": source_enum(),
                    "notes": {"type": "string"},
                },
            },
            "validation": {
                "type": "object",
                "required": ["source", "requireHermeticClosure", "maxMisses", "closureHint", "coverageHint"],
                "additionalProperties": False,
                "properties": {
                    "source": source_enum(),
                    "requireHermeticClosure": {"type": ["boolean", "null"]},
                    "maxMisses": {"type": ["integer", "null"], "minimum": 0},
                    "closureHint": {"type": ["string", "null"]},
                    "coverageHint": {"type": ["string", "null"]},
                },
            },
            "snapshots": {
                "type": "object",
                "additionalProperties": False,
                "properties": {
                    "coreSnapshotType": {"type": ["string", "null"]},
                    "godotSnapshotType": {"type": ["string", "null"]},
                    "comparisonReady": {"type": "boolean"},
                    "notes": {"type": "string"},
                },
            },
            "artifacts": {
                "type": "object",
                "additionalProperties": False,
                "properties": {
                    "coreManifestPath": {"type": ["string", "null"]},
                    "coreSnapshotPath": {"type": ["string", "null"]},
                    "godotCandidatePath": {"type": ["string", "null"]},
                    "gapMatrixPath": {"type": ["string", "null"]},
                },
            },
            "runtimeHints": {
                "type": "object",
                "required": ["requiresGodot", "requiresSnapshotBuilder", "requiresSceneTree", "requiresPhysics"],
                "additionalProperties": False,
                "properties": {
                    "requiresGodot": {"type": "boolean"},
                    "requiresSnapshotBuilder": {"type": "boolean"},
                    "requiresSceneTree": {"type": "boolean"},
                    "requiresPhysics": {"type": "boolean"},
                },
            },
            "limitations": {
                "type": "array",
                "minItems": 1,
                "items": {"type": "string"},
            },
            "normalizationNotes": {
                "type": "array",
                "items": {"type": "string"},
            },
        },
    }


def first_field(core_fixture: dict[str, Any]) -> dict[str, Any]:
    fields = core_fixture.get("fields")
    if isinstance(fields, list) and fields and isinstance(fields[0], dict):
        return fields[0]
    return {}


def build_example(
    core_fixture: dict[str, Any],
    godot_export: dict[str, Any],
    gap_matrix: dict[str, Any],
    candidate_packet: dict[str, Any],
    paths: dict[str, Path],
) -> dict[str, Any]:
    observer = core_fixture.get("observer") if isinstance(core_fixture.get("observer"), dict) else {}
    ray_grid = core_fixture.get("rayGrid") if isinstance(core_fixture.get("rayGrid"), dict) else {}
    transport = core_fixture.get("transport") if isinstance(core_fixture.get("transport"), dict) else {}
    validation = core_fixture.get("validation") if isinstance(core_fixture.get("validation"), dict) else {}
    field = first_field(core_fixture)
    godot_fixture = godot_export.get("fixture") if isinstance(godot_export.get("fixture"), dict) else {}

    return {
        "identity": {
            "name": "grin_radial_smoke_to_hermetic_observatory_grin",
            "version": "v0.preview",
            "description": "Metadata bridge candidate between Core radial GRIN smoke fixture and Godot hermetic observatory GRIN fixture.",
            "parityClaim": "NONE",
            "runtimeExecuted": False,
        },
        "sourceLinks": {
            "coreFixturePath": paths["core"].as_posix(),
            "godotFixturePath": str(godot_fixture.get("path", "Fixtures/fixture_hermetic_observatory_grin.tscn")),
            "godotExportPath": paths["godot_export"].as_posix(),
            "gapMatrixPath": paths["gap_matrix"].as_posix(),
            "candidatePacketPath": paths["candidate_packet"].as_posix(),
        },
        "observer": {
            "source": "core",
            "origin": observer.get("origin", [0, 0, -2]),
            "forward": observer.get("forward", [0, 0, 1]),
            "up": observer.get("up", [0, 1, 0]),
            "fovDegrees": observer.get("fovDegrees", 60),
        },
        "rayGrid": {
            "width": ray_grid.get("width", 40),
            "height": ray_grid.get("height", 22),
        },
        "fields": [
            {
                "source": "core",
                "type": field.get("type", "grin_radial"),
                "center": field.get("center", [0, 0, 0]),
                "radiusOuter": field.get("radiusOuter", 1.5),
                "amplitude": field.get("amplitude", 0.25),
                "curveType": field.get("curveType", "Power"),
                "gamma": field.get("gamma", 1.0),
            }
        ],
        "transport": {
            "source": "core",
            "mode": transport.get("mode", "radial_grin_smoke"),
            "maxStepsPerRay": transport.get("maxStepsPerRay", 32),
            "stepSize": transport.get("stepSize", 0.05),
            "integrator": "smoke_stepper",
        },
        "geometry": {
            "present": True,
            "source": "godot",
            "notes": "Static Godot export contains scene nodes and receiver geometry; Core fixture has no geometry model.",
        },
        "receivers": {
            "present": True,
            "count": 6,
            "source": "godot",
            "notes": "Receiver concept detected from static Godot export receiver signals.",
        },
        "validation": {
            "source": "core",
            "requireHermeticClosure": validation.get("requireHermeticClosure", False),
            "maxMisses": validation.get("maxMisses", 880),
            "closureHint": "godot_static_export_has_closure_signal",
            "coverageHint": "core_smoke_pass",
        },
        "snapshots": {
            "coreSnapshotType": "metric_snapshot",
            "godotSnapshotType": "renderer_snapshot",
            "comparisonReady": False,
            "notes": "Gap matrix marks snapshot output as partial; naming and semantics are not normalized.",
        },
        "artifacts": {
            "coreManifestPath": "output/glowing_heart/<run_id>/manifest.json",
            "coreSnapshotPath": "output/glowing_heart/<run_id>/snapshot_ascii.txt",
            "godotCandidatePath": str(godot_fixture.get("path", "Fixtures/fixture_hermetic_observatory_grin.tscn")),
            "gapMatrixPath": paths["gap_matrix"].as_posix(),
        },
        "runtimeHints": {
            "requiresGodot": False,
            "requiresSnapshotBuilder": False,
            "requiresSceneTree": False,
            "requiresPhysics": False,
        },
        "limitations": [
            "Schema draft only",
            "Godot runtime was not executed",
            "No parity claim",
            "No closure equivalence claim",
            "No transport equivalence claim",
        ],
        "normalizationNotes": build_normalization_targets(gap_matrix, candidate_packet),
    }


def section_table() -> list[tuple[str, str]]:
    return [
        ("identity", "names the bridge fixture and claim status"),
        ("sourceLinks", "records Core, Godot, export, matrix, and candidate packet paths"),
        ("observer", "defines shared camera/observer vectors and field of view"),
        ("rayGrid", "defines ray grid dimensions"),
        ("fields", "defines neutral field parameters such as radial GRIN data"),
        ("transport", "defines transport mode, step budget, step size, and integrator label"),
        ("geometry", "records whether geometry is represented and where it came from"),
        ("receivers", "records receiver presence/count hints for closure-oriented fixtures"),
        ("validation", "records closure and coverage vocabulary without claiming equivalence"),
        ("snapshots", "records Core and Godot snapshot types and comparison readiness"),
        ("artifacts", "records known output and supporting artifact paths"),
        ("runtimeHints", "records runtime requirements without executing either side"),
        ("limitations", "keeps preview and non-parity limits explicit"),
        ("normalizationNotes", "captures the next vocabulary targets before parity work"),
    ]


def build_normalization_targets(gap_matrix: dict[str, Any], candidate_packet: dict[str, Any]) -> list[str]:
    rows = gap_matrix.get("rows")
    row_text = " ".join(
        f"{row.get('category', '')} {row.get('status', '')} {row.get('reason', '')}"
        for row in rows
        if isinstance(row, dict)
    )
    fallback = candidate_packet.get("normalizationNeeded")
    targets = [
        "Shared observer definition",
        "Shared field parameter vocabulary",
        "Shared validation vocabulary",
        "Shared snapshot metric naming",
        "Godot scene metadata export standard",
    ]

    if not row_text and isinstance(fallback, list) and fallback:
        return [str(item) for item in fallback[:5]]
    return targets


def build_report(schema_path: Path, example: dict[str, Any], gap_matrix: dict[str, Any], generated: datetime) -> str:
    normalization_targets = build_normalization_targets(gap_matrix, {})

    lines = [
        "# Project Glowing Heart Shared Fixture Schema Draft (Preview)",
        "",
        f"Generated: {generated.strftime('%Y-%m-%dT%H:%M:%SZ')}",
        "",
        "Parity claim: NONE",
        "",
        "Runtime executed: false",
        "",
        "## Purpose",
        "",
        "This schema defines a neutral fixture vocabulary for bridging xPRIMEray-Core and GD_xPRIMEray.",
        "",
        "## Required Sections",
        "",
        "| Section | Purpose |",
        "|---|---|",
    ]
    lines.extend(f"| {section} | {purpose} |" for section, purpose in section_table())

    lines.extend(
        [
            "",
            "## Schema Artifact",
            "",
            schema_path.as_posix(),
            "",
            "## Normalization Targets",
            "",
        ]
    )
    lines.extend(f"- {target}" for target in normalization_targets[:5])

    lines.extend(
        [
            "",
            "## v1.4.1 Source Label Alignment",
            "",
            "The preview schema now accepts source labels used by the first shared fixture instance:",
            "",
            "- godot_static_export",
            "- mixed_metadata",
            "- core_artifact",
            "- godot_artifact",
            "",
            "This remains a preview vocabulary and does not imply parity.",
            "",
            "## Example Instance",
            "",
            "```json",
            json.dumps(example, indent=2),
            "```",
            "",
            "## What This Does Not Prove",
            "",
            "- No parity",
            "- No runtime equivalence",
            "- No closure equivalence",
            "- No transport equivalence",
            "",
            "## Next Milestone",
            "",
            "v1.4 should create the first shared fixture instance candidate using this schema draft.",
            "",
        ]
    )
    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser(description="Generate the Glowing Heart shared fixture schema preview draft.")
    parser.add_argument("--core-fixture", type=Path, default=DEFAULT_CORE_FIXTURE)
    parser.add_argument("--godot-export", type=Path, default=DEFAULT_GODOT_EXPORT)
    parser.add_argument("--gap-matrix", type=Path, default=DEFAULT_GAP_MATRIX)
    parser.add_argument("--candidate-packet", type=Path, default=DEFAULT_CANDIDATE_PACKET)
    parser.add_argument("--schema", type=Path, default=DEFAULT_SCHEMA)
    parser.add_argument("--report", type=Path, default=DEFAULT_REPORT)
    args = parser.parse_args()

    require_inputs([args.core_fixture, args.godot_export, args.gap_matrix, args.candidate_packet])
    core_fixture = load_object(args.core_fixture)
    godot_export = load_object(args.godot_export)
    gap_matrix = load_object(args.gap_matrix)
    candidate_packet = load_object(args.candidate_packet)
    paths = {
        "core": args.core_fixture,
        "godot_export": args.godot_export,
        "gap_matrix": args.gap_matrix,
        "candidate_packet": args.candidate_packet,
    }

    schema = build_schema()
    example = build_example(core_fixture, godot_export, gap_matrix, candidate_packet, paths)
    generated = datetime.now(timezone.utc)

    args.schema.parent.mkdir(parents=True, exist_ok=True)
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.schema.write_text(json.dumps(schema, indent=2) + "\n", encoding="utf-8")
    args.report.write_text(build_report(args.schema, example, gap_matrix, generated), encoding="utf-8")

    print("[glowing-heart-shared-schema]")
    print(f"schema={args.schema}")
    print(f"report={args.report}")
    print("parity_claim=NONE")
    print("runtime_executed=false")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except SchemaDraftError as exc:
        print(f"[glowing-heart-shared-schema] ERROR: {exc}")
        raise SystemExit(1)
