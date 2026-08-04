---
po_doc_type: evidence
title: Evidence Doctrine
status: partial
engine_commit: "5ce15c13"
generated: false
---

# Evidence Doctrine

## What counts

| Evidence class | Carrier | Use |
|----------------|---------|-----|
| **Sealed observation** | `SealedObservationFrame` + channel data + context | Scientific / portable claims |
| **Fixture / recipe run** | Named recipe result + metadata | PASS/FAIL under guardrails |
| **Presentation capture** | Screenshot with **role-labeled caption** | Teaching, failure notebooks |
| **Terminal telemetry** | `[LiveSummary]`, diagnostic logs | Debug only unless attached as supporting annex |

## What does not count alone

- Magenta plate color
- Unlabeled beauty screenshots
- Display Mode choice
- “It looked closed” without enclosure test record
- Fixture PASS as free-roam certificate

## Caption roles (required for screenshots)

Every screenshot caption must identify one of:

- world viewport
- Observation Plate
- display mapping
- semantic outcome plane
- qualification artifact
- legacy presentation
- planned storyboard

No screenshot may establish an outcome class **by color alone**.

## See also

- [Fixture vs Live Runtime](fixture_vs_live_runtime.md)
- [Reading Qualification Runs](reading_qualification_runs.md)
- [Claim Boundaries](../reference/claim_boundaries.md)
