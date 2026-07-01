# Project Glowing Heart v2.5 NotComparable Channel Artifact

## What Changed

Core retained packets now include `traversal_step_count.csv`. Each row records the executed `MetricRayState.IntegrationSteps` count for one ray coordinate. The existing transport loop already produced this count; v2.5 retains it without changing transport decisions or relabeling bend data.

The run manifest declares both retained channels with their channel ID, kind, Core producer, representation, artifact path, and coordinate-grid dimensions. Fixture identity, fixture comparison identity, and observer basis remain declared in the fixture manifest block.

## CLI

Same-channel comparisons remain backward-compatible:

```bash
dotnet run --project src/XPrimeRay.Testbench.Cli -- compare-packets \
  <left_packet> <right_packet> \
  --channel bend_magnitude_metric --output <comparison_output>
```

Cross-channel eligibility uses explicit sides:

```bash
dotnet run --project src/XPrimeRay.Testbench.Cli -- compare-packets \
  <left_packet> <right_packet> \
  --left-channel bend_magnitude_metric \
  --right-channel traversal_step_count \
  --output <comparison_output>
```

## Case E Result

| Field | Value |
|---|---|
| Left channel | `bend_magnitude_metric` |
| Right channel | `traversal_step_count` |
| Status | `NotComparable` |
| Rule | `traversal_steps_vs_bend_not_comparable` |
| Compared | 0 |
| Maximum difference | 0 |
| Mean difference | 0 |
| Non-zero count | 0 |
| Reason | Bend magnitude and traversal step count represent different retained Core quantities and are not directly comparable without a declared transform. |

The matrix decision occurs before value comparison. This demonstrates declared incompatibility handling only.

## Claim Boundary

- Core-vs-Core only.
- Not a Godot comparison.
- Not image or pixel comparison.
- Not parity.
- Not physical validation.
- Not renderer equivalence.
- This does not compare physical correctness.
- No bend values were relabeled as traversal steps.

Every Difference Packet preserves `runtimeExecuted=false`, `parityClaim=NONE`, and `comparisonMode=core_vs_core`.

## Next Milestone

Glowing Heart v2.6 can create a machine-readable index of the v2.3-v2.5 packet exhibits so future Atlas Graph and Gallery tooling can render the evidence wall automatically.

