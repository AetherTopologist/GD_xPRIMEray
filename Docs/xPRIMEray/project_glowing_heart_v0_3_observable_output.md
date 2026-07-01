# Project Glowing Heart v0.3 Observable Output

## What changed since v0.2

The Core CLI now emits output artifacts for fixture runs:

- `manifest.json`
- `ray_metrics.csv`
- `run_summary.md`

## What this proves

The headless Core can produce portable evidence from a deterministic field-driven fixture.

## What this does not prove

No Godot parity, hermetic closure, collision behavior, portal behavior, or physical correctness is claimed.

## Commands

```bash
dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture Fixtures/grin_radial_smoke.json
dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture Fixtures/grin_radial_smoke.json --no-output
dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture Fixtures/grin_radial_smoke.json --output output/glowing_heart
```

## Next minimal milestone

v0.4 should introduce either manifest compatibility with the existing Observatory catalog, or the first Godot/Core comparison artifact for one READY fixture.
