# Project Glowing Heart v2.12 CI/Preflight Hook

## What Changed

Glowing Heart now has one local preflight command for the evidence chain. It runs the v2.11 health check with the supporting Python, JSON, Atlas Graph, SVG, .NET, and MkDocs checks needed before a commit or CI job.

The runner prints a compact console summary and atomically writes `reports/glowing_heart_v2_12_preflight.preview.md`. It does not mutate source artifacts; its only generated dependencies are the two v2.11 health reports.

## What This Demonstrates

The evidence-chain tooling and documentation can be checked through a repeatable command with a non-zero exit status for required failures. Individual sections remain visible as `PASS`, `FAIL`, or `SKIP`.

## What This Does Not Demonstrate

- Core-vs-Core only.
- Not a Godot comparison.
- Not image or pixel comparison.
- Not parity.
- Not physical validation.
- Not renderer equivalence.
- Preflight verifies artifact synchronization and tooling health, not scientific correctness.

## Command

Run the complete preflight from the repository root:

```bash
python3 tools/glowing_heart_preflight.py
```

## Flags

| Flag | Behavior |
|---|---|
| `--quick` | Skip both .NET builds and MkDocs. |
| `--skip-dotnet` | Skip only the .NET builds. |
| `--skip-mkdocs` | Skip only MkDocs. |
| `--output-md PATH` | Write the Markdown summary to a different path. |

## Checks Included

The runner executes these sections in order:

1. Python syntax for the seven evidence-chain tools.
2. JSON syntax for the packet index, map index, graph, and their schemas.
3. Atlas Graph validation through the shared validator.
4. The v2.11 Evidence Chain health check.
5. SVG parsing, exhibit count, script, and external-link safety checks.
6. Core and Testbench CLI .NET builds unless skipped.
7. MkDocs build unless skipped.

## CI-Ready Usage

The default command is suitable as a CI step because it has no network dependency, writes reports atomically, captures section failures, and returns non-zero when any required section fails.

No GitHub Actions workflow is added in v2.12. The repository's existing workflow is documentation-specific; adopting this broader preflight remains an explicit CI policy decision.

## Failure Modes

Malformed Python, JSON, graph, or SVG input fails its section. Evidence drift fails the health section. Build or documentation errors preserve their command output in the report. Checks continue after a failure where possible so one run presents a useful section summary.

## Next Milestone

Glowing Heart v2.13 can freeze the v2.x evidence-chain artifacts, publish a release-candidate summary, and define the criteria for moving preview contracts to stable versions.

