# Project Glowing Heart v2.9 Atlas Graph Rendered Evidence Map

## What Changed

`tools/atlas_graph_evidence_map_renderer.py` now renders the hardened v2.8.1 Atlas Graph as a deterministic 1400×900 SVG evidence map and a Markdown preview.

The map presents one gallery root and five exhibit cards. Each card reads its status, rule, and metrics from node metadata and displays a claim-boundary indicator. Edges are limited to the five recorded root-to-exhibit relationships.

## What This Demonstrates

Atlas Graph structure and metadata can drive a compact technical visualization without parsing claim text or inventing exhibit values. The rendered topology makes the five recorded comparison decisions discoverable as evidence artifacts.

## What This Does Not Demonstrate

- Core-vs-Core only.
- Not a Godot comparison.
- Not image or pixel comparison.
- Not parity.
- Not physical validation.
- Not renderer equivalence.
- The evidence map renders recorded comparison decisions only; it does not validate scientific correctness.

## Renderer Command

```bash
python3 tools/atlas_graph_evidence_map_renderer.py \
  Docs/Observatory/Observation_Atlas/Atlas_Graph/glowing_heart_difference_packet_exhibits.graph.json \
  reports/glowing_heart_v2_9_evidence_map.svg \
  reports/glowing_heart_v2_9_evidence_map.preview.md
```

## Source and Outputs

- source graph: `Docs/Observatory/Observation_Atlas/Atlas_Graph/glowing_heart_difference_packet_exhibits.graph.json`
- SVG map: `reports/glowing_heart_v2_9_evidence_map.svg`
- Markdown preview: `reports/glowing_heart_v2_9_evidence_map.preview.md`

The renderer uses only the Python standard library and existing Atlas validation logic. The SVG contains no scripts or external resources.

## Visual Rules

- exactly one project root and five artifact exhibits
- exactly one root-to-exhibit edge per exhibit
- no inter-exhibit edges
- green for `Comparable`, amber for `Unknown`, and muted red for `NotComparable`
- status, rule, and metric text sourced from metadata
- visible Claim Boundary badge on every exhibit
- light-background technical-report styling

Status is never inferred from boundary text, topology, color, or edge direction.

## Failure Modes

- invalid Atlas Graph structure: fail before rendering
- unexpected root or exhibit count: fail with the observed count
- inter-exhibit or non-root edge: fail with the edge position and ID
- missing status, rule, metric, or claim boundary: fail with the exhibit position and ID
- unsupported status: fail before output
- overlapping input/output paths or output I/O error: return non-zero

Validation occurs before output writes, so rejected graph inputs do not leave rendered files.

## Next Milestone

Glowing Heart v2.10 can add the evidence-map paths back into the structured index and graph provenance so the chain is discoverable from Difference Packet Index through Atlas Graph, Evidence Map, and Gallery.

