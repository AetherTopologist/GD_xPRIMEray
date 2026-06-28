# Project Glowing Heart v1.8.2 Shared Observer Target

## What changed since v1.8.1

Created a shared observer target artifact for the Glowing Heart GRIN bridge. The target uses the explicit Core smoke observer pose, FOV, resolution, sampling, image origin, and aspect policy, while adopting the selected Godot candidate `far=40` clip value to remove one avoidable future mismatch.

The v1.8.4 cleanup adds `right_vector`, `pixel_aspect_ratio`, and `snapshot_channel` placeholders. Existing camelCase bridge fields remain stable, with their snake_case observer contract mappings documented in `contractFieldMapping`.

## What this proves

The project now has a named observer target that Core and Godot can both align to before any future pixel comparison.

## What this does not prove

No parity.
No pixel comparison.
No Godot runtime execution.
No scene modification.
No renderer equivalence.
No transport equivalence.

## Commands

```bash
python3 tools/glowing_heart_shared_observer_target.py
python3 -m py_compile tools/glowing_heart_shared_observer_target.py
python3 -m json.tool fixtures/shared/glowing_heart_observer_target.v0.preview.json > /tmp/glowing_heart_observer_target.pretty.json
head -220 /tmp/glowing_heart_observer_target.pretty.json
head -180 reports/glowing_heart_shared_observer_target.preview.md
git status --short -- reports/observatory_catalog.json GrinFilmCamera.cs RendererCore/Testing/RenderTestRunner.cs GodotAdapter/SnapshotBuilder.cs
```

## Outputs

```txt
fixtures/shared/glowing_heart_observer_target.v0.preview.json
reports/glowing_heart_shared_observer_target.preview.md
Docs/xPRIMEray/project_glowing_heart_v1_8_2_shared_observer_target.md
```

## Generator output

```txt
[glowing-heart-shared-observer-target]
target=fixtures/shared/glowing_heart_observer_target.v0.preview.json
basis=core_observer_with_godot_far_clip
runtime_executed=false
parity_claim=NONE
pixel_comparison_ready=false

wrote=fixtures/shared/glowing_heart_observer_target.v0.preview.json
wrote=reports/glowing_heart_shared_observer_target.preview.md
```

## Next milestone

Core should explicitly declare near/far and emit an observer instance matching this target. Godot should expose or configure a camera candidate matching this target pose, FOV, resolution, sampling, image origin, and aspect policy. Pixel comparison should remain blocked until both emitted observer instances match the target.
