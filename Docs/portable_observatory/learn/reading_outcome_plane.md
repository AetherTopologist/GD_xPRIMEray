---
po_doc_type: learn
title: Reading the Outcome Plane
status: implemented
engine_commit: "5ce15c13"
scene_id: null
scene_class: null
instrument_tier: semantic
instrument_id: SEM-OUT
channel_id: cathedral.probe.outcome
source_channel_deps: []
units: "ProbeOutcomeCode (enum)"
validity_condition: "Full-frame reading requires Complete snapshot and Unprocessed=0. Per-pixel codes always carry local meaning only."
visualization_mapping: "Optional class overlay / hatch from codes; never flood-fill from RGB magenta."
claim_boundary: "Outcome codes classify transport termination under policy; they are not physical wormhole proof and not OI fixture PASS."
evidence_links: []
contract_links:
  - CathedralProbe/ProbeOutcomeCode.cs
  - CathedralProbe/ProbeFrameSummary.cs
  - CathedralProbe/CathedralProbeObservationFrameAdapter.cs
last_qualified_run: null
generated: false
---

# Reading the Outcome Plane

!!! abstract "Observation card"
    | Field | Value |
    |-------|-------|
    | Status | **implemented** (codes + plane + sealed channel) |
    | Verified against | sealed path `6e69d792`; docs `5ce15c13` |
    | SceneId | any scene that emits Region Probe outcomes |
    | Channel ID | `cathedral.probe.outcome` |
    | Dependencies | transport pass-1 / refine apply (not RGB) |
    | Units | `code` |
    | Validity | Complete snapshot for full-frame claims |
    | Viz mapping | categorical overlay (display) |
    | Claim boundary | Semantic classification only |

---

## Status

| Item | Status |
|------|--------|
| `ProbeOutcomeCode` vocabulary | **implemented** |
| Dense outcome plane + frame summary | **implemented** |
| Sealed channel `cathedral.probe.outcome` | **implemented** |
| Public multi-class Observation Plate overlay | **partial** |
| RGB as selection source | **forbidden** |

---

## Vocabulary (authoritative)

| Code | Notebook class | One-line meaning |
|------|----------------|------------------|
| `Unprocessed` | incomplete | Sample not written; plane not full-frame ready |
| `HitGeometry` | surface | Geometry/matter contact under model |
| `BackgroundResolved` | background | Completed non-surface termination |
| `MaxStepsExhausted` | **unresolved-budget** | Stopped at Transport Effort budget |
| `StoppedEarlyAbsorbed` | **unresolved-secondary** | Early absorb/stop (not “resolved”) |
| `NumericalFailure` | **fault** | Numerical guard |
| `Invalid` | **invalid** | Invalid sample/context |

### Refinement “resolved” (strict)

A sample is **resolved by refinement** only if it was **`MaxStepsExhausted` before** and becomes **`HitGeometry` or `BackgroundResolved` after**.

Not resolved: still max-steps, absorbed-secondary, fault, invalid.

---

## How to read a histogram

Example (synthetic teaching numbers, not a live run):

| Code | Count |
|------|------:|
| HitGeometry | 1200 |
| BackgroundResolved | 2000 |
| MaxStepsExhausted | 400 |
| Other | 0 |
| **Sum** | **3600** |

Checks:

1. Sum = total samples (e.g. 80×45 = 3600, 160×90 = 14400).
2. Unprocessed = 0 for “complete plane.”
3. Large BackgroundResolved is **allowed** and can coexist with a magenta-looking plate.

---

## What the plane is for

- Region analysis (**Unresolved Regions** by default rules).
- Region Refinement targeting.
- Evidence export and host-neutral records.

## What the plane is not for

- Declaring NormalRGB “wrong” without comparing to HitGeometry + normals.
- Selecting regions by picking pink pixels.
- Proving wormholes from remaps alone.

---

## Related channels

| Channel ID | Role | Status |
|------------|------|--------|
| `cathedral.probe.region_label` | Connected labels from semantic plane | **implemented** |
| `cathedral.probe.refinement_level` | Effort history per sample | **implemented** |

---

## Interpretation boundary

!!! warning "Claim boundary"
    Outcome-plane semantics outrank RGB.
    Deeper refinement = more Transport Effort, not more truth.
    BackgroundResolved is a completed class, not “empty universe.”
    Semantic outcome classification under simulated transport—not physical wormhole proof.
