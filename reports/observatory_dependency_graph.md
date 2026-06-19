# Observatory Dependency Graph

**Role:** Observatory Architect  
**Date:** 2026-06-16  
**Version:** 1.0  
**Constraint:** No new concepts created. No renderer changes. Ontology minimization is the primary goal.

---

## The 15 Named Concepts — Classified

| # | Concept | Category | Freeze status |
|---:|---|---|---|
| 1 | Coverage Basin | FOUNDATIONAL | Experimental |
| 2 | Closure Basin | DERIVED | **Freeze** |
| 3 | Ownership Basin | DERIVED | Experimental |
| 4 | Cost Basin | DERIVED | Experimental |
| 5 | Disagreement Basin | DERIVED | Experimental |
| 6 | Sensitivity Basin | DERIVED | Experimental |
| 7 | Risk Nodes | DERIVED | Experimental |
| 8 | Coherence Basin | CROSS-CUTTING | Experimental |
| 9 | Trust Model | CROSS-CUTTING | **Freeze** |
| 10 | Adversarial Reviews | CROSS-CUTTING | Experimental |
| 11 | Observer Storyboard | PRESENTATION | **Freeze** |
| 12 | Observatory Storyboard | PRESENTATION | Experimental |
| 13 | Maturity Ladder | PRESENTATION | Experimental |
| 14 | Basin Atlas | PRESENTATION | Experimental |
| 15 | Query Observatory | PRESENTATION | Experimental |

---

## Category Definitions

**FOUNDATIONAL** — Cannot be derived from any other named concept. Every other concept in the Observatory either directly or transitively requires this as input.

**DERIVED** — Has a formal computational or logical dependency on one or more other concepts. Cannot exist without its parent(s). The derivation chain is deterministic and fully specifiable.

**CROSS-CUTTING** — Does not fit the containment hierarchy. Applies to or qualifies all other concepts without being derived from them. Neither produced by the basin chain nor consumed by it.

**PRESENTATION** — Consumes basin outputs and Trust Model stages to generate visitor-facing or machine-readable artifacts. Does not produce new ontological content; rearranges and renders what exists.

---

## Dependency Graph (ASCII)

```
FOUNDATIONAL
─────────────────────────────────────────────────────────────────
  Coverage Basin
    (all film pixels that were evaluated at all)

DERIVED — containment chain
─────────────────────────────────────────────────────────────────
  Coverage Basin
    └─[contains]──► Closure Basin
                      ├─[contains domain of]──► Ownership Basin
                      │                           └─[×2, sym. diff.]──► Disagreement Basin
                      └─[contains domain of]──► Cost Basin
                                                  └─[×2, signed delta]──► Sensitivity Basin
  Risk Nodes = Coverage Basin interior ∖ Closure Basin interior
  (a derived set, not a basin; no sub-derivations)

CROSS-CUTTING — orthogonal to chain
─────────────────────────────────────────────────────────────────
  Coherence Basin ────────── qualifies topological stability
    (cuts across all six computational basins; requires refinement
     sequence; cannot be derived from a single run)

  Trust Model ────────────── evidence-strength axis 0–5
    (applies to all 15 concepts + all artifacts; self-applicable)

  Adversarial Reviews ─────── QA process for language overclaiming
    (applied to all Observatory-facing documents; not produced by
     any basin, not required to compute any basin)

PRESENTATION — consume, do not produce
─────────────────────────────────────────────────────────────────
  Observer Storyboard ◄──── populates from basin outputs + Trust Model
    └─[instance of]──► Observatory Storyboard (xPRIMEray-specific)
                            └─[artifact instance]──► Query Observatory

  Trust Model ◄──── rendered as ──► Maturity Ladder (thin view)
  Basin Taxonomy v1 ◄──── supersedes ──► Basin Atlas (reference only)
```

---

## Formal Edge List

| From | Edge type | To | Notes |
|---|---|---|---|
| Coverage Basin | `[contains]` | Closure Basin | Closure defined only within Coverage interior |
| Closure Basin | `[contains domain of]` | Ownership Basin | Ownership undefined outside Closure interior |
| Closure Basin | `[contains domain of]` | Cost Basin | Cost undefined outside Closure interior |
| Ownership Basin | `[×2, symmetric diff]` | Disagreement Basin | Requires two Ownership Basins over the same Coverage interior |
| Cost Basin | `[×2, signed delta]` | Sensitivity Basin | Requires two Cost Basins; pixel must be in both Closure Basins |
| Coverage Basin | `[complement within] - [Closure Basin interior]` | Risk Nodes | Not a basin; derived set; no sub-derivations |
| Coherence Basin | `[qualifies]` | All six computational basins | Topological stability criterion; precision-invariant; multi-run |
| Trust Model | `[scores]` | All 15 concepts + artifacts | Evidence-strength axis 0–5; self-applicable |
| Trust Model | `[rendered as]` | Maturity Ladder | Maturity Ladder is a thin presentation view of the Trust Model |
| Observer Storyboard | `[instance of]` | Observatory Storyboard | Observatory Storyboard is xPRIMEray-specific 9-panel contact sheet |
| Observatory Storyboard | `[artifact instance]` | Query Observatory | Query Observatory is one run artifact, not a concept |
| Basin Taxonomy v1 | `[supersedes]` | Basin Atlas | Basin Atlas is retained as relationship reference; taxonomy absorbs its logic |
| Adversarial Reviews | `[QA process on]` | All Observatory documents | Cross-cutting language-quality process |

---

## Circular Dependency Analysis

**Result: No circular dependencies.**

The computational chain flows strictly in one direction:

```
Coverage → Closure → {Ownership, Cost} → {Disagreement, Sensitivity}
```

No concept in this chain requires a concept that is downstream of it.

**One redundancy noted (not a cycle):**

Trust Model and Maturity Ladder both define the same 0–5 stage vocabulary. This is redundancy, not circularity. The Maturity Ladder is a presentation layer — it reads from the Trust Model, it does not feed back into it.

**One self-reference noted (not a cycle):**

The Trust Model can be applied to itself (the Trust Model as an artifact has a maturity score). Self-reference is not circularity; there is no dependency loop.

---

## Minimum Concept Set

**Question: What is the smallest concept set that can explain the current Observatory?**

**Answer: 6 concepts.**

| Minimum concept | Role in minimum set |
|---|---|
| Coverage Basin | The only FOUNDATIONAL concept; defines the observation domain |
| Closure Basin | The gateway to all per-pixel measurements; anchors closure language |
| Ownership Basin | Required to express "what did the ray hit?"; source of Disagreement Basin |
| Cost Basin | Required to express "how hard was evaluation?"; source of Sensitivity Basin |
| Trust Model | Required to assign evidence-strength to any artifact or claim |
| Observer Storyboard | Required to present basin outputs as a structured evidence product |

From these six, every other named concept is either:
- **Derived**: Disagreement Basin, Sensitivity Basin, Risk Nodes, Coherence Basin (topological qualifier on the above)
- **A presentation instance**: Observatory Storyboard, Query Observatory, Maturity Ladder
- **A superseded reference**: Basin Atlas
- **A QA process**: Adversarial Reviews

The Disagreement and Sensitivity Basins are not in the minimum set because they require two instances of their parent basins — they are comparative concepts, not foundational ones. They cannot be demonstrated from a single run.

Coherence Basin is excluded from the minimum set because it requires a refinement sequence (multiple runs at different step sizes). It qualifies the minimum set but is not a prerequisite for defining it.

---

## Concepts Without Unique Roles

| Concept | Role held by another concept | Recommendation |
|---|---|---|
| Maturity Ladder | The Trust Model already defines and owns the 0–5 scale. The Maturity Ladder's only unique content is the Current Anchors table (per-artifact score assignments). | Retain as a presentation view; mark as thin wrapper. Do not extend independently — any scale changes must originate in the Trust Model. |
| Basin Atlas | Basin Taxonomy v1 absorbs and supersedes its relationship logic. The Basin Atlas remains useful as a compact reference card generated from `reports/observatory_catalog.json`. | Retain as a generated artifact, not as the ontology source. Source of truth is Basin Taxonomy v1. |
| Observatory Storyboard | It is an instance of Observer Storyboard, not a distinct concept. Its panel names are xPRIMEray-specific labels over the Observer Storyboard framework. | Retain as an instance definition; do not define new framework capabilities in it. |
| Query Observatory | It is a single artifact instance (one run, one storyboard), not a concept. It has no architectural authority. | Retain as an example artifact; do not promote to maturity Ladder anchor unless a stable catalog or generation gate is established. |

---

## Freeze vs. Experimental Recommendations

### Freeze (stable — changes require strong justification)

| Concept | Reason for freeze |
|---|---|
| **Closure Basin** | Schema defined, artifacts generated per run, panel in Observer Storyboard v1, documented caveats (Characterized/4). The Observatory's central diagnostic concept. Renaming or redefining it would invalidate all existing documentation. |
| **Trust Model** | The 0–5 stage vocabulary is referenced by all crosswalk tables, all gallery pages, and all artifact scores. It is self-consistent and has no known conflicts. Any change propagates to every downstream artifact label. |
| **Observer Storyboard** | Framework is schema-backed, renderer-agnostic, and instantiated (Observatory Storyboard). Panel names are locked in Observer Language v1. Adding panels would invalidate existing nine-panel artifacts. |

### Experimental (active development, definitions may shift)

| Concept | Reason for experimental status |
|---|---|
| **Coverage Basin** | Named as a basin with a dedicated pipeline only recently. No dedicated validation gate or schema entry in the Trust Model yet. Artifact exists (frame_coverage_map) but basin concept is Proposed/0. |
| **Ownership Basin** | Named concept is Proposed/0. No dedicated pipeline or validation gate. The artifact (transport ownership map) is Observed/2. |
| **Cost Basin** | Named concept is Proposed/0 (per Maturity Ladder). No dedicated Cost Basin artifact generation or validation gate. Underlying traversal heatmap is Observed/2. |
| **Disagreement Basin** | Named concept is Proposed/0. Requires two Ownership Basins; dedicated comparative pipeline not established. The 23.8% divergence study is Observed/2 as an artifact. |
| **Sensitivity Basin** | Named concept is Proposed/0 as a generalized form. The Curvature Signature instance is Characterized/4 but is xPRIMEray-specific. The generalized framework is not yet gate-validated. |
| **Risk Nodes** | Derived set, not a basin. Formally defined but without a dedicated artifact pipeline or validation gate. |
| **Coherence Basin** | Requires refinement sequence — cannot be established from a single run. No pipeline, no gate, no existing artifact with confirmed Coherence Basin reading. |
| **Adversarial Reviews** | QA process exists in practice (it produced these audits) but is not formally specified as a process with defined scope, cadence, or output format. |
| **Observatory Storyboard** | Instance definition is stable but the per-panel naming uses Observer Language v1 names that were finalized only recently. Validate all artifact panels against v1 names before promoting. |
| **Maturity Ladder** | Thin wrapper on Trust Model. Keep synchronized; the only unique content (Current Anchors table) must be manually maintained. No automation gate. |
| **Basin Atlas** | Generated artifact. Source of truth moved to Basin Taxonomy v1. Retain for compact reference; update generator to pull from taxonomy rather than defining relationships directly. |
| **Query Observatory** | Single artifact instance. No pipeline, catalog, or generation gate qualifies it as Characterized or above. |

---

## Observable vs. Interpretive Distinction

| Concept | Observable (per-run) | Requires interpretation |
|---|---|---|
| Coverage Basin | Yes — frame_coverage_map is a direct output | No interpretation layer |
| Closure Basin | Yes — hit/miss/budget classification is per-pixel | Scene contract and budget must be declared |
| Ownership Basin | Yes — transport ownership map is a direct output | Receiver zone definitions must be declared |
| Cost Basin | Yes — traversal step count is a direct output | Budget comparison requires step budget declaration |
| Risk Nodes | Yes — complement of Closure within Coverage | Derived automatically from Closure Basin |
| Disagreement Basin | Yes — symmetric difference of two ownership maps | Requires two named perspectives to be declared |
| Sensitivity Basin | Yes — signed delta of two step-count maps | Requires baseline and test parameter declaration |
| Coherence Basin | No — requires refinement sequence | Topological stability is interpretive across runs |
| Trust Model | No — scoring is an assignment by Observatory Architect | Score is a judgment call, not a computed output |
| Adversarial Reviews | No — QA process output is a document | Depends on reviewer judgment |
| Observer Storyboard | Partially — panel population is automated | Verdict (Panel 9) requires human assignment |

---

## Machine-Readable Form

See `reports/observatory_dependency_graph.json` for the full graph in JSON format.

---

## Summary

The Observatory is built on one foundational concept (Coverage Basin) and a strict containment chain. No circular dependencies exist. The minimum concept set that can explain all Observatory artifacts is six concepts. Three concepts are ready to freeze; twelve remain experimental with varying maturity. The concepts most at risk of role duplication (Maturity Ladder, Basin Atlas, Query Observatory) are presentation instances, not architectural concepts — retaining them for visitor access is appropriate as long as they do not define independent ontological claims.
