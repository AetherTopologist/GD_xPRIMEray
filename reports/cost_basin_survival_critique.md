# Cost Basin Survival Critique — hermetic_curved_room mini

**Status:** Adversarial review. Observation and experiment design only. No renderer changes, no new instrumentation, no optimization.

**Premise:** Treat [cost_basin_artifact_v1.md](cost_basin_artifact_v1.md) as guilty until innocent. The alignment table (463 ownership seams / 0 closure / 0 coverage / 0 disagreement) is not evidence of explanatory power until alternative hypotheses are ruled out.

**Input run:** `output/curvature_fps_benchmark/20260607T221311Z`, cell `curvature_000/row` (0% curvature, mini 160×112, full coverage).

**Guardrail:** Hermetic closure validates transport completion within a known scene contract. Cost Basin observes where effort accumulates; it does not claim physical correctness and must not feed runtime decisions.

---

## 1. Assumption Ledger

What Cost Basin v1 claims versus what the data and pipeline actually measure.

| Claim (stated or implied) | What is actually measured | Gap |
|---|---|---|
| "Where computational effort accumulates" | Per-pixel `final_step_count` from `hit_diagnostics.csv`, plus a **proportional estimate** of query count (`step / step_sum × aggregate query total`) | Query dominates wall time (93.8% of `pass2_phys_ms`) but is **not** measured per pixel. Spatial field is step-count terrain with a derived scalar bump. |
| "Basin ridges co-locate with diagnostic evidence" | Ridge pixels (top ~9% of cost values OR gradient ≥ 3.0) within Chebyshev distance 1 of PNG-derived masks | Co-location count, not rate, p-value, or causal link. Code states: *"Alignment means co-location in existing diagnostic outputs, not causal proof."* |
| "Terrain exposes basin/ridge structure" | Hillshade + contours on a field with ~31-step dynamic range (268–299) | Mean 273.2, max 299 (~11% spread). Modes cluster at 268–275. Terrain may overfit shallow variation. |
| "Query effort accumulates where traversal is deep" | Assumed from aggregate Query Observatory metrics | Ladder shows query totals **decrease** from 0% to 25% curvature while mean steps rise. Global query–step coupling is weak or inverted across cells. |
| "High-cost regions predict closure fragility" | Not testable on this run | 0% budget stress, 0 misses, uniform green hit map. No closure exterior exists in the evaluated domain. |
| Maturity implication (Observed → Confirmed) | Artifact generated from real run data | Generation ≠ validation. Alignment table is not a pass/fail gate in [observatory_maturity_ladder.md](observatory_maturity_ladder.md). |

**Pipeline reference:** [tools/generate_cost_basin_artifact.py](../tools/generate_cost_basin_artifact.py)

- `ridge_mask`: percentile ≥ 91 or `|∇cost| ≥ 3.0`
- `image_mask(..., "edge")` for closure/coverage: luminance discontinuity; comment notes uniform maps produce little or no boundary
- `image_mask(..., "active")` for seams/disagreement: chroma > 18, brightness > 70
- `mask_alignment`: ridge pixel within ±1 of any mask pixel

---

## 2. Alternative Explanations for 463 / 0 / 0 / 0

### 2.1 — 463 ownership seam alignment

The headline number. Six alternative explanations that do **not** require Cost Basin to be explanatory.

**A1. Base-rate collision (null hypothesis)**

The hermetic six-receiver chamber projects long, dense ownership seams across the film ([transport_ownership_graph_extractor.py](../tools/transport_ownership_graph_extractor.py) marks every pixel adjacent to a label transition in yellow). Ridge mask marks roughly the top 9% of 17,920 pixels (~1,600 ridge pixels). With ±1 dilation on both masks, hundreds of incidental overlaps are expected even under random ridge placement.

**463 without a null baseline is uninterpretable.** It is a raw count, not evidence that cost structure explains seam topology.

**A2. Shared geometric parent (confound, not mechanism)**

Both ownership seams and `final_step_count` are downstream of the same parent variable: **which receiver a ray hits and how long the path is**. Seams mark receiver-domain boundaries on the film plane. Pixels on either side of a seam often hit different walls at different path lengths. Alignment may reflect **receiver projection geometry** — the fixture is a territory map — not cost driving topology.

**A3. Row-traversal banding (scheduler artifact)**

All eight reported local maxima in [cost_basin_artifact_v1.md](cost_basin_artifact_v1.md) sit on **row y = 96** at x = 1, 9, 17, 25, 33, 41, 49, 57 — periodic spacing of 8 pixels. That pattern is consistent with **row-traversal band scheduling**, not transport difficulty. If ridges track band boundaries, Cost Basin terrain is partially a **render-schedule topography**, not a transport-cost topography.

**A4. Structurally guaranteed seams (tautology risk)**

The hermetic fixture is designed to have six ownership domains at all curvature levels. Seams exist at 0% with field off. Reporting ridge↔seam co-location on a fixture **built to have seams everywhere** is close to tautological. The interesting question is whether overlap **exceeds** what seam density alone predicts.

**A5. Proxy mismatch (step ridges ≠ query basin)**

[renderer_observatory.md](renderer_observatory.md) identifies `pass2_query_ms` as the bottleneck. Cost Basin v1 spatial field is step-weighted. Seam sensitivity in [xeno_zeno_citation_atlas.md](../Docs/xeno_zeno_citation_atlas.md) (X-S Seam Xeno) is an **ownership-classification** phenomenon. Aligning step ridges with seams does not validate the story that query effort forms coherent spatial basins.

**A6. PNG rescaling bleed**

Seam and disagreement masks are extracted after bilinear resize to film resolution. Sub-pixel seam lines fatten by 1–2 pixels, inflating alignment counts without true physical co-location.

**What would survive this attack:**

- Seam alignment significantly above null after controlling for seam pixel count and ridge count (permutation or analytic expectation).
- Normalized alignment rate **rises** with curvature (50–100%: max steps 300–304) while seam geometry is stable — overlap tracking cost gradient, not fixed seam length.
- `final_step_count` gradient correlates with receiver-label transitions in CSV **independently** of PNG chroma thresholding.

---

### 2.2 — 0 closure alignment

**B1. Vacuous by construction**

Mini full-coverage run: `miss_count = 0`, hit map uniformly green ([weekend_fps_curvature_sweep.md](weekend_fps_curvature_sweep.md)). Edge detection on a uniform field yields ~0 closure boundaries. Zero is the **expected outcome of success**, not a Cost Basin finding.

**B2. Wrong layer for this fixture**

[basin_atlas_v1.md](../Docs/basin_atlas_v1.md) separates closure basin (binary in/out) from cost basin (scalar depth): *"The cost basin boundary is not the closure basin boundary."* Testing ridge↔closure-edge alignment on a **fully closed interior** tests a relationship the concept architecture says should not hold at the margin.

**B3. Closure edges exist only in failure regimes**

Meaningful closure boundaries appear at the budget cliff (e.g., budget = 32 → 0% closure in [hermetic-hit-closure validation](../misterylabs_artifacts/validation/hermetic-hit-closure.md)). The happy-path mini run removes the phenomenon under test.

**Verdict:** 0 closure alignment **neither confirms nor refutes** Cost Basin. It is uninformative on this fixture.

---

### 2.3 — 0 coverage alignment

**C1. Full-coverage tautology**

Experiment B reached 100% traced and 100% beauty-written ([curvature_full_coverage_experiment.md](curvature_full_coverage_experiment.md)). `frame_coverage_map` is uniformly filled — no coverage front. Edge mask is empty by the same mechanism as closure.

**C2. Temporal confound in partial runs**

In partial runs (baseline 37.7% traced), uncovered pixels lack `final_step_count`. Cost Basin v1 maps only evaluated pixels. Future ridge↔coverage tests must define whether cost is **per-visit** or **accumulated-to-classify** ([cost_basin_v1.md](cost_basin_v1.md) open question §6) or they will mix "expensive" with "not yet visited."

**C3. Rhetorical asymmetry**

Publishing 463 beside three zeros invites reading "seams matter; closure and coverage do not." The correct reading: **this fixture removed closure and coverage variation.** The comparison is asymmetric and fixture-selected, not discriminative.

**Verdict:** 0 coverage alignment is **uninformative** on full-coverage mini. Survival requires partial-coverage cells.

---

### 2.4 — 0 disagreement alignment

**D1. No unstable subgraphs**

`unstable_subgraph_overlay` is effectively uniform green on hermetic 0% at full closure — stable/plateaued classifications only. The disagreement mask extracts chromatic "active" regions; there are none.

**D2. Wrong instrument for sealed fixture**

Disagreement zones belong to **coherence/oracle instability**. [canonical_fixtures.md](../Docs/Observatory/canonical_fixtures.md) positions `object_island` (~49% hit, budget exhaustion) for ambiguity. Hermetic_curved_room at full closure is the wrong place to expect disagreement signal.

**D3. False hierarchy from fixture selection**

463 vs 0 vs 0 vs 0 creates an apparent ranking driven by **which layers have spatial structure on this fixture**, not by Cost Basin discrimination. Seams exist; disagreement does not. That is fixture choice, not insight.

**Verdict:** 0 disagreement alignment is **uninformative** here. Survival requires oracle/unstable cells (`object_island`, curved-field oracle ladder).

---

## 3. Attack 5 — Is "Cost Basin" the right object?

Even if seam alignment survives null testing, the basin metaphor may still mislead on hermetic mini.

| Kill argument | Evidence |
|---|---|
| Nearly flat scalar field | 268–299 range; tight mode cluster; p99 ≈ max |
| Not a query-cost basin | 93.8% physics time is query; spatial map is steps |
| Duplicate of Panel 6 | `traversal_step_heatmap.png` already answers traversal cost spatially |
| Predictive claim untestable | 0% budget stress at cap = 700; no pixel near failure |
| "Shallow basin" per design doc | [cost_basin_v1.md](cost_basin_v1.md) §6 notes hermetic_curved_room produces a relatively uniform, high but shallow cost field |

**Weakest surviving claim (if any):** Effort is **spatially non-uniform** in step count. That is already established by Panel 6 without terrain, ridges, or alignment rhetoric.

---

## 4. Falsification Battery (CB-K1 – CB-K6)

Observation-only experiments. No renderer changes. No new instrumentation. No optimization.

### CB-K1 — Null alignment model

| Field | Value |
|---|---|
| Input | Ridge mask + seam mask from v1 run (`curvature_000/row`) |
| Procedure | Compute expected ridge↔seam overlap under random ridge placement preserving `ridge_count`; compare 463 to null mean + 3σ |
| Kill | 463 ≤ null expectation → seam alignment is artifact; strip causal language |
| Pass | 463 > null + 3σ → seam overlap may be structural; proceed |

### CB-K2 — Receiver-geometry vs scheduler control

| Field | Value |
|---|---|
| Input | `hit_diagnostics.csv` (receiver/classification, x, y, `final_step_count`); ownership label grid |
| Procedure | Partition step variance by: (a) row index y / band, (b) receiver identity, (c) distance to seam in label grid |
| Kill | Majority of variance and all local maxima explained by **row band index** (y = 96 family) → scheduler topography |
| Pass | Receiver/seam geometry explains more than band index → transport structure may be real |

### CB-K3 — Failure-regime alignment

| Field | Value |
|---|---|
| Input | Hermetic hit-closure budget sweep (32, 100, 200, 320, 700) or smoke preset (72.7% budget stress) |
| Procedure | Ridge↔closure-edge alignment where orange/red boundaries exist |
| Kill | No elevated alignment at closure boundary vs interior → Cost Basin does not predict fragility |
| Pass | Ridges preferentially approach closure exterior → predictive coupling supported |

### CB-K4 — Partial-coverage alignment

| Field | Value |
|---|---|
| Input | Partial run `20260607T185034Z` (37.7% traced) |
| Procedure | Test whether high-step ridges precede uncovered rows in `frame_coverage_map` |
| Kill | No predictive ordering → temporal Cost Basin claim fails ([cost_basin_v1.md](cost_basin_v1.md) §5) |
| Pass | Ridges predict coverage front → temporal claim survives |

### CB-K5 — Cross-fixture discrimination

| Field | Value |
|---|---|
| Input | `object_island` vs `hermetic_curved_room`, same alignment pipeline |
| Procedure | Compare normalized seam vs disagreement alignment rates |
| Kill | Seam alignment only on hermetic; near-zero elsewhere → metric is fixture-density detector |
| Pass | Disagreement alignment rises on ambiguous fixture → metric discriminates phenomena |

### CB-K6 — Curvature sensitivity of alignment

| Field | Value |
|---|---|
| Input | Cells 0%, 50%, 100% from `20260607T221311Z` |
| Procedure | Track raw count and **normalized rate** (alignment / ridge_count) vs max step spread |
| Kill | Alignment static while cost spread grows (299 → 304) → tracks fixed seam geometry, not deepening basin |
| Pass | Normalized alignment covaries with cost spread → basin deepening may co-locate with seams |

**Promotion gate (pre-registered):** Pass **CB-K1 + CB-K3** and at least one of **CB-K4 or CB-K5** before maturity moves above Observed (2).

---

## 5. Verdict Framework

```
Assume misleading
    → CB-K1 null model
        fail → KILL (remain Observed; strip causal claims)
        pass → CB-K2 scheduler vs geometry
            banding wins → KILL
            pass → CB-K3 failure regime
                no cliff correlation → KILL
                pass → CB-K4 partial coverage OR CB-K5 cross-fixture
                    neither passes → KILL
                    pass → SURVIVE (promote toward Confirmed with narrowed claims)
```

---

## 6. Maturity Recommendation

**Current score:** Observed (2) per [observatory_maturity_ladder.md](observatory_maturity_ladder.md).

**Recommendation today:** **Hold at Observed (2).** Do not promote to Confirmed (3) or Characterized (4) on the strength of the 463/0/0/0 table alone.

| Claim | Survives pre-experiment critique? |
|---|---|
| Effort is spatially non-uniform (weak) | Yes — step field varies; Panel 6 already shows this |
| Terrain/ridges reveal meaningful cost structure | Weak — shallow field, banding suspect |
| Ridges explain closure/coverage risk | No — 0/0 on full-coverage is uninformative |
| Ridges co-locate with seams (463) | Unproven — likely geometric confound until CB-K1 |
| Cost Basin predicts query hotspots | No — per-pixel query not measured |
| Alignment table validates maturity | No — not a validation gate |

**Caveat to attach to all published Cost Basin artifacts on hermetic full-coverage:**

> Alignment metrics on a sealed, fully closed, fully covered fixture are largely baseline-rate and fixture-structure effects. The 463 seam count is not evidence of explanatory power without null-model testing. Zero closure, coverage, and disagreement alignments are expected vacuities, not negative findings.

**Conditions to promote:**

| Target | Requirement |
|---|---|
| Confirmed (3) | CB-K1 pass + CB-K3 pass + documented method for normalized alignment |
| Characterized (4) | Above + CB-K4 or CB-K5 pass + repeatability across ≥2 run timestamps + narrowed claim set in artifact markdown |

---

## 7. Narrowed Claim Set (if critique is survived)

If and only if CB-K1–K6 gates pass, Cost Basin on `hermetic_curved_room` mini may claim:

1. **Spatial step-count variation exists** over the closure interior (descriptive; redundant with Panel 6).
2. **Ridge↔seam co-location exceeds null** at a stated significance level (structural co-occurrence, not causation).
3. **In failure regimes**, high-step ridges approach closure boundaries more often than interior pixels (predictive, conditional on budget stress).
4. **Query dominance is aggregate-only** until per-pixel query instrumentation exists; do not infer query basins from step terrain.

Cost Basin may **not** claim on current evidence:

- That seams "explain" cost concentration.
- That the terrain map reveals optimization targets.
- That 0 closure/coverage/disagreement alignment implies those layers are irrelevant globally.
- That maturity exceeds Observed without falsification battery results.

---

## 8. Relation to Optimization Campaign v3

Cost Basin survival is **orthogonal** to cap=320 budget envelope work. Even a valid step-count basin does not justify lowering `StepsPerRay` without closure cliff data. Query dominance (93% of `pass2_phys_ms`) means the primary optimization surface remains the query path, not step-cap trimming — consistent with [renderer_observatory.md](renderer_observatory.md).

---

**End of critique.** Observation only. No renderer changes. No new instrumentation. No optimization.