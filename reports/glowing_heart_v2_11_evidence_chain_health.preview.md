# Glowing Heart v2.11 Evidence Chain Health Preview

The health check walks the recorded Difference Packet Index -> Atlas Graph -> Evidence Map -> Gallery -> Evidence Map Index chain.

## PASS Summary

- Checks: 46
- Errors: 0
- Warnings: 0

## Check Groups

| Group | Status | Checks | Errors | Warnings |
|---|---|---:|---:|---:|
| `difference_packet_index` | `PASS` | 10 | 0 | 0 |
| `atlas_graph` | `PASS` | 12 | 0 | 0 |
| `evidence_map_svg` | `PASS` | 5 | 0 | 0 |
| `gallery_markdown` | `PASS` | 4 | 0 | 0 |
| `evidence_map_index` | `PASS` | 12 | 0 | 0 |
| `cross_chain` | `PASS` | 3 | 0 | 0 |

## Exhibit Consistency

| Exhibit | Status | Rule | Consistent |
|---|---|---|---|
| Comparable Non-Zero (`comparable_nonzero`) | `Comparable` | `bend_magnitude_same_observer` | Yes |
| Comparable Zero (`comparable_zero`) | `Comparable` | `bend_magnitude_same_observer` | Yes |
| NotComparable Channel (`not_comparable_channel`) | `NotComparable` | `traversal_steps_vs_bend_not_comparable` | Yes |
| Unknown Coordinate-Grid Mismatch (`unknown_coordinate_grid_mismatch`) | `Unknown` | `bend_magnitude_context_mismatch` | Yes |
| Unknown Observer Mismatch (`unknown_observer_mismatch`) | `Unknown` | `bend_magnitude_context_mismatch` | Yes |

## Warnings and Errors

None.

## Claim Boundary

- Core-vs-Core only.
- Not a Godot comparison.
- Not image or pixel comparison.
- Not parity.
- Not physical validation.
- Not renderer equivalence.
- Health check validates artifact synchronization, not scientific correctness.

## Next Milestone

Glowing Heart v2.12 can add this command to a local preflight or CI-ready workflow.
