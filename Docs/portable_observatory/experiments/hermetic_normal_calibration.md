---
po_doc_type: experiment
title: Hermetic Normal Calibration
status: partial
engine_commit: "5ce15c13"
scene_id: hermetic_chamber_v0
scene_class: instrument_calibration
instrument_tier: geometric
instrument_id: GEO-N
channel_id: null
source_channel_deps:
  - cathedral.probe.outcome
units: "normals dimensionless; depth scene-length when present; outcomes as codes"
validity_condition: "SNAPSHOT Complete; Unprocessed=0; prefer HitGeometry-dominant pose; FlipNormalToCamera documented when used."
visualization_mapping: "NormalRGB/NdotV/Depth are Display Modes (presentation). Geometry-hit mask from outcome plane. Raw normals only when trustworthy per-sample normals exist."
claim_boundary: "Face/collision normals and display mappings—not material normal maps, not spacetime curvature. Status remains partial until raw normal channels and qualified live multi-face results exist."
evidence_links:
  - Transport/HermeticRoomDisplay.tscn
contract_links:
  - CathedralProbe/ProbeOutcomeCode.cs
  - CathedralProbe/ProbeSeamDiagnosticPixel.cs
last_qualified_run: not yet qualified
generated: false
---

# Hermetic Normal Calibration

!!! abstract "Observation card"
    | Field | Value |
    |-------|-------|
    | Status | **partial** |
    | Verified against | docs ontology `5ce15c13`; sealed outcome `6e69d792`; observer-exclude film `36b003bd` |
    | SceneId | `hermetic_chamber_v0` |
    | SceneClass | instrument_calibration |
    | InstrumentTier | geometric (+ semantic gate) |
    | Channel IDs (now) | `cathedral.probe.outcome` (gate); Display Modes: NormalRGB / NdotV / Depth |
    | Channel IDs (planned) | `geometry.normal`, `geometry.depth`, `geometry.ndotv` |
    | Dependencies | Complete outcome plane before interpreting NormalRGB as “hits” |
    | Units | normal components dimensionless; depth scene units |
    | Validity | HitGeometry for orientation claims; sealed normals if claiming vector truth |
    | Display mapping | NormalRGB = f(n); NdotV = f(n·v); Depth = f(distance) |
    | Claim boundary | No material nmap; no GR; presentation ≠ probe |
    | Last qualified run | not yet qualified |

---

## Why this room is a classroom

The hermetic enclosure is for teaching:

1. **Closure** — known wall planes; observer inside a closed box under a stated test.
2. **Orientation** — distinct face normals (floor / walls / ceiling) when hits and normals are valid.
3. **Policy contrast** — **straight** vs **field-on** comparison at fixed pose.
4. **Calibration failure modes** — e.g. **player self-intersection** as a failure (rays hitting the observer avatar) rather than “missing the room.”

It is **not** a beauty stage and not a wormhole proof.

---

## Status of storyboard layers

| Step | Layer | Status |
|------|--------|--------|
| 1 | Viewport (hermetic box) | **implemented** (scene) |
| 2 | Completed outcome plane | **implemented** |
| 3 | Geometry-hit mask (`HitGeometry`) | **implemented** (semantic); UI overlay **partial** |
| 4 | Raw normal vectors | **planned / partial** as sealed `geometry.normal`; host may shade without sealing |
| 5 | NormalRGB mapping | **available as Display Mode** — not sealed channel |
| 6 | Wall-plane seams in NormalRGB | **partial** — faceting + resolution + validity |
| 7 | Depth shading | **available as Display Mode** |
| 8 | NdotV | **available as Display Mode** |
| 9 | Qualified live multi-face result | **not yet qualified** |

**Current status remains partial** until raw normal channels and qualified live multi-face results exist.

---

## Separate four questions

| Question | Authority |
|----------|-----------|
| Did transport **hit geometry**? | Outcome plane (`HitGeometry`) |
| Is the **normal valid**? | Per-sample normal validity (when available) |
| How is orientation **displayed**? | Display Mode (NormalRGB / NdotV) |
| Does the **enclosure close**? | Closure / six-axis enclosure test—not plate magenta |

If (1) shows HitGeometry but (3) looks wrong, suspect **presentation or normal validity**, not “no hit.”

---

## Straight vs field-on

| Setting | Use |
|---------|-----|
| Field **0** (STRAIGHT) | Baseline enclosure + orientation classroom |
| Field mid / **1** (FULL) | Same pose; compare **semantic** histogram and (if justified) display mapping deformation |

Field dial ≠ evaluated `field.magnitude` channel (planned).

After observer self-intersection correction (`36b003bd` lineage), expect **increased structured room hits** when the plate no longer samples the avatar—still confirm with the outcome plane, not color alone.

---

## Storyboard (progressive disclosure)

### 1. Viewport
**See:** structural walls, floor, ceiling; observer inside.
**Role:** world viewport.
**Why:** establish closed geometry.

### 2. Completed outcome plane
**See:** histogram / class map for full frame.
**Expect:** high HitGeometry on walls/floor if closed and straight/mild field.
**Gate:** lifecycle **Complete**, Unprocessed = 0.
**Role:** semantic outcome plane (data—not RGB).

### 3. Geometry-hit mask
**See:** mask where code = `HitGeometry`.
**Not:** RGB threshold.

### 4. Raw normal vectors
**Status:** treat as **planned/partial** for portable channel.
**If unavailable:** stop geometric-normal success claims.

### 5. NormalRGB mapping
**See:** Observation Plate under NormalRGB Display Mode.
**Mapping only.** Large flat colors ≈ large planar faces **when** hits+normals valid.

### 6. Wall-plane seams
**See:** color discontinuities where face normals jump.
Weak seams at 80×45 are expected—not automatic transport failure.

### 7–8. Depth / NdotV
Same hits, different Display Modes—teaching **Display ≠ Instrument**.

---

## Controls (default)

| Control | Setting |
|---------|---------|
| Display shell | Hermetic |
| Mode | SNAPSHOT Complete |
| Field | 0.00 STRAIGHT baseline, then optional field-on |
| Display Mode cycle | NormalRGB → Depth → NdotV |
| Resolution | 80×45 then 160×90 for evidence stills |

---

## Acceptance (calibration)

- Complete plane; HitGeometry count stable under fixed pose.
- Changing **only** Display Mode changes plate, not outcome histogram.
- No claim of material normal maps.
- If raw normals missing: document **unavailable**, do not invent.
- Multi-face partition **not yet qualified** as a public live success until sealed evidence exists.

Failure notebook (presentation-only baseline): [Hermetic live failure state](../notebook/hermetic_calibration_baseline_live_failure_state.md).

---

## Interpretation boundary

!!! warning "Claim boundary"
    Collision/face normals ≠ PBR normal maps.
    Display ≠ probe.
    NormalRGB does not prove semantic geometry resolution alone.
    Closure is policy-relative enclosure—not exotic topology.
    Geometric-normal success requires HitGeometry **and** trustworthy normals.
