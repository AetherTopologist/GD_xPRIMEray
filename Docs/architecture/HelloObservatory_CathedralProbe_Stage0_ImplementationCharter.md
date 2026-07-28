# Hello Observatory 1.5 — Cathedral Probe Stage 0 Implementation Charter

**Status:** Implementation Contract  
**Mode:** SNAPSHOT only  
**Scope:** Pure C# core · 15 decisions locked  

---

## D1 — Stage 0 Outcome Taxonomy

The taxonomy below is the complete vocabulary for Stage 0. Derived from pass-1 transport state, the NaN guard in the step loop, and a bounded surface classification attached to the pass-1 hit payload. No RGB pixel inspection, no shaded-pixel inspection, no texture readback.

| Code | Value | Source | Resolved? | Refineable? |
|---|---|---|---|---|
| `Unprocessed` | 0 | Initial state — pixel not yet computed in this pass | — | — |
| `HitGeometry` | 1 | `Pass1HitInfo.SurfaceClass == Geometry` and no higher-precedence unresolved/error state | **Yes** | No |
| `BackgroundResolved` | 2 | `Pass1HitInfo.SurfaceClass == Background`, or no hit without a higher-precedence unresolved/error state | **Yes** | No |
| `MaxStepsExhausted` | 3 | `_pass1MaxStepsReached == true` — *primary unresolved target* | No | **Yes — primary** |
| `StoppedEarlyAbsorbed` | 4 | `_pass1StoppedEarly == true` (absorbed inside inner radius, no geometry contact) | No | Yes — secondary |
| `NumericalFailure` | 5 | NaN or overflow detected during integration (new flag on `Pass1HitInfo`) | No | **No — error class** |
| `Invalid` | 255 | Should never appear in a completed pass — corrupted write or buffer misalignment | Error | No |

### Mapping from pass-1 state

Written in `ProcessPass1Pixel` after the step loop returns, before `ShadeChunk()` is called. Evaluated in this priority order:

```csharp
if      (hitInfo.HadNumericalFailure)             outcome = NumericalFailure;
else if (maxStepsReached)                         outcome = MaxStepsExhausted;
else if (stoppedEarly)                            outcome = StoppedEarlyAbsorbed;
else if (hitInfo.SurfaceClass == Geometry)        outcome = HitGeometry;
else if (hitInfo.SurfaceClass == Background)      outcome = BackgroundResolved;
else if (!hitInfo.Found)                          outcome = BackgroundResolved;
else                                              outcome = Invalid;
```

`NumericalFailure` is checked first — a pixel that had both a NaN and a subsequent hit must be flagged as `NumericalFailure`. The hit result under numerical corruption is not trustworthy.

`MaxStepsExhausted` is checked before resolved surface classes. If a pass exhausts its configured step budget, the outcome remains `MaxStepsExhausted` even when a background or geometry collider was incidentally contacted during that exhausted pass. This preserves the unresolved target for D11 and later region analysis.

`Pass1HitInfo.Found` means physics collider contact only. It is not sufficient to establish `HitGeometry`. Stage 0 requires `Pass1HitInfo.SurfaceClass`, populated from the same source/background collider identity mechanism already used by fixture diagnostics.

---

## D2 — Predicates and Behaviors

### Unresolved predicate

```
outcome == MaxStepsExhausted || outcome == StoppedEarlyAbsorbed
```

These pixels produced no geometry answer. A deeper transport attempt may produce a different result.

### Selectable-region predicate

A connected component is selectable only if its dominant outcome (majority code across pixels) is `MaxStepsExhausted`. Regions composed primarily of `StoppedEarlyAbsorbed` are included in the unresolved mask but marked with a distinct display state and excluded from the P-refine action unless explicitly enabled in a future stage.

`StoppedEarlyAbsorbed` (absorbed inside inner radius) is unlikely to resolve with more integration steps — the absorption condition is a transport policy, not a budget limit. Stage 0 does not implement altered policy for this class.

### NumericalFailure behavior

`NumericalFailure` pixels are **excluded from region analysis**. They are counted separately in `ProbeFrameSummary.NumericalFailureCount`. They are displayed as a distinct overlay color (yellow-warning, not magenta) if the failure count exceeds zero. They are never included in a refinement request. The evidence bundle records their count and the context key so they can be reproduced for investigation.

### BackgroundResolved behavior

`BackgroundResolved` is fully resolved. It includes clean no-hit background resolution and declared background-collider contacts when no higher-precedence unresolved/error state applies. Displayed as the current SkyColor or existing background diagnostic presentation — identical to pre-Cathedral-Probe behavior. Does not appear in the unresolved overlay. Does not participate in region analysis. The outcome byte is recorded, enabling future evidence queries.

### Topology-candidate status

**Deferred to Stage 2.** Stage 0 makes no claim that any unresolved region corresponds to a transport topology feature. The region is described only as "unresolved under the current probe policy." The mandatory UI wording enforces this boundary.

---

## D3 — Enum Ownership

**Decision: separate `ProbeOutcomeCode` — do not extend `RayTerminationReason`.**

`ProbeOutcomeCode` is a new standalone `byte` enum defined in `CathedralProbe/ProbeOutcomeCode.cs`. It does not extend `RayTerminationReason`.

### Ownership consequence

`RayBeamRenderer.cs` is a protected file in Stage 0, with one exception:

| File | Change | Reason |
|---|---|---|
| `RayBeamRenderer.cs` | Add `bool HadNumericalFailure` to `Pass1HitInfo` struct only. No enum extension. | The NaN detection flag must travel with the hit result. |
| `RayTerminationReason` enum | Zero changes. No new values. | Transport enum is a transport concern. Cathedral Probe classification is a film-pipeline concern. |

### Why not extend RayTerminationReason?

- `RayTerminationReason` describes what the transport engine did. `ProbeOutcomeCode` describes what the probe concluded. These are different layers.
- The transport enum may gain future values for transport reasons that have no probe significance. The probe enum may gain future values for probe reasons that are not transport concepts.
- Extending the transport enum would create a compile-time dependency from `RayBeamRenderer` on Cathedral Probe semantics — a boundary violation.
- The mapping (transport booleans → probe code) belongs to Cathedral Probe.

### NaN detection addition

Before the existing clamp at `RayBeamRenderer.cs:3620`: detect `float.IsNaN(aSum)`, set a thread-local bool `hadNan = true`. After the step loop: `Pass1HitInfo.HadNumericalFailure = hadNan`. This is the complete change to the transport layer for Stage 0.

---

## D4 — ProbeContextKey Definition

A `readonly struct` implementing `IEquatable<ProbeContextKey>`. Field-by-field equality. All comparisons are exact — no epsilon on quantized values. Quantization is applied at construction, not at comparison.

```csharp
readonly struct ProbeContextKey : IEquatable<ProbeContextKey>
{
    // Camera
    public readonly uint   CameraOriginHash;   // FNV-1a of (round(x,3), round(y,3), round(z,3))
    public readonly uint   CameraBasisHash;    // FNV-1a of forward/right/up quantized to 0.0001
    public readonly float  FovDeg;             // exact float from Camera3D

    // Resolution
    public readonly ushort FilmWidth;
    public readonly ushort FilmHeight;

    // Transport policy (base pass)
    public readonly ushort BaseStepsPerRay;    // from SharedSnapshot
    public readonly float  BendScale;          // quantized to 0.00001
    public readonly float  FieldStrength;      // quantized to 0.001

    // Scene state epochs
    public readonly uint   FieldSourceEpoch;   // see D5 — must be reliable before use
    public readonly uint   GeometryEpoch;      // manually incremented by adapter on scene change
    public readonly uint   BoundaryLayerEpoch; // increments on BoundaryLayerState change

    // Probe policy
    public readonly byte   RefinementPolicyVersion;
}
```

`BoundaryLayerEpoch` is **included** — `BoundaryLayerState` affects which pixels receive `StoppedEarlyAbsorbed`, so boundary configuration changes invalidate prior outcomes.

Shading mode is **excluded** — shading is downstream of probe classification and does not affect the outcome plane.

> **Prerequisite:** ProbeContextKey cannot be trusted until runtime probe D11.2 confirms that `FieldSourceEpoch` increments on every field source mutation path.

---

## D5 — Invalidation Rules

**Invalidation** means: set all `_probeOutcomes[i] = ProbeOutcomeCode.Unprocessed`, zero all `_probeRefinLevel[i]`, clear the `List<ProbeRegionRecord>`, set probe status to `STALE`. Do **not** clear `_img` — the display continues showing the last rendered frame.

| Event | Key fields that change | Invalidation | Notes |
|---|---|---|---|
| Camera movement | CameraOriginHash, CameraBasisHash | **Full reset** | Skip reset if delta < 0.001m and angle < 0.01° |
| FOV change | FovDeg | **Full reset** | |
| Field dial tick | FieldStrength | **Full reset** | Ticks < 0.001 absorbed by quantization — no reset |
| Field-source mutation | FieldSourceEpoch | **Full reset** | Depends on epoch counter reliability (D11.2) |
| Preset switch (Gallery ↔ Hermetic) | CameraOriginHash + CameraBasisHash + GeometryEpoch | **Full reset** | Changes camera and collision geometry simultaneously |
| Geometry change | GeometryEpoch | **Full reset** | GeometryEpoch incremented manually by adapter |
| Resolution change | FilmWidth, FilmHeight | **Full reset + reallocation** | Array sizes change |
| SNAPSHOT → LIVE transition | FilmWidth, FilmHeight (scale 1.0 → 0.5) | **Full reset + reallocation** | LIVE is a non-goal for Stage 0 — set STALE only |
| LIVE → SNAPSHOT transition | FilmWidth, FilmHeight (scale 0.5 → 1.0) | **Full reset + reallocation** | New base pass begins |
| Probe policy change | RefinementPolicyVersion | **Full reset** | Prior refinement levels no longer comparable |
| BendScale change | BendScale | **Full reset** | Field curvature shape changes |
| Shading mode change | *(none — not in key)* | **No reset** | Shading is downstream |
| SkyColor change | *(none)* | **No reset** | SkyColor is a shading parameter |

---

## D6 — Stage 0 Lifecycle

Stage 0 runs in **SNAPSHOT mode only**. LIVE mode is a non-goal.

1. **Entry.** User selects SNAPSHOT mode. GrinFilmCamera prepares for 12 RenderStep() calls at FilmResolutionScale=1.0 (160×90).
2. **Key capture.** At start of first RenderStep(), `CathedralProbeEngine` computes the current `ProbeContextKey`. If it matches and outcomes are not all `Unprocessed`, skip to step 5. If different: full reset (D5), then proceed.
3. **Base pass — PROBING.** 12 RenderStep() calls execute. After each band, `ProcessPass1Pixel` writes one `ProbeOutcomeCode` byte per pixel. Status: `PROBING`.
4. **Pass finalization.** After RenderStep 12: assert no `Unprocessed` pixels remain. Compute `ProbeFrameSummary`.
5. **Selected-index transport availability.** A caller-supplied set of pixel indices can be rerun through the production selected-index executor under a supplied policy and context key. This is enabling transport infrastructure only: it does not select regions, orchestrate refinement, mutate the base film, update `_probeOutcomes`, or write evidence.
6. **Region analysis.** Call `ProbeRegionAnalyzer.Analyze()`. Produces `ushort[] _regionLabels` and `List<ProbeRegionRecord>` sorted by PixelCount descending. Status: `READY`.
7. **User navigation.** J/K scroll through ranked regions. Adapter displays selected region bounding box outline. Telemetry shows region index, pixel count, dominant outcome, refinement level.
8. **Selected-region refinement request — REFINING.** A caller requests a concrete `regionId`. The request is accepted only after a complete SNAPSHOT, for the current `ProbeContextKey`, frame generation, film dimensions, and a non-empty `_regionLabels[]` membership set. Pixel indices are copied from `_regionLabels[]` in deterministic row-major order.
9. **Selected-index execution.** The existing selected-index executor reruns exactly those caller-selected pixels under the bounded `ProbePolicy` refinement effort. It is pumped synchronously from the safe process point established in Commit 3. It does not mutate `_img`, `_probeOutcomes`, `_probeRefinLevel`, labels, display textures, or transport policy while executing.
10. **Atomic refinement application.** After execution, validation rechecks context, frame generation, dimensions, result count, index correspondence, duplicate absence, and bounds. Any failure applies nothing and records a deterministic failure reason. Success replaces the selected pixels' `ProbeOutcomeCode`, updates `_probeRefinLevel` monotonically, reruns `ProbeRegionAnalyzer.Analyze()`, and refreshes `ProbeFrameSummary`.
11. **Result reporting.** Emit a concise `ProbeRefinementSummary` evidence block with pre/post frame counts, selected/applied/resolved counts, transition histogram, child-region counts, context match, atomic-apply flag, and policy values. This is human-readable runtime evidence only, not an evidence-file export.
12. **Repeat or reset.** User may press P again (next refinement level, if ceiling not reached) or R (full reset → step 2).
13. **Evidence export.** Produces evidence bundle (D13) on user command or session end.

> **Synchronous constraint:** The refinement pass in step 7 runs on the calling thread. `MaxPixelsPerRequest` is the only budget cap — set conservatively (e.g., 500 pixels at 160×90).

---

## D7 — Pure C# Types

All types live in a `CathedralProbe/` subfolder of the Godot C# project. No file in this folder may contain `using Godot;`. Types must compile without Godot assemblies.

### ProbeOutcomeCode

```csharp
public enum ProbeOutcomeCode : byte
{
    Unprocessed          = 0,
    HitGeometry          = 1,
    BackgroundResolved   = 2,
    MaxStepsExhausted    = 3,
    StoppedEarlyAbsorbed = 4,
    NumericalFailure     = 5,
    Invalid              = 255,
}
```

### ProbePolicy

```csharp
public readonly struct ProbePolicy
{
    public readonly int   BaseStepsPerRay;       // source of truth: SharedSnapshot.StepsPerRay
    public readonly float BaseStepLength;        // source of truth: SharedSnapshot step length
    public readonly int   RefinedStepsPerRay;    // must be > BaseStepsPerRay
    public readonly float RefinedStepLength;     // must be <= BaseStepLength
    public readonly int   MaxPixelsPerRequest;   // synchronous budget cap per RequestRefinement call
    public readonly byte  MaxRefinementLevel;    // per-pixel ceiling
    public readonly byte  Version;               // increment on any field change
}
```

### ProbeRegionRecord

No heap arrays. Termination distribution stored as inline named fields — blittable, stack-safe.

```csharp
public struct ProbeRegionRecord
{
    public ushort Id;
    public int    PixelCount;
    public ushort MinX, MinY, MaxX, MaxY;
    public byte   MaxRefinementLevel;
    public bool   IsPrimarilyMaxStepsExhausted;  // selectable-region flag (D2)
    // Outcome distribution — inline, no allocation
    public int    CountHitGeometry;
    public int    CountBackgroundResolved;
    public int    CountMaxStepsExhausted;
    public int    CountStoppedEarlyAbsorbed;
    public int    CountNumericalFailure;
}
```

### ProbeFrameSummary

```csharp
public struct ProbeFrameSummary
{
    public int    TotalPixels;
    public int    HitGeometryCount;
    public int    BackgroundResolvedCount;
    public int    MaxStepsExhaustedCount;
    public int    StoppedEarlyAbsorbedCount;
    public int    NumericalFailureCount;
    public int    RegionCount;
    public int    SelectableRegionCount;          // IsPrimarilyMaxStepsExhausted == true
    public int    LargestRegionPixelCount;
    public ushort LargestRegionId;
    // Set after refinement (zero until first RequestRefinement call)
    public int    LastRefinementPixelsAttempted;
    public int    LastRefinementNewlyResolved;
    public int    LastRefinementStillUnresolved;
    public ProbeContextKey ContextKey;
}
```

### ProbeRefinementRequest

```csharp
public readonly struct ProbeRefinementRequest
{
    public readonly ushort RegionId;
    // Pixel list is not stored here — runtime iterates _regionLabels[] for matching pixels
}
```

### ProbeRefinementResult

```csharp
public struct ProbeRefinementResult
{
    public ushort RegionId;
    public int    PixelsAttempted;
    public int    NewlyResolved;
    public int    StillUnresolved;
    public byte   RefinementLevelReached;
    public bool   BudgetCapHit;       // true if MaxPixelsPerRequest was reached
    public bool   CeilingReached;     // true if MaxRefinementLevel blocked further probing
}
```

### ProbeRegionAnalyzer

```csharp
public static class ProbeRegionAnalyzer
{
    // Pure C# BFS flood-fill. No Godot dependencies. Caller allocates regionLabels.
    // NumericalFailure pixels are excluded from region formation.
    // Regions sorted by PixelCount descending before returning.
    public static void Analyze(
        int                             filmW,
        int                             filmH,
        ReadOnlySpan<ProbeOutcomeCode>  outcomes,
        Span<ushort>                    regionLabels,    // caller owns; length == filmW*filmH
        List<ProbeRegionRecord>         results);        // caller provides; Clear()d by method
}
```

---

## D8 — Allocation-Safe Data Layout

Three parallel persistent arrays in `GrinFilmCamera`. No struct-of-arrays, no per-pixel object heap allocations. No per-region pixel-membership arrays.

**Full-film persistent arrays (Cathedral Probe additions):**

| Array | Type | Bytes/px | 160×90 |
|---|---|---|---|
| `_probeOutcomes` | `ProbeOutcomeCode[]` | 1 | 14.4 KB |
| `_probeRefinLevel` | `byte[]` | 1 | 14.4 KB |
| `_regionLabels` | `ushort[]` | 2 | 28.8 KB |
| **Total new** | | **4** | **57.6 KB** |

**Allocation rules:**

- Allocated alongside existing full-film arrays in `GrinFilmCamera`
- Reallocated on resolution change (D5)
- Reset (memset to 0) on `ProbeContextKey` invalidation — not freed
- `_regionLabels` allocated during film-capacity initialization alongside `_probeOutcomes` and `_probeRefinLevel`
- BFS frontier: reusable `int[]` scratch allocated by `ProbeRegionAnalyzer` and reused across calls

**No per-region heap arrays.** When a refinement request arrives for region R, `CathedralProbeEngine` scans `_regionLabels[]` linearly (O(filmW×filmH)) to collect matching pixel indices into a reusable `int[]` scratch buffer. At 160×90 this is 14,400 iterations — imperceptible.

The `List<ProbeRegionRecord>` is cleared and repopulated on each `Analyze()` call. Capacity is not freed between analyses — the list retains its backing array.

---

## D9 — Godot Adapter API

The Godot adapter is a thin GDScript file (`Transport/CathedralProbeAdapter.gd`) that calls a C# surface on `GrinFilmCamera` (or a sibling `CathedralProbeGodotBridge` node). GDScript does not own any probe data.

### C# surface exposed to GDScript

| Method / Property | Returns | Description |
|---|---|---|
| `GetProbeStatus()` | String | One of: `"PROBING"`, `"READY"`, `"UPDATED"`, `"STILL_UNRESOLVED"`, `"STALE"` |
| `GetProbeSummaryDict()` | Dictionary | ProbeFrameSummary fields as string→Variant. No raw arrays. |
| `GetSelectedRegionInfo()` | Dictionary | Selected ProbeRegionRecord fields: id, pixelCount, minX, minY, maxX, maxY, maxRefinementLevel, isPrimarilyMaxStepsExhausted. |
| `GetRegionCount()` | int | Current number of selectable regions. |
| `GetSelectedRegionIndex()` | int | 0-based index in ranked region list. -1 if no selection. |
| `SelectRegionByOffset(int delta)` | void | Moves selection by delta (+1 = K, -1 = J). Wraps at bounds. |
| `RequestRefinementOfSelected()` | bool | Synchronous refinement of selected region. Returns false if ceiling reached or no selection. |
| `ResetProbe()` | void | Invalidates all outcomes, clears region list, sets status STALE. |
| `IncrementGeometryEpoch()` | void | Called by adapter on known geometry changes (collision layer toggle, preset switch). |
| `ExportEvidenceBundle(string directory)` | bool | Writes evidence bundle to directory (D13). Returns false if no completed pass exists. |

### What GDScript must not do

- Access `_probeOutcomes[]`, `_probeRefinLevel[]`, or `_regionLabels[]` directly
- Drive refinement logic or region-ranking order
- Store copies of `ProbeRegionRecord` data beyond what `GetSelectedRegionInfo()` returns
- Make decisions about when to invalidate — only call `IncrementGeometryEpoch()` in response to known scene events

---

## D10 — Controls and Wording

### Key bindings

| Key | Action | Condition |
|---|---|---|
| `P` | Probe deeper — `RequestRefinementOfSelected()` | Status == READY, UPDATED, or STILL_UNRESOLVED; selection active; ceiling not reached |
| `K` | Next region — `SelectRegionByOffset(+1)` | Status == READY, UPDATED, or STILL_UNRESOLVED; regionCount > 0 |
| `J` | Previous region — `SelectRegionByOffset(-1)` | Same as K |
| `R` | Reset probe — `ResetProbe()` | Any status |
| `O` | Toggle unresolved overlay visibility | Any status |

### Status labels

| Label | Color | Meaning |
|---|---|---|
| `PROBING` | Amber | Base pass running |
| `READY` | Green | Base pass complete, regions analyzed, awaiting user |
| `UPDATED` | Violet | Refinement resolved at least one pixel |
| `STILL UNRESOLVED` | Orange | Refinement ran, zero pixels newly resolved at this depth |
| `STALE` | Red | ProbeContextKey changed; outcomes no longer valid |

### Telemetry display

```
Unresolved: [count] px
Regions: [count] (selectable: [selectableCount])
Region [index+1]/[total]: [pixelCount] px | depth [level]/[maxLevel]
Resolved: [newlyResolved] px / [attempted] attempted   ← after refinement
```

### Overlay rendering

- **Unresolved overlay:** `MaxStepsExhausted` pixels as semi-transparent magenta over the film display. Toggle with O.
- **Selected region outline:** 1px white bounding box (MinX/Y, MaxX/Y from `GetSelectedRegionInfo()`). Updated on J/K. Hidden when no selection.
- **NumericalFailure overlay:** `NumericalFailure` pixels as semi-transparent yellow. Always visible when nonzero — not toggleable in Stage 0.

### Mandatory wording

The following string must appear verbatim in the probe panel, adjacent to the P key hint. It may not be paraphrased, softened, or moved below the fold.

> **"More steps = more attempt, not more truth."**

This is a scientific boundary statement, not a disclaimer.

---

## D11 — Three Prerequisite Runtime Probes

These experiments must complete before Stage 0 implementation begins. Each gates a specific implementation decision.

### D11.1 — Outcome distribution probe

**Gates:** termination taxonomy mapping (D1), region analysis input validation.

Add temporary logging in `ProcessPass1Pixel` after the step loop. After each full SNAPSHOT pass, print counts for each termination boolean combination. Run the wormhole scene at FilmResolutionScale=1.0.

**Pass condition:** `MaxStepsReached` count is nonzero and forms coherent image-space clusters verifiable by visual inspection. If zero: the magenta region is produced by a different mechanism — stop and investigate before proceeding.

### D11.2 — Field/geometry epoch reliability probe

**Gates:** `ProbeContextKey.FieldSourceEpoch` (D4), field-source mutation invalidation (D5).

Add logging to every property setter on `FieldSourceSnap` or any type that writes field source amplitudes, positions, or ROuter values. Also log `_fieldSourceLastRefreshFrame` assignments. Run the following sequence and confirm epoch increments on each step:

1. Move field dial (FieldStrength change)
2. Switch preset Gallery → Hermetic
3. Switch preset Hermetic → Gallery
4. Any external field source property change if such a path exists

**Pass condition:** Every mutation path produces a corresponding epoch increment that would change `ProbeContextKey`. If any mutation is silent: add an explicit epoch increment at that callsite before implementing `ProbeContextKey`.

### D11.3 — Selected-pixel rerun feasibility probe

**Gates:** refinement pass viability (D6 step 7), `RefinedStepsPerRay` selection for `ProbePolicy`.

After a SNAPSHOT pass, identify 10 pixels with `MaxStepsReached=true`. For each, manually construct and re-execute a single-pixel transport pass with `StepsPerRay` multiplied by 4 (e.g., 64→256). Log the original outcome, refined outcome, original/refined step counts, timing, and repeat determinism for each pixel.

**Pass conditions:**
- At least one pixel resolves to `HitGeometry` or `BackgroundResolved` — refinement is viable
- Report separately how many selected pixels transition to `HitGeometry` and how many transition to `BackgroundResolved`
- If zero selected pixels resolve, record the negative scientific result and stop before production refinement
- Rerun is deterministic — identical inputs produce identical outcome across two reruns
- Rerun time per pixel is bounded — informs `MaxPixelsPerRequest` setting

**Negative-answer condition:** If zero pixels resolve, the wormhole's magenta region may be a true geometric void, not a budget limitation. Do not proceed with refinement implementation. Escalate to architecture review — the Stage 0 research question may have a negative answer for this scene.

### D11.4 — Selected-index transport promotion

The selected-index transport executor was promoted from the D11.3 prerequisite probe into its own rollback commit between outcome recording and region analysis.

Rationale:

- Temporary full-frame reruns produced physics-space locking warnings when invoked from unsafe callbacks.
- Production refinement requires caller-selected pixels, not a second full-frame render.
- The executor preserves the base film, `_probeOutcomes`, and `_probeRefinLevel` while writing to caller-owned result storage.
- The executor validates the supplied `ProbeContextKey` before running and refuses stale context.
- Execution is synchronous and pumped from a safe process point. It is not threaded refinement, region selection, outcome application, UI, overlay, or evidence export.

Selected-index execution is enabling transport infrastructure only. Region selection and refinement orchestration remain separate later commits.

---

## D12 — File Manifest

### New files

| File | Type | Description |
|---|---|---|
| `CathedralProbe/ProbeOutcomeCode.cs` | enum | 7-value byte enum. No `using Godot;`. |
| `CathedralProbe/ProbeContextKey.cs` | readonly struct | IEquatable. All fields. No `using Godot;`. |
| `CathedralProbe/ProbePolicy.cs` | readonly struct | Policy fields + Version byte. No `using Godot;`. |
| `CathedralProbe/ProbeRegionRecord.cs` | struct | Inline distribution fields. No heap arrays. No `using Godot;`. |
| `CathedralProbe/ProbeFrameSummary.cs` | struct | All count fields + ContextKey embed. No `using Godot;`. |
| `CathedralProbe/ProbeRefinementRequest.cs` | readonly struct | RegionId only. No `using Godot;`. |
| `CathedralProbe/ProbeRefinementResult.cs` | struct | All result fields. No `using Godot;`. |
| `CathedralProbe/ProbeRegionAnalyzer.cs` | static class | BFS Analyze() method. Pure C#. No `using Godot;`. |
| `CathedralProbe/ProbeSelectedIndexResult.cs` | struct | Caller-owned selected-pixel rerun result. No `using Godot;`. |
| `CathedralProbe/CathedralProbeEngine.cs` | class | Orchestrator: key management, RequestRefinement, summary computation. No `using Godot;`. |
| `Transport/CathedralProbeAdapter.gd` | GDScript | Input handling, status label, overlay drawing, telemetry display. Calls C# surface only. |

### Modified files

| File | Change | Scope |
|---|---|---|
| `RayBeamRenderer.cs` | Add `public bool HadNumericalFailure` to `Pass1HitInfo` struct (lines 612–620). Set at NaN guard (~line 3620): detect `float.IsNaN(aSum)` before clamp, set local flag, propagate to `Pass1HitInfo` at loop end. | 2–3 lines total |
| `GrinFilmCamera.cs` | Allocate `_probeOutcomes[]`, `_probeRefinLevel[]`, `_regionLabels[]` alongside existing full-film arrays. In `ProcessPass1Pixel`: write ProbeOutcomeCode from hit info after step loop. Add safe selected-index transport infrastructure. After full SNAPSHOT pass: analyze unresolved regions. Later commits expose C# surface methods for adapter (D9) and wire `IncrementGeometryEpoch()` to preset switch callsite. | Additive, staged by rollback commit |

### Protected — zero changes

| File | Protection reason |
|---|---|
| `ObserverInstrumentationAdapter.cs` | OI seam — separate pipeline |
| All OI fixtures (OI-001 through OI-013A) | Existing acceptance tests must not regress |
| `Transport/FilmController.gd` | Cathedral Probe is C# only in Stage 0 |
| `Transport/TransportChamberPlayer.gd` | Spawn/camera contract unrelated |
| `Transport/TransportChamberWorld.tscn` | Scene geometry unchanged |
| `Transport/HermeticRoomDisplay.tscn` | Hermetic room unchanged |
| `Transport/FieldDialController.gd` | Field dial controls unchanged |
| `UI/ObservatoryWorkbenchPanel.gd/.tscn` | OI workbench UI — separate concern |
| `ObservatoryWorkbench.tscn` | Outer workbench — separate concern |
| `SharedSnapshot` struct body | No new fields — probe policy is separate |
| `RayTerminationReason` enum | Transport enum is untouched (D3) |
| Transport field math (RayBeamRenderer.cs lines 3540–4659, excluding NaN flag) | Transport mathematics are protected |

---

## D13 — Acceptance Tests and Evidence Bundle

### Acceptance tests (console fixture — no Godot display)

All assertions run on C# data structures. No screen capture. No pixel-color reading.

1. **Full pass coverage.** After SNAPSHOT pass: assert `_probeOutcomes[i] != Unprocessed` for all i.
2. **MaxStepsExhausted is nonzero.** Assert `summary.MaxStepsExhaustedCount > 0`. Fail: wormhole scene produces no unresolved pixels — stop and investigate (see D11.3 negative-answer condition).
3. **Region analysis produces regions.** Assert `summary.RegionCount >= 1` and `summary.SelectableRegionCount >= 1`.
4. **NumericalFailure not in region labels.** After Analyze(): assert no pixel with `outcome == NumericalFailure` has a nonzero region label.
5. **Refinement is accepted.** Call `RequestRefinementOfSelected()`. Assert return true.
6. **Refinement level increments.** Find one pixel in the selected region; assert `_probeRefinLevel[i] == 1` after first refinement.
7. **Unresolved count is non-increasing.** Assert `result.NewlyResolved + result.StillUnresolved == result.PixelsAttempted`. Assert `result.NewlyResolved >= 0`. (Count may be zero — that is a valid scientific result, not a test failure.)
8. **Budget cap is respected.** Set `MaxPixelsPerRequest = 10`. Assert `result.PixelsAttempted <= 10`.
9. **Ceiling is respected.** Set `MaxRefinementLevel = 1`. Call RequestRefinement twice. Assert second call returns false.
10. **Context invalidation resets outcomes.** Mutate PolicyVersion (+1). Call key comparison. Assert new key != old key. Assert all `_probeOutcomes[i] == Unprocessed`.
11. **Determinism.** Run identical inputs twice. Assert `probeOutcomes_run1[i] == probeOutcomes_run2[i]` for all i. Assert `regionCount_run1 == regionCount_run2`.

### Evidence bundle

Written by `ExportEvidenceBundle(string directory)`. All files are required for a valid bundle.

| File | Format | Contents |
|---|---|---|
| `probe_context.json` | JSON | All `ProbeContextKey` fields as named key/value pairs. |
| `probe_summary.json` | JSON | `ProbeFrameSummary` — all count fields, refinement results if any. |
| `probe_outcomes.bin` | Raw bytes | `_probeOutcomes[]`, row-major, filmW × filmH bytes. Each byte = `ProbeOutcomeCode` value. |
| `probe_refinement_level.bin` | Raw bytes | `_probeRefinLevel[]`, row-major, filmW × filmH bytes. |
| `probe_regions.json` | JSON | Array of `ProbeRegionRecord` objects, sorted by PixelCount descending. |
| `probe_manifest.txt` | Text | Date, scene name, film dimensions, policy summary. Last line verbatim: `"More steps = more attempt, not more truth."` |

---

## D14 — Explicit Non-Goals

These items are out of scope for Stage 0 and must not be introduced. If a design choice during implementation creates dependency on any of these, the design is wrong.

1. **No LIVE mode refinement.** Cathedral Probe runs in SNAPSHOT mode only. Entering LIVE mode sets status STALE.
2. **No RGB-derived classification.** Unresolved pixel detection uses `ProbeOutcomeCode` exclusively. No code reads pixel color from `_img` or from the film texture.
3. **No pointer/click selection of regions.** J/K keyboard navigation only. No `InputEventMouseButton` in the probe adapter.
4. **No Python runtime in any path.** Python may be used offline for bundle analysis. It must not appear in any runtime path.
5. **No OI ownership of probe data.** `ObserverInstrumentationAdapter.cs` is not modified. The OI adapter does not read, store, or relay `ProbeOutcomeCode`, `ProbeRegionRecord`, or `ProbeFrameSummary`.
6. **No Blender adapter implementation.** `IIntersectionProvider` is not added in Stage 0. `PhysicsDirectSpaceState3D.IntersectRay()` remains the sole intersection backend.
7. **No transport/renderer math changes.** The field computation (line 4659), step loop (lines 3540–3905), NaN handling behavior (clamp continues), and all `SharedSnapshot` fields are unchanged.
8. **No topology claims.** Stage 0 produces no topology-candidate scoring. `ProbeRegionRecord` has no `TopologyCandidateScore` field. The UI, evidence bundle, and console output contain no topology claim.
9. **No async/threaded refinement.** The refinement pass is synchronous on the calling thread.
10. **No multi-region simultaneous refinement.** One `RequestRefinement` call targets one region. Batching is Stage 1.

---

## D15 — Ordered Commit Plan

Six commits. Each is a complete rollback point — reverting a commit leaves the codebase in a valid, buildable, test-passing state.

### Commit 1 — Rollback A · Data structures only

- **Files added:** All 8 pure C# types in `CathedralProbe/`
- **Files modified:** `RayBeamRenderer.cs` — add `bool HadNumericalFailure` to `Pass1HitInfo` struct; add NaN detection in step loop (2–3 lines)
- **Zero behavior change.** New types are unused. NaN flag is set but nothing reads it yet. All OI tests still pass.

### Commit 2 — Rollback B · Outcome recording

*Gate: D11.1 must have cleared before this commit.*

- **Files modified:** `GrinFilmCamera.cs` — allocate `_probeOutcomes[]` and `_probeRefinLevel[]`; populate `ProbeOutcomeCode` in `ProcessPass1Pixel`; compute `ProbeFrameSummary` after full SNAPSHOT pass; log summary to console
- **No display change.** Verification: SNAPSHOT pass completes; console prints outcome counts; `MaxStepsExhaustedCount` matches D11.1 probe results. All OI tests still pass.

### Commit 3 — Rollback C · Safe selected-index transport

*Promoted from D11.3 before region analysis.*

- **Files added:** `CathedralProbe/ProbeSelectedIndexResult.cs`
- **Files modified:** `ProbeContextKey.cs`, `GrinFilmCamera.cs`
- **Behavior:** add a caller-selected pixel rerun executor that accepts pixel indices, refined `StepsPerRay`, refined `StepLength`, and a current `ProbeContextKey`; writes to caller-owned result storage; validates context; preserves base film and persistent outcome/refinement buffers.
- **Non-goals:** no region selection, no refinement orchestration, no outcome application, no full-frame rerun, no UI, no evidence export.
- **Verification:** deterministic selected-index reruns agree across repeated runs and at least one selected exhausted pixel resolves to `HitGeometry` or `BackgroundResolved`. All OI tests still pass.

### Commit 4 — Rollback D · Region analysis

*Gate: D11.2 must have cleared before this commit.*

- **Files modified:** `CathedralProbe/ProbeRegionAnalyzer.cs`, `GrinFilmCamera.cs`, pure C# tests
- **Behavior:** allocate `_regionLabels[]` alongside film-capacity probe buffers; analyze `MaxStepsExhausted` components after complete SNAPSHOT outcome planes; update `ProbeFrameSummary` region counts.
- **Verification:** Console prints region count and largest region pixel count. Acceptance tests 1–4 pass. All OI tests still pass.

### Commit 5A — Rollback E1 · Selected-region refinement orchestration

- **Files added:** `CathedralProbe/ProbeRefinementSummary.cs`, `CathedralProbe/ProbeSeamDiagnosticPixel.cs`, `CathedralProbe/ProbeRegionRefinementEngine.cs`
- **Files modified:** `GrinFilmCamera.cs`, `ProbePolicy.cs`, pure C# tests
- **Behavior:** collect selected region pixels in row-major order, dispatch the existing selected-index executor, validate completion, atomically apply refined outcomes, update refinement levels, rerun region analysis, update `ProbeFrameSummary`, and print a concise human-readable refinement evidence block.
- **Seam diagnostic:** add a pure data contract for configured seam pixels. Disabled/unset by default; unavailable telemetry must be explicit rather than fabricated.
- **Non-goals:** no region-selection UI, mouse interaction, overlays, hatching, evidence-file export, LIVE-mode refinement, transport math change, or visual styling.
- **Verification:** pure orchestration tests cover row-major selection, rejection paths, atomicity, transition counters, child-region regeneration, deterministic failure reasons, seam unavailable telemetry, and bounded warm allocation. All OI tests still pass.

### Commit 5B — Rollback E2 · Reliable snapshot lifecycle

- **Files added:** pure snapshot lifecycle state/result types, pure lifecycle tests, and a maintained headless snapshot lifecycle harness.
- **Files modified:** `GrinFilmCamera.cs`, pure C# tests, and this charter.
- **Behavior:** add explicit snapshot lifecycle states (`Inactive`, `Requested`, `WaitingForPhysics`, `Capturing`, `Complete`, `Incomplete`, `Invalidated`, `Failed`), request generations, physics-safe waiting/start, bounded lifecycle budget, truthful terminal reasons, atomic complete-frame finalization, stable `CATHEDRAL SNAPSHOT` evidence output, and one maintained headless completion demonstration.
- **Boundary:** the current runtime has a single persistent probe outcome/region buffer. During capture, the frame is not refinement-eligible and must not be read as complete evidence; the previous terminal lifecycle result remains available until the replacement request reaches a terminal state.
- **Non-goals:** no diagnostics panel, user-facing keybindings, seam A/B/C calibration, refinement trigger, semantic overlays, evidence-file export, LIVE refinement, fixture calibration, or transport redesign.

### Commit 5C — Rollback E3 · Acceptance fixture and evidence export

- **Files modified:** future bounded fixture/reporting files only.
- **Behavior:** add the acceptance fixture and file evidence export once Commit 5B proves stable.
- **Non-goals:** no UI or overlay work.

### Commit 6 — Rollback F · Godot adapter and UI

- **Files added:** `Transport/CathedralProbeAdapter.gd` — P/J/K/R/O input handling; status label; unresolved overlay (magenta, yellow-NaN, white bounding-box outline); telemetry display; mandatory wording text
- **Verification:** Open `ObservatoryWorkbench.tscn` in editor — no errors on load. Enter SNAPSHOT mode — status shows PROBING then READY. Press J/K — region selection changes, bounding box visible. Press P — status changes to UPDATED or STILL UNRESOLVED. Press R — status returns to STALE. "More steps = more attempt, not more truth." visible adjacent to P key hint. All OI tests still pass.

> **Rollback guarantee:** The project builds and all OI acceptance tests pass at every commit boundary.
