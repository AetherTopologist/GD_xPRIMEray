# Project Glowing Heart v1.0 Shared Fixture Candidate

## What changed since v0.9

The project now generates the first shared fixture candidate packet linking the Core radial GRIN smoke fixture to a best-effort Godot fixture candidate.

## What this proves

The bridge can identify a plausible cross-system fixture target using metadata only.

## What this does not prove

No parity.
No shared execution.
No Godot runtime behavior.
No SnapshotBuilder export.
No closure equivalence.
No transport equivalence.

## Commands

python3 tools/glowing_heart_shared_fixture_candidate.py

## Outputs

reports/glowing_heart_shared_fixture_candidate.preview.json
reports/glowing_heart_shared_fixture_candidate.preview.md

## Next milestone

v1.1 should create a static Godot fixture metadata export packet for the selected candidate, still without executing Godot.
