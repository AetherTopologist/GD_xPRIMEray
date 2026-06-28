# Optics Landscape

## Scope

This view describes optical-modeling concepts relevant to Project Glowing Heart. It does not compare numerical accuracy, convergence, physical validity, or execution performance.

## Technology Contexts

| Context | Capability vocabulary | Project Glowing Heart evidence |
|---|---|---|
| Geometrical ray optics | ray origin and direction, intersections, path state, observer projection | The shared fixture and observer vocabularies are introduced in [v1.3](../../xPRIMEray/project_glowing_heart_v1_3_shared_fixture_schema.md) and [v1.7](../../xPRIMEray/project_glowing_heart_v1_7_shared_observer_contract.md). |
| Gradient-index optics | spatially varying optical fields and curved ray paths | The Glowing Heart smoke fixture and observable-output trail are documented in [v0.1](../../xPRIMEray/project_glowing_heart_v0_1_baseline.md) through [v0.3](../../xPRIMEray/project_glowing_heart_v0_3_observable_output.md). |
| Lens and optical-system design tools | surfaces, media, apertures, field points, analysis outputs | Shared fixture candidates identify neutral metadata needed for future interchange in [v1.0](../../xPRIMEray/project_glowing_heart_v1_0_shared_fixture_candidate.md). |
| Wave and field solvers | field-domain quantities, boundary conditions, units, sampled outputs | Project Glowing Heart currently documents ray/measurement contracts; no wave-optics equivalence is claimed. |
| Optical measurement pipelines | units, normalization, dynamic range, channel meaning, artifact provenance | [v1.9](../../xPRIMEray/project_glowing_heart_v1_9_shared_snapshot_measurement_contract.md) defines these fields for preview snapshot declarations. |

Terminology may be cross-checked against public references such as [RayOptics](https://ray-optics.readthedocs.io/), [Optiland](https://optiland.readthedocs.io/), and [COMSOL Ray Optics](https://www.comsol.com/ray-optics-module). Their inclusion identifies adjacent tool categories, not validated interchange or shared physical results.

## Current Boundary

The repository evidence supports describing a GRIN-oriented transport prototype and its observation contracts. It does not establish agreement with an external optics package, laboratory measurement, wave solver, or scientific reference dataset.
