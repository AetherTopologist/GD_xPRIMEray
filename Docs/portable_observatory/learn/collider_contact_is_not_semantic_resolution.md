---
po_doc_type: learn
title: Collider Contact Is Not Semantic Resolution
status: partial
engine_commit: "5ce15c13"
scene_id: portal_gallery_seam_v0
scene_class: evidence_qualification
instrument_tier: semantic
instrument_id: SEM-OUT
channel_id: cathedral.probe.outcome
source_channel_deps: []
units: "ProbeOutcomeCode; film pass-2 hit counts are separate telemetry"
validity_condition: "Outcome-plane claims require Complete snapshot and Unprocessed=0. Aggregate film hitPct is not pass-1 semantic authority. Per-pixel contact traces require diagnostic export not yet attached."
visualization_mapping: "NormalRGB paints from a normal source (pass-1 or pass-2 depending on path); color never establishes ProbeOutcomeCode."
claim_boundary: "Do not call all-MaxStepsExhausted with nonzero film hits a renderer bug until a per-pixel pass-1 diagnostic proves misclassification. Stage 0 terminal outcome is not historical contact existence. No wormhole, proper-time, or physical-gravity claim."
evidence_links: []
contract_links:
  - CathedralProbe/ProbeOutcomeCode.cs
  - RayBeamRenderer.cs
  - GrinFilmCamera.cs
last_qualified_run: not yet qualified
generated: false
audit_sources:
  - codex_classification_seam_diagnostic
  - claude_classification_seam_diagnosis_audit
---

# Collider Contact Is Not Semantic Resolution

!!! abstract "Observation card"
    | Field | Value |
    |-------|-------|
    | Status | **partial** (doctrine + code-proven seam; per-pixel trial export unavailable) |
    | Verified against | sealed outcome path `6e69d792` lineage; diagnostics `5ce15c13`; observer-exclude film `36b003bd` |
    | SceneId (illustrative trial) | Gallery / `portal_gallery_seam_v0` class |
    | Channel ID (authority) | `cathedral.probe.outcome` |
    | Claim boundary | Terminal semantic class ≠ incidental physics contact ≠ film shading hits ≠ NormalRGB color |
    | Last qualified run | not yet qualified (no sealed per-pixel contact CSV attached) |

!!! warning "Do not declare a renderer bug yet"
    Film performance counters can report **nonzero hits** while the outcome plane reports **every pixel** as `MaxStepsExhausted`. Code-backed audits treat this as a **classification / telemetry seam**, not proven misclassification. A future diagnostics-only export must cross-tab pass-1 fields before any “bug” claim.

---

## Core distinction (four layers)

| Layer | What it is | What it is **not** |
|-------|------------|---------------------|
| **Physics collider contact** | Ray-segment intersection with a collision shape (nearest accepted pass-1 hit may store position/normal/collider) | Automatic “this sample is HitGeometry” |
| **Accepted semantic geometry resolution** | Stage 0 decision that the **terminal** semantic class is surface geometry under policy (`HitGeometry`) | “A collider was ever touched along the path” |
| **Completed transport outcome** | Full-frame lifecycle Complete + written `ProbeOutcomeCode` per sample | Pretty plate · nonzero film hitPct · watchdog silence |
| **Displayed NormalRGB color** | Visualization mapping from **some** normal source onto the Observation Plate | Proof of outcome class or normal validity census |

```text
physics collider contact
  ≠  accepted semantic geometry resolution
  ≠  completed transport outcome
  ≠  displayed NormalRGB color
```

Sister doctrine: [Display Is Not Probe](display_is_not_probe.md).

---

## Conceptual ladder

Read top → bottom. Each arrow is a **policy or packaging step**, not identity.

```text
Ray samples
  → collider contact          (physics intersection may occur along the path)
  → surface identity          (which collider / source group: fixture geometry, background, …)
  → termination policy        (StopOnHit, max steps, max distance, absorb rules)
  → ProbeOutcomeCode          (terminal semantic class under Stage 0 precedence)
  → observation channel       (sealed cathedral.probe.outcome, …)
  → Display Mode              (NormalRGB / Depth / … presentation only)
```

| Rung | Public language | Authority |
|------|-----------------|-----------|
| Ray samples | Transport Lens samples | Integrator path |
| Collider contact | Physics contact along segments | Pass-1 hit metadata (`Found`, nearest hit) |
| Surface identity | Fixture / background / other routing | Collider ID → surface class map |
| Termination policy | Numerical / scene policy | `StopOnHit`, step budget, distance |
| `ProbeOutcomeCode` | Semantic outcome | Terminal classification helper |
| Observation channel | Sealed instrument plane | `cathedral.probe.outcome` |
| Display Mode | Color recipe | Host shading — **not** outcome |

**Unresolved Regions** and Region Refinement hang off the **outcome plane**, not off contact history or NormalRGB. See [Reading the Outcome Plane](reading_outcome_plane.md) and [Tuning the Region Probe](../experiments/tuning_the_region_probe.md).

---

## The mystery: nonzero film hits, all MaxStepsExhausted

### Aggregate trial summaries (run-log)

Observed **complete** outcome-plane histograms (aggregate log lines; not a per-pixel export):

| Frame | Total | HitGeometry | BackgroundResolved | MaxStepsExhausted | StoppedEarly | Numerical | Unprocessed |
|-------|------:|------------:|-------------------:|------------------:|-------------:|----------:|------------:|
| 80×45 | 3 600 | 0 | 0 | **3 600** | 0 | 0 | 0 |
| 160×90 | 14 400 | 0 | 0 | **14 400** | 0 | 0 | 0 |

Film performance **hit** counters reported for those runs (illustrative aggregates from the trial telemetry): **1 056** (80×45) and **320** (160×90). Those counters are **not** pass-1 outcome counts.

### Two different authorities

| Counter / plane | Pass | Meaning |
|-----------------|------|---------|
| **Region Probe / Cathedral outcome plane** | Pass-1 terminal classification | Semantic `ProbeOutcomeCode` after transport policy |
| **Film hits** (perf / hitPct style telemetry) | Pass-2 accepted / best-hit samples for **shading** | Display-path sample acceptance — **different authority** |

```text
Cathedral / Region Probe  =  pass-1 terminal classification
Film hits                 =  pass-2 accepted shading samples
```

**Consequence:** nonzero Film hits do **not** contradict an all-`MaxStepsExhausted` Region Probe frame. They measure different things.

### Proven classification precedence (code)

Stage 0 production helper implements, in order:

```text
NumericalFailure
  > MaxStepsExhausted
  > StoppedEarlyAbsorbed
  > HitGeometry
  > BackgroundResolved
  > Invalid
```

So a ray that **contacts Geometry** at an intermediate step and later sets **max-steps reached** classifies as:

**`MaxStepsExhausted`** — not `HitGeometry`.

That is the current charter’s **terminal** interpretation: the outcome describes **how transport ended under policy**, not whether contact ever existed.

### Why MaxStepsExhausted can coexist with earlier geometry contact

Code-proven combination:

1. Pass-1 hit state can persist after contact (`Found` / nearest hit metadata).
2. Gallery scene transport is configured **not** to stop on hit (`StopOnHit = false`; trail not terminated on hit).
3. `maxStepsReached` is computed **after** the integration loop, **independent** of hit presence.
4. Classification checks **max-steps before** surface class.

Therefore `MaxStepsExhausted` can mean any of:

1. no geometry was found;
2. geometry was found earlier but transport **continued**;
3. background was found earlier but transport **continued**;
4. transport never reached an accepted terminal state under other rules.

**It does not distinguish those cases** on today’s sealed outcome plane alone.

!!! tip "Truthful Stage 0 reading"
    For Gallery-style non-terminating contact policy, all-`MaxStepsExhausted` can mean:

    > “Transport may have touched geometry, kept going, and exhausted the step budget.”

    It does **not** automatically mean “geometry was never hit.”

### Conceptual cross-tab (when Found / MaxSteps known)

Assuming no numerical failure and no stopped-early flag:

| Found (contact) | SurfaceClass | MaxSteps | Resulting `ProbeOutcomeCode` |
|----------------:|--------------|---------:|------------------------------|
| 1 | Geometry | 0 | `HitGeometry` |
| 1 | Geometry | 1 | **`MaxStepsExhausted`** |
| 1 | Background | 0 | `BackgroundResolved` |
| 1 | Background | 1 | **`MaxStepsExhausted`** |
| 0 | any | 1 | `MaxStepsExhausted` |
| 0 | any | 0 | `BackgroundResolved` |

**Unavailable in the cited Windows trial:** exact per-pixel counts for these six rows, collider-ID histograms, 20-pixel traces, and normal validity clusters — no raw per-pixel pass-1 export was present in-repo for that run. Do not invent them.

---

## Why “hitPct” may be misleading public terminology

| Telemetry label | Operator may hear | Safer public language |
|-----------------|-------------------|------------------------|
| Film **hits** / **hitPct** | “X% of samples resolved as geometry” | **Pass-2 accepted shading hits** (display path) |
| Probe **HitGeometry** count | Same as film hits | **Pass-1 terminal semantic geometry** only |
| “No hits” from all MaxSteps | “Rays missed the room” | **Terminal class is budget exhaustion** — contact history unknown without extra channels |

Public UX should not use a bare **hitPct** for both film shading and outcome planes. Prefer:

- **Transport Effort** — step budget / steps used (numerical policy, not time/energy)
- **Terminal outcome** — `ProbeOutcomeCode` histogram
- **Pass-2 shading hits** — film display acceptance (if shown at all)

---

## Future Inspector: expose Transport Effort **and** terminal outcome

The Inspector should not collapse these into one number.

| Panel field | Role |
|-------------|------|
| Lifecycle Complete / Unprocessed | Frame readiness |
| Outcome histogram | Terminal semantic classes |
| **Transport Effort** (policy max / steps used when sealed) | How hard transport worked — **planned** continuous channel today |
| Optional **historical contact** diagnostic | “Any geometry contact along path” — **not** today’s outcome enum; would be a separate channel if chartered |
| Field dial | Control scale — not evaluated field magnitude map |
| Display Mode | Presentation only |

Without dual exposure, operators will keep equating film hitPct with semantic resolution.

See [Diagnostics](../architecture/diagnostics.md): `[LiveSummary]` and terminal dumps are **not** automatic Evidence; sealed frames remain the carrier.

---

## Why NormalRGB must depend on an authoritative normal source

| Claim | Requirement |
|-------|-------------|
| “This brown/blue plate shows wall orientation” | Normals from an **authoritative** hit (valid pass-1 or sealed `geometry.normal` when shipped) **and** usually HitGeometry (or an explicit contact mask) |
| “NormalRGB proves semantic resolution” | **Forbidden** — Display Mode only |
| “Default up-vector looks like a normal” | Pass-1 may default unset normals; without **validity**, do not census faces |

Code audits note:

- Pass-1 can store nearest-hit normals from physics intersections.
- There is no sealed per-sample `NormalValid` export on the Stage 0 outcome plane today.
- Pass-2 shading normals (`BestHn`-class paths) are a **separate** selection from pass-1 terminal semantics.

**Rule:** NormalRGB must be captioned as a **Display Mode** whose normal source is named. Prefer mapping only after outcome (or a future contact/normal validity channel) qualifies the sample. Planned chain:

```text
geometry.normal (planned sealed) → normal_rgb.v1 → Observation Plate
```

Until then: host NormalRGB is presentation, not portable geometry instrument. See [Display Modes](../reference/display_modes.md) and [Hermetic Normal Calibration](../experiments/hermetic_normal_calibration.md).

---

## Geometry identity (not the first broken seam)

Audits of Gallery configuration (code + scene; not a substitute for missing per-pixel IDs):

- Film transport mask excludes the observer player layer after `36b003bd` (player layer 2; mask 1).
- Room walls/floor/ceiling participate as fixture/raytrace geometry sources for surface-class routing.
- **First semantic seam is not “walls missing from the source set.”**

**Proven first seam:**

```text
pass-1 incidental contact
  + continued transport (StopOnHit false)
  + maxStepsReached
  → MaxStepsExhausted   (terminal class wins)
```

**Second seam:** film hit counter ≠ pass-1 outcome count.

### Watchdog is not this outcome plane

Frame render-step watchdogs (`RenderStepMaxMs`, budget abort diagnostics) are **runtime telemetry**. They do not write `MaxStepsExhausted` into the outcome plane. A **completed** frame with Unprocessed = 0 and all MaxSteps is **not** explained by “watchdog painted max-steps.” Incomplete interrupted probe passes would show Unprocessed > 0 if classification never ran.

---

## Audit verdict (Codex + Claude)

| Question | Verdict |
|----------|---------|
| Is all-MaxSteps + nonzero film hits a proven renderer bug? | **No** — not on aggregate logs alone |
| Is Stage 0 taxonomy lying? | **No** — terminal class under precedence is intentional |
| Primary surprise source | **Termination policy** (non-stop-on-hit) + **terminal vs historical contact** |
| Secondary surprise source | **Telemetry naming** (film hits vs probe geometry) |
| Surface identity routing broken? | **Not indicated** by code/scene audit |
| Need for correction | Diagnostics export first; policy/charter change only if product intent shifts |

### Recommended diagnostics-only next boundary (not done here)

No engine change in this documentation note. Future diagnostics-only work may export per pixel:

- `Found`, surface class, numerical / max-step / stopped flags
- final steps, collider id/name
- normal validity
- first-contact / final-contact step if retained

Optional later semantic work (charter change only):

- separate **historical any-geometry contact** from **terminal acceptance**
- keep MaxSteps precedence for terminal outcome
- rename public film “hits” to pass-2 shading hits

---

## Claim boundary

!!! danger "Do not claim"
    - Renderer bug solely from film hits ≠ outcome histogram
    - Magenta or NormalRGB color as MaxSteps or HitGeometry
    - Path length / step count as proper time or energy
    - Closure or wormhole proof from this seam
    - Fixture PASS as live free-roam qualification
    - Causal structure from processing order alone

!!! success "Do claim carefully"
    - Simulated transport under numerical policy
    - Terminal semantic classification (`ProbeOutcomeCode`)
    - Policy-relative reading of MaxStepsExhausted
    - Distinct authorities: contact · terminal outcome · film shading · display mapping

---

## Pipeline sketch (internal)

Production path (lab names; public ladder above is preferred for first-run language):

```text
RayBeamRenderer.BuildRaySegmentsCamera_Pass1
  → Pass1HitInfo
  → ClassifyProbeSurfaceClass
  → ClassifyCathedralProbeOutcome
  → probe outcomes plane
  → FinalizeCathedralProbeSnapshotSummary / ProbeFrameSummary
  → sealed cathedral.probe.outcome (when adapted)
```

Region Probe is the public name for this Cathedral Probe Stage 0 architecture.

---

## See also

- [Display Is Not Probe](display_is_not_probe.md)
- [Reading the Outcome Plane](reading_outcome_plane.md)
- [Snapshot Completeness vs Resolution](snapshot_completeness_vs_resolution.md)
- [Tuning the Region Probe](../experiments/tuning_the_region_probe.md)
- [Claim Boundaries](../reference/claim_boundaries.md)
- [Diagnostics](../architecture/diagnostics.md)
- [Observation Channels](../reference/observation_channels.md)

*Collider contact ≠ semantic resolution. Film hits ≠ outcome plane. Display ≠ probe.*
