---
po_doc_type: learn
title: Display Is Not Probe
status: partial
engine_commit: "5ce15c13"
scene_id: null
scene_class: null
instrument_tier: null
channel_id: null
units: null
validity_condition: "Doctrine page; scientific claims require sealed channels and Complete snapshots."
visualization_mapping: "Explains mapping vs measurement; does not define a display recipe."
claim_boundary: "Display paths never establish ProbeOutcomeCode or region membership by color alone."
evidence_links: []
contract_links:
  - CathedralProbe/ProbeOutcomeCode.cs
  - src/XPrimeRay.ObservationLayer/SealedObservationFrame.cs
last_qualified_run: null
generated: false
---

# Display Is Not Probe

!!! abstract "Observation card"
    | Field | Value |
    |-------|-------|
    | Status | **partial** (doctrine; UX overlays still evolving) |
    | Verified against | `5ce15c13` (docs ontology; sealed path base `6e69d792`) |
    | Claim boundary | Measurement ≠ visualization mapping |

---

## The split in one breath

| Side | What it is | Authority |
|------|------------|-----------|
| **Probe / instrument** | Transport computation + sealed observation channels | Scientific |
| **Display** | Coloring samples or host buffers for human eyes | Presentation only |

If a pixel is magenta, brown, or white on the plate, that is a **mapping result** unless a caption names a sealed channel and a Complete snapshot backs the class.

---

## World viewport vs Observation Plate

| Surface | Public name | What it shows |
|---------|-------------|----------------|
| Large 3D Godot view | **World viewport** | Scene meshes, lighting, locomotion camera |
| Rectangular film inset | **Observation Plate** | Mapped transport samples / host film buffer |

The world viewport is **not** the Observation Plate. Portal lettering can warp in the plate while the mesh looks “normal” in the viewport—or the reverse presentation artifacts can dominate. Scientific reading prefers the plate **plus** Inspector counts, not the free camera beauty view alone.

**Internal pair:** Observation Plate (`FilmView` / film buffer); Transport Lens (`GrinFilmCamera` implementation).

---

## Measurement vs mapping

```text
Instrument  →  Observation channel (sealed)  →  Display Mode (mapping)  →  Observation Plate
```

- **Measurement:** e.g. `cathedral.probe.outcome` stores `ProbeOutcomeCode` per sample.
- **Mapping:** e.g. NormalRGB takes a normal vector and paints \(n\cdot 0.5+0.5\).
- **Plate:** shows the mapped colors (and may use legacy fallback fills when data is missing).

Changing **Display Mode** must not change the outcome histogram. If it appears to, you are not reading a sealed plane—or the frame was re-generated.

---

## Semantic plane vs visual color

| Layer | Example | Use for |
|-------|---------|---------|
| Semantic plane | `HitGeometry`, `MaxStepsExhausted` | Region Probe seeds, histograms, evidence |
| Visual color | Magenta block, brown NormalRGB face | Teaching, reconnaissance, **never** sole class proof |

**RGB (or NormalRGB) cannot identify scientific regions.** Unresolved Regions are connected components on the **outcome plane** (default: `MaxStepsExhausted`), carried by `cathedral.probe.region_label`—not flood-fills of pink pixels.

---

## Why RGB fails as a region classifier

1. **Fallback fills** reuse a fixed presentation color for many host failure/missing paths.
2. **Upscale blocks** (80×45 → plate) create rectangular magenta that looks “semantic” but follows the sample grid.
3. **BackgroundResolved** and **HitGeometry** can both shade non-magenta or both fall into fallback depending on path—color does not encode the enum.
4. **Opacity** (e.g. 40%) blends plate with the viewport and further confuses class reading.

---

## Compact rules

1. Viewport ≠ Observation Plate.
2. Display Mode ≠ Instrument.
3. Plate color ≠ outcome code.
4. Region Probe outlines come from sealed labels, not lasso-on-magenta.
5. Complete frame ≠ successful geometry resolution.
6. Deeper Region Refinement ≠ greater truth—only more **Transport Effort**.

---

## See also

- [The Great Magenta Confusion](great_magenta_confusion.md)
- [Collider Contact Is Not Semantic Resolution](collider_contact_is_not_semantic_resolution.md)
- [Reading the Outcome Plane](reading_outcome_plane.md)
- [Public Vocabulary](../reference/public_vocabulary.md)
- [Claim Boundaries](../reference/claim_boundaries.md)
