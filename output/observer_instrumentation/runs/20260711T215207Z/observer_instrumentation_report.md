# Observer Instrumentation Diagnostics

**Run:** `20260711T215207Z`
**Bundle:** `/home/bb/code/godot_xPRIMEray/output/observer_instrumentation/runs/20260711T215207Z`
**Overall verdict:** **PASS**
**Duration:** 4s

---

## Guardrail

This report is produced by Observer Instrumentation headless diagnostic fixtures.
It does not modify the renderer, transport pipeline, or observatory catalog.
PNG artifacts encode instrument observations, not rendered pixels.

---

## Fixture Verdicts

| Fixture | Name | Category | Verdict | Exit |
|---------|------|----------|---------|------|
| OI-001 | Equator UV Band Probe | Core | **PASS** | 0 |
| OI-005 | Checker Chain Verification | Core | **PASS** | 0 |
| OI-006 | Checker Diagnostic PNG | Core | **PASS** | 0 |
| OI-011 | Texture CPU Source Parity | Extended | **PASS** | 0 |
| OI-012 | Texture Sample Diagnostic PNG | Extended | **PASS** | 0 |

Core fixtures (OI-001, OI-005, OI-006) determine the overall verdict.
Extended fixtures (OI-011, OI-012) are informational; their failure does not affect the overall verdict.

Summary: core_pass=3 core_fail=0 extended_pass=2 extended_fail=0 skipped=0

---

## Artifacts

- [oi_006_checker_diagnostic.png](artifacts/oi_006_checker_diagnostic.png) — OI-006 source 40x22 RGBA8; black=dark checker, white=light checker
- [oi_007_checker_diagnostic_upscaled.png](artifacts/oi_007_checker_diagnostic_upscaled.png) — OI-007 nearest-neighbor 16x upscale 640x352; pixel classes preserved
- [oi_012_texture_sample_diagnostic.png](artifacts/oi_012_texture_sample_diagnostic.png) — OI-012 texture sample source; pixels encode sampled RGBA texel color
- [oi_012_texture_sample_diagnostic_upscaled.png](artifacts/oi_012_texture_sample_diagnostic_upscaled.png) — OI-012 texture sample 16x upscale

---

## Logs

Raw per-fixture command output:

- `logs/oi_001.log` — OI-001 Equator UV Band Probe
- `logs/oi_005.log` — OI-005 Checker Chain Verification
- `logs/oi_006.log` — OI-006 Checker Diagnostic PNG
- `logs/oi_011.log` — OI-011 Texture CPU Source Parity
- `logs/oi_012.log` — OI-012 Texture Sample Diagnostic PNG

---

## Observer PNG Doctrine

Pixel color semantics (canonical RGBA8):

| Class | Hex | Meaning |
|-------|-----|---------|
| Black | `000000FF` | Dark checker sample (SampleValue=true) |
| White | `FFFFFFFF` | Light checker sample (SampleValue=false) |
| Magenta | `FF00FFFF` | Unresolved probe hit — failure signal; zero required |
| Gray | `808080FF` | Non-surface ray (miss, absorbed, other geometry) |
| Transparent | `00000000` | No debug ray slot mapped to this film coordinate |

---

*OI-013A diagnostic bundle. Not a transport validation result. Not a renderer comparison.*
