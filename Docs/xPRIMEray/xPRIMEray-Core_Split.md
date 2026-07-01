Absolutely. This is the **xPRIMEray-Core split**: take the glowing engine-heart out of Godot, give it a clean lab bench, and let every future engine become just an observer window 🐝🔭

```txt
xPRIMEray-Core/
├─ README.md
├─ .gitignore
├─ LICENSE
├─ Directory.Build.props
├─ xprimeray-core.sln
│
├─ src/
│  ├─ XPrimeRay.Core/
│  │  ├─ Fields/
│  │  │  ├─ IScalarField.cs
│  │  │  ├─ IVectorField.cs
│  │  │  ├─ GrinField.cs
│  │  │  └─ WormholeField.cs
│  │  ├─ Geometry/
│  │  │  ├─ Vec3.cs
│  │  │  ├─ RayState.cs
│  │  │  ├─ Bounds.cs
│  │  │  └─ OpticalIsland.cs
│  │  ├─ Integrators/
│  │  │  ├─ ITransportIntegrator.cs
│  │  │  ├─ EulerIntegrator.cs
│  │  │  ├─ Rk4Integrator.cs
│  │  │  └─ IntegrationResult.cs
│  │  ├─ Transport/
│  │  │  ├─ TransportScene.cs
│  │  │  ├─ TransportRunner.cs
│  │  │  ├─ TransportSettings.cs
│  │  │  └─ HitRecord.cs
│  │  ├─ Fixtures/
│  │  │  ├─ FixtureDefinition.cs
│  │  │  ├─ FixtureLoader.cs
│  │  │  └─ FixtureRegistry.cs
│  │  ├─ Validation/
│  │  │  ├─ ClosureValidator.cs
│  │  │  ├─ SweepRunner.cs
│  │  │  └─ ValidationReport.cs
│  │  └─ XPrimeRay.Core.csproj
│  │
│  ├─ XPrimeRay.Adapters/
│  │  ├─ Godot/
│  │  │  ├─ GodotVectorMapper.cs
│  │  │  ├─ GodotSceneAdapter.cs
│  │  │  └─ README.md
│  │  ├─ Unreal/
│  │  │  └─ README.md
│  │  └─ XPrimeRay.Adapters.csproj
│  │
│  └─ XPrimeRay.Testbench.Cli/
│     ├─ Program.cs
│     ├─ Commands/
│     │  ├─ RunFixtureCommand.cs
│     │  ├─ SweepCommand.cs
│     │  ├─ RenderCommand.cs
│     │  └─ ValidateCommand.cs
│     ├─ Output/
│     │  ├─ CsvReportWriter.cs
│     │  └─ ManifestWriter.cs
│     └─ XPrimeRay.Testbench.Cli.csproj
│
├─ Fixtures/
│  ├─ hermetic_curved_room.json
│  ├─ curved_minimal.json
│  ├─ object_island.json
│  ├─ corner_probe_reference.json
│  └─ oracle_closure.experimental.json
│
├─ tests/
│  ├─ XPrimeRay.Core.Tests/
│  │  ├─ GrinFieldTests.cs
│  │  ├─ Rk4IntegratorTests.cs
│  │  ├─ ClosureValidatorTests.cs
│  │  └─ XPrimeRay.Core.Tests.csproj
│  └─ XPrimeRay.Testbench.Tests/
│
├─ Docs/
│  ├─ architecture.md
│  ├─ migration-plan.md
│  ├─ adapter-contract.md
│  ├─ fixtures.md
│  ├─ validation-philosophy.md
│  └─ mythos.md
│
├─ assets/
│  ├─ sigils/
│  │  └─ bee-sigil.svg
│  └─ branding/
│
└─ output/
   └─ .gitkeep
```

## `.gitignore`

```gitignore
bin/
obj/
.vs/
.vscode/*
!.vscode/extensions.json
*.user
*.suo
*.log

output/*
!output/.gitkeep

TestResults/
coverage/
*.trx

*.png
*.mp4
*.gif
*.exr
*.blend

.env
.env.*
```

## Initial `README.md`

````md
# xPRIMEray-Core

Pure C# optical transport engine core for xPRIMEray.

This repository contains the engine heart: fields, GRIN curvature, wormhole transport math,
ray-state integration, fixture validation, sweep automation, and portable testbench tooling.

No Godot dependencies. No rendering-engine lock-in.

Godot, Unreal, Blender, web viewers, and future observatory shells connect through adapters.

## Vision

xPRIMEray is an optical transport observatory.

It asks:

- What does light do inside curved media?
- How do observers disagree?
- When does a scene close hermetically?
- Where do optical islands form?
- Can geometry, perception, and validation become one instrument?

The Core is the rigorous layer.

MisterY Labs / AetherTopologist remains the mythic-human interface layer: bee sigil, perceptual expansion,
safe rabbit holes, inspiration constellations, and exploratory observatory artifacts.

The separation is intentional:

```txt
xPRIMEray-Core      = engine, math, fixtures, validation
GD_xPRIMEray        = Godot observer shell
MisterY Labs        = public mythos, gallery, story, interface
````

## Project Goals

* Pure C# core
* Zero Godot dependencies
* CLI-first Testbench
* Fixture-driven validation
* Adapter-ready architecture
* Batch captures and sweeps
* Human-readable manifests
* Claude/Codex/Grok friendly repo layout

## First Commands

```bash
dotnet build
dotnet test

dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture Fixtures/hermetic_curved_room.json
dotnet run --project src/XPrimeRay.Testbench.Cli -- sweep Fixtures/hermetic_curved_room.json --param curvatureAmp=0,0.25,0.5,0.75,1.0
dotnet run --project src/XPrimeRay.Testbench.Cli -- validate output/latest/manifest.json
```

## Core Rule

The Core does not know what Godot is.

Adapters translate engine-specific concepts into Core concepts.

````

## Migration plan from `GD_xPRIMEray`

```md
# Migration Plan

## Phase 0: Freeze current working baseline

- Tag current Godot repo:
  - `gd-xprimeray-pre-core-split`
- Preserve current fixtures, reports, screenshots, and validation outputs.
- Do not refactor while migrating.

## Phase 1: Extract pure math types

Move or recreate:

- vector math
- ray state
- bounds
- field sampling
- GRIN field parameters
- wormhole / overspace math
- integrator structs

Target:

```txt
src/XPrimeRay.Core/Geometry
src/XPrimeRay.Core/Fields
src/XPrimeRay.Core/Integrators
````

Rule:

* No `Godot.Vector3`
* No `Node`
* No `Resource`
* No scene-tree dependency
* No engine input/output calls

## Phase 2: Extract transport runner

Move logic responsible for:

* ray stepping
* hit/miss recording
* traversal budget
* closure detection
* row/tile/checker traversal strategy
* diagnostic counters

Target:

```txt
src/XPrimeRay.Core/Transport
src/XPrimeRay.Core/Validation
```

## Phase 3: Convert canonical fixtures

Current fixtures become JSON definitions.

Start with:

* `hermetic_curved_room`
* `curved_minimal`
* `object_island`
* `corner_probe_reference`
* `oracle_closure.experimental`

Each fixture should define:

* scene bounds
* observer/camera origin
* ray grid
* field type
* curvature settings
* transport settings
* expected validation gates

## Phase 4: Build Testbench CLI

CLI becomes the first official non-Godot consumer.

Minimum commands:

```bash
xpr run-fixture Fixtures/hermetic_curved_room.json
xpr sweep Fixtures/hermetic_curved_room.json --param curvatureAmp=0,0.25,0.5,0.75,1
xpr render Fixtures/hermetic_curved_room.json --width 320 --height 180
xpr validate output/latest/manifest.json
```

## Phase 5: Replace Godot internals with adapter calls

Godot should become:

* input
* visualization
* HUD
* capture
* UI shell

Godot should not own:

* integrator logic
* GRIN math
* validation rules
* fixture truth
* closure semantics

## Phase 6: Add future adapters

Adapter targets:

* Godot
* Unreal
* Blender
* Web/WASM
* headless batch farm

````

## Testbench CLI skeleton

```csharp
// src/XPrimeRay.Testbench.Cli/Program.cs

using XPrimeRay.Core.Fixtures;
using XPrimeRay.Core.Transport;
using XPrimeRay.Core.Validation;

if (args.Length == 0)
{
    PrintHelp();
    return 1;
}

var command = args[0];

return command switch
{
    "run-fixture" => RunFixture(args),
    "sweep" => RunSweep(args),
    "render" => RunRender(args),
    "validate" => Validate(args),
    _ => Unknown(command)
};

static int RunFixture(string[] args)
{
    var fixturePath = args.ElementAtOrDefault(1);

    if (string.IsNullOrWhiteSpace(fixturePath))
    {
        Console.Error.WriteLine("Missing fixture path.");
        return 1;
    }

    var fixture = FixtureLoader.Load(fixturePath);

    var scene = TransportScene.FromFixture(fixture);
    var runner = new TransportRunner(scene, fixture.TransportSettings);

    var result = runner.Run();

    var report = ClosureValidator.Validate(result, fixture.Validation);

    Console.WriteLine($"Fixture: {fixture.Name}");
    Console.WriteLine($"Rays: {result.RayCount}");
    Console.WriteLine($"Hits: {result.HitCount}");
    Console.WriteLine($"Misses: {result.MissCount}");
    Console.WriteLine($"Closure: {(report.Passed ? "PASS" : "FAIL")}");

    return report.Passed ? 0 : 2;
}

static int RunSweep(string[] args)
{
    Console.WriteLine("Sweep command placeholder.");
    Console.WriteLine("Example: sweep fixture.json --param curvatureAmp=0,0.25,0.5,0.75,1.0");
    return 0;
}

static int RunRender(string[] args)
{
    Console.WriteLine("Render command placeholder.");
    Console.WriteLine("CLI render should emit raw buffers, CSV diagnostics, and manifest metadata.");
    return 0;
}

static int Validate(string[] args)
{
    Console.WriteLine("Validate command placeholder.");
    return 0;
}

static int Unknown(string command)
{
    Console.Error.WriteLine($"Unknown command: {command}");
    PrintHelp();
    return 1;
}

static void PrintHelp()
{
    Console.WriteLine("""
    xPRIMEray-Core Testbench

    Commands:
      run-fixture <fixture.json>
      sweep <fixture.json> --param name=a,b,c
      render <fixture.json> --width 320 --height 180
      validate <manifest.json>
    """);
}
````

## Core interfaces

```csharp
namespace XPrimeRay.Core.Fields;

public interface IScalarField
{
    double Sample(double x, double y, double z);
}
```

```csharp
namespace XPrimeRay.Core.Geometry;

public readonly record struct Vec3(double X, double Y, double Z)
{
    public static Vec3 operator +(Vec3 a, Vec3 b) =>
        new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vec3 operator *(Vec3 v, double s) =>
        new(v.X * s, v.Y * s, v.Z * s);
}
```

```csharp
namespace XPrimeRay.Core.Geometry;

public readonly record struct RayState(
    Vec3 Position,
    Vec3 Direction,
    double OpticalTime,
    int StepIndex
);
```

```csharp
namespace XPrimeRay.Core.Integrators;

using XPrimeRay.Core.Geometry;

public interface ITransportIntegrator
{
    RayState Step(RayState state, double stepSize);
}
```

```csharp
namespace XPrimeRay.Core.Transport;

public sealed class TransportRunner
{
    private readonly TransportScene _scene;
    private readonly TransportSettings _settings;

    public TransportRunner(TransportScene scene, TransportSettings settings)
    {
        _scene = scene;
        _settings = settings;
    }

    public TransportResult Run()
    {
        // Placeholder for row/tile/checker traversal.
        return new TransportResult(
            RayCount: 0,
            HitCount: 0,
            MissCount: 0
        );
    }
}

public readonly record struct TransportResult(
    int RayCount,
    int HitCount,
    int MissCount
);
```

## Example fixture JSON

```json
{
  "name": "hermetic_curved_room",
  "description": "Closed optical room with GRIN curvature field.",
  "scene": {
    "bounds": {
      "min": [-3, -3, -3],
      "max": [3, 3, 3]
    }
  },
  "observer": {
    "origin": [0, 0, -2.5],
    "forward": [0, 0, 1],
    "up": [0, 1, 0],
    "fovDegrees": 70
  },
  "rayGrid": {
    "width": 320,
    "height": 180
  },
  "field": {
    "type": "grin_radial",
    "radiusOuter": 3.0,
    "amplitude": 0.6
  },
  "transportSettings": {
    "stepSize": 0.015,
    "maxStepsPerRay": 700,
    "traversal": "row"
  },
  "validation": {
    "requireHermeticClosure": true,
    "maxMisses": 0
  }
}
```

## Godot adapter consumption example

```csharp
// GD_xPRIMEray side, not Core side.

using Godot;
using XPrimeRay.Core.Geometry;
using XPrimeRay.Core.Transport;
using XPrimeRay.Core.Fixtures;

public partial class XPrimeRayGodotRunner : Node
{
    [Export] public string FixturePath = "res://Fixtures/hermetic_curved_room.json";

    public override void _Ready()
    {
        var fixture = FixtureLoader.Load(ProjectSettings.GlobalizePath(FixturePath));

        var scene = TransportScene.FromFixture(fixture);
        var runner = new TransportRunner(scene, fixture.TransportSettings);

        var result = runner.Run();

        GD.Print($"xPRIMEray Core result:");
        GD.Print($"Rays: {result.RayCount}");
        GD.Print($"Hits: {result.HitCount}");
        GD.Print($"Misses: {result.MissCount}");
    }

    private static Vector3 ToGodot(Vec3 v) =>
        new((float)v.X, (float)v.Y, (float)v.Z);

    private static Vec3 FromGodot(Vector3 v) =>
        new(v.X, v.Y, v.Z);
}
```

## Collaboration prompt for Claude/Codex/Grok

```md
You are working in the xPRIMEray-Core repository.

Mission:
Build a pure C# optical transport core extracted from GD_xPRIMEray.

Hard rules:
- No Godot dependencies in XPrimeRay.Core.
- No engine-specific types in Core.
- All fixtures must be file-driven.
- CLI Testbench is the first official consumer.
- Adapters translate external engine concepts into Core concepts.
- Preserve MisterY Labs / AetherTopologist mythos only in docs, assets, and public interface language, not in mathematical internals.

Immediate tasks:
1. Create the proposed folder structure.
2. Add .gitignore, README.md, Directory.Build.props, and solution file.
3. Add XPrimeRay.Core project.
4. Add Vec3, RayState, IScalarField, ITransportIntegrator, TransportRunner skeletons.
5. Add XPrimeRay.Testbench.Cli project with run-fixture command.
6. Add Fixtures/hermetic_curved_room.json.
7. Add Docs/migration-plan.md and Docs/adapter-contract.md.
8. Add tests for Vec3 and fixture loading.

Definition of done:
- `dotnet build` passes.
- `dotnet test` passes.
- CLI accepts `run-fixture Fixtures/hermetic_curved_room.json`.
- Core project contains zero Godot references.
```

This split is exactly right: **Core becomes the instrument**, Godot becomes the first telescope, and MisterY Labs becomes the star map 🐝🔭
