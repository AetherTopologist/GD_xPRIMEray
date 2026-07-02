# Project Glowing Heart v2.11 Evidence Chain Health Check

## What Changed

Glowing Heart now has a deterministic health check for the complete recorded evidence chain. The validator checks the Difference Packet Index, Atlas Graph, evidence-map SVG, both generated gallery pages, and Evidence Map Index as one synchronized artifact set.

It uses the shared Atlas Graph validator and optional local `jsonschema` support. Source artifacts are read-only; JSON and Markdown reports are written atomically after checks complete.

## Chain Checked

```text
Difference Packet Index
  -> Atlas Graph
  -> Evidence Map
  -> Gallery
  -> Evidence Map Index
```

The check covers IDs, counts, statuses, rules, metric summaries, claim boundaries, recorded paths, SVG structure and safety, gallery provenance, and fixed comparison-stage claim guards.

## What This Demonstrates

The five recorded exhibits are synchronized across the machine-readable indexes, graph metadata, SVG cards, and generated gallery sections. Drift in a checked field produces an error and a non-zero exit status.

## What This Does Not Demonstrate

- Core-vs-Core only.
- Not a Godot comparison.
- Not image or pixel comparison.
- Not parity.
- Not physical validation.
- Not renderer equivalence.
- Health checking validates artifact synchronization, not scientific correctness.

## Command

```bash
python3 tools/glowing_heart_evidence_chain_health.py \
  --output-json reports/glowing_heart_v2_11_evidence_chain_health.preview.json \
  --output-md reports/glowing_heart_v2_11_evidence_chain_health.preview.md
```

The default source paths are canonical repository paths. Source-path options permit isolated checks of temporary copies without mutating the evidence chain.

## Failure Modes

The command exits non-zero when any error-severity check exists. Examples include a changed graph status, a missing SVG exhibit group, stale Deferred Case E gallery text, a durable `/tmp` source path, a broken claim guard, malformed JSON/XML, or mismatched exhibit metadata.

An unavailable optional `jsonschema` package is reported as a non-blocking warning. Structural and cross-chain checks still run without network access.

## Output Reports

- structured health report: `reports/glowing_heart_v2_11_evidence_chain_health.preview.json`
- visitor-readable report: `reports/glowing_heart_v2_11_evidence_chain_health.preview.md`
- validator: `tools/glowing_heart_evidence_chain_health.py`

The report timestamp is inherited from the source Difference Packet Index, making unchanged health reports deterministic.

## Next Milestone

Glowing Heart v2.12 can add the health check to a local preflight script or documented CI-ready command so evidence-chain drift is caught before commit.

