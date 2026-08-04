---
po_doc_type: evidence
title: Reading Qualification Runs
status: partial
engine_commit: "5ce15c13"
generated: false
---

# Reading Qualification Runs

## Checklist

1. **Recipe name / SceneId / SceneClass** present?
2. **Engine commit** recorded?
3. **Context** (pose, field policy, dimensions) frozen?
4. **Lifecycle Complete** and Unprocessed = 0 for full-frame claims?
5. **Channels cited** actually implemented and attached?
6. **Display Mode** documented as mapping only?
7. **Claim boundary** explicit (what is **not** claimed)?

## Prefer tables over prose

| Field | Example |
|-------|---------|
| Status | partial / qualified / failed baseline |
| Verified commit | `5ce15c13` |
| SceneId | `hermetic_chamber_v0` |
| Channels | `cathedral.probe.outcome` |
| Last qualified run | not yet qualified |

Use **planned**, **unknown**, **not yet assigned**, **not yet qualified** when metadata is missing—do not invent.

## Notebook form

Research notebook entries should include: date or milestone, engine commit, question, direct observations, interpretation, correction notes, unresolved questions, claim boundary. See [Research Notebook](../notebook/index.md).
