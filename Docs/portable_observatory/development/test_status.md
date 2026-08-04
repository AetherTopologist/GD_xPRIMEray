---
po_doc_type: development
title: Test Status
status: partial
engine_commit: "5ce15c13"
generated: false
claim_boundary: "This curated page does not duplicate generated test matrices. Link out rather than invent PASS rates."
---

# Test Status

**Curated note:** authoritative test matrices and latest run tables belong in **generated** or CI artifacts when available. This page only points.

| Area | Notes |
|------|--------|
| Observation layer / diagnostics unit tests | Present under `src/XPrimeRay.Diagnostics.Tests` and related projects at HEAD |
| Cathedral / Region Probe contracts | Engine commits in probe lineage (`32aa9ab9` … `6e69d792`) |
| OI headless fixtures | Sibling evidence lane; fixture PASS ≠ live plate |
| Latest public qualification table | **not yet assigned** as a generated GH Pages artifact |

Do not copy large enum/schema tables by hand. Prefer:

- Sealed channel list: [Observation Channels](../reference/observation_channels.md)
- Gallery / GH fixture browsers under Project Glowing Heart and Observatory Gallery

When a generated test-status page ships, link it here and keep this page as a stub index.
