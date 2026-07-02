# Project Glowing Heart v3.2 Automated Fixture Gallery Index

## What Changed

Glowing Heart now has a fixture-family-first discovery index. Visitors can locate the gallery, evidence map, dashboard, and health view for a fixture/observer/channel group without reading the milestone archive.

The first entry is generated from the v3.0 dashboard seed by `tools/glowing_heart_fixture_gallery_index.py`. It does not create comparisons or derive new measurements.

## Browse the Gallery

| Fixture Family | Observer | Channels | Gallery | Evidence Map | Dashboard | Health |
|---|---|---|---|---|---|---|
| `grin_radial_smoke_family_v1` | Core smoke observer | `bend_magnitude_metric`, `traversal_step_count` | [Difference Packet Gallery](project_glowing_heart_v2_4_difference_packet_gallery.md) | [Evidence Map](project_glowing_heart_v2_9_evidence_map.md) | [Dashboard](project_glowing_heart_v3_1_dashboard_renderer.md) | [Evidence Chain Health](project_glowing_heart_v2_11_evidence_chain_health.md) |

The representative fixture is `Fixtures/grin_radial_smoke.json`. The entry records two `Comparable`, two `Unknown`, one `NotComparable`, and zero `RequiresTransform` exhibits.

## Machine-Readable Contract

- schema: `schemas/glowing_heart/fixture_gallery_index.v0.preview.json`
- generated index: `reports/glowing_heart_v3_2_fixture_gallery_index.preview.json`
- preview table: `reports/glowing_heart_v3_2_fixture_gallery_index.preview.md`
- generator: `tools/glowing_heart_fixture_gallery_index.py`

The contract records fixture family and path, observer basis, channels, artifact links, evidence and dashboard views, status counts, claim boundaries, maturity, and curiosity tier.

Generate the index with:

```bash
python3 tools/glowing_heart_fixture_gallery_index.py
```

## Source Artifacts

The entry is seeded from the v3.0 dashboard data and links:

- v2.4 generated gallery
- v2.9 evidence map
- v2.10 Evidence Map Index
- v2.11 health report
- v3.0 dashboard seed
- v3.1 Markdown and SVG dashboard

All paths use canonical repository casing.

## What This Does Not Demonstrate

- Core-vs-Core recorded artifacts only.
- Not a Godot comparison.
- Not image or pixel comparison.
- Not parity.
- Not physical validation.
- Not renderer equivalence.
- Fixture gallery indexing organizes existing artifacts; it does not establish scientific correctness.

## Next Milestone

Glowing Heart v3.3 can render this machine-readable index as a fixture-gallery landing page while preserving the same artifact and claim boundaries.

