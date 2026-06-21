# Project Glowing Heart v0.6 First Human Observable

## What changed since v0.5

The Core CLI now emits three human-observable transport metric snapshots:

- `snapshot.ppm`
- `snapshot_heatmap.csv`
- `snapshot_ascii.txt`

## What this proves

The headless Core can now produce a visible artifact from a deterministic field-driven fixture without launching Godot.

## What this does not prove

This is not a rendered scene. This is not Godot parity. This is not hermetic closure. This is not collision behavior. This is not portal behavior. This is not full optical correctness.

## How to view

Run:

```bash
dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture fixtures/grin_radial_smoke.json
```

Then open:

```txt
output/glowing_heart/<run_id>/snapshot.ppm
```

Or inspect:

```txt
output/glowing_heart/<run_id>/snapshot_ascii.txt
```

## Next minimal milestone

v0.7 should begin the first comparison bridge: Core snapshot artifact vs. nearest Godot fixture artifact, with no parity claim yet.
