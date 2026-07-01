# Project Glowing Heart Shared Fixture Instance (Preview)

Generated: 2026-06-22T22:02:45Z

Runtime executed: false

Parity claim: NONE

## Instance

| Field | Value |
|---|---|
| Name | glowing_heart_grin_bridge |
| Version | v0.preview |
| Core Fixture | Fixtures/grin_radial_smoke.json |
| Godot Fixture | Fixtures/fixture_hermetic_observatory_grin.tscn |

## What This Instance Represents

A metadata bridge candidate between the Core radial GRIN smoke fixture and the Godot hermetic observatory GRIN fixture.

## Shared Sections

| Section | Source | Status |
|---|---|---|
| observer | core | present |
| rayGrid | core | present |
| fields | core | present |
| transport | core | present |
| receivers | godot_static_export | hinted |
| validation | mixed_metadata | partial |

## Hints

| Hint | Value |
|---|---|
| Receivers | Receiver signal detected in static Godot fixture export; count not normalized. |
| Closure | godot_static_export_has_closure_signal |

## Not Ready For

- parity
- pixel comparison
- closure equivalence
- transport equivalence
- public demo claim

## Next Step

v1.5 should create a public-demo readiness checklist and rank what is still needed before Grok begins public interface/demo framing.
