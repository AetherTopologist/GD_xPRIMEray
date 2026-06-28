# Adapter Landscape

## Scope

This view describes the metadata and translation layers needed to connect Core artifacts, Godot fixtures, and future comparison packets.

## Adapter Layers

| Layer | Responsibility | Project Glowing Heart evidence |
|---|---|---|
| Fixture description | name scene inputs, field parameters, provenance, and source-specific extensions | Preview schema and instance work appears in [v1.3](../../xPRIMEray/project_glowing_heart_v1_3_shared_fixture_schema.md), [v1.4](../../xPRIMEray/project_glowing_heart_v1_4_shared_fixture_instance.md), and [v1.4.1](../../xPRIMEray/project_glowing_heart_v1_4_1_schema_alignment.md). |
| Static engine export | extract candidate metadata without asserting runtime behavior | Godot candidate metadata and export artifacts are documented in [v0.9](../../xPRIMEray/project_glowing_heart_v0_9_godot_fixture_metadata.md) and [v1.1](../../xPRIMEray/project_glowing_heart_v1_1_godot_fixture_export.md). |
| Observer normalization | map pose, basis, projection, clipping, sampling, and image conventions | [v1.7](../../xPRIMEray/project_glowing_heart_v1_7_shared_observer_contract.md) defines the vocabulary; [v1.8](../../xPRIMEray/project_glowing_heart_v1_8_milestone.md) records alignment status. |
| Measurement normalization | declare channel semantics, units, ranges, color models, and comparison rules | [v1.9](../../xPRIMEray/project_glowing_heart_v1_9_shared_snapshot_measurement_contract.md) defines the preview contract. |
| Artifact packaging | bind manifests, reports, datasets, provenance, and catalog-shaped metadata | The Core Observatory entry packet is documented in [v0.4](../../xPRIMEray/project_glowing_heart_v0_4_observatory_entry.md). |
| Difference packet | bind aligned observations, transforms, masks, tolerances, and result claims | Planned after v1.9; no packet or pixel comparison is claimed. |

Relevant interoperability contexts include [glTF](https://www.khronos.org/gltf/), [OpenUSD](https://openusd.org/release/index.html), [OpenColorIO](https://opencolorio.org/), and engine-specific import/export systems. Project Glowing Heart's preview schemas are project-local contracts and are not presented as implementations of those standards.

## Translation Rule

An adapter must preserve source provenance and expose unknowns. It must not convert missing evidence into an inferred match. A translation is complete only for the fields explicitly covered by its contract and cited artifacts.
