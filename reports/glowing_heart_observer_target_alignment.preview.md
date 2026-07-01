# Project Glowing Heart Observer Target Alignment (Preview)

Generated: 2026-06-26T15:50:33Z

Runtime executed: false

Parity claim: NONE

Shared target: Fixtures/shared/glowing_heart_observer_target.v0.preview.json

Pixel comparison ready: false

## Core vs Shared Target

Pixel comparison ready: false

| Status | Count |
|---|---:|
| MATCH | 10 |
| MISMATCH | 0 |
| PARTIAL | 1 |
| UNKNOWN | 2 |

### Metrics

| Metric | Value |
|---|---|
| positionDeltaEuclidean | 0.0 |
| forwardDot | 1.0 |
| upDot | 1.0 |
| fovDeltaDegrees | 0.0 |
| nearDelta | null |
| farDelta | null |

### Checks

| Category | Status | Reason |
|---|---|---|
| position | MATCH | Core observer position matches the shared target. |
| forward | MATCH | Core observer forward matches the shared target. |
| up | MATCH | Core observer up matches the shared target. |
| fov_degrees | MATCH | Core observer fov_degrees matches the shared target. |
| fov_axis | MATCH | Core observer fov_axis matches the shared target. |
| resolution | MATCH | Core observer resolution matches the shared target. |
| projection_type | MATCH | Core observer projection_type matches the shared target. |
| near | UNKNOWN | Core observer near is underspecified relative to the shared target. |
| far | UNKNOWN | Core observer far is underspecified relative to the shared target. |
| coordinate_handedness | PARTIAL | Core observer coordinate_handedness partially matches the shared target but uses source-specific vocabulary. |
| pixel_sampling | MATCH | Core observer pixel_sampling matches the shared target. |
| image_origin | MATCH | Core observer image_origin matches the shared target. |
| aspect_policy | MATCH | Core observer aspect_policy matches the shared target. |

### Blocking Deltas

- Core observer near is underspecified relative to the shared target.
- Core observer far is underspecified relative to the shared target.
- Core observer coordinate_handedness partially matches the shared target but uses source-specific vocabulary.

### Recommended Actions

- Make near/far explicit in Core fixture or shared instance.
- Confirm observer contract fields are emitted by Core tooling.
- Normalize inferred right-handed coordinate labels to the shared target vocabulary.


## Godot vs Shared Target

Pixel comparison ready: false

| Status | Count |
|---|---:|
| MATCH | 5 |
| MISMATCH | 3 |
| PARTIAL | 1 |
| UNKNOWN | 4 |

### Metrics

| Metric | Value |
|---|---|
| positionDeltaEuclidean | 2.0 |
| forwardDot | -1.0 |
| upDot | 1.0 |
| fovDeltaDegrees | 15.0 |
| nearDelta | 0.0 |
| farDelta | 0.0 |

### Checks

| Category | Status | Reason |
|---|---|---|
| position | MISMATCH | Godot observer position differs from the shared target. |
| forward | MISMATCH | Godot observer forward vector opposes the shared target. |
| up | MATCH | Godot observer up matches the shared target. |
| fov_degrees | MISMATCH | Godot observer fov_degrees differs from the shared target. |
| fov_axis | MATCH | Godot observer fov_axis matches the shared target. |
| resolution | UNKNOWN | Godot observer resolution is unknown or underspecified relative to the shared target. |
| projection_type | MATCH | Godot observer projection_type matches the shared target. |
| near | MATCH | Godot observer near matches the shared target. |
| far | MATCH | Godot observer far matches the shared target. |
| coordinate_handedness | PARTIAL | Godot observer coordinate_handedness partially matches the shared target but uses source-specific vocabulary. |
| pixel_sampling | UNKNOWN | Godot observer pixel_sampling is unknown or underspecified relative to the shared target. |
| image_origin | UNKNOWN | Godot observer image_origin is unknown or underspecified relative to the shared target. |
| aspect_policy | UNKNOWN | Godot observer aspect_policy is unknown or underspecified relative to the shared target. |

### Blocking Deltas

- Godot observer position differs from the shared target.
- Godot observer forward vector opposes the shared target.
- Godot observer fov_degrees differs from the shared target.
- Godot observer resolution is unknown or underspecified relative to the shared target.
- Godot observer coordinate_handedness partially matches the shared target but uses source-specific vocabulary.
- Godot observer pixel_sampling is unknown or underspecified relative to the shared target.
- Godot observer image_origin is unknown or underspecified relative to the shared target.
- Godot observer aspect_policy is unknown or underspecified relative to the shared target.

### Recommended Actions

- Configure or document Camera3D pose to target position [0,0,-2].
- Configure Camera3D to look along target forward [0,0,1], or document adapter transform.
- Set/confirm FOV 60 vertical.
- Define output resolution 40x22 for comparison fixture.
- Export pixel sampling, image origin, and aspect policy.
- Normalize Godot coordinate labels to the shared target vocabulary.


## Global Next Actions

- Do not attempt difference.ppm until both sides match the target.
- Create a snapshot channel contract before comparing pixels.
- Keep public language at perceptual demo / engineering prototype level.

## Bottom Line

Core and Godot are not both aligned to the shared observer target. Pixel comparison remains blocked.
