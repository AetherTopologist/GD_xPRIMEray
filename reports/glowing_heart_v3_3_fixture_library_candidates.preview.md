# Glowing Heart v3.3 — Fixture Library Candidates

**Generated:** 2026-07-03  
**Milestone:** Glowing Heart v3.3 — Fixture Library Release Candidate  
**Claim boundary:** Core smoke transport only. Not Godot comparison. Not parity. Not physical validation.

---

## Discovery Summary

| Category | Count |
|---|---|
| Total fixture files discovered | 56 |
| Core-runnable JSON fixtures | 4 |
| Godot-only scene fixtures (.tscn) | 50 |
| Shared metadata bridges | 2 |

## Fixture Families Detected

| Family | Core Runnable | Fixtures | Notes |
|---|---|---|---|
| `grin_radial_smoke_family_v1` | ✅ Yes | 4 | Only Core-runnable family in library |
| `hermetic_observatory_godot` | ❌ Godot | 3 | .tscn, deferred |
| `blackhole_minimal_godot` | ❌ Godot | 3 | .tscn, deferred |
| `einstein_ring_minimal_godot` | ❌ Godot | 3 | .tscn, deferred |
| `grin_basic_visual_godot` | ❌ Godot | 9 | .tscn, deferred |
| `curved_minimal_godot` | ❌ Godot | 2 | .tscn, deferred |
| `boundary_shell_godot` | ❌ Godot | 3 | .tscn, deferred |
| `metric_basic_visual_godot` | ❌ Godot | 3 | .tscn, deferred |
| `overspace_hermetic_godot` | ❌ Godot | 3 | .tscn, deferred |
| `overspace_wormhole_godot` | ❌ Godot | 5 | .tscn, deferred |
| `atomic_orbital_godot` | ❌ Godot | 2 | .tscn, deferred |
| `wormhole_transport_demo_godot` | ❌ Godot | 1 | .tscn, deferred |
| `domain_resolver_stress_godot` | ❌ Godot | 1 | .tscn, deferred |
| `shared_metadata` | ❌ Not runnable | 2 | Schema bridges only |

## Candidate Decisions

| Fixture | Role | Decision | Reason |
|---|---|---|---|
| `Fixtures/grin_radial_smoke.json` | Canonical primary | **include** | Foundation fixture; all prior evidence chain references this |
| `Fixtures/grin_radial_smoke_variant.json` | Sensitivity variant | **include** | Amplitude +20% → measurable maxBend increase |
| `Fixtures/grin_radial_smoke_observer_variant.json` | Observer variant | **include** | FOV +1° → distinct ASCII pattern; Core eligibility test |
| `Fixtures/grin_radial_smoke_resolution_variant.json` | Resolution variant | **include** | 41x22 grid → 902 rays; confirms non-standard grid support |
| `Fixtures/shared/glowing_heart_grin_bridge.v0.preview.json` | Metadata bridge | **defer** | Not runnable; schema only |
| `Fixtures/shared/glowing_heart_observer_target.v0.preview.json` | Observer contract | **defer** | Not runnable; contract only |
| 50× `.tscn` Godot fixtures | Godot scene | **defer** | Require Godot runtime |

Structured source: `reports/glowing_heart_v3_3_fixture_library_candidates.preview.json`
