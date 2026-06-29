# Atlas Constitution

**Observation Atlas — Field Rules**

These are the standing rules for how Atlas entries are written and read. They do not change with milestone numbering. If a page conflicts with a rule here, the page should be corrected — not the rule relaxed for convenience.

This document is not a milestone, a roadmap, or a marketing artifact.

---

## Rule 1 — Observation first

Start with what is being observed, not with how pretty the output looks.

An image is one kind of artifact an observation may leave behind. The observation itself is: a receiver encountering a phenomenon under stated conditions, producing a measurement with stated units, uncertainty, and scope.

The Atlas does not begin with what something looks like. It begins with what is being observed, by what, under what conditions, and what the receiver actually records. Images, renders, visualizations, and diagrams appear later in the chain — as artifacts of a completed observation, or as representations of a hypothetical one.

This rule prevents any entry from substituting visual appeal for observational rigor.

---

## Rule 2 — Observer grammar

Every observer in the Atlas answers the same sequence of questions. No entry is complete until every node in the grammar is addressed or explicitly marked unknown.

```
Phenomenon
  → Field             (what physical or computational field carries the phenomenon?)
  → Transport         (how does the carrier evolve through space, time, or parameter?)
  → Interaction       (what happens when the carrier meets the observer boundary?)
  → Receiver          (what records the interaction, and with what sensitivity and range?)
  → Measurement       (what value is recorded, in what units, under what calibration?)
  → Artifact          (what persistent object does the measurement produce?)
  → Interpretation    (what claim does the artifact support, and with what confidence?)
  → Claim Boundary    (what is supported, what is inferred, what is unknown?)
```

An observer entry that stops at Artifact and does not reach Interpretation and Claim Boundary is incomplete. An observer entry that reaches Claim Boundary and declares Supported where the evidence supports only Inferred is an incomplete or misleading entry.

---

## Rule 3 — Representation principle

The Atlas organizes representations of observations, not reality itself.

A biological observer entry describes how the human retina represents a scene, not what the scene is. A computational observer entry describes how a ray integrator represents a field traversal, not what the field is. A scientific instrument entry describes how a detector represents a signal, not what the phenomenon is.

This distinction matters because representations contain choices: what to sample, what to discard, what to normalize, what to encode. Two representations of the same phenomenon that make different choices are not the same representation, and their artifacts are not directly comparable without a documented transform that accounts for the differing choices.

No representation is neutral. Every entry in the Atlas should make its representation choices explicit.

---

## Rule 4 — Map, not ranking

The Atlas is a map of observation systems, not a ranking of technologies.

It records how observer families relate: shared grammar nodes, overlapping vocabulary, where comparison needs a declared transform, and where comparison is not supported by current evidence.

A biological observer and a computational observer may share a field in the Observer Grammar — both have a Receiver, for instance — without this implying equivalence. The shared grammar node is a navigational tool. It is not a claim that retinal transduction and ray integration are the same process.

Entries that position one observer family as superior to another, or that imply a hierarchy of observational validity, should be corrected.

---

## Rule 5 — Evidence ladder

Every capability claim in the Atlas is placed at a specific maturity level. No claim may exceed its evidence.

```
Vision             — Direction stated; no implementation exists
Prototype          — Minimal implementation exists; scope is narrow
Experimental       — Implementation produces inspectable results; determinism verified
Internal Validation — Results checked against a declared reference under stated conditions within this project's evidence trail
Public Demo        — Results demonstrated publicly; claim scope declared and bounded
Research Ready     — Methodology documented; results reproducible by a reader following the record
Production Ready   — Stable, maintained, suitable for external dependence under declared scope
```

Internal Validation is not a public physics-validation or parity claim unless explicitly scoped and cited.

Moving a claim up the ladder requires new evidence, not revision of the label. Labeling a Vision-level capability as Experimental because it is aspirationally plausible is an incomplete or misleading entry. Labeling an Experimental capability as Vision because the scope is narrow also suppresses evidence and should be corrected.

When two documents in the Atlas assign different maturity levels to the same capability, both documents should be corrected. The disagreement is an error in both, not a choice between two valid views.

---

## Rule 6 — Claim boundary

Every entry classifies what may be said today:

**Supported** — Inspectable evidence exists in this repository or a cited external source. A reader can follow the reference and verify the claim independently.

**Inferred** — Reasoning from supported evidence is stated and checkable, but independent replication or measurement may not exist yet. The basis for the inference is declared.

**Unknown** — Evidence or comparison basis is absent. Either the measurement has not been performed, or the comparison basis has not been established. Unknown is honest scope, not failure.

No fourth category exists. Absence of evidence is Unknown, not Supported. Plausibility is not evidence. Analogy is not a documented transform. A claim that is visually compelling but not backed by inspectable evidence is Unknown until evidence is produced.

The phrase "no parity claim" in Atlas documents means: no claim that two observer outputs are equivalent, comparable, or interchangeable has been made or is implied, unless a documented transform and evidence basis are cited.

---

## Rule 7 — Translation principle

The Atlas provides a shared descriptive language across biological, optical, scientific, computational, and educational observer systems without assuming equivalence between them.

The Observer Grammar (Rule 2) is a translation frame. It allows a human retina, a scanning electron microscope, a GRIN ray integrator, and an educational orbital diagram to be described in the same structural vocabulary. This shared vocabulary makes relationships visible and comparisons explicit.

Translation is not equivalence. Describing a GRIN transport observer and a spectrometer in the same grammar does not imply that bend magnitude and spectral channel are the same kind of measurement. It implies that both have a Receiver, a Measurement, an Artifact, and a Claim Boundary — and that the differences between them can be stated precisely within that shared frame.

Any bridge drawn between observer families in the Atlas must be supported by a declared transform, cited evidence, or an explicit statement that the bridge is speculative and at Vision maturity.

---

## Closing note

The Atlas is meant to invite careful comparison: same questions, different receivers, different limits. Bridges between territories are welcome when they cite evidence, declare transforms, or plainly mark speculation at Vision maturity. Analogy alone is not a bridge.