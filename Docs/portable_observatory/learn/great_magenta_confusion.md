---
po_doc_type: learn
title: The Great Magenta Confusion
status: partial
engine_commit: "5ce15c13"
scene_id: portal_gallery_seam_v0
scene_class: evidence_qualification
instrument_tier: semantic
instrument_id: SEM-OUT
channel_id: cathedral.probe.outcome
source_channel_deps: []
units: "display RGB vs ProbeOutcomeCode"
validity_condition: "Screenshots illustrate presentation hazards; scientific class requires outcome plane."
visualization_mapping: "Legacy SkyColor/magenta fallback is presentation; outcome overlay is planned/partial UI."
claim_boundary: "Magenta is a legacy fallback/presentation color whose meaning depends on the display path. Only ProbeOutcomeCode or the sealed outcome channel is authoritative."
evidence_links:
  - screenshots/2026-07-26 Canonical Portal Scene with GRIN Film Camera Diagnostics normals.png
  - screenshots/2026-07-26 Canonical Portal Scene with GRIN Film Camera Diagnostics normals_B.png
contract_links:
  - CathedralProbe/ProbeOutcomeCode.cs
last_qualified_run: null
generated: false
---

# The Great Magenta Confusion

!!! abstract "Observation card"
    | Field | Value |
    |-------|-------|
    | Status | **partial** (teaching + sealed outcome plane; legacy HUD may still mislead) |
    | Verified against | `5ce15c13` docs; captures 2026-07-26 |
    | SceneId | `portal_gallery_seam_v0` |
    | SceneClass | evidence_qualification |
    | Channel ID (truth) | `cathedral.probe.outcome` |
    | Dependencies | transport outcomes, not TextureRect readback |
    | Units | codes vs 8-bit display |
    | Validity | Do not select regions from RGB |
    | Viz mapping | NormalRGB + legacy magenta fill |
    | Claim boundary | Display ≠ probe |
    | Evidence | portal screenshots as **presentation examples only** |

---

## Retired wording

!!! danger "Retire"
    **Magenta = no hit**

This equation is **false as doctrine**. Magenta is a **legacy fallback / presentation color** whose meaning depends on the **display path**. It is not a `ProbeOutcomeCode`.

**Authoritative:** `ProbeOutcomeCode` or sealed channel `cathedral.probe.outcome` only.

---

## Corrected public wording

```text
RGB / NormalRGB / Depth / NdotV = Display Modes (presentation)
Legacy magenta blocks = host fallback fill — not ProbeOutcomeCode
Unresolved-budget = MaxStepsExhausted on the outcome plane
Background = BackgroundResolved (completed non-surface)
Surface = HitGeometry
```

---

## Status

| Item | Status |
|------|--------|
| Outcome plane as authority | **implemented** |
| Live plate can show grid-aligned magenta | **observed** |
| Legend “Magenta = no hit” on HUD | **legacy — retire from teaching** |
| Outcome-driven hatch overlay | **partial / planned UI** |
| RGB flood-fill region select | **forbidden** |

---

## Screenshots (presentation examples only)

### Portal A

**File:** `screenshots/2026-07-26 Canonical Portal Scene with GRIN Film Camera Diagnostics normals.png`

| Element | Caption role |
|---------|----------------|
| 3D room / portal | **world viewport** |
| Film inset | **Observation Plate** |
| Brown/blue fields | **display mapping** (NormalRGB-like) |
| Rectangular magenta | **legacy presentation** |
| Chrome / old legend | **legacy presentation** (not validated semantics) |

**Safe observations:** grid-aligned magenta (upscaled sample blocks); coarse orientation-like fields; possible glyph warp (transport presentation active); 80×45 + ~40% opacity reduce plate authority.

**Not established from color alone:** MaxStepsExhausted, no-hit, Unresolved Region membership.

### Portal B

**File:** `screenshots/2026-07-26 Canonical Portal Scene with GRIN Film Camera Diagnostics normals_B.png`

Same roles: viewport + Observation Plate + display mapping + legacy presentation. Second presentation example only.

---

## What must not be concluded from RGB alone

| Forbidden | Why |
|-----------|-----|
| Magenta = no collider hit | Plane may be all hits / all background |
| Magenta = MaxStepsExhausted | Requires outcome code |
| Missing seam = transport failed | May be shading/res/background-resolved |
| White/blue = semantic mesh class | NormalRGB encodes **n**, not object labels |

Reconnaissance doctrine: **rendered magenta is not a trustworthy source for Unresolved Region selection.**

---

## What changed / why (teaching)

| Layer | Changes with field? | Authority |
|-------|---------------------|-----------|
| World mesh | No | Structure |
| Glyph warp on portal | Yes (transport presentation) | Display of simulated transport |
| Magenta rectangle | May persist | **Not** outcome truth |
| Outcome histogram | Policy-dependent | **Authoritative** |

---

## Interpretation boundary

!!! danger "Claim boundary"
    Never build Region Probe selection from plate colors.
    Never equate legacy magenta with a single semantic code without the plane.
    Deeper refine is more Transport Effort, not automatic geometry.
    Region Probe (Cathedral architecture) does not prove wormholes.
