# Basin Taxonomy v1

**MisterY Labs — Observer Language v1 compatible**

> A basin is a region of the observation space defined by a stability or assignment criterion. Every Observatory measurement can be expressed as a basin. They are not synonyms or alternatives — they stratify the same domain along orthogonal axes and form a strict dependency chain.

---

## Purpose

The Observatory has accumulated six measurement concepts that all operate over the same per-pixel domain but answer different questions. Expressing them as a unified basin taxonomy makes two things explicit:

1. **Dependency order.** No basin can be computed without its parent basin being established first. The containment hierarchy is the artifact generation order.
2. **Non-conflation.** Each basin answers exactly one question. Conflating two basins (the most common misread is Coverage with Closure) produces false claims about what a measurement demonstrates.

This document is an ontology, not an implementation specification. It defines what each basin is, what it requires, what it produces, and how it relates to the others. Code and artifact pipelines are separate concerns.

---

## Basin Definition Schema

Every basin entry uses six fields:

| Field | Content |
|---|---|
| **Question answered** | The single interrogative the basin resolves |
| **Input artifacts** | What must exist before this basin can be computed |
| **Output artifacts** | What the basin produces (map, scalar, or classification) |
| **Maturity** | Trust Model score (0–5) for the named concept and for its underlying evidence |
| **Trust level** | What category of claim the basin supports |
| **Relationship graph** | Formal edges to the other five basins |

---

## 1. Coverage Basin

### Question answered

Which pixels in the declared film domain have been evaluated at all?

### Input artifacts

- Film dimensions (declared domain boundary)
- `frame_coverage_map` — per-pixel binary: traced / untraced
- Traced fraction and beauty fraction metrics from `hit_diagnostics.csv`

### Output artifacts

- **Coverage map** — per-pixel binary (traced / untraced)
- **Traced fraction** — percentage of film pixels that have had rays fired through them
- **Beauty fraction** — percentage of film pixels with committed color output

### Maturity

| | Score | Stage |
|---|---|---|
| Named concept (Coverage Basin) | 0 | Proposed |
| Underlying artifact (frame_coverage_map) | 2 | Observed |

The named basin concept has no dedicated validation gate or schema entry. The underlying artifacts are generated per run. The gap represents the work horizon: establishing a Coverage Basin pipeline means defining a declared domain, a formal coverage contract, and a validation gate.

### Trust level

**Run-scoped precondition.** Coverage is not a transport claim. It is the precondition that makes all other basins possible. A coverage measurement cannot be wrong in the transport sense — it reports what was attempted, not what succeeded.

### Relationship graph

- **Contains** → Closure Basin (closure can only occur for traced pixels; untraced pixels have no closure status)
- **Contains** → all other basins (no basin is defined outside the Coverage Basin interior)
- **Exterior** → untraced pixels — they have no basin status at all, not even Risk Node status

### Common misread

**Coverage Basin ≠ Closure Basin.** 37.9% coverage with 100% closure means: of the pixels traced, all converged. The remaining 62.1% are outside the Coverage Basin — they have no closure status. They are not Risk Nodes; they were simply never evaluated.

---

## 2. Closure Basin

### Question answered

Which evaluated pixels converged to a terminal classification within the declared step budget?

### Input artifacts

- Coverage Basin interior (domain)
- `hit_diagnostics.csv` — per-pixel terminal state: hit, miss, budget-exhausted-with-hit, budget-exhausted-without-hit
- Step budget declaration (the contract parameter that defines the basin boundary)

### Output artifacts

- **Closure map** — per-pixel: green (hit within budget), orange (hit on overrun step), red (miss / budget exhausted without hit)
- **Closure percentage** — fraction of evaluated pixels that converged (scalar)
- **Closure basin margin** — the orange layer: pixels that found a hit on the overrun step (budget stress, still valid terminal classifications)

### Maturity

| | Score | Stage |
|---|---|---|
| Named concept (Closure Basin) | 4 | Characterized |
| Underlying artifact | 4 | Characterized |

Schema defined. Artifacts generated per run. Panel defined in Observer Storyboard v1. Caveats documented in canonical_fixtures.md and observatory_trust_model.md. The only Characterized basin in this taxonomy.

### Trust level

**Scene-contract-scoped.** The closure claim is: within the declared step budget and scene contract, these pixels reached a terminal state. Not a physical claim. Not a claim about the untraced region. Not a claim that the classifications are physically accurate.

### Relationship graph

- **Contained by** → Coverage Basin
- **Contains domain of** → Ownership Basin (ownership is undefined outside the closure interior)
- **Contains domain of** → Cost Basin (cost is undefined at pixels that did not converge)
- **Exterior** → Risk Nodes (complement of closure interior within the Coverage Basin)
- **May contain** → a subset of Coherence Basin (closure is budget-scoped; coherence is precision-invariant — a closed pixel may still be topologically unstable)

### Common misread

**100% closure ≠ physical correctness.** "All pixels classified" means the scene contract was satisfied, not that the classifications are physically accurate. Closure is a computational invariant, not a physical one.

---

## 3. Ownership Basin

### Question answered

Which receiver zone claimed each ray after terminal classification?

### Input artifacts

- Closure Basin interior (domain — ownership is only defined where closure succeeded)
- Transport ownership map from the traversal harness
- Receiver zone definitions from the declared scene contract

### Output artifacts

- **Ownership map** — per-pixel zone assignment (each pixel colored by its receiver)
- **Per-zone ownership percentage** — fraction of closure interior assigned to each receiver
- **Ownership seams** — the boundary layer where zone assignment is geometrically sensitive (1–3 pixel width patterns at receiver boundaries)

### Maturity

| | Score | Stage |
|---|---|---|
| Named concept (Ownership Basin) | 0 | Proposed |
| Underlying artifact (transport_ownership map) | 2 | Observed |

Transport ownership maps are generated per run. "Ownership Basin" as a named concept with its own validation gate and schema is not yet established.

### Trust level

**Scene-contract-scoped, comparative.** Ownership assignment is relative to the declared receiver geometry. Different scene contracts produce different ownership basins over the same pixels. Comparing two ownership basins (from two perspectives) requires that both were computed over the same declared domain.

### Relationship graph

- **Contained by** → Closure Basin (ownership undefined outside closure interior)
- **Two instances generate** → Disagreement Basin (symmetric difference of two Ownership Basins from different perspectives)
- **Ownership seams** are X-S Seam Xeno candidate zones (regions where small direction changes flip zone assignment)

### Common misread

**Ownership Basin ≠ Closure Basin.** A pixel can be inside the closure basin (it converged) but at an ownership seam where zone assignment is unstable. Closure and ownership are orthogonal after closure succeeds: closure says "this pixel converged," ownership says "it converged to this zone."

---

## 4. Cost Basin

### Question answered

How many traversal steps were required at each pixel to reach terminal classification?

### Input artifacts

- Closure Basin interior (domain — cost is undefined at pixels that did not converge)
- Per-pixel step count from the traversal harness
- Step budget declaration (defines the cost ceiling)

### Output artifacts

- **Traversal step heatmap** — per-pixel scalar (absolute step count), cool-to-warm color encoding
- **Budget stress heatmap** — per-pixel fraction of budget consumed (leading indicator of closure basin margin pressure)
- **Aggregate metrics** — mean traversal steps, p95 frame time, budget stress percentage

### Maturity

| | Score | Stage |
|---|---|---|
| Named concept (Cost Basin) | 0 | Proposed |
| Underlying artifact (traversal heatmap) | 2 | Observed |

From the Observatory Maturity Ladder: "no dedicated Cost Basin artifact generation or validation gate exists yet." The traversal heatmap is generated per run; the Cost Basin as a named, gate-validated concept is not established.

### Trust level

**Run-scoped.** Cost is a measurement of computational work, not a transport claim. A high-cost pixel is expensive; it is not incorrect. Cost measurements do not generalize across different budgets, scene geometries, or camera positions.

### Relationship graph

- **Contained by** → Closure Basin (cost is undefined at Risk Nodes)
- **Two instances generate** → Sensitivity Basin (signed delta between baseline and activated cost basins)
- **Margin predicts** → Risk Node expansion at higher parameter values (high-cost pixels near the closure margin may become risk nodes as parameter increases)
- **Ridges co-locate with** → Coherence Basin boundaries (topological instability requires more steps to resolve classification — correlation, not definition)

### Common misread

**High cost ≠ incorrect result.** A pixel at 695 traversal steps is expensive. If it converged, it is inside both the Closure Basin and the Cost Basin. Cost measures computational difficulty, not classification accuracy. **Cost Basin boundary ≠ Closure Basin boundary:** the cost basin has no boundary in the same sense — it is a continuous scalar field. High-cost regions near the closure margin predict risk node candidates, but the two boundaries are defined by different criteria.

---

## 5. Disagreement Basin

### Question answered

Where do two Ownership Basins assign different terminal classifications to the same pixel?

### Input artifacts

- Two Ownership Basins from different perspectives (e.g., curved transport vs. straight transport, or two field amplitudes)
- Both Ownership Basins must be defined over the same Coverage Basin interior
- Both perspectives must have closed over the evaluated region (two Closure Basins required)

### Output artifacts

- **Disagreement map** — per-pixel binary: agree / disagree
- **Disagreement percentage** — fraction of evaluated pixels where the two perspectives diverge (e.g., 23.8%)
- **Spatial clustering** — whether disagreement is boundary-aligned (X-B, X-S Xeno candidates), concentrated (X-C Caustic), or diffuse (X-F Far-field)

### Maturity

| | Score | Stage |
|---|---|---|
| Named concept (Disagreement Basin) | 0 | Proposed |
| Underlying artifact (Ch. 2 divergence study) | 2 | Observed |

The 23.8% divergence measurement from Observatory Ch. 2 exists as a real artifact. "Disagreement Basin" as a named concept with a dedicated generation pipeline and validation gate is not established.

### Trust level

**Scene-contract-scoped, comparative.** The disagreement claim is always relative to two named perspectives over a named scene and field configuration. It is a measurement of model divergence, not an error rate. Neither perspective is declared ground truth.

### Relationship graph

- **Derived from** → two Ownership Basins via symmetric difference: pixels where Ownership\_Basin\_A ≠ Ownership\_Basin\_B
- **Requires** → two Closure Basins (both perspectives must have closed over the evaluated pixels)
- **Xeno citations** are structured Disagreement Basin findings: disagreements with geometric or topological spatial signatures that persist across observer positions
- **May indicate** → Coherence Basin boundary: disagreement that persists across all parameter values and budgets may reflect topological instability, not just model divergence

### Common misread

**Disagreement Basin ≠ error map.** A pixel in the disagreement basin is one where two valid models classify differently. 23.8% disagreement is not 23.8% error. Neither model is declared ground truth; the disagreement is a measurement of model divergence within this scene and field configuration, scoped to the evaluated coverage.

---

## 6. Sensitivity Basin

### Question answered

Where does the observation space respond measurably to a named parameter change, and in which direction?

### Input artifacts

- Two Cost Basins: baseline parameter value (e.g., 0% curvature) + test parameter value (e.g., 50% curvature)
- Both cost basins must be defined over the same Coverage Basin interior
- Both closures must have succeeded at the evaluated pixels (pixel must be inside both Closure Basins)
- Declared baseline (the zero-point of the signed delta)

### Output artifacts

- **Sensitivity Signature map** — per-pixel signed scalar: activation delta (test cost − baseline cost)
  - Blue: negative delta — parameter made transport easier (fewer steps)
  - Red: positive delta — parameter made transport harder (more steps)
  - Black: zero delta — no measurable response
- **Sensitivity basin extent** — fraction of pixels with non-zero activation delta
- **Sensitivity ladder** — stacked panels across parameter levels showing how the signature evolves (the Curvature Signature Ladder in xPRIMEray)

### Maturity

| | Score | Stage |
|---|---|---|
| Named concept (Sensitivity Basin) | 0 | Proposed |
| Underlying artifact (Curvature Signature, xPRIMEray instance) | 4 | Characterized |

The Curvature Signature is the canonical xPRIMEray instance of the Sensitivity Basin. It is Characterized: artifacts generated per run, ladder format defined, panel defined in Observer Storyboard v1, caveats documented. "Sensitivity Basin" as the generalized named concept — applicable beyond curvature to any named parameter — is Proposed.

### Trust level

**Scene-contract-scoped, parameter-relative.** The sensitivity claim is always relative to a declared baseline and a named parameter. It does not generalize beyond the declared fixture, camera, and field configuration. A Sensitivity Signature with no declared baseline has no meaning.

### Relationship graph

- **Derived from** → two Cost Basins: Sensitivity\_Basin = Cost\_Basin(test) − Cost\_Basin(baseline)
- **Only defined at** → pixels inside both Closure Basins (at baseline and at test value)
- **Basin shift events** — pixels that move between Closure Basin interior and exterior when the parameter changes — are NOT captured in the Sensitivity Basin; they require separate reporting
- **Sharp discontinuities** in the Sensitivity Basin map are Coherence Basin boundary candidates: where parameter response is discontinuous, the underlying topology is unstable
- **High positive values near Closure Basin margin** predict Risk Node expansion at higher parameter values: the Sensitivity Basin is the predictive tool for risk node candidates
- **Z-Fi Zeno citations** (field-induced Zeno) manifest as Sensitivity Basin regions where cost diverges rather than converges as the parameter increases

### Common misread

**Red ≠ incorrect; black ≠ unaffected.** Red means more traversal steps at the test parameter value than at baseline — a measurement of increased computational difficulty, not a correctness failure. Black means no measurable change in step count; the region may still respond to the parameter in ways that step count does not capture. **The Sensitivity Basin does not generalize:** it is scoped to this scene, this camera, this field configuration, and this baseline. A signature measured at 50% curvature vs. 0% in a sealed room does not predict sensitivity in an open-target fixture.

---

## Relationship Graph

```
Coverage Basin (domain: all film pixels)
     │
     │ [contains]
     ▼
Closure Basin (domain: traced pixels that reached terminal classification)
     │
     ├──────────────────────────────────┐
     │ [contains domain of]             │ [contains domain of]
     ▼                                  ▼
Ownership Basin                      Cost Basin
(zone assignment per pixel)          (step count per pixel)
     │                                  │
     │ [two instances → symmetric diff] │ [two instances → signed delta]
     ▼                                  ▼
Disagreement Basin                  Sensitivity Basin
(where two Ownership Basins differ)  (delta between two Cost Basins)


Risk Nodes: Coverage Basin interior minus Closure Basin interior
            (complement of Closure within Coverage — a derived set, not a basin)

Coherence Basin: independent topological criterion
                 cuts across all six basins; defined by precision-invariance, not budget
```

**Edge semantics:**

| Edge type | Meaning |
|---|---|
| `[contains]` | Domain dependency: the inner basin is only defined within the outer basin's interior |
| `[contains domain of]` | The child basin uses the parent's interior as its measurement domain |
| `[two instances → symmetric diff]` | Two instances of the parent basin are required; the child is their symmetric difference |
| `[two instances → signed delta]` | Two instances required; the child is their signed arithmetic difference |

**Critical rule:** Computation flows top-to-bottom. No basin can be produced without its parent basin being established. Coverage → Closure → [Ownership, Cost] → [Disagreement, Sensitivity] is the mandatory dependency order.

---

## Containment Hierarchy

```
Coverage Basin
└── Closure Basin
      ├── Ownership Basin
      │     └── Disagreement Basin  (requires two Ownership Basins)
      └── Cost Basin
            └── Sensitivity Basin   (requires two Cost Basins)

Risk Nodes = Coverage Basin interior ∖ Closure Basin interior
Coherence Basin = orthogonal topological partition (cuts across all levels)
```

---

## Maturity-Trust Matrix

| Basin | Concept maturity | Artifact maturity | Trust level | Observer Language v1 |
|---|---|---|---|---|
| Coverage Basin | Proposed (0) | Observed (2) | Run-scoped precondition | Panel 7 — Evidence Coverage |
| Closure Basin | Characterized (4) | Characterized (4) | Scene-contract | Panel 5 — Closure Basin |
| Ownership Basin | Proposed (0) | Observed (2) | Scene-contract, comparative | Panel 4 — Domain ownership (Observatory Story) |
| Cost Basin | Proposed (0) | Observed (2) | Run-scoped | Panels 6–7 — Traversal / Budget stress |
| Disagreement Basin | Proposed (0) | Observed (2) | Scene-contract, comparative | Panel 4 — Disagreements (Observer Storyboard v1) |
| Sensitivity Basin | Proposed (0) | Characterized (4) for Curvature Signature | Scene-contract, parameter-relative | Panel 8 — Sensitivity Signature |

**Reading the two maturity columns:**

- **Concept maturity** — score for the named basin as a declared, gate-validated, pipeline-backed concept. All newly named basins are Proposed (0): no dedicated validation gate or schema entry exists yet.
- **Artifact maturity** — score for the underlying evidence that realizes the basin (frame_coverage_map, transport_ownership, traversal heatmap, curvature_signature). These are higher because the artifacts exist and are generated per run.

The gap between columns is the explicit work horizon for each basin. Closing the gap for any basin means: establishing a dedicated artifact generation pipeline with a declared scene contract, a formal validation gate, and a schema entry in the Observatory Trust Model.

---

## Cross-Cutting Phenomena

These are not basins. They are properties of the basin system.

### Risk Nodes

The complement of the Closure Basin interior within the Coverage Basin interior. A derived set, not a seventh basin. Risk nodes are characterized along two axes:

- **Budget-reducible** — inside the Coherence Basin; they would close with a larger step budget. They are a budget limitation, not a topological feature.
- **Persistent** — Zeno citation candidates (Z-B: Budget Zeno). They may lie outside the Coherence Basin; no budget increase resolves them.

Risk nodes are defined by the Closure Basin. They are not computed independently.

### Coherence Basin

An independent topological partition that cuts across all six basins. It is defined by **precision-invariance**: a region is inside the Coherence Basin if the terminal classification remains stable as step size decreases. No budget increase can expand the Coherence Basin — it is defined by topology, not computation.

The Coherence Basin relates to the six basins as follows:

| Basin | Coherence relationship |
|---|---|
| Coverage Basin | Coherence Basin is a subset of the Coverage Basin interior (coherence requires evaluation) |
| Closure Basin | Coherence Basin is a subset of the Closure Basin at sufficient budget; a pixel can be closed but topologically unstable |
| Ownership Basin | Ownership seams and Seam Xeno citations often mark Coherence Basin boundaries |
| Cost Basin | Cost Basin ridges often co-locate with Coherence Basin boundaries — correlation, not definition |
| Disagreement Basin | Persistent disagreement (invariant to all parameter values and budgets) may indicate Coherence Basin boundary |
| Sensitivity Basin | Sharp discontinuities in the Sensitivity Basin are Coherence Basin boundary candidates |

Establishing the Coherence Basin for a given region requires a refinement sequence (multiple runs at decreasing step sizes), not a single run. It is the only Observatory concept that is inherently multi-run.

---

## Glossary Extensions

Terms introduced or formalized by this taxonomy, extending Observer Language v1:

**Basin** — a region of the observation space defined by a stability or assignment criterion. Each basin answers exactly one question about its domain. Six canonical basins are defined here; all share the per-pixel film domain.

**Basin shift event** — a pixel that moves between Closure Basin interior and exterior when a parameter changes. Not captured by the Sensitivity Basin (which requires closure at both parameter values). Must be reported separately as a change in coverage or closure status.

**Concept maturity** — the Trust Model score for a named basin concept as a declared, gate-validated, pipeline-backed entity. Distinct from artifact maturity.

**Artifact maturity** — the Trust Model score for the underlying evidence that realizes a basin concept (maps, scalars, and heatmaps generated by the run harness).

**Dependency order** — the mandatory computation sequence imposed by the containment hierarchy: Coverage → Closure → [Ownership, Cost] → [Disagreement, Sensitivity]. No child basin can be established without its parent.

**Domain** — the set of pixels over which a basin is defined. Each basin has a different domain: Coverage (all film pixels), Closure (traced pixels), Ownership and Cost (closure interior), Disagreement and Sensitivity (intersection of two parent basin interiors).
