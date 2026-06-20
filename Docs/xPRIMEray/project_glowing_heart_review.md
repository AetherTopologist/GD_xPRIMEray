# Project Glowing Heart — Engineering Review

**Document status:** Planning pass, 2026-06-20  
**Authored by:** AetherTopologist / Billy Broch + Claude Sonnet 4.6  
**Scope:** Architectural alignment review for the proposed xPRIMEray-Core extraction from GD_xPRIMEray  
**Source proposal:** `Docs/xPRIMEray/xPRIMEray-Core_Split.md`

---

## Decisions Needed Before Implementation

Five open questions gate downstream phases. Each is framed so that future contributors (human or AI) can evaluate the trade-off independently and record the verdict here.

| # | Decision | Option A | Option B | Recommendation |
|---|---|---|---|---|
| 1 | **Vector precision** | `System.Numerics.Vector3` (float — already in RendererCore) | Custom `Vec3` (double — as in Split proposal) | **Start with System.Numerics; gate upgrade to double on Phase 2 acceptance criteria** |
| 2 | **Repo structure** | Subfolder inside GD_xPRIMEray (mono-repo, Phases 1-6) | New standalone repo immediately | **Mono-repo until Phase 6; split at Phase 7** |
| 3 | **Fixture format** | Extend existing `sample_worlds/config.schema.json` | New fixture JSON schema as proposed in split doc | **New schema; sample_worlds schemas inform field names but are not directly reused** |
| 4 | **CLI output compatibility** | Observatory-compatible manifest from day one | New schema, migrate Python tools later | **Observatory-compatible from day one; Python tools are load-bearing** |
| 5 | **RenderTestRunner strategy** | Decompose (extract CLI parser + closure logic) | Wrap (thin Core adapter, keep Godot Node intact) | **Wrap first (Phase 5), decompose in Phase 6 when closure logic is extracted** |

---

## Executive Summary

**Can this split be executed safely?**

Yes. The architecture already has all three major adapter seams partially in place:

- `GodotAdapter/SnapshotBuilder.cs` (fully implemented, 16KB) — the scene-to-data bridge
- `RenderBackends/IRenderBackend.cs` (thin stubs) — the rendering abstraction
- `RendererCore/Transport/IMetricField.cs` + `IIntegrator.cs` — transport interfaces at Tier 0

The proposal in `xPRIMEray-Core_Split.md` describes what the architecture is already becoming. The extraction is an act of making the implicit explicit.

**Risk summary:**

| Layer | Risk | Effort |
|---|---|---|
| Math extraction (~18 files) | LOW — zero Godot deps, already pure System.Numerics | 1-2 days |
| Transport runner extraction | MEDIUM — SceneSnapshot ownership, RaySeg struct | 1-2 weeks |
| CLI Testbench | MEDIUM-HIGH — net-new, no headless path exists yet | 2-4 weeks |
| Repository split | LOW — structure is already separable | 1-2 days |
| Dual fixture maintenance | MEDIUM — .tscn + JSON in parallel creates drift risk | Ongoing |

**Key benefits:**
- Core math is verifiable independently of Godot version, platform, or rendering budget
- Fixture validation decouples from `godot.exe` (which is slow, platform-specific, and requires display server)
- A `dotnet test` of the transport core can run in CI in seconds instead of minutes
- Third-party adapters (Unreal, web WASM, Blender) become formally possible
- Physics and rendering concerns are impossible to accidentally entangle going forward

**Technical debt disappears:**
- The conflation of `RayBeamRenderer` (rendering node) with transport state (RaySeg struct) is resolved
- The 305KB `RenderTestRunner.cs` Godot Node is no longer the only CI test mechanism
- `ObjectSeededTileScheduler` taking `Camera3D` as a parameter is resolved by interface injection

**Technical debt created:**
- Dual fixture format during migration (`fixtures/*.json` + `.tscn` scenes must stay in sync)
- NuGet packaging infrastructure if Core ships as a package rather than a submodule
- Adapter maintenance overhead when Godot APIs change (currently absorbed inside the monolith)

---

## Current State Map

### Five-Layer Pipeline (Today)

```
┌─────────────────────────────────────────────────────────────────┐
│ GODOT SCENE GRAPH                                               │
│ FieldSource3D × N, StaticBody3D geometry, Camera3D, lights      │
└──────────────────────────────┬──────────────────────────────────┘
                               │ GodotAdapter/SnapshotBuilder.cs
                               │ (deterministic NodePath sort, param packing,
                               │  TLAS build, geometry extraction)
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│ RendererCore (engine-agnostic, data-oriented)                   │
│                                                                 │
│  SceneSnapshot (immutable per-frame data contract)              │
│  ↓                                                              │
│  ObjectSeededTileScheduler  → band schedule (row/tile/checker)  │
│  ↓                                                              │
│  MetricHeuristicIntegrator.Step()  (heuristic midpoint, dt-adaptive) │
│  ↓                                                              │
│  FieldSystem.AccelAt()  (GRIN TLAS-pruned radial profiles)      │
│  ↓                                                              │
│  BVH / AABB geometry intersection                               │
│  ↓                                                              │
│  BoundaryLayerVolume crossing events                            │
│  ↓                                                              │
│  RaySeg polylines (hit records, path data)                      │
└──────────────────────────────┬──────────────────────────────────┘
                               │ RenderBackends/IRenderBackend.cs
                               │ (thin stubs: Legacy, Core)
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│ FILM + OVERLAYS                                                 │
│ GrinFilmCamera, FilmOverlay2D, RayViz, FrameSnapshotBus         │
│ GrinObserveDemoHud, WormholeResearchOverlay                     │
└──────────────────────────────┬──────────────────────────────────┘
                               │ PNG capture + log parsing
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│ OBSERVATORY (post-process, never in feedback loop)              │
│ reports/observatory_catalog.json                                │
│ tools/observatory_*.py, tools/renderhealth_regress.py           │
│ 49 shell scripts, 78 dated study folders                        │
└─────────────────────────────────────────────────────────────────┘
```

### Transport Pipeline (Detailed)

1. **Ray generation** — `GrinFilmCamera` maps per-pixel NDC to world rays via `Camera3D.ProjectRayNormal()`
2. **Snapshot** — `SnapshotBuilder` extracts all enabled `FieldSource3D` nodes: resolves params, packs into `PackedParamBuffer` (8 floats/entity), builds `FieldTLAS`, extracts geometry into `GeometryEntitySOA`
3. **Scheduling** — `ObjectSeededTileScheduler.BuildBandSchedule(Camera3D, SceneSnapshot)` emits ordered tile work; `ObjectProbeOracle` uses Godot physics API for depth-sorted probe acquisition
4. **Integration** — `MetricHeuristicIntegrator.Step()`: heuristic midpoint scheme, adaptive `dt`, curvature-based step sizing, constraint drift tracking
5. **Field sampling** — `FieldSystem.AccelAt(Vector3, in SceneSnapshot)`: TLAS prunes candidate sources, evaluates profile curves (`Power`, `Exponential`, `Polynomial`, `AtomicOrbital`), accumulates acceleration
6. **Collision** — `CurvatureBoundGrid` computes per-ray radius bounds; BVH intersection against `GeometryEntitySOA`; `BoundaryLayerVolume` handles `EntryOnly/ExitOnly/EntryAndExit` crossing events
7. **Output** — `RaySeg[]` polylines with per-segment radius bounds and hit metadata → `RayBeamRenderer` for overlay → `FrameSnapshotBus` for PNG capture

### Key Data Contracts

| Contract | Type | Location | Engine-agnostic? |
|---|---|---|---|
| `SceneSnapshot` | Immutable snapshot | `RendererCore/SceneSnapshot/` | YES (System.Numerics only) |
| `PackedParamBuffer` | float[] field params | `RendererCore/SceneSnapshot/` | YES |
| `MetricRayState` | Per-ray integration state | `RendererCore/Transport/` | YES |
| `StepResult` | Per-step integrator output | `RendererCore/Transport/` | YES |
| `RaySeg` | Ray polyline segment | `RayBeamRenderer.cs` (root!) | **NO — embedded in Godot Node** |
| `EvalResult` | Field acceleration + metadata | `FieldMath.cs` (root) | YES |
| `DomainSignature` | Per-pixel curvature domain | `RendererCore/Common/` | YES |

---

## Core Extraction Candidates

### HIGH CONFIDENCE — Zero refactor, zero Godot deps, extract immediately

These files use only `System`, `System.Numerics`, and `System.Collections.Generic`. No `using Godot;` anywhere.

| File | Lines | What it is |
|---|---|---|
| `RendererCore/Fields/FieldSystem.cs` | 157 | Core GRIN acceleration accumulator; `AccelAt(Vector3, in SceneSnapshot)` |
| `RendererCore/Fields/FieldCurves.cs` | 45 | Profile curve evaluation: Power, Polynomial, Exponential, AtomicOrbital |
| `RendererCore/Fields/FieldModels.cs` | 24 | Enums: `MetricModel`, `FieldShapeType`, `FieldCurveType` |
| `RendererCore/Fields/FieldTLAS.cs` | ~100 | Top-level acceleration structure for field candidate pruning |
| `RendererCore/Transport/MetricHeuristicIntegrator.cs` | 209 | Primary integrator: heuristic midpoint, adaptive dt, error estimation |
| `RendererCore/Transport/MetricTransportTypes.cs` | 93 | `MetricRayState` struct + `MetricFallbackCause` enum |
| `RendererCore/Transport/IMetricField.cs` | 10 | Interface: `AccelAt(Vector3, in SceneSnapshot)` |
| `RendererCore/Transport/IIntegrator.cs` | 12 | Interface: `Step(in MetricRayState, float dt, IMetricField, in SceneSnapshot)` |
| `RendererCore/Transport/StepResult.cs` | 10 | Output struct: NewState, ErrorEstimate, ConstraintDrift, RecommendedDt |
| `RendererCore/Integrators/StepPolicy.cs` | 17 | Step size acceptance policy |
| `RendererCore/Config/ResearchModeConfig.cs` | 112 | `TransportModel`, `IntegratorKind`, `ResearchTier` enums + tolerances |
| `RendererCore/SceneSnapshot/Aabb3.cs` | 78 | Pure AABB struct with intersection logic |
| `RendererCore/Geometry/CurvatureBoundGrid.cs` | 184 | Per-ray curvature-based radius bounds (System.Numerics throughout) |
| `RendererCore/Common/DomainTelemetry.cs` | ~50 | Pure structs: `CurvatureDomainKind`, `DomainSignature`, `PixelDomainState` |
| `RendererCore/Validation/ReferenceTransportOracle.cs` | ~80 | Pure record types: oracle settings, trajectory records, epsilon stability class |
| `RendererCore/Testing/CalibratedPreset.cs` | ~150 | Pure config struct: preset render configuration |
| `RendererCore/Scheduling/SceneTransportMemory.cs` | ~100 | Pure diagnostic records: basin, seam, precision region records |
| `FieldMath.cs` (root) | 239 | `EvalResult` struct, `EvalFieldAccel()`, Hermite edge ramps — pure static math |

**These 18 files form the extractable mathematical heart.** They constitute the instrument: field physics, integration schemes, spatial acceleration, diagnostic types.

### MEDIUM CONFIDENCE — Thin adapter needed (< 1 day per file)

| File | Godot coupling | Extraction path |
|---|---|---|
| `RendererCore/Transport/MetricSegmentCompatibility.cs` | `RayBeamRenderer.RaySeg` struct ref + `ToGodot()/ToNumerics()` conversion methods | Move core logic to Core; bridge methods go to `XPrimeRay.Adapters.Godot` |
| `RendererCore/Scheduling/ObjectSeededTileScheduler.cs` | `Camera3D camera` parameter in `BuildBandSchedule()` | Replace with `ICameraObserver` interface; Godot adapter implements it |
| `RendererCore/Scheduling/DomainEmergenceAnalyzer.cs` | Stray `using Godot;` import only; logic is pure | Remove import; already pure |
| `RendererCore/Common/DebugOverlayBus.cs` | `Godot.Color` in event payloads | Replace with `(float R, float G, float B, float A)` tuple or custom `RgbaColor` struct |
| `GodotAdapter/SnapshotBuilder.cs` | IS the adapter — extracts from Godot scene tree | Already correctly placed; its output types (`SceneSnapshot`, `PackedParamBuffer`) move to Core |
| `BoundaryLayerVolume.cs` (root) | `Node3D` base class, Godot `[Export]` attributes | Math-only crossing logic is pure; extract `IBoundaryVolume` interface for Core, keep Node3D subclass in Godot |

### LOW CONFIDENCE — Stays Godot-bound or requires major decomposition

| File | Why it can't extract | Notes |
|---|---|---|
| `RendererCore/Scheduling/ObjectProbeOracle.cs` | Calls `Camera3D.GetWorld3D().DirectSpaceState.IntersectRay()` — Godot physics API | Core defines `IGeometryQueryProvider`; Godot implements it with physics API; Core uses stub BVH instead |
| `RendererCore/Testing/RenderTestRunner.cs` (305KB) | `public partial class RenderTestRunner : Node` — lifecycle entangled with Godot frame loop | Wrap first (Phase 5); decompose in Phase 6 |
| `RendererCore/Testing/SceneAutoCalibrator.cs` | Scene tree traversal, `NodePath`, `GodotObject` — introspects live Godot scene | Classification logic can extract; traversal stays in Godot |
| `GrinFilmCamera.cs` (root) | Orchestrates entire render pipeline from Godot side; `partial class : Node` | Stays Godot; becomes thin adapter delegating to Core `TransportRunner` |
| `RayBeamRenderer.cs` (root) | `partial class : Node3D`; owns `RaySeg` struct definition | Stays Godot; `RaySeg` struct migrates to Core as `RaySegment` |
| `FilmOverlay2D.cs` | `TextureRect`, Godot overlay rendering, `ImageTexture` | Stays Godot; visualization only |
| `FrameSnapshotBus.cs` | Godot signal system, texture/image capture | Stays Godot; output pipeline |

---

## Adapter Boundary Candidates

Five seams define where Core ends and Godot begins. Each seam has a proposed interface.

### Seam 1: Vector Type Boundary

**Current state:** RendererCore uses `System.Numerics.Vector3` (float). Root-level files (`FieldSource3D.cs`, `RayBeamRenderer.cs`) still use `Godot.Vector3` in some export paths.

**Proposed:** All transport math uses `System.Numerics.Vector3`. Godot adapter converts at the boundary via `SnapshotBuilder` (already done) and `GodotVectorMapper.cs` (proposed in split doc, not yet created).

**Decision gate:** If upgrading to double-precision `Vec3`, this is where the precision boundary lives. All physics integrators use double; all Godot-facing paths convert to float for rendering.

### Seam 2: Camera/Observer Boundary

**Current state:** `ObjectSeededTileScheduler.BuildBandSchedule(SceneSnapshot, Camera3D, Options)` takes a live Godot `Camera3D` node.

**Proposed interface:**
```csharp
// In Core: XPrimeRay.Core/Geometry/ICameraObserver.cs
public interface ICameraObserver
{
    Vector3 WorldPosition { get; }
    Vector3 Forward { get; }
    Vector3 Up { get; }
    float FovDegrees { get; }
    (int W, int H) Resolution { get; }
}

// In Godot adapter:
public sealed class GodotCameraObserver : ICameraObserver
{
    private readonly Camera3D _cam;
    // ... wraps Camera3D GlobalPosition, Basis.Z, etc.
}
```

### Seam 3: Scene Snapshot Boundary

**Current state:** `SceneSnapshot` is defined inside `RendererCore/SceneSnapshot/` and referenced everywhere. `IMetricField.AccelAt(Vector3, in SceneSnapshot)` means Core currently depends on an internal type.

**Proposed:** `SceneSnapshot` moves to `XPrimeRay.Core/Scene/`. It is the **only** shared type passed from the Godot adapter into the transport pipeline. `SnapshotBuilder` constructs it; everything downstream only reads it.

### Seam 4: Ray Output Boundary

**Current state:** `RayBeamRenderer.RaySeg` (defined inside a Godot `Node3D` subclass) is referenced in `MetricSegmentCompatibility.cs`. This couples the output format to a Godot node.

**Proposed:** Move struct to Core as `XPrimeRay.Core/Transport/RaySegment.cs`. Add `ToGodotRaySeg()` extension method in Godot adapter. `RayBeamRenderer` consumes `RaySegment[]` via adapter.

### Seam 5: Physics Narrowphase Boundary

**Current state:** `ObjectProbeOracle` issues Godot physics ray casts for depth-sorted probe acquisition. This is the **only** place in RendererCore that requires a live Godot scene.

**Proposed interface** (already specified in `Docs/spec_ray_transport_interfaces_1.md`):
```csharp
// In Core:
public interface IGeometryQueryProvider
{
    bool CastRay(Vector3 origin, Vector3 direction, float maxDistance,
                 out GeometryHit hit, uint layerMask = uint.MaxValue);
}

// In Core (Phase 2 stub):
public sealed class BvhGeometryProvider : IGeometryQueryProvider { /* uses GeometryEntitySOA */ }

// In Godot adapter:
public sealed class GodotPhysicsProvider : IGeometryQueryProvider { /* uses DirectSpaceState */ }
```

---

## Fixture System Review

### Scoring Rubric

| Score | Meaning |
|---|---|
| READY | Can be expressed as a JSON fixture definition today; deterministic; no Godot-specific behavior |
| NEEDS REFACTOR | Core transport logic is clean but controller mixes test and presentation concerns |
| EXPERIMENTAL | Non-deterministic (time-dependent, physics-frame-count-sensitive, or multi-session) |

### READY (15 fixtures)

These fixtures exercise hermetic closure, reference geometry, and field transport in deterministic, purely-parametric scenes. They map cleanly to the proposed JSON fixture schema.

| Fixture | Key test | JSON feasibility |
|---|---|---|
| `fixture_hermetic_curved_room` (3 variants) | Sealed 6-surface closure; `requireHermeticClosure: true` | Direct: bounds, field amp, step params |
| `fixture_blackhole_minimal` | Schwarzschild-inspired radial field; photon capture | Direct: field center, ROuter, amp |
| `fixture_blackhole_minimal_grin` | Black hole via GRIN profile | Direct |
| `fixture_blackhole_minimal_metric` | Metric null geodesic variant | Direct: `integratorKind: "Heuristic"` |
| `fixture_einstein_ring_minimal` (2 variants) | Photon sphere lensing, ring histogram | Direct: ring radius, bin count |
| `fixture_einstein_ring_minimal_metric` | Metric variant | Direct |
| `fixture_curved_minimal` | Minimal curved space straight vs. curved comparison | Direct: beta/gamma params |
| `fixture_curved_minimal_backdrop` | Textured backdrop variant | Needs texture ref in JSON |
| Boundary stress tests (3 fixtures) | Shell crossing policies; `EntryOnly`, `ExitOnly`, `Both` | Direct: BoundaryLayerVolume params |
| Corner probe reference | Reference ray cache validation | Direct |

**JSON fixture template (hermetic_curved_room):**
```json
{
  "name": "hermetic_curved_room",
  "description": "Sealed 6-receiver closure test. GRIN curvature with Power profile.",
  "observer": { "origin": [0,0,0], "forward": [0,0,-1], "fovDegrees": 82.0 },
  "rayGrid": { "width": 320, "height": 180 },
  "field": { "type": "grin_radial", "radiusOuter": 4.75, "amplitude": 0.006, "curveType": "Power", "gamma": 1.0 },
  "transport": { "stepSize": 0.015, "maxStepsPerRay": 700, "integrator": "Heuristic" },
  "validation": { "requireHermeticClosure": true, "maxMisses": 0, "closureReceivers": 6 }
}
```

### NEEDS REFACTOR (10 fixtures)

These fixtures work correctly but their controllers mix visual presentation with test contract. The transport logic is sound; the fixture definition needs separation from the overlay/HUD code.

| Fixture | Coupling issue | Refactor path |
|---|---|---|
| `fixture_grin_basic_visual` | `GrinBasicVisualController` (1,875 lines) mixes param tuning, HUD, capture scheduling, and field config | Extract `GrinBasicFixtureDefinition` (pure params) from `GrinBasicVisualController` (UI) |
| `fixture_atomic_orbital_grin_room` | `AtomicOrbitalGrinRoomController` writes CSV telemetry + configures field + drives test lifecycle | Extract telemetry to observer; keep field config as fixture param |
| `fixture_atomic_orbital_visual_observatory` | `AtomicOrbitalVisualObservatoryController` (359 lines) mixes observatory navigation with orbital config | Separate observatory shell from fixture definition |
| Wormhole witness mouth + exit (2 fixtures) | `WormholeCheckpointSequencer` (841 lines) drives checkpoint sequences with Godot timers | Extract checkpoint sequence as deterministic state machine (no wall clock) |
| Off-axis GRIN observation variants (3 fixtures) | Depend on `GrinBasicVisualController` | Same refactor as parent |
| GRIN hermetic observatory tile variants | Tile scheduling params mixed into controller | Expose as JSON `transport.traversal` field |

### EXPERIMENTAL (13 fixtures)

These fixtures are non-deterministic, narratively-driven, or in active research. They are not candidates for JSON fixture format in the near term.

| Fixture group | Why experimental |
|---|---|
| Wormhole checkpoint multi-sequence | Frame-count-dependent; sequence is stateful across multiple portal crossings |
| Recursive mirror ghost portal | Non-terminating in some configurations; bounce count budget varies |
| Observer ladder / multi-observer disagreement | Requires simultaneous multiple camera views; no single ground truth |
| Overspace trophy room demo | Interactive navigation; not a fixed test |
| Living room physics | Asset-dependent scene; not parametric |
| Cathedral probe world | Multi-session probe array; requires external probe schedule |
| Wormhole transport demo (new, 2026-06-20) | Orb ring is runtime-instantiated + animated; non-deterministic spin state |

**Note on the wormhole transport demo:** The `fixture_wormhole_transport_demo.tscn` added in the previous session is a visual showcase fixture. It will be the Phase 8 public demo (WASM web export). It is NOT a closure validation fixture — its purpose is perceptual demonstration, not mathematical truth-verification.

---

## Validation System Review

### What Moves to Core

| Component | Current location | Move to |
|---|---|---|
| `ReferenceTransportOracle` record types | `RendererCore/Validation/` | `XPrimeRay.Core/Validation/` — already pure |
| `ClosureValidator` logic | Embedded in `RenderTestRunner.cs` (305KB) | `XPrimeRay.Core/Validation/ClosureValidator.cs` |
| `EpsilonStabilityClass` enum | `ReferenceTransportOracle.cs` | Moves with oracle types |
| `CalibratedPreset` struct | `RendererCore/Testing/` | `XPrimeRay.Core/Calibration/` |
| `SceneProbeReport` struct | `RendererCore/Testing/` | `XPrimeRay.Core/Calibration/` |
| Fixture JSON schema + `FixtureLoader` | Not yet implemented | `XPrimeRay.Core/Fixtures/` (new) |
| `SweepRunner` | Currently manual Python + bat scripts | `XPrimeRay.Core/Validation/SweepRunner.cs` (new) |

### What Stays in Godot

| Component | Why |
|---|---|
| `SceneAutoCalibrator` | Scene tree traversal, NodePath, GodotObject introspection |
| `RenderTestRunner` (the Node lifecycle) | `partial class : Node`; drives frame loop, capture scheduling |
| `FrameSnapshotBus` | Godot signal system, ImageTexture capture |
| `ObjectProbeOracle` | Godot physics API |
| Python post-process tools | Engine-agnostic already; continue to consume observatory manifest |

### Observatory Catalog

The `reports/observatory_catalog.json` format is already engine-agnostic. It must remain the **shared output format** for both the current Godot test runner and the future CLI testbench. No schema changes; new entries use the same structure.

The CLI testbench's `validate` command should append to the same catalog, or emit a separate catalog JSON that the observatory indexer merges.

**Invariant:** Python tooling (`tools/observatory_*.py`, `tools/renderhealth_regress.py`) must never require code changes to consume CLI-generated output. Observatory compatibility is a day-one CLI acceptance criterion.

---

## Migration Risk Assessment

| # | Risk | Probability | Impact | Mitigation |
|---|---|---|---|---|
| 1 | **SceneSnapshot ownership conflict** — Both Core transport and Godot adapter depend on `SceneSnapshot`; moving it requires updating all references simultaneously | HIGH | HIGH | Move early (Phase 1); create a compatibility `using` alias in RendererCore during transition |
| 2 | **RaySeg struct migration** — `MetricSegmentCompatibility.cs` references `RayBeamRenderer.RaySeg`; moving the struct is a breaking change to RayBeamRenderer | MEDIUM | MEDIUM | Define `RaySegment` in Core first; add `ToRaySeg()` adapter; migrate RayBeamRenderer to consume `RaySegment[]` |
| 3 | **RenderTestRunner decomposition risk** — 305KB Godot Node contains closure logic, CLI parsing, frame lifecycle, and capture scheduling; any extraction risks regression | HIGH | HIGH | Wrap first (thin Core adapter in Phase 5); decompose in Phase 6 with full fixture baseline as regression guard |
| 4 | **Dual fixture source drift (.tscn + JSON)** — `fixtures/*.json` and `.tscn` scene defaults can silently diverge, creating two sources of truth for field parameters | HIGH | MEDIUM | CI `fixture-sync` step: auto-reads `[Export]` defaults from fixture controllers and validates against JSON; fail on mismatch |
| 5 | **ObjectProbeOracle gap in CLI** — CLI testbench has no Godot physics engine; depth-sorted probe acquisition cannot run; tile scheduling degrades silently | MEDIUM | MEDIUM | Phase 2: implement `BvhGeometryProvider` stub using `GeometryEntitySOA`; log explicit warning if Godot provider is unavailable |
| 6 | **float vs. double precision mismatch** — Proposal specifies double-precision `Vec3`; RendererCore uses float (`System.Numerics.Vector3`); mixing precisions risks subtle geodesic drift in long paths | MEDIUM | HIGH | Decision gate (see Decisions Needed #1); do not mix; pick one and enforce in `Directory.Build.props` |
| 7 | **Godot version lock in SnapshotBuilder** — `SnapshotBuilder.cs` uses Godot 4 C# API; a Godot 5 migration would require rewrite of the adapter | LOW | MEDIUM | By design: Core is version-agnostic; adapter absorbs engine churn. Document this explicitly in adapter README |
| 8 | **Assembly reference graph complexity** — Currently all C# in one Godot project; splitting into `XPrimeRay.Core.csproj` + `XPrimeRay.Adapters.Godot.csproj` requires NuGet packaging or submodule strategy | MEDIUM | MEDIUM | Mono-repo first (Phases 1-6): `src/XPrimeRay.Core/` folder inside GD_xPRIMEray project, referenced as project reference. Split at Phase 7. |
| 9 | **Fixture determinism** — Several controllers modulate field amplitude by wall-clock time or physics frame count; CLI testbench runs are not reproducible | MEDIUM | HIGH | Fixture JSON must specify fixed seeds; controllers must accept `--seed` override; time-dependent features gate on `TimeEnabled = false` in fixture config |
| 10 | **Python tool coupling to Godot output paths** — 49 shell scripts expect Godot-specific log formats and output directory structures; CLI testbench output must match exactly | HIGH | MEDIUM | Define manifest format first; CLI testbench implements it from day one (Decision #4); validate with observatory indexer against a test run |
| 11 | **Dual fixture format maintenance cost** — During Phases 3-6, both JSON fixtures and .tscn scenes are authoritative for different audiences; maintaining both is engineering overhead | MEDIUM | LOW | Treat `.tscn` as the "editable" source and JSON as the "validated export"; code-gen step extracts JSON from controller Export defaults via reflection |
