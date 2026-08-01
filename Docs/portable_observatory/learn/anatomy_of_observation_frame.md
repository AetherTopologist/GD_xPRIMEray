---
po_doc_type: learn
title: Anatomy of an Observation Frame
status: partial
engine_commit: "6e69d792"
scene_id: null
scene_class: null
instrument_tier: semantic
instrument_id: null
channel_id: null
source_channel_deps:
  - cathedral.probe.outcome
  - cathedral.probe.region_label
  - cathedral.probe.refinement_level
units: "frame-level mixed; per-channel units apply"
validity_condition: "A full-frame scientific claim requires a Complete snapshot, Unprocessed=0, histogram sum=total, matching context and dimensions."
visualization_mapping: "Host display recipes only; sealed channels are authoritative."
claim_boundary: "An observation frame is a sealed measurement package, not a pretty picture and not automatic physical truth."
evidence_links: []
contract_links:
  - src/XPrimeRay.ObservationLayer/SealedObservationFrame.cs
  - src/XPrimeRay.ObservationLayer/ObservationFrameDescriptor.cs
  - CathedralProbe/CathedralProbeObservationFrameAdapter.cs
  - CathedralProbe/ProbeFrameSummary.cs
last_qualified_run: null
generated: false
---

# Anatomy of an Observation Frame

!!! abstract "Observation card"
    | Field | Value |
    |-------|-------|
    | Status | **partial** |
    | Verified against | `6e69d792` (portable observation + sealed frames; lifecycle base `ef29ff79`) |
    | SceneId | *(host-supplied; not fixed on this page)* |
    | SceneClass | n/a (conceptual) |
    | InstrumentTier | semantic (+ optional tiers later) |
    | Channel IDs (present) | `cathedral.probe.outcome`, `cathedral.probe.region_label`, `cathedral.probe.refinement_level` |
    | Dependencies | outcome ← transport; region_label ← outcome; refinement_level ← outcome plane |
    | Units | codes / labels / levels (see channels) |
    | Validity | Complete snapshot · Unprocessed = 0 for full-frame claims |
    | Viz mapping | display-only |
    | Claim boundary | Frame completeness ≠ transport “success” or correct NormalRGB |
    | Evidence | sealed `SealedObservationFrame` + `ProbeFrameSummary` |

---

## Status

| Layer | Status |
|-------|--------|
| Portable sealed frame + descriptors | **implemented** (`XPrimeRay.ObservationLayer`) |
| Cathedral adapter: outcome / region / refinement channels | **implemented** |
| Geometric / field / path heatmaps as sealed channels | **planned** |
| Public multi-host replay gallery | **planned** |

---

## One concept

An **observation frame** is a host-neutral package:

```text
context + dimensions + policy fingerprint
    + one or more sealed channels
    + validity / claim metadata
```

The Godot film plate is a **visualization mapping** of some channels (or of legacy shading). It is not the frame.

---

## Parts of a frame

1. **Identity** — experiment/scene/host/engine commit (record-level; see portable experiment model).  
2. **Context key** — pose, field policy, dimensions, generation (must match for refine/compare).  
3. **Dimensions** — film width × height (e.g. 80×45, 160×90).  
4. **Lifecycle state** — request → pumping → **Complete** (or timeout / incomplete).  
5. **Channels** — dense planes or records with descriptors (id, type, domain, units, validity, claim boundary, deps).  
6. **Frame summary** — counts (hits, background, max-steps, faults, regions, last refine stats).  
7. **Evidence emission** — preferably exactly once per successful generation.

### Cathedral channels today

| Channel ID | What it holds | Units |
|------------|---------------|-------|
| `cathedral.probe.outcome` | `ProbeOutcomeCode` per sample | code |
| `cathedral.probe.region_label` | connected-component label | label id |
| `cathedral.probe.refinement_level` | deepenings applied | level |

---

## Completeness vs resolution

| Phrase | Means |
|--------|--------|
| Snapshot **complete** | Lifecycle Complete · Unprocessed=0 · histogram consistent |
| Transport **resolved** (refine sense) | Prior max-steps → **HitGeometry** or **BackgroundResolved** |
| Still **unresolved-budget** | Still `MaxStepsExhausted` |

A complete frame may be all max-steps or all background. That is still a complete measurement.

---

## What changed / why (reader exercise)

| If you change… | Frame should… |
|----------------|---------------|
| Camera / field / resolution / preset | New generation; old context **stale** |
| Opacity / NormalRGB only | Display only; plane unchanged |
| Refinement P | May update outcome + refinement_level for selected samples only |

---

## Interpretation boundary

!!! warning "Claim boundary"
    - Display ≠ probe.  
    - RGB/NormalRGB are presentation.  
    - Physics waiting ≠ transport failure.  
    - Lifecycle timeout ≠ MaxStepsExhausted.  
    - Do not invent channels that are not in the sealed frame.

---

## See also

- [Reading the Outcome Plane](reading_outcome_plane.md)  
- [Tuning the Cathedral Probe](../experiments/tuning_the_cathedral_probe.md)  
- Charter: `Docs/architecture/HelloObservatory_CathedralProbe_Stage0_ImplementationCharter.md`
