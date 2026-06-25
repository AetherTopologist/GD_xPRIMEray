# Project Glowing Heart Shared Observer Contract (Preview)

Generated: 2026-06-24T01:09:41Z

Parity claim: NONE

Runtime executed: false

## Purpose

This preview contract defines one observer vocabulary that xPRIMEray-Core and GD_xPRIMEray can both eventually emit or consume before pixel comparison.

## Contract Artifact

schemas/glowing_heart/shared_observer_contract.v0.preview.json

## Required Fields

| Field | Purpose |
|---|---|
| coordinate_system | records handedness, basis axes, and units before vector comparison |
| position | records observer/camera world position |
| forward | records normalized forward direction |
| up | records normalized up direction |
| fov | records field-of-view degrees and whether the axis is vertical, horizontal, diagonal, or unknown |
| resolution | records output width and height |
| projection | records perspective, orthographic, or unknown projection model |
| near | records near clip plane distance |
| far | records far clip plane distance |
| right_vector | records right-vector derivation and up re-orthogonalization |
| aspect_policy | records which aspect ratio drives horizontal/vertical frustum scaling |
| pixel_sampling | records center/corner/multi-sample ray construction |
| image_origin | records image origin, row order, and NDC Y direction |
| pixel_aspect_ratio | records pixel width divided by pixel height |
| snapshot_channel | records whether the observer output is a bend metric, rendered intensity, RGB render, depth, or unknown |

## Example Instance

```json
{
  "schema": "xprimeray.glowing_heart.shared_observer_contract.v0.preview",
  "version": "v0.preview",
  "parityClaim": "NONE",
  "runtimeExecuted": false,
  "coordinate_system": {
    "source": "shared",
    "handedness": "right_handed",
    "world_up_axis": "+Y",
    "forward_axis": "vector",
    "vector_space": "shared_world",
    "units": "scene_units",
    "notes": "Shared observer vectors are explicit; engine-native adapter transforms still need source-specific validation."
  },
  "position": [0, 0, -2],
  "forward": [0, 0, 1],
  "up": [0, 1, 0],
  "fov": {
    "degrees": 60,
    "axis": "vertical"
  },
  "resolution": {
    "width": 40,
    "height": 22
  },
  "projection": {
    "type": "perspective",
    "orthographic_size": null,
    "frustum": "symmetric"
  },
  "near": 0.01,
  "far": 1000,
  "right_vector": {
    "derivation": "cross_up_forward",
    "explicit": null,
    "reorthogonalize_up": true,
    "notes": "Matches current Core TransportRunner convention: right = cross(up, forward), then up = cross(forward, right)."
  },
  "aspect_policy": {
    "source": "core",
    "mode": "resolution_width_over_height",
    "value": null,
    "notes": "Core uses width / height when building ray directions."
  },
  "pixel_sampling": {
    "mode": "center",
    "x": 0.5,
    "y": 0.5,
    "notes": "Core currently samples pixel centers with x + 0.5 and y + 0.5."
  },
  "image_origin": {
    "origin": "top_left",
    "row_order": "top_to_bottom",
    "ndc_y": "positive_up",
    "notes": "Core maps increasing image y downward while NDC y remains positive upward."
  },
  "pixel_aspect_ratio": 1.0,
  "snapshot_channel": {
    "type": "bend_magnitude_metric",
    "comparisonReady": false,
    "notes": "Core preview snapshots are metric outputs, not Godot rendered intensity."
  },
  "normalizationNotes": [
    "Confirm Godot camera FOV axis before pixel comparison.",
    "Confirm handedness and forward-axis transforms at the adapter boundary.",
    "Confirm near/far clipping behavior before difference.ppm generation.",
    "Resolve current bridge candidate pose mismatch: Core origin [0, 0, -2] looking +Z vs Godot Camera3D identity looking engine-native -Z.",
    "Resolve current bridge candidate FOV mismatch: Core 60 degrees vs Godot scene 75 degrees.",
    "Resolve current bridge candidate far clip mismatch: example/Core comparison target 1000 vs Godot scene 40."
  ],
  "limitations": [
    "Preview observer vocabulary only",
    "Godot runtime was not executed",
    "No pixel comparison was performed",
    "No parity claim",
    "No validation claim"
  ]
}
```

## Why This Advances difference.ppm

`difference.ppm` needs Core and Godot to describe the same observer before rendered pixels can be meaningfully compared. This contract isolates the observer variables that would otherwise make pixel deltas ambiguous: coordinate basis, camera position, view direction, FOV axis, resolution, projection, clip planes, aspect policy, right-vector derivation, pixel sampling, image origin, and output channel semantics.

## Audit Patch Notes

The v1.7 audit identified several comparison blockers that were only free-text notes in the first draft. This patch moves those blockers into required schema fields:

- Core `origin` maps to contract `position`.
- Core ray construction uses pixel centers.
- Core image rows are top-to-bottom while NDC Y is positive upward.
- Core right-vector derivation is `cross(up, forward)` with up re-orthogonalization.
- Aspect ratio source must be explicit.
- Snapshot channel semantics must be explicit so bend-magnitude metrics are not silently compared with rendered intensity.

The audit also records current bridge candidate mismatches that must be resolved before any `difference.ppm` packet:

- Core pose: position `[0, 0, -2]`, forward `[0, 0, 1]`; Godot Camera3D is currently identity with engine-native default forward.
- Core FOV: 60 degrees; Godot scene FOV: 75 degrees.
- Core/example far plane target: 1000; Godot scene far plane: 40.

## What This Does Not Prove

- No Core/Godot pixel equivalence
- No Godot runtime execution
- No renderer parity
- No physics validation
- No closure proof

## Next Milestone

The next comparison milestone should create Core and Godot observer contract instances for the selected shared fixture candidate, then audit whether both instances can drive a future `difference.ppm` packet without hidden camera assumptions.
