---
po_doc_type: reference
title: Public Vocabulary
status: partial
engine_commit: "5ce15c13"
generated: false
claim_boundary: "Public labels for UI and pedagogy; sealed channel IDs and enums remain authoritative when labels diverge."
---

# Public Vocabulary

**Canonical public glossary** for the Portable Observatory.

**Status:** partial — docs/demo pack; not a code rename of sealed contracts.
**Verified against:** docs ontology at `5ce15c13`; sealed channels base `6e69d792`.

**Core rules**

1. Display is not probe.
2. Outcome-plane semantics outrank RGB presentation.
3. Deeper effort is more attempt, not automatically more truth.
4. Public words stand alone first; internal names appear only when technically useful.

---

## Term cards

### Transport Lens

| | |
|--|--|
| **Public definition** | The interactive way of looking at how the engine carries rays through the authored scene and field policy. |
| **Internal equivalent** | `GrinFilmCamera` curved / film transport path (live or snapshot)—not a physical camera body |
| **Measures or displays** | Drives transport samples that instruments and display paths consume |
| **Claim boundary** | Not a physical spacetime lens, GR light deflection, or laboratory optic unless a named model is evidenced |
| **Confused with** | Free-roam locomotion camera · Observatory panel chrome · “truth camera” · OI fixtures |
| **Example UI** | Film LIVE / SNAPSHOT inset labeled as the Transport Lens path |
| **Example sentence** | “Enable the Transport Lens before reading the Observation Plate.” |

**Vs free-roam camera:** locomotion (walk/fly) moves the observer pose; the Transport Lens is the **instrument path** that samples transport from that pose. Moving the player without enabling the lens is not an observation.

---

### Observation Plate

| | |
|--|--|
| **Public definition** | The rectangular image surface that presents mapped observation channels or host display shading for one live or frozen frame. |
| **Internal equivalent** | FilmView / film buffer / plate region (e.g. host 320×180 over 80×45 or 160×90 samples) |
| **Measures or displays** | **Displays** a mapping; does not by itself seal scientific classes |
| **Claim boundary** | Pixels are presentation unless a caption names a sealed channel and Complete snapshot |
| **Confused with** | World **viewport** · outcome plane (data) · OI evidence PNG doctrine |
| **Example UI** | The film TextureRect / plate widget in the HUD |
| **Example sentence** | “Watch the Observation Plate—not the big 3D viewport—when field strength changes.” |

**Vs viewport:** the viewport is the 3D world view; the plate is the instrument face.

---

### Display Mode

| | |
|--|--|
| **Public definition** | A presentation recipe that colors samples (or host buffers) for human viewing. |
| **Internal equivalent** | `FilmShadingMode` and related host recipes (DepthHeatmap, NormalRGB, NdotV, …) |
| **Measures or displays** | **Displays** only; does not define a sealed instrument channel |
| **Claim boundary** | Changing Display Mode must not change outcome histograms |
| **Confused with** | **Instrument** · sealed geometry channels · outcome overlay |
| **Example UI** | Key **N** cycles display shading |
| **Example sentence** | “NormalRGB is a Display Mode, not a proof that `geometry.normal` is sealed.” |

---

### Instruments

| | |
|--|--|
| **Public definition** | Named measurements that produce portable observation records (semantic, geometric, field, transport, or process-history). |
| **Internal equivalent** | Instrument tier + channel IDs (e.g. `cathedral.probe.outcome`); OI headless fixtures are a sibling evidence lane |
| **Measures or displays** | **Measures** defined quantities into channels |
| **Claim boundary** | Each instrument measures only its defined quantity |
| **Confused with** | HUD chrome · Field Dial alone · Display Mode |
| **Example UI** | Semantic outcome instrument vs planned geometric instruments |
| **Example sentence** | “Instruments seal channels; Display Modes only paint them.” |

---

### Inspector

| | |
|--|--|
| **Public definition** | Panel for context, validity, counts, histograms, and dependencies for the current plate or snapshot. |
| **Internal equivalent** | Tab telemetry / probe detail / host side panel |
| **Measures or displays** | Explains **records**; does not create new physics |
| **Claim boundary** | Numbers and context only—not OI PASS and not physical truth |
| **Confused with** | Field Dial · Evidence Console · Transport Lens plate |
| **Example UI** | **Tab** opens the Inspector |
| **Example sentence** | “Open the Inspector for HitGeometry counts before interpreting NormalRGB.” |

---

### Region Probe

| | |
|--|--|
| **Public definition** | Tooling that records per-sample transport outcomes, finds **Unresolved Regions**, and can request deeper **Transport Effort** on a selection. |
| **Internal equivalent** | **Cathedral Probe** Stage 0 architecture (`ProbeOutcomeCode`, region analysis, refinement, snapshot lifecycle) |
| **Measures or displays** | Semantic outcomes, region labels, refinement levels |
| **Claim boundary** | Architectural metaphor—not wormhole proof or religious physics |
| **Confused with** | Beauty render · entire product name · lasso-on-magenta |
| **Example UI** | **P** probe deeper; **J**/**K** cycle regions (when wired) |
| **Example sentence** | “Use the Region Probe only on Complete SNAPSHOT outcome planes.” |

**Vs Cathedral Probe:** same subsystem; **Region Probe** is public UI language; **Cathedral Probe** remains architecture/lab naming.

---

### Region Refinement

| | |
|--|--|
| **Public definition** | Re-running transport for samples in a selected outcome-defined region under higher effort policy, then recording semantic class changes. |
| **Internal equivalent** | Selected-region refinement; `ProbeRefinementSummary`; refinement level channel |
| **Measures or displays** | Effort history + class transitions |
| **Claim boundary** | **Resolved** only means prior max-steps → HitGeometry \| BackgroundResolved; deeper ≠ truer |
| **Confused with** | Full-frame re-render · ML adaptation · mesh LOD |
| **Example UI** | **P** on selected Unresolved Region |
| **Example sentence** | “Region Refinement raises Transport Effort on the outline—watch the counts.” |

---

### Transport Effort

| | |
|--|--|
| **Public definition** | How much computational work the integrator was allowed or used (policy max and/or steps used). |
| **Internal equivalent** | `StepsPerRay` / refined max steps / steps used (when recorded) |
| **Measures or displays** | Numerical **policy** budget—not energy or time |
| **Claim boundary** | Not proper time, coordinate time, or physical energy; not Field Strength |
| **Confused with** | Field strength · path length · “try harder = truth” |
| **Example UI** | Refinement raises effort on selected samples |
| **Example sentence** | “Transport Effort is a step budget, not field magnitude.” |

---

### Path Length

| | |
|--|--|
| **Public definition** | Summed length of the discrete path a sample took in scene space until termination, **when recorded**. |
| **Internal equivalent** | Integrated path length / Σ step lengths |
| **Measures or displays** | Scene-length proxy (planned as sealed `transport.path_length`) |
| **Claim boundary** | Not optical path in materials; **not proper time** unless a named model defines otherwise |
| **Confused with** | Depth-to-first-hit · step count · field strength |
| **Example UI** | Grayed-out until sealed channel ships |
| **Example sentence** | “Path Length, when sealed, is scene units—not cosmic time.” |

**Status:** planned / partial for “exists now.”

---

### Closure

| | |
|--|--|
| **Public definition** | Property that samples or world directions remain **enclosed** under a stated straight or policy test (hermetic teaching). |
| **Internal equivalent** | Hermetic enclosure / hit-closure language in gallery docs |
| **Measures or displays** | Policy-relative enclosure, not free metaphor for any pink blob |
| **Claim boundary** | Relative to **policy and geometry**—not physical isolation of a universe |
| **Confused with** | Unresolved Regions · portal throat · Complete snapshot |
| **Example UI** | Hermetic shell experiments |
| **Example sentence** | “Closure is an enclosure test, not an Unresolved Region outline.” |

---

### Unresolved Regions

| | |
|--|--|
| **Public definition** | Connected groups of samples whose sealed outcome is unresolved under probe policy (default: budget/step exhaustion). |
| **Internal equivalent** | Components on `MaxStepsExhausted`; `cathedral.probe.region_label` |
| **Measures or displays** | Semantic region membership |
| **Claim boundary** | Unresolved ≠ empty space ≠ wormhole; may persist after refinement |
| **Confused with** | Legacy magenta blocks · BackgroundResolved · Closure |
| **Example UI** | Outline from region_label, never from RGB flood-fill |
| **Example sentence** | “Unresolved Regions come from the outcome map.” |

---

### Evidence Console

| | |
|--|--|
| **Public definition** | Place where qualified runs, recipes, and fixture evidence are launched and reviewed under guardrails. |
| **Internal equivalent** | ObservatoryWorkbench / TestBench / OI diagnostics recipes |
| **Measures or displays** | Recipe-bound PASS/FAIL and archives |
| **Claim boundary** | PASS applies to **named fixtures/recipes**, not live free roam |
| **Confused with** | Inspector · Transport Lens · marketing gallery |
| **Example UI** | **Esc** / Workbench |
| **Example sentence** | “The Evidence Console runs sealed recipes; free roam is exploration.” |

---

### Experiment Gallery

| | |
|--|--|
| **Public definition** | Curated list of scenes and experiments with fixed poses, instruments, and claim boundaries. |
| **Internal equivalent** | Scene catalog / sample worlds / docs experiments |
| **Measures or displays** | Menu of contexts—not automatic full instrument coverage |
| **Claim boundary** | Listing a scene ≠ every instrument implemented for it |
| **Confused with** | Evidence runs · Glowing Heart fixture browser versions |
| **Example UI** | Scene cards (Hermetic, Portal Seam, …) |
| **Example sentence** | “Pick Hermetic Calibration from the Experiment Gallery.” |

---

## Explicit distinctions (summary)

| Pair | Distinction |
|------|-------------|
| Transport Lens vs free-roam camera | Instrument path vs locomotion pose |
| Observation Plate vs viewport | Mapped samples vs 3D world view |
| Display Mode vs Instrument | Color recipe vs measured channel |
| Region Probe vs Cathedral Probe | Public UI vs architecture name |
| Transport Effort vs field strength | Step budget vs field dial policy scale |
| Path Length vs depth | Integrated path vs first-hit distance mapping |
| Closure vs Unresolved Regions | Enclosure test vs max-steps components |

---

## Cross-term map

| Public | Internal touchpoints |
|--------|----------------------|
| Transport Lens | `GrinFilmCamera` / film path |
| Observation Plate | FilmView / plate |
| Display Mode | `FilmShadingMode` |
| Instruments | Channel + tier IDs |
| Inspector | Tab telemetry |
| Region Probe | Cathedral Probe subsystem |
| Region Refinement | Selected-region refine (**P**) |
| Evidence Console | ObservatoryWorkbench / TestBench |
| Transport Effort | Step budget / refine policy |
| Path Length | Integrated path (**planned** sealed) |
| Closure | Hermetic enclosure tests |
| Unresolved Regions | `MaxStepsExhausted` components |

---

## Claims vocabulary must not enable

- Physical wormhole confirmation
- Coordinate / proper time as measured
- Material normal maps from NormalRGB
- Magenta = no hit / MaxStepsExhausted
- Deeper = truer
- Complete snapshot = correct beauty plate
- Field FULL = universal physical maximum
- Fixture PASS = live free-roam OK

---

## Version

**Observatory Public Vocabulary v1** (canonical GitHub Pages form).
Prior draft path: `learn/observatory_public_vocabulary_v1.md` (retained as alias pointer).

*Display ≠ probe.*
