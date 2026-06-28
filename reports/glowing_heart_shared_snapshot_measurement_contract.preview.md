# Project Glowing Heart Shared Snapshot and Measurement Contract (Preview)

Parity claim: NONE

Runtime executed: false

## What This Contract Solves

The observer contract says where samples come from. This measurement contract says what those samples mean. It defines channel identity and type, units, normalization, dynamic range, color interpretation, comparison policy, claim boundaries, and the artifacts needed to interpret a snapshot.

Without those declarations, two equally sized images can look mechanically comparable while encoding unrelated quantities.

## Channel Vocabulary

| Channel type | Meaning |
|---|---|
| `bend_magnitude_metric` | scalar measurement of ray deflection |
| `rendered_intensity` | scalar rendered brightness or intensity |
| `rgb_render` | multi-component rendered color |
| `depth` | distance or depth measurement under declared units and convention |
| `hit_miss` | binary intersection result |
| `closure_state` | categorical or encoded closure result |
| `unknown` | channel semantics have not been established |

Comparison status is one of `COMPARABLE`, `NOT_COMPARABLE`, `REQUIRES_TRANSFORM`, or `UNKNOWN`.

## Why Channel Semantics Matter

Pixel coordinates and dimensions describe storage layout, not meaning. Comparability also requires compatible measured quantities, units, normalization, dynamic range, component interpretation, and transfer functions. A transform must identify its inputs, outputs, algorithm, parameters, and provenance before it can authorize a transformed comparison.

## Core Pixels Are Not Godot Render Pixels

Core's current `bend_magnitude_metric` snapshot records a scalar ray-deflection measurement. A Godot `rgb_render` records rendered color, while `rendered_intensity` records image brightness under a rendering pipeline. These are different observables even when they refer to the same ray or image coordinate.

Therefore:

| Source | Target | Status | Reason |
|---|---|---|---|
| Core `bend_magnitude_metric` | Godot `rgb_render` | `NOT_COMPARABLE` | deflection magnitude is not rendered color; no transform is defined |
| Core `bend_magnitude_metric` | Godot `rendered_intensity` | `NOT_COMPARABLE` | deflection magnitude is not rendered brightness; no transform is defined |

If a future, reviewed transform is defined, the applicable rule may become `REQUIRES_TRANSFORM`. Until then, direct subtraction, `difference.ppm`, and pixel-equivalence claims are blocked.

## Claim Boundary

This contract can establish that a channel's semantics and comparison policy are declared. It does not establish renderer parity, pixel equivalence, runtime equivalence, transport equivalence, or scientific validation.

No parity.
No pixel comparison.
No Godot runtime execution.

## Artifact

`schemas/glowing_heart/shared_snapshot_measurement_contract.v0.preview.json`

## What v2.0 Needs Next

The v2.0 Difference Packet Design should bind two snapshot declarations to exact artifacts and observer instances; select an explicit comparison rule; reference any required transform; define alignment, masks, tolerance, missing/non-finite sample handling, and difference encoding; and carry provenance plus claim-safe result status. A difference packet must refuse comparison when the channel policy is `NOT_COMPARABLE` or `UNKNOWN`.
