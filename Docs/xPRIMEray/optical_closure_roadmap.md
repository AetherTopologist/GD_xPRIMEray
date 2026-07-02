# Project Optical Closure — Roadmap

**Status:** Stage 0 complete · Stage 1 pending implementation  
**Constraint:** No new intersection authority at any stage before Stage 4.

---

## Stage 0 — Documentation and Safety Framework ✅

*No code. No schema. No fixture.*

- [x] Architecture safety audit with Claude/Grok reconciliation
- [x] Project overview page
- [x] Epistemic airlock page
- [x] Glossary
- [x] Roadmap
- [x] mkdocs.yml nav entry

**Acceptance:** All documentation pages render. The architecture audit records the "do not touch"
list. The nav entry is present as a single section under Project Glowing Heart.

---

## Stage 1 — Post-Hit Procedural UV and Checker

*Godot film/diagnostic layer. No transport changes. No new intersection.*

New files, all downstream of validated hit:

- `ProbeMetadataResource.cs` — maps `ColliderName → ProbeEntry (center, radius, role, material)`
- `SphericalUvHelper.cs` — pure static: `FromNormal()`, `CheckerState()`, `IsDentRegion()`
- `OpticalProbeInterpreter.cs` — takes `HitPayload` + `ProbeMetadataResource` + observer pose;
  produces `ProbeInterpretation`

**Acceptance criterion:** Given any existing `HitPayload` with `HadHit = true`, the interpreter
produces a `ProbeInterpretation` without calling any new intersection code. Unit tests cover:
probe region sampled, probe surface hit outside region, other geometry hit, transport not surface
hit, unresolved.

**Do not start Stage 1 until:**

- Q2 (ColliderId stability) is answered
- Q3 (FilmOverlay2D integration point) is answered

---

## Stage 2 — Diagnostic Overlays and Observer Sweep

*New output writers in Godot diagnostic pipeline. No transport changes.*

- UV overlay image writer (PPM or PNG, false-color)
- Checker overlay image writer (binary)
- Accessibility diagnostic map writer (5-class false-color)
- Observer sweep table writer (CSV: observer origin row → diagnostic state counts)
- Plain-language output report (Markdown)

**Acceptance criterion:** Running the sweep at 5+ observer distances produces a CSV table showing
that moving closer reopens the probe region. Each row is a committed fixture run.

---

## Stage 3 — Optional Authored UV Lookup *(deferred)*

*Metadata-only. Still downstream of validated hit.*

- Optional `Vector2[]` UV table per probe in `ProbeMetadataResource`
- Angular-proximity lookup from `localNormal` to authored UV (not barycentric — no triangle hit needed)
- Falls back to analytic spherical when no authored table present

**Only start after Stage 2 observer sweep data is validated.**

---

## Stage 4 — Experimental Geometry Branch *(only if explicitly justified)*

*Separate experimental path. Explicitly not part of validated hit authority.*

- Separate Core experiment: sphere intersection in `TransportRunner` under a new, clearly labeled
  experimental mode
- Explicit schema version: `xprimeray.oc001.experimental.v0`
- Separate evidence chain — does not touch Glowing Heart schemas or channels
- Requires: a new milestone decision, a new audit, a new preflight
- All outputs labeled "experimental" — never promoted to validated without a separate verification pass

**Do not start Stage 4 without an explicit decision and a new architecture review.**

---

## Constraints (apply at every stage)

- Validated hit pipeline is sovereign at every stage.
- No new intersection code before Stage 4.
- No changes to hermetic closure definition.
- No changes to Glowing Heart evidence chain.
- No parity claims.
- No proof-of-portal language.
- All diagnostic outputs labeled with epistemic tier.
