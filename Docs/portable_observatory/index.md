---
po_doc_type: landing
title: Portable Observatory
status: partial
engine_commit: "5ce15c13"
generated: false
claim_boundary: "This lane teaches how to observe simulated transport. It does not certify physical gravity, wormholes, or proper time."
---

# Portable Observatory

**Display ≠ probe.**
**Plate color ≠ outcome code.**
**Deeper refinement ≠ greater truth.**
**Complete frame ≠ successful geometry resolution.**
**Viewport ≠ Observation Plate.**
**Display Mode ≠ Instrument.**
**Fixture PASS ≠ live free-roam qualification.**

The Portable Observatory is the public documentation lane for reading sealed transport observations in xPRIMEray—across Godot and (where adapters exist) Project Glowing Heart—without collapsing measurement into pretty pictures.

---

## First-run spine

Work in this order:

1. **Choose a Scene** — experiment context (Hermetic chamber, portal gallery, …).
2. **Enable the Transport Lens** — live or snapshot transport path that computes samples.
3. **Choose a Display Mode** — how channels or host shading are **mapped to color**.
4. **Read the Observation Plate** — the instrument face (mapped samples), not the full 3D world view.
5. **Open the Inspector** — context, validity, counts, dependencies.
6. **Use the Region Probe only on qualified observations** — SNAPSHOT Complete, outcome plane ready; never select from RGB alone.
7. **Open Evidence** for recipe-bound conclusions — fixtures and archived runs, not free-roam verdicts.

```text
Scene
  → Transport Lens
    → Display Mode
      → Observation Plate
        → Inspector
          → Region Probe
            → Evidence
```

---

## Ownership table

| Layer | Owns |
|-------|------|
| **Scene** | Experiment context (geometry, portals, shells, intended claim class). |
| **Transport Lens** | Computes transport samples under the current field/policy (`GrinFilmCamera` path). |
| **Instruments** | Define **measured** quantities (semantic outcomes, planned geometry/field/transport records). |
| **Observation channels** | Carry sealed measurements (`cathedral.probe.outcome`, …). |
| **Display Modes** | Color those channels or host shading for human viewing. |
| **Observation Plate** | Presents the mapping (FilmView / plate buffer). |
| **Inspector** | Explains context, validity, counts, and dependencies. |
| **Evidence Console** | Handles qualified recipes and archived results. |

---

## Implementation status (truth)

### Sealed channels — **implemented**

| Channel ID | Role |
|------------|------|
| `cathedral.probe.outcome` | Per-sample `ProbeOutcomeCode` |
| `cathedral.probe.region_label` | Connected components on the semantic plane |
| `cathedral.probe.refinement_level` | Region-refinement history level |

### Display paths — **available as presentation, not sealed raw channels**

| Display Mode | Role |
|--------------|------|
| RGB / host beauty path | Presentation color (not outcome authority) |
| NormalRGB | \(n \mapsto n\cdot0.5+0.5\) mapping |
| Depth | Distance / depth heatmap mapping |
| NdotV / TwoSidedNdotV | Facing brightness mapping |

### Planned channels — **not yet sealed** (do not speak in present tense as shipped)

- `geometry.normal`, `geometry.depth`, `geometry.ndotv`
- `transport.final_step_count`, `transport.path_length`, `transport.boundary_crossing_count`
- `field.magnitude`, `field.gradient_magnitude`

**Intended future mapping chain (planned):**

```text
geometry.normal  → normal_rgb.v1 mapping  → Observation Plate
geometry.depth   → depth_heatmap.v1 mapping → Observation Plate
```

Until those channels ship, NormalRGB/Depth remain **host display paths**, not portable sealed geometry instruments.

---

## Where to go next

| Need | Page |
|------|------|
| First doctrine | [Display Is Not Probe](learn/display_is_not_probe.md) |
| Glossary | [Public Vocabulary](reference/public_vocabulary.md) |
| Frame anatomy | [Anatomy of an Observation Frame](learn/anatomy_of_observation_frame.md) |
| Outcome codes | [Reading the Outcome Plane](learn/reading_outcome_plane.md) |
| Magenta hazard | [The Great Magenta Confusion](learn/great_magenta_confusion.md) |
| Closed-box calibration | [Hermetic Normal Calibration](experiments/hermetic_normal_calibration.md) |
| Region effort | [Tuning the Region Probe](experiments/tuning_the_region_probe.md) |
| Diagnostics levels | [Diagnostics](architecture/diagnostics.md) |
| Claim hygiene | [Claim Boundaries](reference/claim_boundaries.md) |

### Historical lanes (preserved)

Older material keeps its own language; bridge when terms differ:

- [Observatory Atlas](../Observatory/observatory_atlas.md)
- [Observatory Gallery](../Observatory_Gallery/index.md)
- [Project Glowing Heart](../xPRIMEray/project_glowing_heart_atlas_link.md)

---

## Core doctrine (compact)

| Confuse… | With… | Correction |
|----------|-------|------------|
| Observation Plate color | Outcome class | Only sealed outcome / codes |
| Viewport beauty | Measurement | Display path only |
| Display Mode | Instrument | Mapping ≠ measured channel |
| Region Refinement depth | Truth | More **Transport Effort**, not greater physics truth |
| Complete snapshot | Geometry solved | Completeness is lifecycle, not HitGeometry success |
| Fixture PASS | Live free-roam OK | Recipe-bound evidence only |

*Simulated transport. Numerical policy. Scene units. Policy-relative closure.*
