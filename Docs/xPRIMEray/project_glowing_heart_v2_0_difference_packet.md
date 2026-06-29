# Project Glowing Heart v2.0 — Difference Packet

**Status:** Vision
**Milestone:** v2.0
**Claim boundary:** No parity claim. No runtime executed. No pixel comparison.

---

## What a Difference Packet is

A Difference Packet is a measurement artifact produced by comparing two declared snapshots under a stated transform.

It is not evidence that two systems agree. It is not evidence that two systems disagree. It is a record of what the numerical difference between two specific channels looked like, under specific conditions, with specific declared assumptions. It says precisely that and nothing more.

A Difference Packet without declared transforms, declared channel identities, and declared claim boundaries is not an artifact. It is an unattributed number.

---

## Why Difference Packets exist

Two snapshots can exist in the same repository and mean entirely different things.

The Core CLI produces `bend_magnitude_metric` — a scalar measurement of ray deflection in a GRIN field. One value per ray, recording how much the ray's direction changed as it traversed the field.

Godot produces some form of `rgb_render` or `rendered_intensity` — a display-referred color or luminance value per pixel, depending on shading, materials, exposure, and tone mapping decisions made inside the renderer.

If someone subtracts one from the other and calls the result a "difference image," they have compared ray deflection against display color. The coordinate grid may align. The resolution may match. The numbers will produce a result. That result will not mean anything.

Difference Packets exist so this cannot happen silently. Before any numerical comparison is permitted, both channels must declare: what they measure, what units they use, what normalization was applied, what color model they encode, and whether comparison to a specific target channel is `COMPARABLE`, `NOT_COMPARABLE`, `REQUIRES_TRANSFORM`, or `UNKNOWN`. Only a pair of channels where both sides declare `COMPARABLE` or `REQUIRES_TRANSFORM` (with the transform cited) may produce a Difference Packet that supports a claim.

---

## The prerequisite chain

A Difference Packet requires all of the following to exist first.

**Shared Observer Contract** — both observations must be taken by the same declared observer, or observers whose parameters are reconciled. Observer position, forward vector, field of view, resolution, near/far, and coordinate conventions must match or be explicitly accounted for. See [v1.7](project_glowing_heart_v1_7_shared_observer_contract.md) and [v1.8 alignment](project_glowing_heart_v1_8_milestone.md).

**Shared Snapshot Measurement Contract** — each snapshot channel must carry a declared identity, producer, channel type, units, normalization, dynamic range, and color model before any comparison is attempted. See [v1.9](project_glowing_heart_v1_9_shared_snapshot_measurement_contract.md) and the schema at [schemas/glowing_heart/shared_snapshot_measurement_contract.v0.preview.json](../../schemas/glowing_heart/shared_snapshot_measurement_contract.v0.preview.json).

**Declared comparison policy** — the measurement contract for each channel declares a `comparisonPolicy` with a `defaultStatus`, a `sameChannelStatus`, and explicit `crossChannelRules` for every known pairing. If no rule covers a pairing, the `missingRuleStatus` applies (currently `UNKNOWN`).

**Declared transform, if required** — if a pairing is `REQUIRES_TRANSFORM`, the transform must be specified as a citable artifact before comparison proceeds. "Approximately equivalent" is not a transform.

Only when all four preconditions are met does computing a difference produce a Difference Packet rather than a meaningless table of numbers.

---

## COMPARABLE, NOT_COMPARABLE, UNKNOWN

These are not judgments about whether a comparison would be interesting. They are statements about whether a comparison is currently defined.

**COMPARABLE** — The two channels measure the same observable under the same declared conventions. Subtracting one from the other produces a value whose meaning is stated. The comparison is supported.

Example: two `bend_magnitude_metric` channels from the same fixture under the same observer, produced by Core at two different times. If the results are deterministic, the difference should be zero. If it is not zero, that is evidence of non-determinism. The comparison means something because both channels measure the same thing.

**NOT_COMPARABLE** — The two channels measure different observables, or apply incompatible normalization, or use incompatible color models, and no transform is currently defined that bridges them. Subtracting one from the other produces a number whose meaning is not stated.

Example: Core `bend_magnitude_metric` vs Godot `rgb_render`. Ray deflection and display color. These are not the same observable. A pixel-by-pixel difference between them would produce a result. That result would not tell you whether the transport is equivalent, whether the rendering is equivalent, or whether anything is working correctly. It would tell you how far apart two unrelated numbers are at each pixel coordinate.

The `NOT_COMPARABLE` label protects the evidence trail from this. It is not a claim that a transform could never be defined. It is a statement that no transform is currently defined, and therefore no comparison currently exists.

**UNKNOWN** — The comparison policy has not been declared for this pairing. Neither a `COMPARABLE` nor a `NOT_COMPARABLE` ruling is available. The `missingRuleStatus` in the current measurement contract schema is `UNKNOWN`, which means unspecified cross-channel pairings do not silently default to comparable. They default to needing explicit review.

`UNKNOWN` does not mean "probably fine." It means the question has not been answered. A Difference Packet cannot be issued for a `UNKNOWN` pairing.

---

## The Difference Packet is a measurement artifact, not a validation artifact

A Difference Packet records what happened when two comparable channels were numerically compared. It does not certify that either system is correct.

Validation asks: does this system produce results that match a known ground truth? A Difference Packet does not answer that question. It answers a different one: given two systems that declare the same measurement channel under the same observer, how much did their outputs differ?

If Core and Godot both produced `bend_magnitude_metric` channels under the same declared observer, a Difference Packet could be produced and would record the per-pixel difference in ray deflection magnitude. A small difference might indicate that the transport implementations are close. A large difference might indicate that they use different integration steps, different field models, or different coordinate origins.

None of that is validation. That is measurement comparison — useful for diagnosing implementation differences, checking for regressions, and understanding where the systems agree and where they do not. Those are valuable goals. They are not the goal of physics validation, scientific calibration, or proof of correctness.

The Difference Packet is a tool in the evidence trail. It belongs in the Atlas's observer grammar at the **Artifact** and **Interpretation** nodes — not at the **Claim Boundary** node, which must be declared separately.

---

## Observer Grammar placement

Using the [Observation Atlas Observer Grammar](../Observatory/Observation_Atlas/ATLAS_CONSTITUTION.md):

```
Phenomenon       — optical transport through a declared GRIN fixture
Field            — spatially varying refractive index field n(r)
Transport        — ray integration under the declared integrator and step policy
Interaction      — ray termination at hit, miss, or boundary crossing
Receiver         — the declared observer: position, forward, FOV, resolution
Measurement      — channel-typed sample per pixel coordinate; declared units, normalization, dynamic range
Artifact         — snapshot.ppm / snapshot_heatmap.csv / difference.ppm / difference_heatmap.csv
Interpretation   — per-pixel difference magnitude; summary statistics (max, mean, percentiles)
Claim Boundary   — declares what the difference supports, infers, or leaves unknown
```

The Difference Packet is the artifact produced between **Measurement** and **Interpretation**. It does not exist before two fully-declared measurements exist. It does not validate claims on its own — the Claim Boundary node must be filled in separately for each Difference Packet.

---

## Representation grammar

The [Representation Principle](../Observatory/Observation_Atlas/ATLAS_CONSTITUTION.md) (Rule 3) applies here directly.

Two snapshots that look similar may represent different observables. Two snapshots that look different may represent the same observable under different normalization. Appearance does not determine comparability.

A Difference Packet derived from `bend_magnitude_metric` and `rgb_render` and visualized as a false-color heat map would look like a result. It would not be one. The visualization would represent the difference between two unrelated numbers at each coordinate. That representation would be coherent as a picture and meaningless as evidence.

The Representation Principle requires that every artifact identify:

- **What observable it encodes** — which channel type, which units
- **What normalization was applied** — before or after differencing
- **What color model the visualization uses** — and whether that model encodes information or is decorative
- **What the artifact supports as a claim** — and what it does not

A Difference Packet that does not declare all four of these is not a Difference Packet. It is an image file.

---

## Relationship to the Measurement Contract

The [Shared Snapshot Measurement Contract](project_glowing_heart_v1_9_shared_snapshot_measurement_contract.md) is the prerequisite document for every Difference Packet. The contract schema at [schemas/glowing_heart/shared_snapshot_measurement_contract.v0.preview.json](../../schemas/glowing_heart/shared_snapshot_measurement_contract.v0.preview.json) defines:

- `channelType` — the controlled vocabulary of what a channel measures (`bend_magnitude_metric`, `rendered_intensity`, `rgb_render`, `depth`, `hit_miss`, `closure_state`, `unknown`)
- `comparisonPolicy.defaultStatus` — the standing policy when no specific rule applies
- `comparisonPolicy.sameChannelStatus` — what is permitted when both sides declare the same channel type
- `comparisonPolicy.crossChannelRules` — explicit rulings for specific pairings
- `comparisonPolicy.missingRuleStatus` — what applies when no rule covers a pairing (`UNKNOWN`)
- `claimBoundary.allowsPixelComparison` — boolean; false on all current preview contracts
- `claimBoundary.allowsParityClaim` — const false; may never be true without a separate validation artifact

A Difference Packet at v2.0 will cite the measurement contract for each channel it compares. The contract is part of the artifact record, not a background assumption.

---

## Claim boundary for this document

**Supported:** The design rationale for Difference Packets is stated. The prerequisite chain is declared. The channel vocabulary is defined in the v1.9 schema.

**Inferred:** The artifact format (difference.ppm, difference_heatmap.csv) follows by analogy from the v0.6 snapshot format. The exact schema for a Difference Packet has not been written yet.

**Unknown:** Whether Core and Godot will produce the same channel type under the same observer in practice. Whether the per-pixel differences will be within a meaningful range. Whether any transform exists that would make `bend_magnitude_metric` and `rgb_render` comparable.

No Difference Packet has been produced. No comparison has been run. This document records the reasoning that governs how comparison will be done when it is eventually attempted.

---

*Cross-references:*
*[Observation Atlas](../Observatory/Observation_Atlas/README.md) — [Atlas Constitution](../Observatory/Observation_Atlas/ATLAS_CONSTITUTION.md) — [v1.7 Observer Contract](project_glowing_heart_v1_7_shared_observer_contract.md) — [v1.8 Milestone](project_glowing_heart_v1_8_milestone.md) — [v1.9 Measurement Contract](project_glowing_heart_v1_9_shared_snapshot_measurement_contract.md) — [Measurement Contract Schema](../../schemas/glowing_heart/shared_snapshot_measurement_contract.v0.preview.json)*
