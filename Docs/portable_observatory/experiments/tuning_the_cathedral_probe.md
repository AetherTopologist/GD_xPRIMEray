---
po_doc_type: experiment
title: Tuning the Cathedral Probe
status: partial
engine_commit: "6e69d792"
scene_id: portal_gallery_seam_v0
scene_class: evidence_qualification
instrument_tier: semantic
instrument_id: SEM-OUT
channel_id: cathedral.probe.outcome
source_channel_deps:
  - cathedral.probe.outcome
  - cathedral.probe.region_label
  - cathedral.probe.refinement_level
units: "codes; counts; levels; planned heatmaps in steps/length"
validity_condition: "SNAPSHOT Complete; Unprocessed=0; RGB never used for selection; refine only on valid context."
visualization_mapping: "Categorical masks from outcome/region/level channels; continuous heatmaps planned from transport records when sealed."
claim_boundary: "Tuning adjusts probe policy and reading of sealed channels; not a claim of physical truth convergence."
evidence_links: []
contract_links:
  - CathedralProbe/ProbeRegionRefinementEngine.cs
  - CathedralProbe/ProbeRefinementSummary.cs
  - CathedralProbe/ProbeSnapshotLifecycleModel.cs
  - CathedralProbe/CathedralProbeObservationFrameAdapter.cs
last_qualified_run: null
generated: false
---

# Tuning the Cathedral Probe

!!! abstract "Observation card"
    | Field | Value |
    |-------|-------|
    | Status | **partial** |
    | Verified against | `6e69d792` (plane/refine/lifecycle sealed path) |
    | SceneId | `portal_gallery_seam_v0` (primary); hermetic secondary |
    | SceneClass | evidence_qualification |
    | Channels (now) | `cathedral.probe.outcome`, `.region_label`, `.refinement_level` |
    | Channels (planned) | final-step-count, path-length, field strength map |
    | Dependencies | Complete SNAPSHOT outcome plane |
    | Units | enum / label / level; heatmaps TBD |
    | Validity | Context match for refine; no RGB select |
    | Viz mapping | overlays from sealed channels only |
    | Claim boundary | Deeper ≠ truer; complete ≠ “solved” |

---

## Purpose

Teach operators (and future hosts) how to **tune reading and effort** on the Cathedral Probe without confusing:

- **categorical semantics** (what class is this sample?),
- **budget unresolved** (did we stop for steps?),
- **continuous transport effort** (how hard did we work?—when available),
- **refinement history** (how many deepenings?).

---

## Channel / view matrix

| View | What it shows | Exists now? | Source |
|------|----------------|-------------|--------|
| **Categorical outcome mask** | Per-sample `ProbeOutcomeCode` | **implemented** (plane + sealed channel); UI overlay **partial** | `cathedral.probe.outcome` |
| **Max-step exhaustion mask** | Binary / hatch where code = `MaxStepsExhausted` | **implemented** as filter on outcome; UI **partial** | derived from outcome |
| **Region labels** | Connected unresolved-budget components | **implemented** | `cathedral.probe.region_label` |
| **Refinement-level history** | Per-sample L | **implemented** | `cathedral.probe.refinement_level` |
| **Continuous final-step-count heatmap** | Steps used at termination | **planned** (not a sealed Cathedral channel yet) | future transport record |
| **Path-length heatmap** | Integrated path length | **planned** | future transport record |
| **NormalRGB / Depth / NdotV** | Display shading | **implemented** presentation | film shading — **not** probe truth |
| **Legacy magenta fill** | Film fallback presentation | **observed** | display — **not** outcome |

!!! tip "Tuning order"
    1) Qualify SNAPSHOT complete → 2) read outcome histogram → 3) max-step mask / regions → 4) refine (P) → 5) only then ask for continuous heatmaps when channels ship.

---

## Distinguish the five diagnostic ideas

### 1. Categorical outcome mask
- **Variable:** enum class per pixel.  
- **Use:** who is surface / background / unresolved-budget / fault.  
- **Not:** intensity of effort.

### 2. Max-step exhaustion mask
- **Variable:** boolean `outcome == MaxStepsExhausted`.  
- **Use:** default **unresolved-budget** region seeds.  
- **Not:** all magenta RGB pixels.

### 3. Continuous final-step-count heatmap *(planned)*
- **Variable:** steps used (count).  
- **Use:** budget pressure; compare pixels under same policy.  
- **Not:** coordinate time.  
- **Status:** **planned** — do not claim implemented.

### 4. Path-length heatmap *(planned)*
- **Variable:** integrated path length (scene length).  
- **Use:** geometric effort along path.  
- **Not:** proper time unless engine defines it.  
- **Status:** **planned**.

### 5. Refinement-level history
- **Variable:** level L per sample.  
- **Use:** where extra probe effort was applied.  
- **Not:** truer physics.  
- **Status:** **implemented** as sealed channel.

---

## Operator sequence (Stage 0)

1. Freeze pose · set Field · **SNAPSHOT** until Complete.  
2. Confirm Unprocessed = 0 (lifecycle ≠ pretty frame alone).  
3. Read **outcome** histogram (categorical).  
4. If max-step regions exist: select largest (default) · outline from **region_label**.  
5. **P** refine once · read summary:  
   - **resolved** = → HitGeometry or BackgroundResolved only  
   - remaining max-steps = unresolved-budget  
   - absorbed / fault / invalid counted separately  
6. Toggle display shading (N) — histogram must **not** change.  
7. Change field or move → **STALE** · new SNAPSHOT.

---

## Controls (do not overload)

| Key | Role |
|-----|------|
| G | Film / SNAPSHOT |
| P | Refine selected region |
| J / K | Cycle regions *(when wired)* |
| R | Reset refinement *(when wired)* |
| , . 0 1 | Field |
| N | Display shading only |
| Tab | Telemetry / histograms |
| H | Display preset (invalidates context) |

---

## Evidence to record when tuning

- `ProbeFrameSummary` before/after refine  
- `ProbeRefinementSummary` (atomic flag, transitions)  
- Context key + dimensions  
- Explicit note: RGB not used  

---

## Interpretation boundary

!!! warning "Claim boundary"
    - Categorical mask ≠ continuous effort heatmap.  
    - Max-step mask ≠ legacy magenta.  
    - Step/path heatmaps are **planned**; labeling them “time” or “causality” is forbidden until defined.  
    - Complete all-max-steps plane is still a complete snapshot.  
    - Deeper ≠ truer.
