---
po_doc_type: learn
title: Snapshot Completeness vs Resolution
status: partial
engine_commit: "5ce15c13"
channel_id: cathedral.probe.outcome
validity_condition: "Completeness is lifecycle + Unprocessed=0; resolution is semantic transition under refine policy."
claim_boundary: "A Complete snapshot is not proof of correct geometry, beauty success, or physical closure."
contract_links:
  - CathedralProbe/ProbeSnapshotLifecycleModel.cs
  - CathedralProbe/ProbeOutcomeCode.cs
generated: false
---

# Snapshot Completeness vs Resolution

!!! abstract "Observation card"
    | Field | Value |
    |-------|-------|
    | Status | **partial** |
    | Verified against | lifecycle `ef29ff79`; refine semantics `5b9df901`; sealed frames `6e69d792` |
    | Channel | `cathedral.probe.outcome` (+ refinement when applicable) |
    | Claim boundary | Complete ≠ solved; resolved ≠ true |

---

## Two different words

| Term | Means | Does **not** mean |
|------|-------|-------------------|
| **Complete** (snapshot lifecycle) | Frame finished pumping; **Unprocessed = 0**; histogram sums to sample count; context consistent | Pretty plate · all HitGeometry · closed enclosure proven |
| **Resolved** (refinement sense) | Sample was **`MaxStepsExhausted` before** and became **`HitGeometry` or `BackgroundResolved` after** | Still max-steps · absorbed-secondary · fault · “found the wormhole” |
| **Unresolved Region** | Connected **unresolved-budget** samples (default: max-steps) under policy | Magenta blob · empty space · exotic topology |

---

## Completeness checklist

A full-frame scientific claim needs:

1. Lifecycle state **Complete** (not timeout-as-pretty).
2. `Unprocessed = 0`.
3. Histogram sum = width × height.
4. Matching context key (pose, field policy, dimensions, generation).
5. Prefer evidence emission once per successful generation.

A plane that is **all** `MaxStepsExhausted` or **all** `BackgroundResolved` can still be **Complete**. That is a finished measurement of “we did not get surface hits,” not a broken snapshot.

---

## Resolution ladder (policy-relative)

```text
MaxStepsExhausted  --(Region Refinement / more Transport Effort)-->
    HitGeometry  or  BackgroundResolved   ⇒  "resolved" under Stage 0 definition
```

Anything else after refine (still max-steps, absorbed, fault, invalid) is **not** resolved in that strict sense.

!!! warning "Doctrine"
    **Deeper refinement ≠ greater truth.** Raising Transport Effort is more numerical attempt under policy—not automatic discovery of correct physics or topology.

---

## Common confusions

| Phrase people say | Prefer |
|-------------------|--------|
| “Snapshot failed because the plate is magenta” | Check lifecycle + outcome histogram |
| “Complete means the room closed” | Completeness is frame lifecycle; **Closure** is a separate enclosure test |
| “Resolved means wormhole” | Resolved means class transition under policy only |
| “Physics wait = MaxStepsExhausted” | Timeout/incomplete ≠ budget exhaustion class |

---

## Related

- [Anatomy of an Observation Frame](anatomy_of_observation_frame.md)
- [Tuning the Region Probe](../experiments/tuning_the_region_probe.md)
- [Claim Boundaries](../reference/claim_boundaries.md)
