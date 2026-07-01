# Project Glowing Heart v2.6 Difference Packet Index

## What Changed

The five verified Difference Packet Gallery cases now have a machine-readable exhibit index:

- schema: `schemas/glowing_heart/difference_packet_index.v0.preview.json`
- data: `reports/glowing_heart_v2_6_difference_packet_index.preview.json`
- preview: `reports/glowing_heart_v2_6_difference_packet_index.preview.md`

Each entry records fixture and channel identities, status, compatibility rule, metrics, claim boundaries, durable source paths, optional latest verification output, and recommended renderer forms.

## What This Demonstrates

Gallery exhibits can be represented as stable structured records instead of requiring future tools to parse Markdown. The index preserves the decision vocabulary across two `Comparable`, two `Unknown`, and one `NotComparable` exhibit.

## What This Does Not Demonstrate

- Core-vs-Core only.
- Not a Godot comparison.
- Not image or pixel comparison.
- Not parity.
- Not physical validation.
- Not renderer equivalence.
- Index validity describes exhibit structure, not scientific correctness.

## Runtime Terminology

Core transport may execute to produce retained artifacts. Difference Packet `runtimeExecuted=false` means the comparison stage did not execute a runtime or Godot; it evaluated already-retained Core artifacts. This field does not imply that the source artifacts appeared without Core execution.

## Schema Overview

The top level requires an index ID, title, version, generation timestamp, entries, and a shared claim boundary. Every entry requires:

- stable exhibit identity and source case
- one allowed Difference Packet status
- named compatibility rule and reason
- left and right fixture and channel paths
- fixed `comparisonMode=core_vs_core`
- fixed `parityClaim=NONE`
- fixed `runtimeExecuted=false`
- Difference Packet schema identity
- complete zero-or-positive metrics
- entry-specific claim boundaries
- gallery and source-report paths
- one or more renderer recommendations

`lastVerifiedOutputPath` is optional and explicitly non-durable. Stable fixture, gallery, and report references remain repository paths.

## Exhibit List

| Exhibit | Case | Status | Meaning |
|---|---|---|---|
| Comparable Zero | A | `Comparable` | Eligible deterministic inputs had zero numeric difference. |
| Comparable Non-Zero | B | `Comparable` | Eligible retained Core artifacts contained numeric differences. |
| Unknown Observer Mismatch | C | `Unknown` | Observer eligibility failed before value comparison. |
| Unknown Coordinate-Grid Mismatch | D | `Unknown` | Grid eligibility failed before value comparison. |
| NotComparable Channel | E | `NotComparable` | Bend magnitude and traversal-step channels are declared incompatible without a transform. |

## Future Atlas and Gallery Use

Atlas Graph tooling can map each exhibit to an `artifact` or `measurement` node while retaining its claim boundary. Gallery tooling can select `table`, `card`, or `matrix` from `recommendedRenderer` and render metrics directly. Both can link to durable evidence reports without treating temporary verification paths as sources.

## Next Milestone

Glowing Heart v2.7 can generate the Markdown gallery from the v2.6 JSON index, making the visitor-readable gallery a deterministic rendering of the structured exhibit data.

