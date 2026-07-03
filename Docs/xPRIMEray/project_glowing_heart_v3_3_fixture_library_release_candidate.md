# Glowing Heart v3.3 — Fixture Library Release Candidate

**Status:** Release Candidate  
**Date:** 2026-07-03  
**Claim boundary:** Core smoke transport only · Not Godot comparison · Not parity · Not physical validation · Not renderer equivalence

---

## What Changed

Glowing Heart v3.3 introduces the first automated fixture library run — a complete pass of every Core-runnable fixture through the CLI testbench, organized by family and published as a browsable gallery.

Previous milestones (v3.0–v3.2) built the dashboard, evidence map, and gallery index infrastructure. v3.3 is the first milestone where those structures are populated with a full library run rather than a single fixture.

**New in v3.3:**

- 4 fixture runs in `output/glowing_heart_library_v3_3/` (all PASS)
- Fixture candidate classification report (56 total fixtures surveyed)
- Fixture selection report (4 selected, 52 deferred)
- Fixture library index (JSON + Markdown)
- Fixture library index schema at `schemas/glowing_heart/fixture_library_index.v0.preview.json`
- Site-published artifact mirror at `Docs/assets/glowing_heart/v3_3/` (ASCII, manifest, run_summary, ray_metrics per fixture)
- Browsable MkDocs gallery page
- Health / preflight report

## What This Demonstrates

- The Core CLI testbench can run the full library of Core-runnable fixtures in a single automated pass.
- The `grin_radial_smoke_family_v1` family (4 fixtures) is stable: all 4 configurations pass validation across amplitude, observer FOV, and grid resolution variations.
- The bend-magnitude transport metric is consistently measurable and sensitive to GRIN field parameters: a +20% amplitude increase produces a ~20% mean-bend increase (simulation-bounded measurement).
- Site-publishable lightweight artifacts can be generated and mirrored to `Docs/assets/` without committing binary PPM files.
- The fixture library structure (`candidates → selection → index → gallery`) provides a repeatable pipeline for future library expansion.

## What This Does Not Demonstrate

- Godot parity. No Godot runtime was executed.
- Image or pixel comparison. No pixel-level comparison was performed.
- Hermetic closure. The smoke transport mode does not model geometry intersection; all rays miss by design.
- Physical validation. These are simulation-bounded measurements only.
- Renderer equivalence. Core and Godot renderer outputs are not compared here.
- Any claim about physical reality, hidden geometry, or optical phenomena beyond the declared simulation parameters.

## Fixture Selection Method

1. Scan all files under `Fixtures/` — 56 total.
2. Classify by extension and comparisonIdentity: 4 Core JSON fixtures, 50 Godot `.tscn` fixtures, 2 shared metadata bridges.
3. Identify unique roles within each Core-runnable family: canonical primary, sensitivity variant (amplitude), observer variant (FOV), resolution variant (grid width).
4. Select one fixture per unique role — all 4 in the single Core-runnable family.
5. Defer all Godot-only and metadata-only fixtures conservatively without prejudice.

## Artifact Publishing Policy

| Artifact type | Location | Published to site |
|---|---|---|
| Gallery page | `Docs/xPRIMEray/` | ✅ Yes (under docs_dir) |
| ASCII snapshots | `Docs/assets/glowing_heart/v3_3/` | ✅ Yes |
| Manifests | `Docs/assets/glowing_heart/v3_3/` | ✅ Yes |
| Run summaries | `Docs/assets/glowing_heart/v3_3/` | ✅ Yes |
| Ray metrics CSVs | `Docs/assets/glowing_heart/v3_3/` | ✅ Yes |
| Reports / index | `reports/` | ❌ Not in docs_dir |
| Full run packets | `output/glowing_heart_library_v3_3/` | ❌ Not in docs_dir (text artifacts tracked in git) |
| PPM snapshots | `output/glowing_heart_library_v3_3/` | ❌ Binary, gitignored |

## How to Regenerate

```bash
# Build
dotnet build src/XPrimeRay.Core/XPrimeRay.Core.csproj
dotnet build src/XPrimeRay.Testbench.Cli/XPrimeRay.Testbench.Cli.csproj

# Run all 4 library fixtures
dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture Fixtures/grin_radial_smoke.json \
  --output output/glowing_heart_library_v3_3
dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture Fixtures/grin_radial_smoke_variant.json \
  --output output/glowing_heart_library_v3_3
dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture Fixtures/grin_radial_smoke_observer_variant.json \
  --output output/glowing_heart_library_v3_3
dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture Fixtures/grin_radial_smoke_resolution_variant.json \
  --output output/glowing_heart_library_v3_3

# Mirror lightweight artifacts to Docs/assets for site publishing
# (copy snapshot_ascii.txt, manifest.json, run_summary.md, ray_metrics.csv per run)
```

New run IDs will differ from the v3.3 committed run IDs (timestamp-based). The metrics should be identical for any unmodified Core build.

## Known Deferred Fixture Classes

| Class | Count | Blocking dependency |
|---|---|---|
| Godot scene fixtures | 50 | Godot runtime · `PhysicsServer3D` collision narrowphase · `Camera3D` projection |
| Hermetic closure family | 3 | Godot runtime + `ClosureValidator` Godot binding |
| Black hole / Einstein ring / curved | 6 | Godot metric transport mode |
| GRIN visual family | 9 | Godot scene graph + visual controllers |
| Wormhole / overspace | 8 | Godot checkpoint sequencer + portal transport |
| Atomic orbital | 2 | Godot GRIN room visual controller |
| Boundary shell | 3 | Godot boundary physics |

All deferred without prejudice. Graduation path: implement each transport mode in Core, define a JSON fixture schema for it, confirm the CLI runs it cleanly.

## Next Milestone

**Glowing Heart v3.4 — Published Fixture Artifact Browser**

If v3.3 confirms publishable artifact paths (confirmed ✅), v3.4 should:

- Refine the GitHub Pages browsing experience with stable artifact naming per fixture (not timestamp-based run IDs)
- Add an SVG or lightweight visual thumbnail per fixture (computed from ASCII using a post-processing step, or a simple color-mapped PNG generated by the CLI)
- Introduce fixture-family navigation in the gallery (a TOC or family overview when more than one family exists)
- Add a `--retain-as <name>` flag or a fixture library manifest that pins canonical run artifacts by fixture name rather than timestamp
- Track the fixture library against `reports/observatory_catalog.json` as a new catalog section: `fixture_library_v3_4`
- Run preflight `python3 tools/glowing_heart_preflight.py` cleanly with v3.3 artifacts visible
