# Project Glowing Heart Shared Observer Target (Preview)

Generated: 2026-06-27T20:37:04Z

Runtime executed: false

Parity claim: NONE

## Target Observer

| Field | Value |
|---|---|
| Position | [0, 0, -2] |
| Forward | [0, 0, 1] |
| Up | [0, 1, 0] |
| FOV | 60 vertical |
| Resolution | 40x22 |
| Projection | perspective |
| Near | 0.01 |
| Far | 40 |
| Right Vector | cross_up_forward |
| Pixel Sampling | center |
| Image Origin | top_left |
| Aspect Policy | horizontal_scaled_by_width_over_height |
| Pixel Aspect Ratio | 1.0 |
| Snapshot Channel | bend_magnitude_metric (comparison ready: false) |

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
