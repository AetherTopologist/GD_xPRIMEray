---
po_doc_type: architecture
title: Weekend Visual Milestone — Documentation Package
status: partial
engine_commit: "6e69d792"
generated: false
---

# Weekend Visual Milestone — Documentation Package

**Prepared:** 2026-08-01  
**Verified against:** `6e69d792` (portable observation + sealed frames; lifecycle `ef29ff79`; refine `32aa9ab9` / `5b9df901`)  
**Code changes:** none in this package (docs only).

## Pages

| # | Page | Path | Status |
|---|------|------|--------|
| 1 | Anatomy of an Observation Frame | [learn/anatomy_of_observation_frame.md](learn/anatomy_of_observation_frame.md) | partial |
| 2 | Reading the Outcome Plane | [learn/reading_outcome_plane.md](learn/reading_outcome_plane.md) | implemented |
| 3 | Hermetic Normal Calibration | [experiments/hermetic_normal_calibration.md](experiments/hermetic_normal_calibration.md) | partial |
| 4 | The Great Magenta Confusion | [learn/great_magenta_confusion.md](learn/great_magenta_confusion.md) | partial |
| 5 | Tuning the Cathedral Probe | [experiments/tuning_the_cathedral_probe.md](experiments/tuning_the_cathedral_probe.md) | partial |

## Implemented channel IDs (do not invent others as shipped)

- `cathedral.probe.outcome`
- `cathedral.probe.region_label`
- `cathedral.probe.refinement_level`

## Planned (label clearly in all public figures)

- Spatial field-strength / gradient channels  
- Continuous final-step-count heatmap  
- Path-length heatmap  
- Portable raw normal / depth buffers  
- Full Godot outcome/region overlay UX  

## Nav hook (when wiring mkdocs)

Add under `Portable Observatory` → Learn / Experiments as in the documentation architecture plan. Not applied to `mkdocs.yml` in this package.
