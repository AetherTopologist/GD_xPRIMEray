# Project Glowing Heart — Execution Plan

**Document status:** Planning pass, 2026-06-20  
**Authored by:** AetherTopologist / Billy Broch + Claude Sonnet 4.6  
**Companion document:** `Docs/xPRIMEray/project_glowing_heart_review.md`

---

## Mission Statement

> The Core becomes the instrument. Godot becomes the first telescope. MisterY Labs becomes the observatory.

Project Glowing Heart extracts the mathematical engine of xPRIMEray — field systems, transport integrators, GRIN physics, fixture validation — into a standalone pure C# library called `xPRIMEray-Core`. Godot, Unreal, web, and all future render environments become interchangeable observer shells that connect through typed adapters.

The result is a codebase where mathematical truth is verifiable independently of any rendering engine, on any platform, in seconds, without launching Godot.

---

## Architectural Target

```
MisterY Labs (public observatory, gallery, mythos)
  ↓
Observer Shells (GD_xPRIMEray, Unreal adapter, web WASM)
  ↓
XPrimeRay.Adapters.Godot / .Unreal / .Web
  ↓
xPRIMEray-Core (XPrimeRay.Core.dll)
Fields · Integrators · Transport · Fixtures · Validation
```

---

## Phase 0: Freeze & Baseline

**Goal:** Lock the current state before any structural changes. Establish a validated regression baseline.

### Tasks

1. Create git tag: `git tag gd-xprimeray-pre-core-split`
2. Run all 15 READY fixtures (see review doc for list); capture PNG outputs and observatory catalog entries
3. Record baseline manifest: `reports/baseline_pre_split_manifest.json` with run IDs, fixture names, verdicts
4. Verify `dotnet build` passes clean in current state
5. Record current `reports/observatory_catalog.json` as frozen baseline snapshot

### Files affected

- `reports/baseline_pre_split_manifest.json` (new, generated)
- `reports/observatory_catalog.json` (snapshot only, no changes)

### Acceptance criteria

- [ ] Git tag `gd-xprimeray-pre-core-split` exists
- [ ] Baseline manifest JSON exists with ≥15 READY fixture entries
- [ ] All 15 READY fixture verdicts are PASS or documented exceptions
- [ ] Current `dotnet build` is clean (zero warnings on key assemblies)

### Rollback strategy

Phase 0 is purely additive (tag + capture). Nothing to roll back.

---

## Phase 1: Mathematical Core Extraction

**Goal:** Extract the 18 pure-C# files from RendererCore into a standalone `XPrimeRay.Core` project inside the mono-repo. Establish the `Directory.Build.props` enforcement rule.

### Target structure (inside GD_xPRIMEray, not a new repo yet)

```
src/
  XPrimeRay.Core/
    Fields/
      FieldSystem.cs
      FieldCurves.cs
      FieldModels.cs
      FieldTLAS.cs
    Geometry/
      Aabb3.cs
      CurvatureBoundGrid.cs
      Vec3.cs            ← decision: System.Numerics alias or custom double-precision
      RayState.cs        ← corresponds to MetricRayState
      Bounds.cs
    Integrators/
      ITransportIntegrator.cs    ← IIntegrator.cs renamed
      MetricHeuristicIntegrator.cs
      StepResult.cs
      StepPolicy.cs
    Fields/
      IMetricField.cs
      MetricTransportTypes.cs
    Scene/
      SceneSnapshot.cs           ← moved from RendererCore/SceneSnapshot/
      PackedParamBuffer.cs
      GeometryEntitySOA.cs
      FieldEntitySOA.cs
    Config/
      ResearchModeConfig.cs
    Common/
      DomainTelemetry.cs
    Validation/
      ReferenceTransportOracle.cs  ← data types only at this stage
    XPrimeRay.Core.csproj
  Directory.Build.props
```

### `Directory.Build.props` (enforces zero Godot in Core)

```xml
<Project>
  <PropertyGroup Condition="$(MSBuildProjectName.StartsWith('XPrimeRay.Core'))">
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>

  <!-- Fail build if any Godot reference appears in Core project -->
  <Target Name="EnforceNoCoreGodotReference"
          BeforeTargets="Build"
          Condition="$(MSBuildProjectName) == 'XPrimeRay.Core'">
    <Error Condition="'@(Reference)' != '' And
                      $([System.String]::Copy('%(Reference.Identity)').Contains('GodotSharp'))"
           Text="XPrimeRay.Core must not reference Godot. Found: %(Reference.Identity)" />
  </Target>
</Project>
```

### Files affected

- **New:** `src/XPrimeRay.Core/XPrimeRay.Core.csproj`
- **New:** `src/Directory.Build.props`
- **Moved (copy-then-delete after validation):** 18 files listed above
- **Updated:** `RendererCore/Fields/FieldSystem.cs` → `using` alias or removed (dependent on Decisions #1)
- **Updated:** References in `GrinFilmCamera.cs`, `RayBeamRenderer.cs` updated to point to Core types

### Acceptance criteria

- [ ] `dotnet build src/XPrimeRay.Core/XPrimeRay.Core.csproj` passes with zero errors, zero Godot references
- [ ] Existing Godot project still builds (RendererCore references Core via project reference)
- [ ] All 15 READY fixtures still produce identical outputs vs. Phase 0 baseline
- [ ] `Directory.Build.props` enforcement rule blocks a test `using Godot;` import in Core

### Rollback strategy

Files are copied into Core, not deleted from RendererCore until Godot build is verified clean. If Godot build breaks, remove Core project reference and restore RendererCore originals. Git stash recovers any partial state.

---

## Phase 2: Transport Extraction

**Goal:** Make `TransportRunner` (the headless transport loop) functional without Godot. Define `IGeometryQueryProvider`. Achieve the first non-Godot fixture run.

### Tasks

1. Create `XPrimeRay.Core/Transport/TransportRunner.cs` — headless ray stepping loop consuming `IMetricField` and `IGeometryQueryProvider`
2. Create `XPrimeRay.Core/Transport/TransportScene.cs` — populated from `FixtureDefinition` (no Godot scene tree)
3. Create `XPrimeRay.Core/Transport/TransportSettings.cs` — pure config struct
4. Create `XPrimeRay.Core/Transport/HitRecord.cs` — per-ray hit/miss result
5. Create `XPrimeRay.Core/Geometry/IGeometryQueryProvider.cs` — physics narrowphase interface
6. Create `XPrimeRay.Core/Geometry/BvhGeometryProvider.cs` — stub BVH implementation using `GeometryEntitySOA`
7. Fix `MetricSegmentCompatibility.cs`: define `RaySegment` in Core; move `ToGodot()/ToNumerics()` to adapter
8. Fix `ObjectSeededTileScheduler.cs`: replace `Camera3D` parameter with `ICameraObserver`

### Files affected

- **New:** 6 Core transport files (listed above)
- **Modified:** `RendererCore/Transport/MetricSegmentCompatibility.cs` — removes `RayBeamRenderer.RaySeg` dep
- **Modified:** `RendererCore/Scheduling/ObjectSeededTileScheduler.cs` — removes `Camera3D` param
- **New:** `GodotAdapter/GodotCameraObserver.cs`
- **New:** `GodotAdapter/GodotSegmentAdapter.cs`

### Acceptance criteria

- [ ] `TransportRunner` runs `hermetic_curved_room` fixture JSON headlessly (no Godot) and produces non-zero ray count
- [ ] Hit/miss counts within ±5% of Godot baseline (BVH stub will differ from Godot physics; tolerance documented)
- [ ] Godot build still passes; existing fixture outputs unchanged
- [ ] `MetricSegmentCompatibility.cs` no longer has `using Godot;`

### Rollback strategy

`ICameraObserver` injection is additive; old `Camera3D` path can be kept as an overload during transition. `RaySegment` struct is new; `RaySeg` is not removed until RayBeamRenderer is updated and verified.

---

## Phase 3: Fixture System

**Goal:** Define the canonical JSON fixture format. Convert the 5 READY hermetic/closure fixtures to JSON. Implement `FixtureLoader`, `FixtureRegistry`, `FixtureDefinition`.

### JSON fixture schema (canonical fields)

```json
{
  "$schema": "https://xprimeray.dev/schemas/fixture/v1.json",
  "name": "string",
  "description": "string",
  "observer": {
    "origin": [x, y, z],
    "forward": [x, y, z],
    "up": [x, y, z],
    "fovDegrees": 70.0
  },
  "rayGrid": { "width": 320, "height": 180 },
  "scene": {
    "bounds": { "min": [x,y,z], "max": [x,y,z] },
    "receivers": []
  },
  "fields": [
    {
      "type": "grin_radial",
      "center": [x,y,z],
      "radiusInner": 0.0,
      "radiusOuter": 3.0,
      "amplitude": 0.6,
      "curveType": "Power",
      "gamma": 1.0
    }
  ],
  "transport": {
    "stepSize": 0.015,
    "maxStepsPerRay": 700,
    "integrator": "Heuristic",
    "bendScale": 1.0,
    "fieldStrength": 1.0,
    "seed": 0
  },
  "validation": {
    "requireHermeticClosure": true,
    "maxMisses": 0,
    "closureReceivers": 6,
    "tolerances": { "posAbs": 0.01, "rel": 0.001 }
  }
}
```

### Initial fixture conversions

| JSON file | Source fixture | Notes |
|---|---|---|
| `fixtures/hermetic_curved_room.json` | `fixture_hermetic_curved_room.tscn` | Amp=0.006, Power, ROuter=4.75 |
| `fixtures/hermetic_curved_room_strong.json` | Strong-field variant | Amp=0.02 |
| `fixtures/blackhole_minimal.json` | `fixture_blackhole_minimal.tscn` | Schwarzschild proxy |
| `fixtures/curved_minimal.json` | `fixture_curved_minimal.tscn` | Beta/gamma params |
| `fixtures/boundary_shell_crossing.json` | Boundary stress fixtures | EntryAndExit policy |

### Fixture-sync CI step

To prevent `.tscn` / JSON drift, add a build target that reads each fixture controller's `[Export]` defaults via reflection and compares them to the JSON:

```bash
dotnet run --project tools/fixture_sync -- \
  --tscn Fixtures/fixture_hermetic_curved_room.tscn \
  --json fixtures/hermetic_curved_room.json \
  --fail-on-mismatch
```

This runs in CI as a pre-merge gate. It does not modify either file; it only reports divergence.

### Files affected

- **New:** `fixtures/` directory with 5 JSON files
- **New:** `src/XPrimeRay.Core/Fixtures/FixtureDefinition.cs`
- **New:** `src/XPrimeRay.Core/Fixtures/FixtureLoader.cs`
- **New:** `src/XPrimeRay.Core/Fixtures/FixtureRegistry.cs`
- **New:** `tools/fixture_sync/` (CI validation tool)

### Acceptance criteria

- [ ] 5 JSON fixtures load without error via `FixtureLoader.Load(path)`
- [ ] `TransportRunner.Run()` produces closure PASS for `hermetic_curved_room.json`
- [ ] CI fixture-sync step runs and passes for all 5 converted fixtures
- [ ] JSON schema file exists at `fixtures/schema/fixture-v1.json`

### Rollback strategy

JSON fixtures are additive. `.tscn` scenes are unchanged. If FixtureLoader has bugs, delete the `fixtures/` directory and Core fixture files.

---

## Phase 4: CLI Testbench

**Goal:** Build `XPrimeRay.Testbench.Cli` as the first official non-Godot consumer of Core. Emit observatory-compatible manifest output.

### CLI commands

```bash
# Run a single fixture; exit 0 on PASS, 2 on FAIL
xpr run-fixture fixtures/hermetic_curved_room.json

# Sweep a parameter range; emit manifest per value
xpr sweep fixtures/hermetic_curved_room.json --param field.amplitude=0,0.25,0.5,0.75,1.0

# Batch render (emit raw buffer + PNG via System.Drawing or SixLabors.ImageSharp)
xpr render fixtures/hermetic_curved_room.json --width 320 --height 180 --output output/

# Validate a manifest against observatory schema
xpr validate output/latest/manifest.json
```

### Observatory-compatible manifest format

```json
{
  "run_id": "20260620T120000Z",
  "fixture": "hermetic_curved_room",
  "artifact_type": "closure_validation",
  "category": "Canonical",
  "source_path": "output/20260620T120000Z/closure_report.json",
  "timestamp": "2026-06-20T12:00:00Z",
  "verdict": "PASS",
  "closure": "PASS",
  "coverage": "PASS",
  "notes": "CLI run via xpr run-fixture"
}
```

This entry format is **identical** to existing `reports/observatory_catalog.json` entries. Python tooling requires no changes.

### Files affected

- **New:** `src/XPrimeRay.Testbench.Cli/` (full project as outlined in split doc)
- **New:** `src/XPrimeRay.Testbench.Cli/Output/ManifestWriter.cs`
- **New:** `src/XPrimeRay.Testbench.Cli/Output/CsvReportWriter.cs`

### Acceptance criteria

- [ ] `dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture fixtures/hermetic_curved_room.json` exits 0
- [ ] Manifest JSON appended to observatory catalog passes Python observatory indexer without modification
- [ ] `sweep` command runs 5 amplitude values and emits 5 manifest entries
- [ ] CLI installs as a dotnet tool: `dotnet tool install --global xprimeray-testbench` (local feed OK for this milestone)

### Rollback strategy

CLI is a new standalone project. No existing Godot code is modified. Remove `src/XPrimeRay.Testbench.Cli/` to roll back.

---

## Phase 5: Godot Adapter Formalization

**Goal:** Make `GodotAdapter/SnapshotBuilder.cs` the formal, documented boundary between Godot and Core. Replace remaining Godot type leaks with adapter interfaces. Ensure Godot produces identical results to CLI testbench for shared fixtures.

### Tasks

1. Rename/reorganize `GodotAdapter/` → `src/XPrimeRay.Adapters.Godot/` with explicit `.csproj`
2. Implement `GodotCameraObserver : ICameraObserver`
3. Implement `GodotPhysicsProvider : IGeometryQueryProvider`
4. Replace `ObjectSeededTileScheduler(Camera3D)` with `ObjectSeededTileScheduler(ICameraObserver)` throughout
5. Route `RenderBackends/CoreBackend.cs` through `TransportRunner` (currently a stub)
6. Add adapter contract README: `src/XPrimeRay.Adapters.Godot/README.md`

### Adapter contract README (excerpt)

```md
# XPrimeRay.Adapters.Godot

This project is the ONLY permitted Godot reference in the xPRIMEray repository.

Rules:
- Core project MUST NOT reference this adapter.
- Adapters translate Godot types into Core types.
- All Core types are consumed via interfaces; no concrete Core classes are exposed to Godot.
- Godot version changes are absorbed here; Core is not affected.
```

### Files affected

- **New:** `src/XPrimeRay.Adapters.Godot/XPrimeRay.Adapters.Godot.csproj`
- **New:** `src/XPrimeRay.Adapters.Godot/GodotCameraObserver.cs`
- **New:** `src/XPrimeRay.Adapters.Godot/GodotPhysicsProvider.cs`
- **New:** `src/XPrimeRay.Adapters.Godot/GodotVectorMapper.cs`
- **New:** `src/XPrimeRay.Adapters.Godot/README.md`
- **Modified:** `RendererCore/Scheduling/ObjectSeededTileScheduler.cs`
- **Modified:** `RenderBackends/CoreBackend.cs` → routes to `TransportRunner`

### Acceptance criteria

- [ ] Godot runs `hermetic_curved_room` fixture via adapter path; results match CLI testbench output within tolerance
- [ ] `GrinFilmCamera.cs` still produces identical visual output to Phase 0 baseline
- [ ] No `using Godot;` in any file under `src/XPrimeRay.Core/`
- [ ] Adapter README is accurate and complete

### Rollback strategy

Adapter formalization is interface injection; old Camera3D path can coexist as overload during transition. CoreBackend stub was already no-op; restoring stub is a one-line change.

---

## Phase 6: Validation Migration

**Goal:** Extract closure validation logic from `RenderTestRunner.cs` into `XPrimeRay.Core/Validation/ClosureValidator.cs`. CLI testbench `validate` command becomes functionally complete. CI can validate closure without launching Godot.

### Tasks

1. Extract `ClosureValidator` from `RenderTestRunner.cs` into Core
2. Extract `SweepRunner` as a Core type (currently manual Python + bat scripts)
3. Implement `xpr validate output/latest/manifest.json` fully
4. Create `XPrimeRay.Core.Tests/` with unit tests for `ClosureValidator` and `FixtureLoader`
5. Add CI script: `scripts/core_ci.sh` — runs `dotnet build && dotnet test` without Godot

### CI script skeleton

```bash
#!/usr/bin/env bash
set -euo pipefail

echo "[Core CI] Building..."
dotnet build src/XPrimeRay.Core/XPrimeRay.Core.csproj -c Release

echo "[Core CI] Running tests..."
dotnet test src/XPrimeRay.Core.Tests/ -c Release --no-build

echo "[Core CI] Running hermetic_curved_room fixture..."
dotnet run --project src/XPrimeRay.Testbench.Cli -c Release -- \
  run-fixture fixtures/hermetic_curved_room.json

echo "[Core CI] All checks passed."
```

### Files affected

- **New:** `src/XPrimeRay.Core/Validation/ClosureValidator.cs`
- **New:** `src/XPrimeRay.Core/Validation/SweepRunner.cs`
- **New:** `src/XPrimeRay.Core/Validation/ValidationReport.cs`
- **New:** `tests/XPrimeRay.Core.Tests/` (project + 3-5 test files)
- **New:** `scripts/core_ci.sh`
- **Modified:** `RendererCore/Testing/RenderTestRunner.cs` — delegates closure logic to Core `ClosureValidator`

### Acceptance criteria

- [ ] `scripts/core_ci.sh` passes on a machine without Godot installed
- [ ] `xpr validate` exits 0 for Phase 4 sweep output
- [ ] `dotnet test` passes with ≥10 unit tests covering `ClosureValidator`, `FixtureLoader`, `MetricHeuristicIntegrator`
- [ ] RenderTestRunner still passes Phase 0 baseline (regression guard)

### Rollback strategy

Core `ClosureValidator` is additive. `RenderTestRunner` delegates to it via a thin call; if the delegate fails, restore the inline logic from git history. Tests can be deleted without consequence.

---

## Phase 7: Repository Split

**Goal:** Extract `src/XPrimeRay.Core/` into a standalone `xprimeray-core` GitHub repository. `GD_xPRIMEray` consumes it as a NuGet package or git submodule.

### Decision: NuGet package vs. git submodule

| Option | Pros | Cons |
|---|---|---|
| **NuGet package** | Clean versioning, standard .NET tooling, easy for third-party adapters | Requires package feed (GitHub Packages or NuGet.org), publish workflow |
| **Git submodule** | No package feed needed, live source changes | Git complexity, submodule maintenance overhead |

**Recommendation:** NuGet package via GitHub Packages for Phase 7; NuGet.org for Phase 8 public launch.

### Repository structure (xprimeray-core)

```
xprimeray-core/
  src/XPrimeRay.Core/          ← moved from GD_xPRIMEray
  src/XPrimeRay.Testbench.Cli/ ← moved from GD_xPRIMEray
  tests/XPrimeRay.Core.Tests/  ← moved from GD_xPRIMEray
  fixtures/                    ← 5 READY fixtures in JSON
  docs/                        ← architecture.md, adapter-contract.md, etc.
  assets/sigils/bee-sigil.svg  ← identity
  README.md
  xprimeray-core.sln
  Directory.Build.props
  .gitignore
```

### Files affected

- **New repo:** `xprimeray-core` (GitHub, private until Phase 8)
- **Modified:** `GD_xPRIMEray.csproj` — adds `<PackageReference Include="XPrimeRay.Core" />`
- **Removed from GD_xPRIMEray:** `src/XPrimeRay.Core/` and `src/XPrimeRay.Testbench.Cli/`

### Acceptance criteria

- [ ] `xprimeray-core` repo builds independently: `dotnet build && dotnet test` on a machine with only .NET SDK (no Godot)
- [ ] `GD_xPRIMEray` builds by consuming `XPrimeRay.Core` as a package reference
- [ ] All Phase 0 baseline fixture outputs are reproduced via CLI
- [ ] Observatory catalog is populated by CLI run (not Godot run) for all 5 READY fixtures

### Rollback strategy

Revert `GD_xPRIMEray.csproj` to project reference. `src/XPrimeRay.Core/` is still in git history. The standalone repo can be archived without impact.

---

## Phase 8: Public Launch

**Goal:** `xprimeray-core` goes public. CLI testbench is installable. The wormhole transport demo becomes the public-facing showcase.

### Tasks

1. Make `xprimeray-core` GitHub repo public
2. Publish CLI as a `dotnet tool`: `dotnet tool install -g xprimeray-testbench`
3. Write Core docs: `architecture.md`, `adapter-contract.md`, `fixtures.md`, `migration-plan.md`, `mythos.md`
4. Build Godot WASM export of `fixture_wormhole_transport_demo.tscn` as the "Glowing Heart" showcase
5. Link WASM demo on MisterY Labs project page
6. Observatory catalog entry for WASM showcase run
7. Update `Docs/xPRIMEray/` to reference Core repo

### Wormhole Transport Demo as Showcase

`fixture_wormhole_transport_demo.tscn` (implemented 2026-06-19) demonstrates:
- Negative-amp throat `FieldSource3D` (effective n(r) < 1)
- 5 positive-amp orb ring (converging channel, animated)
- `WormholePortal` dual-world teleportation
- `WormholeTransportHUD` researcher quote overlay (Puthoff, Davis/Froning, Cramer, Miley)
- `PlasmaGlowOrb.gdshader` (Fresnel rim with redshift/blueshift)

This is the perceptual gateway: it demonstrates what the instrument sees, not just that the instrument validates. It is not a closure fixture — it is the face of the project for public audiences.

**WASM build requirements:**
- Godot 4 WASM export template installed
- No filesystem access required (fixtures are runtime-instantiated)
- HUD displays automatically; no user input required for demo loop

### Observatory integration

WASM showcase entry in catalog:
```json
{
  "artifact_type": "wasm_showcase",
  "fixture": "wormhole_transport_demo",
  "category": "Demo",
  "run_id": "public_launch_2026",
  "source_path": "https://misterylabs.dev/demos/glowing-heart/",
  "verdict": "OBSERVED"
}
```

### Acceptance criteria

- [ ] `xprimeray-core` GitHub repo is public with complete README and docs
- [ ] `dotnet tool install -g xprimeray-testbench` succeeds from NuGet.org
- [ ] WASM demo loads in Chrome/Firefox without Godot installed
- [ ] Observatory catalog links to WASM demo URL
- [ ] A new contributor can clone Core, run `dotnet build && dotnet test`, and see PASS in under 5 minutes

---

## Progress Tracker

| Phase | Status | Gate |
|---|---|---|
| 0: Freeze & Baseline | PENDING | git tag + baseline manifest |
| 1: Math Core Extraction | PENDING | `dotnet build` zero Godot refs |
| 2: Transport Extraction | PENDING | headless hermetic_curved_room run |
| 3: Fixture System | PENDING | 5 JSON fixtures + fixture-sync CI |
| 4: CLI Testbench | PENDING | `xpr run-fixture` exits 0 |
| 5: Godot Adapter | PENDING | Godot matches CLI output |
| 6: Validation Migration | PENDING | CI without Godot passes |
| 7: Repository Split | PENDING | Independent builds |
| 8: Public Launch | PENDING | WASM demo live |

---

## Agent Handoff Prompt

When handing this project to a new AI contributor, paste the following:

```
You are working on xPRIMEray-Core, the pure C# engine heart extracted from GD_xPRIMEray.

Repository context:
- GD_xPRIMEray: Godot 4 C# project at /home/bb/code/godot_xPRIMEray/
- Core library target: src/XPrimeRay.Core/ (may not exist yet; check Phase status)
- Architecture docs: Docs/xPRIMEray/project_glowing_heart_review.md
- Execution plan: Docs/xPRIMEray/project_glowing_heart_execution_plan.md
- Source proposal: Docs/xPRIMEray/xPRIMEray-Core_Split.md

Hard rules:
1. No Godot types in XPrimeRay.Core (enforced by Directory.Build.props)
2. Observatory catalog format is sacred — CLI output must match it exactly
3. The 15 READY fixtures are the regression baseline — any change must not alter their output
4. Fixture JSON and .tscn defaults must stay in sync — fixture-sync CI enforces this
5. The wormhole transport demo is the public showcase — do not alter its core behavior

Current phase: [INSERT PHASE 0-8]
Current task: [INSERT SPECIFIC TASK FROM PHASE]

Check the Progress Tracker table for current status before starting work.
```
