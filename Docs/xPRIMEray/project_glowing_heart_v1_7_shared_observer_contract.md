# Project Glowing Heart v1.7 Shared Observer Contract

## What changed since v1.6

The project now has a preview shared observer contract schema that names the camera/observer vocabulary needed before pixel comparison.

## What this proves

Core and Godot can both eventually describe the same observer using one vocabulary:

- position
- forward
- up
- fov
- resolution
- projection
- near
- far
- coordinate_system
- right_vector
- aspect_policy
- pixel_sampling
- image_origin
- pixel_aspect_ratio
- snapshot_channel

This is the first artifact that directly advances future `difference.ppm` work by removing hidden observer assumptions.

## Audit patch

The v1.7 audit found that the first draft named the main camera variables but left several pixel-comparison conventions in prose. The contract now requires those conventions as schema fields:

- pixel sampling location
- image origin and row order
- aspect-ratio policy
- right-vector derivation and up re-orthogonalization
- coordinate vector space
- snapshot channel semantics

It also documents current bridge candidate mismatches that must be resolved before `difference.ppm`:

- Core observer uses position `[0, 0, -2]` and forward `[0, 0, 1]`; the Godot candidate camera is currently identity/default-forward.
- Core fixture FOV is 60 degrees; the Godot scene FOV is 75 degrees.
- The example far plane is 1000; the Godot scene far plane is 40.

## What this does not prove

No Godot runtime execution.
No Core/Godot pixel equivalence.
No renderer parity.
No physics validation.
No closure proof.

## Commands

```bash
python3 -m json.tool schemas/glowing_heart/shared_observer_contract.v0.preview.json > /tmp/shared_observer_contract.pretty.json
python3 - <<'PY'
import json
from pathlib import Path
import jsonschema

schema = json.loads(Path("schemas/glowing_heart/shared_observer_contract.v0.preview.json").read_text())
jsonschema.Draft202012Validator.check_schema(schema)
print("schema_valid=true")
PY
head -220 /tmp/shared_observer_contract.pretty.json
head -220 reports/glowing_heart_shared_observer_contract.preview.md
```

## Outputs

```txt
schemas/glowing_heart/shared_observer_contract.v0.preview.json
reports/glowing_heart_shared_observer_contract.preview.md
Docs/xPRIMEray/project_glowing_heart_v1_7_shared_observer_contract.md
```

## Next milestone

Create Core and Godot observer contract instances for the selected shared fixture candidate, then audit whether those instances are sufficient to seed a future `difference.ppm` comparison packet.
