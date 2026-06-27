# Project Glowing Heart v1.8 Observer Milestone

## At a glance

**v1.8 defines and measures the observer bridge** — the shared camera contract that xPRIMEray-Core and Godot must agree on before any pixel-by-pixel comparison can run.

**Pixel comparison and parity are not ready yet.** Core and Godot still disagree on several observer fields. The milestone reports those gaps explicitly; it does not close them.

This is engineering groundwork at perceptual-demo / prototype level. It prepares future `difference.ppm` work; it does not prove renderer parity, transport equivalence, or pixel equality.

---

## What the observer bridge is

Before comparing two renders pixel by pixel, both sides must describe the same virtual camera: where it sits, which way it looks, field of view, resolution, and how pixels are sampled and indexed.

v1.7 named that vocabulary in a shared observer contract. **v1.8 turns that contract into concrete instances, measures how Core and Godot differ, defines a single target both should meet, and reports remaining gaps per side.**

That measurement trail is the observer bridge.

---

## What v1.8 accomplished

| Step | What happened | Visitor takeaway |
|------|---------------|------------------|
| [v1.8](project_glowing_heart_v1_8_observer_instances.md) | Placed Core and Godot observer metadata side by side for one shared fixture | Both sides can now be described in one vocabulary |
| [v1.8.1](project_glowing_heart_v1_8_1_observer_reconciliation.md) | Compared the two instances directly | They do not match; pixel comparison was blocked |
| [v1.8.2](project_glowing_heart_v1_8_2_shared_observer_target.md) | Wrote a single shared observer target both sides should adopt | A concrete alignment goal exists |
| [v1.8.3](project_glowing_heart_v1_8_3_observer_target_alignment.md) | Measured each side against that target | Remaining work is explicit per side |

All steps produced preview artifacts only. No Godot runtime was executed for these reports.

---

## Current status

| Question | Answer |
|----------|--------|
| Is the observer bridge defined? | **Yes** — shared contract (v1.7), instances, target, and alignment reports (v1.8) |
| Has the bridge been measured? | **Yes** — reconciliation and per-side target alignment reports list concrete deltas |
| Is pixel comparison ready? | **No** — `pixel_comparison_ready=false` in alignment output |
| Is parity claimed? | **No** — `parity_claim=NONE` throughout |
| Was Godot executed at runtime? | **No** — static metadata and fixture inference only |

---

## Safe to say publicly

- Project Glowing Heart is actively building Core/Godot comparison infrastructure.
- v1.8 established an observer bridge: instances, a shared target, and alignment measurements.
- The project can name exactly which camera fields still block pixel comparison.
- Progress is real engineering work at prototype level.

## Do not claim

- Renderer parity between Core and Godot.
- Pixel equivalence or successful `difference.ppm` comparison.
- Transport or runtime equivalence.
- Physics validation, hermetic closure, or any scientific proof.
- That Godot was run to verify camera behavior — these reports use static metadata.

---

## Sub-milestone detail

### v1.8 — Observer instances

Created side-by-side observer instances for Core and Godot. Core used an explicit smoke observer from `fixtures/grin_radial_smoke.json`; Godot used a static `Camera3D` candidate inferred from the selected fixture.

Outputs:

```txt
reports/glowing_heart_observer_instances.preview.json
reports/glowing_heart_observer_instances.preview.md
```

### v1.8.1 — Observer reconciliation

Compared Core and Godot instances directly. Blocking deltas were found in position, forward direction, FOV, Godot resolution, Godot sampling metadata, Godot image-origin metadata, Godot aspect-policy metadata, and Core near/far specification.

Outputs:

```txt
reports/glowing_heart_observer_reconciliation.preview.json
reports/glowing_heart_observer_reconciliation.preview.md
```

### v1.8.2 — Shared observer target

Created a shared observer target. The target uses the explicit Core smoke observer for pose, FOV, resolution, sampling, image origin, and aspect policy, and deliberately adopts the selected Godot candidate `far=40` clip value to reduce one known future mismatch.

Outputs:

```txt
fixtures/shared/glowing_heart_observer_target.v0.preview.json
reports/glowing_heart_shared_observer_target.preview.md
```

### v1.8.3 — Target alignment

Compared each observer instance against the shared target:

- Core observer → shared target
- Godot observer → shared target

This separates remaining work by side. Core primarily needs explicit near/far and normalized contract emission. Godot needs target camera pose, target forward direction (or a documented adapter transform), FOV 60° vertical, resolution 40×22, and exported sampling/origin/aspect metadata.

Outputs:

```txt
reports/glowing_heart_observer_target_alignment.preview.json
reports/glowing_heart_observer_target_alignment.preview.md
```

---

## What's next

Close the reported alignment gaps on both sides. Pixel comparison stays blocked until Core and Godot each match the shared target. The next engineering steps are contract emission on Core and camera configuration (or documented transforms) on Godot — not a public parity announcement.