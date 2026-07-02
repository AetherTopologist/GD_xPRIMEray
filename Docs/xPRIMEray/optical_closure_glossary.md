# Optical Closure — Terminology Glossary

**Project Optical Closure · Language Reference**

---

**validated hit remains sovereign**
: The existing hit system (`RayBeamRenderer` → `HitPayload` → `HadHit`) is the only authoritative
  intersection event. No code in Phase 1 introduces a competing intersection path.

**post-hit optical probe interpretation**
: Computation that happens after the validated hit produces a surface contact, using only the data
  that hit already produced (position, normal, collider ID, transport classification). No new
  intersection occurs.

**probe metadata**
: A lightweight resource declaring probe sphere center, radius, role, and material parameters.
  Used only for post-hit UV computation and diagnostic classification. Not intersection
  configuration.

**observer-dependent optical accessibility**
: A post-hit diagnostic describing whether a declared probe-region is visually sampled by the
  existing validated transport result from a given observer pose. Not hermetic closure. Not a
  transport classification.

**transport-mediated appearance**
: How a surface looks through the curved-ray transport model from a given observer position.
  Different from how the surface would look under straight-line transport.

**diagnostic overlay**
: A visual output derived from post-hit interpretation data. UV map, checker overlay, or
  accessibility class map. Not transport truth. Not validation evidence.

**hermetic closure**
: The validated transport property: every pixel in the output grid is classified (hit or miss).
  Defined by `ClosureValidator`. Unchanged by Project Optical Closure.

**optical closure** *(use with care)*
: An interpretive concept: a probe region that is not sampled by any ray from the current
  observer's transport result. Not the same as hermetic closure. Do not use as a raw pixel
  classification until observer sweeps have validated the behavior. Prefer `probe_region_not_sampled`
  or `closure_candidate` until then.

**probe_region_not_sampled**
: The preferred diagnostic state label when the probe's declared region of interest is not reached
  by the current observer's transport result. Replaces "optically closed" until validated.

**closure candidate**
: A pixel or region where `probe_region_not_sampled` is observed and consistent across a range of
  nearby observer poses. May be promoted to "optical closure" after observer sweep validation.

**simulation-bounded claim**
: A statement about what the simulation produces, explicitly bounded to the simulation. Does not
  extend to claims about physical reality.

**reproducible fixture**
: A declared, pinned fixture that produces the same output on any clone of the repository. OC-001
  is a reproducible fixture once implemented.

**inspiration, not evidence**
: The distinction between lore/mythology as creative fuel and lore/mythology as proof. MisterY
  Labs treats the former as inspiration for measurable experiments, not as conclusions to validate.

**epistemic tier**
: One of six categories — Established Mathematics, Implemented Engine Behavior, Validated Fixture
  Output, Experimental Interpretation, Lore/Artistic Inspiration, Open Questions — used to label
  every claim on the site.

**curiosity is welcome; conclusions are earned**
: The project's anchor phrase for community framing. The question is the invitation; the
  reproducible fixture is the answer's boundary.
