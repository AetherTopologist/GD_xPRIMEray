# Project Glowing Heart v2.4 Difference Packet Gallery

## How to Read This Gallery

- `Comparable` means the declared channel, fixture comparison identity, observer basis, and coordinate grid passed the named rule checks.
- `Unknown` means the system refused to compare values because eligibility was not established.
- `NotComparable` is reserved for channel pairs declared incompatible by the compatibility matrix.
- `Deferred` means the correct retained artifact does not exist yet; no current gallery entry is deferred after v2.5.

These statuses describe comparison eligibility. They do not assess scientific correctness.

## Comparable Zero

| Field | Value |
|---|---|
| Case | A |
| Left fixture | `Fixtures/grin_radial_smoke.json` |
| Right fixture | `Fixtures/grin_radial_smoke.json` |
| Requested channel | `bend_magnitude_metric` |
| Status | `Comparable` |
| Rule | `bend_magnitude_same_observer` |
| Compared | 880 |
| Maximum difference | 0 |
| Mean difference | 0 |
| Non-zero count | 0 |

**What this shows:** Two deterministic retained Core artifacts from the same fixture have identical values under this comparison.

**What this does not show:** Zero difference does not establish equivalence with another runtime or measurement system.

## Comparable Non-Zero

| Field | Value |
|---|---|
| Case | B |
| Left fixture | `Fixtures/grin_radial_smoke.json` |
| Right fixture | `Fixtures/grin_radial_smoke_variant.json` |
| Requested channel | `bend_magnitude_metric` |
| Status | `Comparable` |
| Rule | `bend_magnitude_same_observer` |
| Compared | 880 |
| Maximum difference | 0.00031490779 |
| Mean difference | 0.00019547191841352225 |
| Non-zero count | 872 |

**What this shows:** The amplitude variant produces a numeric distinction between declared-compatible retained Core artifacts.

**What this does not show:** The numeric distinction does not establish physical correctness or agreement with another runtime.

## Unknown Observer Mismatch

| Field | Value |
|---|---|
| Case | C |
| Left fixture | `Fixtures/grin_radial_smoke.json` |
| Right fixture | `Fixtures/grin_radial_smoke_observer_variant.json` |
| Requested channel | `bend_magnitude_metric` |
| Status | `Unknown` |
| Rule | `bend_magnitude_context_mismatch` |
| Compared | 0 |
| Maximum difference | 0 |
| Mean difference | 0 |
| Non-zero count | 0 |

**What this shows:** The observer-basis mismatch stops eligibility before value comparison.

**What this does not show:** Zero compared values are not evidence that the retained values agree or disagree.

## Unknown Coordinate-Grid Mismatch

| Field | Value |
|---|---|
| Case | D |
| Left fixture | `Fixtures/grin_radial_smoke.json` |
| Right fixture | `Fixtures/grin_radial_smoke_resolution_variant.json` |
| Requested channel | `bend_magnitude_metric` |
| Status | `Unknown` |
| Rule | `bend_magnitude_context_mismatch` |
| Compared | 0 |
| Maximum difference | 0 |
| Mean difference | 0 |
| Non-zero count | 0 |

**What this shows:** The coordinate-grid mismatch stops eligibility before value comparison.

**What this does not show:** The packet does not resample, transform, or compare values across grids.

## NotComparable Incompatible Channel

| Field | Value |
|---|---|
| Case | E |
| Left fixture | `Fixtures/grin_radial_smoke.json` |
| Right fixture | `Fixtures/grin_radial_smoke.json` |
| Requested channel | left `bend_magnitude_metric`; right `traversal_step_count` |
| Status | `NotComparable` |
| Rule | `traversal_steps_vs_bend_not_comparable` |
| Compared | 0 |
| Maximum difference | 0 |
| Mean difference | 0 |
| Non-zero count | 0 |

**What this shows:** The matrix rejects two authentic retained Core quantities that are declared incompatible without a transform.

**What this does not show:** No values were compared, and bend data was not relabeled as traversal steps.

## Claim Boundary

- Core-vs-Core only.
- Not a Godot comparison.
- Not image or pixel comparison.
- Not parity.
- Not physical validation.
- Not renderer equivalence.
- Non-zero difference demonstrates numeric distinction between retained Core artifacts only.
- Zero difference between deterministic Core packets does not establish equivalence with another runtime or measurement system.

The source measurements and verification paths are recorded in `reports/glowing_heart_v2_3_difference_fixture_cases.preview.md`.

## Next Milestone

Glowing Heart v2.6 can create a machine-readable index of the v2.3-v2.5 packet exhibits for future Gallery and Atlas tooling.
