# Project Optical Closure Stage 1B Completion Report

Status: **PASS**

Stage 1B adds a Godot-free, post-hit observer-instrument framework to the Stage 1A library. It includes immutable input/output records, construction-time metadata, deterministic instrument ordering, feature masks that default to disabled, a reusable frame buffer, and three synthetic instruments. No renderer, overlay, adapter, scene, schema, fixture, or Glowing Heart evidence-chain integration was added.

## Changed Files

New library files:

- `src/XPrimeRay.ObserverInstrumentation/Abstractions/IObserverInstrument.cs`
- `src/XPrimeRay.ObserverInstrumentation/Abstractions/InstrumentContext.cs`
- `src/XPrimeRay.ObserverInstrumentation/Abstractions/InstrumentEnums.cs`
- `src/XPrimeRay.ObserverInstrumentation/Abstractions/InstrumentObservation.cs`
- `src/XPrimeRay.ObserverInstrumentation/Metadata/InstrumentMetadataCatalog.cs`
- `src/XPrimeRay.ObserverInstrumentation/Metadata/InstrumentTargetMetadata.cs`
- `src/XPrimeRay.ObserverInstrumentation/Runtime/InstrumentFrameBuffer.cs`
- `src/XPrimeRay.ObserverInstrumentation/Runtime/InstrumentRegistry.cs`
- `src/XPrimeRay.ObserverInstrumentation/Instruments/InstrumentObservationFactory.cs`
- `src/XPrimeRay.ObserverInstrumentation/Instruments/SurfaceUvInstrument.cs`
- `src/XPrimeRay.ObserverInstrumentation/Instruments/CheckerProbeInstrument.cs`
- `src/XPrimeRay.ObserverInstrumentation/Instruments/FlagCaptureInstrument.cs`

Modified library file:

- `src/XPrimeRay.ObserverInstrumentation/Math/UvRevealRegion.cs` exposes its immutable validity state for metadata construction checks.

New test files:

- `src/XPrimeRay.ObserverInstrumentation.Tests/Stage1BTestData.cs`
- `src/XPrimeRay.ObserverInstrumentation.Tests/MetadataCatalogTests.cs`
- `src/XPrimeRay.ObserverInstrumentation.Tests/InstrumentTests.cs`
- `src/XPrimeRay.ObserverInstrumentation.Tests/RegistryAndBufferTests.cs`
- `src/XPrimeRay.ObserverInstrumentation.Tests/AllocationTests.cs`

Modified test files:

- `src/XPrimeRay.ObserverInstrumentation.Tests/Program.cs` registers the Stage 1B suites.
- `src/XPrimeRay.ObserverInstrumentation.Tests/TestAssert.cs` supports enum equality in the console runner.

No solution or project-file changes were required. Stage 1A had already added and isolated both projects.

## New Public Types

Immutable DTOs and enums:

- `InstrumentContext`
- `InstrumentObservation`
- `InstrumentTargetMetadata`
- `InstrumentDiagnosticState`
- `InstrumentKind`
- `InstrumentHitKind`
- `ObserverInstrumentMask`

Framework:

- `IObserverInstrument`
- `InstrumentMetadataCatalog`
- `InstrumentRegistry`
- `InstrumentFrameBuffer`

Instruments:

- `SurfaceUvInstrument`
- `CheckerProbeInstrument`
- `FlagCaptureInstrument`

`InstrumentMetadataCatalog` copies validated entries into an ordinal, unique-`ColliderName` lookup at construction. Empty, invalid, and duplicate entries are rejected. It has no loader and exposes no mutation API.

## Test Results

Release build result: **0 warnings, 0 errors**.

| Suite | Coverage | Result |
|---|---|---|
| `SphericalUvMathTests` | Canonical vectors, wrapping, poles, invalid vectors | PASS |
| `CheckerProbeMathTests` | Checker parity and invalid coordinates/tile counts | PASS |
| `UvRevealRegionTests` | Ordinary, seam-aware, and invalid regions | PASS |
| `MetadataCatalogTests` | Lookup, duplicate names, empty names, invalid metadata | PASS |
| `InstrumentTests` | Sampled, outside region, other geometry, non-surface, missing/invalid metadata | PASS |
| `RegistryAndBufferTests` | Ordering, default-off flags, reuse, stale-data prevention | PASS |
| `AllocationTests` | Warmed registry/catalog/instrument/frame-buffer evaluation | PASS |

Result: **7 suites, 0 failures**.

## Allocation Result

After 2,000 warm-up evaluations, the console test measured:

```text
0 bytes / 100000 evaluations
```

The measured path includes ordinal metadata lookup, three enabled instruments, observation construction, append operations, and frame-buffer clearing/reuse. Construction and configuration remain setup-time operations and are outside the measured per-hit path.

## Dependency and Protection Checks

`XPrimeRay.ObserverInstrumentation.csproj` still uses `Microsoft.NET.Sdk`, targets `net8.0`, and has no package or project references. A source scan found no `Godot`, `Godot.NET.Sdk`, `GodotSharp`, `HitPayload`, `RayBeamRenderer`, `GrinFilmCamera`, `FilmOverlay2D`, or `SnapshotBuilder` references in the library or test runner.

Protected renderer, overlay, adapter, transport, scheduler, schema, fixture, and Glowing Heart evidence-chain paths are unchanged. In particular:

- `RayBeamRenderer.cs`
- `GrinFilmCamera.cs`
- `FilmOverlay2D.cs`
- `GodotAdapter/SnapshotBuilder.cs`
- `RendererCore/Testing/RenderTestRunner.cs`
- `RendererCore/Transport/`
- `RendererCore/Fields/`
- `schemas/`
- `Fixtures/`
- `reports/observatory_catalog.json`

Stage 1B reads synthetic post-hit contexts only. It adds no intersection authority and changes no transport, geometry, hit classification, or optical truth.

## Stage 1C Gate Checklist

Completed prerequisites:

- [x] Stage 1A math tests pass.
- [x] Stage 1B synthetic path tests pass.
- [x] Feature mask defaults to `None`.
- [x] Registry order is deterministic after sealing.
- [x] Metadata lookup is immutable after construction.
- [x] Invalid or unavailable metadata fails closed.
- [x] Frame-buffer reuse prevents stale observations from being exposed or retained.
- [x] Warmed per-hit evaluation allocates zero bytes.
- [x] Godot-free dependency boundary is intact.

Required before Stage 1C is complete:

- [ ] Define one additive Godot adapter that projects existing completed hit data into `InstrumentContext`.
- [ ] Verify every mapped field comes from the existing sovereign hit payload; do not raycast or recompute classification.
- [ ] Require an explicit nonzero feature mask and existing collider-name availability before evaluation.
- [ ] Keep instrumentation disabled when collider names are unavailable; do not alter collision-query behavior.
- [ ] Size/reuse `InstrumentFrameBuffer` outside per-ray evaluation and mirror existing debug-buffer capacity safely.
- [ ] Evaluate once from the completed debug-hit span so both existing population paths share one attachment point.
- [ ] Add an observation-only overlay method such as `SetInstrumentObservations(...)` without changing `SetData(...)`.
- [ ] Clear observation count/storage when the existing overlay is cleared.
- [ ] Add no drawing, flag presentation, scene, fixture, schema, or metadata-file loading.
- [ ] Re-run zero-allocation checks for the adapter-to-buffer integration path.
- [ ] Verify both debug-hit production modes and the feature-disabled path.
- [ ] Confirm all renderer, transport, classification, snapshot, and evidence artifacts remain behaviorally unchanged.

## Stage 1C Plan

1. Add a single `GodotHitContextAdapter` beside the runtime integration code. It performs field translation only, including Godot-to-`System.Numerics` vector conversion and existing termination-disposition mapping.
2. Add setup-time ownership for the sealed registry, immutable metadata catalog, enabled mask, and reusable frame buffer. Default the mask to `None`.
3. Attach evaluation once beside the existing final debug/film handoff, after `_dbgHits` has been populated by either existing path. Do not edit either hit writer.
4. Add `FilmOverlay2D.SetInstrumentObservations(...)` as an independent storage call. Preserve the existing `SetData(...)` signature and behavior byte-for-byte.
5. Keep Stage 1C storage-only: no drawing, reveal behavior, scene edits, fixture work, or public presentation.
6. Add focused integration checks for adapter mapping, disabled/missing-name behavior, capacity reuse, clearing, both hit-production paths, and zero allocation after warm-up.
7. Re-run protected-file, dependency, build, and regression checks before considering any later presentation work.

Stage 1C is **eligible for implementation planning but not implemented by this report**. Its safe scope is additive translation, evaluation, and observation storage only.
