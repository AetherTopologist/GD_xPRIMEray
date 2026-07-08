# Observer PNG Doctrine

Every PNG produced by the Observer Instrumentation layer is a **diagnostic artifact**,
not a rendered image. It encodes instrument observations — not light transport, not
renderer output, not texture samples drawn from the Godot scene. This doctrine defines
what a conforming Observer PNG is, how it must be constructed, and how it is accepted.

All existing and future Observer PNG artifacts are bound by this doctrine.
Deviation from any rule is a defect, not a design choice.

---

## Purpose

An Observer PNG makes one frame of instrument observations human-inspectable.
It is produced from a single completed frame of `InstrumentObservation` records.
Its pixels encode diagnostic state — which rays resolved, which failed, which class
of surface was sampled — so a human can verify instrument correctness without a renderer.

An Observer PNG is **not**:
- a rendered frame
- a texture sampled from the Godot scene
- an accumulation across multiple frames
- a screenshot or visual parity check against the renderer
- evidence of optical closure or physical correctness

---

## Canonical Colors

All Observer PNGs use `Image.Format.Rgba8`. Every color below is the exact 8-bit RGBA
representation. No other colors may appear in a conforming artifact.

| Class              | Name          | R   | G   | B   | A   | Hex (RRGGBBAA) |
|--------------------|---------------|-----|-----|-----|-----|----------------|
| Dark sample        | Black         | 0   | 0   | 0   | 255 | `000000FF`     |
| Light sample       | White         | 255 | 255 | 255 | 255 | `FFFFFFFF`     |
| Unresolved probe   | Magenta       | 255 | 0   | 255 | 255 | `FF00FFFF`     |
| Non-surface ray    | Gray          | 128 | 128 | 128 | 255 | `808080FF`     |
| Unregistered slot  | Transparent   | 0   | 0   | 0   | 0   | `00000000`     |

These values are invariant. Future instruments may add new semantic classes only
by appending to this table in a doctrine revision. They may not reuse an existing
color for a different meaning.

---

## Pixel Semantics

Each pixel class has exactly one meaning. The mapping from observation state to pixel
class is defined per-instrument in the instrument's specification. The following
definitions are universal across all Observer PNGs:

**Transparent** `(0,0,0,0)` — no diagnostic pixel was written at this film coordinate.
This may mean no debug ray slot mapped to the coordinate in the current frame, or the
coordinate remained outside the diagnostic projection. It must not be interpreted as a
transport result.

**Gray** `(128,128,128,255)` — the ray at this coordinate produced a transport
classification of `TransportClassNotSurfaceHit`, `OtherGeometryHit`, or was absorbed,
resulting in no instrument resolving to `RegionSampled`. The ray ran; it simply did
not hit an instrumented surface.

**Magenta** `(255,0,255,255)` — a probe-named surface was hit, but the instrument
failed to resolve a valid observation. Causes include: metadata missing from the
catalog, UV out of domain, instrument math failure. Magenta is a **failure signal**.
Zero magenta pixels is required for acceptance.

**White** `(255,255,255,255)` — the primary instrument produced a `RegionSampled`
observation whose boolean `SampleValue` is `false` (light / off). Currently defined
for `CheckerProbeInstrument`.

**Black** `(0,0,0,255)` — the primary instrument produced a `RegionSampled`
observation whose boolean `SampleValue` is `true` (dark / on). Currently defined
for `CheckerProbeInstrument`.

Future instrument classes that produce a non-boolean primary result (e.g.,
`SampledColor` from `TextureSampleInstrument`) will extend this table with new
canonical colors under a doctrine revision rather than redefining existing ones.

---

## One Observation = One Pixel

Each pixel is painted exactly once, from the observations of a single `RayIndex`.
The observation corresponding to `_dbgHitPx[ri]`, `_dbgHitPy[ri]` is written to
pixel `(px, py)`. If multiple instruments produce observations for the same `RayIndex`,
the pixel encodes the **primary instrument's** state, defined by the specific fixture.
Secondary observations (e.g., `SurfaceUv`) are used for validation but do not paint
pixels independently.

No pixel is painted twice. No pixel accumulates across observations.

---

## No Interpolation

Upscales are nearest-neighbor only. The upscale loop is defined as:

```
for oy in [0, upH):
  for ox in [0, upW):
    upImg[ox, oy] = srcImg[ox / UpscaleFactor, oy / UpscaleFactor]
```

No bilinear, no bicubic, no antialias, no gamma correction in the upscale pass.
An upscale is a pixel replication only. The canonical color table is preserved
exactly in every upscaled pixel.

---

## No Renderer Dependency

An Observer PNG depends only on:

1. `InstrumentObservation` records from a single completed frame
2. The coordinate spans `GetDebugRayPixelXForTesting()` / `GetDebugRayPixelYForTesting()`
3. `FilmFrameIdentity` (film width, height, frame sequence, scene tag)
4. The canonical color table above

It does not depend on:

- Any Godot renderer texture, `Image.GetPixel` from a rendered frame, or `SetData`
- `FilmOverlay2D`, `SnapshotBuilder`, `FrameSnapshotBus`, or `RenderTestRunner`
- Any transport ray segment data beyond `_dbgHitPx`/`_dbgHitPy` coordinates
- Godot physics, collision, or broadphase state
- The visual appearance of the Godot viewport

Godot `Image` may be used only as a deterministic PNG-writing container in headless
fixtures. It must not read from or compare against the rendered viewport, renderer
texture, `FilmOverlay2D` output, or any Godot material display.

An Observer PNG that requires the renderer to produce its pixels is not an Observer PNG.

---

## Deterministic Ordering

Pixels are painted in debug ray slot order: `ri = 0, 1, …, coordCount-1`. The
coordinate for slot `ri` is `(_dbgHitPx[ri], _dbgHitPy[ri])`, set at the moment
the transport engine wrote `_dbgHits[ri]`. This order is determined by the transport
engine's commit loop and is stable for a given scene and configuration.

Two Observer PNGs from the same scene, configuration, and frame sequence must be
pixel-identical.

---

## Artifact Provenance

Every Observer PNG must be accompanied by a console log line at the moment of write.
The required fields are:

```
[OI-NNN] PNG written: <absolute-path> (<W>x<H>) [scale=Nx] coordCount=<N> maxRayIndex=<N>
```

Optional fields (per-instrument): `black=N white=N magenta=N gray=N transparent=N`

The log line is the provenance record. It must be emitted before the fixture calls
`GetTree().Quit()`. An Observer PNG written without its provenance log line is
non-conforming.

File path convention:

```
res://output/observer_instrumentation/oi_NNN_<descriptor>[_upscaled].png
```

The directory `output/observer_instrumentation/` must be created by the fixture
using `DirAccess.MakeDirRecursiveAbsolute` before the first `SavePng` call.

---

## Acceptance Rules

A conforming Observer PNG run must satisfy all of the following. Violation of any
rule is a test failure.

**Coordinate span integrity** (validated before any pixel is written):
- `GetDebugRayPixelXForTesting().Length == GetDebugRayPixelYForTesting().Length`
- `maxRayIndex < coordCount` (no observation references a slot beyond the span)
- All `obs.RayIndex` values satisfy `0 ≤ RayIndex < coordCount`
- All `(px, py)` satisfy `0 ≤ px < filmWidth` and `0 ≤ py < filmHeight`

**Pixel correctness** (validated after write by counting pixels in the source image):
- Zero magenta pixels (`magenta == 0`)
- Both signal classes present where the instrument is active (e.g., `black > 0 && white > 0` for the checker diagnostic)
- No pixel color other than the five canonical colors appears

**Instrument correctness carryover** (from the OI-NNN acceptance run that precedes PNG write):
- All numeric acceptance criteria for the frame pass before `WriteDiagnosticPng` is called
- PNG write is conditional on instrument acceptance — a failed frame does not produce a PNG

**Upscale consistency** (when an upscaled variant exists):
- `upscaled_count(class) == source_count(class) × UpscaleFactor²` for every class
- No pixel in the upscaled image differs in color from its source slot

**File system**:
- `oi_NNN_<descriptor>.png` exists at the reported path
- `SavePng` returned `Error.Ok`

---

## Future Extensions

New Observer PNG artifacts are added by:

1. Assigning the next available `OI-NNN` number
2. Defining which instrument(s) paint which pixel class(es) in a new section of the
   instrument's specification
3. If a new semantic class is needed, appending a row to the canonical color table
   under a doctrine revision — not reusing an existing color
4. Writing a new headless fixture (`OiNNNXxxxxHeadless.cs`) that:
   - Passes the existing OI-NNN acceptance criteria before calling `WriteDiagnosticPng`
   - Validates all coordinate span integrity rules
   - Validates pixel counts per canonical class
   - Emits the provenance log line
5. Registering the new PNG path in this section

### Currently registered Observer PNGs

| Artifact | File | Source instrument | Scale |
|---|---|---|---|
| OI-006 | `oi_006_checker_diagnostic.png` | `CheckerProbeInstrument` | 1× (source) |
| OI-007 | `oi_007_checker_diagnostic_upscaled.png` | OI-006 upscale | 16× |
| OI-009 | *(planned)* `oi_009_texture_sample_diagnostic.png` | `TextureSampleInstrument` | TBD |

---

*This doctrine is authoritative. When a conflict exists between this document and
any fixture implementation, the doctrine governs and the fixture is the defect.*
