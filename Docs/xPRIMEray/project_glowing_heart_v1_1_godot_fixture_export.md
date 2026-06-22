# Project Glowing Heart v1.1 Godot Fixture Export

## What changed since v1.0

The selected Godot fixture candidate now has a static metadata export packet.

## What this proves

The Godot fixture can be inspected without executing Godot, producing machine-readable metadata for future bridge work.

## What this does not prove

No parity.
No shared execution.
No SnapshotBuilder export.
No closure equivalence.
No transport equivalence.

## Commands

python3 tools/glowing_heart_godot_fixture_export.py

## Outputs

reports/glowing_heart_godot_fixture_export.preview.json
reports/glowing_heart_godot_fixture_export.preview.md

## Next milestone

v1.2 should compare the Core fixture metadata and static Godot fixture export in one gap matrix, still without claiming parity.
