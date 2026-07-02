# Project Glowing Heart v3.1 Dashboard Seed Renderer

## What Changed

Glowing Heart can now render the v3.0 dashboard seed as a visitor-readable Markdown report and a compact SVG dashboard. The renderer presents one observer/fixture group and five evidence cards while preserving source status, rules, metrics, channels, and claim boundaries.

The implementation uses repository-local inputs and the Python standard library. When `jsonschema` is installed, the seed is checked against its preview schema before rendering; fixed claim guards and dashboard structure are always checked directly.

## Dashboard View

The dashboard header shows:

- Core smoke observer basis
- `grin_radial_smoke_family_v1` fixture family
- `bend_magnitude_metric` and `traversal_step_count` channels
- status counts: Comparable 2, Unknown 2, NotComparable 1, RequiresTransform 0

Five fixed cards show the exhibit title, seed status, rule, compared and non-zero counts, maximum and mean differences, channels, and a visible claim-boundary indicator. The Markdown output records every exhibit claim boundary in full.

## Source Discipline

Status and metric values come only from `reports/glowing_heart_v3_0_dashboard_seed.preview.json`. The renderer does not infer status from color, labels, claim text, or card position.

The seed schema is `schemas/glowing_heart/dashboard_seed.v0.preview.json`. Invalid schema data, unsupported status, mismatched status counts, missing metrics, duplicate exhibit IDs, or missing boundaries fail before output rendering.

## Command

```bash
python3 tools/glowing_heart_dashboard_renderer.py \
  reports/glowing_heart_v3_0_dashboard_seed.preview.json \
  schemas/glowing_heart/dashboard_seed.v0.preview.json \
  reports/glowing_heart_v3_1_dashboard.preview.md \
  reports/glowing_heart_v3_1_dashboard.svg
```

## Outputs

- Markdown dashboard: `reports/glowing_heart_v3_1_dashboard.preview.md`
- SVG dashboard: `reports/glowing_heart_v3_1_dashboard.svg`

Both files are deterministic generated views and should be regenerated rather than hand-edited for exhibit values.

## SVG Safety

The SVG is self-contained XML with a `0 0 1400 900` view box. It contains one dashboard-group element, five exhibit groups, no script elements, and no external links.

## What This Does Not Demonstrate

- Core-vs-Core only.
- Not a Godot comparison.
- Not image or pixel comparison.
- Not parity.
- Not physical validation.
- Not renderer equivalence.
- Rendering recorded dashboard evidence does not establish scientific correctness.

## Next Milestone

Glowing Heart v3.2 can extend dashboard health checks so the seed and both generated dashboard views remain synchronized without adding new comparison behavior.

