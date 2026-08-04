---
po_doc_type: architecture
title: Diagnostics
status: partial
engine_commit: "5ce15c13"
verified_commit: "5ce15c1362615e1e0fa4ba02f18c6dae78c243c2"
generated: false
claim_boundary: "Diagnostics verbosity controls terminal/runtime telemetry. Sealed observation frames remain the evidence carrier. Summary counters may be n/a when authoritative values are unavailable."
---

# Diagnostics

**Verified commit:** `5ce15c1362615e1e0fa4ba02f18c6dae78c243c2`
(`perf(observatory): centralize transport diagnostics`)

Public story: control how much **runtime telemetry** the host prints or aggregates. Diagnostics are **not** a substitute for sealed Evidence.

**Internal:** `XPrimeRay.Diagnostics` — `DiagnosticVerbosity`, `RuntimeDiagnosticPolicy`, `DiagnosticRuntimeMode`.

---

## Verbosity ladder

| Level | Public name | Intent |
|-------|-------------|--------|
| **Off** | Off | Minimal / silence non-error noise |
| **Summary** | Summary | Coarse live runtime counters (default for Live) |
| **Frame** | Frame | Per-snapshot / frame-oriented detail (default for Snapshot / headless qualification) |
| **Region** | Region | Region-audit oriented detail |
| **PixelTrace** | PixelTrace | Selected-pixel traces (count limited) |
| **TransportTrace** | TransportTrace | Deep transport path traces (**opt-in** only) |

Environment (when used):

- `XPRIMERAY_DIAGNOSTIC_VERBOSITY` — parse to verbosity enum
- `XPRIMERAY_DIAGNOSTIC_TRANSPORT_TRACE` — opt-in for TransportTrace (`1` / `true` / `yes`)

---

## Runtime modes (defaults)

| `DiagnosticRuntimeMode` | Default verbosity |
|-------------------------|-------------------|
| Live | Summary |
| Snapshot | Frame |
| HeadlessQualification | Frame |
| ExplicitRegionAudit | Region |
| ExplicitSelectedPixelAudit | PixelTrace |

---

## `[LiveSummary]` and terminal output

| Fact | Doctrine |
|------|----------|
| `[LiveSummary]` lines | **Runtime performance / live telemetry** (film size, frames, render steps, …) |
| Terminal dump | **Not automatically Evidence** |
| Sealed observation frames | **Evidence carrier** for scientific claims |
| Summary semantic counters | May report **n/a** when authoritative values are unavailable—do not invent numbers |

Use the Inspector and sealed `ProbeFrameSummary` / channel data for outcome classes. Do not promote a green-looking LiveSummary into a qualification PASS.

---

## Categories (internal)

Lifecycle, Render, Transport, Boundary, Probe, Performance, Error—used for gating which messages a verbosity allows. Errors may still surface when terminal policy requires.

---

## Relation to Region Probe

| Diagnostics Region/PixelTrace | Region Probe |
|-------------------------------|--------------|
| Telemetry detail about audits | Semantic outcome regions + optional refine |
| Does not seal channels by itself | Adapter seals outcome/region/refinement |

---

## Claim boundary

!!! warning
    Diagnostics help operators **debug**. They do not establish proper time, physical gravity, or wormhole confirmation. TransportTrace is process detail under numerical policy—not causal structure of spacetime.

---

## See also

- [Evidence Doctrine](../evidence/evidence_doctrine.md)
- [Sealed Frames](sealed_frames.md)
