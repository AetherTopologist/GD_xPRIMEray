---
po_doc_type: experiment
title: Portal Seam
status: partial
engine_commit: "5ce15c13"
scene_id: portal_gallery_seam_v0
scene_class: evidence_qualification
instrument_tier: semantic
channel_id: cathedral.probe.outcome
source_channel_deps:
  - cathedral.probe.outcome
units: "ProbeOutcomeCode; presentation RGB for storyboards only"
validity_condition: "Presentation captures illustrate Display Modes; scientific seam claims need Complete outcome planes."
visualization_mapping: "NormalRGB and legacy magenta are display paths; outcome plane is authority."
claim_boundary: "Warped portal lettering shows simulated transport presentation is active—not physical wormhole confirmation. Magenta blocks are not MaxStepsExhausted."
evidence_links:
  - screenshots/2026-07-26 Canonical Portal Scene with GRIN Film Camera Diagnostics normals.png
  - screenshots/2026-07-26 Canonical Portal Scene with GRIN Film Camera Diagnostics normals_B.png
contract_links:
  - CathedralProbe/ProbeOutcomeCode.cs
last_qualified_run: not yet qualified
generated: false
---

# Portal Seam

!!! abstract "Observation card"
    | Field | Value |
    |-------|-------|
    | Status | **partial** |
    | Verified against | presentation captures 2026-07-26; sealed outcome path `6e69d792` lineage |
    | SceneId | `portal_gallery_seam_v0` |
    | SceneClass | evidence_qualification |
    | Channel IDs (authority) | `cathedral.probe.outcome` (+ region/refinement when probing) |
    | Display mapping | NormalRGB + host fallback (legacy magenta) |
    | Last qualified run | not yet qualified |
    | Claim boundary | No wormhole proof from glyphs or plate color |

---

## Scientific purpose

Study **seam presentation** and transport class structure near portal geometry under Gallery shell:

1. Is simulated transport **presentation** active (glyph warp, curved path)?
2. What does the **outcome plane** say (HitGeometry vs background vs max-steps)?
3. Are Unresolved Regions eligible for Region Probe?

Do **not** start from magenta flood-fill.

---

## Screenshots (role-labeled)

### Capture A — portal-facing plate

**File:** `screenshots/2026-07-26 Canonical Portal Scene with GRIN Film Camera Diagnostics normals.png`

**Roles in the capture:**

| Region of image | Role |
|-----------------|------|
| Large 3D room / portal mesh | **world viewport** |
| Film inset | **Observation Plate** |
| Brown/blue face fields on plate | **display mapping** (NormalRGB-like) |
| Grid-aligned magenta blocks | **legacy presentation** fallback—not semantic outcome plane |
| HUD legend “Magenta = no hit” (if visible) | **legacy presentation** chrome—**retired** as doctrine |

**Safe reading:** transport presentation can be active (warped portal lettering) while plate still shows large fallback fills. **Semantic classes are not determined from the image alone.**

### Capture B — alternate portal-facing plate

**File:** `screenshots/2026-07-26 Canonical Portal Scene with GRIN Film Camera Diagnostics normals_B.png`

Same roles: viewport + Observation Plate + display mapping + legacy magenta. Use as a second **presentation** example only.

---

## Operator path

1. Gallery shell · fixed portal-facing pose.
2. Transport Lens SNAPSHOT Complete.
3. Inspector: outcome histogram.
4. Display Mode NormalRGB for teaching orientation **only on HitGeometry context**.
5. Region Probe only from `region_label`, never from magenta.

Teaching sister page: [The Great Magenta Confusion](../learn/great_magenta_confusion.md).

---

## Interpretation boundary

!!! danger "Claim boundary"
    Portal Seam does **not** verify physical wormholes.
    Glyph warp ≠ outcome code.
    Magenta ≠ no hit.
    Fixture PASS elsewhere ≠ this live pose qualified.
