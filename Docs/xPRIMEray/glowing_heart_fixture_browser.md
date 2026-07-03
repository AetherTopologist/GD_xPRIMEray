# Glowing Heart Fixture Browser

## Core-runnable GRIN smoke fixture artifacts

**Health: 4 fixtures · 12 attempted resolution runs PASS**  
**Boundary: Core artifacts only · no Godot or image comparison**

Browse the retained fixture family by experimental role and sampling density. The highest published passing tier is surfaced first; manifests and metric tables remain available for deeper inspection.

## Reading Boundary

These pages expose measurements produced by the Core smoke transport fixtures. They do not establish Godot parity, physical validation, renderer equivalence, or proof. Higher resolution means a denser sampling grid, not a different transport model.

## grin_radial_smoke_family_v1

Radial GRIN smoke transport. Observer-facing bend-magnitude snapshots from a radial gradient-index field. The only Core-runnable fixture family at v3.3.

<div class="grid cards" markdown>

-   **`grin_radial_smoke`**

    **Canonical** · `PASS`

    Best passing tier: **Gallery detail 320x176** · 56320 rays

    Channels: `bend_magnitude_metric` · `traversal_step_count`

    [Gallery detail preview](../assets/glowing_heart/v3_4/grin_radial_smoke/standard/snapshot_ascii.txt) · [Run summary](../assets/glowing_heart/v3_4/grin_radial_smoke/standard/run_summary.md)

    Raw data: [Manifest](../assets/glowing_heart/v3_4/grin_radial_smoke/standard/manifest.json) · [Metrics CSV](../assets/glowing_heart/v3_4/grin_radial_smoke/standard/ray_metrics.csv)

    Evidence: [Fixture gallery](project_glowing_heart_v3_3_fixture_library_gallery.md) · [Resolution ladder](project_glowing_heart_v3_4_resolution_ladder.md)

-   **`grin_radial_smoke_variant`**

    **Amplitude sensitivity** · `PASS`

    Best passing tier: **Gallery detail 320x176** · 56320 rays

    Channels: `bend_magnitude_metric` · `traversal_step_count`

    [Gallery detail preview](../assets/glowing_heart/v3_4/grin_radial_smoke_variant/standard/snapshot_ascii.txt) · [Run summary](../assets/glowing_heart/v3_4/grin_radial_smoke_variant/standard/run_summary.md)

    Raw data: [Manifest](../assets/glowing_heart/v3_4/grin_radial_smoke_variant/standard/manifest.json) · [Metrics CSV](../assets/glowing_heart/v3_4/grin_radial_smoke_variant/standard/ray_metrics.csv)

    Evidence: [Fixture gallery](project_glowing_heart_v3_3_fixture_library_gallery.md) · [Resolution ladder](project_glowing_heart_v3_4_resolution_ladder.md)

-   **`grin_radial_smoke_observer_variant`**

    **Observer sensitivity** · `PASS`

    Best passing tier: **Gallery detail 320x176** · 56320 rays

    Channels: `bend_magnitude_metric` · `traversal_step_count`

    [Gallery detail preview](../assets/glowing_heart/v3_4/grin_radial_smoke_observer_variant/standard/snapshot_ascii.txt) · [Run summary](../assets/glowing_heart/v3_4/grin_radial_smoke_observer_variant/standard/run_summary.md)

    Raw data: [Manifest](../assets/glowing_heart/v3_4/grin_radial_smoke_observer_variant/standard/manifest.json) · [Metrics CSV](../assets/glowing_heart/v3_4/grin_radial_smoke_observer_variant/standard/ray_metrics.csv)

    Evidence: [Fixture gallery](project_glowing_heart_v3_3_fixture_library_gallery.md) · [Resolution ladder](project_glowing_heart_v3_4_resolution_ladder.md)

-   **`grin_radial_smoke_resolution_variant`**

    **Grid sensitivity** · `PASS`

    Best passing tier: **Gallery detail 320x176** · 56320 rays

    Channels: `bend_magnitude_metric` · `traversal_step_count`

    [Gallery detail preview](../assets/glowing_heart/v3_4/grin_radial_smoke_resolution_variant/standard/snapshot_ascii.txt) · [Run summary](../assets/glowing_heart/v3_4/grin_radial_smoke_resolution_variant/standard/run_summary.md)

    Raw data: [Manifest](../assets/glowing_heart/v3_4/grin_radial_smoke_resolution_variant/standard/manifest.json) · [Metrics CSV](../assets/glowing_heart/v3_4/grin_radial_smoke_resolution_variant/standard/ray_metrics.csv)

    Evidence: [Fixture gallery](project_glowing_heart_v3_3_fixture_library_gallery.md) · [Resolution ladder](project_glowing_heart_v3_4_resolution_ladder.md)

</div>

## Resolution Ladder

| Fixture | Baseline | Compact | Gallery detail | Extended |
|---|---|---|---|---|
| `grin_radial_smoke` | [40x22 · PASS](../assets/glowing_heart/v3_4/grin_radial_smoke/smoke/snapshot_ascii.txt) | [80x44 · PASS](../assets/glowing_heart/v3_4/grin_radial_smoke/mini/snapshot_ascii.txt) | [320x176 · PASS](../assets/glowing_heart/v3_4/grin_radial_smoke/standard/snapshot_ascii.txt) | Not run in v3.4 (scope stop) |
| `grin_radial_smoke_variant` | [40x22 · PASS](../assets/glowing_heart/v3_4/grin_radial_smoke_variant/smoke/snapshot_ascii.txt) | [80x44 · PASS](../assets/glowing_heart/v3_4/grin_radial_smoke_variant/mini/snapshot_ascii.txt) | [320x176 · PASS](../assets/glowing_heart/v3_4/grin_radial_smoke_variant/standard/snapshot_ascii.txt) | Not run in v3.4 (scope stop) |
| `grin_radial_smoke_observer_variant` | [40x22 · PASS](../assets/glowing_heart/v3_4/grin_radial_smoke_observer_variant/smoke/snapshot_ascii.txt) | [80x44 · PASS](../assets/glowing_heart/v3_4/grin_radial_smoke_observer_variant/mini/snapshot_ascii.txt) | [320x176 · PASS](../assets/glowing_heart/v3_4/grin_radial_smoke_observer_variant/standard/snapshot_ascii.txt) | Not run in v3.4 (scope stop) |
| `grin_radial_smoke_resolution_variant` | [41x22 · PASS](../assets/glowing_heart/v3_4/grin_radial_smoke_resolution_variant/smoke/snapshot_ascii.txt) | [80x44 · PASS](../assets/glowing_heart/v3_4/grin_radial_smoke_resolution_variant/mini/snapshot_ascii.txt) | [320x176 · PASS](../assets/glowing_heart/v3_4/grin_radial_smoke_resolution_variant/standard/snapshot_ascii.txt) | Not run in v3.4 (scope stop) |

Extended is a policy deferral, not a failed run. It was not run in v3.4 because Gallery detail was the declared stopping point.

## Downloads and Developer Sources

- [Fixture library index JSON](https://github.com/xPRIMEray/GD_xPRIMEray/blob/main/reports/glowing_heart_v3_3_fixture_library_index.preview.json)
- [Resolution ladder JSON](https://github.com/xPRIMEray/GD_xPRIMEray/blob/main/reports/glowing_heart_v3_4_resolution_ladder.preview.json)
- [Fixture library schema](https://github.com/xPRIMEray/GD_xPRIMEray/blob/main/schemas/glowing_heart/fixture_library_index.v0.preview.json)
- [Resolution ladder schema](https://github.com/xPRIMEray/GD_xPRIMEray/blob/main/schemas/glowing_heart/fixture_resolution_ladder.v0.preview.json)
- Each fixture card links its publishable manifest and metrics CSV.

## Claim Boundary

- Core fixture artifact browsing only.
- No Godot comparison or Godot runtime execution.
- No image or pixel comparison.
- No parity claim.
- No physical validation, renderer equivalence, or proof.
- PASS describes the recorded fixture run checks only.
