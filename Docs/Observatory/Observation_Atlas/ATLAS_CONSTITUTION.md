# Atlas Constitution

**Observation Atlas — Permanent Engineering Philosophy**

This document defines the reasoning principles behind the Observation Atlas. It is not a milestone, a roadmap, or a marketing artifact. It does not change with versioning cycles. When a section, page, capability claim, or visualization conflicts with these doctrines, the doctrine stands and the page requires revision.

---

## Doctrine 1 — Observation First

Observations precede images.

An image is one form of artifact that an observation may produce. It is not the observation itself. The observation is the act of a receiver encountering a phenomenon under declared conditions, producing a measurement with declared units, uncertainty, and scope.

The Atlas does not begin with what something looks like. It begins with what is being observed, by what, under what conditions, and what the receiver actually records. Images, renders, visualizations, and diagrams appear later in the chain — as artifacts of a completed observation, or as representations of a hypothetical one.

This doctrine prevents any entry from substituting visual appeal for observational rigor.

---

## Doctrine 2 — Observer Grammar

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

An observer entry that stops at Artifact and does not reach Interpretation and Claim Boundary is incomplete. An observer entry that reaches Claim Boundary and declares Supported where the evidence supports only Inferred is a defect.

---

## Doctrine 3 — Representation Principle

The Atlas organizes representations of observations, not reality itself.

A biological observer entry describes how the human retina represents a scene, not what the scene is. A computational observer entry describes how a ray integrator represents a field traversal, not what the field is. A scientific instrument entry describes how a detector represents a signal, not what the phenomenon is.

This distinction matters because representations contain choices: what to sample, what to discard, what to normalize, what to encode. Two representations of the same phenomenon that make different choices are not the same representation, and their artifacts are not directly comparable without a documented transform that accounts for the differing choices.

No representation is neutral. Every entry in the Atlas should make its representation choices explicit.

---

## Doctrine 4 — Atlas Principle

The Atlas maps relationships rather than rankings.

The Atlas does not rank observer families by quality, realism, scientific authority, or capability. It maps how observer families relate to one another: what they share in grammar structure, where their measurement vocabularies overlap, where comparison requires a declared transform, and where comparison is not currently supported by evidence.

A biological observer and a computational observer may share a field in the Observer Grammar — both have a Receiver, for instance — without this implying equivalence. The shared grammar node is a navigational tool. It is not a claim that retinal transduction and ray integration are the same process.

Entries that position one observer family as superior to another, or that imply a hierarchy of observational validity, require revision.

---

## Doctrine 5 — Evidence Ladder

Every capability claim in the Atlas is placed at a specific maturity level. No claim may exceed its evidence.

```
Vision             — Direction stated; no implementation exists
Prototype          — Minimal implementation exists; scope is narrow
Experimental       — Implementation produces inspectable results; determinism verified
Internal Validation — Results validated against a declared reference under controlled conditions
Public Demo        — Results demonstrated publicly; claim scope declared and bounded
Research Ready     — Methodology documented; results reproducible by a reader following the record
Production Ready   — Validated, stable, maintained, suitable for external dependence
```

Moving a claim up the ladder requires new evidence, not revision of the label. Labeling a Vision-level capability as Experimental because it is aspirationally plausible is a defect. Labeling an Experimental capability as Vision because the scope is narrow is also a defect — it suppresses evidence.

When two documents in the Atlas assign different maturity levels to the same capability, both documents require revision. The disagreement is a defect in both, not a choice between two valid views.

---

## Doctrine 6 — Claim Boundary

Every observer, artifact, and visualization in the Atlas explicitly declares the standing of its claims across three categories:

**Supported** — The claim is backed by inspectable evidence available in this repository or a cited external source. A reader following the reference can verify the claim independently.

**Inferred** — The claim follows from supported evidence by reasoning that is stated and checkable, but the inference has not been validated by an independent measurement or replication. The basis for the inference is declared.

**Unknown** — The claim cannot currently be made. Either the evidence does not exist, the measurement has not been performed, or the comparison basis has not been established. Unknown is not a negative result. It is an accurate statement of current knowledge.

No fourth category exists. Absence of evidence is Unknown, not Supported. Plausibility is not evidence. Analogy is not a documented transform. A claim that is visually compelling but not backed by inspectable evidence is Unknown until evidence is produced.

The phrase "no parity claim" in Atlas documents means: no claim that two observer outputs are equivalent, comparable, or interchangeable has been made or is implied, unless a documented transform and evidence basis are cited.

---

## Doctrine 7 — Translation Principle

The Atlas provides a shared descriptive language across biological, optical, scientific, computational, and educational observer systems without assuming equivalence between them.

The Observer Grammar (Doctrine 2) is a translation frame. It allows a human retina, a scanning electron microscope, a GRIN ray integrator, and an educational orbital diagram to be described in the same structural vocabulary. This shared vocabulary makes relationships visible and comparisons explicit.

Translation is not equivalence. Describing a GRIN transport observer and a spectrometer in the same grammar does not imply that bend magnitude and spectral channel are the same kind of measurement. It implies that both have a Receiver, a Measurement, an Artifact, and a Claim Boundary — and that the differences between them can be stated precisely within that shared frame.

Any bridge drawn between observer families in the Atlas must be supported by a declared transform, cited evidence, or an explicit statement that the bridge is speculative and at Vision maturity.

---

## Closing Statement

The Atlas welcomes exploration. Every bridge between territories must be supported by inspectable evidence rather than analogy alone.
