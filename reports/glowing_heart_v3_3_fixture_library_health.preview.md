# Glowing Heart v3.3 — Fixture Library Health Report

**Generated:** 2026-07-03  
**Milestone:** Glowing Heart v3.3 — Fixture Library Release Candidate  
**Claim boundary:** Core artifact generation only. Not Godot comparison. Not parity. Not physical validation.

---

## Build Health

| Check | Status | Notes |
|---|---|---|
| `dotnet build XPrimeRay.Core` | ✅ PASS | 0 warnings, 0 errors |
| `dotnet build XPrimeRay.Testbench.Cli` | ✅ PASS | 0 warnings, 0 errors |

## Fixture Run Results

| Fixture | Validation | Rays | Hits | Misses | Mean Bend | Max Bend |
|---|---|---|---|---|---|---|
| `grin_radial_smoke` | ✅ PASS | 880 | 0 | 880 | 9.83e-4 | 5.62e-3 |
| `grin_radial_smoke_variant` | ✅ PASS | 880 | 0 | 880 | 1.18e-3 | 6.71e-3 |
| `grin_radial_smoke_observer_variant` | ✅ PASS | 880 | 0 | 880 | 9.59e-4 | 5.62e-3 |
| `grin_radial_smoke_resolution_variant` | ✅ PASS | 902 | 0 | 902 | 9.60e-4 | 5.62e-3 |

**All hits = 0:** Expected. The `radial_grin_smoke` transport mode is field-only; no geometry intersection is modeled. All rays traverse the field and exit without a surface hit. This is correct behavior.

## Fixture Counts

| | Count |
|---|---|
| Fixtures discovered | 56 |
| Core JSON fixtures | 4 |
| Selected | 4 |
| Run | 4 |
| Passed | 4 |
| Deferred | 52 |
| Failed | 0 |

## Deferred Fixtures

| Reason | Count |
|---|---|
| Godot runtime required (.tscn) | 50 |
| Schema/metadata only (not runnable) | 2 |

No fixtures failed. All deferrals are due to runtime capability boundary (Godot dependency), not transport errors.

## Publishable Artifacts

| Artifact | Path | Published to Site |
|---|---|---|
| Gallery page | `Docs/xPRIMEray/project_glowing_heart_v3_3_fixture_library_gallery.md` | ✅ Yes (under docs_dir) |
| Release candidate doc | `Docs/xPRIMEray/project_glowing_heart_v3_3_fixture_library_release_candidate.md` | ✅ Yes |
| ASCII snapshots (×4) | `Docs/assets/glowing_heart/v3_3/*/snapshot_ascii.txt` | ✅ Yes |
| Manifests (×4) | `Docs/assets/glowing_heart/v3_3/*/manifest.json` | ✅ Yes |
| Run summaries (×4) | `Docs/assets/glowing_heart/v3_3/*/run_summary.md` | ✅ Yes |
| Ray metrics CSVs (×4) | `Docs/assets/glowing_heart/v3_3/*/ray_metrics.csv` | ✅ Yes |
| Candidates JSON | `reports/glowing_heart_v3_3_fixture_library_candidates.preview.json` | ❌ reports/ not in docs_dir |
| Selection JSON | `reports/glowing_heart_v3_3_fixture_library_selection.preview.json` | ❌ reports/ not in docs_dir |
| Index JSON | `reports/glowing_heart_v3_3_fixture_library_index.preview.json` | ❌ reports/ not in docs_dir |
| Schema | `schemas/glowing_heart/fixture_library_index.v0.preview.json` | ❌ schemas/ not in docs_dir |
| Run packets (full CSVs, PPM) | `output/glowing_heart_library_v3_3/*/` | ❌ PPM gitignored; CSVs in git but not in docs_dir |

**Site publishing policy:** Lightweight artifacts (ASCII, manifest, run_summary, ray_metrics) are mirrored to `Docs/assets/glowing_heart/v3_3/` for GitHub Pages publication. Reports, schemas, and full run packets remain in their canonical locations for repo consumers.

## MkDocs Gallery Link Status

| Nav entry | Target file | Status |
|---|---|---|
| v3.3 Fixture Library Gallery | `xPRIMEray/project_glowing_heart_v3_3_fixture_library_gallery.md` | ✅ File exists |
| v3.3 Release Candidate | `xPRIMEray/project_glowing_heart_v3_3_fixture_library_release_candidate.md` | ✅ File exists |

## Claim Boundary Scan

Expected: no `proves`, `validated`, `pixel parity`, `matches reality`, `renderer equivalence`, `physical validation` in new files.

New v3.3 files contain only negative-boundary language and simulation-bounded measurement claims. ✅

## Protected File Status

| File | Modified | Expected |
|---|---|---|
| `reports/observatory_catalog.json` | No | ✅ Untouched |
| `GrinFilmCamera.cs` | No | ✅ Untouched |
| `RendererCore/Testing/RenderTestRunner.cs` | No | ✅ Untouched |
| `GodotAdapter/SnapshotBuilder.cs` | No | ✅ Untouched |
