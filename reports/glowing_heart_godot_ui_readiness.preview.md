# Project Glowing Heart — Godot Demo / UI Readiness Proposal (Preview)

Generated: 2026-06-27T00:00:00Z

Runtime executed: false

Parity claim: NONE

Validation claim: NONE

Readiness verdict: **SAFE_WITH_LIMITS** — suitable for perceptual Godot demo framing; **not** ready for parity, pixel equivalence, or scientific validation claims.

---

## Purpose

This proposal defines claim-safe Godot UI language, overlay text, panel layout, and screenshot guidance for the selected public demo candidate. It supports **Stream B — Godot Observatory UI** and **Layer 5 — Public Interface** from the Glowing Heart architecture vision.

No code or scenes are modified here. This is a language and layout specification only.

---

## Selected demo candidate

| Field | Value |
|-------|-------|
| Scene | `Fixtures/fixture_hermetic_observatory_grin.tscn` |
| Name | `fixture_hermetic_observatory_grin` |
| Rank | 1 |
| Demo safety | SAFE_WITH_LIMITS |
| Tags | fixture, grin, hermetic, curved, observatory |
| Transport hint | GRIN field-driven curved-ray transport |
| Claim boundary | Visual / perceptual demo only; no parity or validation claim |

**Why this scene:** Selected by the shared fixture bridge as the Godot-side GRIN observatory candidate. It is the same fixture used in observer-bridge measurement work (v1.8.x), which helps visitors connect the Godot view to documented engineering progress — without implying Core/Godot agreement.

---

## What the demo is

Use this framing consistently in titles, captions, and panel headers.

> **Project Glowing Heart — GRIN Observatory Preview**  
> Experimental observatory visualization. Perceptual demonstration only. No parity or validation claim.

The demo shows:

- A **Godot observatory shell** exploring a sealed GRIN field scene.
- **Curved-ray transport visualization** as an engineering prototype — how rays bend through a refractive field in this scene setup.
- **Active bridge-candidate work** — the same fixture family used to define and measure the Core/Godot observer contract (v1.8 milestone).
- A **work-in-progress engineering initiative** inside xPRIMEray, not a finished product demo.

Visitor-friendly one-liner:

> *See how xPRIMEray visualizes curved-ray transport in a controlled GRIN observatory scene — an engineering preview, not a physics proof.*

---

## What the demo is not

State these boundaries visibly. Do not bury them in footnotes.

| Misunderstanding risk | Correct boundary label |
|-----------------------|------------------------|
| "This proves wormhole physics" | Not a physics validation demo |
| "Core and Godot match" | No Core/Godot parity claim |
| "Pixels are equivalent" | Pixel comparison not ready (`pixel_comparison_ready=false`) |
| "Hermetic closure is proven" | Closure status not demonstrated here |
| "This is scientifically confirmed" | No scientific confirmation or institutional endorsement |
| "This replaces the Core instrument" | Godot is the observatory shell; Core is the instrument |

Permanent disclaimer block (recommended for every public screenshot and page):

```txt
Experimental observatory visualization.
Perceptual demonstration only.
No parity or validation claim.
Engineering prototype — work in progress.
```

---

## Primary status banner (always visible)

Place a persistent banner at the top of the viewport or screenshot frame. High contrast, readable at thumbnail size.

**Banner text (recommended):**

```txt
EXPERIMENTAL OBSERVATORY VISUALIZATION  ·  PERCEPTUAL DEMO ONLY  ·  NO PARITY OR VALIDATION CLAIM
```

**Secondary line (smaller, optional):**

```txt
Godot observatory shell  ·  Observer bridge measured, pixel comparison not ready
```

**Do not use on the banner:** proof, validated, matches, parity, confirmed, complete.

---

## Recommended screenshot layout

### Layout A — Single Godot viewport (recommended for v2.2)

Use this until a formal side-by-side packet exists (architecture vision v2.3).

```
┌─────────────────────────────────────────────────────────────────────────┐
│ STATUS BANNER — Experimental observatory visualization…               │
├──────────────────────────────────────┬──────────────────────────────────┤
│                                      │  OBSERVER PANEL                  │
│                                      │  View: Godot Camera3D            │
│         MAIN VIEWPORT                │  Bridge status: measured, not    │
│    (Presentation mode — rays only)   │    aligned for pixel compare     │
│                                      ├──────────────────────────────────┤
│                                      │  FIELD PANEL                     │
│                                      │  GRIN field · sealed box scene   │
│                                      ├──────────────────────────────────┤
│                                      │  SNAPSHOT PANEL                  │
│                                      │  Godot runtime view · no Core    │
│                                      │  artifact shown in this packet   │
├──────────────────────────────────────┴──────────────────────────────────┤
│ LIMITATIONS STRIP — No parity · No pixel equivalence · No validation   │
├─────────────────────────────────────────────────────────────────────────┤
│ FOOTER — Fixture name · capture date · Project Glowing Heart · WIP     │
└─────────────────────────────────────────────────────────────────────────┘
```

### Layout B — Future side-by-side (v2.3 only, not yet)

When Core and Godot artifacts are shown together, **never** imply equivalence.

```
┌──────────────────────┬──────────────────────┐
│  CORE ARTIFACT       │  GODOT OBSERVATORY   │
│  (instrument output) │  (visualization shell)│
│  Label: NOT COMPARED │  Label: NOT COMPARED │
└──────────────────────┴──────────────────────┘
         ▲                        ▲
    separate captions        separate captions
    no "match" language      no "match" language
```

**Current recommendation:** Do **not** publish Layout B yet. Observer alignment gaps remain on both sides (v1.8.3). Showing two images side by side without strong disclaimers invites parity misread.

### Capture settings

| Setting | Recommendation | Reason |
|---------|----------------|--------|
| Observatory mode | **Presentation** (Ctrl+6) | Rays only; no grid, crosshair, or diagnostic noise |
| Overlays | Minimal | Reduces "validated instrument" appearance |
| Resolution | Native capture; note dimensions in footer | Supports future observer contract work |
| Date stamp | Required | Signals freshness; avoids stale-screenshot proof misread |
| Source link | Link to preview artifacts | Connects demo to inspectable engineering trail |

---

## Recommended UI panels

Four panels keep visitor comprehension high while staying claim-safe. Use short labels; avoid jargon in headings.

### 1. Observer panel

**Panel title:** `Observer (Preview)`

**Purpose:** Show which camera/view produced the image without claiming Core/Godot agreement.

| Label | Safe text | Notes |
|-------|-----------|-------|
| View source | `Godot Camera3D` | Never label "shared observer" until aligned |
| Bridge status | `Observer bridge defined and measured` | Accurate per v1.8 |
| Comparison status | `Pixel comparison not ready` | Required honesty |
| Parity | `None claimed` | Mirror `parity_claim=NONE` |
| Alignment note | `Core and Godot observer fields still differ` | Prevents "same camera" assumption |

**Avoid:** "Matched observer," "Aligned camera," "Equivalent viewpoint," "Validated pose."

**Tooltip (optional):**  
`The observer bridge names the camera contract both sides must meet before pixel comparison. Measurement exists; alignment is incomplete.`

---

### 2. Field panel

**Panel title:** `Field (GRIN Preview)`

**Purpose:** Describe the scene's refractive setup without physics-proof language.

| Label | Safe text | Notes |
|-------|-----------|-------|
| Scene type | `Sealed GRIN observatory` | Matches fixture metadata |
| Transport mode | `Curved-ray GRIN transport` | Concept label, not proof |
| Field role | `Refractive field visualization` | Not "physically complete" |
| Fixture | `fixture_hermetic_observatory_grin` | Grounds demo in named artifact |

**Avoid:** "Proven field," "Validated GRIN," "Physically accurate," "Hermetic closure confirmed."

**Tooltip (optional):**  
`This scene exercises GRIN field visualization in a sealed observatory layout. It is a perceptual engineering preview.`

---

### 3. Snapshot panel

**Panel title:** `Snapshot (Godot View)`

**Purpose:** Clarify what the visitor is looking at and what artifact type it is not.

| Label | Safe text | Notes |
|-------|-----------|-------|
| Output type | `Godot runtime viewport` | Distinguish from Core CLI artifact |
| Core artifact | `Not shown in this demo packet` | Until v2.3 packet exists |
| Channel | `Visual preview only` | No snapshot channel contract claim yet |
| Determinism | `Not a parity reference frame` | Blocks equivalence misread |

**Avoid:** "Pixel snapshot," "Reference render," "Ground truth," "Matches Core output."

**If showing Core `snapshot.ppm` elsewhere on a progress page:**  
Caption it separately as `Core CLI preview artifact — not compared to Godot in this demo`.

---

### 4. Limitations panel

**Panel title:** `Limitations`

**Purpose:** Front-load what visitors must not infer. This panel is not optional for public screenshots.

| Limitation | Visitor-facing line |
|------------|---------------------|
| Parity | `No Core/Godot parity claim` |
| Pixels | `No pixel equivalence or difference map` |
| Validation | `No scientific or physics validation` |
| Closure | `Hermetic closure not demonstrated here` |
| Endorsement | `No institutional endorsement` |
| Runtime scope | `Demo shows Godot visualization only` |

**Compact strip variant (for narrow layouts):**

```txt
Limitations: no parity · no pixel equivalence · no validation · engineering prototype
```

---

## Safe overlay text catalog

Use verbatim or adapt slightly. Full machine-readable copy lives in `reports/glowing_heart_godot_ui_overlay_text.preview.json`.

### Tier 1 — Required (every public surface)

- `Experimental observatory visualization.`
- `Perceptual demonstration only.`
- `No parity or validation claim.`
- `Engineering prototype — work in progress.`

### Tier 2 — Context (panels and captions)

- `Godot observatory shell — visualization, not instrument output.`
- `Observer bridge defined and measured; pixel comparison not ready.`
- `GRIN field preview in a sealed observatory scene.`
- `Active Project Glowing Heart engineering initiative.`
- `Preview artifact — inspectable trail, not a finished product.`

### Tier 3 — Footer / metadata

- `Fixture: fixture_hermetic_observatory_grin`
- `Captured: YYYY-MM-DD (UTC)`
- `parity_claim=NONE`
- `See: reports/glowing_heart_public_demo_readiness.preview.md`

---

## Forbidden wording audit

Remove or rewrite any UI string containing these terms or implications.

| Forbidden | Safer replacement |
|-----------|-------------------|
| proof / proved | preview / demonstrates visually |
| validated / verified | experimental / under development |
| matches reality | perceptual visualization |
| pixel parity | pixel comparison not ready |
| physically complete | engineering prototype |
| scientific confirmation | no validation claim |
| closure proven | closure not demonstrated here |
| equivalent / identical output | separate outputs, not compared |
| confirms physics | explores transport concepts visually |

---

## Misunderstanding prevention checklist

Run before publishing any screenshot, clip, or public page embed.

- [ ] Status banner visible and readable at thumbnail size?
- [ ] "Experimental" and "no parity or validation claim" present?
- [ ] Godot labeled as observatory shell, Core labeled separately (if shown)?
- [ ] No side-by-side layout implying equivalence?
- [ ] Observer panel states pixel comparison not ready?
- [ ] Limitations panel or strip present?
- [ ] No forbidden words in overlays, HUD, or captions?
- [ ] Capture date and fixture name in footer?
- [ ] Link to preview artifacts (readiness gate, v1.8 milestone)?
- [ ] Presentation mode used (minimal diagnostic overlays)?
- [ ] `parity_claim=NONE` preserved in any JSON/metadata export?

---

## Alignment with current engineering status

From v1.8 milestone and public demo readiness gate:

| Status | Value |
|--------|-------|
| Observer bridge defined | Yes |
| Bridge measured | Yes |
| Pixel comparison ready | No |
| Parity claim | NONE |
| Safe for Godot visual demo framing | Yes |
| Safe for physics validation claims | No |
| Safe for pixel parity claims | No |
| Godot runtime required for screenshots | Yes (not executed for this proposal) |

Public progress pages may describe v1.8 observer-bridge work. The Godot demo must **not** imply that work is complete or that outputs match.

---

## Recommended public page pairing

When embedding the Godot demo on a site or README:

1. **Lead** with the status banner phrase.
2. **Show** the screenshot (Layout A).
3. **Link** to `Docs/xPRIMEray/project_glowing_heart_v1_8_milestone.md` for bridge context.
4. **Link** to `reports/glowing_heart_public_demo_readiness.preview.md` for claim boundaries.
5. **State** explicitly: demo is perceptual; bridge is measured; comparison is future work.

**Safe page subtitle:**

> Active engineering initiative — Godot observatory preview for Project Glowing Heart.

---

## Blocking items before first public screenshot packet

These remain open from the public demo readiness gate. This proposal addresses the overlay-text and UI-audit items; runtime capture is still required.

1. Capture one current Godot screenshot using Layout A and Presentation mode.
2. Apply banner + four panels (or strip equivalents) in post-captioning.
3. Date-stamp and name the fixture in the footer.
4. Run the misunderstanding prevention checklist.
5. Publish as v2.2 "First Godot Demo Packet" per architecture vision — still with no parity claim.

**Do not block on:** observer alignment completion, pixel comparison, or Core side-by-side — those are later milestones.

---

## Bottom line

The Godot demo is ready to be **framed** publicly as an experimental observatory visualization. It is **not** ready to be **described** as validated, parity-aligned, or pixel-comparable.

**Safe public phrase (use everywhere):**

> Experimental observatory visualization. Perceptual demonstration only. No parity or validation claim.