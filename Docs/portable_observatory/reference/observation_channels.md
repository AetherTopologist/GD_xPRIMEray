---
po_doc_type: reference
title: Observation Channels
status: partial
engine_commit: "5ce15c13"
generated: false
claim_boundary: "Only listed implemented channels are sealed today. Planned IDs must not be written in present tense as shipped."
---

# Observation Channels

Curated status page. Prefer generated catalogs when they exist; this table is intentionally short.

**Verified against:** sealed adapter `6e69d792`; diagnostics context `5ce15c13`.

---

## Implemented (sealed)

| Channel ID | Content | Units / domain | Dependencies | Claim boundary |
|------------|---------|----------------|--------------|----------------|
| `cathedral.probe.outcome` | `ProbeOutcomeCode` per sample | enum / code | Transport pass-1 / refine apply | Semantic termination class under policy—not physical wormhole proof |
| `cathedral.probe.region_label` | Connected-component label | label id | Derived from outcome plane | Region membership from **codes**, not RGB |
| `cathedral.probe.refinement_level` | Deepenings applied | level | Outcome plane + refine history | Effort history—not greater truth |

---

## Display paths (presentation only — not sealed raw channels)

These may appear on the Observation Plate via host shading:

| Display path | Mapping idea | Sealed channel? |
|--------------|--------------|-----------------|
| RGB / host beauty | Host color recipe | **No** |
| NormalRGB | \(n\cdot 0.5+0.5\) | **No** — not `geometry.normal` |
| Depth | Distance heatmap | **No** — not `geometry.depth` |
| NdotV / TwoSidedNdotV | Facing brightness | **No** — not `geometry.ndotv` |

---

## Planned channels (future)

Do **not** describe these as currently sealed:

| Planned ID | Intended role |
|------------|---------------|
| `geometry.normal` | Portable per-sample normals |
| `geometry.depth` | Portable depth |
| `geometry.ndotv` | Portable \(n\cdot v\) |
| `transport.final_step_count` | Steps used at termination |
| `transport.path_length` | Integrated path length (scene units) |
| `transport.boundary_crossing_count` | Boundary events |
| `field.magnitude` | Evaluated field magnitude |
| `field.gradient_magnitude` | Gradient magnitude |

### Intended future display chain

```text
geometry.normal  →  normal_rgb.v1 mapping  →  Observation Plate
geometry.depth   →  depth_heatmap.v1 mapping  →  Observation Plate
```

Until then: Display Modes paint **host** buffers; Instruments do not yet seal those geometry planes portably.

---

## See also

- [Display Modes](display_modes.md)
- [Reading the Outcome Plane](../learn/reading_outcome_plane.md)
- [Anatomy of an Observation Frame](../learn/anatomy_of_observation_frame.md)
