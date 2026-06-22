# Project Glowing Heart Shared Fixture Candidate (Preview)

Generated: 2026-06-21T23:10:03Z

Parity claim: NONE

Runtime executed: false

## Core Fixture

| Field | Value |
|---|---|
| Name | grin_radial_smoke |
| Path | fixtures/grin_radial_smoke.json |
| Mode | radial_grin_smoke |
| Ray Grid | 40x22 |
| Steps per Ray | 32 |
| Step Size | 0.05 |
| Field Type | grin_radial |
| Radius Outer | 1.5 |
| Amplitude | 0.25 |

## Godot Candidate

| Field | Value |
|---|---|
| Name | fixture_hermetic_observatory_grin |
| Path | Fixtures/fixture_hermetic_observatory_grin.tscn |
| Category | READY_CANDIDATE |
| Transport Hint | grin |
| Closure Hint | likely |
| Confidence | HIGH |

## Why This Candidate

Preferred Godot GRIN observatory fixture found in the v0.9 candidate index.

## Shared Concepts

- fixture
- grin
- field-driven bending
- observatory artifact

## Differences

- Core fixture is simplified JSON; Godot fixture is a .tscn scene.
- Core snapshot visualizes bend magnitude; Godot artifacts are renderer/HUD/observatory outputs.
- Core smoke fixture has no closure claim; Godot candidate may involve hermetic closure.
- Core currently has no geometry, collision, portal, or scene tree.

## Normalization Needed Before Parity

- Shared fixture schema
- Shared observer/camera definition
- Shared field parameter mapping
- Shared validation vocabulary
- Shared snapshot metric naming
- Godot scene metadata export path

## Bridge Status

Current state:
SHARED FIXTURE CANDIDATE IDENTIFIED

Verification:
METADATA ONLY

Parity:
NONE

Recommendation:
Proceed to static Godot metadata export for the selected candidate.
