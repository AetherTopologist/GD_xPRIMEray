# Project Glowing Heart v1.8 Observer Instances

## What changed since v1.7

The shared observer contract now has concrete Core and Godot observer instances for the selected shared fixture candidate.

## What this proves

The project can place Core and Godot observer metadata side by side using one vocabulary.

## What this does not prove

No parity.
No runtime equivalence.
No pixel comparison.
No renderer equivalence.
No transport equivalence.

## Commands

```bash
python3 tools/glowing_heart_observer_instances.py
python3 -m py_compile tools/glowing_heart_observer_instances.py
python3 -m json.tool reports/glowing_heart_observer_instances.preview.json > /tmp/observer_instances.pretty.json
head -220 /tmp/observer_instances.pretty.json
head -180 reports/glowing_heart_observer_instances.preview.md
git status --short -- reports/observatory_catalog.json GrinFilmCamera.cs RendererCore/Testing/RenderTestRunner.cs GodotAdapter/SnapshotBuilder.cs
```

## Outputs

```txt
reports/glowing_heart_observer_instances.preview.json
reports/glowing_heart_observer_instances.preview.md
```

## Next milestone

v1.8.1 should reconcile these observer instances and report whether pixel comparison is ready.
