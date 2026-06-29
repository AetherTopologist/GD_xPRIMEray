# 40 Computational Observers

## Scope

Computational observers select, transport, sample, and encode simulated information. Examples include raster cameras, ray tracers, path tracers, field-aware ray systems, metric transport, diagnostic probes, and numerical detectors.

Entries in this family should follow the shared [Observer Grammar](observer_grammar.md).

## Observer Families

| Family | Observation model |
|---|---|
| Raster camera | Projects scene primitives into an image-space sampling and depth pipeline |
| Ray tracer | Evaluates scene interactions along rays emitted or traced through an observer model |
| Path tracer | Samples multi-interaction light transport under an estimator and film model |
| GRIN transport observer | Evolves ray direction through a spatially varying refractive field |
| Metric transport observer | Evolves paths under a declared metric or geodesic model |
| Diagnostic observer | Emits measurements such as depth, hit state, closure state, or transport metrics |

## Project Glowing Heart Evidence

The standalone Core CLI and artifacts are documented from [v0.1](../../xPRIMEray/project_glowing_heart_v0_1_baseline.md). The shared observer vocabulary appears in [v1.7](../../xPRIMEray/project_glowing_heart_v1_7_shared_observer_contract.md), target alignment in [v1.8](../../xPRIMEray/project_glowing_heart_v1_8_milestone.md), and measurement-channel semantics in [v1.9](../../xPRIMEray/project_glowing_heart_v1_9_shared_snapshot_measurement_contract.md).

Current evidence does not establish renderer, runtime, transport, or pixel equivalence. See [Project Glowing Heart](README.md#relationship-to-project-glowing-heart).

## Reading boundary

This territory follows the Atlas [reading boundary](README.md#reading-boundary).
