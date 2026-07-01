# Project Glowing Heart v1.8.3 Observer Target Alignment

## What changed since v1.8.2

Created a static alignment report that compares each observer instance against the shared observer target:

- Core observer -> shared target
- Godot observer -> shared target

## What this proves

The project can now separate the remaining Core alignment work from the remaining Godot alignment work before any pixel comparison.

## What this does not prove

No parity.
No pixel comparison.
No Godot runtime execution.
No scene modification.
No renderer equivalence.
No transport equivalence.

## Commands

```bash
python3 tools/glowing_heart_observer_target_alignment.py
python3 -m py_compile tools/glowing_heart_observer_target_alignment.py
python3 -m json.tool reports/glowing_heart_observer_target_alignment.preview.json > /tmp/glowing_heart_observer_target_alignment.pretty.json
head -260 /tmp/glowing_heart_observer_target_alignment.pretty.json
head -220 reports/glowing_heart_observer_target_alignment.preview.md
git status --short -- reports/observatory_catalog.json GrinFilmCamera.cs RendererCore/Testing/RenderTestRunner.cs GodotAdapter/SnapshotBuilder.cs
```

## Outputs

```txt
reports/glowing_heart_observer_target_alignment.preview.json
reports/glowing_heart_observer_target_alignment.preview.md
Docs/xPRIMEray/project_glowing_heart_v1_8_3_observer_target_alignment.md
Docs/xPRIMEray/project_glowing_heart_v1_8_milestone.md
```

## Generator output

```txt
[glowing-heart-observer-target-alignment]
target=Fixtures/shared/glowing_heart_observer_target.v0.preview.json
core_pixel_comparison_ready=false
godot_pixel_comparison_ready=false
pixel_comparison_ready=false
runtime_executed=false
parity_claim=NONE

wrote=reports/glowing_heart_observer_target_alignment.preview.json
wrote=reports/glowing_heart_observer_target_alignment.preview.md
```

## Next milestone

Core should make near/far explicit and emit observer contract fields without underspecified values. Godot should configure or document the camera pose, forward direction, FOV, resolution, pixel sampling, image origin, and aspect policy to match the target. Pixel comparison remains blocked until both sides match the target.
