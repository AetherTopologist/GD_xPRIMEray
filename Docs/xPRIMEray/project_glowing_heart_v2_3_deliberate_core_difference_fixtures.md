# Project Glowing Heart v2.3 Deliberate Core Difference Fixtures

## What changed

Three Core fixture variants now exercise deliberate Difference Packet decisions: a changed field amplitude, a changed observer field of view, and a changed ray-grid resolution. Fixtures may declare a transport-neutral `comparisonIdentity`; retained manifests and Difference Packets preserve both the concrete fixture name and that identity.

The shared identity lets distinct, explicitly related Core fixtures reach the compatibility matrix without disguising them as the same fixture. It does not bypass the matrix: observer and coordinate-grid requirements still apply before value comparison.

## What this demonstrates

The retained comparison path distinguishes four outcomes: deterministic zero difference, eligible non-zero difference, observer mismatch, and coordinate-grid mismatch. The non-zero case shows only that two declared-compatible retained Core scalar grids contain numerically distinct values after a fixture field parameter changes.

## What this does not demonstrate

- Core-vs-Core only.
- Not a Godot comparison.
- Not image or pixel comparison.
- Not parity.
- Not physical validation.
- Not renderer equivalence.
- Non-zero difference demonstrates numeric distinction between retained Core artifacts only.
- Zero difference between deterministic Core packets does not establish equivalence with another runtime or measurement system.

## Case table

| Case | Input change | Status | Rule | Compared | Max difference | Mean difference | Non-zero |
|---|---|---|---|---:|---:|---:|---:|
| A | none; repeated deterministic base fixture | `Comparable` | `bend_magnitude_same_observer` | 880 | 0 | 0 | 0 |
| B | field amplitude 0.25 to 0.3 | `Comparable` | `bend_magnitude_same_observer` | 880 | 0.00031490779 | 0.00019547191841352225 | 872 |
| C | observer FOV 60 to 61 degrees | `Unknown` | `bend_magnitude_context_mismatch` | 0 | 0 | 0 | 0 |
| D | grid 40x22 to 41x22 | `Unknown` | `bend_magnitude_context_mismatch` | 0 | 0 | 0 | 0 |
| E | incompatible retained channel | Deferred | Not applicable | 0 | 0 | 0 | 0 |

Case E is deferred because the current retained packet contains `bend_magnitude_metric`, not an authentic `traversal_step_count` grid. Relabeling bend values would make the artifact misleading.

## Commands

Generate each input with the Core CLI:

```bash
dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture fixtures/grin_radial_smoke.json --output /tmp/glowing_heart_v2_3/base_left
dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture fixtures/grin_radial_smoke_variant.json --output /tmp/glowing_heart_v2_3/field_variant
dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture fixtures/grin_radial_smoke_observer_variant.json --output /tmp/glowing_heart_v2_3/observer_variant
dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture fixtures/grin_radial_smoke_resolution_variant.json --output /tmp/glowing_heart_v2_3/grid_variant
```

Compare a retained pair:

```bash
dotnet run --project src/XPrimeRay.Testbench.Cli -- compare-packets \
  <left_packet_directory> <right_packet_directory> \
  --channel bend_magnitude_metric --output <comparison_directory>
```

## Failure modes

- An undeclared requested channel returns `Unknown` without value comparison.
- A requested channel that does not match both retained declarations returns `Unknown`.
- Different comparison identities return `Unknown`.
- Observer or coordinate-grid mismatches return `Unknown` with zero metrics.
- Missing or malformed manifests and scalar-grid artifacts remain command errors.
- An incompatible channel pair can return `NotComparable` only when both authentic channel artifacts are available and declared.

## Claim boundaries

`runtimeExecuted=false` and `parityClaim=NONE` remain fixed in every Difference Packet. Compatibility is an artifact eligibility decision, not a statement about scientific correctness or another runtime.

## Next milestone

Glowing Heart v2.4 can create a Difference Packet Gallery for the zero, non-zero, observer-mismatch, and coordinate-grid-mismatch cases. An incompatible-channel gallery entry should wait for an authentic second retained Core channel. The gallery remains Core-vs-Core only.

