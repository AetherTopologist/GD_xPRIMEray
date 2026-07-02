# Glowing Heart v2.13 Evidence Chain Release Candidate Preview

Release status: **RC PASS**

Glowing Heart v2.x is frozen as a Core-vs-Core evidence-chain release candidate for recorded comparison decisions and their generated views.

## Artifact Chain

```text
Difference Packet Index
  -> Atlas Graph
  -> Evidence Map SVG
  -> Gallery Markdown
  -> Evidence Map Index
  -> Evidence Chain Health Check
  -> Preflight
```

Canonical sources are the Difference Packet Index JSON, Atlas Graph JSON, Evidence Map Index JSON, schemas, fixtures, and tools. Gallery Markdown, Evidence Map SVG, previews, health reports, and preflight reports are generated views and should not be hand-edited for exhibit values.

## Locked Boundaries

- Core-vs-Core only.
- No Godot comparison.
- No image or pixel comparison.
- No parity claim.
- No physical validation claim.
- No renderer equivalence claim.
- No proof claim.
- Zero Core difference does not establish equivalence with another runtime or measurement system.
- Non-zero difference demonstrates numeric distinction between retained Core artifacts only.

## Release Candidate Checklist

| Criterion | Status |
|---|---|
| Full v2.12 preflight | `PASS` |
| v2.11 Evidence Chain health | `PASS` |
| Five exhibits | `PASS` |
| Status counts: Comparable 2, Unknown 2, NotComparable 1 | `PASS` |
| Atlas Graph validation | `PASS` |
| SVG five-card and safety checks | `PASS` |
| MkDocs build | `PASS` |
| Core Godot dependency scan | `PASS` |
| Protected-file check | `PASS` |

## Stable Promotion Gate

Stable status still requires a schema-version decision, CI/preflight adoption, an external audit, language review, generated-artifact naming policy, clean-clone reproducibility, and a Glowing Heart link check.

## Next Milestone

**Glowing Heart v3.0 — Observer Fixture Dashboard Seed** can generalize the five-case set across multiple observer, fixture, and channel combinations without changing these claim boundaries.

