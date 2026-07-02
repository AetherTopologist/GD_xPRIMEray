# Glowing Heart v3.0 Observer Fixture Dashboard Seed Preview

The dashboard seed groups the frozen v2.x five-exhibit set by observer basis, fixture family, and measurement channels. It is structured input for future generated dashboard views.

## Dashboard Groups

| Group | Observer basis | Fixture family | Fixtures | Channels | Exhibits |
|---|---|---|---:|---:|---:|
| Core Smoke Observer / GRIN Radial Smoke Family v1 | Core smoke observer | `grin_radial_smoke_family_v1` | 4 | 2 | 5 |

Status counts: `Comparable` 2, `Unknown` 2, `NotComparable` 1, `RequiresTransform` 0.

## Exhibits

| Exhibit | Status | Rule | Channels | Compared | Maximum difference | Mean difference | Non-zero |
|---|---|---|---|---:|---:|---:|---:|
| Comparable Zero | `Comparable` | `bend_magnitude_same_observer` | `bend_magnitude_metric` / `bend_magnitude_metric` | 880 | 0 | 0 | 0 |
| Comparable Non-Zero | `Comparable` | `bend_magnitude_same_observer` | `bend_magnitude_metric` / `bend_magnitude_metric` | 880 | 0.00031490779 | 0.00019547191841352225 | 872 |
| Unknown Observer Mismatch | `Unknown` | `bend_magnitude_context_mismatch` | `bend_magnitude_metric` / `bend_magnitude_metric` | 0 | 0 | 0 | 0 |
| Unknown Coordinate-Grid Mismatch | `Unknown` | `bend_magnitude_context_mismatch` | `bend_magnitude_metric` / `bend_magnitude_metric` | 0 | 0 | 0 | 0 |
| NotComparable Channel | `NotComparable` | `traversal_steps_vs_bend_not_comparable` | `bend_magnitude_metric` / `traversal_step_count` | 0 | 0 | 0 | 0 |

## Source Artifact Chain

| Role | Path |
|---|---|
| Difference Packet Index | `reports/glowing_heart_v2_6_difference_packet_index.preview.json` |
| Atlas Graph | `Docs/Observatory/Observation_Atlas/Atlas_Graph/glowing_heart_difference_packet_exhibits.graph.json` |
| Evidence Map | `reports/glowing_heart_v2_9_evidence_map.svg` |
| Gallery | `Docs/xPRIMEray/project_glowing_heart_v2_4_difference_packet_gallery.md` |
| Evidence Map Index guard | `reports/glowing_heart_v2_10_evidence_map_index.preview.json` |

## Claim Boundary

- Core-vs-Core only.
- Not a Godot comparison.
- Not image or pixel comparison.
- Not parity.
- Not physical validation.
- Not renderer equivalence.
- Dashboard seed organizes recorded evidence only; it does not validate scientific correctness.

## Future Dashboard Directions

Future groups can add observer, fixture-family, and channel combinations only when backed by structured source exhibits with explicit comparison eligibility, metrics, provenance, and claim boundaries. The next milestone can render this seed without adding comparison behavior.

