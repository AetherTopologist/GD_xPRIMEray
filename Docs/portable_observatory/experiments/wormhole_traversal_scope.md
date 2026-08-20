---
po_doc_type: experiment
title: Wormhole Traversal Observatory
status: scoped
engine_commit: "0ad87f52"
scene_id: portable_observatory.wormhole_traversal.v0
scene_class: deterministic_experiment_scope
instrument_tier: planned
claim_boundary: "A GR-inspired simulation and instrument scope; not evidence of physical wormholes, optical closure, or UAP phenomena."
generated: false
---

# Wormhole Traversal Observatory

## Scope card

This document scopes a future experiment, also called the **Spatial Manifold
Traversal Instrument**. It does not change runtime behavior.

The experiment models continuous traveler and ray traversal through a static,
non-rotating, Morris–Thorne-inspired manifold. Its central teaching result is:

```text
external mouth separation != wormhole proper path length
```

The baseline must remain visibly and semantically separate from speculative
rotating, modal, helical, ZPE, Hinductor, or ER=EPR interpretations.

### Scientific-status labels

| Layer | Status and language |
|---|---|
| GR-inspired baseline | Intrinsic coordinate, areal-radius profile, throat minimum, proper-distance accounting, and static non-rotating geometry. This is an inspired simulation model, not a GR validation. |
| Rendering / instrument abstraction | Traveler camera, ray transport, synchronized plots, HUD, sealed channels, replay, and artifact packaging. These describe the implementation and its measurements. |
| Speculative future model | Rotating throats, azimuthal shear, phase/helical modes, Hinductor-inspired analogies, and ER=EPR visual concepts. These are excluded from the baseline. |

## Conceptual baseline

Use an intrinsic traversal coordinate:

```text
ell ∈ [-L_A, +L_B]
ell = 0 at the minimum-area throat
```

Define an areal-radius function `r(ell)` such that:

```text
r(0) = r0
dr/dell = 0 at ell = 0
r(ell) increases toward both exteriors
```

The initial domain is:

```text
Exterior A → Mouth A → throat coordinate domain → minimum-area throat
           → Mouth B → Exterior B
```

This is an intrinsic manifold domain, not a literal Euclidean cylindrical
tube. The following must be explicit simulation parameters and provenance
fields, never hidden constants:

- `L_A`, `L_B`, and throat coordinate bounds;
- `r0` and the chosen `r(ell)` parameterization;
- mouth-region definitions;
- external A/B separation and units;
- traveler initial state, speed, and integration policy;
- camera/ray sampling and artifact policy.

Minimum continuous traveler state:

- traversal coordinate `ell`;
- proper/local clock value;
- local velocity;
- side/region identity;
- orientation basis;
- camera and ray-origin state.

No traveler position may jump from one mouth to the other as the baseline
transport implementation.

## Required synchronized views

The first useful instrument should synchronize all views to one sealed
traveler state and one acquisition generation.

1. **Traveler Worldline** — continuous motion from Exterior A through the
   throat to Exterior B.
2. **Areal Radius Profile** — U-shaped `r(ell)` profile with the traveler
   position marked.
3. **Proper-Distance Ruler** — signed intrinsic distance through the throat,
   for example `-10 m ... -5 m ... 0 ... +5 m ... +10 m`.
4. **External Separation Readout** — external mouth separation shown
   independently from wormhole proper path length.
5. **Traveler Camera / Optical View** — optical transport through the modeled
   geometry, not decorative tunnel effects.
6. **Optional Test-Particle Train** — later diagnostic for coordinate bunching
   versus changes in physical proper spacing.

The ruler and separation readout are primary: both values must remain visible
when the camera view is being inspected.

## Architecture and ownership

### Proposed file/module ownership map

These are planned ownership boundaries, not implementation instructions for
WT-0.

| Concern | Proposed owner | Boundary |
|---|---|---|
| Intrinsic parameters and `r(ell)` | `src/XPrimeRay.Core/WormholeTraversal/` pure C# types | No Godot nodes, rendering, or physics queries. |
| Frozen manifold geometry | `src/XPrimeRay.Core/SpatialKernel/` and `FrozenGeometrySnapshot` | Stable primitive identity and immutable provenance. |
| Authoritative intersections | Spatial Kernel pure C# linear scan | Dual validation first; no new Godot-only semantic authority. |
| Optional acceleration | BVH-v0 module | Acceleration only; no semantic or taxonomy changes. |
| Traveler integration | `WormholeTraversal` pure state/integration types | Deterministic stepping and replayable state transitions. |
| Godot scene/player adapter | Observatory Transport layer | Input, camera presentation, and scene composition only. |
| Optical instrument adapter | `GrinFilmCamera` adapter boundary | Consumes the planned authority; does not create a parallel intersection authority. |
| Sealed channels and provenance | `XPrimeRay.ObservationLayer` | Worldline, `ell`, `r`, proper distance, separation, policy, and validity metadata. |
| Views/HUD | Observatory presentation layer | Maps sealed channels; never infers measurements from RGB. |
| Artifact capture | `scripts/capture_*` and portable bundle infrastructure | Deterministic binaries, PNGs, manifest, replay proof. |

### Spatial Kernel dependency map

```text
WormholeTraversal parameters
        ↓
FrozenGeometrySnapshot + stable primitive identity
        ↓
pure C# linear-scan intersection authority ──→ dual-validation reports
        ↓
sealed transport / traveler channels
        ├──→ worldline, ell, radius, ruler, separation views
        ├──→ traveler optical adapter
        └──→ deterministic artifact bundle

BVH-v0 ──(acceleration only)──→ same intersection authority
Godot physics ──(diagnostic/host adapter only)──→ never semantic baseline
```

The instrument must not depend semantically on BVH. Deterministic replay and
artifact reproducibility remain release gates throughout the ladder.

## Milestone ladder

The existing roadmap is a hard sequencing constraint. Wormhole Traversal work
must not reorder the current release path:

```text
DONE  0ad87f52  finalize snapshot contacts by deterministic replay
NEXT  Artifact 001 recapture/rebaseline, then tag the exact producer
THEN  control ontology split: E experiment, H presentation, G freeze,
      Q sealed interrogation, POSE HELD; H must not teleport or swap fields
THEN  Spatial Kernel milestone 1
THEN  BVH-v0 acceleration-only validation
THEN  Godot 4.7.1 qualification
```

Wormhole work begins only as a planned lane after those gates, or in isolated
documentation/design work that cannot affect them.

| Milestone | Scope | Candidate commit |
|---|---|---|
| WT-0 | This scope and ownership document only. | `docs(observatory): scope continuous wormhole traversal instrument` |
| WT-1 | Pure `ell`, `r(ell)`, mouth regions, throat minimum, and proper-path model. No rendering. | `feat(spatial): add intrinsic wormhole throat model` |
| WT-2 | Continuous traveler state and deterministic traversal/replay. No teleport-style discontinuity. | `feat(spatial): add deterministic wormhole traveler` |
| WT-3 | Synchronized worldline, radius, ruler, and external-separation instruments. | `feat(observatory): instrument wormhole traversal channels` |
| WT-4 | Couple traveler camera and rays to the frozen throat geometry through Spatial Kernel authority. | `feat(observatory): couple optical transport to throat geometry` |
| WT-5 | Freeze the first Wormhole Traversal artifact with manifest-visible parameters and replay proof. | `feat(observatory): capture wormhole traversal baseline` |
| WT-6 | Optional fixed-interval test-particle train and spacing diagnostic. | `feat(observatory): add traversal particle train diagnostic` |
| FUTURE | Rotating/modal throat, azimuthal shear, phase/helical modes, Hinductor analogy, ER=EPR concepts. | Separate proposals only after the static baseline is frozen. |

Every milestone must preserve the release gates above and retain dual
validation until the relevant authority is proven stable.

## Commit strategy

This is the auditable execution plan for the ladder. Each commit has one
semantic purpose and must be independently revertible. A later commit may
consume an earlier sealed contract, but must not smuggle a new authority,
control, or interpretation into that contract.

### Revised visible roadmap

The following order is unchanged through the current qualification boundary:

```text
1. Artifact 001 recapture and rebaseline
2. Tag the exact Artifact 001 producing commit
3. Control ontology split
   E = Experiment · H = presentation only · G = commit/freeze observation
   Q = sealed interrogation · POSE HELD
4. Spatial Kernel milestone 1
   FrozenGeometrySnapshot · stable primitive identity · pure C# linear scan
   intersection authority · dual validation first
5. BVH-v0
   acceleration only; no semantic change
6. Godot 4.7.1 qualification
7. Wormhole Traversal baseline branch: WT-0 through WT-5
8. Optional WT-6 spacing diagnostic
9. Separate extension branch: rotating/modal throat experiments
```

WT-0 documentation may be reviewed now because it has no runtime effect. WT-1
and later implementation commits are blocked until item 6 is complete unless
a release owner explicitly creates an isolated research branch. That exception
must not merge or reorder the production roadmap.

### Dependency ordering

```text
release Artifact 001
        ↓
control ownership + sealed observation contract
        ↓
Spatial Kernel immutable geometry + pure intersection authority
        ↓
BVH-v0 validation as acceleration-only
        ↓
Godot 4.7.1 qualification
        ↓
WT-1 pure throat coordinate model
        ↓
WT-2 deterministic traveler
        ↓
WT-3 synchronized geometry instruments
        ↓
WT-4 optical transport adapter
        ↓
WT-5 Wormhole Traversal artifact baseline
        ↓
WT-6 optional train diagnostic
        ↓
extension branch: rotating/modal variants
```

The critical dependency is not visual: WT-4 must use the same frozen geometry
and Spatial Kernel identity that WT-1 through WT-3 qualify. BVH may replace a
linear scan for performance only after identical semantic results are shown.

### WT-0 — scope only

**Commit:** `docs(observatory): scope continuous wormhole traversal instrument`

**Purpose:** Record the experiment thesis, scientific status, ownership,
dependencies, non-goals, and sequencing boundary.

**Expected files/modules:** This document and, if needed later, a navigation
link under the existing Portable Observatory documentation lane.

**Must not change:** Runtime code, scenes, controls, transport math, Spatial
Kernel behavior, Artifact 001 output, tags, or claim vocabulary elsewhere.

**Acceptance:** Markdown lint/diff check; document names `ell`, `r(ell)`,
proper distance, external separation, stable authority, artifact provenance,
and the baseline/extension boundary.

**Artifact/provenance:** None generated. The document is a planning artifact.

**Rollback boundary:** Revert the documentation commit; no runtime rollback is
needed.

### WT-1 — intrinsic throat coordinate model

**Commit:** `feat(spatial): add intrinsic throat coordinate model`

**Purpose:** Add only pure math/data for signed `ell`, throat radius `r0`,
`r(ell)`, mouth bounds, minimum throat, proper path length, and deterministic
parameter serialization.

**Expected files/modules:** Proposed `src/XPrimeRay.Core/WormholeTraversal/`
types, pure tests, and a versioned canonical serializer. No Godot scene or
camera adapter.

**Must not change:** Rendering, controls, player motion, ray queries, collision
semantics, ProbeOutcomeCode, BVH, snapshot lifecycle, or teleport behavior.

**Acceptance:**

- `r(0) == r0` and the throat derivative condition hold within declared
  numerical tolerance;
- radius increases toward both exteriors for the chosen baseline profile;
- mouth bounds and proper path length are explicit and validated;
- parameter serialization is fixed-order, locale-independent, deterministic,
  and hash-stable;
- invalid bounds and non-positive throat radius fail closed;
- repeated runs produce identical model and serialization hashes.

**Artifact/provenance:** Store model schema/version, all parameters, units,
and canonical parameter hash in test fixtures only. No public capture yet.

**Rollback boundary:** Revert the pure Core module and its tests. Existing
Observatory behavior must be byte- and image-stable.

### WT-2 — deterministic manifold traveler

**Commit:** `feat(observatory): add deterministic manifold traveler`

**Purpose:** Add a visual-independent traveler state machine/worldline that
progresses continuously across `ell = 0`, tracks proper time/local clock,
velocity, side/region, orientation, and replay state.

**Expected files/modules:** Pure traveler state/integration types adjacent to
the WT-1 model, deterministic replay tests, and a small adapter DTO if needed.

**Must not change:** Godot locomotion, camera input, optical rays, scene
transforms, render scheduling, controls, or current spatial intersection
authority.

**Acceptance:**

- a fixed seed and policy produce identical state traces and hashes;
- `ell` is continuous and crosses zero without a mouth-to-mouth jump;
- proper/local clock and velocity evolve according to the declared policy;
- side/region transitions occur only at declared bounds;
- pause/resume and replay preserve the same trace;
- invalid integration state is explicit and not silently clamped into a claim.

**Artifact/provenance:** A replay trace fixture records initial state, policy,
step count, units, and state hash. It is not yet a visual artifact.

**Rollback boundary:** Remove the traveler module and replay fixtures; WT-1
model and production transport remain intact.

### WT-3 — synchronized throat traversal geometry instruments

**Commit:** `feat(observatory): instrument throat traversal geometry`

**Purpose:** Present the traveler worldline, `r(ell)` profile, signed
proper-distance ruler, and external-vs-intrinsic separation as synchronized
instrument views.

**Expected files/modules:** ObservationLayer channel descriptors, pure view
mapping/read models, Observatory host adapter, and focused tests. Keep the
worldline/profile math outside Godot UI code.

**Must not change:** Optical transport, player locomotion semantics, spatial
intersection authority, current Probe Views, current Artifact 001 channels,
or any speculative throat behavior.

**Acceptance:**

- all four views consume one traveler generation and one sealed state;
- `ell = 0` and minimum `r` align across ruler/profile/worldline views;
- external separation and proper path length are displayed as distinct fields;
- changing a view does not rerun traversal or mutate source channels;
- incomplete or invalid traveler state displays unavailable, not fabricated 0;
- repeated mapping produces identical pixels and hashes.

**Artifact/provenance:** A diagnostic frame may record channel IDs, units,
generation, model hash, and mapping hash. It is a qualified instrument preview,
not the Wormhole Traversal baseline artifact.

**Rollback boundary:** Revert the view adapters and channels without touching
WT-1/WT-2 state or existing Cathedral artifacts.

### WT-4 — optical transport coupling

**Commit:** `feat(optics): trace traveler view through throat manifold`

**Purpose:** Connect traveler camera and rays to the intrinsic throat geometry
through Spatial Kernel authority.

**Expected files/modules:** Optical adapter, ray-to-manifold coordinate bridge,
dual-validation harness, and transport-specific tests. `GrinFilmCamera` may
adapt the result but must not own a second semantic intersection path.

**Must not change:** WT-1 geometry semantics, WT-2 traveler integration,
existing Cathedral contact authority, ProbeOutcomeCode taxonomy, BVH meaning,
or control ontology.

**Acceptance:**

- every optical query declares the frozen geometry snapshot and stable
  primitive identity used;
- pure linear-scan and any host/accelerated result agree under dual validation;
- optical traces are deterministic across replay and fresh processes;
- no teleport-style camera discontinuity is introduced;
- view changes do not request transport or alter the traveler trace;
- failures identify unavailable/invalid transport rather than falling back to
  decorative tunnel imagery.

**Artifact/provenance:** Record geometry snapshot hash, authority token,
intersection/replay hashes, ray policy, camera basis, and validity counts.

**Rollback boundary:** Disable only the wormhole optical adapter and return to
WT-3 geometry instruments; do not roll back Spatial Kernel or BVH contracts.

### WT-5 — baseline Wormhole Traversal artifact

**Commit:** `docs(observatory): baseline Wormhole Traversal Artifact 001`

**Purpose:** Freeze the first reproducible static, non-rotating baseline.

**Expected files/modules:** Capture recipe, manifest/schema documentation,
run-summary template, and generated artifacts only in an explicitly excluded
output directory. No new geometry or UI behavior.

**Must not change:** WT-4 semantics, controls, transport policy, current
Artifact 001 bundle, rotating/modal extensions, or source measurements.

**Acceptance:**

- one deterministic recipe produces the same model, traveler, ray, source,
  mapped, and display hashes;
- manifest separates SceneId, scene path, engine commit, model hash, traveler
  state, geometry snapshot, authority token, and acquisition generation;
- all parameters have units and provenance;
- worldline, profile, ruler, separation, and camera stills come from the same
  sealed generation;
- known limitations and scientific-status labels are present;
- incomplete, stale, or mixed-generation captures fail closed.

**Artifact/provenance:** This is the first public `qualified_visualization`
bundle for the wormhole lane. It is not scientific evidence without a separate
bounded claim.

**Rollback boundary:** Remove the recipe/docs and uncommitted generated
outputs. Preserve the WT-4 implementation and its qualification evidence.

### WT-6 — traversal spacing diagnostic

**Commit:** `feat(observatory): add traversal spacing diagnostic`

**Purpose:** Add a fixed-interval particle/train diagnostic to distinguish
coordinate bunching from changes in proper spacing.

**Expected files/modules:** Pure train generation and spacing channels, a
separate readout/view, replay tests, and an artifact extension.

**Must not change:** The single-traveler baseline artifact, static throat
geometry, optical authority, controls, or speculative extension branch.

**Acceptance:**

- train injection intervals are explicit in coordinate and proper-time terms;
- particle identities remain stable through `ell = 0`;
- coordinate spacing and proper spacing are reported separately;
- a fixed replay preserves ordering, identities, and hashes;
- the diagnostic does not call bunching evidence of a physical effect.

**Artifact/provenance:** Train artifacts use a new schema/version or explicit
optional section and cannot overwrite the WT-5 baseline.

**Rollback boundary:** Remove only train channels, views, and artifacts; WT-5
remains a valid single-traveler specimen.

### Baseline and extension branches

The **baseline branch** ends at the frozen WT-5 static, non-rotating,
Morris–Thorne-inspired artifact. WT-6 is an optional diagnostic on that branch.

The **extension branch** starts only after WT-5 is tagged and contains separate
commits for rotating geometry, frame-dragging-like optical shear, helical
phase/modal structures, Hinductor-inspired analogies, ZPE-threshold toy
models, and ER=EPR visualization concepts. Each must be labeled an
**exploratory simulation construct** unless it cites a specific established
physical model. No extension may silently change the baseline scene,
parameters, authority token, or artifact interpretation.

## Recommended tag points

Tags should identify reproducible boundaries rather than broad development
periods:

| Tag point | Recommendation | Purpose |
|---|---|---|
| Cathedral Artifact 001 | `observatory-artifact-001-rebaseline` on the exact producing commit | Freezes the current contact authority and presentation specimen before new work. |
| Spatial Kernel 1 | `observatory-spatial-kernel-v1` after dual validation passes | Identifies the first immutable geometry/intersection authority. |
| BVH-v0 | `observatory-bvh-v0` only after acceleration-vs-linear parity | Makes the no-semantic-change boundary auditable. |
| Godot qualification | `observatory-godot-4.7.1-qualified` | Gates wormhole implementation work. |
| Wormhole baseline | `observatory-wormhole-traversal-001` on WT-5’s exact producing commit | Freezes the static baseline before WT-6 or extensions. |

Tags are proposals; they must not be created by WT-0 or WT-1 automatically.

## Sequencing safety review

The proposed sequencing is safe only if the following remain true:

- WT-1 does not infer geometry authority from the current Godot physics path;
- WT-2 does not reuse player locomotion as the deterministic traveler;
- WT-3 remains a projection layer and does not introduce controls or optical
  transport;
- WT-4 consumes Spatial Kernel snapshots and does not bypass them;
- WT-5 cannot overwrite or relabel Artifact 001;
- WT-6 is optional and cannot alter the single-traveler baseline;
- all rotating/modal work is branched after WT-5.

The main architectural risk is premature coupling of the intrinsic model to
`GrinFilmCamera` or the current linked-portal scene. That would create a second
spatial truth source and make later BVH and Godot 4.7.1 qualification unsafe.
The smallest safe response is to keep WT-1 and WT-2 pure, require a frozen
geometry snapshot at WT-4, and keep the existing roadmap gates ahead of all
runtime Wormhole Traversal commits.

## Explicit non-goals

- No runtime implementation in WT-0.
- No instantaneous traveler teleport trigger as the baseline model.
- No mesh deformation used as a substitute for spatial geometry.
- No new spatial intersection authority bypassing Spatial Kernel.
- No semantic dependency on BVH; BVH is acceleration only.
- No changes to current contact authority, ProbeOutcomeCode, controls,
  snapshot lifecycle, or Artifact 001 release sequence.
- No rotating throat, spin transport, azimuthal shear, helical flux, ZPE,
  Hinductor, ER=EPR, or physical wormhole claim in the baseline.
- No inference of geometry, proper distance, or traversal state from RGB,
  legacy magenta, viewport pixels, or decorative tunnel effects.
- No hidden external-separation or proper-length constants.
- No scientific claim that the simulation validates GR, optical closure,
  UAPs, or an observable physical wormhole.

## Qualification and artifact contract

Before WT-5 can qualify, a fixed traveler recipe must record:

- semantic SceneId and scene path separately;
- all manifold, traveler, camera, ray, and policy parameters;
- geometry and field/boundary epochs where applicable;
- one frozen geometry snapshot and one acquisition generation;
- source channel hashes before and after mapping;
- deterministic replay status and intersection-authority identity;
- external separation and intrinsic proper path as separate measured fields;
- validity and claim-boundary text for every view.

The artifact class is **qualified visualization** unless a separate bounded
evidence claim is approved. The views are synchronized projections of sealed
channels, not independent physical measurements.

## Future / speculative extensions

Only after the static non-rotating baseline is frozen may separate experiments
consider:

- rotating throats and azimuthal shear;
- phase or helical modal structure;
- Hinductor-inspired modal analogies;
- ER=EPR visualization concepts.

Each extension must carry its own scientific-status label and must not alter
the baseline artifact or silently share its interpretation vocabulary.

## Architectural conflicts

No conflict is introduced by WT-0. The only required constraint is sequencing:
the existing Artifact 001, control ontology, Spatial Kernel, BVH-v0, and Godot
qualification gates remain authoritative. The current linked-mouth/portal
prototype remains a separate implementation baseline; this scope does not
retroactively relabel it as a full throat-aware manifold.
