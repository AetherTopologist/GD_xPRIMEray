# Project Glowing Heart Observer Reconciliation (Preview)

Generated: 2026-06-24T21:52:08Z

Runtime executed: false

Parity claim: NONE

Pixel comparison ready: false

## Summary

| Status | Count |
|---|---:|
| MATCH | 3 |
| MISMATCH | 3 |
| PARTIAL | 1 |
| UNKNOWN | 6 |

## Numeric Metrics

| Metric | Value |
|---|---|
| positionDeltaEuclidean | 2.0 |
| forwardDot | -1.0 |
| upDot | 1.0 |
| fovDeltaDegrees | 15.0 |
| nearDelta | null |
| farDelta | null |

## Checks

| Category | Status | Reason |
|---|---|---|
| position | MISMATCH | Core and Godot observer positions differ. |
| forward | MISMATCH | Core and Godot forward vectors oppose each other. |
| up | MATCH | Core and Godot up vectors match. |
| fov_degrees | MISMATCH | Core and Godot FOV degrees differ. |
| fov_axis | MATCH | Core and Godot FOV axes match. |
| resolution | UNKNOWN | Godot resolution is unknown from static metadata. |
| projection_type | MATCH | Core and Godot projection types match. |
| near | UNKNOWN | Core near clip plane is not specified; comparison is underspecified. |
| far | UNKNOWN | Core far clip plane is not specified; comparison is underspecified. |
| coordinate_handedness | PARTIAL | Both observers indicate right-handed coordinates, but labels are source-specific/inferred. |
| pixel_sampling | UNKNOWN | Godot pixel sampling convention is unknown from static metadata. |
| image_origin | UNKNOWN | Godot image origin and row order are unknown from static metadata. |
| aspect_policy | UNKNOWN | Godot aspect policy is unknown from static metadata. |

## Blocking Deltas

- Core and Godot observer positions differ.
- Core and Godot forward vectors oppose each other.
- Core and Godot FOV degrees differ.
- Godot resolution is unknown from static metadata.
- Godot pixel sampling convention is unknown from static metadata.
- Godot image origin and row order are unknown from static metadata.
- Godot aspect policy is unknown from static metadata.
- Core near/far are not specified in the Core fixture.
- Both observers indicate right-handed coordinates, but labels are source-specific/inferred.

## Recommended Next Actions

- Define a v1.8.2 shared observer target with explicit agreed values: position, forward, up, FOV, resolution, projection, near/far, pixel sampling, image origin, and aspect policy.
- Create a shared observer target instance with explicit agreed pose/FOV/resolution.
- Update Core fixture or shared instance to reference that target.
- Export or statically define a Godot observer candidate matching that target.
- Add pixelSampling, imageOrigin, and aspectPolicy to enforced observer instances.
- Only attempt difference.ppm after observer reconciliation passes.

## Bottom Line

The observer instances are not yet reconciled. Pixel comparison is not ready.
