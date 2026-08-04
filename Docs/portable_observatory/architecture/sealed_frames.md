---
po_doc_type: architecture
title: Sealed Frames
status: partial
engine_commit: "5ce15c13"
generated: false
---

# Sealed Frames

A **sealed observation frame** is a host-neutral measurement package:

```text
context + dimensions + policy fingerprint
  + one or more sealed channels
  + validity / claim metadata
```

**Internal:** `SealedObservationFrame`, `ObservationFrameDescriptor`, `ISealedObservationChannel`.

---

## Completeness

| Requirement | Why |
|-------------|-----|
| Lifecycle Complete | Frame finished |
| Unprocessed = 0 | Full-frame claims only |
| Histogram sum = total samples | Consistency |
| Context match | Refine/compare validity |

**Complete frame ≠ successful geometry resolution.** See [Snapshot Completeness vs Resolution](../learn/snapshot_completeness_vs_resolution.md).

---

## Channels present today

See [Observation Channels](../reference/observation_channels.md):

- `cathedral.probe.outcome`
- `cathedral.probe.region_label`
- `cathedral.probe.refinement_level`

---

## Evidence carrier

Sealed frames (plus summaries) are the preferred **evidence carrier**. Terminal `[LiveSummary]` lines are runtime telemetry—not automatic qualification evidence. See [Diagnostics](diagnostics.md) and [Evidence Doctrine](../evidence/evidence_doctrine.md).

---

## See also

- [Anatomy of an Observation Frame](../learn/anatomy_of_observation_frame.md)
