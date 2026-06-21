# Project Glowing Heart v0.4 Observatory Entry Packet

## What changed since v0.3

The Core CLI output packet now includes `observatory_entry.json` beside:

- `manifest.json`
- `ray_metrics.csv`
- `run_summary.md`

The run manifest schema and phase were bumped to v0.4, and the manifest artifact list now names the Observatory entry file.

## What this proves

The headless Core can emit a portable evidence packet that includes a row-shaped Observatory catalog entry. Existing Observatory tooling can recognize the familiar catalog vocabulary: `category`, `fixture`, `run_id`, `artifact_type`, `coverage`, `closure`, `verdict`, `timestamp`, and `source_path`.

## What this does not prove

No Godot parity, hermetic closure, collision behavior, portal behavior, image rendering, or automatic catalog append is claimed.

## Acceptance command

```bash
dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture fixtures/grin_radial_smoke.json
```

Expected packet shape:

```txt
output/glowing_heart/<run_id>/
  manifest.json
  ray_metrics.csv
  run_summary.md
  observatory_entry.json
```

## Next minimal milestone

v0.5 should introduce the first Core-vs-Godot comparison note for `grin_radial_smoke`, without broad renderer coupling.
