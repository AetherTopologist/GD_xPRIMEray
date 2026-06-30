# Three-Layer Visitor Map (Preview)

**The Atlas is a map of observation systems, not a ranking of technologies.**

A visitor orientation route across xPRIMEray Runtime, Project Glowing Heart, and the Observation Atlas.

Graph ID: `atlas.three_layer_visitor_map.v0_2`

Version: `v0.1`

## In plain terms

Start with the runtime for engine and diagnostics, use Project Glowing Heart for protocol and artifact context, and use the Observation Atlas for the wider observer field guide.

## Claim Boundary

This is a visitor orientation map, not an architecture dependency graph or ranking.

## Diagram

```mermaid
flowchart LR
    xprimeray_runtime["xPRIMEray Runtime"]
    project_glowing_heart["Project Glowing Heart"]
    observation_atlas["Observation Atlas"]
    xprimeray_runtime -->|runtime context| project_glowing_heart
    project_glowing_heart -->|protocol context| observation_atlas
```

## Nodes

| ID | Label | Type | Category | Evidence depth | Description | Claim Boundary |
|---|---|---|---|---|---|---|
| xprimeray_runtime | xPRIMEray Runtime | project | runtime_and_diagnostics | Experimental | Repository territory for curved-ray runtime, observatory output, and diagnostics. | Visitor orientation only; this node does not summarize runtime correctness. |
| project_glowing_heart | Project Glowing Heart | project | protocol_and_artifacts | Experimental | Protocol and artifact trail for fixtures, observers, snapshots, measurements, and comparison eligibility. | Protocol records do not establish renderer or observer equivalence. |
| observation_atlas | Observation Atlas | territory | observer_field_guide | Prototype | Field guide mapping biological, optical, scientific, computational, and educational observers. | The field guide maps relationships without ranking observer families. |

## Edges

| ID | From | To | Relationship | Label | Claim Boundary |
|---|---|---|---|---|---|
| runtime_contextualizes_glowing_heart | xprimeray_runtime | project_glowing_heart | contextualizes | runtime context | The relationship is a reading aid, not an architecture dependency. |
| glowing_heart_contextualizes_atlas | project_glowing_heart | observation_atlas | contextualizes | protocol context | The relationship is a reading aid, not a hierarchy. |

## Evidence

| Label | Reference | Kind |
|---|---|---|
| Repository Home | `Docs/index.md` | repository_doc |
| Visual Wayfinding | `Docs/Observatory/Observation_Atlas/visual_wayfinding.md` | repository_doc |
| Atlas Constitution | `Docs/Observatory/Observation_Atlas/ATLAS_CONSTITUTION.md` | repository_doc |

## Status

This preview describes graph structure only.

No parity claim.
No scientific validation claim.
No proof claim.
