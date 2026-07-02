# Project Optical Closure — Architecture Safety Audit

**Status:** Reconciled architecture · v0.2.preview  
**Scope:** Post-hit interpretive layer only · additive · no transport changes  
**Epistemic tier:** Validated fixture output (existing hit pipeline) · Experimental interpretation (OC-001 diagnostics)

> **This is a safety-critical architectural alignment document.**  
> Project Optical Closure extends the visual interpretive layer of xPRIMEray.  
> It does not replace or modify the validated transport / hit classification system.

---

## Reconciliation: Claude Prior Audit vs. Grok Correction

The prior audit proposed adding sphere intersection to Core's `TransportRunner` as a Phase 1
prerequisite. Grok's architectural review identified this as a category error: it would create a
parallel hit authority that competes with the existing validated hit system.

The corrected architecture is the middle ground below.

| Topic | Claude (prior) | Grok correction | **Middle-ground decision** |
|---|---|---|---|
| Sphere intersection in Phase 1 | Add analytic sphere test to `TransportRunner` | Use existing validated hit data; no new intersection authority | **No new intersection in Phase 1. Deferred to Stage 4 only if explicitly justified.** |
| UV computation source | Compute from Core-internal sphere test result | Infer probe sphere center from fixture metadata; compute UV from existing hit position | **UV computed from existing hit position + fixture geometry metadata (probe center only, no intersection code).** |
| `GeometryDefinition` in fixture schema | Add geometry[] for intersection dispatch | Add geometry[] as metadata only for UV computation | **Add geometry[] as probe metadata only — not intersection configuration. Schema clearly marks field as `probeMetadata`, not `geometry`.** |
| New surface data in `TransportResult` | Add `SurfaceMetrics[]` to `TransportResult` | Post-hit interpretation in film/diagnostic layer, not transport output | **Phase 1: film/diagnostic layer on Godot side. Core `SurfaceMetrics` deferred entirely.** |
| `AccessibilityClass` enum location | Core Transport layer alongside hit classification | Parallel diagnostic channel, not a transport class or replacement for hermetic closure | **Diagnostic overlay only. Not transport truth. Defined as post-hit interpretation, not a classification event.** |
| "Optically closed" as pixel class | Use directly | Use "closure candidate" or "probe region not sampled" until observer sweeps validate | **Use `probe_region_not_sampled` until validated by observer sweep. "Optical closure" is an interpretation, not a pixel classification.** |
| New channel registry entries | Add 3 channels to Core registry | Only if downstream diagnostic channels are additive and clearly scoped | **Additive diagnostic channels only, versioned separately. No existing channels modified. Schema explicitly labels these as diagnostic, not transport.** |
| New transport mode | `"optical_closure_v1"` in Core | No new transport mode in Phase 1; film/diagnostic layer dispatches post-hit | **No new transport mode in Phase 1.** |
| Hermetic closure rule | `AccessibilityClass.Unresolved` as catch-all | Hermetic closure is unchanged — optical accessibility is a separate diagnostic concept | **Hermetic closure rule untouched. Accessibility diagnostic does not replace or extend it.** |

!!! danger "Validated hit pipeline remains sovereign"
    The existing hit system — `RayBeamRenderer` → `HitPayload` → `HadHit` — is the only
    authoritative intersection event. No code in Phase 1 introduces a competing intersection path.
    Any UV, checker, dent-region, or accessibility interpretation is computed **after** a validated
    hit, using only the data that hit already produced.

---

## Corrected Architecture

### What "post-hit optical probe interpretation" means

After the validated transport run produces a `HitPayload` with `HadHit = true`, the following
data is already available with no new computation:

- `HitPayload.Position` — 3D hit point in world space
- `HitPayload.Normal` — surface normal at hit point
- `HitPayload.ColliderId` — which Godot object was hit
- `HitPayload.Distance` — path length to hit
- `HitPayload.TerminationReason` — transport classification
- `RaySeg[]` — full curved-ray path leading to the hit
- Observer pose from fixture — origin, forward, up, FOV

From these, Phase 1 computes **purely as interpretation**:

- Analytic spherical UV (from `HitPayload.Normal` or computed `hitPos - probeCenter`)
- Checker state at that UV coordinate
- Approximate dent-region membership
- Optical accessibility diagnostic class
- Overlay images (UV, checker, accessibility, hit classification)

### Where UV computation lives in Phase 1

For analytic spherical UV, no intersection test is needed:

```
1. Validated hit returns HitPayload.Position and HitPayload.ColliderId.
2. Look up probe metadata: which declared probe sphere does this ColliderId belong to?
   (Probe metadata is in the fixture or a lightweight Godot Resource/Node — not a hit system.)
3. Compute local surface vector: localNormal = normalize(hitPos - probeSphere.Center)
4. UV = SphericalUv.FromNormal(localNormal)
5. CheckerState = SphericalUv.CheckerState(u, v, tilesU, tilesV)
6. DentRegion = IsDentRegion(localNormal, dentLatitude)
```

Step 2 is a dictionary lookup by `ColliderId`, not a raycasting operation. No new intersection
code. The `ColliderId` was produced by the existing validated hit.

### Probe metadata (not intersection configuration)

The fixture or Godot scene declares probe sphere metadata for the sole purpose of post-hit
interpretation. This must be explicitly distinguished from intersection configuration.

```json
{
  "probeMetadata": [
    {
      "colliderName": "probe_sphere",
      "role": "probe",
      "center": [0, 0, 0],
      "radius": 0.8,
      "material": {
        "uvMode": "analytic_spherical",
        "checkerTilesU": 8,
        "checkerTilesV": 8,
        "hasPolarDent": true,
        "polarDentLatitude": 0.85,
        "polarDentDepth": 0.15
      }
    },
    {
      "colliderName": "reference_sphere",
      "role": "reference",
      "center": [-1.8, 0, 0],
      "radius": 0.6,
      "material": {
        "uvMode": "analytic_spherical",
        "checkerTilesU": 6,
        "checkerTilesV": 6
      }
    }
  ]
}
```

The key field is `colliderName` (or `colliderId`) — this is the link from the validated hit back
to the probe metadata. No intersection happens here.

### Optical accessibility as a diagnostic

Optical accessibility is **not** hermetic closure. It is **not** a hit classification. It is
**not** proof of hidden geometry.

Definition: *"A post-hit diagnostic describing whether a declared probe-region is visually sampled
by the existing validated transport result from a given observer pose."*

| Diagnostic State | Meaning |
|---|---|
| `probe_region_sampled` | Hit occurred on a declared probe collider, within the probe's declared active region |
| `probe_surface_hit_outside_region` | Hit on a probe collider but outside dent or declared region of interest |
| `other_geometry_hit` | Hit on a non-probe collider |
| `transport_class_not_surface_hit` | Transport classification was not a surface hit (miss, budget exceeded, etc.) |
| `diagnostic_unresolved` | ColliderId not found in probe metadata; seam condition; or other interpretation failure |

!!! warning "Use \"probe region not sampled\", not \"optically closed\""
    The term "optically closed" implies a physical claim about the geometry. Do not use it as a
    raw pixel classification until observer sweeps have validated that the transport model
    consistently produces this state across a range of observer positions. Until then, prefer
    `probe_region_not_sampled` or `closure_candidate`.

---

## Explicit "Do Not Touch" List

| Protected | Why |
|---|---|
| `RayBeamRenderer.cs` — hit detection, `RaySeg`, `HitPayload` | Validated hit pipeline. Sovereign. |
| `RendererCore/Transport/` — `IIntegrator`, `IMetricField`, `MetricHeuristicIntegrator` | Transport layer. No classification changes. |
| `RendererCore/Scheduling/ObjectSeededTileScheduler.cs` | Scheduling. No changes. |
| `GrinFilmCamera.cs` | Camera / pipeline lifecycle. No changes. |
| `SnapshotBuilder.cs` | Godot adapter. No changes. |
| `src/XPrimeRay.Core/Comparison/` — all existing 6 channels | Channel registry. Additive only. |
| `src/XPrimeRay.Core/Transport/TransportRunner.cs` — existing modes | No new transport modes in Phase 1. |
| `src/XPrimeRay.Core/Transport/TransportResult.cs` — existing fields | No `SurfaceMetrics` in Phase 1. |
| `src/XPrimeRay.Core/Transport/RayMetric.cs` | No new fields in Phase 1. |
| `Fixtures/grin_radial_smoke.json` and family | Protected production fixtures. |
| `reports/glowing_heart_v2_6_difference_packet_index.preview.json` | Evidence chain. Frozen. |
| `schemas/glowing_heart/difference_packet.v2.0.preview.json` | Frozen schema. |
| `src/XPrimeRay.Core/Validation/ClosureValidator.cs` | Hermetic closure definition. Untouched. |

---

## Revised Staged Roadmap

### Stage 0 — Documentation and Safety Framework *(no code)*

- Architecture safety audit (this document)
- Project Optical Closure overview page
- Epistemic airlock page
- Glossary page
- Roadmap page
- mkdocs.yml nav update

No code. No schema. No fixture. Pure documentation and alignment.

### Stage 1 — Post-Hit Procedural UV and Checker *(Godot film/diagnostic layer)*

All computation downstream of validated hit. No transport changes.

- `ProbeMetadataResource.cs` — lightweight Godot `Resource` (or plain class) mapping
  `ColliderId → (center, radius, role, MaterialParams)`. Read-only at runtime.
- `SphericalUvHelper.cs` — pure static: `FromNormal(Vector3) → (u, v)`,
  `CheckerState(u, v, tilesU, tilesV) → bool`, `IsDentRegion(localNormal, latitude) → bool`
- `OpticalProbeInterpreter.cs` — post-hit interpreter: takes `HitPayload`,
  `ProbeMetadataResource`, observer pose; produces `ProbeInterpretation` record
- `ProbeInterpretation` record: `UvU`, `UvV`, `CheckerState`, `DentRegionMember`,
  `DiagnosticState` (5-class enum above), `ProbeRole`, `ColliderName`
- No changes to `RayBeamRenderer`, transport, or hit detection

Acceptance: given any existing `HitPayload`, the interpreter produces a `ProbeInterpretation`
without calling any new intersection code.

### Stage 2 — Diagnostic Overlays and Observer Sweep *(Godot diagnostic output)*

- UV overlay image writer (PPM or PNG)
- Checker overlay image writer
- Accessibility diagnostic map writer (5-class false-color)
- Observer sweep table writer (CSV, per observer origin)
- Plain-language output report

These are new output writers attached to the existing film pipeline, not transport components.
They read from `ProbeInterpretation` records produced in Stage 1.

### Stage 3 — Optional Authored UV Lookup *(deferred; metadata-only)*

- Optional `Vector2[]` UV lookup table per probe, stored in `ProbeMetadataResource`
- UV interpolation from authored table, keyed by `localNormal` angular proximity (not barycentric)
- Still downstream of validated hit — no new intersection
- Only activated when authored UV table is present; falls back to analytic spherical otherwise

### Stage 4 — Experimental Geometry Branch *(only if explicitly justified)*

- Separate experimental geometry/mesh correspondence path in Core
- Explicitly NOT part of validated hit authority
- Explicitly labeled as experimental in all schemas and outputs
- Requires separate milestone, separate schema version, separate evidence chain
- Does not touch any existing fixture, channel, or comparison contract

---

## Updated Architecture: Critical Gaps (Revised)

| # | Gap | Layer | Phase 1 action | Phase 2+ action |
|---|---|---|---|---|
| G1 | No probe metadata in fixture | Fixture JSON / Godot resource | Add `probeMetadata[]` as metadata-only, no intersection | — |
| G2 | ~~No sphere intersection in Core~~ | ~~Core / TransportRunner~~ | **Not needed in Phase 1** | Stage 4 only if justified |
| G3 | ~~No surface data in RayMetric~~ | ~~Core / Transport~~ | **Not needed in Phase 1** | Stage 4 only |
| G4 | No UV lookup in film layer | Godot / film pipeline | `SphericalUvHelper` + `OpticalProbeInterpreter` | Authored UV table in Stage 3 |
| G5 | No accessibility diagnostic channel | Diagnostic output layer | `ProbeInterpretation.DiagnosticState` enum | May add to channel registry in Stage 2 |

G2 and G3 from the prior audit are explicitly **not gaps for Phase 1**. They were misclassified.

---

## New Types (Phase 1 Only)

These are the only new types needed in Phase 1. No Core transport changes.

```csharp
// Lightweight Godot Resource (or plain class). Not part of hit system.
public class ProbeMetadataResource
{
    // Map from ColliderName or GodotInstanceId → ProbeEntry
    public Dictionary<string, ProbeEntry> Probes { get; init; } = new();
    public ProbeEntry? FindByColliderName(string name) { ... }
    public ProbeEntry? FindByInstanceId(long id) { ... }
}

public record ProbeEntry
{
    public string ColliderName { get; init; } = "";
    public string Role { get; init; } = "probe";       // "probe" | "reference" | "field_volume"
    public Vector3 Center { get; init; }
    public float Radius { get; init; }
    public ProbeMaterialParams Material { get; init; } = new();
}

public record ProbeMaterialParams
{
    public int CheckerTilesU { get; init; } = 8;
    public int CheckerTilesV { get; init; } = 8;
    public bool HasPolarDent { get; init; }
    public float PolarDentLatitude { get; init; } = 0.85f;
    public float PolarDentDepth { get; init; } = 0.15f;
}

// Pure static math. No Godot dependency. No fixture dependency.
public static class SphericalUvHelper
{
    public static (float u, float v) FromNormal(Vector3 normal) { ... }
    public static bool CheckerState(float u, float v, int tilesU, int tilesV) { ... }
    public static bool IsDentRegion(Vector3 localNormal, float latitudeRadians) { ... }
}

// Output of post-hit interpretation. Not a transport type.
public record ProbeInterpretation
{
    public DiagnosticState State { get; init; }
    public string ColliderName { get; init; } = "";
    public string ProbeRole { get; init; } = "";
    public float UvU { get; init; }
    public float UvV { get; init; }
    public bool CheckerState { get; init; }
    public bool InDentRegion { get; init; }
    public Vector3 LocalNormal { get; init; }
}

public enum DiagnosticState : byte
{
    ProbeRegionSampled          = 0,
    ProbeSurfaceHitOutsideRegion = 1,
    OtherGeometryHit            = 2,
    TransportClassNotSurfaceHit  = 3,
    DiagnosticUnresolved        = 4
}
```

---

## Proposed Nav Structure

Update `Project OC-001` to `Project Optical Closure` with these sub-pages:

```yaml
  - Project Optical Closure:
      - Overview: xPRIMEray/optical_closure_overview.md
      - Architecture Safety Audit: xPRIMEray/project_oc_001_architecture_audit.md
      - OC-001 Minimal Optical Closure: xPRIMEray/oc_001_fixture.md
      - Epistemic Airlock: xPRIMEray/optical_closure_epistemic_airlock.md
      - Glossary: xPRIMEray/optical_closure_glossary.md
      - Roadmap: xPRIMEray/optical_closure_roadmap.md
```

---

## Updated Terminology

<div class="oc-term-grid">
  <div class="oc-term-card use">
    <div class="tc-head">Use</div>
    <ul>
      <li>validated hit remains sovereign</li>
      <li>post-hit optical probe interpretation</li>
      <li>transport-mediated appearance</li>
      <li>observer-dependent optical accessibility</li>
      <li>diagnostic overlay, not transport truth</li>
      <li>simulation-bounded claim</li>
      <li>inspiration, not evidence</li>
      <li>hermetic closure ≠ optical closure</li>
      <li>probe region not sampled</li>
      <li>closure candidate (until validated)</li>
      <li>reproducible fixture</li>
      <li>curiosity is welcome; conclusions are earned</li>
    </ul>
  </div>
  <div class="oc-term-card avoid">
    <div class="tc-head">Avoid</div>
    <ul>
      <li>new hit detection</li>
      <li>parallel truth system</li>
      <li>proof of portals</li>
      <li>proof of hidden geometry</li>
      <li>replacing classification</li>
      <li>extending validation claims</li>
      <li>optically closed (as pixel class before validation)</li>
      <li>this proves…</li>
      <li>hidden physics</li>
      <li>reality claims beyond simulation output</li>
      <li>destabilizing rabbit-hole framing</li>
    </ul>
  </div>
</div>

---

## Epistemic Tier Framework

Every claim on the site belongs to exactly one tier.

<div class="oc-tier-strip">
  <div class="oc-tier-cell oc-tier-math">
    <div class="oc-tier-name">Established Mathematics</div>
    <div class="oc-tier-desc">Spherical UV from atan2/asin. GRIN refraction index gradient. Ray bending under a radial field. Derived, not measured.</div>
  </div>
  <div class="oc-tier-cell oc-tier-engine">
    <div class="oc-tier-name">Implemented Engine Behavior</div>
    <div class="oc-tier-desc">How the validated transport and hit system behaves for a given scene. Reproducible; anyone can clone and run.</div>
  </div>
  <div class="oc-tier-cell oc-tier-valid">
    <div class="oc-tier-name">Validated Fixture Output</div>
    <div class="oc-tier-desc">A committed measurement from a pinned fixture run. The Glowing Heart evidence chain. OC-001 diagnostic outputs once verified by observer sweep.</div>
  </div>
  <div class="oc-tier-cell oc-tier-exp">
    <div class="oc-tier-name">Experimental Interpretation</div>
    <div class="oc-tier-desc">Observation that this transport configuration produces probe_region_not_sampled for a given observer. Reproducible and bounded — not a claim about physical reality.</div>
  </div>
  <div class="oc-tier-cell oc-tier-lore">
    <div class="oc-tier-name">Lore / Artistic Inspiration</div>
    <div class="oc-tier-desc">MisterY Labs mythology, portal lore, non-Euclidean spaces as inspiration. Never promoted to a higher tier without new evidence.</div>
  </div>
  <div class="oc-tier-cell oc-tier-open">
    <div class="oc-tier-name">Open Questions</div>
    <div class="oc-tier-desc">Things the simulation cannot answer. Named explicitly. Curiosity is welcome; conclusions are earned.</div>
  </div>
</div>

---

## Open Questions Before Implementation

!!! question "Q1 — Critical: Full structure of `GeometryEntitySOA`"
    The audit read only 12 lines: `WorldBounds: Aabb3[]` and `GodotInstanceIds: long[]`. Does it
    hold sphere primitives, vertex buffers, or UV arrays not visible in those lines? The answer
    determines Stage 3 authored UV table design.

!!! question "Q2 — Critical: Is `ColliderId` from `HitPayload` stable across scene reloads?"
    If the probe metadata lookup is keyed by `ColliderId` (Godot instance ID) and that ID changes
    across scene loads, the lookup breaks. Determine if `ColliderName` or a stable exported
    property on the probe node is the correct key.

!!! question "Q3 — High: Where exactly does `FilmOverlay2D` integrate post-hit data?"
    The `ProbeInterpretation` output needs to hook in downstream of `RayBeamRenderer` and upstream
    of the final film output. Confirm the integration point — does `FilmOverlay2D` have an
    existing extension slot, or does a new overlay node need to sit in the scene tree?

!!! question "Q4 — High: Does `ClosureValidator` in Core check per-pixel or per-aggregate?"
    Confirm whether adding `ProbeInterpretation` as an optional side-channel requires any
    ClosureValidator awareness, or if the two are fully independent.

!!! question "Q5 — Medium: Is `IGeometryQueryProvider` already defined?"
    Referenced in `spec_ray_transport_interfaces_1.md`. If present, the Stage 4 experimental
    geometry branch should implement it. Do not use it in Phase 1.

!!! question "Q6 — Medium: Does `HitPayload.Normal` reliably point outward for all probe geometries?"
    Analytic spherical UV from `HitPayload.Normal` assumes the normal points away from the sphere
    center. Verify this holds for convex geometry under the current Godot physics raycasting.

---

## What This Audit Does Not Claim

- This audit does not implement any code changes.
- It does not validate extraordinary geometric claims.
- It does not claim parity between Core and Godot transport.
- It does not claim scientific correctness of any OC-001 output.
- All OC-001 outputs are simulation-bounded claims. Reproducible fixture results are not proof of
  physical phenomena.
- Optical accessibility diagnostics are interpretive overlays. They do not modify, replace, or
  extend the validated hit pipeline's classification authority.
