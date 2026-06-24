# Project Glowing Heart v1.5 Public Demo Readiness

## What changed since v1.4

The project now has a public-demo readiness gate that turns preview artifacts into explicit claim boundaries, safe/unsafe artifact lists, a ranked visual demo candidate, blocking deltas, and handoff tasks.

## What this proves

The project can identify what is safe to say publicly today:

- Project Glowing Heart is an active engineering initiative.
- xPRIMEray-Core has standalone preview artifacts.
- A shared Core/Godot fixture bridge candidate exists.
- Public progress framing is safe when parity and validation claims are excluded.

## What this does not prove

No Godot runtime execution.
No Core/Godot parity.
No hermetic closure proof.
No wormhole physics validation.
No pixel equivalence.
No public scientific validation.
No institutional endorsement.

## Commands

```bash
python3 tools/glowing_heart_public_demo_readiness.py
python3 -m py_compile tools/glowing_heart_public_demo_readiness.py
python3 -m json.tool reports/glowing_heart_public_demo_readiness.preview.json > /tmp/public_demo_readiness.pretty.json
head -220 /tmp/public_demo_readiness.pretty.json
head -220 reports/glowing_heart_public_demo_readiness.preview.md
git status --short -- reports/observatory_catalog.json GrinFilmCamera.cs RendererCore/Testing/RenderTestRunner.cs GodotAdapter/SnapshotBuilder.cs
```

## Outputs

```txt
reports/glowing_heart_public_demo_readiness.preview.json
reports/glowing_heart_public_demo_readiness.preview.md
```

## Next milestone

v1.6 should create the first Grok handoff packet for public-facing demo/interface language and layout, while preserving the current no-parity, no-validation boundary.
