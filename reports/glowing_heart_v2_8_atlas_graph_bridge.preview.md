# Glowing Heart v2.8 Atlas Graph Bridge Preview

## Generation Result

- source index: `reports/glowing_heart_v2_6_difference_packet_index.preview.json`
- generated graph: `Docs/Observatory/Observation_Atlas/Atlas_Graph/glowing_heart_difference_packet_exhibits.graph.json`
- Markdown preview: `reports/atlas_graph_glowing_heart_difference_packet_exhibits.preview.md`
- graph nodes: 6
- exhibit nodes: 5
- edges: 5
- groups: 1
- Atlas validator warnings: 0

## Graph Shape

The gallery root produces five artifact exhibits in source-index order:

1. `comparable_zero`
2. `comparable_nonzero`
3. `unknown_observer_mismatch`
4. `unknown_coordinate_grid_mismatch`
5. `not_comparable_channel`

No edge asserts ranking, scientific correctness, or comparison eligibility beyond the source compatibility decision.

## Generator Guards

Negative tests rejected bad `parityClaim`, `runtimeExecuted=true`, a non-Core comparison mode, missing claim boundaries, and duplicate exhibit IDs. Every failed case returned non-zero and wrote no graph output.

## Schema Decision

Atlas Graph `node.metadata` is optional and accepts domain-specific structured data. Existing example graphs remain valid without metadata.

## Claim Boundary

- Core-vs-Core only.
- Not a Godot comparison.
- Not image or pixel comparison.
- Not parity.
- Not physical validation.
- Not renderer equivalence.
- The bridge maps evidence structure only; it does not validate scientific correctness.

