# Project Glowing Heart v2.0 Difference Packet Implementation

## What changed

The standalone Core CLI can now emit an opt-in Difference Packet for a Core-vs-Core self-comparison. The packet records snapshot and channel identities, observer and comparison bases, status, transform requirement, reason, zero-valued self-difference metrics, and explicit claim boundaries.

Use `--emit-difference` to add the packet to a normal artifact run. Without that option, existing artifact output remains unchanged. With `--no-output`, the CLI reports comparison eligibility without writing files.

## What this demonstrates

The CLI can emit a formal Difference Packet artifact for a declared comparable Core-vs-Core channel pair.

## What it does not prove

No Godot parity.
No physical validation.
No renderer equivalence.
No cross-channel comparison.
No image or pixel comparison.
No proof of physical correctness.

The v2.0 packet compares one Core `bend_magnitude_metric` sample grid with itself. A zero difference is expected by construction and does not establish equivalence with another implementation or measurement system.

## Commands

```bash
dotnet build src/XPrimeRay.Core/XPrimeRay.Core.csproj
dotnet build src/XPrimeRay.Testbench.Cli/XPrimeRay.Testbench.Cli.csproj
dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture fixtures/grin_radial_smoke.json --emit-difference
dotnet run --project src/XPrimeRay.Testbench.Cli -- run-fixture fixtures/grin_radial_smoke.json --emit-difference --no-output
```

## Outputs

When output and difference emission are enabled, the run packet adds:

```txt
difference_packet.json
difference_summary.md
```

The manifest includes `differencePacketJson` and `differenceSummaryMarkdown` only for an emitted Difference Packet. The short-lived duplicate `differenceSummaryMd` alias had no repository consumers and was removed in v2.0.1 before release.

The run manifest remains at schema v0.6 for this narrow optional addition. A future manifest schema v0.7 or equivalent should formally describe conditional Difference Packet artifacts before external consumers depend on them.

The v2.0.1 preview schema structurally checks emitted packets during verification with the repository's existing `jsonschema` tooling. Schema checking is not yet wired into the CLI output path.

Expected Core self-comparison metrics for `grin_radial_smoke`:

```txt
countCompared=880
maxAbsDifference=0
meanAbsDifference=0
nonZeroCount=0
```

The packet preserves `runtimeExecuted=false` and `parityClaim=NONE`. In this contract trail, `runtimeExecuted=false` records that no external or Godot runtime comparison was executed; the standalone Core fixture command still runs to produce its declared samples.

## Next milestone

A later milestone may compare two separately retained Core snapshots with matching declared channels and observer bases. Cross-channel and Godot comparisons remain outside v2.0.
