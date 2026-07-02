# OC-001 Minimal Optical Closure — Fixture

**Status:** Stage 0 · Design only · not yet implemented  
**Epistemic tier:** Implemented engine behavior (once built) · Experimental interpretation (results)

## Fixture concept

OC-001 is a three-object scene:

1. **Reference sphere** — a plain checkerboard sphere with no field interaction, used as a
   straight-transport baseline.
2. **Probe sphere** — a checkerboard sphere with a polar dent feature, placed inside or adjacent
   to the GRIN field volume.
3. **GRIN field volume (north-offset)** — a radial gradient index field centered above the probe
   sphere, curving rays away from the probe's dent region at the default observer distance.

The observer begins far enough away that the dent/checker region on the probe sphere is not
reached by the curved transport. Moving the observer closer reopens the region. The fixture
documents this observer-dependent behavior as a sweep table, not as a single binary classification.

## Fixture design (probe metadata only — no intersection configuration)

```json
{
  "name": "oc_001_minimal_optical_closure",
  "description": "Observer-dependent optical accessibility probe. Reference sphere, probe sphere with polar dent, and a north-offset GRIN field volume.",
  "probeMetadata": [
    {
      "colliderName": "reference_sphere",
      "role": "reference",
      "center": [-1.8, 0, 0],
      "radius": 0.6,
      "material": {
        "uvMode": "analytic_spherical",
        "checkerTilesU": 6,
        "checkerTilesV": 6
      }
    },
    {
      "colliderName": "probe_sphere",
      "role": "probe",
      "center": [0, 0, 0],
      "radius": 0.8,
      "material": {
        "uvMode": "analytic_spherical",
        "checkerTilesU": 8,
        "checkerTilesV": 8,
        "hasPolarDent": true,
        "polarDentLatitude": 0.85,
        "polarDentDepth": 0.15
      }
    }
  ]
}
```

!!! info "probeMetadata is not intersection configuration"
    The `probeMetadata` block is used only for post-hit UV computation and diagnostic
    classification. It does not configure the hit system. The Godot scene's physics geometry
    determines what gets hit; `probeMetadata` tells the interpretive layer how to read that result.

## Expected output set

| Output | Channel | Description |
|---|---|---|
| Beauty render | `checker_albedo` (diagnostic) | Checker pattern as seen from the default observer position |
| Straight vs curved comparison | `ray_hit_flag` (existing) | Hit map with field off vs field on |
| UV coordinate overlay | `surface_uv_coords` (diagnostic) | False-color UV map at hit points |
| Checkerboard overlay | `checker_albedo` (diagnostic) | Binary checker state per pixel |
| Curvature / field overlay | `bend_magnitude_metric` (existing) | Field curvature at each sample |
| Hit classification map | collider ID per pixel | Which object was hit |
| Accessibility diagnostic map | `DiagnosticState` enum | 5-class probe accessibility per pixel |
| Observer sweep table | all diagnostics per observer origin | How accessibility changes with distance |

## What this fixture does not demonstrate

- Not a Godot parity comparison.
- Not a Core-vs-Core comparison (Phase 1 is Godot-side only).
- Not proof of hidden geometry.
- Not proof of optical closure in physical space.
- Simulation-bounded claim only.
