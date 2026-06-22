# Project Glowing Heart Gap Matrix (Preview)

Generated: 2026-06-22T01:00:40Z

Runtime executed: false

Parity claim: NONE

## Summary

| Status | Count |
|---|---:|
| MATCH | 2 |
| PARTIAL | 6 |
| MISSING_IN_CORE | 4 |
| MISSING_IN_GODOT_EXPORT | 0 |
| UNKNOWN | 2 |

## Gap Matrix

| Category | Status | Reason |
|---|---|---|
| Fixture Identity | MATCH | Core fixture exists at fixtures/grin_radial_smoke.json; Godot fixture export exists at Fixtures/fixture_hermetic_observatory_grin.tscn. |
| Observer / Camera | PARTIAL | Core observer exists; Godot Camera3D detected, but no shared observer/camera schema exists. |
| Field Definition | PARTIAL | Core has a grin_radial field; Godot export has GRIN/FieldSource signals, but field parameters are not normalized. |
| Transport Concept | PARTIAL | Core has radial_grin_smoke transport settings; Godot export has Ray/Renderer/Transport references, but no shared transport baseline. |
| Validation | PARTIAL | Core validation metadata exists; Godot export has static closure/contract signals, but validation vocabulary is not shared. |
| Closure Concept | MISSING_IN_CORE | Godot export has closure-style signals; Core smoke fixture explicitly does not require hermetic closure. |
| Receiver Concept | MISSING_IN_CORE | Godot export includes receiver nodes/groups; Core smoke fixture has no receiver or collision target concept. |
| Snapshot Output | PARTIAL | Core packets include metric snapshots; Godot side is represented as renderer/observatory output metadata, not a shared snapshot type. |
| Observatory Artifact | MATCH | Both sides are represented in the Glowing Heart observatory artifact chain. |
| Runtime Dependency | PARTIAL | Core fixture can run outside Godot; Godot fixture is a scene that requires Godot for runtime behavior. |
| Scene Graph | MISSING_IN_CORE | Godot export has scene nodes; Core fixture is JSON metadata plus transport settings with no scene graph. |
| Geometry | MISSING_IN_CORE | Godot export includes StaticBody3D receiver geometry; Core smoke fixture has no geometry or collision model. |
| Portal / Wormhole | UNKNOWN | No portal or wormhole signal is present in the Core fixture or static Godot export. |
| Boundary Modeling | UNKNOWN | No explicit boundary modeling signal is present in the static Godot export; Core fixture has no boundary model. |

## Bridge Readiness

Readiness Score:
50 / 100

Method:
MATCH = 10 points
PARTIAL = 5 points
everything else = 0

This score is informational only.

It is NOT parity.

It is NOT scientific validation.

It is only a planning aid.

## Recommended Normalization Targets

- Shared fixture schema
- Shared observer/camera definition
- Shared field parameter vocabulary
- Shared transport baseline vocabulary
- Shared validation and closure vocabulary
