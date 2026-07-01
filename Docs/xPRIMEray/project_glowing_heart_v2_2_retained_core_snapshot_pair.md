# Project Glowing Heart v2.2 Retained Core Snapshot Pair

## What changed

The Core CLI can compare two separately retained Core run packets. It loads each manifest and `snapshot_heatmap.csv`, checks declared channel, fixture identity, observer basis, and coordinate grid, evaluates the pair through `ChannelCompatibilityMatrix`, and writes a Difference Packet without modifying either input.

New run manifests expose the existing observer basis, snapshot channel, and snapshot representation needed for retained comparison.

## v2.2.1 hardening

v2.2.1 aligns the JSON schema and compatibility matrix previews with retained comparison behavior. The Difference Packet schema remains the v2.0 core contract, with optional v2.1 and v2.2 preview extension fields for compatibility and provenance.

Requested-channel checks now happen before compatibility evaluation. A coordinate-grid mismatch is handled as an `Unknown` comparison-eligibility result with zero compared values, not as a successful comparison; the coordinate checks inside value comparison remain a second safety layer.

## What this demonstrates

Two retained scalar-grid artifacts can be compared under a named compatibility rule when their declarations and coordinate grids satisfy the rule conditions.

For two repeated deterministic `grin_radial_smoke` runs, the expected difference is zero across 880 retained samples.

## What this does not demonstrate

This is Core-vs-Core only.
This is not a Godot comparison.
This is not image or pixel comparison.
This is not parity.
This is not physical validation.
This is not renderer equivalence.

A zero difference between two deterministic Core runs does not establish equivalence with another runtime or measurement system.

## Command example

```bash
dotnet run --project src/XPrimeRay.Testbench.Cli -- compare-packets \
  /tmp/glowing_heart_v2_2_left/<run_packet_dir> \
  /tmp/glowing_heart_v2_2_right/<run_packet_dir> \
  --channel bend_magnitude_metric \
  --output /tmp/glowing_heart_v2_2_compare
```

## Output files

```txt
difference_packet.json
difference_summary.md
```

The comparison output records `comparisonScope=retained_snapshot_pair`, both run IDs, both manifest paths, channel registry and matrix versions, and the named compatibility rule.

## Required comparability conditions

- both manifests declare the requested channel
- fixture identity matches
- observer basis matches
- coordinate sets match
- the compatibility matrix returns `Comparable`

Values are not compared when those conditions fail.

## Failure modes

- missing packet directory, manifest, or scalar-grid artifact: command error
- older manifest without retained-comparison declarations: command error with the missing field named
- missing channel declaration: `Unknown`
- requested channel mismatch: `Unknown`
- fixture, observer, or coordinate-grid mismatch: `Unknown` under the current bend-magnitude rule
- declared incompatible channel pair: `NotComparable`
- output path equal to either input packet: command error

## Next milestone

v2.3 may add retained Core comparison fixtures that deliberately exercise non-zero values and mismatch statuses. It should remain Core-vs-Core and must not introduce image comparison or Core-to-Godot comparison.
