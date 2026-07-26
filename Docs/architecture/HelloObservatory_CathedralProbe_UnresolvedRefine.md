PROJECT

xPRIMEray
Hello Observatory 1.5
Cathedral Probe: Unresolved Region Refinement

CONTEXT

Hello Observatory 1.4 introduced:

- interactive normalized FieldStrength control
- Gallery and Hermetic display presets
- GrinFilmCamera LIVE / SNAPSHOT / OFF behavior
- NormalRGB film output
- magenta miss sentinel
- fixed-pose field response experiments
- observer instrumentation and evidence fixtures
- a Godot-hosted interactive Observatory Workbench

The current live demo produces structured magenta regions where transport does not resolve a geometry hit under the current ray policy. A wormhole or portal surface is an especially useful example because the unresolved region has coherent image-space structure and a meaningful relationship to transport topology.

We now want to transform magenta from a terminal “no hit” color into an interactive scientific state:

UNRESOLVED UNDER THE CURRENT PROBE POLICY

The user should be able to identify an unresolved image region and ask the instrument to allocate deeper transport effort specifically to that region.

The desired interaction is conceptually:

film sample
→ transport attempt
→ persistent outcome record
→ unresolved mask
→ coherent region selection
→ targeted refinement
→ updated evidence

The user experience should feel intuitive enough that a non-specialist can discover a seam visually and ask:

“Think harder here.”

ARCHITECTURAL BOUNDARY

The core Cathedral Probe architecture must live inside xPRIMEray C#.

The Godot side must remain an adapter and interface layer.

Godot may:

- display film and diagnostic overlays
- receive user input
- request probe operations
- show telemetry
- manage scene integration
- export evidence artifacts
- translate scene state into transport inputs

Godot must not become the owner of:

- per-pixel probe memory
- transport termination classification
- region refinement policy
- step-budget allocation
- transport result truth
- core scheduling logic
- connected-region scientific state
- portable evidence semantics

The long-term direction includes:

- Blender harness trials
- non-Godot adapters
- deterministic console fixtures
- scientific evidence bundles
- potentially other scene hosts

Therefore the Cathedral Probe core must not depend on:

- Godot node types
- Godot Image or Texture objects
- Godot input systems
- Godot scene trees
- editor-only state
- Python runtime execution
- Blender APIs

Python may be used externally for offline analysis and experimental policy research, but it must not sit inside the live per-ray stepping loop.

CURRENT DOCTRINE

- measurements before pictures
- transport engine separate from observer instrumentation
- observer records separate from presentation
- diagnostic images are evidence, not proof of physical correctness
- field strength changes transport policy without modifying authored field amplitudes
- camera pose, geometry, field state, and transport policy must be captured in evidence
- protected renderer and observer boundaries should remain stable unless a change is justified
- no claims beyond what the measurement proves
- do not silently reinterpret magenta without recording the termination reason

PRIMARY RESEARCH QUESTION

Can xPRIMEray persist enough per-film-sample transport state to identify unresolved image-space regions and selectively rerun those samples with a deeper or altered probe policy, while preserving deterministic transport semantics and a portable adapter boundary?

SECONDARY QUESTIONS

- What does one film pixel need to remember?
- What exactly counts as unresolved?
- What are the termination classes?
- Can coherent regions be discovered without reading rendered RGB pixels?
- How should regions be ranked and selected?
- How does LIVE mode interact with persistent refinement state?
- What invalidates prior probe memory?
- How do camera, geometry, field, film resolution, and policy changes affect cache validity?
- How should progressive work be scheduled?
- What can remain synchronous for Stage 0?
- What eventually needs threading or job scheduling?
- How can this later serve Blender without redesign?
- How do we preserve reproducibility?
- How do we prevent “deeper” from being interpreted as “more true”?
- How do we distinguish no geometry, exited world, budget exhaustion, topology candidate, numerical failure, and invalid scene state?

TARGET MILESTONE

Hello Observatory 1.5
Cathedral Probe Stage 0

Candidate bounded scope:

- persistent per-sample outcome code
- explicit termination reason
- unresolved mask generated from transport results
- connected image-space region discovery
- selected-region refinement request
- deeper step budget for selected samples only
- refinement level per sample
- before/after unresolved counts
- fixed camera and scene state validation
- portable C# core API
- minimal Godot adapter controls and telemetry
- deterministic console acceptance fixture
- evidence export suitable for later Blender trials

Do not assume this scope is correct. Audit the living repository first.