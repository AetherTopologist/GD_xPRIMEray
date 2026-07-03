# Glowing Heart v3.3 — Fixture Library Index

**Milestone:** Glowing Heart v3.3 — Fixture Library Release Candidate  
**Generated:** 2026-07-03  
**Claim boundary:** Core artifact generation only. Not Godot comparison. Not parity. Not physical validation.

---

## Run Summary

| | Count |
|---|---|
| Fixtures selected | 4 |
| Fixtures passed | 4 |
| Fixtures deferred | 52 |
| Fixtures failed | 0 |

## Family: `grin_radial_smoke_family_v1`

Radial GRIN smoke transport. The only Core-runnable fixture family at v3.3.

| Fixture | Role | Grid | Rays | Mean Bend | Max Bend | Validation | Artifact Path |
|---|---|---|---|---|---|---|---|
| `grin_radial_smoke` | canonical_primary | 40×22 | 880 | 9.83e-4 | 5.62e-3 | **PASS** | `Docs/assets/glowing_heart/v3_3/grin_radial_smoke/` |
| `grin_radial_smoke_variant` | sensitivity_variant | 40×22 | 880 | 1.18e-3 | 6.71e-3 | **PASS** | `Docs/assets/glowing_heart/v3_3/grin_radial_smoke_variant/` |
| `grin_radial_smoke_observer_variant` | observer_variant | 40×22 | 880 | 9.59e-4 | 5.62e-3 | **PASS** | `Docs/assets/glowing_heart/v3_3/grin_radial_smoke_observer_variant/` |
| `grin_radial_smoke_resolution_variant` | resolution_variant | 41×22 | 902 | 9.60e-4 | 5.62e-3 | **PASS** | `Docs/assets/glowing_heart/v3_3/grin_radial_smoke_resolution_variant/` |

### Channels available

- `bend_magnitude_metric` — per-ray bend magnitude scalar grid (ASCII preview + PPM + CSV)
- `traversal_step_count` — per-ray integration step count scalar grid (CSV)

### Field parameters across family

| Fixture | amplitude | fovDegrees | gridWidth |
|---|---|---|---|
| canonical | 0.25 | 60 | 40 |
| sensitivity_variant | **0.30** (+20%) | 60 | 40 |
| observer_variant | 0.25 | **61** (+1°) | 40 |
| resolution_variant | 0.25 | 60 | **41** (+1 col) |

### Observable sensitivity result

The +20% amplitude variant (`grin_radial_smoke_variant`) produces:
- mean bend: +19.9% vs canonical (0.001179 vs 0.000983)
- max bend: +19.4% vs canonical (0.006713 vs 0.005624)

This confirms the Core transport is linearly sensitive to GRIN field amplitude at these parameter values. The observation is simulation-bounded; no physical interpretation is claimed.

## Deferred Fixture Categories

| Category | Count | Reason |
|---|---|---|
| Godot-only scene fixtures (.tscn) | 50 | Require Godot runtime |
| Shared metadata bridges | 2 | Schema/contract files; not runnable |

## Structured Sources

- Candidates: `reports/glowing_heart_v3_3_fixture_library_candidates.preview.json`
- Selection: `reports/glowing_heart_v3_3_fixture_library_selection.preview.json`
- Index: `reports/glowing_heart_v3_3_fixture_library_index.preview.json`
- Schema: `schemas/glowing_heart/fixture_library_index.v0.preview.json`
