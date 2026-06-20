# Project Glowing Heart v0.2 First Pulse

## What changed since v0.1

- Added `fixtures/grin_radial_smoke.json`.
- Added Core DTO support for observer data and radial GRIN field parameters.
- Replaced the CLI-only placeholder path for `radial_grin_smoke` with a deterministic Core ray stepper.
- Added validation for field sample counts, nonzero mean bend, and finite numeric output.

## What this proves

The Core can now load a fixture and run a deterministic field-driven transport smoke test without Godot. Rays advance from a fixed observer, sample a radial GRIN-like field, bend deterministically, and emit validation-style bend statistics.

## What this does not prove

This does not prove Godot visual parity, hermetic closure, physical optical correctness, fixture equivalence with READY scenes, collision behavior, portal behavior, or renderer lifecycle integration.

## Acceptance commands

```bash
dotnet build src/XPrimeRay.Core/XPrimeRay.Core.csproj
dotnet build src/XPrimeRay.Testbench.Cli/XPrimeRay.Testbench.Cli.csproj
dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture fixtures/glowing_heart_minimal.json
dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture fixtures/grin_radial_smoke.json
dotnet build "Physical Light and Camera Units.csproj" --no-restore
rg -n "using Godot|Godot.NET.Sdk|GodotSharp" src/XPrimeRay.Core
```

## Next milestone

v0.3 should either export and replay a Godot snapshot through Core, or establish one READY fixture parity test with documented tolerances.
