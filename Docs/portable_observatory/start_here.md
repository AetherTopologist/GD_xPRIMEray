---
po_doc_type: learn
title: Portable Observatory — Start Here
status: partial
engine_commit: "5ce15c13"
generated: false
---

# Start Here

Welcome to the **Portable Observatory** documentation lane.

This is not a game walkthrough and not a claim that the engine proves general relativity. It is a lab manual for **simulated transport observation**: what you can measure, what you can only display, and what you must not conclude from color alone.

## Five minutes

1. Read the [Portable Observatory landing](index.md) spine and ownership table.
2. Internalize [Display Is Not Probe](learn/display_is_not_probe.md).
3. Skim [Public Vocabulary](reference/public_vocabulary.md) for **Transport Lens**, **Observation Plate**, **Display Mode**, **Region Probe**.

## Twenty minutes

| Step | Page | Why |
|------|------|-----|
| 1 | [Anatomy of an Observation Frame](learn/anatomy_of_observation_frame.md) | What a sealed measurement package contains |
| 2 | [Reading the Outcome Plane](learn/reading_outcome_plane.md) | Authoritative semantic classes |
| 3 | [The Great Magenta Confusion](learn/great_magenta_confusion.md) | Why plate magenta is not an outcome |
| 4 | [Snapshot Completeness vs Resolution](learn/snapshot_completeness_vs_resolution.md) | Complete ≠ solved |
| 5 | [Hermetic Normal Calibration](experiments/hermetic_normal_calibration.md) | Classroom for closure and orientation |

## Operator path (live host)

```text
Pick Scene → enable Transport Lens (G) → set Display Mode (N)
  → read Observation Plate → open Inspector (Tab)
  → only then Region Probe (P) on Complete SNAPSHOT
  → Evidence Console (Esc) for recipes
```

See [Controls](reference/controls.md) and [Running the Observatory](development/running_the_observatory.md).

## What this lane will not do

- Treat magenta as “no hit”
- Treat deeper Region Refinement as automatic truth
- Treat NormalRGB as sealed `geometry.normal`
- Treat fixture PASS as live free-roam qualification
- Speak planned channels (`geometry.*`, `transport.path_length`, …) as already shipped

Continue: [Portable Observatory home](index.md).
