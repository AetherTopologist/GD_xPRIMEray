#!/usr/bin/env python3
"""Generate the v1.8.1 observer reconciliation audit.

This is static metadata reconciliation only. It does not execute Godot, modify
scenes, modify Core transport, touch renderer lifecycle files, or claim parity.
"""

from __future__ import annotations

import json
import math
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


INPUT = Path("reports/glowing_heart_observer_instances.preview.json")
CONTRACT = Path("schemas/glowing_heart/shared_observer_contract.v0.preview.json")
OUTPUT_JSON = Path("reports/glowing_heart_observer_reconciliation.preview.json")
OUTPUT_MD = Path("reports/glowing_heart_observer_reconciliation.preview.md")
STATUSES = ("MATCH", "MISMATCH", "PARTIAL", "UNKNOWN")
TOLERANCE = 1e-9

REQUIRED_READY_CATEGORIES = {
    "position",
    "forward",
    "up",
    "fov_degrees",
    "fov_axis",
    "resolution",
    "projection_type",
    "pixel_sampling",
    "image_origin",
    "aspect_policy",
}

RECOMMENDED_NEXT_ACTIONS = [
    "Define a v1.8.2 shared observer target with explicit agreed values: position, forward, up, FOV, resolution, projection, near/far, pixel sampling, image origin, and aspect policy.",
    "Create a shared observer target instance with explicit agreed pose/FOV/resolution.",
    "Update Core fixture or shared instance to reference that target.",
    "Export or statically define a Godot observer candidate matching that target.",
    "Add pixelSampling, imageOrigin, and aspectPolicy to enforced observer instances.",
    "Only attempt difference.ppm after observer reconciliation passes.",
]


class ReconciliationError(Exception):
    pass


def utc_now() -> str:
    return datetime.now(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")


def load_json_object(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:
        raise ReconciliationError(f"{path}: failed to load JSON: {exc}") from exc
    if not isinstance(value, dict):
        raise ReconciliationError(f"{path}: expected JSON object")
    return value


def require_inputs(paths: list[Path]) -> None:
    for path in paths:
        if not path.is_file():
            raise ReconciliationError(f"{path}: required input not found")


def require_dict(value: Any, label: str) -> dict[str, Any]:
    if not isinstance(value, dict):
        raise ReconciliationError(f"{label}: expected object")
    return value


def is_number(value: Any) -> bool:
    return isinstance(value, (int, float)) and not isinstance(value, bool)


def is_unknown_value(value: Any) -> bool:
    return value is None or value == "unknown" or value == "unknown_static_tscn"


def numeric_vec3(value: Any) -> list[float] | None:
    if not isinstance(value, list) or len(value) != 3:
        return None
    if any(not is_number(item) for item in value):
        return None
    return [float(item) for item in value]


def normalize(value: list[float]) -> list[float] | None:
    length = math.sqrt(sum(item * item for item in value))
    if length <= TOLERANCE:
        return None
    return [item / length for item in value]


def dot(a: list[float] | None, b: list[float] | None) -> float | None:
    if a is None or b is None:
        return None
    norm_a = normalize(a)
    norm_b = normalize(b)
    if norm_a is None or norm_b is None:
        return None
    return sum(left * right for left, right in zip(norm_a, norm_b))


def euclidean_delta(a: list[float] | None, b: list[float] | None) -> float | None:
    if a is None or b is None:
        return None
    return math.sqrt(sum((left - right) ** 2 for left, right in zip(a, b)))


def clean_number(value: float | None) -> float | None:
    if value is None:
        return None
    return round(value, 12)


def vector_status(core: Any, godot: Any) -> str:
    core_vec = numeric_vec3(core)
    godot_vec = numeric_vec3(godot)
    if core_vec is None or godot_vec is None:
        return "UNKNOWN"
    if all(abs(left - right) <= TOLERANCE for left, right in zip(core_vec, godot_vec)):
        return "MATCH"
    return "MISMATCH"


def scalar_status(core: Any, godot: Any) -> str:
    if is_unknown_value(core) or is_unknown_value(godot):
        return "UNKNOWN"
    if core == godot:
        return "MATCH"
    if is_number(core) and is_number(godot) and abs(float(core) - float(godot)) <= TOLERANCE:
        return "MATCH"
    return "MISMATCH"


def resolution_status(core: dict[str, Any], godot: dict[str, Any]) -> str:
    core_width = core.get("width")
    core_height = core.get("height")
    godot_width = godot.get("width")
    godot_height = godot.get("height")
    if any(is_unknown_value(value) for value in [core_width, core_height, godot_width, godot_height]):
        return "UNKNOWN"
    return "MATCH" if core_width == godot_width and core_height == godot_height else "MISMATCH"


def coordinate_handedness_status(core: dict[str, Any], godot: dict[str, Any]) -> str:
    core_value = core.get("handedness")
    godot_value = godot.get("handedness")
    if is_unknown_value(core_value) or is_unknown_value(godot_value):
        return "UNKNOWN"
    if core_value == godot_value:
        return "MATCH"
    if isinstance(core_value, str) and isinstance(godot_value, str):
        if "right_handed" in core_value and "right_handed" in godot_value:
            return "PARTIAL"
    return "MISMATCH"


def pixel_sampling_status(core: dict[str, Any], godot: dict[str, Any]) -> str:
    core_value = core.get("samplePosition")
    godot_value = godot.get("samplePosition")
    if is_unknown_value(core_value) or is_unknown_value(godot_value):
        return "UNKNOWN"
    return "MATCH" if core_value == godot_value else "MISMATCH"


def image_origin_status(core: dict[str, Any], godot: dict[str, Any]) -> str:
    values = [core.get("origin"), core.get("rowOrder"), godot.get("origin"), godot.get("rowOrder")]
    if any(is_unknown_value(value) for value in values):
        return "UNKNOWN"
    return "MATCH" if core.get("origin") == godot.get("origin") and core.get("rowOrder") == godot.get("rowOrder") else "MISMATCH"


def aspect_policy_status(core: dict[str, Any], godot: dict[str, Any]) -> str:
    core_policy = core.get("policy")
    godot_policy = godot.get("policy")
    if is_unknown_value(core_policy) or is_unknown_value(godot_policy):
        return "UNKNOWN"
    return "MATCH" if core_policy == godot_policy else "MISMATCH"


def reason_for(category: str, status: str, core: Any, godot: Any, metrics: dict[str, Any]) -> str:
    if category == "position":
        return "Core and Godot observer positions differ." if status == "MISMATCH" else "Core and Godot observer positions match."
    if category == "forward":
        if status == "MISMATCH" and metrics.get("forwardDot") == -1:
            return "Core and Godot forward vectors oppose each other."
        return "Core and Godot forward vectors differ." if status == "MISMATCH" else "Core and Godot forward vectors match."
    if category == "up":
        return "Core and Godot up vectors match." if status == "MATCH" else "Core and Godot up vectors differ."
    if category == "fov_degrees":
        return "Core and Godot FOV degrees differ." if status == "MISMATCH" else "Core and Godot FOV degrees match."
    if category == "fov_axis":
        return "Core and Godot FOV axes match." if status == "MATCH" else "Core and Godot FOV axis comparison is incomplete."
    if category == "resolution":
        return "Godot resolution is unknown from static metadata." if status == "UNKNOWN" else "Core and Godot resolutions differ."
    if category == "projection_type":
        return "Core and Godot projection types match." if status == "MATCH" else "Core and Godot projection types differ."
    if category == "near":
        return "Core near clip plane is not specified; comparison is underspecified." if status == "UNKNOWN" else "Core and Godot near clip planes differ."
    if category == "far":
        return "Core far clip plane is not specified; comparison is underspecified." if status == "UNKNOWN" else "Core and Godot far clip planes differ."
    if category == "coordinate_handedness":
        return "Both observers indicate right-handed coordinates, but labels are source-specific/inferred." if status == "PARTIAL" else "Coordinate handedness comparison is incomplete."
    if category == "pixel_sampling":
        return "Godot pixel sampling convention is unknown from static metadata." if status == "UNKNOWN" else "Pixel sampling conventions match."
    if category == "image_origin":
        return "Godot image origin and row order are unknown from static metadata." if status == "UNKNOWN" else "Image origin conventions match."
    if category == "aspect_policy":
        return "Godot aspect policy is unknown from static metadata." if status == "UNKNOWN" else "Aspect policies match."
    return f"{category} status is {status}."


def check(category: str, status: str, core: Any, godot: Any, metrics: dict[str, Any]) -> dict[str, Any]:
    if status not in STATUSES:
        raise ReconciliationError(f"{category}: invalid status {status}")
    return {
        "category": category,
        "status": status,
        "core": core,
        "godot": godot,
        "reason": reason_for(category, status, core, godot, metrics),
    }


def build_checks(core: dict[str, Any], godot: dict[str, Any], metrics: dict[str, Any]) -> list[dict[str, Any]]:
    core_fov = require_dict(core.get("fov"), "coreObserver.fov")
    godot_fov = require_dict(godot.get("fov"), "godotObserver.fov")
    core_resolution = require_dict(core.get("resolution"), "coreObserver.resolution")
    godot_resolution = require_dict(godot.get("resolution"), "godotObserver.resolution")
    core_projection = require_dict(core.get("projection"), "coreObserver.projection")
    godot_projection = require_dict(godot.get("projection"), "godotObserver.projection")
    core_coordinate = require_dict(core.get("coordinate_system"), "coreObserver.coordinate_system")
    godot_coordinate = require_dict(godot.get("coordinate_system"), "godotObserver.coordinate_system")
    core_pixel = require_dict(core.get("pixelSampling"), "coreObserver.pixelSampling")
    godot_pixel = require_dict(godot.get("pixelSampling"), "godotObserver.pixelSampling")
    core_origin = require_dict(core.get("imageOrigin"), "coreObserver.imageOrigin")
    godot_origin = require_dict(godot.get("imageOrigin"), "godotObserver.imageOrigin")
    core_aspect = require_dict(core.get("aspectPolicy"), "coreObserver.aspectPolicy")
    godot_aspect = require_dict(godot.get("aspectPolicy"), "godotObserver.aspectPolicy")

    return [
        check("position", vector_status(core.get("position"), godot.get("position")), core.get("position"), godot.get("position"), metrics),
        check("forward", vector_status(core.get("forward"), godot.get("forward")), core.get("forward"), godot.get("forward"), metrics),
        check("up", vector_status(core.get("up"), godot.get("up")), core.get("up"), godot.get("up"), metrics),
        check("fov_degrees", scalar_status(core_fov.get("degrees"), godot_fov.get("degrees")), core_fov.get("degrees"), godot_fov.get("degrees"), metrics),
        check("fov_axis", scalar_status(core_fov.get("axis"), godot_fov.get("axis")), core_fov.get("axis"), godot_fov.get("axis"), metrics),
        check("resolution", resolution_status(core_resolution, godot_resolution), core_resolution, godot_resolution, metrics),
        check("projection_type", scalar_status(core_projection.get("type"), godot_projection.get("type")), core_projection.get("type"), godot_projection.get("type"), metrics),
        check("near", scalar_status(core.get("near"), godot.get("near")), core.get("near"), godot.get("near"), metrics),
        check("far", scalar_status(core.get("far"), godot.get("far")), core.get("far"), godot.get("far"), metrics),
        check("coordinate_handedness", coordinate_handedness_status(core_coordinate, godot_coordinate), core_coordinate.get("handedness"), godot_coordinate.get("handedness"), metrics),
        check("pixel_sampling", pixel_sampling_status(core_pixel, godot_pixel), core_pixel, godot_pixel, metrics),
        check("image_origin", image_origin_status(core_origin, godot_origin), core_origin, godot_origin, metrics),
        check("aspect_policy", aspect_policy_status(core_aspect, godot_aspect), core_aspect, godot_aspect, metrics),
    ]


def summary_counts(checks: list[dict[str, Any]]) -> dict[str, int]:
    summary = {status: 0 for status in STATUSES}
    for item in checks:
        summary[item["status"]] += 1
    return summary


def pixel_ready(checks: list[dict[str, Any]]) -> bool:
    status_by_category = {item["category"]: item["status"] for item in checks}
    return all(status_by_category.get(category) == "MATCH" for category in REQUIRED_READY_CATEGORIES)


def blocking_deltas(checks: list[dict[str, Any]]) -> list[str]:
    reasons = {item["category"]: item["reason"] for item in checks if item["status"] != "MATCH"}
    ordered = [
        "Core and Godot observer positions differ.",
        "Core and Godot forward vectors oppose each other.",
        "Core and Godot FOV degrees differ.",
        "Godot resolution is unknown from static metadata.",
        "Godot pixel sampling convention is unknown from static metadata.",
        "Godot image origin and row order are unknown from static metadata.",
        "Godot aspect policy is unknown from static metadata.",
        "Core near/far are not specified in the Core fixture.",
    ]
    actual = set(reasons.values())
    if "Core near clip plane is not specified; comparison is underspecified." in actual or "Core far clip plane is not specified; comparison is underspecified." in actual:
        actual.add("Core near/far are not specified in the Core fixture.")

    result = [item for item in ordered if item in actual]
    for reason in reasons.values():
        if reason not in result and reason not in {
            "Core near clip plane is not specified; comparison is underspecified.",
            "Core far clip plane is not specified; comparison is underspecified.",
        }:
            result.append(reason)
    return result


def build_metrics(core: dict[str, Any], godot: dict[str, Any]) -> dict[str, Any]:
    core_position = numeric_vec3(core.get("position"))
    godot_position = numeric_vec3(godot.get("position"))
    core_forward = numeric_vec3(core.get("forward"))
    godot_forward = numeric_vec3(godot.get("forward"))
    core_up = numeric_vec3(core.get("up"))
    godot_up = numeric_vec3(godot.get("up"))
    core_fov = require_dict(core.get("fov"), "coreObserver.fov").get("degrees")
    godot_fov = require_dict(godot.get("fov"), "godotObserver.fov").get("degrees")
    core_near = core.get("near")
    godot_near = godot.get("near")
    core_far = core.get("far")
    godot_far = godot.get("far")

    fov_delta = abs(float(core_fov) - float(godot_fov)) if is_number(core_fov) and is_number(godot_fov) else None
    near_delta = abs(float(core_near) - float(godot_near)) if is_number(core_near) and is_number(godot_near) else None
    far_delta = abs(float(core_far) - float(godot_far)) if is_number(core_far) and is_number(godot_far) else None

    return {
        "positionDeltaEuclidean": clean_number(euclidean_delta(core_position, godot_position)),
        "forwardDot": clean_number(dot(core_forward, godot_forward)),
        "upDot": clean_number(dot(core_up, godot_up)),
        "fovDeltaDegrees": clean_number(fov_delta),
        "nearDelta": clean_number(near_delta),
        "farDelta": clean_number(far_delta),
    }


def build_packet(generated: str, observer_packet: dict[str, Any]) -> dict[str, Any]:
    core = require_dict(observer_packet.get("coreObserver"), "coreObserver")
    godot = require_dict(observer_packet.get("godotObserver"), "godotObserver")
    metrics = build_metrics(core, godot)
    checks = build_checks(core, godot, metrics)
    ready = pixel_ready(checks)

    return {
        "schema": "xprimeray.glowing_heart.observer_reconciliation.v1.8.1",
        "generatedUtc": generated,
        "runtimeExecuted": False,
        "parityClaim": "NONE",
        "input": INPUT.as_posix(),
        "pixelComparisonReady": ready,
        "summary": summary_counts(checks),
        "metrics": metrics,
        "checks": checks,
        "blockingDeltas": blocking_deltas(checks),
        "recommendedNextActions": RECOMMENDED_NEXT_ACTIONS,
    }


def format_value(value: Any) -> str:
    if value is None:
        return "null"
    if isinstance(value, (list, dict)):
        return json.dumps(value)
    return str(value)


def render_markdown(packet: dict[str, Any]) -> str:
    summary = packet["summary"]
    metrics = packet["metrics"]
    checks = packet["checks"]
    blocking = "\n".join(f"- {item}" for item in packet["blockingDeltas"])
    actions = "\n".join(f"- {item}" for item in packet["recommendedNextActions"])
    checks_rows = "\n".join(f"| {item['category']} | {item['status']} | {item['reason']} |" for item in checks)
    metric_rows = "\n".join(f"| {key} | {format_value(value)} |" for key, value in metrics.items())

    return f"""# Project Glowing Heart Observer Reconciliation (Preview)

Generated: {packet["generatedUtc"]}

Runtime executed: false

Parity claim: NONE

Pixel comparison ready: {str(packet["pixelComparisonReady"]).lower()}

## Summary

| Status | Count |
|---|---:|
| MATCH | {summary["MATCH"]} |
| MISMATCH | {summary["MISMATCH"]} |
| PARTIAL | {summary["PARTIAL"]} |
| UNKNOWN | {summary["UNKNOWN"]} |

## Numeric Metrics

| Metric | Value |
|---|---|
{metric_rows}

## Checks

| Category | Status | Reason |
|---|---|---|
{checks_rows}

## Blocking Deltas

{blocking}

## Recommended Next Actions

{actions}

## Bottom Line

The observer instances are not yet reconciled. Pixel comparison is not ready.
"""


def main() -> int:
    try:
        require_inputs([INPUT, CONTRACT])
        observer_packet = load_json_object(INPUT)
        _ = load_json_object(CONTRACT)
        packet = build_packet(utc_now(), observer_packet)

        OUTPUT_JSON.parent.mkdir(parents=True, exist_ok=True)
        OUTPUT_MD.parent.mkdir(parents=True, exist_ok=True)
        OUTPUT_JSON.write_text(json.dumps(packet, indent=2) + "\n", encoding="utf-8")
        OUTPUT_MD.write_text(render_markdown(packet), encoding="utf-8")
    except ReconciliationError as exc:
        print(f"[glowing-heart-observer-reconciliation] ERROR: {exc}")
        return 1

    status_by_category = {item["category"]: item["status"] for item in packet["checks"]}
    print("[glowing-heart-observer-reconciliation]")
    print(f"pixel_comparison_ready={str(packet['pixelComparisonReady']).lower()}")
    print(f"position_status={status_by_category['position']}")
    print(f"forward_status={status_by_category['forward']}")
    print(f"fov_status={status_by_category['fov_degrees']}")
    print(f"runtime_executed={str(packet['runtimeExecuted']).lower()}")
    print(f"parity_claim={packet['parityClaim']}")
    print()
    print(f"wrote={OUTPUT_JSON.as_posix()}")
    print(f"wrote={OUTPUT_MD.as_posix()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
