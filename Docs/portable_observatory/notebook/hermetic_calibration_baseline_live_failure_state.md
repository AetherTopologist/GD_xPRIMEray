---
po_doc_type: notebook
title: Hermetic Calibration Baseline — Live Failure State
status: partial
engine_commit: "6e69d792"
scene_id: hermetic_chamber_v0
scene_class: instrument_calibration
instrument_tier: geometric
instrument_id: GEO-N
channel_id: cathedral.probe.outcome
source_channel_deps: []
units: "presentation RGB only in this entry; no sealed histogram attached"
validity_condition: "Screenshots are display evidence only. No full-frame scientific claim without Complete snapshot + outcome plane."
visualization_mapping: "Observation Plate as host presentation (NormalRGB/Depth labels may appear in chrome); not ProbeOutcomeCode."
claim_boundary: "Do not interpret magenta as an outcome code. Do not claim hits, normals, context match, or field responsibility from these plates alone."
evidence_links:
  - screenshots/2026-07-26 Canonical Portal Scene with GRIN Film Camera Diagnostics normals.png
  - screenshots/2026-07-26 Canonical Portal Scene with GRIN Film Camera Diagnostics normals_B.png
contract_links:
  - CathedralProbe/ProbeOutcomeCode.cs
  - Docs/portable_observatory/experiments/hermetic_normal_calibration.md
  - Docs/portable_observatory/learn/great_magenta_confusion.md
last_qualified_run: null
generated: false
notebook_correction_of: null
---

# Hermetic Calibration Baseline — Live Failure State

**Notebook type:** live failure / presentation evidence
**Date:** 2026-07-26 (captures) · entry prepared 2026-08-01 · ontology pass note 2026-08-03
**Engine commit (entry baseline):** `6e69d792`
**Doctrine:** Display ≠ probe. Outcome-plane semantics outrank RGB. Magenta on the plate is **not** a `ProbeOutcomeCode`.

---

## Question

Can these live captures establish a **Hermetic Normal Calibration** baseline (closed box → HitGeometry → valid normals → multi-face NormalRGB seams)?

---

## Experiment identity

| Field | Value |
|-------|--------|
| Intent | Establish Hermetic Normal Calibration baseline |
| Actual result | **Live failure state** relative to that intent |
| Evidence class | **Presentation only** (two live screenshots) |
| Sealed outcome plane for these frames | **Not attached to this notebook entry** |
| SceneIds involved (host) | Gallery / portal-facing (captures); Hermetic intended for calibration ladder below |

**Presentation evidence (paths):**

1. `screenshots/2026-07-26 Canonical Portal Scene with GRIN Film Camera Diagnostics normals.png`
2. `screenshots/2026-07-26 Canonical Portal Scene with GRIN Film Camera Diagnostics normals_B.png`

Chrome visible in captures includes roughly: Film LIVE · Preview 80×45 · Opacity ~40% · Field ~0.95 · exploratory legend including legacy “Magenta = no hit” wording (treat as **legacy presentation chrome**, not validated semantics).

---

## DIRECT OBSERVATIONS

From the two supplied live screenshots **as pixels and UI chrome only**:

| # | Observation | Caption role |
|---|-------------|--------------|
| 1 | **Gallery viewport** shows **visible scene geometry** (walls, portal surface, lettering / room structure depending on frame). | **world viewport** |
| 2 | **Gallery film plate** is **predominantly magenta**, with a **narrow gray (or non-magenta) region** along part of the plate—not a multi-face orientation partition. | **Observation Plate** + **legacy presentation** |
| 3 | **Hermetic viewport** (when shown / compared in the live session narrative) is **nearly white**—enclosure structure is **not** clearly readable as a multi-wall calibration cage in that presentation. | **world viewport** |
| 4 | **Hermetic film plate** is **predominantly magenta**. | **Observation Plate** + **legacy presentation** |
| 5 | **No clean multi-face NormalRGB partition** is visible on either film plate. | **display mapping** (failed teaching read) |

**Also directly visible as presentation constraints:** plate at **80×45** upscaled; **~40%** opacity softens plate authority against the 3D view; portal glyphs can appear **warped** in Gallery (transport presentation active)—still **not** an outcome-plane class.

---

## INTERPRETATION

**Live failure state:** presentation shows **magenta-dominant plates** and a **near-white Hermetic viewport**, so the session **cannot** be used as a successful Hermetic normal-calibration baseline until the experiment ladder below is executed with sealed evidence.

This is a **qualification failure of the live baseline**, not a proof of transport absence (warped portal lettering in Gallery still suggests active transport presentation).

---

## NOT ESTABLISHED

| Question | Why not established |
|----------|---------------------|
| Whether rays **hit Hermetic geometry** | Requires `cathedral.probe.outcome` with `HitGeometry` counts; RGB magenta does not decide this |
| Whether **normals exist** / are valid | Requires trustworthy per-sample normals + typically HitGeometry |
| Whether **camera contexts match** | Requires context key / lifecycle record per frame |
| Whether **magenta means no hit** | **Forbidden inference**; legacy legend is not the outcome plane |
| Whether **field curvature** is responsible for magenta plates | Requires Straight vs field-on under Complete snapshots + histograms |

---

## CORRECTION NOTES

| Prior / risky reading | Correction |
|-----------------------|------------|
| HUD “Magenta = no hit” | **Retired as doctrine**; presentation chrome only |
| Magenta plate ⇒ MaxStepsExhausted | Requires sealed outcome plane |
| Field ~0.95 caused failure | Unproven without histogram delta |
| NormalRGB “proves” faces | Display mapping only |

Post-`36b003bd` (exclude observer from film transport): later sessions may show **increased structured room hits** after self-intersection correction—still require sealed histograms; this notebook’s captures predate or do not attach that evidence.

---

## Experiment ladder (required next)

Execute in order. **Do not skip to NormalRGB interpretation.**
**Do not interpret magenta as an outcome code** at any step.

### 1. Six-axis straight-ray enclosure test

| | |
|--|--|
| **Goal** | Confirm the observer is **inside** a closed collision/visual enclosure (Hermetic primary). |
| **Pass signal** | Consistent interior hits on all six axes under **straight** policy. |
| **Fail signal** | Escape to void, exterior wall face, or no enclosure contact. |

### 2. Completed FieldStrength = 0 snapshot

| | |
|--|--|
| **Goal** | Probe-complete SNAPSHOT with Straight transport. |
| **Pass signal** | Lifecycle **Complete**; Unprocessed = 0; histogram sums to total. |
| **Note** | Complete ≠ “pretty NormalRGB.” |

### 3. Semantic outcome histogram

| | |
|--|--|
| **Channel** | `cathedral.probe.outcome` |
| **Report** | Full code counts |
| **Forbidden** | Inferring histogram from plate magenta |

### 4. Valid-normal census

Among **HitGeometry** only. If unavailable: record **unavailable**; do not claim geometric-normal success.

### 5. NormalRGB encoding comparison

Compare Display Mode NormalRGB **only on HitGeometry** (and valid normals if known). Prefer 160×90 and 100% opacity for evidence stills.

### 6. Field-on repeat

Same pose; only field strength changes. Report semantic delta—not “magenta because field.”

---

## UNRESOLVED QUESTIONS

- Sealed multi-face NormalRGB success under Hermetic still **not yet qualified**.
- Whether raw `geometry.normal` ships before UI hatch overlays.
- How much of the 2026-07-26 magenta dominance was self-intersection vs fallback vs true max-steps.

---

## CLAIM BOUNDARY

| Allowed from these two screenshots | Disallowed |
|--------------------------------------|------------|
| Direct observation list above | Magenta ⇒ no hit / max-steps |
| “Baseline not qualified for Hermetic Normal Calibration” | Rays miss Hermetic geometry (unproven) |
| Need for experiment ladder 1–6 | Field curvature caused magenta (unproven) |
| Display ≠ probe reminder | Context match Gallery↔Hermetic (unproven) |

---

## Relation to other notes

- Calibration intent: [Hermetic Normal Calibration](../experiments/hermetic_normal_calibration.md)
- Magenta teaching: [The Great Magenta Confusion](../learn/great_magenta_confusion.md)
- Outcome reading: [Reading the Outcome Plane](../learn/reading_outcome_plane.md)

---

## One-line conclusion

**Live presentation shows magenta-dominant Observation Plates and an unreadable Hermetic viewport; until the six-step ladder produces a Complete outcome plane and (if available) a valid-normal census, this session is a failed Hermetic normal-calibration baseline, not a semantic transport diagnosis.**

*Display ≠ probe. Magenta is not an outcome code.*
