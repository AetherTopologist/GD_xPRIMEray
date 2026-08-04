---
po_doc_type: architecture
title: Adapters
status: partial
engine_commit: "5ce15c13"
generated: false
---

# Adapters

Adapters project host-specific probe/transport results into the portable Observation Layer.

| Adapter (internal) | Public role | Status |
|--------------------|-------------|--------|
| `CathedralProbeObservationFrameAdapter` | Region Probe outcomes → sealed channels | **implemented** |
| Future geometry adapters | Normals/depth/ndotv → `geometry.*` | **planned** |
| Future field adapters | Magnitude/gradient → `field.*` | **planned** |
| Future transport-record adapters | Step count/path length → `transport.*` | **planned** |

Adapters must:

1. Refuse incomplete planes for full-frame seal when policy requires Complete.
2. Attach channel descriptors with claim boundaries.
3. Not invent channels that were not measured.

Display Mode shaders and film shading are **not** adapters. They do not seal.

---

## See also

- [Observation Layer](observation_layer.md)
- [Godot to Glowing Heart](godot_to_glowing_heart.md)
