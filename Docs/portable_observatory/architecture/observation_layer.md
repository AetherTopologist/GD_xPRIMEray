---
po_doc_type: architecture
title: Observation Layer
status: partial
engine_commit: "5ce15c13"
generated: false
claim_boundary: "Describes portable sealed observation packaging; not a full API reference."
---

# Observation Layer

**Internal:** `XPrimeRay.ObservationLayer`
**Public story:** host-neutral packaging of measurements so Godot and other hosts can share **Evidence**, not just screenshots.

---

## Role

The Observation Layer defines:

- Frame descriptors (context, dimensions, policy fingerprint)
- Sealed channels with IDs, units, validity, claim boundaries, dependencies
- Adapters that **project** host/probe results into those channels

It does **not** replace Display Modes. Mapping descriptors may describe how a channel **could** be colored; the plate remains presentation.

---

## Status

| Piece | Status |
|-------|--------|
| Portable sealed frame types | **implemented** (`6e69d792` lineage) |
| Cathedral / Region Probe adapter (3 channels) | **implemented** |
| Full geometry/field/transport channel set | **planned** |
| Generated public API catalog | **planned / generated lane** |

---

## Spine mapping

```text
Transport Lens (host) → Instruments → Observation channels → (optional) Display Mode → Observation Plate
                              ↘ SealedObservationFrame → Evidence
```

---

## See also

- [Sealed Frames](sealed_frames.md)
- [Adapters](adapters.md)
- [Godot to Glowing Heart](godot_to_glowing_heart.md)
