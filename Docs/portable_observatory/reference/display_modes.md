---
po_doc_type: reference
title: Display Modes
status: partial
engine_commit: "5ce15c13"
generated: false
claim_boundary: "Display Modes are visualization mappings. They are not sealed instruments and not outcome authority."
---

# Display Modes

A **Display Mode** colors samples or host film buffers for human viewing. It is **not** an Instrument and **not** a sealed observation channel.

**Internal pair:** Display Mode (`FilmShadingMode` / host recipe).

---

## Available as display paths (today)

| Mode | Typical mapping | Good for | Not good for |
|------|-----------------|----------|--------------|
| **RGB** / host beauty | Host film color recipe | Reconnaissance of presentation | Semantic region ID |
| **NormalRGB** | \(n_i \mapsto 0.5 n_i + 0.5\) | Coarse face orientation teaching | Material normal maps; sealed `geometry.normal` claims |
| **Depth** | Distance / depth heatmap | Relative near/far structure | Proper time; sealed `geometry.depth` |
| **NdotV** | \(\mathrm{saturate}(n\cdot v)\) | Facing brightness teaching | Semantic mesh labels |
| **TwoSidedNdotV** | \(\mathrm{saturate}(|n\cdot v|)\) | Two-sided facing demo | Same limits as NdotV |

Legacy **magenta / SkyColor fallback** may appear when the host presentation path has no sample color to show. That fill is **presentation**, not `ProbeOutcomeCode`.

---

## Relationship to planned channels

| Planned sealed channel | Planned mapping | Plate |
|------------------------|-----------------|-------|
| `geometry.normal` | `normal_rgb.v1` | Observation Plate |
| `geometry.depth` | `depth_heatmap.v1` | Observation Plate |

**Present-tense rule:** Until those channels ship, say “NormalRGB **display path**,” not “geometry.normal instrument.”

---

## Operator checks

1. Change Display Mode only → outcome histogram **unchanged**.
2. Change field / pose → new generation; plate may change for transport **and** display reasons—use Inspector.
3. Never seed Region Probe from Display Mode color alone.

---

## See also

- [Display Is Not Probe](../learn/display_is_not_probe.md)
- [Observation Channels](observation_channels.md)
- [The Great Magenta Confusion](../learn/great_magenta_confusion.md)
