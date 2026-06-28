#!/usr/bin/env python3
"""Generate the v1.8.2 shared observer target preview.

This is a target contract artifact only. It does not execute Godot, modify
scenes, modify Core transport, touch renderer lifecycle files, or claim parity.
"""

from __future__ import annotations

import json
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


CONTRACT = Path("schemas/glowing_heart/shared_observer_contract.v0.preview.json")
OBSERVER_INSTANCES = Path("reports/glowing_heart_observer_instances.preview.json")
RECONCILIATION = Path("reports/glowing_heart_observer_reconciliation.preview.json")
CORE_FIXTURE = Path("fixtures/grin_radial_smoke.json")
BRIDGE = Path("fixtures/shared/glowing_heart_grin_bridge.v0.preview.json")
OUTPUT_JSON = Path("fixtures/shared/glowing_heart_observer_target.v0.preview.json")
OUTPUT_MD = Path("reports/glowing_heart_shared_observer_target.preview.md")


class TargetError(Exception):
    pass


def utc_now() -> str:
    return datetime.now(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")


def load_json_object(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:
        raise TargetError(f"{path}: failed to load JSON: {exc}") from exc
    if not isinstance(value, dict):
        raise TargetError(f"{path}: expected JSON object")
    return value


def require_inputs(paths: list[Path]) -> None:
    for path in paths:
        if not path.is_file():
            raise TargetError(f"{path}: required input not found")


def require_dict(value: Any, label: str) -> dict[str, Any]:
    if not isinstance(value, dict):
        raise TargetError(f"{label}: expected object")
    return value


def require_vec3(value: Any, label: str) -> list[int | float]:
    if (
        not isinstance(value, list)
        or len(value) != 3
        or any(not isinstance(item, (int, float)) or isinstance(item, bool) for item in value)
    ):
        raise TargetError(f"{label}: expected numeric vec3")
    return value


def require_number(value: Any, label: str) -> int | float:
    if not isinstance(value, (int, float)) or isinstance(value, bool):
        raise TargetError(f"{label}: expected number")
    return value


def require_int(value: Any, label: str) -> int:
    if not isinstance(value, int) or isinstance(value, bool):
        raise TargetError(f"{label}: expected integer")
    return value


def assert_equal(actual: Any, expected: Any, label: str) -> None:
    if actual != expected:
        raise TargetError(f"{label}: expected {expected!r}, found {actual!r}")


def build_target_observer(
    core_fixture: dict[str, Any],
    observer_instances: dict[str, Any],
    bridge: dict[str, Any],
) -> dict[str, Any]:
    observer = require_dict(core_fixture.get("observer"), f"{CORE_FIXTURE}:observer")
    ray_grid = require_dict(core_fixture.get("rayGrid"), f"{CORE_FIXTURE}:rayGrid")
    bridge_observer = require_dict(bridge.get("observer"), f"{BRIDGE}:observer")
    godot_observer = require_dict(observer_instances.get("godotObserver"), f"{OBSERVER_INSTANCES}:godotObserver")

    position = require_vec3(observer.get("origin"), f"{CORE_FIXTURE}:observer.origin")
    forward = require_vec3(observer.get("forward"), f"{CORE_FIXTURE}:observer.forward")
    up = require_vec3(observer.get("up"), f"{CORE_FIXTURE}:observer.up")
    fov_degrees = require_number(observer.get("fovDegrees"), f"{CORE_FIXTURE}:observer.fovDegrees")
    width = require_int(ray_grid.get("width"), f"{CORE_FIXTURE}:rayGrid.width")
    height = require_int(ray_grid.get("height"), f"{CORE_FIXTURE}:rayGrid.height")
    godot_far = require_number(godot_observer.get("far"), f"{OBSERVER_INSTANCES}:godotObserver.far")
    godot_near = require_number(godot_observer.get("near"), f"{OBSERVER_INSTANCES}:godotObserver.near")

    assert_equal(position, [0, 0, -2], "selected Core observer position")
    assert_equal(forward, [0, 0, 1], "selected Core observer forward")
    assert_equal(up, [0, 1, 0], "selected Core observer up")
    assert_equal(fov_degrees, 60, "selected Core observer fovDegrees")
    assert_equal(width, 40, "selected Core rayGrid.width")
    assert_equal(height, 22, "selected Core rayGrid.height")
    assert_equal(godot_near, 0.01, "selected Godot candidate near")
    assert_equal(godot_far, 40, "selected Godot candidate far")
    assert_equal(bridge_observer.get("source"), "core", "bridge observer source")

    return {
        "position": position,
        "forward": forward,
        "up": up,
        "fov": {
            "degrees": fov_degrees,
            "axis": "vertical",
        },
        "resolution": {
            "width": width,
            "height": height,
        },
        "projection": {
            "type": "perspective",
            "orthographic_size": None,
        },
        "near": godot_near,
        "far": godot_far,
        "coordinate_system": {
            "handedness": "right_handed",
            "world_up_axis": "+Y",
            "forward_axis": "explicit_vector",
            "units": "scene_units",
        },
        "right_vector": {
            "derivation": "cross_up_forward",
            "explicit": None,
            "reorthogonalize_up": True,
            "notes": "Matches current Core TransportRunner convention; Godot adapter transform still requires review.",
        },
        "pixelSampling": {
            "samplePosition": "center",
            "xConvention": "x_plus_0_5",
            "yConvention": "y_plus_0_5_then_ndc_y_flip",
        },
        "imageOrigin": {
            "origin": "top_left",
            "rowOrder": "top_to_bottom",
        },
        "aspectPolicy": {
            "policy": "horizontal_scaled_by_width_over_height",
        },
        "pixel_aspect_ratio": 1.0,
        "snapshot_channel": {
            "type": "bend_magnitude_metric",
            "comparisonReady": False,
            "notes": "Core emits bend-magnitude metric snapshots. Godot rendered channel is not yet normalized; v1.9 will define shared snapshot/channel semantics.",
        },
        "contractFieldMapping": {
            "aspectPolicy": "aspect_policy",
            "pixelSampling": "pixel_sampling",
            "imageOrigin": "image_origin",
            "right_vector": "right_vector",
            "pixel_aspect_ratio": "pixel_aspect_ratio",
            "snapshot_channel": "snapshot_channel",
        },
    }


def build_packet(generated: str, target_observer: dict[str, Any]) -> dict[str, Any]:
    return {
        "schema": "xprimeray.glowing_heart.shared_observer_target.v1.8.2",
        "generatedUtc": generated,
        "runtimeExecuted": False,
        "parityClaim": "NONE",
        "contract": CONTRACT.as_posix(),
        "targetName": "glowing_heart_grin_bridge_observer_target",
        "basis": {
            "selectedFrom": "core_observer_with_godot_far_clip",
            "reason": "Core observer is explicit; Godot candidate far clip is adopted to reduce one known mismatch.",
        },
        "targetObserver": target_observer,
        "alignmentIntent": {
            "core": "Core should make near/far explicit and confirm current observer matches this target.",
            "godot": "Godot candidate should move or configure Camera3D to this pose/FOV/resolution before pixel comparison.",
            "pixelComparison": "Not ready until Core and Godot emit observer instances matching this target.",
        },
        "limitations": [
            "Target only",
            "Godot runtime was not executed",
            "No scene was modified",
            "No parity claim",
            "No pixel comparison",
        ],
    }


def validate_context(contract: dict[str, Any], reconciliation: dict[str, Any]) -> None:
    assert_equal(
        contract.get("description"),
        "Preview contract for describing one observer/camera vocabulary across xPRIMEray-Core and GD_xPRIMEray. This is the observer normalization prerequisite for future pixel comparison and difference.ppm work; it is not a parity claim.",
        f"{CONTRACT}:description",
    )
    assert_equal(reconciliation.get("pixelComparisonReady"), False, f"{RECONCILIATION}:pixelComparisonReady")
    assert_equal(reconciliation.get("parityClaim"), "NONE", f"{RECONCILIATION}:parityClaim")


def format_vec(value: list[Any]) -> str:
    return "[" + ", ".join(str(item) for item in value) + "]"


def render_markdown(packet: dict[str, Any]) -> str:
    observer = packet["targetObserver"]
    fov = observer["fov"]
    resolution = observer["resolution"]
    projection = observer["projection"]
    pixel_sampling = observer["pixelSampling"]
    image_origin = observer["imageOrigin"]
    aspect_policy = observer["aspectPolicy"]

    return f"""# Project Glowing Heart Shared Observer Target (Preview)

Generated: {packet["generatedUtc"]}

Runtime executed: false

Parity claim: NONE

## Target Observer

| Field | Value |
|---|---|
| Position | {format_vec(observer["position"])} |
| Forward | {format_vec(observer["forward"])} |
| Up | {format_vec(observer["up"])} |
| FOV | {fov["degrees"]} {fov["axis"]} |
| Resolution | {resolution["width"]}x{resolution["height"]} |
| Projection | {projection["type"]} |
| Near | {observer["near"]} |
| Far | {observer["far"]} |
| Right Vector | {observer["right_vector"]["derivation"]} |
| Pixel Sampling | {pixel_sampling["samplePosition"]} |
| Image Origin | {image_origin["origin"]} |
| Aspect Policy | {aspect_policy["policy"]} |
| Pixel Aspect Ratio | {observer["pixel_aspect_ratio"]} |
| Snapshot Channel | {observer["snapshot_channel"]["type"]} (comparison ready: false) |

## Contract Field Mapping

The target retains the existing camelCase bridge keys `aspectPolicy`, `pixelSampling`, and `imageOrigin` for v1.8.3 compatibility. `contractFieldMapping` documents their snake_case observer contract equivalents alongside the new snake_case fields.

## Why This Target

The Core observer is explicit and already produces the standalone bend-magnitude snapshot. The Godot candidate currently exposes far=40, so this target adopts far=40 to remove one avoidable future mismatch.

## What Must Change Later

- Core must explicitly declare near/far in fixture or shared instance.
- Godot candidate must expose or configure camera pose to [0,0,-2] looking +Z.
- Godot output must declare resolution 40x22 or another agreed resolution.
- Godot sampling/image origin/aspect policy must be known.
- Only then can pixel comparison become meaningful.

## Status

Pixel comparison ready: false

Parity claim: NONE
"""


def write_json(path: Path, value: dict[str, Any]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")


def write_markdown(path: Path, value: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(value, encoding="utf-8")


def main() -> int:
    try:
        require_inputs([CONTRACT, OBSERVER_INSTANCES, RECONCILIATION, CORE_FIXTURE, BRIDGE])
        contract = load_json_object(CONTRACT)
        observer_instances = load_json_object(OBSERVER_INSTANCES)
        reconciliation = load_json_object(RECONCILIATION)
        core_fixture = load_json_object(CORE_FIXTURE)
        bridge = load_json_object(BRIDGE)
        validate_context(contract, reconciliation)

        target_observer = build_target_observer(core_fixture, observer_instances, bridge)
        packet = build_packet(utc_now(), target_observer)
        write_json(OUTPUT_JSON, packet)
        write_markdown(OUTPUT_MD, render_markdown(packet))
    except TargetError as exc:
        print(f"[glowing-heart-shared-observer-target] ERROR: {exc}")
        return 1

    print("[glowing-heart-shared-observer-target]")
    print(f"target={OUTPUT_JSON.as_posix()}")
    print(f"basis={packet['basis']['selectedFrom']}")
    print(f"runtime_executed={str(packet['runtimeExecuted']).lower()}")
    print(f"parity_claim={packet['parityClaim']}")
    print("pixel_comparison_ready=false")
    print()
    print(f"wrote={OUTPUT_JSON.as_posix()}")
    print(f"wrote={OUTPUT_MD.as_posix()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
