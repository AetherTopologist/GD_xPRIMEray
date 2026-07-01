# Project Glowing Heart v2.8 Atlas Graph Bridge

## What Changed

`tools/glowing_heart_index_to_atlas_graph.py` now translates the v2.6 Difference Packet Index into an Atlas Graph document. The generated graph contains one gallery project node, five exhibit artifact nodes, five bounded `produces` edges, and one exhibit group.

The Atlas Graph schema now permits optional `node.metadata`. Existing graphs do not require metadata, so the extension is backward-compatible.

## What This Demonstrates

Structured Difference Packet exhibits can become Atlas Graph nodes without parsing gallery prose. Each exhibit node preserves its status, rule, source case, fixtures, channels, metrics, fixed comparison claims, and original claim-boundary array.

## What This Does Not Demonstrate

- Core-vs-Core only.
- Not a Godot comparison.
- Not image or pixel comparison.
- Not parity.
- Not physical validation.
- Not renderer equivalence.
- The Atlas Graph bridge maps evidence structure only; it does not validate scientific correctness.

## Source Index

`reports/glowing_heart_v2_6_difference_packet_index.preview.json`

The generator enforces `parityClaim=NONE`, `runtimeExecuted=false`, and `comparisonMode=core_vs_core` for every entry before writing.

## Generated Atlas Graph

- graph JSON: `Docs/Observatory/Observation_Atlas/Atlas_Graph/glowing_heart_difference_packet_exhibits.graph.json`
- Markdown preview: `reports/atlas_graph_glowing_heart_difference_packet_exhibits.preview.md`
- bridge report: `reports/glowing_heart_v2_8_atlas_graph_bridge.preview.md`

## Graph Node Model

The root uses `type=project` and each exhibit uses `type=artifact`. Node tags expose status, rule, and left/right channels for lightweight renderers. Optional metadata carries the complete exhibit payload:

- status, rule, and source case
- left and right fixture and channel
- compared, maximum, mean, and non-zero metrics
- parity, runtime, and comparison-mode guards
- original structured claim boundary

The root `produces` each exhibit as an artifact relationship. Edge boundaries explicitly prevent the relationship from implying ranking, correctness, or eligibility beyond the recorded compatibility decision.

## Claim-Boundary Handling

Each exhibit keeps a readable `claimBoundary` string for standard Atlas renderers and the source boundary array in `metadata.claimBoundary`. The graph-level boundary comes from the index. No boundary is inferred from status or metrics.

## Future Tooling Path

Atlas tooling can use metadata for status colors, metric summaries, channel labels, and claim-boundary indicators. The standard Atlas Markdown renderer can continue using the stable common node fields.

## Next Milestone

Glowing Heart v2.9 can render a compact visual evidence map from the generated graph, showing the gallery root, five exhibits, status colors, and claim-boundary indicators without implying parity, validation, or proof.

