---
po_doc_type: reference
title: Controls
status: partial
engine_commit: "5ce15c13"
generated: false
claim_boundary: "Key bindings document host defaults; some keys may be partially wired. Planned binds are labeled."
---

# Controls

Public control legend for the Godot Transport Lens host. Internal names only where useful.

---

## Compact strip

```text
G plate · N display · , . 0 1 field · Tab inspector · Esc evidence
P probe · J/K region · H shell · V walk/fly
```

---

## Primary keys

| Key | Public name | What it does | Boundary |
|-----|-------------|--------------|----------|
| **G** | Plate mode | Cycles Observation Plate OFF → SNAPSHOT → LIVE → OFF | SNAPSHOT for sealed probe work; LIVE exploratory display |
| **N** | Display Mode | Cycles film shading (Depth / NormalRGB / NdotV / …) | Presentation only |
| **Tab** | Inspector | Opens Observation Inspector | Numbers/context; not OI PASS |
| **0** / **1** | Field ends | Field strength → STRAIGHT (0) or FULL (1) | Policy scale—not “nature max” |
| **,** / **.** | Field fine | Step field strength down / up | Field Dial control—not evaluated field map |
| **P** | Probe deeper | Region Refinement on selection | More Transport Effort—not automatic truth |
| **J** / **K** | Prev / next region | Cycle Unresolved Regions | When wired |
| **R** | Reset probe effort | Clear refinement memory for context | When wired |
| **H** | Scene shell | Gallery ↔ Hermetic display preset | Invalidates observation context |
| **V** | Walk / Fly | Locomotion | Free-roam camera ≠ Transport Lens |
| **[** / **]** | Plate opacity | Presentation blend | Not measurement |
| **Esc** | Evidence Console | Workbench / release mouse | Recipe-bound evidence |

### Proposed / partial binds

| Key | Public name | Note |
|-----|-------------|------|
| **O** | Orientation display | Proposed dedicated orientation cycle; until bound, use **N** |
| **D** | Depth display | Proposed shortcut; until bound, use **N** |

---

## Operator order (safe)

1. Pose + field → **G** SNAPSHOT until Complete.
2. **Tab** histogram / validity.
3. **N** only after you know outcomes (display does not change classes).
4. **P** only on outcome-defined Unresolved Regions.
5. **Esc** for fixture recipes—not free-roam PASS.

---

## See also

- [Public Vocabulary](public_vocabulary.md)
- [Running the Observatory](../development/running_the_observatory.md)
- [Tuning the Region Probe](../experiments/tuning_the_region_probe.md)
