---
po_doc_type: development
title: Commit Discipline
status: partial
engine_commit: "5ce15c13"
generated: false
---

# Commit Discipline (documentation lane)

## Scope for ontology / docs commits

**Stage only:**

- `Docs/**`
- `mkdocs.yml`
- documentation-only supporting assets if necessary

**Do not stage** in docs commits:

- engine / C# / Godot scenes / shaders
- tests that change transport or observation contracts
- `bin/` · `obj/` · generated evidence dumps unless intentionally curated

## Message style

Prefer scoped messages, e.g.:

```text
docs(observatory): align GitHub Pages ontology
```

## Metadata honesty

On experiment pages, use:

- `planned` · `unknown` · `not yet assigned` · `not yet qualified`

Never invent verified commits, SceneIds, or last qualified runs.

## Claim scrub before merge

Search docs for:

- `magenta = no hit`
- present-tense planned channels
- proper time / physical gravity / verified wormholes without qualification
- unexplained bare `GrinFilmCamera` / `Cathedral` as first-run language

## Historical packages

Keep weekend / milestone package notes under Development or as labeled historical. Do not erase Atlas / Gallery / Glowing Heart lanes.
