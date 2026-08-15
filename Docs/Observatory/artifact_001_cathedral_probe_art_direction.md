---
title: Artifact 001 — Cathedral Probe Art Direction
description: First-release reel, overlays, Pages hero, and compositor recipe for the public Cathedral Probe sequence
---

# Artifact 001 — Cathedral Probe Art Direction

**Status:** design specification. Not a measurement paper. Not a composed master.
**Specimen:** Codex run7, `/tmp/artifact001_run7/`, committed stills under `Docs/assets/observatory/artifact_001/stills/`.
**Reproduction:** `ARTIFACT001_OUTPUT=/tmp/artifact001 ./scripts/capture_artifact001.sh`
**Authority:** `manifest.json` from that run. Nothing below invents a count, a Probe View, or a closure claim.

This is the first public artifact built from a real human instrument sequence:

```text
F  reveal Field Structure
H  enter Hermetic presentation
G  acquire one formal Complete SNAPSHOT
Q  Outcome
Q  Contact Events
Q  Transport Effort
```

The captures are the artwork. The compositor may crop, hold, dissolve, and letter. It may not paint new rings, fake a hit, or grade the plates into a prettier lie.

---

## Established language

```text
THE CATHEDRAL PROBE
Same window. Different light.
Same frame. Same transport. Different questions.
Render the path. Inspect the journey.
```

Pedagogy, not ornament:

| Metaphor | Means, exactly |
|---|---|
| Window | One sealed film plate. One generation. One camera pose after Hermetic. |
| Light | A Probe View: a presentation mapping of sealed arrays. |
| Stained glass | The plate, nearest-neighbor, read through different mappings. |
| Cathedral | Earned by the geometry the transport already drew. Not arches, not nave, not rose-window clipart. |

Do not add religious ornamentation. Do not add cyberpunk chrome. Do not imply UAP validation.

---

## Visual inventory (looked at, not imagined)

Viewport stills are 1152×648. Sealed plates are 160×90. All Hermetic stills share one camera pose. `establish.png` is a different pose (Gallery control). `snapshot_complete.png` and `outcome.png` are **byte-identical** (`b01ce730…`).

| File | What it actually shows | Role |
|---|---|---|
| `establish.png` | Cartesian gallery. Two bright discs. Pedestal. Field Structure **OFF**. Probe View unavailable. | Before. Ordinary scene presentation. |
| `hermetic_field_structure.png` | White field. Green outer ring. Cyan density arrows. Small authored core. Field Structure **ON**. Probe View unavailable. | Threshold. The room withdraws. |
| `snapshot_complete.png` | Instrument HUD on the first sealed plate. Probe View already **Outcome**. | Lock. G has finished. |
| `outcome.png` | Same pixels as `snapshot_complete.png`. | First question, already on the glass. |
| `contact_events.png` | Same pose. Concentric contact bands visible through the HUD. | The stained glass. |
| `transport_effort.png` | Same pose. Uniform coral plate under the HUD. | Third question. Full budget used. |

Portable plates (HUD-free, these are the Q artwork):

| Plate | Measured fact | Appearance |
|---|---|---|
| `outcome_display.png` | Outcomes: `MaxStepsExhausted = 14400 / 14400` | Uniform amber `#E89A30` `(232,154,48)` |
| `contact_events_display.png` | Contacts: `0=1670`, `1=7165`, `2=3254`, `3+=2311` | Concentric navy / blue / teal / amber rings around a central disk |
| `transport_effort_display.png` | Effort min/mean/max `1.0 / 1.0 / 1.0`, valid `14400/14400` | Uniform coral (effort = 1.0) |

**Do not invent structure on the two uniform plates.** Uniformity is the measurement: every pixel exhausted its step budget; every pixel used the full effort. Contact Events is the only Q view with spatial structure. That contrast *is* the lesson.

`snapshot_complete.png == outcome.png` is not a sequencing error to hide. G seals the plate with Outcome already selected. The compositor changes only the sentence.

---

## Two picture systems

Keep them distinct.

1. **Instrument stills** (viewport PNGs). HUD, keys, Field Structure overlay. Used for Explore → Freeze (shots 1–3). They prove a person can do this with F, H, G.
2. **Plate stills** (portable `*_display.png`). HUD-free, nearest-neighbor only. Used for Interrogate (shots 4–6). They are the stained glass.

Do not treat the HUD as the cathedral. Do not treat the plates as a game UI.

Upscale law (Observer PNG doctrine): **nearest-neighbor only**. No bilinear, no bicubic, no AI upscale.

---

## 1. Canonical reel — 26.0 seconds

**Master:** 1920×1080, 30 fps, Rec.709, yuv420p, no look-up table, no grain, no bloom, no chromatic aberration.

Duration window is 20–30 s. 26.0 s is the canonical cut. A 22.0 s trim exists by shortening title and end holds (see social, §7). Do not shorten Contact Events.

### Dramatic spine

1. Name the instrument.
2. Show the Cartesian room.
3. Withdraw the room. Leave authored field geometry.
4. Lock one Complete SNAPSHOT.
5. Ask Outcome. The plate is a single amber field.
6. Ask Contact Events. The same plate becomes the window.
7. Ask Transport Effort. The same plate is a single coral field.
8. State the claim boundary and sit down.

---

## 2. Exact shot order

Times are start-of-shot on the finished master. Transitions are included in the outgoing/incoming holds, not added on top.

| # | t in | dur | Source | Picture system | Motion | Transition in |
|---|-----:|----:|---|---|---|---|
| 0 | 0.00 | 2.40 | title card | type on black | none | — |
| 1 | 2.40 | 3.20 | `establish.png` | instrument | +2% push, 3.20 s | 8-frame dissolve from title |
| 2 | 5.60 | 3.60 | `hermetic_field_structure.png` | instrument | hold, then +1% toward the core after 1.2 s | 6-frame dissolve (the room withdraws) |
| 3 | 9.20 | 2.40 | `snapshot_complete.png` | instrument | **hard hold** | 2-frame cut (G is a lock) |
| 4 | 11.60 | 2.80 | `outcome_display.png` full-bleed | plate | **hard hold** | 0-frame cut; overlay text only changes from shot 3 |
| 5 | 14.40 | 4.80 | `contact_events_display.png` full-bleed | plate | +4% push, nearest-neighbor | 4-frame cut |
| 6 | 19.20 | 2.80 | `transport_effort_display.png` full-bleed | plate | **hard hold** | 4-frame cut |
| 7 | 22.00 | 4.00 | end card | type on black | none | 10-frame fade to black, then type |

**Total: 26.00 s / 780 frames.**

Shot 3 → 4 is the same sealed generation and, in the viewport pair, the same PNG. The audience must feel “the sentence changed, the observation did not.” Do not crossfade 3 into 4 as if they were different pictures.

Optional one-frame flash at 14.40: the viewport `contact_events.png` for 3 frames, then the clean plate. This is the only permitted HUD-to-glass splice. If used, steal the 3 frames from the front of shot 5. Do not do this for Outcome or Effort — they have nothing to reveal under the HUD that the plate does not already say.

---

## 3. Overlay microcopy

One type family. Two sizes.

- **Key:** IBM Plex Mono, 22 px at 1080p, letter-spacing 0.18 em, white 92%.
- **Line:** IBM Plex Sans or Source Serif 4, 28 px, weight 400, white 88%.
- **Quiet:** 16 px mono, white 55%.

Position: lower-left inside a full-width lower-third. Never top-right (that is where the live HUD already lives on instrument stills). Never centered except on title and end cards.

The Hermetic stills and the Gallery floor are near-white. White type cannot sit raw on those pixels. Every picture shot (1–6) carries a 88 px high lower-third: transparent at the top, `#07070B` at 78–88% at the baseline. Type lives inside that bar, 64 px from the left, 28 px from the bottom. Title and end cards stay type-on-black with no bar.

Fade: 12 frames in, hold, 8 frames out. First overlay on a shot starts 10 frames after picture.

| Shot | Overlay (exactly these strings) |
|---|---|
| 0 | `THE CATHEDRAL PROBE` / `Same window. Different light.` |
| 1 | `Gallery` |
| 2 | `F  Field Structure` / `H  Hermetic` / `Same window.` |
| 3 | `G  Complete SNAPSHOT` / `14400 / 14400  ·  generation 1` |
| 4 | `Q  Outcome` / `Terminal semantic outcome` / `MaxStepsExhausted` |
| 5 | `Q  Contact Events` / `Same frame. Same transport. Different questions.` |
| 6 | `Q  Transport Effort` / `Numerical step budget used · not time · not field strength` / `effort = 1.0` |
| 7 | see §5 |

Forbidden on the reel:

- Histogram dumps (`0=1670…`) except the single end-card identity line.
- SHA-256 strings.
- “Portal,” “wormhole confirmed,” “anomaly,” “UAP.”
- Fake scanlines, targeting reticles, glitch, holographic grids.
- Latin, gothic blackletter, crosses, rose-window illustrations.
- Any number not in `run_summary.md` / `manifest.json`.

Legend colors, if a compositor draws a three-swatch key on shot 5 only:

| Label | RGB | Meaning |
|---|---|---|
| 0 | `24, 48, 92` | no accepted contact |
| 1 | `56, 128, 190` | one |
| 2 | `80, 190, 168` | two |
| 3+ | `236, 154, 48` | three or more |

Do not put the full Outcome legend on shot 4. The plate is one color. One word is enough: `MaxStepsExhausted`.

---

## 4. Transition timing

All values at 30 fps.

| Junction | Type | Frames | Intent |
|---|---|---:|---|
| 0 → 1 | dissolve | 8 | Arrive in a room, not a trailer smash. |
| 1 → 2 | dissolve | 6 | Cartesian presentation withdraws. This is the discovery cut. |
| 2 → 3 | hard cut | 2 | Acquisition is a lock, not a mood. |
| 3 → 4 | hard cut | 0 | Same plate. New sentence. |
| 4 → 5 | hard cut | 4 | New question. The glass changes. |
| 5 → 6 | hard cut | 4 | New question. The glass changes again. |
| 6 → 7 | fade to black | 10 | Leave the plate before preaching. |

Audio-picture sync: the G lock click lands on the first frame of shot 3. Each Q cut may have a dry 1-frame tick, 8 dB below the G click, no reverb.

Do not use whip pans, light-leaks, or match-dissolves that imply the rings “grow” out of the amber field. They do not grow. They were always in the sealed arrays.

---

## 5. Title card and end card

Black `#07070B`. No vignette illustration. No logo lockup larger than 96 px.

### Title — 2.40 s

```
                    ARTIFACT 001

               THE CATHEDRAL PROBE

            Same window. Different light.
```

`ARTIFACT 001` is 12 px mono, 0.28 em tracking, 50% white, appears at frame 8.
Title fades in over 16 frames, line two 8 frames later.
Hold through frame 64, fade 8 frames into shot 1.

### End — 4.00 s

```
          Render the path. Inspect the journey.

        One recorded observation. Three illuminations.

     14400 / 14400  ·  generation 1  ·  Hermetic  ·  Field Structure ON

     No optical closure from this bundle alone.
     Contact Events are not a unique-surface census.
     Transport Effort is not time, energy, or field magnitude.
     Color is presentation mapping, not measurement authority.
     No UAP validation.
```

First couplet: 32 px serif, 88% white.
Identity line: 16 px mono, 60%.
Boundaries: 13 px mono, 42%, 1.35 line-height. They must be readable. They are not fine print to dodge.

xPRIMEray wordmark, 72 px, 70% opacity, 48 px from bottom center. Use the existing research mark (`Docs/assets/xPRIMEray_Logo_Research.png`). Do not invent a cathedral glyph.

---

## 6. GitHub Pages hero treatment

Do **not** replace the three measured lobby cards (Observer Disagreement 23.8%, Hermetic Closure, Coherence Basin). Those are research findings. Artifact 001 is the first public *instrument* artifact. It sits **above** the three-card grid as a full-bleed strip.

### Layout

```
┌─────────────────────────────────────────────────────────────┐
│  [contact_events_display.png, nearest-neighbor, full bleed] │
│  left 42% dark gradient #07070B → transparent               │
│                                                             │
│  ARTIFACT 001                                               │
│  THE CATHEDRAL PROBE                                        │
│  Same window. Different light.                              │
│                                                             │
│  14400 / 14400 Complete · generation 1                      │
│                                                             │
│  Hermetic withdraws the Cartesian room. One sealed          │
│  plate. Three questions.                                    │
│                                                             │
│  [ Watch the 26s reel ]   [ Read the sequence ]             │
│                                                             │
│  Color is presentation mapping, not measurement authority.  │
└─────────────────────────────────────────────────────────────┘
```

Under the strip, a 3-up caption row — instrument stills, not plates:

| Cell | File | Caption |
|---|---|---|
| Explore | `establish.png` | Gallery. Field Structure off. |
| Freeze | `hermetic_field_structure.png` | F + H. The room withdraws. |
| Interrogate | `outcome.png` | G, then Q. Same generation. |

Hook copy (allowed; no new numbers):

> Hermetic + Field Structure removes ordinary Cartesian presentation. What remains is authored field geometry and one sealed plate. Outcome is uniformly `MaxStepsExhausted`. Contact Events draws the concentric journey. Transport Effort is uniformly `1.0`. Same frame. Same transport. Different questions.

Drop-in markup: `Docs/assets/observatory/artifact_001/hero_fragment.md`
Drop-in CSS: `Docs/assets/observatory/artifact_001/hero.css`

Do not ship the strip to `Docs/index.md` until a composed reel file exists. The preview in `preview.html` is the approval surface.

---

## 7. Social teaser — 9.5 seconds

Same master codec. Safe-center for 1:1 crop: keep the Contact Events disk inside the central 1080×1080.

| # | t in | dur | Source | Overlay |
|---|-----:|----:|---|---|
| S0 | 0.00 | 0.80 | title (no “ARTIFACT 001”) | `THE CATHEDRAL PROBE` |
| S1 | 0.80 | 1.60 | `establish.png` | none |
| S2 | 2.40 | 2.00 | `hermetic_field_structure.png` | `Same window.` |
| S3 | 4.40 | 4.00 | `contact_events_display.png` +4% push | `Different light.` |
| S4 | 8.40 | 1.10 | end card, first couplet only | `Render the path.` |

Cuts: 6-frame dissolve S1→S2, hard elsewhere. No Outcome, no Effort, no HUD-heavy seal shot. The teaser is the withdraw and the glass. Identity and claim boundaries live on the canonical reel and the Pages footnote.

1:1 and 9:16 derivatives are center-crops of this same 16:9 teaser. Do not re-edit.

---

## 8. Thumbnail concept

**1920×1080 and 1280×720.** Built from the real stills. Not a generated illustration.

```
┌──────────────────────────────┬────────────────────┐
│                              │  establish.png     │
│  contact_events_display.png  │  (top 50%)         │
│  nearest-neighbor            │────────────────────│
│  center disk on the          │  hermetic_field_   │
│  left two-thirds             │  structure.png     │
│                              │  (bottom 50%)      │
│  THE CATHEDRAL PROBE         │                    │
│  Same window. Different      │                    │
│  light.                      │                    │
└──────────────────────────────┴────────────────────┘
```

Rules:

- Left panel is the plate, not the HUD screenshot.
- Right stack is the threshold: room / no room.
- No fake third sun, no extra rings, no glow pass.
- Title sits on a 48 px high 70% black bar at the bottom of the left panel, not as a 3D-extruded logo.
- Do not put `14400` on the thumbnail. Curiosity first; numbers on the reel.

`preview.html` § Thumbnail is the approval comp.

---

## 9. Music cue map

Reference: a dry observatory, not a trailer, not a nave.

No choir. No pipe organ. No vocal. No percussion except the two functional clicks. Prefer unmetered air over a 4/4 pulse. If a pulse exists, it is ≤ 52 BPM and exits before shot 5 so the rings are not marched.

| t | Cue | Level | Notes |
|---|---|---|---|
| 0.00 | almost silence | — |  |
| 0.40 | single high glass partial, fade in 1.6 s | −22 LUFS short | one pitch, no chord |
| 2.40 | low room tone (air, not music) | −28 | under Gallery |
| 5.60 | room tone drops out over 0.4 s | — | the withdraw is also an audio event |
| 6.20 | sparse sine, same pitch as 0.40, quieter | −26 | field geometry, not melody |
| 9.20 | **G lock click** + held fifth | click −16 peak; fifth −24 | first frame of shot 3 |
| 11.60 | fifth remains; no new note | — | Outcome is a color field, not a swell |
| 14.40 | Q tick; add one stacked fifth, very slow | −20 | the only harmonic motion in the piece |
| 19.20 | Q tick; collapse to one pitch | −24 | Effort is a single number |
| 22.00 | fade all musical tone in 0.6 s | — |  |
| 22.80 | decaying glass harmonic, 2.4 s | −26 | under the couplet, gone before boundaries |

Loudness: −16 LUFS integrated for the canonical reel, −14 LUFS for the teaser, true peak ≤ −1.5 dBTP. Do not sidechain the plates to a beat.

Licensed-or-original only. If no cue is ready, ship the reel silent. Silence is legal. A temp epic bed is not.

---

## 10. Compositor execution notes

Machine recipe: `Docs/assets/observatory/artifact_001/compositor_recipe.json`.

An automated compositor must do the following, in order.

### 10.1 Verify sources

Hash every file in `stills/` against `compositor_recipe.json` → `sources[].sha256`. Abort on mismatch. Do not “find a similar PNG.”

Confirm:

```text
sha256(snapshot_complete.png) == sha256(outcome.png)
== b01ce7301599cf85cb97a22ba3301560d32310246d2e6de77060df1c6aee4983
```

### 10.2 Prepare rasters

- Viewport stills: scale to 1920×1080 with **nearest-neighbor** (they are already 16:9).
- Plates: scale 160×90 → 1920×1080 with **nearest-neighbor** (`ffmpeg -sws_flags neighbor`).
- Do not sharpen. Do not denoise. Do not color-manage into Display P3.

### 10.3 Overlay plates

Render overlays as premultiplied RGBA PNG sequences from the recipe `overlays[]` (HTML/CSS or SVG → 1920×1080, exact strings). Do not hand-letter in a video editor. Do not let an image model write the type.

### 10.4 Motion

Only shots 1, 2 (after 1.2 s), and 5 may move. Use integer zoom toward the image center. Sample with nearest-neighbor so plate pixels stay square. Uniform plates (4, 6) must not Ken Burns — a zoom on a flat color is a lie that something is there.

### 10.5 Edit

Concatenate per `edl[]`. Apply the listed transitions. Write a 26.000 s master. If the result is not 780 frames at 30 fps, fail.

### 10.6 Derivatives

From the master, no re-grade:

| Output | How |
|---|---|
| `artifact001_canonical_26s.mp4` | the master |
| `artifact001_teaser_95s.mp4` | cut per `teaser_edl[]` |
| `artifact001_teaser_1x1.mp4` | center 1080×1080 of the teaser |
| `artifact001_teaser_9x16.mp4` | center 1080×1920 of the teaser |
| `artifact001_thumb_1920.png` | frame layout §8, stills only |
| `artifact001_thumb_1280.png` | same, scaled |

### 10.7 Sidecar

Write `artifact001_edit_manifest.json` containing: source hashes, EDL, overlay strings, ffmpeg version, output hashes, and the five claim-boundary sentences. A reel without a sidecar is not releasable.

### 10.8 What the compositor must not do

- Generate or inpaint pixels on any plate.
- Crossfade Outcome into Contact Events.
- Animate rings expanding.
- Draw field geometry that is not in `hermetic_field_structure.png`.
- Use `snapshot_complete.png` and `outcome.png` as if they were different pictures.
- Put claimed telemetry on screen that is not in the run7 `run_summary.md`.
- Add a music bed that covers the claim-boundary card.

---

## Claim boundaries (repeat on every public surface)

From the specimen `manifest.json`:

- No optical closure is established by this bundle alone.
- Contact Events are not a unique-surface census.
- Transport Effort is not time, energy, or field magnitude.
- HitGeometry cannot be inferred from Contact Events.
- Color is presentation mapping, not measurement authority.
- No UAP validation is claimed.

This artifact is a **qualified visualization**. It shows how the public instrument is used. It does not certify a physical wormhole, a UAP, or optical closure.

---

## Approval preview

Open `Docs/assets/observatory/artifact_001/preview.html` in a browser. It comps the title, end, storyboard, Pages hero, social frames, and thumbnail from the real stills.

Do not promote Artifact 001 to the lobby or the Gallery “Signature Exhibits” list until a hashed master reel exists.
