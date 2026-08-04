---
po_doc_type: architecture
title: Weekend Visual Milestone — Documentation Package (historical)
status: partial
engine_commit: "6e69d792"
generated: false
---

# Weekend Visual Milestone — Documentation Package

**Historical package note** (2026-08-01). Superseded for navigation by the Portable Observatory lane; retained for correction history.

**Prepared:** 2026-08-01
**Original verified against:** `6e69d792`
**Ontology pass:** 2026-08-03 (`5ce15c13` HEAD at pass) — pages expanded and wired in `mkdocs.yml`.

## Original five pages (still present)

| # | Page | Path | Status |
|---|------|------|--------|
| 1 | Anatomy of an Observation Frame | [learn/anatomy_of_observation_frame.md](learn/anatomy_of_observation_frame.md) | partial |
| 2 | Reading the Outcome Plane | [learn/reading_outcome_plane.md](learn/reading_outcome_plane.md) | implemented |
| 3 | Hermetic Normal Calibration | [experiments/hermetic_normal_calibration.md](experiments/hermetic_normal_calibration.md) | partial |
| 4 | The Great Magenta Confusion | [learn/great_magenta_confusion.md](learn/great_magenta_confusion.md) | partial |
| 5 | Tuning the Region Probe | [experiments/tuning_the_region_probe.md](experiments/tuning_the_region_probe.md) | partial |

(Item 5 was originally titled “Tuning the Cathedral Probe”; public default is now **Region Probe**.)

## Implemented channel IDs (do not invent others as shipped)

- `cathedral.probe.outcome`
- `cathedral.probe.region_label`
- `cathedral.probe.refinement_level`

## Planned (label clearly)

See [Observation Channels](reference/observation_channels.md).

## Nav

Portable Observatory is wired in `mkdocs.yml`. Landing: [index.md](index.md).
