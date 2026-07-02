# Project Optical Closure — Overview

**Status:** Stage 0 · Documentation and safety framework only  
**Epistemic tier:** Established mathematics + Implemented engine behavior

Project Optical Closure is a controlled computational optics research program built on top of the
validated curved-ray transport system in xPRIMEray. It extends the **visual interpretive layer**
of the engine — it does not modify the validated transport or hit classification system.

## What this project explores

A curved-ray transport system bends ray paths through space. Given a probe sphere, a GRIN field
volume, and a specific observer position, some parts of the sphere's surface may not be reachable
by any ray the observer can cast — not because the geometry is hidden, but because the transport
model curves rays away from that surface.

This is **observer-dependent optical accessibility**: the question of whether a declared
probe-region is visually sampled by the existing validated transport result from a given observer
pose.

## What this project is not

- It is not a claim that hidden geometry exists in physical reality.
- It is not a modification to the validated hit pipeline.
- It is not a new intersection system.
- It is not proof of portals, hidden dimensions, or non-Euclidean physics.
- It is not an extension of hermetic closure validation.

## Who this is for

**Graphics programmers and engineers** — the implementation is a pure post-hit interpretive layer
using existing hit data. No new intersection authority is introduced.

**Physicists and optics researchers** — the transport model is GRIN (gradient index) optics, a
well-established framework. The results are reproducible fixtures, not extraordinary claims.

**Artists and lore-curious readers** — the visual phenomena here are genuinely interesting. The
project names the boundary between "what the simulation shows" and "what that means" clearly and
honestly. Curiosity is welcome; conclusions are earned.

**Game developers and Shadertoy explorers** — the checkerboard UV probe is a classic verification
tool for ray-surface interaction. The dent geometry adds observer-dependent interest.

## The core distinction

> **Hermetic closure ≠ Optical closure.**
>
> Hermetic closure is the validated transport property: every pixel is classified.
> Optical closure is an interpretive diagnostic: a probe region is not sampled by the observer's
> transport result. These are independent concepts. Optical closure does not replace, extend, or
> modify hermetic closure.

## Navigation

- [Architecture Safety Audit](project_oc_001_architecture_audit.md) — full technical audit with
  Claude/Grok reconciliation table and staged roadmap
- [OC-001 Minimal Optical Closure](oc_001_fixture.md) — fixture design and expected outputs
- [Epistemic Airlock](optical_closure_epistemic_airlock.md) — language safety framework
- [Glossary](optical_closure_glossary.md) — terminology reference
- [Roadmap](optical_closure_roadmap.md) — staged implementation plan
