# Project Glowing Heart — MisterY Labs Public Presentation Proposal

**Document status:** Planning pass, 2026-06-20  
**Authored by:** AetherTopologist / Billy Broch + Claude Sonnet 4.6  
**Audience:** AetherTopologist (site author), future web collaborators  
**Scope:** Proposal only — no site code, no nav changes, no redesign

---

## Design Principle

> Add, don't disrupt.

MisterY Labs has an existing identity, navigation, and observatory presence. Project Glowing Heart should appear as a **natural extension** of what's already there — not a rebrand, not a takeover. Visitors who don't care about the engineering initiative should not notice any change. Visitors who are looking for the instrument should find it immediately.

---

## The Story in One Paragraph

xPRIMEray was born inside a Godot project. It grew into something larger than any one engine could contain. Project Glowing Heart is the act of recognizing what the project has become: not a Godot plugin, but an optical transport observatory. The Core is being extracted from the engine shell so it can be the instrument it was always meant to be. Godot becomes the first telescope. Unreal, web, and future engines become future telescopes. And MisterY Labs becomes the observatory where the public can look through any of them.

---

## Umwelt Framing

Every organism perceives only the slice of reality its senses can access. A bee's umwelt includes ultraviolet, polarized light, magnetic fields. A fish's umwelt includes lateral-line pressure waves. A human's umwelt is narrow.

xPRIMEray expands the human optical umwelt. It makes visible what light *actually does* inside curved media — not a cartoon of it, but the geodesics that light would physically follow through a GRIN medium, a wormhole throat, a Schwarzschild geometry. These paths exist. They are mathematically precise. Most people have never seen them.

Project Glowing Heart is the act of building the instrument clean enough to carry that precision into any environment — into Godot, into Unreal, into a browser, into the hands of a student or a physicist or a filmmaker — without losing the rigor.

The bee sigil is not decoration. It is a reminder: the instrument determines what can be seen. If the instrument is limited to one engine, one platform, one rendering budget — so is the umwelt. Project Glowing Heart removes that limitation.

---

## Deliverable 1: Homepage Callout

**Placement:** Below the existing hero / above the observatory section. One block, non-disruptive.

**Proposed content:**

---

### Project Glowing Heart — Active Engineering Initiative

xPRIMEray-Core is being extracted from GD_xPRIMEray into a pure C# optical transport library.

The Core will contain all field physics, transport integrators, GRIN curvature math, fixture validation, and sweep automation — with zero dependency on Godot or any rendering engine.

Godot becomes the first adapter. Unreal, web, and future engines follow.

[→ Project Page](/projects/glowing-heart)  
[→ Observatory](/observatory)

---

**Style notes:**
- One heading, one paragraph, two links
- No graphics required — the text carries it
- Can include the bee sigil SVG if desired (15px, inline, left of heading)
- Does not replace any existing section; insert above observatory intro or below hero tagline

---

## Deliverable 2: Project Page

**URL proposal:** `/projects/glowing-heart` or `/glowing-heart`

**Page sections:**

---

### ⬡ Project Glowing Heart

*The Core becomes the instrument. Godot becomes the first telescope. MisterY Labs becomes the observatory.*

---

#### Mission

xPRIMEray asks: what does light actually do inside curved space?

Not a simulation of it. Not a cartoon. The actual geodesics — the paths that light would follow through a GRIN lens, a traversable wormhole throat, a Schwarzschild field — computed rigorously, step by step, using the same physics that govern real photons in curved media.

Until now, that computation lived inside a Godot project. Project Glowing Heart extracts it.

**xPRIMEray-Core** is a pure C# optical transport library. It contains:

- GRIN field acceleration systems (FieldSystem, FieldCurves, TLAS)
- Heuristic + RK45 metric null geodesic integrators
- Fixture-driven validation (closure, sweep, oracle comparison)
- CLI testbench: `xpr run-fixture`, `xpr sweep`, `xpr validate`

No Godot. No rendering engine. Just math.

---

#### Architecture

```
MisterY Labs
Observatory · Gallery · Inspiration Cards · Mythos
    ↓
Observer Shells
GD_xPRIMEray (Godot) · Unreal Adapter · Web / WASM
    ↓
XPrimeRay.Adapters
GodotSceneAdapter · UnrealAdapter · WebAdapter
    ↓
xPRIMEray-Core
Fields · Integrators · Transport · Fixtures · Validation
```

The Core does not know what Godot is. Adapters translate engine-specific concepts (scene trees, physics, camera APIs) into Core concepts (field parameters, observer position, geometry queries).

---

#### The Bee Sigil

Bee sigil asset placeholder: `assets/sigils/bee-sigil.svg`.

The bee navigates by polarized sky light — a channel of information entirely outside the human visual umwelt. It builds in hexagonal efficiency. It perceives what we cannot.

xPRIMEray-Core is the bee's instrument, not the bee's eye. It extends what can be seen, not what the seer prefers to see. The rigor is not in service of any engine. It is in service of the geometry.

---

#### Live Demo — Wormhole Transport (WASM)

*[Embed WASM iframe or screenshot + link]*

The wormhole transport fixture demonstrates:

- A negative-energy GRIN throat: n(r) < 1 achieved via negative-amplitude FieldSource3D
- 5 plasma orbs in an animated ring, creating a converging GRIN channel
- Rays from the source region bending through the throat and teleporting via linked WormholePortal
- Researcher quote overlay unlocking as rays traverse (Puthoff, Davis & Froning, Cramer, Miley)

This is not a validated closure fixture. It is a perceptual demonstration — an invitation to inhabit the view of a photon approaching a traversable geometry shortcut.

---

#### Phase Progress

| Phase | Goal | Status |
|---|---|---|
| 0 | Freeze & Baseline | — |
| 1 | Math Core Extraction | — |
| 2 | Transport Extraction | — |
| 3 | Fixture System | — |
| 4 | CLI Testbench | — |
| 5 | Godot Adapter | — |
| 6 | Validation Migration | — |
| 7 | Repository Split | — |
| 8 | Public Launch | — |

*(Status updates as phases complete)*

---

#### Links

- [xprimeray-core on GitHub](#) *(available at Phase 8)*
- [Observatory Catalog](/observatory)
- [Architecture Review](project_glowing_heart_review.md)
- [Glossary](../glossary.md)
- [Start Here for Contributors](../start_here.md)

---

## Deliverable 3: Observatory References

The observatory (`/observatory` or equivalent) already exists and links to `reports/observatory_catalog.json`. No structural changes are needed. The following additions integrate Project Glowing Heart:

### Addition 1: Observatory intro line

Add one sentence to the observatory intro:

> Observatory entries are generated by both the Godot test runner and the xPRIMEray-Core CLI testbench. All entries use the same manifest format.

### Addition 2: Filter / tag for CLI-generated entries

When CLI testbench entries appear in the catalog (Phase 4+), add a `"source": "cli"` field to distinguish them from Godot-generated entries. The observatory indexer can display this as a tag: `[CLI]` vs `[Godot]`.

### Addition 3: Wormhole transport showcase entry

```json
{
  "artifact_type": "wasm_showcase",
  "fixture": "wormhole_transport_demo",
  "category": "Demo",
  "run_id": "public_launch",
  "source_path": "https://misterylabs.dev/demos/glowing-heart/",
  "timestamp": "2026-XX-XXT00:00:00Z",
  "verdict": "OBSERVED",
  "notes": "Phase 8 public demo. GRIN wormhole transport with orb ring and WormholeTransportHUD."
}
```

This is the observatory's record of the showcase — the running log of what the instrument has been seen doing, now including its public face.

---

## Deliverable 4: Documentation Links

### For contributors (technical audience)

| Document | Purpose | Location |
|---|---|---|
| Project Glowing Heart Review | Engineering review, extraction candidates, risks | `Docs/xPRIMEray/project_glowing_heart_review.md` |
| Execution Plan | 8-phase roadmap with acceptance criteria | `Docs/xPRIMEray/project_glowing_heart_execution_plan.md` |
| Architecture Charter | Master design document | `Docs/_xPRIMEray_arch_charter_v3-ChatClaudeGrokCoherencePass2.md` |
| Glossary | Shared vocabulary for all contributors | `Docs/glossary.md` |
| Field Extraction Rules | How SnapshotBuilder extracts scene data | `Docs/spec_field_extraction_rules_1.md` |
| Ray Transport Interfaces | IMetricField, IIntegrator, IRayTransport spec | `Docs/spec_ray_transport_interfaces_1.md` |
| Start Here | Entry point for new contributors | `Docs/start_here.md` |

### For public audiences (non-technical)

| Link | Purpose |
|---|---|
| Project page (`/projects/glowing-heart`) | Mission, architecture diagram, live demo, phase progress |
| Observatory (`/observatory`) | Running log of what the instrument has seen |
| Inspiration Cards | Epistemic framing: Maxwell/GRIN, Gauss/Riemann, frontier physics |
| GitHub (Phase 8) | Source code for adventurous visitors |

---

## What This Is Not

Project Glowing Heart is not a claim about physics validity. The instruments in xPRIMEray model effective index media, GRIN optics, and metric-inspired transport. They do not prove wormholes exist, do not validate vacuum engineering claims, and do not endorse or derive from any specific researcher cited in the HUD overlay. The researcher names in `WormholeTransportHUD.cs` are inspirational context for a perceptual demonstration, not scientific endorsement.

This distinction is already present in `Docs/MisterYLabs/INSPIRATION_CARD_FEATURE_LINKS.md`:

> These cards describe ideas xPRIMEray is inspired by and resonates with. They do not claim that xPRIMEray proves, derives from, validates, or is endorsed by any researcher or institution.

The same framing applies to all public-facing Project Glowing Heart content.

---

## Implementation Notes (for site author)

1. The project page can be written in the existing CMS/static site format — no new tooling needed
2. The WASM embed requires a web server that can serve `.wasm` files with correct MIME type (`application/wasm`)
3. Bee sigil SVG exists at `assets/sigils/bee-sigil.svg` in the Core repo; copy to site assets
4. Phase progress tracker should be manually updated as phases complete (no CI integration needed initially)
5. Observatory additions (`"source": "cli"` tag) require a one-line change to `tools/observatory_indexer.py`
