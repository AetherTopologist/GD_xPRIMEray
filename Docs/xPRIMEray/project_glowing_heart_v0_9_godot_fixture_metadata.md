# Project Glowing Heart v0.9 Godot Fixture Metadata

## What changed since v0.8

A static metadata extractor now scans the repo for Godot fixture candidates and emits preview JSON/Markdown indexes.

## What this proves

The Godot-side fixture landscape can be inspected without executing Godot.

## What this does not prove

No parity.
No shared fixture execution.
No SnapshotBuilder export.
No closure equivalence.
No transport equivalence.

## Commands

python3 tools/glowing_heart_godot_fixture_index.py

## Outputs

reports/glowing_heart_godot_fixture_candidates.preview.json
reports/glowing_heart_godot_fixture_candidates.preview.md

## Next milestone

v1.0 should create the first shared fixture candidate packet:

Core fixture metadata
+
Godot fixture metadata
+
explicit gap map

without claiming parity.
