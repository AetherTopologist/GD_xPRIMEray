# Glowing Heart v2.4 Difference Packet Gallery Preview

This visitor index presents the verified v2.3.1 retained Core cases. It does not add comparison capability.

## Gallery

| Entry | Fixtures | Channel | Status | Rule | Compared | Max | Mean | Non-zero | What this shows |
|---|---|---|---|---|---:|---:|---:|---:|---|
| Comparable Zero | base vs base | `bend_magnitude_metric` | `Comparable` | `bend_magnitude_same_observer` | 880 | 0 | 0 | 0 | Deterministic retained Core values are identical. |
| Comparable Non-Zero | base vs amplitude variant | `bend_magnitude_metric` | `Comparable` | `bend_magnitude_same_observer` | 880 | 0.00031490779 | 0.00019547191841352225 | 872 | Declared-compatible retained Core artifacts are numerically distinct. |
| Unknown Observer Mismatch | base vs observer variant | `bend_magnitude_metric` | `Unknown` | `bend_magnitude_context_mismatch` | 0 | 0 | 0 | 0 | Observer mismatch prevents value comparison. |
| Unknown Coordinate-Grid Mismatch | base vs resolution variant | `bend_magnitude_metric` | `Unknown` | `bend_magnitude_context_mismatch` | 0 | 0 | 0 | 0 | Coordinate-grid mismatch prevents value comparison. |
| Deferred Incompatible Channel | no authentic pair | Not available | `Deferred` | Not evaluated | 0 | 0 | 0 | 0 | The required retained channel artifact does not exist yet. |

## Fixture Paths

- Base: `Fixtures/grin_radial_smoke.json`
- Amplitude variant: `Fixtures/grin_radial_smoke_variant.json`
- Observer variant: `Fixtures/grin_radial_smoke_observer_variant.json`
- Resolution variant: `Fixtures/grin_radial_smoke_resolution_variant.json`

## Status Vocabulary

- `Comparable`: all declared eligibility conditions passed.
- `Unknown`: eligibility was not established, so values were not compared.
- `NotComparable`: reserved for a declared incompatible channel pair.
- `Deferred`: the correct retained artifact is not available.

## What This Does Not Show

The gallery is Core-vs-Core only. It is not a Godot comparison, image or pixel comparison, parity claim, physical validation, or renderer equivalence. Non-zero difference demonstrates numeric distinction between retained Core artifacts only. Zero difference does not establish equivalence with another runtime or measurement system.

## Claim Boundary

Every represented Difference Packet preserves `runtimeExecuted=false`, `parityClaim=NONE`, and `comparisonMode=core_vs_core`. Case E has no packet and remains deferred; bend data was not relabeled.

