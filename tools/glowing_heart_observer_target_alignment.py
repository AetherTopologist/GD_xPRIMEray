#!/usr/bin/env python3
"""Generate the v1.8.3 observer-to-target alignment report.

This is static target-alignment reporting only. It does not execute Godot,
modify scenes, modify Core transport, touch renderer lifecycle files, or claim
parity.
"""

from __future__ import annotations

import json
import math
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


OBSERVER_INSTANCES = Path("reports/glowing_heart_observer_instances.preview.json")
RECONCILIATION = Path("reports/glowing_heart_observer_reconciliation.preview.json")
TARGET = Path("fixtures/shared/glowing_heart_observer_target.v0.preview.json")
CONTRACT = Path("schemas/glowing_heart/shared_observer_contract.v0.preview.json")
OUTPUT_JSON = Path("reports/glowing_heart_observer_target_alignment.preview.json")
OUTPUT_MD = Path("reports/glowing_heart_observer_target_alignment.preview.md")

STATUSES = ("MATCH", "MISMATCH", "PARTIAL", "UNKNOWN")
TOLERANCE = 1e-9
CHECK_CATEGORIES = (
    "position",
    "forward",
    "up",
    "fov_degrees",
    "fov_axis",
    "resolution",
    "projection_type",
    "near",
    "far",
    "coordinate_handedness",
    "pixel_sampling",
    "image_origin",
    "aspect_policy",
)

CORE_RECOMMENDED_ACTIONS = [
    "Make near/far explicit in Core fixture or shared instance.",
    "Confirm observer contract fields are emitted by Core tooling.",
    "Normalize inferred right-handed coordinate labels to the shared target vocabulary.",
]

GODOT_RECOMMENDED_ACTIONS = [
    "Configure or document Camera3D pose to target position [0,0,-2].",
    "Configure Camera3D to look along target forward [0,0,1], or document adapter transform.",
    "Set/confirm FOV 60 vertical.",
    "Define output resolution 40x22 for comparison fixture.",
    "Export pixel sampling, image origin, and aspect policy.",
    "Normalize Godot coordinate labels to the shared target vocabulary.",
]

GLOBAL_NEXT_ACTIONS = [
    "Do not attempt difference.ppm until both sides match the target.",
    "Create a snapshot channel contract before comparing pixels.",
    "Keep public language at perceptual demo / engineering prototype level.",
]


class AlignmentError(Exception):
    pass


def utc_now() -> str:
    return datetime.now(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")


def load_json_object(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:
        raise AlignmentError(f"{path}: failed to load JSON: {exc}") from exc
    if not isinstance(value, dict):
        raise AlignmentError(f"{path}: expected JSON object")
    return value


def require_inputs(paths: list[Path]) -> None:
    for path in paths:
        if not path.is_file():
            raise AlignmentError(f"{path}: required input not found")


def require_dict(value: Any, label: str) -> dict[str, Any]:
    if not isinstance(value, dict):
        raise AlignmentError(f"{label}: expected object")
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


def vector_status(side: Any, target: Any) -> str:
    side_vec = numeric_vec3(side)
    target_vec = numeric_vec3(target)
    if side_vec is None or target_vec is None:
        return "UNKNOWN"
    if all(abs(left - right) <= TOLERANCE for left, right in zip(side_vec, target_vec)):
        return "MATCH"
    return "MISMATCH"


def scalar_status(side: Any, target: Any) -> str:
    if is_unknown_value(side) or is_unknown_value(target):
        return "UNKNOWN"
    if side == target:
        return "MATCH"
    if is_number(side) and is_number(target) and abs(float(side) - float(target)) <= TOLERANCE:
        return "MATCH"
    return "MISMATCH"


def resolution_status(side: dict[str, Any], target: dict[str, Any]) -> str:
    values = [side.get("width"), side.get("height"), target.get("width"), target.get("height")]
    if any(is_unknown_value(value) for value in values):
        return "UNKNOWN"
    return "MATCH" if side.get("width") == target.get("width") and side.get("height") == target.get("height") else "MISMATCH"


def coordinate_handedness_status(side: dict[str, Any], target: dict[str, Any]) -> str:
    side_value = side.get("handedness")
    target_value = target.get("handedness")
    if is_unknown_value(side_value) or is_unknown_value(target_value):
        return "UNKNOWN"
    if side_value == target_value:
        return "MATCH"
    if isinstance(side_value, str) and isinstance(target_value, str) and target_value in side_value:
        return "PARTIAL"
    return "MISMATCH"


def pixel_sampling_status(side: dict[str, Any], target: dict[str, Any]) -> str:
    keys = ("samplePosition", "xConvention", "yConvention")
    if any(is_unknown_value(side.get(key)) or is_unknown_value(target.get(key)) for key in keys):
        return "UNKNOWN"
    return "MATCH" if all(side.get(key) == target.get(key) for key in keys) else "MISMATCH"


def image_origin_status(side: dict[str, Any], target: dict[str, Any]) -> str:
    keys = ("origin", "rowOrder")
    if any(is_unknown_value(side.get(key)) or is_unknown_value(target.get(key)) for key in keys):
        return "UNKNOWN"
    return "MATCH" if all(side.get(key) == target.get(key) for key in keys) else "MISMATCH"


def aspect_policy_status(side: dict[str, Any], target: dict[str, Any]) -> str:
    side_policy = side.get("policy")
    target_policy = target.get("policy")
    if is_unknown_value(side_policy) or is_unknown_value(target_policy):
        return "UNKNOWN"
    return "MATCH" if side_policy == target_policy else "MISMATCH"


def reason_for(side_name: str, category: str, status: str, metrics: dict[str, Any]) -> str:
    prefix = f"{side_name} observer"
    if status == "MATCH":
        return f"{prefix} {category} matches the shared target."
    if status == "UNKNOWN":
        if category in {"near", "far"}:
            return f"{prefix} {category} is underspecified relative to the shared target."
        return f"{prefix} {category} is unknown or underspecified relative to the shared target."
    if status == "PARTIAL":
        return f"{prefix} {category} partially matches the shared target but uses source-specific vocabulary."
    if category == "forward" and metrics.get("forwardDot") == -1:
        return f"{prefix} forward vector opposes the shared target."
    return f"{prefix} {category} differs from the shared target."


def check(side_name: str, category: str, status: str, side: Any, target: Any, metrics: dict[str, Any]) -> dict[str, Any]:
    if status not in STATUSES:
        raise AlignmentError(f"{category}: invalid status {status}")
    return {
        "category": category,
        "status": status,
        "side": side,
        "target": target,
        "reason": reason_for(side_name, category, status, metrics),
    }


def build_metrics(side: dict[str, Any], target: dict[str, Any]) -> dict[str, Any]:
    side_position = numeric_vec3(side.get("position"))
    target_position = numeric_vec3(target.get("position"))
    side_forward = numeric_vec3(side.get("forward"))
    target_forward = numeric_vec3(target.get("forward"))
    side_up = numeric_vec3(side.get("up"))
    target_up = numeric_vec3(target.get("up"))
    side_fov = require_dict(side.get("fov"), "side.fov").get("degrees")
    target_fov = require_dict(target.get("fov"), "target.fov").get("degrees")
    side_near = side.get("near")
    target_near = target.get("near")
    side_far = side.get("far")
    target_far = target.get("far")

    fov_delta = abs(float(side_fov) - float(target_fov)) if is_number(side_fov) and is_number(target_fov) else None
    near_delta = abs(float(side_near) - float(target_near)) if is_number(side_near) and is_number(target_near) else None
    far_delta = abs(float(side_far) - float(target_far)) if is_number(side_far) and is_number(target_far) else None

    return {
        "positionDeltaEuclidean": clean_number(euclidean_delta(side_position, target_position)),
        "forwardDot": clean_number(dot(side_forward, target_forward)),
        "upDot": clean_number(dot(side_up, target_up)),
        "fovDeltaDegrees": clean_number(fov_delta),
        "nearDelta": clean_number(near_delta),
        "farDelta": clean_number(far_delta),
    }


def build_checks(side_name: str, side: dict[str, Any], target: dict[str, Any], metrics: dict[str, Any]) -> list[dict[str, Any]]:
    side_fov = require_dict(side.get("fov"), f"{side_name}.fov")
    target_fov = require_dict(target.get("fov"), "targetObserver.fov")
    side_resolution = require_dict(side.get("resolution"), f"{side_name}.resolution")
    target_resolution = require_dict(target.get("resolution"), "targetObserver.resolution")
    side_projection = require_dict(side.get("projection"), f"{side_name}.projection")
    target_projection = require_dict(target.get("projection"), "targetObserver.projection")
    side_coordinate = require_dict(side.get("coordinate_system"), f"{side_name}.coordinate_system")
    target_coordinate = require_dict(target.get("coordinate_system"), "targetObserver.coordinate_system")
    side_pixel = require_dict(side.get("pixelSampling"), f"{side_name}.pixelSampling")
    target_pixel = require_dict(target.get("pixelSampling"), "targetObserver.pixelSampling")
    side_origin = require_dict(side.get("imageOrigin"), f"{side_name}.imageOrigin")
    target_origin = require_dict(target.get("imageOrigin"), "targetObserver.imageOrigin")
    side_aspect = require_dict(side.get("aspectPolicy"), f"{side_name}.aspectPolicy")
    target_aspect = require_dict(target.get("aspectPolicy"), "targetObserver.aspectPolicy")

    return [
        check(side_name, "position", vector_status(side.get("position"), target.get("position")), side.get("position"), target.get("position"), metrics),
        check(side_name, "forward", vector_status(side.get("forward"), target.get("forward")), side.get("forward"), target.get("forward"), metrics),
        check(side_name, "up", vector_status(side.get("up"), target.get("up")), side.get("up"), target.get("up"), metrics),
        check(side_name, "fov_degrees", scalar_status(side_fov.get("degrees"), target_fov.get("degrees")), side_fov.get("degrees"), target_fov.get("degrees"), metrics),
        check(side_name, "fov_axis", scalar_status(side_fov.get("axis"), target_fov.get("axis")), side_fov.get("axis"), target_fov.get("axis"), metrics),
        check(side_name, "resolution", resolution_status(side_resolution, target_resolution), side_resolution, target_resolution, metrics),
        check(side_name, "projection_type", scalar_status(side_projection.get("type"), target_projection.get("type")), side_projection.get("type"), target_projection.get("type"), metrics),
        check(side_name, "near", scalar_status(side.get("near"), target.get("near")), side.get("near"), target.get("near"), metrics),
        check(side_name, "far", scalar_status(side.get("far"), target.get("far")), side.get("far"), target.get("far"), metrics),
        check(side_name, "coordinate_handedness", coordinate_handedness_status(side_coordinate, target_coordinate), side_coordinate.get("handedness"), target_coordinate.get("handedness"), metrics),
        check(side_name, "pixel_sampling", pixel_sampling_status(side_pixel, target_pixel), side_pixel, target_pixel, metrics),
        check(side_name, "image_origin", image_origin_status(side_origin, target_origin), side_origin, target_origin, metrics),
        check(side_name, "aspect_policy", aspect_policy_status(side_aspect, target_aspect), side_aspect, target_aspect, metrics),
    ]


def summary_counts(checks: list[dict[str, Any]]) -> dict[str, int]:
    summary = {status: 0 for status in STATUSES}
    for item in checks:
        summary[item["status"]] += 1
    return summary


def pixel_ready(checks: list[dict[str, Any]]) -> bool:
    return all(item["status"] == "MATCH" for item in checks)


def blocking_deltas(checks: list[dict[str, Any]]) -> list[str]:
    return [item["reason"] for item in checks if item["status"] != "MATCH"]


def build_side_packet(
    side_name: str,
    side: dict[str, Any],
    target: dict[str, Any],
    recommended_actions: list[str],
) -> dict[str, Any]:
    metrics = build_metrics(side, target)
    checks = build_checks(side_name, side, target, metrics)
    return {
        "summary": summary_counts(checks),
        "pixelComparisonReady": pixel_ready(checks),
        "metrics": metrics,
        "checks": checks,
        "blockingDeltas": blocking_deltas(checks),
        "recommendedActions": recommended_actions,
    }


def build_packet(generated: str, instances: dict[str, Any], target_packet: dict[str, Any]) -> dict[str, Any]:
    core = require_dict(instances.get("coreObserver"), "coreObserver")
    godot = require_dict(instances.get("godotObserver"), "godotObserver")
    target = require_dict(target_packet.get("targetObserver"), "targetObserver")

    core_vs_target = build_side_packet("Core", core, target, CORE_RECOMMENDED_ACTIONS)
    godot_vs_target = build_side_packet("Godot", godot, target, GODOT_RECOMMENDED_ACTIONS)
    ready = core_vs_target["pixelComparisonReady"] and godot_vs_target["pixelComparisonReady"]

    return {
        "schema": "xprimeray.glowing_heart.observer_target_alignment.v1.8.3",
        "generatedUtc": generated,
        "runtimeExecuted": False,
        "parityClaim": "NONE",
        "target": TARGET.as_posix(),
        "pixelComparisonReady": ready,
        "coreVsTarget": core_vs_target,
        "godotVsTarget": godot_vs_target,
        "globalNextActions": GLOBAL_NEXT_ACTIONS,
    }


def validate_context(instances: dict[str, Any], reconciliation: dict[str, Any], target_packet: dict[str, Any], contract: dict[str, Any]) -> None:
    if instances.get("parityClaim") != "NONE":
        raise AlignmentError(f"{OBSERVER_INSTANCES}: parityClaim must remain NONE")
    if reconciliation.get("parityClaim") != "NONE":
        raise AlignmentError(f"{RECONCILIATION}: parityClaim must remain NONE")
    if target_packet.get("parityClaim") != "NONE":
        raise AlignmentError(f"{TARGET}: parityClaim must remain NONE")
    if target_packet.get("runtimeExecuted") is not False:
        raise AlignmentError(f"{TARGET}: runtimeExecuted must remain false")
    if contract.get("title") != "Project Glowing Heart Shared Observer Contract Preview":
        raise AlignmentError(f"{CONTRACT}: unexpected contract title")


def format_value(value: Any) -> str:
    if value is None:
        return "null"
    if isinstance(value, (dict, list)):
        return json.dumps(value)
    return str(value)


def side_markdown(title: str, side_packet: dict[str, Any]) -> str:
    summary = side_packet["summary"]
    metrics = side_packet["metrics"]
    checks = side_packet["checks"]
    blocking = "\n".join(f"- {item}" for item in side_packet["blockingDeltas"])
    actions = "\n".join(f"- {item}" for item in side_packet["recommendedActions"])
    check_rows = "\n".join(
        f"| {item['category']} | {item['status']} | {item['reason']} |" for item in checks
    )
    metric_rows = "\n".join(f"| {key} | {format_value(value)} |" for key, value in metrics.items())

    return f"""## {title}

Pixel comparison ready: {str(side_packet["pixelComparisonReady"]).lower()}

| Status | Count |
|---|---:|
| MATCH | {summary["MATCH"]} |
| MISMATCH | {summary["MISMATCH"]} |
| PARTIAL | {summary["PARTIAL"]} |
| UNKNOWN | {summary["UNKNOWN"]} |

### Metrics

| Metric | Value |
|---|---|
{metric_rows}

### Checks

| Category | Status | Reason |
|---|---|---|
{check_rows}

### Blocking Deltas

{blocking}

### Recommended Actions

{actions}
"""


def render_markdown(packet: dict[str, Any]) -> str:
    global_actions = "\n".join(f"- {item}" for item in packet["globalNextActions"])
    return f"""# Project Glowing Heart Observer Target Alignment (Preview)

Generated: {packet["generatedUtc"]}

Runtime executed: false

Parity claim: NONE

Shared target: {packet["target"]}

Pixel comparison ready: {str(packet["pixelComparisonReady"]).lower()}

{side_markdown("Core vs Shared Target", packet["coreVsTarget"])}

{side_markdown("Godot vs Shared Target", packet["godotVsTarget"])}

## Global Next Actions

{global_actions}

## Bottom Line

Core and Godot are not both aligned to the shared observer target. Pixel comparison remains blocked.
"""


def write_json(path: Path, value: dict[str, Any]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")


def write_markdown(path: Path, value: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(value, encoding="utf-8")


def main() -> int:
    try:
        require_inputs([OBSERVER_INSTANCES, RECONCILIATION, TARGET, CONTRACT])
        instances = load_json_object(OBSERVER_INSTANCES)
        reconciliation = load_json_object(RECONCILIATION)
        target_packet = load_json_object(TARGET)
        contract = load_json_object(CONTRACT)
        validate_context(instances, reconciliation, target_packet, contract)

        packet = build_packet(utc_now(), instances, target_packet)
        write_json(OUTPUT_JSON, packet)
        write_markdown(OUTPUT_MD, render_markdown(packet))
    except AlignmentError as exc:
        print(f"[glowing-heart-observer-target-alignment] ERROR: {exc}")
        return 1

    print("[glowing-heart-observer-target-alignment]")
    print(f"target={packet['target']}")
    print(f"core_pixel_comparison_ready={str(packet['coreVsTarget']['pixelComparisonReady']).lower()}")
    print(f"godot_pixel_comparison_ready={str(packet['godotVsTarget']['pixelComparisonReady']).lower()}")
    print(f"pixel_comparison_ready={str(packet['pixelComparisonReady']).lower()}")
    print(f"runtime_executed={str(packet['runtimeExecuted']).lower()}")
    print(f"parity_claim={packet['parityClaim']}")
    print()
    print(f"wrote={OUTPUT_JSON.as_posix()}")
    print(f"wrote={OUTPUT_MD.as_posix()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
