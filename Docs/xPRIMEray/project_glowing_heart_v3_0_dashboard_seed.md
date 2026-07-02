# Project Glowing Heart v3.0 Observer Fixture Dashboard Seed

## What Changed

Glowing Heart now has a machine-readable dashboard seed that organizes the frozen v2.x evidence set by observer basis, fixture family, channels, and exhibits. The first group wraps all five existing Difference Packet exhibits without changing their status, rules, metrics, or claim boundaries.

`tools/glowing_heart_dashboard_seed.py` derives the seed from the Difference Packet Index and cross-checks it against the Evidence Map Index and Atlas Graph before writing JSON atomically.

## What This Demonstrates

The existing evidence chain can supply a grouped dashboard data model without parsing generated Markdown or inventing comparison results. The model separates group identity and source provenance from exhibit-level comparison decisions.

## What This Does Not Demonstrate

- Core-vs-Core only.
- Not a Godot comparison.
- Not image or pixel comparison.
- Not parity.
- Not physical validation.
- Not renderer equivalence.
- Dashboard seed organizes recorded evidence only; it does not validate scientific correctness.

## Dashboard Seed Schema

The preview schema is `schemas/glowing_heart/dashboard_seed.v0.preview.json`. It requires fixed top-level claim guards, one or more dashboard groups, canonical source paths, status counts, and complete exhibit references.

Allowed statuses remain `Comparable`, `Unknown`, `NotComparable`, and `RequiresTransform`. The fixed comparison-stage fields are:

```text
comparisonMode = core_vs_core
parityClaim = NONE
runtimeExecuted = false
```

Here, `runtimeExecuted=false` describes dashboard assembly. It is not a historical statement about the Core runs that produced retained artifacts.

## First Dashboard Group

| Field | Value |
|---|---|
| Group ID | `core_smoke_observer_grin_radial_smoke_family_v1` |
| Observer basis | Core smoke observer |
| Fixture family | `grin_radial_smoke_family_v1` |
| Fixture paths | 4 |
| Channels | `bend_magnitude_metric`, `traversal_step_count` |
| Exhibits | 5 |
| Status counts | Comparable 2, Unknown 2, NotComparable 1, RequiresTransform 0 |

The group contains Comparable Zero, Comparable Non-Zero, Unknown Observer Mismatch, Unknown Coordinate-Grid Mismatch, and NotComparable Channel.

## How v2.x Feeds v3.0

The v2.6 Difference Packet Index supplies exhibit identity, fixture paths, channels, status, rule, metrics, and boundaries. The v2.8 Atlas Graph supplies a second structured representation of status, rule, and metrics. The v2.10 Evidence Map Index supplies the discovery identity and claim-boundary cross-check.

The v2.9 Evidence Map and v2.4 Gallery remain source-linked generated views. Metrics are copied from the structured Difference Packet Index, not inferred from those views.

Generate the seed with:

```bash
python3 tools/glowing_heart_dashboard_seed.py
```

## Adding Future Groups

A future group should introduce a unique group ID, explicit observer basis, fixture-family identity, canonical fixture paths, declared channels, and source artifact paths. Every referenced exhibit must already have structured status, rule, metrics, comparison guards, and a non-empty claim boundary.

Adding a group does not make its channels comparable. Eligibility remains a property of declared comparison rules and retained evidence, and unsupported combinations should remain `Unknown`, `NotComparable`, or `RequiresTransform` as appropriate.

## Next Milestone

**Glowing Heart v3.1 — Dashboard Seed Renderer** can render the v3.0 seed as compact Markdown and SVG dashboard views without adding new comparison behavior.

