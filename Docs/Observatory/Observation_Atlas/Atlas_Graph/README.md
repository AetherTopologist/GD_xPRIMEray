# Atlas Graph

Atlas Graph is the data layer behind future Atlas diagrams and educational fixtures.
It is not a ranking system.
It is not a claim engine.
It is a structured map of observer concepts, relationships, and claim boundaries.

## Atlas Graph and Observation Atlas

The **Observation Atlas** is the human-readable field guide. **Atlas Graph** is a structured data layer that can supply diagrams, reference tables, and future educational adapters for that field guide. Graph records support multiple presentations without making the presentation itself the source of graph meaning.

## Who this is for

- Atlas authors maintaining concepts and relationships
- documentation contributors generating consistent previews
- future SVG, site, and educational-fixture adapter authors
- reviewers checking references, evidence depth, and claim boundaries

## Contract Spine

| Resource | Purpose |
|---|---|
| `schemas/atlas_graph/atlas_graph_schema.v0.preview.json` | Preview JSON Schema for graphs, nodes, edges, groups, evidence, and render hints |
| [Observer Journey graph](example_observer_journey.graph.json) | Navigation-oriented example across observer families |
| [Observer Grammar graph](example_observer_grammar.graph.json) | Shared descriptive sequence from phenomenon to claim boundary |
| [Three-Layer Visitor Map](example_three_layer_visitor_map.graph.json) | Visitor orientation across runtime, protocol, and field guide |
| [v0.1 notes](atlas_graph_v0_1.md) | Scope, workflow, and extension boundary |

## Tooling

```bash
python3 tools/atlas_graph_validate.py Docs/Observatory/Observation_Atlas/Atlas_Graph/example_observer_journey.graph.json
python3 tools/atlas_graph_markdown.py Docs/Observatory/Observation_Atlas/Atlas_Graph/example_observer_journey.graph.json reports/atlas_graph_observer_journey.preview.md
```

The validator checks contract structure and graph references. The Markdown tool produces a descriptive preview and Mermaid flowchart.

Atlas Graph validates structure only, not scientific correctness or observer equivalence.

## Governance

Atlas Graph does not replace Glowing Heart evidence/run schemas. Glowing Heart remains responsible for fixture, observer, snapshot, measurement, difference, and artifact evidence contracts. Atlas Graph may reference that evidence, but it does not strengthen or reinterpret its claims.

## Claim Boundary

No parity claim.
No scientific validation claim.
No proof claim.
