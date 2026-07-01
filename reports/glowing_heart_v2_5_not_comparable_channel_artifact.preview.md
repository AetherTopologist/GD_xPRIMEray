# Glowing Heart v2.5 NotComparable Channel Artifact Preview

## Retained Packet

- Packet: `/tmp/glowing_heart_v2_5_packet/20260701T030406Z_grin_radial_smoke`
- Fixture: `Fixtures/grin_radial_smoke.json`
- Bend artifact: `snapshot_heatmap.csv`
- Traversal artifact: `traversal_step_count.csv`
- Traversal rows: 880
- Executed step count represented in this fixture: 32 for each retained ray

## Difference Packet

- Output: `/tmp/glowing_heart_v2_5_compare/difference_packet.json`
- Left channel: `bend_magnitude_metric`
- Right channel: `traversal_step_count`
- Status: `NotComparable`
- Rule: `traversal_steps_vs_bend_not_comparable`
- Count compared: 0
- Maximum absolute difference: 0
- Mean absolute difference: 0
- Non-zero count: 0
- Reason: Bend magnitude and traversal step count represent different retained Core quantities and are not directly comparable without a declared transform.

## What This Shows

The compatibility matrix refuses value comparison for two authentic retained Core channels declared incompatible without a transform.

## Claim Boundary

Core-vs-Core only. This is not a Godot comparison, image or pixel comparison, parity claim, physical validation, renderer equivalence, or assessment of physical correctness. Bend data was not relabeled.
