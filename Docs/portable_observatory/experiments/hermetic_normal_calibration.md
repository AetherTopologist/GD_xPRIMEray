---
po_doc_type: experiment
title: Hermetic Normal Calibration
status: partial
engine_commit: "6e69d792"
scene_id: hermetic_chamber_v0
scene_class: instrument_calibration
instrument_tier: geometric
instrument_id: GEO-N
channel_id: null
source_channel_deps:
  - cathedral.probe.outcome
units: "normals dimensionless; depth scene-length when present; outcomes as codes"
validity_condition: "SNAPSHOT Complete; Unprocessed=0; prefer HitGeometry-dominant pose; FlipNormalToCamera documented."
visualization_mapping: "NormalRGB/NdotV/Depth are film shading modes (presentation). Geometry-hit mask from outcome plane. Raw normals only when trustworthy per-sample normals exist."
claim_boundary: "Face/collision normals and display mappings—not material normal maps, not spacetime curvature. Do not claim geometric-normal success without HitGeometry + valid normal data."
evidence_links:
  - Transport/HermeticRoomDisplay.tscn
contract_links:
  - CathedralProbe/ProbeOutcomeCode.cs
  - CathedralProbe/ProbeSeamDiagnosticPixel.cs
last_qualified_run: null
generated: false
---

# Hermetic Normal Calibration

!!! abstract "Observation card"
    | Field | Value |
    |-------|-------|
    | Status | **partial** |
    | Verified against | `6e69d792` |
    | SceneId | `hermetic_chamber_v0` |
    | SceneClass | instrument_calibration |
    | InstrumentTier | geometric (+ semantic gate) |
    | Channel IDs (now) | `cathedral.probe.outcome` (gate); display: NormalRGB / NdotV / Depth |
    | Channel IDs (planned) | portable `geo.normal`, `geo.depth`, `geo.ndotv` buffers |
    | Dependencies | Complete outcome plane before interpreting NormalRGB as “hits” |
    | Units | normal components dimensionless; depth scene units |
    | Validity | HitGeometry for orientation claims; sealed normals if claiming vector truth |
    | Viz mapping | NormalRGB = f(n); NdotV = f(n·v); Depth = f(distance) |
    | Claim boundary | No material nmap; no GR; presentation ≠ probe |

---

## Status of storyboard layers

| Step | Layer | Status |
|------|--------|--------|
| 1 | Viewport (hermetic box) | **implemented** (scene) |
| 2 | Completed outcome plane | **implemented** |
| 3 | Geometry-hit mask (`HitGeometry`) | **implemented** (semantic); UI overlay **partial** |
| 4 | Raw normal vectors | **partial / often unavailable** as sealed channel; film uses shading path |
| 5 | NormalRGB mapping | **implemented** as **display** shading |
| 6 | Wall-plane seams in NormalRGB | **partial** — depends on collision faceting + resolution |
| 7 | Depth shading | **implemented** as **display** mode |
| 8 | NdotV / two-sided NdotV | **implemented** as **display** mode |

---

## Scientific purpose

In a **closed box**, with pose fixed, separate:

1. **Whether transport hit geometry** (outcome plane).  
2. **How orientation is presented** (NormalRGB / NdotV).  
3. **Whether seams are visible** (resolution + normal quality + mask).  

If (1) shows HitGeometry but (2)/(3) look wrong, suspect **presentation or normal validity**, not “no hit.”

---

## Storyboard (progressive disclosure)

### 1. Viewport
**See:** blue structural walls, floor, ceiling; observer inside.  
**What changed:** nothing yet.  
**Why:** establish closed geometry (Blue A / structure).

### 2. Completed outcome plane
**See:** histogram / class map for full frame.  
**Expect:** high HitGeometry on walls/floor if closed and straight/mild field.  
**Gate:** lifecycle **Complete**, Unprocessed = 0.

### 3. Geometry-hit mask
**See:** binary or tint mask where code = `HitGeometry`.  
**Not:** RGB threshold.  
**Why:** only these samples may support orientation claims.

### 4. Raw normal vectors
**See:** per-hit (nx,ny,nz) if instrument exposes them.  
**Status:** treat as **planned/partial** for portable channel; seam diagnostics often mark normals unavailable.  
**If unavailable:** stop geometric-normal success claims.

### 5. NormalRGB mapping
**See:** film `n*0.5+0.5` per component.  
**Mapping only.** Large flat colors = large planar faces.

### 6. Wall-plane seams
**See:** color discontinuities where face normals jump.  
**Weak seams at 80×45** are expected; not automatic transport failure.  
**Compare** mask (3) to plate (5).

### 7. Depth
**See:** Distance-based shading on hits.  
**Why:** independent geometric channel from orientation.

### 8. NdotV
**See:** facing-brightness, not normal RGB.  
**Why:** teaching “same hits, different display.”

---

## Controls (default)

| Control | Setting |
|---------|---------|
| Display | Hermetic |
| Mode | SNAPSHOT Complete |
| Field | 0.00 STRAIGHT (baseline), then optional 0.95 |
| Shading cycle | NormalRGB → Depth → NdotV |
| Resolution | 80×45 then 160×90 |

---

## Acceptance (calibration)

- Complete plane; HitGeometry count stable under fixed pose.  
- Changing **only** shading changes plate, not outcome histogram.  
- No claim of material normal maps.  
- If raw normals missing: document **unavailable**, do not invent.

---

## Interpretation boundary

!!! warning "Claim boundary"
    Collision/face normals ≠ PBR normal maps.  
    Display ≠ probe.  
    Geometric-normal success requires HitGeometry **and** trustworthy normals—not NormalRGB alone.
