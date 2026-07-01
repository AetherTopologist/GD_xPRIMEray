# Project Glowing Heart v2.7 Gallery Renderer

## What Changed

The Difference Packet Gallery is now generated from the v2.6 machine-readable index. `tools/glowing_heart_gallery_renderer.py` validates required exhibit structure and writes both the visitor-facing documentation page and report preview in stable index order.

The generated outputs include an introduction, status summary, one section per exhibit, entry-specific claim boundaries, the shared index boundary, and a future-tooling note.

## What This Demonstrates

The human-readable gallery can be reproduced deterministically from structured exhibit data. Metrics, fixture paths, channels, statuses, rules, reasons, and claim boundaries come directly from the index rather than hand-authored gallery values.

## What This Does Not Demonstrate

- Core-vs-Core only.
- Not a Godot comparison.
- Not image or pixel comparison.
- Not parity.
- Not physical validation.
- Not renderer equivalence.
- Renderer checks exhibit structure, not scientific correctness.

## Renderer Command

```bash
python3 tools/glowing_heart_gallery_renderer.py \
  reports/glowing_heart_v2_6_difference_packet_index.preview.json \
  reports/glowing_heart_v2_4_difference_packet_gallery.preview.md \
  Docs/xPRIMEray/project_glowing_heart_v2_4_difference_packet_gallery.md
```

## Source and Outputs

- source index: `reports/glowing_heart_v2_6_difference_packet_index.preview.json`
- index schema: `schemas/glowing_heart/difference_packet_index.v0.preview.json`
- report gallery: `reports/glowing_heart_v2_4_difference_packet_gallery.preview.md`
- documentation gallery: `Docs/xPRIMEray/project_glowing_heart_v2_4_difference_packet_gallery.md`

The renderer does not modify the source index. Both gallery outputs are intentionally byte-identical.

`lastVerifiedOutputPath` is a temporary verification trace, not a durable source artifact. The durable gallery source is the v2.6 JSON index together with its repository fixtures and reports; the renderer does not emit `/tmp` paths as durable links.

## Failure Modes

- missing or invalid index JSON: fail with the source location when available
- missing required top-level or entry field: fail with the field and entry position
- missing metric: fail with its entry and metric context
- empty entry or claim-boundary arrays: fail before writing
- duplicate exhibit ID or unsupported status: fail before writing
- overlapping input and output paths: fail before reading or writing output
- output I/O failure: report `FAIL` and return a non-zero exit code

Outputs use atomic replacement so a completed write does not expose a partially rendered file.

## Next Milestone

Glowing Heart v2.8 can bridge the five index exhibits into Atlas Graph nodes and relationships while preserving each exhibit claim boundary.
