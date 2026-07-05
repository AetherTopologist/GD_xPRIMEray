# Observer Instrument OI-001 Equator Completion Report

## Result

**PASS**

OI-001 completed the first headless activation of `SurfaceUvInstrument`. The test reads existing hit records, derives spherical UV coordinates for the `uv_probe` sphere, evaluates numeric acceptance criteria, prints one deterministic result, and exits.

## Changed Files

- `src/XPrimeRay.ObserverInstrumentation/Runtime/ObserverInstrumentationSession.cs`
- `ObserverInstrumentationAdapter.cs`
- `GrinFilmCamera.cs`
- `src/XPrimeRay.ObserverInstrumentation.Tests/Stage1CLifecycleTests.cs`
- `Tests/ObserverInstrumentation/Oi001EquatorHeadless.cs`
- `Tests/ObserverInstrumentation/Oi001ProbeBody.cs`
- `test-oi-001-equator.tscn`

The production-facing additions are limited to frame sequencing and internal, read-only test access to the current observation buffer and overflow state.

## Headless Fixture

- Collider identity: `uv_probe`
- Analytic surface: sphere centered at `[0, 0, 0]`
- Enabled instrument mask: `SurfaceUv` only
- Observer position: `(0, 0, 4)`, aimed along `-Z`
- Observer FOV: 8 degrees
- Grid: `40 x 22`
- Transport: straight, integrated field disabled
- Output: numeric diagnostics only

## Headless Output

```text
[OI-001][FirstObservation] state=RegionSampled collider=uv_probe hasUv=1
[OI-001] PASS reason=acceptance met surface=880 valid=880 vMin=0.433414 vMax=0.566586 vMean=0.499994 vBand=880/880 (100.00%) unresolvedProbe=0 unresolvedAny=0 otherGeometry=0 nonSurface=0 otherInstrument=0 invalidUv=0 overflow=0 dropped=0
```

## Observation Results

| Measure | Result |
|---|---:|
| Surface UV observations | 880 |
| Valid UV observations | 880 |
| `RegionSampled` observations | 880 |
| V minimum | 0.433414 |
| V maximum | 0.566586 |
| V mean | 0.499994 |
| Within `abs(v - 0.5) <= 0.1` | 880 / 880 (100.00%) |
| Unresolved probe observations | 0 |
| Invalid UV observations | 0 |
| Checker or flag observations | 0 |
| Buffer overflow | false |
| Dropped observations | 0 |

All accepted UV values were finite, with `u` in `[0, 1)` and `v` in `[0, 1]`.

## Regression Results

- Observer instrumentation console suite: 10 suites passed, 0 failed.
- Enabled-session allocation check: 0 bytes allocated across 100,000 evaluations.
- Host build: 0 errors and no new warnings attributable to OI-001.
- Headless OI-001 run: no warning or error output.

## Boundary Verification

- No transport behavior changed.
- No hit classification changed.
- No intersection or raycast authority was added.
- `_dbgHits` ownership and writers remain unchanged.
- Existing `SetData(...)` signatures and calls remain unchanged.
- No checker, flag, PNG, screenshot, overlay drawing, or visualization polish was added.
- `RayBeamRenderer.cs`, `FilmOverlay2D.cs`, `RendererCore/`, `GodotAdapter/`, schemas, existing fixtures, and `reports/observatory_catalog.json` were untouched.

OI-001 stops at numeric spherical-UV correctness for one controlled equatorial probe. It makes no optical-closure, renderer-equivalence, or physical-correctness claim.
