# Project Glowing Heart v2.1 Channel Registry and Compatibility Matrix

## What changed

Difference Packet status now comes from a named channel compatibility rule. A Core registry declares channel identity and representation metadata, while a compatibility matrix evaluates a pair under explicit fixture and observer context.

The registry declares comparison rules through the matrix. It does not replace snapshot data, observer declarations, or retained artifact provenance.

## Initial channels

| Channel | Kind | Representation |
|---|---|---|
| `bend_magnitude_metric` | measurement | scalar grid |
| `ray_hit_flag` | measurement | binary grid |
| `traversal_step_count` | measurement | scalar grid |
| `observer_basis` | identity | metadata |
| `fixture_identity` | identity | metadata |
| `runtime_metadata` | metadata | metadata |

## Initial compatibility rules

| Rule | Status | Conditions |
|---|---|---|
| `bend_magnitude_same_observer` | `Comparable` | same channel type, fixture identity, observer basis, and coordinate grid |
| `ray_hit_same_fixture_observer` | `Comparable` | same channel type, fixture identity, observer basis, and coordinate grid |
| `traversal_steps_vs_bend_not_comparable` | `NotComparable` | none |
| `core_to_future_image_requires_transform` | `RequiresTransform` | declared transform |
| `missing_channel_declaration` | `Unknown` | declared channel identity is absent |

An undeclared pair defaults to `Unknown`. No Core channel paired with a future Godot or image channel can be `Comparable` in v2.1.

## Selector semantics

The preview matrix uses structured selector objects. Fields within one selector are combined with AND. An `anyOf` list combines selector objects with OR. For example, the future-image rule matches a right-side channel when `producer` is `future_godot` OR `kind` is `image`, exactly mirroring the C# predicate.

## Required condition tokens

| Token | Meaning |
|---|---|
| `same_observer_basis` | Observer pose, direction, projection, sampling, and resolution are declared compatible |
| `same_fixture_identity` | Both snapshots identify the same fixture source and basis |
| `declared_transform` | A documented transform exists and is cited before comparison |
| `same_channel_type` | Both sides declare the same channel identifier |
| `same_coordinate_grid` | Sample coordinates align |

All five tokens are part of the preview vocabulary. Tokens not exercised by the current Core self-comparison are reserved for later milestones and do not authorize additional comparison scope.

## Implementation authority

The v2.1 C# `ChannelCompatibilityMatrix` is the runtime authority. The preview JSON matrix is a declarative mirror used for documentation, inspection, and future tooling. If they diverge, this is a documentation/tooling defect to fix before relying on external consumers.

## Difference Packet integration

The v2.0 Core self-comparison still compares one `bend_magnitude_metric` grid with itself. Its `Comparable` status is now backed by `bend_magnitude_same_observer`, and emitted packets record:

```txt
compatibilityRuleId
channelRegistryVersion
compatibilityMatrixVersion
```

The expected self-comparison remains 880 compared samples, maximum absolute difference 0, mean absolute difference 0, and non-zero count 0.

The packet keeps schema ID `xprimeray.glowing_heart.difference_packet.v2.0`. The v2.1 fields `compatibilityRuleId`, `channelRegistryVersion`, and `compatibilityMatrixVersion` are optional preview extensions in that schema. This avoids disrupting existing v2.0 readers while allowing new packets to cite their compatibility source.

## Artifacts

```txt
schemas/glowing_heart/channel_registry.v0.preview.json
schemas/glowing_heart/channel_compatibility_matrix.v0.preview.json
Docs/xPRIMEray/glowing_heart/channel_registry.preview.json
Docs/xPRIMEray/glowing_heart/channel_compatibility_matrix.preview.json
```

## Claim boundary

The registry and matrix declare channel identity and comparison eligibility. They do not validate scientific correctness. They do not establish renderer equivalence. They do not compare Core to Godot. They do not perform image comparison or prove physical truth.

No parity claim.
No physical-validation claim.
No equivalence claim.
No proof claim.

## Next milestone

v2.2 should compare two separately retained Core snapshots with the same declared channel, fixture identity, observer basis, and coordinate grid. It should not compare Core to Godot.
