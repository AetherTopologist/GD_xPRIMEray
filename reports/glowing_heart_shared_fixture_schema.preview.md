# Project Glowing Heart Shared Fixture Schema Draft (Preview)

Generated: 2026-06-22T04:52:41Z

Parity claim: NONE

Runtime executed: false

## Purpose

This schema defines a neutral fixture vocabulary for bridging xPRIMEray-Core and GD_xPRIMEray.

## Required Sections

| Section | Purpose |
|---|---|
| identity | names the bridge fixture and claim status |
| sourceLinks | records Core, Godot, export, matrix, and candidate packet paths |
| observer | defines shared camera/observer vectors and field of view |
| rayGrid | defines ray grid dimensions |
| fields | defines neutral field parameters such as radial GRIN data |
| transport | defines transport mode, step budget, step size, and integrator label |
| geometry | records whether geometry is represented and where it came from |
| receivers | records receiver presence/count hints for closure-oriented fixtures |
| validation | records closure and coverage vocabulary without claiming equivalence |
| snapshots | records Core and Godot snapshot types and comparison readiness |
| artifacts | records known output and supporting artifact paths |
| runtimeHints | records runtime requirements without executing either side |
| limitations | keeps preview and non-parity limits explicit |
| normalizationNotes | captures the next vocabulary targets before parity work |

## Schema Artifact

schemas/glowing_heart/shared_fixture_schema.v0.preview.json

## Normalization Targets

- Shared observer definition
- Shared field parameter vocabulary
- Shared validation vocabulary
- Shared snapshot metric naming
- Godot scene metadata export standard

## Example Instance

```json
{
  "identity": {
    "name": "grin_radial_smoke_to_hermetic_observatory_grin",
    "version": "v0.preview",
    "description": "Metadata bridge candidate between Core radial GRIN smoke fixture and Godot hermetic observatory GRIN fixture.",
    "parityClaim": "NONE",
    "runtimeExecuted": false
  },
  "sourceLinks": {
    "coreFixturePath": "fixtures/grin_radial_smoke.json",
    "godotFixturePath": "Fixtures/fixture_hermetic_observatory_grin.tscn",
    "godotExportPath": "reports/glowing_heart_godot_fixture_export.preview.json",
    "gapMatrixPath": "reports/glowing_heart_gap_matrix.preview.json",
    "candidatePacketPath": "reports/glowing_heart_shared_fixture_candidate.preview.json"
  },
  "observer": {
    "source": "core",
    "origin": [
      0,
      0,
      -2
    ],
    "forward": [
      0,
      0,
      1
    ],
    "up": [
      0,
      1,
      0
    ],
    "fovDegrees": 60
  },
  "rayGrid": {
    "width": 40,
    "height": 22
  },
  "fields": [
    {
      "source": "core",
      "type": "grin_radial",
      "center": [
        0,
        0,
        0
      ],
      "radiusOuter": 1.5,
      "amplitude": 0.25,
      "curveType": "Power",
      "gamma": 1.0
    }
  ],
  "transport": {
    "source": "core",
    "mode": "radial_grin_smoke",
    "maxStepsPerRay": 32,
    "stepSize": 0.05,
    "integrator": "smoke_stepper"
  },
  "geometry": {
    "present": true,
    "source": "godot",
    "notes": "Static Godot export contains scene nodes and receiver geometry; Core fixture has no geometry model."
  },
  "receivers": {
    "present": true,
    "count": 6,
    "source": "godot",
    "notes": "Receiver concept detected from static Godot export receiver signals."
  },
  "validation": {
    "source": "core",
    "requireHermeticClosure": false,
    "maxMisses": 880,
    "closureHint": "godot_static_export_has_closure_signal",
    "coverageHint": "core_smoke_pass"
  },
  "snapshots": {
    "coreSnapshotType": "metric_snapshot",
    "godotSnapshotType": "renderer_snapshot",
    "comparisonReady": false,
    "notes": "Gap matrix marks snapshot output as partial; naming and semantics are not normalized."
  },
  "artifacts": {
    "coreManifestPath": "output/glowing_heart/<run_id>/manifest.json",
    "coreSnapshotPath": "output/glowing_heart/<run_id>/snapshot_ascii.txt",
    "godotCandidatePath": "Fixtures/fixture_hermetic_observatory_grin.tscn",
    "gapMatrixPath": "reports/glowing_heart_gap_matrix.preview.json"
  },
  "runtimeHints": {
    "requiresGodot": false,
    "requiresSnapshotBuilder": false,
    "requiresSceneTree": false,
    "requiresPhysics": false
  },
  "limitations": [
    "Schema draft only",
    "Godot runtime was not executed",
    "No parity claim",
    "No closure equivalence claim",
    "No transport equivalence claim"
  ],
  "normalizationNotes": [
    "Shared observer definition",
    "Shared field parameter vocabulary",
    "Shared validation vocabulary",
    "Shared snapshot metric naming",
    "Godot scene metadata export standard"
  ]
}
```

## What This Does Not Prove

- No parity
- No runtime equivalence
- No closure equivalence
- No transport equivalence

## Next Milestone

v1.4 should create the first shared fixture instance candidate using this schema draft.
