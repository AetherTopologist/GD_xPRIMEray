# Project Glowing Heart v1.8.5 — Architecture Vision & Version Strategy

Atlas integration reference: the [Atlas Constitution](../Observatory/Observation_Atlas/ATLAS_CONSTITUTION.md) governs observer, representation, evidence, translation, and claim-boundary language used by future Glowing Heart interfaces.

You are working inside `/home/bb/code/godot_xPRIMEray`.

This is a documentation and roadmap task.

Mission:

Create a dedicated architecture vision document for Project Glowing Heart as its own mini-versioned program inside xPRIMEray.

This document should explain how Glowing Heart evolves from:

```txt
CLI Core Artifact
```

to:

```txt
Artifact Viewer
```

to:

```txt
Interactive Observatory Workbench
```

to:

```txt
Godot / Core Comparison Pipeline
```

to:

```txt
Interactive Heart
```

The purpose is to guide future Codex, Claude, Grok, and human contributors so work can proceed in parallel without confusing Core truth, Godot visualization, and public-facing demo language.

This is NOT implementation.

This is NOT a site redesign.

This is NOT runtime execution.

This is a strategy/architecture document.

---

## Hard Boundaries

Do not run Godot.

Do not modify Godot scenes.

Do not modify Core transport.

Do not modify renderer lifecycle files:

* `GrinFilmCamera.cs`
* `RenderTestRunner.cs`
* `SnapshotBuilder.cs`

Do not modify production catalog:

```txt
reports/observatory_catalog.json
```

Do not claim parity.

---

## Create

```txt
Docs/xPRIMEray/project_glowing_heart_architecture_vision.md
```

Optionally update:

```txt
Docs/xPRIMEray/project_glowing_heart_v1_8_milestone.md
```

with one small link to this vision doc, if appropriate.

---

## Required Structure

```md
# Project Glowing Heart Architecture Vision

## Purpose

Project Glowing Heart is the mini-versioned engine-heart program inside xPRIMEray.

It exists to extract, stabilize, observe, validate, and eventually interactively explore the xPRIMEray transport core without binding the engine heart to Godot.

## Core Principle

The Core is the instrument.

Godot is the first observatory shell.

MisterY Labs / public pages are the human-facing map.

No frontend is allowed to create stronger claims than the artifacts support.

## Program Layers

### Layer 1 — Heart

Standalone Core, CLI, fixtures, transport, artifacts.

### Layer 2 — Evidence

Manifests, snapshots, observatory entries, galleries.

### Layer 3 — Bridge

Shared fixture schema, observer contract, snapshot/channel contract, target alignment, future difference packets.

### Layer 4 — Observatory Shells

Godot first. Future shells may include web viewer, Unreal, or other environments.

### Layer 5 — Public Interface

Claim-safe language, demo captions, progress pages, UI audit, visitor comprehension.

## Mini-Version Strategy

Use this structure:

| Range | Theme | Meaning |
|---|---|---|
| v0.x | Heartbeat | Core exists and emits artifacts |
| v1.x | Bridge | Core and Godot learn shared vocabulary |
| v2.x | Measurement | Difference packets, snapshot/channel contracts, artifact viewer |
| v3.x | Interactive Heart | Users edit observer/field/testbench parameters |
| v4.x | Multi-Observer Observatory | multiple observers, detectors, instruments |
| v5.x | Adapter Ecosystem | Godot, web, Unreal, future shells |

## Current Status

As of v1.8.x:

- Shared observer vocabulary exists.
- Core/Godot observer instances exist.
- Direct reconciliation exists.
- Shared observer target exists.
- Target alignment exists.
- Pixel comparison is not ready.
- Parity claim remains NONE.

## Near-Term Path

### v1.9 — Shared Snapshot & Measurement Contract

Defines what a snapshot channel means.

### v2.0 — Difference Packet Design

Defines how two observations can be compared without claiming parity.

### v2.1 — Static Artifact Viewer

GitHub Pages-friendly viewer for existing Glowing Heart packets.

### v2.2 — First Godot Demo Packet

Claim-safe Godot screenshot/output packet for the selected candidate.

### v2.3 — Core/Godot Side-by-Side Packet

Core artifact beside Godot artifact with no equivalence claim.

### v3.0 — Interactive Heart

A basic workbench where users can edit fixture parameters:
- field center X/Y/Z
- field radius
- field amplitude
- observer position
- observer direction
- FOV
- resolution
- step count

Then generate a new Core artifact packet.

## Parallel Work Streams

### Stream A — Core / Artifact / Contract

Best agents:
Codex, Claude

Responsibilities:
- schemas
- tools
- fixtures
- manifests
- validation
- reconciliation
- artifact viewer

### Stream B — Godot Observatory UI

Best agents:
Codex, Grok, Claude

Responsibilities:
- demo candidate selection
- screenshot capture guidance
- Godot UI readability
- safe overlay text
- visual affordances
- public-facing polish

### Stream C — Public Interface / MisterY Labs

Best agents:
Grok, Claude, GPT

Responsibilities:
- safe captions
- visitor comprehension
- progress pages
- claim boundaries
- site navigation
- "what this is / is not" language

## Mirror Rule

Every Godot-facing demo improvement should mirror back into Glowing Heart artifacts when possible.

Every Glowing Heart contract improvement should eventually inform Godot UI or demo structure.

But neither side should block the other unless a claim boundary is at risk.

## Claim Boundary Rule

Allowed:
- engineering prototype
- perceptual demo
- observatory visualization
- artifact packet
- bridge candidate
- no parity claim

Forbidden:
- physics proof
- validated wormhole
- pixel parity
- closure proven
- matches reality
- endorsed by institution or researcher

## What v3.0 Means

Interactive Heart is not a decorative GUI.

It is a fixture authoring workbench.

The user edits a testbench.

The system emits reproducible artifacts.

The artifact trail remains inspectable.

A GUI is only valid if it writes back to the same fixture/schema/artifact system.

## Success Definition

Project Glowing Heart succeeds when:

1. users can author or modify a simple fixture,
2. run it through the standalone Core,
3. inspect the resulting artifact packet,
4. compare it against other observations when appropriate,
5. understand exactly what is and is not claimed.
```

---

## Verification

Run:

```bash
head -260 Docs/xPRIMEray/project_glowing_heart_architecture_vision.md

git status --short -- reports/observatory_catalog.json GrinFilmCamera.cs RendererCore/Testing/RenderTestRunner.cs GodotAdapter/SnapshotBuilder.cs
```

Expected:

* Vision doc exists.
* No protected files touched.
* No parity claims.
* v3.0 Interactive Heart is defined as fixture-authoring workbench, not decorative GUI.

---

## Final Report

Report:

1. File created
2. Whether milestone page was linked
3. Key version bands
4. Parallel work streams
5. v3.0 definition
6. Confirmation protected files untouched
