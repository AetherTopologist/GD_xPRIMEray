---
po_doc_type: learn
title: The Great Magenta Confusion
status: partial
engine_commit: "6e69d792"
scene_id: portal_gallery_seam_v0
scene_class: evidence_qualification
instrument_tier: semantic
instrument_id: SEM-OUT
channel_id: cathedral.probe.outcome
source_channel_deps: []
units: "display RGB vs ProbeOutcomeCode"
validity_condition: "Screenshots illustrate presentation hazards; scientific class requires outcome plane."
visualization_mapping: "Legacy SkyColor/magenta fallback is presentation; outcome overlay is planned/partial UI."
claim_boundary: "Magenta on the plate is not authoritative MaxStepsExhausted, no-hit, or region membership."
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
    | Status | **partial** (teaching + sealed outcome plane; UI legend may still say “no hit”) |
    | Verified against | `6e69d792` |
    | SceneId | `portal_gallery_seam_v0` |
    | SceneClass | evidence_qualification |
    | Channel ID (truth) | `cathedral.probe.outcome` |
    | Dependencies | transport outcomes, not TextureRect readback |
    | Units | codes vs 8-bit display |
    | Validity | Do not select regions from RGB |
    | Viz mapping | NormalRGB + legacy magenta fill |
    | Claim boundary | Display ≠ probe |
    | Evidence | live screenshots 2026-07-26 |

---

## Status

| Item | Status |
|------|--------|
| Outcome plane as authority | **implemented** |
| Live film can show grid-aligned magenta | **observed** |
| Legend “Magenta = no hit” on HUD | **legacy / to retire** |
| Outcome-driven hatch overlay | **partial / planned UI** |
| RGB flood-fill region select | **forbidden** |

---

## What the screenshots safely show

From the two portal-facing captures (Field ~0.95, Preview **80×45**, Opacity **40%**):

1. **Rectangular / grid-aligned magenta** — upscaled film blocks, not portal-contour-following.  
2. **Broad brown/blue NormalRGB-like fields** — coarse face orientation display.  
3. **Warped portal lettering (EARTH)** — transport/curved path is **active**.  
4. **Weak film seams vs clear world seams** — presentation/resolution limit.  
5. **80×45 + 40% opacity** — low spatial and contrast authority for class reading.

---

## What must not be concluded from RGB alone

| Forbidden | Why |
|-----------|-----|
| Magenta = no collider hit | Plane may be all hits / all background |
| Magenta = MaxStepsExhausted | Requires outcome code |
| Missing seam = transport failed | May be shading/res/background-resolved |
| White/blue = semantic mesh class | NormalRGB encodes **n**, not object labels |

Reconnaissance doctrine: **rendered magenta is not a trustworthy source for unresolved-region selection.**

---

## Corrected public wording

**Retire:** `Magenta = no hit`

**Prefer:**

```text
RGB / NormalRGB = display shading only
Legacy magenta blocks = film fallback presentation — not ProbeOutcomeCode
Unresolved-budget = MaxStepsExhausted on the outcome plane
Background = BackgroundResolved (completed non-surface)
Surface = HitGeometry
```

---

## What changed / why (teaching)

| Layer | Changes with field? | Authority |
|-------|---------------------|-----------|
| World mesh | No | Structure |
| Glyph warp on portal | Yes (transport) | Display of transport |
| Magenta rectangle | May persist | **Not** outcome truth |
| Outcome histogram | Yes/no depending on policy | **Authoritative** |

---

## Interpretation boundary

!!! danger "Claim boundary"
    Never build Cathedral Probe region selection from plate colors.  
    Never equate legacy magenta with a single semantic code without the plane.  
    Deeper refine is more evidence, not automatic geometry.
