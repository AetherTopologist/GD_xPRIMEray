# xPRIMEray-Core Testbench Run

## Fixture

- Name: grin_radial_smoke_resolution_variant
- Mode: radial_grin_smoke
- Resolution: 41x22

## Result

- Validation: PASS
- Rays: 902
- Hits: 0
- Misses: 902
- Steps per ray: 32
- Field samples: 28864
- Mean bend: 0.000960229547
- Max bend: 0.00562415784

## Human Observable Artifacts

- `snapshot.ppm` - grayscale PPM visualization of per-pixel bend magnitude
- `snapshot_heatmap.csv` - per-ray bend magnitude table
- `traversal_step_count.csv` - executed integration-step count per ray
- `snapshot_ascii.txt` - terminal-viewable bend magnitude map

## Interpretation

This run is a Project Glowing Heart v0.6 first human observable snapshot. It proves the Core CLI can emit a visible transport metric artifact from a deterministic field-driven smoke fixture without launching Godot.

## Limitations

- Godot parity is not claimed.
- Hermetic closure is not claimed.
- Collision behavior is not modeled.
- Portal behavior is not modeled.
