# Glowing Heart v2.3 Difference Fixture Cases

Regenerated from canonical `Fixtures/` definitions on 2026-07-01 UTC. Paths identify the fresh verification artifacts used for this preview.

| Case | Left packet | Right packet | Requested channel | Status | Rule | Compared | Max difference | Mean difference | Non-zero | Reason |
|---|---|---|---|---|---|---:|---:|---:|---:|---|
| A: deterministic zero | `/tmp/glowing_heart_v2_3_1/base_left/20260701T023726Z_grin_radial_smoke` | `/tmp/glowing_heart_v2_3_1/base_right/20260701T023726Z_grin_radial_smoke` | `bend_magnitude_metric` | `Comparable` | `bend_magnitude_same_observer` | 880 | 0 | 0 | 0 | Matching retained Core fixture comparison identity, observer basis, and coordinate grid. |
| B: deliberate non-zero | `/tmp/glowing_heart_v2_3_1/base_left/20260701T023726Z_grin_radial_smoke` | `/tmp/glowing_heart_v2_3_1/field_variant/20260701T023727Z_grin_radial_smoke_variant` | `bend_magnitude_metric` | `Comparable` | `bend_magnitude_same_observer` | 880 | 0.00031490779 | 0.00019547191841352225 | 872 | Distinct declared Core fixtures share `grin_radial_smoke_family_v1`; the variant changes field amplitude from 0.25 to 0.3. |
| C: observer mismatch | `/tmp/glowing_heart_v2_3_1/base_left/20260701T023726Z_grin_radial_smoke` | `/tmp/glowing_heart_v2_3_1/observer_variant/20260701T023727Z_grin_radial_smoke_observer_variant` | `bend_magnitude_metric` | `Unknown` | `bend_magnitude_context_mismatch` | 0 | 0 | 0 | 0 | The retained observer basis mismatch makes comparison eligibility unknown; values were not compared. |
| D: coordinate-grid mismatch | `/tmp/glowing_heart_v2_3_1/base_left/20260701T023726Z_grin_radial_smoke` | `/tmp/glowing_heart_v2_3_1/grid_variant/20260701T023728Z_grin_radial_smoke_resolution_variant` | `bend_magnitude_metric` | `Unknown` | `bend_magnitude_context_mismatch` | 0 | 0 | 0 | 0 | The retained coordinate-grid mismatch makes comparison eligibility unknown; values were not compared. |
| E: incompatible channel | Not generated | Not generated | Not applicable | Deferred | Not applicable | 0 | 0 | 0 | 0 | No authentic retained `traversal_step_count` scalar-grid artifact exists; bend data was not relabeled to manufacture this case. |

## Comparison outputs

- Case A: `/tmp/glowing_heart_v2_3_1/case_a_zero`
- Case B: `/tmp/glowing_heart_v2_3_1/case_b_nonzero`
- Case C: `/tmp/glowing_heart_v2_3_1/case_c_observer`
- Case D: `/tmp/glowing_heart_v2_3_1/case_d_grid`

## Claim boundary

- Core-vs-Core only.
- Not a Godot comparison.
- Not image or pixel comparison.
- Not parity.
- Not physical validation.
- Not renderer equivalence.
- Non-zero difference demonstrates numeric distinction between retained Core artifacts only.
- Zero difference between deterministic Core packets does not establish equivalence with another runtime or measurement system.
