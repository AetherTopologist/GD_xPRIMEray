# Project Glowing Heart Observer Instances (Preview)

Generated: 2026-06-24T03:20:56Z

Runtime executed: false

Parity claim: NONE

## Core Observer

| Field | Value |
|---|---|
| Fixture | fixtures/grin_radial_smoke.json |
| Position | [0, 0, -2] |
| Forward | [0, 0, 1] |
| Up | [0, 1, 0] |
| FOV | 60 vertical |
| Resolution | 40x22 |
| Projection | perspective |
| Confidence | HIGH |

## Godot Observer

| Field | Value |
|---|---|
| Fixture | Fixtures/fixture_hermetic_observatory_grin.tscn |
| Camera Node | Camera3D |
| Position | [0, 0, 0] inferred/default |
| Forward | [0, 0, -1] inferred/default |
| Up | [0, 1, 0] inferred/default |
| FOV | 75 vertical |
| Resolution | unknown |
| Projection | perspective |
| Near | 0.01 |
| Far | 40 |
| Confidence | MEDIUM |

## Important Notes

- These observer instances are not reconciled.
- Pixel comparison is not ready.
- Godot runtime was not executed.
- Static `.tscn` data may omit runtime transforms inherited from parents.
- No parity claim is made.

## Extraction Notes

- Observer instances are side-by-side only; they are not reconciled.
- Core FOV axis and ray conventions are inferred from TransportRunner behavior.
- Godot pose vectors are inferred from Camera3D defaults because no camera transform is explicit in the static .tscn.
- Godot resolution, pixel sampling, aspect policy, and image origin are not available from static .tscn text.

## Limitations

- Godot runtime was not executed.
- Godot scene graph was not instantiated.
- No parity claim.
- No pixel comparison.
- No renderer equivalence claim.
- No transport equivalence claim.

## Next Milestone

v1.8.1 should reconcile these two observer instances and compute pose/FOV/resolution readiness.
