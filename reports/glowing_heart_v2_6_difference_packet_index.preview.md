# Glowing Heart v2.6 Difference Packet Index Preview

This index turns the five verified Difference Packet Gallery exhibits into structured records for future renderers. Durable references use repository paths; `/tmp` paths are labeled only as the latest verification outputs.

## Status Summary

| Status | Exhibits |
|---|---:|
| `Comparable` | 2 |
| `Unknown` | 2 |
| `NotComparable` | 1 |
| `RequiresTransform` | 0 |

## Exhibits

| ID | Case | Status | Rule | Channels | Compared | Non-zero | Source report |
|---|---|---|---|---|---:|---:|---|
| `comparable_zero` | A | `Comparable` | `bend_magnitude_same_observer` | bend / bend | 880 | 0 | `reports/glowing_heart_v2_3_difference_fixture_cases.preview.md` |
| `comparable_nonzero` | B | `Comparable` | `bend_magnitude_same_observer` | bend / bend | 880 | 872 | `reports/glowing_heart_v2_3_difference_fixture_cases.preview.md` |
| `unknown_observer_mismatch` | C | `Unknown` | `bend_magnitude_context_mismatch` | bend / bend | 0 | 0 | `reports/glowing_heart_v2_3_difference_fixture_cases.preview.md` |
| `unknown_coordinate_grid_mismatch` | D | `Unknown` | `bend_magnitude_context_mismatch` | bend / bend | 0 | 0 | `reports/glowing_heart_v2_3_difference_fixture_cases.preview.md` |
| `not_comparable_channel` | E | `NotComparable` | `traversal_steps_vs_bend_not_comparable` | bend / traversal steps | 0 | 0 | `reports/glowing_heart_v2_5_not_comparable_channel_artifact.preview.md` |

## Runtime Terminology

Core transport executed to produce the retained artifacts. An indexed Difference Packet records `runtimeExecuted=false` because the comparison stage did not execute a runtime or Godot; it only read retained artifacts and evaluated declared compatibility.

## Claim Boundary

- Core-vs-Core only.
- Not a Godot comparison.
- Not image or pixel comparison.
- Not parity.
- Not physical validation.
- Not renderer equivalence.
- Index validity describes exhibit structure, not scientific correctness.

## Future Tooling

The JSON index supplies stable IDs, metrics, status vocabulary, source references, and renderer hints for tables, cards, matrices, and Atlas Graph nodes. A future renderer can consume the index without parsing prose reports.

