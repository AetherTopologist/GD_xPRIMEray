#!/usr/bin/env python3
"""Generate the v1.8 observer instance packet.

This is static metadata extraction only. It does not execute Godot, modify
scenes, modify Core transport, touch renderer lifecycle files, or claim parity.
"""

from __future__ import annotations

import json
import re
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


CONTRACT = Path("schemas/glowing_heart/shared_observer_contract.v0.preview.json")
CORE_FIXTURE = Path("Fixtures/grin_radial_smoke.json")
SHARED_INSTANCE_CANDIDATES = [
    Path("Fixtures/shared/glowing_heart_grin_bridge.v0.preview.json"),
    Path("Fixtures/shared/glowing_heart_grin_bridge.v0.preview.json"),
]
GODOT_EXPORT = Path("reports/glowing_heart_godot_fixture_export.preview.json")
SHARED_CANDIDATE = Path("reports/glowing_heart_shared_fixture_candidate.preview.json")
OUTPUT_JSON = Path("reports/glowing_heart_observer_instances.preview.json")
OUTPUT_MD = Path("reports/glowing_heart_observer_instances.preview.md")
DEFAULT_GODOT_FIXTURE = Path("Fixtures/fixture_hermetic_observatory_grin.tscn")


class ObserverPacketError(Exception):
    pass


def utc_now() -> str:
    return datetime.now(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")


def load_json_object(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:
        raise ObserverPacketError(f"{path}: failed to load JSON: {exc}") from exc
    if not isinstance(value, dict):
        raise ObserverPacketError(f"{path}: expected JSON object")
    return value


def require_inputs(paths: list[Path]) -> None:
    for path in paths:
        if not path.is_file():
            raise ObserverPacketError(f"{path}: required input not found")


def first_existing(paths: list[Path], label: str) -> Path:
    for path in paths:
        if path.is_file():
            return path
    choices = ", ".join(path.as_posix() for path in paths)
    raise ObserverPacketError(f"{label}: required input not found; checked {choices}")


def require_dict(value: Any, label: str) -> dict[str, Any]:
    if not isinstance(value, dict):
        raise ObserverPacketError(f"{label}: expected object")
    return value


def require_vec3(value: Any, label: str) -> list[int | float]:
    if (
        not isinstance(value, list)
        or len(value) != 3
        or any(not isinstance(item, (int, float)) or isinstance(item, bool) for item in value)
    ):
        raise ObserverPacketError(f"{label}: expected numeric vec3")
    return value


def selected_godot_fixture(candidate: dict[str, Any]) -> Path:
    godot_candidate = candidate.get("godotCandidate")
    if isinstance(godot_candidate, dict) and isinstance(godot_candidate.get("path"), str):
        return Path(godot_candidate["path"])
    return DEFAULT_GODOT_FIXTURE


def parse_node_header(line: str) -> dict[str, str] | None:
    match = re.fullmatch(r'\[node name="([^"]+)" type="([^"]+)"(?: parent="([^"]+)")?.*\]', line.strip())
    if not match:
        return None
    return {
        "name": match.group(1),
        "type": match.group(2),
        "parent": match.group(3) or "",
    }


def parse_float_property(lines: list[str], key: str) -> float | None:
    pattern = re.compile(rf"^{re.escape(key)}\s*=\s*(-?\d+(?:\.\d+)?)\s*$")
    for line in lines:
        match = pattern.match(line.strip())
        if match:
            value = float(match.group(1))
            return int(value) if value.is_integer() else value
    return None


def parse_transform_position(lines: list[str]) -> list[float] | None:
    pattern = re.compile(r"^transform\s*=\s*Transform3D\(([^)]*)\)\s*$")
    for line in lines:
        match = pattern.match(line.strip())
        if not match:
            continue
        parts = [part.strip() for part in match.group(1).split(",")]
        if len(parts) != 12:
            continue
        try:
            return [float(parts[9]), float(parts[10]), float(parts[11])]
        except ValueError:
            return None
    return None


def camera_block(path: Path) -> tuple[dict[str, str], list[str]]:
    lines = path.read_text(encoding="utf-8").splitlines()
    current_node: dict[str, str] | None = None
    current_lines: list[str] = []

    for line in lines:
        node = parse_node_header(line)
        if node is not None:
            if current_node and current_node.get("type") == "Camera3D":
                return current_node, current_lines
            current_node = node
            current_lines = []
            continue
        if current_node is not None:
            current_lines.append(line)

    if current_node and current_node.get("type") == "Camera3D":
        return current_node, current_lines

    raise ObserverPacketError(f"{path}: no Camera3D node found in static tscn")


def build_core_observer(core_fixture: dict[str, Any]) -> dict[str, Any]:
    observer = require_dict(core_fixture.get("observer"), f"{CORE_FIXTURE}:observer")
    ray_grid = require_dict(core_fixture.get("rayGrid"), f"{CORE_FIXTURE}:rayGrid")
    position = require_vec3(observer.get("origin"), f"{CORE_FIXTURE}:observer.origin")
    forward = require_vec3(observer.get("forward"), f"{CORE_FIXTURE}:observer.forward")
    up = require_vec3(observer.get("up"), f"{CORE_FIXTURE}:observer.up")
    width = ray_grid.get("width")
    height = ray_grid.get("height")
    fov_degrees = observer.get("fovDegrees")

    if not isinstance(width, int) or not isinstance(height, int):
        raise ObserverPacketError(f"{CORE_FIXTURE}: rayGrid width/height must be integers")
    if not isinstance(fov_degrees, (int, float)) or isinstance(fov_degrees, bool):
        raise ObserverPacketError(f"{CORE_FIXTURE}: observer.fovDegrees must be numeric")

    return {
        "source": "core_fixture",
        "fixturePath": CORE_FIXTURE.as_posix(),
        "position": position,
        "forward": forward,
        "up": up,
        "fov": {
            "degrees": fov_degrees,
            "axis": "vertical",
            "basis": "inferred_from_core_transport_runner",
            "inferred": True,
        },
        "resolution": {
            "width": width,
            "height": height,
            "source": "rayGrid",
        },
        "projection": {
            "type": "perspective",
            "orthographic_size": None,
            "inferred": True,
        },
        "near": None,
        "far": None,
        "coordinate_system": {
            "source": "core_fixture",
            "handedness": "right_handed_inferred",
            "world_up_axis": "+Y",
            "forward_axis": "explicit_vector",
            "units": "scene_units",
            "notes": "Core fixture uses explicit forward/up vectors. Handedness inferred from System.Numerics vector math and TransportRunner behavior.",
            "inferred": True,
        },
        "pixelSampling": {
            "samplePosition": "center",
            "xConvention": "x_plus_0_5",
            "yConvention": "y_plus_0_5_then_ndc_y_flip",
            "source": "TransportRunner",
            "inferred": True,
        },
        "aspectPolicy": {
            "source": "TransportRunner",
            "policy": "horizontal_scaled_by_width_over_height",
            "inferred": True,
        },
        "imageOrigin": {
            "source": "TransportRunner",
            "origin": "top_left",
            "rowOrder": "top_to_bottom",
            "inferred": True,
        },
        "confidence": "HIGH",
        "extractionMethod": "json_fixture_plus_code_convention",
    }


def build_godot_observer(godot_fixture: Path) -> dict[str, Any]:
    node, lines = camera_block(godot_fixture)
    fov = parse_float_property(lines, "fov")
    near = parse_float_property(lines, "near")
    far = parse_float_property(lines, "far")
    transform_position = parse_transform_position(lines)
    position_inferred = transform_position is None

    return {
        "source": "godot_static_tscn",
        "fixturePath": godot_fixture.as_posix(),
        "cameraNode": {
            "name": node["name"],
            "type": node["type"],
            "parent": node["parent"],
        },
        "position": transform_position if transform_position is not None else [0, 0, 0],
        "positionInferred": position_inferred,
        "forward": [0, 0, -1],
        "forwardInferred": True,
        "up": [0, 1, 0],
        "upInferred": True,
        "fov": {
            "degrees": fov,
            "axis": "vertical",
            "basis": "Godot Camera3D default/tscn property" if fov is not None else "unknown_static_tscn",
            "inferred": fov is None,
        },
        "resolution": {
            "width": None,
            "height": None,
            "source": "unknown_static_tscn",
        },
        "projection": {
            "type": "perspective",
            "orthographic_size": None,
            "inferred": True,
        },
        "near": near,
        "far": far,
        "coordinate_system": {
            "source": "godot_static_tscn",
            "handedness": "right_handed_godot_3d",
            "world_up_axis": "+Y",
            "forward_axis": "-Z_camera_forward",
            "units": "godot_scene_units",
            "notes": "Static inference only; scene was not instantiated.",
            "inferred": True,
        },
        "pixelSampling": {
            "samplePosition": "unknown",
            "source": "static_tscn_unavailable",
        },
        "aspectPolicy": {
            "source": "static_tscn_unavailable",
            "policy": "unknown",
        },
        "imageOrigin": {
            "source": "static_tscn_unavailable",
            "origin": "unknown",
            "rowOrder": "unknown",
        },
        "confidence": "MEDIUM",
        "extractionMethod": "static_tscn_text_scan_plus_godot_defaults",
    }


def build_packet(generated: str, core_observer: dict[str, Any], godot_observer: dict[str, Any]) -> dict[str, Any]:
    return {
        "schema": "xprimeray.glowing_heart.observer_instances.v1.8",
        "generatedUtc": generated,
        "runtimeExecuted": False,
        "parityClaim": "NONE",
        "contract": CONTRACT.as_posix(),
        "coreObserver": core_observer,
        "godotObserver": godot_observer,
        "notes": [
            "Observer instances are side-by-side only; they are not reconciled.",
            "Core FOV axis and ray conventions are inferred from TransportRunner behavior.",
            "Godot pose vectors are inferred from Camera3D defaults because no camera transform is explicit in the static .tscn.",
            "Godot resolution, pixel sampling, aspect policy, and image origin are not available from static .tscn text.",
        ],
        "limitations": [
            "Godot runtime was not executed.",
            "Godot scene graph was not instantiated.",
            "No parity claim.",
            "No pixel comparison.",
            "No renderer equivalence claim.",
            "No transport equivalence claim.",
        ],
    }


def fmt_vec(value: list[Any]) -> str:
    return "[" + ", ".join(str(item) for item in value) + "]"


def render_markdown(packet: dict[str, Any]) -> str:
    core = packet["coreObserver"]
    godot = packet["godotObserver"]
    core_fov = core["fov"]
    godot_fov = godot["fov"]
    core_resolution = core["resolution"]
    godot_resolution = godot["resolution"]
    godot_position_suffix = " inferred/default" if godot.get("positionInferred") else ""
    godot_forward_suffix = " inferred/default" if godot.get("forwardInferred") else ""
    godot_up_suffix = " inferred/default" if godot.get("upInferred") else ""
    godot_resolution_value = (
        f"{godot_resolution['width']}x{godot_resolution['height']}"
        if godot_resolution.get("width") and godot_resolution.get("height")
        else "unknown"
    )

    notes = "\n".join(f"- {note}" for note in packet["notes"])
    limitations = "\n".join(f"- {item}" for item in packet["limitations"])

    return f"""# Project Glowing Heart Observer Instances (Preview)

Generated: {packet["generatedUtc"]}

Runtime executed: false

Parity claim: NONE

## Core Observer

| Field | Value |
|---|---|
| Fixture | {core["fixturePath"]} |
| Position | {fmt_vec(core["position"])} |
| Forward | {fmt_vec(core["forward"])} |
| Up | {fmt_vec(core["up"])} |
| FOV | {core_fov["degrees"]} {core_fov["axis"]} |
| Resolution | {core_resolution["width"]}x{core_resolution["height"]} |
| Projection | {core["projection"]["type"]} |
| Confidence | {core["confidence"]} |

## Godot Observer

| Field | Value |
|---|---|
| Fixture | {godot["fixturePath"]} |
| Camera Node | {godot["cameraNode"]["name"]} |
| Position | {fmt_vec(godot["position"])}{godot_position_suffix} |
| Forward | {fmt_vec(godot["forward"])}{godot_forward_suffix} |
| Up | {fmt_vec(godot["up"])}{godot_up_suffix} |
| FOV | {godot_fov["degrees"]} {godot_fov["axis"]} |
| Resolution | {godot_resolution_value} |
| Projection | {godot["projection"]["type"]} |
| Near | {godot["near"]} |
| Far | {godot["far"]} |
| Confidence | {godot["confidence"]} |

## Important Notes

- These observer instances are not reconciled.
- Pixel comparison is not ready.
- Godot runtime was not executed.
- Static `.tscn` data may omit runtime transforms inherited from parents.
- No parity claim is made.

## Extraction Notes

{notes}

## Limitations

{limitations}

## Next Milestone

v1.8.1 should reconcile these two observer instances and compute pose/FOV/resolution readiness.
"""


def main() -> int:
    try:
        require_inputs([CONTRACT, CORE_FIXTURE, GODOT_EXPORT, SHARED_CANDIDATE])
        _shared_instance_path = first_existing(SHARED_INSTANCE_CANDIDATES, "shared fixture instance")
        core_fixture = load_json_object(CORE_FIXTURE)
        shared_candidate = load_json_object(SHARED_CANDIDATE)
        _ = load_json_object(GODOT_EXPORT)
        godot_fixture = selected_godot_fixture(shared_candidate)
        require_inputs([godot_fixture])

        core_observer = build_core_observer(core_fixture)
        godot_observer = build_godot_observer(godot_fixture)
        packet = build_packet(utc_now(), core_observer, godot_observer)

        OUTPUT_JSON.parent.mkdir(parents=True, exist_ok=True)
        OUTPUT_MD.parent.mkdir(parents=True, exist_ok=True)
        OUTPUT_JSON.write_text(json.dumps(packet, indent=2) + "\n", encoding="utf-8")
        OUTPUT_MD.write_text(render_markdown(packet), encoding="utf-8")
    except ObserverPacketError as exc:
        print(f"[glowing-heart-observer-instances] ERROR: {exc}")
        return 1

    print("[glowing-heart-observer-instances]")
    print(f"core_fixture={core_observer['fixturePath']}")
    print(f"godot_fixture={godot_observer['fixturePath']}")
    print(f"core_confidence={core_observer['confidence']}")
    print(f"godot_confidence={godot_observer['confidence']}")
    print(f"runtime_executed={str(packet['runtimeExecuted']).lower()}")
    print(f"parity_claim={packet['parityClaim']}")
    print()
    print(f"wrote={OUTPUT_JSON.as_posix()}")
    print(f"wrote={OUTPUT_MD.as_posix()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
