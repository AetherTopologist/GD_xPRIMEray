# Project Glowing Heart v0.1 Baseline

## Snapshot

- Branch: `main`
- Date/time: `2026-06-20T16:04:25-05:00`
- Baseline intent: additive artifact drop only. This v0.1 pass creates a pure C# Core project and CLI testbench without moving or deleting existing Godot renderer files.

## Git Status Summary

The worktree already contained unrelated documentation path changes before this artifact work began:

```txt
 D Docs/basin_taxonomy_v1.md
 D Docs/observer_storyboard/observer_storyboard_v1.md
 D Docs/observer_storyboard/observer_storyboard_v1.schema.json
?? Docs/basin_atlas_v1.md
?? Docs/Observatory/canonical_fixtures.md
?? Docs/xeno_zeno_citation_atlas.md
```

These existing changes are recorded as baseline context and are intentionally not touched by v0.1.

## Expected Created Files and Directories

```txt
src/XPrimeRay.Core/
src/XPrimeRay.Testbench.Cli/
Fixtures/glowing_heart_minimal.json
Docs/xPRIMEray/project_glowing_heart_v0_1_baseline.md
Docs/xPRIMEray/project_glowing_heart_v0_1_artifact.md
```

The existing solution is expected to gain the two new project entries. The root Godot project is expected to gain only narrow `Compile Remove` exclusions for the two new standalone project folders.

## Minimal Extraction Notes

Copied/adapted into Core:

- `FieldModels`
- `FieldCurves`
- `Aabb3`
- `MetricTransportTypes`
- `StepResult`
- `StepPolicy`
- `ReferenceTransportOracle` records, with Godot vector use translated to `System.Numerics.Vector3`

Skipped optional extraction candidates for v0.1:

- `FieldSystem`, `FieldTLAS`, `IMetricField`, `IIntegrator`, and `MetricHeuristicIntegrator`: useful but pull in the current `SceneSnapshot` contract and broaden the extraction surface.
- `CurvatureBoundGrid`: depends on snapshot/TLAS types and is better extracted with the real transport path.
- `DomainTelemetry`: currently imports Godot vector types and is not needed for the wiring proof.
- `Common/DomainTelemetry.cs`, `Geometry/CurvatureBoundGrid.cs`, and full scene snapshot types remain in the Godot-side renderer until a later adapter milestone.
