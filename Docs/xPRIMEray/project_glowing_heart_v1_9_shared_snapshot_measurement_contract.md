# Project Glowing Heart v1.9 Shared Snapshot and Measurement Contract

## What changed since v1.8.4

The observer trail now has a companion contract that defines what a snapshot channel measures before any future pixel comparison.

## What was created

- a preview JSON Schema for channel identity, type, units, normalization, dynamic range, color model, comparison policy, claim boundary, and artifact references
- a fixed channel vocabulary covering bend magnitude, rendered intensity, RGB, depth, hit/miss, closure state, and unknown channels
- comparison statuses for comparable, not comparable, transform-required, and unknown relationships
- an explicit rule that Core bend-magnitude measurements are not comparable to Godot RGB or rendered-intensity output without a defined transform

## What this proves

Project Glowing Heart can describe the semantics and comparison eligibility of observation channels without assuming that all image-shaped artifacts contain the same kind of value.

## What this does not prove

No parity.
No pixel comparison.
No Godot runtime execution.
No renderer equivalence.
No transport equivalence.

## Outputs

```txt
schemas/glowing_heart/shared_snapshot_measurement_contract.v0.preview.json
reports/glowing_heart_shared_snapshot_measurement_contract.preview.md
Docs/xPRIMEray/project_glowing_heart_v1_9_shared_snapshot_measurement_contract.md
```

## Next milestone

v2.0 should design a Difference Packet that binds aligned observers, declared channels, concrete artifacts, comparison rules, transforms, tolerances, and claim-safe result metadata.
