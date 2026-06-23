# Basin Atlas v1

**MisterY Labs — Observer Language v1 compatible**

> A basin is a region of the observation space defined by a stability criterion. Different stability criteria produce different basins. The five concepts in this atlas share measurement space but answer different questions. They are not synonyms, subsets, or alternatives to each other.

---

## Orientation

Five concepts describe the structure of the observation space in xPRIMEray:

| Concept | Primary question | Domain |
|---|---|---|
| **Closure Basin** | Where did evaluation converge? | Computational (budget-scoped) |
| **Cost Basin** | How expensive was convergence? | Computational (step count) |
| **Coherence Basin** | Where is the topology stable? | Topological (precision-invariant) |
| **Sensitivity Signature** | What changed when the parameter changed? | Differential (parameter-relative) |
| **Risk Nodes** | Where did evaluation fail to converge? | Computational (exterior of closure) |

They share a common measurement space (the scene domain, per pixel) but they are stratifications of that space along different axes. A pixel can be simultaneously: inside the Closure Basin (converged), at high cost in the Cost Basin (expensive), outside the Coherence Basin (topologically unstable), showing a positive Sensitivity Signature (harder than baseline), and therefore a candidate risk node at higher parameter values.

**Physical analogy for orientation only:** Think of a mountain watershed. The Closure Basin is the drainage area — where rain eventually reaches a lake. The Cost Basin is the topography of that drainage area — how far and how steep the path. The Coherence Basin is which parts of the drainage area remain stable when the ground shifts. The Sensitivity Signature is how the water flow changes when wind activates. Risk Nodes are where rain hits a ridge and does not drain within the declared time window.

This analogy is imprecise. Use it to establish relative positions; do not extend it.

---

## Closure Basin

### What it measures

The set of inputs (pixels, rays, evaluation points) for which the measurement system converges to a terminal classification within the declared step budget. Inside the basin, every evaluated point has been assigned a final state: hit, miss, exit, portal event, or budget-exhausted-with-hit (overrun step). Outside the basin, evaluation terminated without a classification.

Closure is a binary property per pixel, but the basin has structure: a dense interior (easy convergence, low cost), a boundary layer (marginal convergence, high budget stress), and an exterior (risk nodes — no convergence within budget).

### What units it uses

- **Per pixel:** binary — inside / outside / boundary
- **Aggregate:** percentage of evaluated domain that is inside the basin (e.g., 100% hermetic closure, 49% for object_island)
- **Boundary width:** expressed in pixels at the observed scale, or as a percentage of budget consumed at the margin

The closure percentage is the scalar integral of the basin. The basin itself is the spatial object that produces that scalar.

### How it is visualized

**Panel 5 — Closure map** (Observer Language v1). Color encoding:
- Green: inside the basin, classified as hit within budget
- Orange: inside the basin, classified as hit on the overrun step (budget stress, still valid)
- Red: outside the basin — miss or budget exhausted without hit

The budget stress heatmap (Panel 7) shows the boundary layer explicitly: pixels approaching the budget limit are highlighted before the closure map shows them as orange or red. Panel 7 is a leading indicator; Panel 5 is the terminal result.

### Common misinterpretations

**"Inside the closure basin means the result is physically correct."** The basin measures classification completion, not physical accuracy. A ray that hits a wall in 400 steps is inside the basin. Whether that path reflects real physics is a separate question the basin cannot answer.

**"100% closure basin means 100% of the scene was measured."** Closure rate is computed over the traced fraction only. A 37.9% coverage run with 100% closure means: of the pixels actually traced, all converged. The remaining 62.1% are uncovered, not closed.

**"The basin boundary is a failure zone."** The boundary layer (orange pixels, budget stress) is a diagnostic finding about traversal cost. Budget-exhausted-with-hit pixels are valid terminal classifications. They are more expensive than interior pixels but not failed.

**"The closure basin is fixed for a given scene."** The basin changes with step budget, field amplitude, and camera position. A 300-step budget may produce a 72.7% closure rate; a 700-step budget may produce 100%. Budget is a parameter of the basin, not a constant.

### Relationships to the others

- **Cost Basin:** The cost basin exists only inside the closure basin. Every point with a cost has already converged; cost is undefined for exterior points. The closure basin is the domain of the cost basin.
- **Coherence Basin:** The coherence basin can be a strict subset of the closure basin. Closure is budget-scoped and local; coherence is topological and precision-invariant. A region can be inside the closure basin (it converged) but outside the coherence basin (the convergence is topologically unstable).
- **Sensitivity Signature:** The signature is only defined at points that appear in both the baseline and the activated closure basins. If the parameter change causes a point to enter or leave the closure basin, that is a basin shift event, not just a cost delta.
- **Risk Nodes:** Risk nodes are the exterior of the closure basin by definition. The set of risk nodes is the complement of the closure basin interior, computed over the evaluated domain.

---

## Cost Basin

### What it measures

The distribution of traversal cost — the number of integration steps required to reach terminal classification — across the observation domain. The cost basin is a scalar field over the closure basin interior: at every point where evaluation converged, cost records how much computation was required.

The cost basin is not a pass/fail measure. It is a topography of computational difficulty. Its peaks are where transport was hardest; its valleys are where transport completed early.

### What units it uses

- **Per pixel:** traversal step count (integer), in the range [1, declared budget + 1]
- **Aggregate:** mean traversal steps, p95 frame time, budget stress percentage
- **Derived:** fraction of budget consumed per pixel (step count / max budget)

The overrun step (budget + 1) is a valid terminal step permitted by the loop design. A pixel that converges on step 701 of a 700-step budget has cost 701. This is inside the cost basin; it is not a risk node.

### How it is visualized

**Traversal step heatmap** — part of Panel 6 (Traversal steps) in the Observatory Story. Color encodes absolute step count from cool (low cost, fast convergence) to warm (high cost, slow convergence). Hot spots are cost peaks.

The budget stress heatmap (Panel 7) is a derived view of the cost basin: it shows which pixels have consumed a high fraction of their budget, regardless of the absolute count. It answers "which pixels are close to the cost ceiling" rather than "which pixels have high absolute cost."

### Common misinterpretations

**"High cost means the result is wrong."** A pixel with 695 traversal steps is expensive, not incorrect. Traversal cost measures computational difficulty, not physical accuracy. A high-cost result that still converges is inside both the closure basin and the cost basin.

**"The cost basin boundary is the closure basin boundary."** These are different edges. The cost basin has no boundary in the same sense — it is a continuous scalar field that peaks near the closure basin boundary but extends through the entire closure interior. A pixel near the boundary can have high cost and still converge; a pixel in the basin interior can have unexpectedly high cost due to field structure.

**"The cost basin shape is stable."** The cost basin changes with field amplitude (the Sensitivity Signature maps this change directly), scene geometry, camera position, and step budget. An early curvature signature ladder may show a different cost basin shape than a later full-coverage run at the same amplitude.

**"Low cost means the transport was simple."** Low traversal cost means the ray reached a receiver quickly. In a sealed room with curvature deflecting rays efficiently toward walls, low cost may reflect the field's help rather than geometric simplicity.

### Relationships to the others

- **Closure Basin:** The cost basin is a stratification of the closure basin interior. The closure basin is binary (in/out); the cost basin is scalar (how many steps). Remove the closure basin and the cost basin has no domain.
- **Coherence Basin:** High-cost regions in the cost basin often (but not always) coincide with coherence basin boundaries. Topological instability requires the traversal to "decide" between competing classifications at higher cost. The correlation is diagnostic, not definitional.
- **Sensitivity Signature:** The Sensitivity Signature is the signed delta between two cost basin snapshots: Cost\_Basin(activated) − Cost\_Basin(baseline). It is the directional derivative of the cost basin with respect to the named parameter. Blue pixels are where the cost basin lowered; red pixels are where it rose.
- **Risk Nodes:** Risk nodes have no cost basin entry. They did not converge, so no step count was assigned at terminal classification. The cost basin is undefined at risk nodes; extreme cost values at the closure basin margin predict where risk nodes will appear at higher parameter values.

---

## Coherence Basin

### What it measures

The regions of the observation space where transport behavior is topologically stable — where the qualitative structure of the solution does not change as measurement precision increases. A region inside the coherence basin produces the same terminal classification regardless of step-size refinement, within a declared tolerance. A region outside the coherence basin produces different classifications at different precisions, and no integration budget eliminates this instability.

Coherence is a property of the observation space geometry, not of the measurement budget. It identifies where the scene topology supports a stable answer.

### What units it uses

Coherence is topological: inside or outside, not scalar. The coherence basin does not have a "depth" or "cost." Proximity to the coherence basin boundary can be quantified by measuring how much the classification changes under step-size perturbation (e.g., halving the step size shifts the classification probability by X%), but this is a secondary diagnostic, not the primary measure.

The canonical evidence for coherence basin boundaries is the step-size divergence pattern from the Zeno/Zeno citation atlas: a region is outside the coherence basin when successive step-size refinements produce growing disagreement rather than convergence.

### How it is visualized

**Observatory Ch. 4 — Coherence Basin.** Topological instability bands: regions that persist as disagreement patterns across integration budget levels. These appear as banding or structured disagreement in the closure map that does not shrink when the budget increases.

There is no dedicated Panel in the current nine-panel Observer Storyboard v1 for the Coherence Basin directly. It is inferred from:
- Persistent patterns in the Closure map (Panel 5) across budget levels
- Banding in the traversal step heatmap (Panel 6) that does not resolve with budget increase
- Zeno citations (Z-B type: Budget Zeno) at persistent risk node locations

A full Coherence Basin visualization requires multiple runs at increasing budgets, not a single-run panel.

### Common misinterpretations

**"Coherence Basin = Closure Basin."** These are different criteria. The closure basin is budget-scoped: with sufficient budget, a region can be pulled inside the closure basin. The coherence basin is precision-invariant: no budget increase eliminates a coherence basin boundary. A region can be inside the closure basin (it converged at this budget) and outside the coherence basin (the convergence changes at higher precision).

**"Outside the coherence basin means the renderer failed."** Topological instability is a property of the scene geometry and field structure, not a renderer defect. The renderer correctly identifies and reports that the region does not have a stable answer at any tested precision.

**"The coherence basin can be expanded by increasing the budget."** Budget can expand the closure basin; it cannot expand the coherence basin. The coherence basin boundary is defined by topology, not computation. Regions outside the coherence basin are candidates for Zeno citations because they will remain unstable regardless of budget.

**"Topological instability bands are visual artifacts."** The instability bands visible in Observatory Ch. 4 are diagnostic findings, not rendering artifacts. They appear in the same spatial locations across independent runs with matched parameters.

### Relationships to the others

- **Closure Basin:** The coherence basin is generally a subset of the closure basin when examined at any finite budget. A region outside the coherence basin can still be inside the closure basin at that budget — it converged, but the convergence is fragile. The inclusion is asymmetric: inside coherence basin → inside closure basin at sufficient budget; inside closure basin → not necessarily inside coherence basin.
- **Cost Basin:** Coherence basin boundaries often correspond to cost basin ridges — regions where traversal cost is high because the topology makes classification difficult. The correspondence is diagnostic. The cost basin boundary is defined by budget exhaustion; the coherence basin boundary is defined by topological instability. These are different criteria that often co-locate.
- **Sensitivity Signature:** Sharp discontinuities in the Sensitivity Signature — pixels where the parameter delta changes abruptly from negative to positive or vice versa — are candidates for coherence basin boundaries. The Sensitivity Signature makes coherence basin boundaries indirectly visible as edges in the activation delta map.
- **Risk Nodes:** Persistent risk nodes — those that do not resolve even at maximum tested budget — are likely outside the coherence basin and are Zeno citation candidates (specifically Z-B: Budget Zeno). Budget-reducible risk nodes may be inside the coherence basin: they are temporarily outside the closure basin due to budget limits, not due to topological instability.

---

## Sensitivity Signature

### What it measures

The signed spatial map of how the observation changes when one named parameter changes from a declared baseline to a test value. It is always a relative measurement: it records the difference between two states of the observation space, not an absolute property of either state.

In xPRIMEray the canonical instance is the **Curvature Signature**: per-pixel traversal-step delta when field amplitude changes from 0% (baseline) to N% (test value). Blue = negative delta (fewer steps at test value than at baseline; field made transport easier here). Red = positive delta (more steps; field made transport harder here). Black = no measurable delta.

The Sensitivity Signature is a derivative in disguise. It is the first-order approximation of how the cost basin responds to the named parameter.

### What units it uses

- **Per pixel:** signed step count delta (integer: test\_cost − baseline\_cost), or signed percentage change relative to baseline cost
- **Color encoding:** signed diverging colormap — blue (negative) through black (zero) through red (positive)
- **Aggregate:** mean absolute delta, fraction of pixels with non-zero signature, spatial extent of blue vs. red regions

A Sensitivity Signature without a declared baseline has no meaning. The baseline must be named explicitly in the panel caption (e.g., "0% curvature baseline") before the signature values can be interpreted.

### How it is visualized

**Panel 8 — Sensitivity Signature** (Observer Language v1). In the Observatory Story, this is Panel 9 (Curvature Signature). The Curvature Signature Ladder stacks multiple signature panels across parameter levels (25%, 50%, 75%, 100% vs. baseline 0%), showing how the signature evolves as the parameter increases.

The ladder format is the canonical display for the Sensitivity Signature. A single signature panel shows one parameter delta; the ladder shows how the delta accumulates and shifts as the parameter ramps.

### Common misinterpretations

**"Red regions are incorrect or bad."** Red means more traversal steps were required at this parameter value than at the baseline. It is a measurement of change in computational difficulty, not a correctness judgment. A red region can be inside the closure basin, inside the coherence basin, and produce a physically meaningful result.

**"The Sensitivity Signature is an error map."** It maps parameter response — where and how much the cost basin changed when the parameter changed. Error would require a comparison to a ground truth. The Sensitivity Signature compares two states of the same measurement system under different parameter values.

**"Black means this region was unaffected by the parameter."** Black means the traversal step count did not change measurably between baseline and test value. The region may still have been affected in ways that traversal cost does not capture (e.g., different ray paths that happen to require the same number of steps). Black is a null delta, not a null response.

**"The signature generalizes beyond this fixture and field."** The Sensitivity Signature is scoped to the declared fixture, camera position, field configuration, and baseline. A signature measured at field amplitude 50% vs. 0% in the hermetic curved room does not predict what the signature will look like in an open-target fixture or at a different camera position.

### Relationships to the others

- **Cost Basin:** The Sensitivity Signature is the signed difference between two cost basins: Cost\_Basin(test) − Cost\_Basin(baseline). It is defined only at pixels that are inside both cost basins (converged at both baseline and test). Pixels that are inside the closure basin at baseline but outside at test (or vice versa) appear as closure basin shift events, not as ordinary signature values.
- **Closure Basin:** A positive Sensitivity Signature (red, higher cost at test value) near the closure basin boundary is a warning: this region is moving toward the budget limit under parameter activation. If the signature is sufficiently large, the region may become a risk node at higher parameter values. The signature predicts closure basin shrinkage.
- **Coherence Basin:** Sharp edges in the Sensitivity Signature often indicate coherence basin boundaries. The field activation changes the cost basin continuously; where the change is discontinuous, the underlying topology is unstable. The Sensitivity Signature makes coherence basin boundaries indirectly visible.
- **Risk Nodes:** Pixels that show extreme positive Sensitivity Signature across multiple parameter levels are candidates to become risk nodes at higher amplitudes. The signature ladder is the predictive tool: a region that doubles its step count from 0% to 50% may exhaust the budget by 100%.

---

## Risk Nodes

### What it measures

Points in the observation space where evaluation did not converge to a terminal classification within the declared step budget. Risk nodes are the exterior of the closure basin, identified per pixel. They are diagnostic findings: the measurement system correctly identified these points as non-converging within the declared constraints.

Risk nodes have two sub-categories:
- **Budget-reducible:** The point would converge with a larger budget. These are inside the coherence basin; they are a budget limitation, not a topological feature.
- **Persistent:** The point does not converge even at the maximum tested budget. These are Zeno citation candidates; they may be outside the coherence basin.

### What units it uses

- **Per pixel:** binary — risk node / not risk node
- **Sub-category:** budget-exhausted-with-hit (overrun step, classified as valid hit) vs. budget-exhausted-without-hit (genuine miss)
- **Aggregate:** risk node count, risk node percentage of evaluated domain (budget\_exhausted\_without\_hit %), spatial extent

The overrun step (loop condition `s ≤ maxIntegrationSteps`) allows one step past the declared budget. Pixels that find a hit on the overrun step are classified as valid hits, not risk nodes. The cost basin records them at cost = budget + 1. They are inside the closure basin, not risk nodes.

### How it is visualized

**Red pixels in the Closure map (Panel 5):** budget-exhausted-without-hit, or genuine miss in open-target fixtures.

**Budget stress heatmap (Panel 7):** the leading indicator. Pixels approaching the budget limit are flagged before they become risk nodes. The heatmap shows the risk node candidate region — where closure basin boundary pressure is highest.

**Aggregate:** `budget_exhausted_without_hit` count in `hit_diagnostics.csv`. In a hermetic sealed fixture this should be 0; in open-target fixtures it reflects the expected miss rate.

### Common misinterpretations

**"Risk nodes are renderer failures."** The renderer correctly classified these points: they did not converge within the declared budget. The measurement system reported the truth. A risk node is a finding, not an error.

**"Risk nodes should always be zero."** In a hermetic sealed room, zero risk nodes is the acceptance gate. In an open-target fixture (e.g., curved\_minimal with a small sphere), most rays are expected to miss; risk nodes are the expected result for rays that do not find a target.

**"All risk nodes are equivalent."** Budget-reducible risk nodes and persistent risk nodes are different categories with different implications. A budget-reducible risk node is a cost problem: increase the budget and it closes. A persistent risk node is a topological problem: it may be a Zeno citation and will not close regardless of budget.

**"Risk nodes mark where the scene is broken."** Risk nodes identify where the observation space does not support convergence within the declared contract. The scene may be correct; the contract may be the limiting factor. A scene with a complex caustic region may produce risk nodes at low budget that disappear at high budget — the scene is not broken, the budget was too low.

### Relationships to the others

- **Closure Basin:** Risk nodes are the complement of the closure basin over the evaluated domain. There is no risk node inside the closure basin, and no closure basin interior point is a risk node. They partition the evaluated domain into two disjoint sets.
- **Cost Basin:** Risk nodes have no cost basin entry. Traversal cost is defined only where closure succeeded. The cost basin interior ends where risk nodes begin. Extreme cost values at the closure basin margin are the warning sign that the margin may expand into the risk node zone at higher parameter values.
- **Coherence Basin:** Budget-reducible risk nodes may be inside the coherence basin — they are temporarily outside the closure basin due to budget limits, not topological instability. Persistent risk nodes are Zeno citation candidates: they may lie outside the coherence basin where no budget increase resolves the instability.
- **Sensitivity Signature:** A risk node has no Sensitivity Signature value, because the signature requires a defined cost at both baseline and test values. If a point is a risk node at the test value but not at the baseline, the event is a closure basin shift, reported separately from the signature. The signature predicts risk node expansion: extreme positive delta near the current margin forecasts where risk nodes will appear at higher amplitudes.

---

## Relationship Matrix

How the five concepts compare across the four key dimensions.

| Dimension | Closure Basin | Cost Basin | Coherence Basin | Sensitivity Signature | Risk Nodes |
|---|---|---|---|---|---|
| **Primary axis** | Budget (step count) | Budget (step count) | Precision (step size) | Parameter (named delta) | Budget (step count) |
| **Domain** | Full evaluation domain | Closure basin interior only | Full scene domain | Closure basin at both baseline and test | Complement of closure basin |
| **Scalar or spatial** | Both (% scalar + map) | Scalar field (map) | Spatial region (binary) | Signed scalar field (map) | Both (count + map) |
| **Changes with budget?** | Yes (expands) | Yes (reshapes) | No (budget-invariant) | No (parameter-relative) | Yes (contracts) |
| **Changes with parameter?** | May shift boundary | Yes (signature captures this) | May shift boundary | Yes, by definition | May expand or contract |
| **Defined at risk nodes?** | No (they are exterior) | No | May be | No | Yes (they are the subject) |

---

## Containment and Derivation Hierarchy

```
Full evaluation domain
  ├── Closure Basin (evaluated pixels that converged within budget)
  │     ├── Cost Basin (scalar traversal cost at each converged pixel)
  │     │     └── Sensitivity Signature (delta between two cost basin snapshots)
  │     └── Coherence Basin (subset: where convergence is topologically stable)
  │           └── Boundary: where Sensitivity Signature shows sharp discontinuities
  └── Risk Nodes (evaluated pixels that did not converge within budget)
        ├── Budget-reducible (inside coherence basin; resolve with more budget)
        └── Persistent (Zeno citation candidates; outside coherence basin)
```

**Reading the hierarchy:**

- The Cost Basin cannot exist without the Closure Basin; it is a stratification of its interior.
- The Sensitivity Signature cannot exist without two Cost Basins; it is their signed difference.
- The Coherence Basin is not derived from the Closure or Cost Basin; it is an independent topological classification that partially overlaps them.
- Risk Nodes are the residual of the Closure Basin; they are identified by closure failure, not by a separate measurement.

---

## Glossary Extensions to Observer Language v1

The following terms are used in this atlas and extend the Observer Language v1 vocabulary:

**Basin shift event** — When a parameter change causes a pixel to enter or leave the Closure Basin. This is not captured by the Sensitivity Signature (which requires the pixel to be inside both basins); it must be reported as a separate observation. A basin shift event is more significant than a cost delta: the pixel changed convergence status, not just cost.

**Budget-reducible risk node** — A risk node that resolves when the step budget increases. It is inside the coherence basin; the budget was the limiting factor. Distinct from a persistent risk node.

**Persistent risk node** — A risk node that remains outside the closure basin regardless of budget increase within the tested range. A candidate for Zeno citation (Z-B: Budget Zeno). Evidence for being outside the coherence basin.

**Cost peak** — A local maximum in the Cost Basin: a pixel or region where traversal cost is higher than all neighboring pixels. Cost peaks near the closure basin boundary are risk node candidates. Cost peaks in the basin interior may indicate field structure (e.g., a caustic, a seam, a field boundary).

**Closure basin margin** — The boundary layer of the Closure Basin, containing pixels at high budget stress (approaching or using the overrun step). The margin is the transition zone between the stable interior and the risk node exterior. Budget stress heatmaps visualize the margin.

**Activation delta** — The value of the Sensitivity Signature at a specific pixel: the signed difference in traversal cost between the test parameter value and the baseline. Activation delta > 0 (red) means more steps at test value; activation delta < 0 (blue) means fewer steps; activation delta = 0 (black) means no measurable change.
