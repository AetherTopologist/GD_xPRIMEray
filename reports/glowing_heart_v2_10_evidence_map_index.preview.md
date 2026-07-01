# Glowing Heart v2.10 Evidence Map Index Preview

The evidence map index makes one verified route discoverable across the structured Difference Packet catalog, Atlas Graph, rendered evidence map, and visitor gallery.

## Artifact Chain

| Stage | Artifact | Generator |
|---|---|---|
| Difference Packet Index | `reports/glowing_heart_v2_6_difference_packet_index.preview.json` | Difference Packet indexing workflow |
| Atlas Graph | `Docs/Observatory/Observation_Atlas/Atlas_Graph/glowing_heart_difference_packet_exhibits.graph.json` | `tools/glowing_heart_index_to_atlas_graph.py` |
| Evidence Map | `reports/glowing_heart_v2_9_evidence_map.svg` | `tools/atlas_graph_evidence_map_renderer.py` |
| Gallery | `Docs/xPRIMEray/project_glowing_heart_v2_4_difference_packet_gallery.md` | `tools/glowing_heart_gallery_renderer.py` |

Graph ID: `glowing_heart.difference_packet_exhibits`

## Exhibits

| Exhibit | Status | Rule | Graph node | Gallery section | Map card |
|---|---|---|---|---|---|
| `comparable_zero` | `Comparable` | `bend_magnitude_same_observer` | `comparable_zero` | `#comparable-zero` | Comparable Zero |
| `comparable_nonzero` | `Comparable` | `bend_magnitude_same_observer` | `comparable_nonzero` | `#comparable-non-zero` | Comparable Non-Zero |
| `unknown_observer_mismatch` | `Unknown` | `bend_magnitude_context_mismatch` | `unknown_observer_mismatch` | `#unknown-observer-mismatch` | Unknown Observer Mismatch |
| `unknown_coordinate_grid_mismatch` | `Unknown` | `bend_magnitude_context_mismatch` | `unknown_coordinate_grid_mismatch` | `#unknown-coordinate-grid-mismatch` | Unknown Coordinate-Grid Mismatch |
| `not_comparable_channel` | `NotComparable` | `traversal_steps_vs_bend_not_comparable` | `not_comparable_channel` | `#notcomparable-channel` | NotComparable Channel |

## Status Counts

| Status | Count |
|---|---:|
| `Comparable` | 2 |
| `Unknown` | 2 |
| `NotComparable` | 1 |
| `RequiresTransform` | 0 |

## Claim Boundary

- Core-vs-Core only.
- Not a Godot comparison.
- Not image or pixel comparison.
- Not parity.
- Not physical validation.
- Not renderer equivalence.
- Index validity describes exhibit structure, not scientific correctness.

`runtimeExecuted=false` records that this indexing and rendering stage did not execute a runtime. It does not describe the historical generation of retained Core source artifacts.

Structured source: `reports/glowing_heart_v2_10_evidence_map_index.preview.json`

## Future Tooling

A future health check can walk this recorded chain and reject drift in exhibit IDs, counts, statuses, claim boundaries, or generated artifact paths.
