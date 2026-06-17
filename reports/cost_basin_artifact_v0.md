# Cost Basin Artifact v0

**Question:** Where does computational effort accumulate?

**Status:** Observed artifact. Reporting layer only. No renderer optimization.

## Outputs

- Heatmap: `reports/cost_basin_heatmap.png`
- Ladder: `reports/cost_basin_ladder.png`
- Explanation: `reports/cost_basin_artifact_v0.md`

## Inputs

- Hit diagnostics: `output/curvature_fps_benchmark/20260607T221311Z/cells/curvature_000/row/hermetic_curved_room__curvature_fps_0__baseline_prune_off__scheduler-baseline__targetms-1000__stride-1__runid-1.hit_diagnostics.csv`
- Traversal heatmap: `output/curvature_fps_benchmark/20260607T221311Z/cells/curvature_000/row/traversal_step_heatmap.png`
- Query Observatory metrics: `output/curvature_fps_benchmark/20260607T221311Z/cells/curvature_000/row/curvature_fps_result.json`

## Method

Cost Basin v0 uses `final_step_count` as the measured spatial effort field. When per-pixel `query_count` or `substep_count` are not present in `hit_diagnostics.csv`, v0 derives observation-only attribution from the aggregate Query Observatory metrics in `latest_perf_frame_report`:

- `query_count` is estimated spatially in proportion to each pixel's share of total `final_step_count`.
- `substep_count` is represented by the aggregate ratio `subdivided_ray_queries / segments`.
- `pass2_query_ms` is reported as aggregate context, not assigned as a per-pixel timer.

This makes the heatmap a cost-observation artifact, not a scheduling or optimization signal.

## Reading The Heatmap

Bright yellow/white regions are the local Cost Basin: pixels where computational effort accumulates relative to the rest of the same frame. Blue/green regions are lower-effort portions of the same scene contract.

For the base cell `0%`, the basin is mostly a traversal-depth basin: mean `final_step_count` is 273.2, max is 299. Query work dominates the physics phase (93.8% of `pass2_phys_ms`), so the observed traversal field also predicts where query effort accumulates.

## Cost Basin Ladder

| cell | final_step_count mean | final_step_count max | query_count total | substep_count mean | query cost % |
|---|---:|---:|---:|---:|---:|
| 0% | 273.2 | 299 | 7,917,798 | 2.49 | 93.8% |
| 25% | 273.5 | 299 | 6,639,439 | 2.08 | 92.7% |
| 50% | 274.5 | 300 | 6,696,700 | 2.08 | 92.4% |
| 75% | 276.1 | 302 | 6,749,914 | 2.08 | 92.4% |
| 100% | 278.4 | 304 | 6,817,023 | 2.08 | 92.5% |

## Interpretation

The ladder asks whether the basin shifts as curvature changes. In this hermetic curved-room run, closure remains complete while effort stays concentrated in the same broad traversal-depth structure. Curvature changes the depth and fine shape of the basin, but the artifact does not claim physical correctness.

## Verdict

**Cost Basin Artifact v0: OBSERVED.**

The artifact answers where computational effort accumulates for the selected run using existing diagnostics only. It does not optimize renderer behavior, alter scheduling, or feed runtime decisions.
