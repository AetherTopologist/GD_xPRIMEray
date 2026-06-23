#!/usr/bin/env python3
"""Generate the v1.4 shared fixture instance preview candidate.

This is metadata bridge generation only. It does not execute Godot, instantiate
scene trees, modify Core transport, modify renderer lifecycle files, or claim
parity.
"""

from __future__ import annotations

import argparse
import json
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


DEFAULT_SCHEMA = Path("schemas/glowing_heart/shared_fixture_schema.v0.preview.json")
DEFAULT_CORE_FIXTURE = Path("fixtures/grin_radial_smoke.json")
DEFAULT_CANDIDATE_PACKET = Path("reports/glowing_heart_shared_fixture_candidate.preview.json")
DEFAULT_GODOT_EXPORT = Path("reports/glowing_heart_godot_fixture_export.preview.json")
DEFAULT_GAP_MATRIX = Path("reports/glowing_heart_gap_matrix.preview.json")
DEFAULT_SCHEMA_REPORT = Path("reports/glowing_heart_shared_fixture_schema.preview.md")
DEFAULT_INSTANCE = Path("fixtures/shared/glowing_heart_grin_bridge.v0.preview.json")
DEFAULT_REPORT = Path("reports/glowing_heart_shared_fixture_instance.preview.md")

TOP_LEVEL_SECTIONS = [
    "identity",
    "sourceLinks",
    "observer",
    "rayGrid",
    "fields",
    "transport",
    "geometry",
    "receivers",
    "validation",
    "snapshots",
    "artifacts",
    "runtimeHints",
    "limitations",
    "normalizationNotes",
]

LIMITATIONS = [
    "Schema draft only",
    "Shared fixture instance candidate only",
    "Godot runtime was not executed",
    "Godot scene graph was not instantiated",
    "No parity claim",
    "No closure equivalence claim",
    "No transport equivalence claim",
    "No pixel comparison claim",
]

NORMALIZATION_NOTES = [
    "Observer/camera mapping must be normalized before pixel comparison",
    "Field parameter vocabulary must be normalized between Core and Godot",
    "Validation vocabulary must distinguish smoke coverage from hermetic closure",
    "Snapshot metric naming must distinguish bend magnitude from rendered intensity",
    "Receiver and closure semantics must be exported from Godot metadata before parity review",
]


class InstanceError(Exception):
    pass


def utc_now() -> str:
    return datetime.now(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")


def load_object(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:
        raise InstanceError(f"{path}: failed to load JSON: {exc}") from exc
    if not isinstance(value, dict):
        raise InstanceError(f"{path}: expected JSON object")
    return value


def require_inputs(paths: list[Path]) -> None:
    for path in paths:
        if not path.is_file():
            raise InstanceError(f"{path}: required input not found")


def require_dict(value: Any, path: str) -> dict[str, Any]:
    if not isinstance(value, dict):
        raise InstanceError(f"{path}: expected object")
    return value


def require_list(value: Any, path: str) -> list[Any]:
    if not isinstance(value, list):
        raise InstanceError(f"{path}: expected array")
    return value


def selected_godot_path(candidate: dict[str, Any], fallback: str) -> str:
    godot_candidate = candidate.get("godotCandidate")
    if isinstance(godot_candidate, dict) and isinstance(godot_candidate.get("path"), str):
        return godot_candidate["path"]
    return fallback


def godot_hint(godot_export: dict[str, Any], key: str) -> bool:
    hints = godot_export.get("classifiedHints")
    return isinstance(hints, dict) and hints.get(key) is True


def build_instance(
    core_fixture: dict[str, Any],
    candidate: dict[str, Any],
    godot_export: dict[str, Any],
    gap_matrix: dict[str, Any],
) -> dict[str, Any]:
    observer = require_dict(core_fixture.get("observer"), "fixtures/grin_radial_smoke.json:observer")
    ray_grid = require_dict(core_fixture.get("rayGrid"), "fixtures/grin_radial_smoke.json:rayGrid")
    transport = require_dict(core_fixture.get("transport"), "fixtures/grin_radial_smoke.json:transport")
    fields = require_list(core_fixture.get("fields"), "fixtures/grin_radial_smoke.json:fields")
    validation = require_dict(core_fixture.get("validation"), "fixtures/grin_radial_smoke.json:validation")

    godot_fixture_path = selected_godot_path(
        candidate,
        "Fixtures/fixture_hermetic_observatory_grin.tscn",
    )
    has_receiver_signal = godot_hint(godot_export, "hasReceiverSignal")
    has_closure_signal = godot_hint(godot_export, "hasClosureSignal")

    instance_fields: list[dict[str, Any]] = []
    for index, field in enumerate(fields):
        field_obj = require_dict(field, f"fixtures/grin_radial_smoke.json:fields[{index}]")
        instance_fields.append(
            {
                "source": "core",
                "type": field_obj.get("type"),
                "center": field_obj.get("center"),
                "radiusOuter": field_obj.get("radiusOuter"),
                "amplitude": field_obj.get("amplitude"),
                "curveType": field_obj.get("curveType"),
                "gamma": field_obj.get("gamma"),
            }
        )

    _ = gap_matrix  # Loaded intentionally; the instance references its path and uses v1.3 context.

    return {
        "identity": {
            "name": "glowing_heart_grin_bridge",
            "version": "v0.preview",
            "description": "Metadata bridge candidate linking Core radial GRIN smoke fixture to Godot hermetic observatory GRIN fixture.",
            "parityClaim": "NONE",
            "runtimeExecuted": False,
        },
        "sourceLinks": {
            "coreFixturePath": "fixtures/grin_radial_smoke.json",
            "godotFixturePath": godot_fixture_path,
            "godotExportPath": "reports/glowing_heart_godot_fixture_export.preview.json",
            "gapMatrixPath": "reports/glowing_heart_gap_matrix.preview.json",
            "candidatePacketPath": "reports/glowing_heart_shared_fixture_candidate.preview.json",
        },
        "observer": {
            "source": "core",
            "origin": observer.get("origin"),
            "forward": observer.get("forward"),
            "up": observer.get("up"),
            "fovDegrees": observer.get("fovDegrees"),
        },
        "rayGrid": {
            "width": ray_grid.get("width"),
            "height": ray_grid.get("height"),
        },
        "fields": instance_fields,
        "transport": {
            "source": "core",
            "mode": transport.get("mode"),
            "maxStepsPerRay": transport.get("maxStepsPerRay"),
            "stepSize": transport.get("stepSize"),
            "integrator": "smoke_stepper",
        },
        "geometry": {
            "present": False,
            "source": "core",
            "notes": "Core radial GRIN smoke fixture does not model scene geometry yet.",
        },
        "receivers": {
            "present": has_receiver_signal,
            "count": None,
            "source": "godot_static_export",
            "notes": (
                "Receiver signal detected in static Godot fixture export; count not normalized."
                if has_receiver_signal
                else "No receiver signal detected in static Godot fixture export."
            ),
        },
        "validation": {
            "source": "mixed_metadata",
            "requireHermeticClosure": False,
            "maxMisses": validation.get("maxMisses"),
            "closureHint": (
                "godot_static_export_has_closure_signal"
                if has_closure_signal
                else "godot_static_export_closure_signal_unknown"
            ),
            "coverageHint": "core_smoke_pass",
        },
        "snapshots": {
            "coreSnapshotType": "bend_magnitude_metric_snapshot",
            "godotSnapshotType": "renderer_or_observatory_snapshot_candidate",
            "comparisonReady": False,
            "notes": "Core emits metric snapshots; Godot selected fixture has not yet exported a comparable snapshot.",
        },
        "artifacts": {
            "coreManifestPath": "output/glowing_heart/latest/manifest.json",
            "coreSnapshotPath": "output/glowing_heart/latest/snapshot.ppm",
            "godotCandidatePath": godot_fixture_path,
            "gapMatrixPath": "reports/glowing_heart_gap_matrix.preview.json",
        },
        "runtimeHints": {
            "requiresGodot": False,
            "requiresSnapshotBuilder": False,
            "requiresSceneTree": False,
            "requiresPhysics": False,
        },
        "limitations": LIMITATIONS,
        "normalizationNotes": NORMALIZATION_NOTES,
    }


def validate_instance(instance: dict[str, Any]) -> None:
    missing = [section for section in TOP_LEVEL_SECTIONS if section not in instance]
    if missing:
        raise InstanceError(f"instance missing top-level sections: {', '.join(missing)}")
    extra = [section for section in instance if section not in TOP_LEVEL_SECTIONS]
    if extra:
        raise InstanceError(f"instance has unexpected top-level sections: {', '.join(extra)}")
    identity = require_dict(instance.get("identity"), "identity")
    if identity.get("parityClaim") != "NONE":
        raise InstanceError("identity.parityClaim must remain NONE")
    if identity.get("runtimeExecuted") is not False:
        raise InstanceError("identity.runtimeExecuted must remain false")


def write_json(path: Path, value: dict[str, Any]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")


def build_markdown(instance: dict[str, Any], generated_utc: str) -> str:
    identity = require_dict(instance["identity"], "identity")
    links = require_dict(instance["sourceLinks"], "sourceLinks")
    receivers = require_dict(instance["receivers"], "receivers")
    validation = require_dict(instance["validation"], "validation")

    receiver_status = "hinted" if receivers.get("present") else "absent"

    return f"""# Project Glowing Heart Shared Fixture Instance (Preview)

Generated: {generated_utc}

Runtime executed: false

Parity claim: NONE

## Instance

| Field | Value |
|---|---|
| Name | {identity["name"]} |
| Version | {identity["version"]} |
| Core Fixture | {links["coreFixturePath"]} |
| Godot Fixture | {links["godotFixturePath"]} |

## What This Instance Represents

A metadata bridge candidate between the Core radial GRIN smoke fixture and the Godot hermetic observatory GRIN fixture.

## Shared Sections

| Section | Source | Status |
|---|---|---|
| observer | core | present |
| rayGrid | core | present |
| fields | core | present |
| transport | core | present |
| receivers | godot_static_export | {receiver_status} |
| validation | mixed_metadata | partial |

## Hints

| Hint | Value |
|---|---|
| Receivers | {receivers.get("notes")} |
| Closure | {validation.get("closureHint")} |

## Not Ready For

- parity
- pixel comparison
- closure equivalence
- transport equivalence
- public demo claim

## Next Step

v1.5 should create a public-demo readiness checklist and rank what is still needed before Grok begins public interface/demo framing.
"""


def write_markdown(path: Path, value: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(value, encoding="utf-8")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--schema", type=Path, default=DEFAULT_SCHEMA)
    parser.add_argument("--core-fixture", type=Path, default=DEFAULT_CORE_FIXTURE)
    parser.add_argument("--candidate-packet", type=Path, default=DEFAULT_CANDIDATE_PACKET)
    parser.add_argument("--godot-export", type=Path, default=DEFAULT_GODOT_EXPORT)
    parser.add_argument("--gap-matrix", type=Path, default=DEFAULT_GAP_MATRIX)
    parser.add_argument("--schema-report", type=Path, default=DEFAULT_SCHEMA_REPORT)
    parser.add_argument("--instance", type=Path, default=DEFAULT_INSTANCE)
    parser.add_argument("--report", type=Path, default=DEFAULT_REPORT)
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    require_inputs(
        [
            args.schema,
            args.core_fixture,
            args.candidate_packet,
            args.godot_export,
            args.gap_matrix,
            args.schema_report,
        ]
    )

    core_fixture = load_object(args.core_fixture)
    candidate = load_object(args.candidate_packet)
    godot_export = load_object(args.godot_export)
    gap_matrix = load_object(args.gap_matrix)
    instance = build_instance(core_fixture, candidate, godot_export, gap_matrix)
    validate_instance(instance)

    generated_utc = utc_now()
    write_json(args.instance, instance)
    write_markdown(args.report, build_markdown(instance, generated_utc))

    links = instance["sourceLinks"]
    identity = instance["identity"]
    print("[glowing-heart-shared-instance]")
    print(f"instance={args.instance}")
    print(f"core_fixture={links['coreFixturePath']}")
    print(f"godot_fixture={links['godotFixturePath']}")
    print(f"runtime_executed={str(identity['runtimeExecuted']).lower()}")
    print(f"parity_claim={identity['parityClaim']}")
    print()
    print(f"wrote={args.instance}")
    print(f"wrote={args.report}")


if __name__ == "__main__":
    main()
