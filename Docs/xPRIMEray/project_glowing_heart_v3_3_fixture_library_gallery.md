# Glowing Heart v3.3 — Fixture Library Gallery

**Status:** Release Candidate · All 4 fixtures PASS  
**Milestone:** First browsable automated fixture artifact library  
**Claim boundary:** Core smoke transport only · Not Godot comparison · Not parity · Not physical validation · Not renderer equivalence

---

## What this gallery is

A browsable index of every Core-runnable fixture that has been run through the xPRIMEray-Core CLI testbench as part of Project Glowing Heart v3.3. Each card shows the recorded transport artifact — bend-magnitude metric snapshots — with links to the retained artifact files.

Every output here is simulation-bounded. These are measurements of what the Core transport model does, not claims about physical reality.

---

## Status Summary

| | Count |
|---|---|
| Total fixture files in library | 56 |
| Core-runnable JSON fixtures | 4 |
| Selected and run | 4 |
| **PASS** | **4** |
| FAIL | 0 |
| Deferred (Godot-only) | 50 |
| Deferred (metadata-only) | 2 |

---

## Family: `grin_radial_smoke_family_v1`

Radial GRIN smoke transport through a spherical gradient-index field. Observer at `[0, 0, −2]`, field centered at origin. Each pixel records the cumulative bend-magnitude of the ray passing through that angle of the field volume. No geometry intersection is modeled; all rays are field-only.

The four fixtures in this family are distinguished by a single parameter change each, making them a minimal sensitivity test suite for the Core transport.

---

### 1 — `grin_radial_smoke` · Canonical Primary

| | |
|---|---|
| **Fixture** | `Fixtures/grin_radial_smoke.json` |
| **Grid** | 40 × 22 |
| **Rays** | 880 |
| **Field samples** | 28 160 |
| **GRIN amplitude** | 0.25 |
| **Observer FOV** | 60° |
| **Mean bend** | 9.83 × 10⁻⁴ |
| **Max bend** | 5.62 × 10⁻³ |
| **Validation** | ✅ PASS |
| **Channels** | `bend_magnitude_metric`, `traversal_step_count` |
| **Run ID** | `20260703T030244Z_grin_radial_smoke` |

**Artifacts (site-published):**  
[manifest.json](../assets/glowing_heart/v3_3/grin_radial_smoke/manifest.json) · [run_summary.md](../assets/glowing_heart/v3_3/grin_radial_smoke/run_summary.md) · [ray_metrics.csv](../assets/glowing_heart/v3_3/grin_radial_smoke/ray_metrics.csv)

**Resolution ladder:** [Baseline](../assets/glowing_heart/v3_4/grin_radial_smoke/smoke/snapshot_ascii.txt) · [Compact](../assets/glowing_heart/v3_4/grin_radial_smoke/mini/snapshot_ascii.txt) · [Gallery detail](../assets/glowing_heart/v3_4/grin_radial_smoke/standard/snapshot_ascii.txt) · [Full ladder](project_glowing_heart_v3_4_resolution_ladder.md)

**ASCII preview** (`bend_magnitude` · scale: `.` = low → `@` = high):
```
   ..:--==++**####%%%%####**++==--:..   
  ..::-==++**##%%%%%%%%%%##**++==-::..  
  .::--=++*###%%%@@@@@@%%%###*++=--::.  
 ..::-=++**#%%%@@@@@@@@@@%%%#**++=-::.. 
 ..:--=+**##%%@@@@@@@@@@@@%%##**+=--:.. 
..::-=++*##%%@@@@@@@@@@@@@@%%##*++=-::..
..:--=+**#%%@@@@@%%%%%%@@@@@%%#**+=--:..
..:--=+*##%%@@@@%%####%%@@@@%%##*+=--:..
..:-==+*##%@@@@%%#*++*#%%@@@@%##*+==-:..
.::-=++*#%%@@@@%#*+==+*#%@@@@%%#*++=-::.
.::-=++*#%%@@@@%#+=::=+#%@@@@%%#*++=-::.
.::-=++*#%%@@@@%#+=::=+#%@@@@%%#*++=-::.
.::-=++*#%%@@@@%#*+==+*#%@@@@%%#*++=-::.
..:-==+*##%@@@@%%#*++*#%%@@@@%##*+==-:..
..:--=+*##%%@@@@%%####%%@@@@%%##*+=--:..
..:--=+**#%%@@@@@%%%%%%@@@@@%%#**+=--:..
..::-=++*##%%@@@@@@@@@@@@@@%%##*++=-::..
 ..:--=+**##%%@@@@@@@@@@@@%%##**+=--:.. 
 ..::-=++**#%%%@@@@@@@@@@%%%#**++=-::.. 
  .::--=++*###%%%@@@@@@%%%###*++=--::.  
  ..::-==++**##%%%%%%%%%%##**++==-::..  
   ..:--==++**####%%%%####**++==--:..   
```

---

### 2 — `grin_radial_smoke_variant` · Sensitivity Variant (+20% amplitude)

| | |
|---|---|
| **Fixture** | `Fixtures/grin_radial_smoke_variant.json` |
| **Grid** | 40 × 22 |
| **Rays** | 880 |
| **Field samples** | 28 160 |
| **GRIN amplitude** | **0.30** (+20% vs canonical) |
| **Observer FOV** | 60° |
| **Mean bend** | 1.18 × 10⁻³ (+19.9% vs canonical) |
| **Max bend** | 6.71 × 10⁻³ (+19.4% vs canonical) |
| **Validation** | ✅ PASS |
| **Channels** | `bend_magnitude_metric`, `traversal_step_count` |
| **Run ID** | `20260703T030247Z_grin_radial_smoke_variant` |

**Artifacts (site-published):**  
[manifest.json](../assets/glowing_heart/v3_3/grin_radial_smoke_variant/manifest.json) · [run_summary.md](../assets/glowing_heart/v3_3/grin_radial_smoke_variant/run_summary.md) · [ray_metrics.csv](../assets/glowing_heart/v3_3/grin_radial_smoke_variant/ray_metrics.csv)

**Resolution ladder:** [Baseline](../assets/glowing_heart/v3_4/grin_radial_smoke_variant/smoke/snapshot_ascii.txt) · [Compact](../assets/glowing_heart/v3_4/grin_radial_smoke_variant/mini/snapshot_ascii.txt) · [Gallery detail](../assets/glowing_heart/v3_4/grin_radial_smoke_variant/standard/snapshot_ascii.txt) · [Full ladder](project_glowing_heart_v3_4_resolution_ladder.md)

**ASCII preview:**
```
   ..:--==++**####%%%%####**++==--:..   
  ..::-==++*###%%%%%%%%%%###*++==-::..  
  .::--=++*##%%%%@@@@@@%%%%##*++=--::.  
 ..::-=++*##%%%@@@@@@@@@@%%%##*++=-::.. 
 ..:--=+**#%%%@@@@@@@@@@@@%%%#**+=--:.. 
..::-=++*##%%@@@@@@@@@@@@@@%%##*++=-::..
..:--=+**#%%@@@@@%%%%%%@@@@@%%#**+=--:..
..:--=+*##%%@@@@%%####%%@@@@%%##*+=--:..
..:-==+*##%@@@@%%#*++*#%%@@@@%##*+==-:..
.::-=++*#%%@@@@%#*+==+*#%@@@@%%#*++=-::.
.::-=++*#%%@@@@%#+=::=+#%@@@@%%#*++=-::.
.::-=++*#%%@@@@%#+=::=+#%@@@@%%#*++=-::.
.::-=++*#%%@@@@%#*+==+*#%@@@@%%#*++=-::.
..:-==+*##%@@@@%%#*++*#%%@@@@%##*+==-:..
..:--=+*##%%@@@@%%####%%@@@@%%##*+=--:..
..:--=+**#%%@@@@@%%%%%%@@@@@%%#**+=--:..
..::-=++*##%%@@@@@@@@@@@@@@%%##*++=-::..
 ..:--=+**#%%%@@@@@@@@@@@@%%%#**+=--:.. 
 ..::-=++*##%%%@@@@@@@@@@%%%##*++=-::.. 
  .::--=++*##%%%%@@@@@@%%%%##*++=--::.  
  ..::-==++*###%%%%%%%%%%###*++==-::..  
   ..:--==++**####%%%%####**++==--:..   
```

!!! note "Sensitivity observation"
    A 20% increase in GRIN amplitude produces a ~20% increase in mean and max bend magnitude. The structural shape of the field is preserved. This is a simulation-bounded measurement of Core transport sensitivity. No physical interpretation is claimed.

---

### 3 — `grin_radial_smoke_observer_variant` · Observer Variant (+1° FOV)

| | |
|---|---|
| **Fixture** | `Fixtures/grin_radial_smoke_observer_variant.json` |
| **Grid** | 40 × 22 |
| **Rays** | 880 |
| **Field samples** | 28 160 |
| **GRIN amplitude** | 0.25 |
| **Observer FOV** | **61°** (+1° vs canonical) |
| **Mean bend** | 9.59 × 10⁻⁴ (−2.5% vs canonical) |
| **Max bend** | 5.62 × 10⁻³ (≈ same) |
| **Validation** | ✅ PASS |
| **Channels** | `bend_magnitude_metric`, `traversal_step_count` |
| **Run ID** | `20260703T030253Z_grin_radial_smoke_observer_variant` |

**Artifacts (site-published):**  
[manifest.json](../assets/glowing_heart/v3_3/grin_radial_smoke_observer_variant/manifest.json) · [run_summary.md](../assets/glowing_heart/v3_3/grin_radial_smoke_observer_variant/run_summary.md) · [ray_metrics.csv](../assets/glowing_heart/v3_3/grin_radial_smoke_observer_variant/ray_metrics.csv)

**Resolution ladder:** [Baseline](../assets/glowing_heart/v3_4/grin_radial_smoke_observer_variant/smoke/snapshot_ascii.txt) · [Compact](../assets/glowing_heart/v3_4/grin_radial_smoke_observer_variant/mini/snapshot_ascii.txt) · [Gallery detail](../assets/glowing_heart/v3_4/grin_radial_smoke_observer_variant/standard/snapshot_ascii.txt) · [Full ladder](project_glowing_heart_v3_4_resolution_ladder.md)

**ASCII preview:**
```
   ..::--=++***##########***++=--::..   
  ..::--=++**###%%%%%%%%###**++=--::..  
  ..:--==+**##%%%%@@@@%%%%##**+==--:..  
  .::-==+**##%%@@@@@@@@@@%%##**+==-::.  
 ..:--=++*##%%@@@@@@@@@@@@%%##*++=--:.. 
 ..:-==+**#%%@@@@@@@@@@@@@@%%#**+==-:.. 
 .::-=++*##%@@@@@@%%%%@@@@@@%##*++=-::. 
..::-=+**#%%@@@@%%####%%@@@@%%#**+=-::..
..:--=+*##%%@@@@%#****#%@@@@%%##*+=--:..
..:--=+*##%@@@@%#*+==+*#%@@@@%##*+=--:..
..:-==+*##%@@@@%#*=::=*#%@@@@%##*+==-:..
..:-==+*##%@@@@%#*=::=*#%@@@@%##*+==-:..
..:--=+*##%@@@@%#*+==+*#%@@@@%##*+=--:..
..:--=+*##%%@@@@%#****#%@@@@%%##*+=--:..
..::-=+**#%%@@@@%%####%%@@@@%%#**+=-::..
 .::-=++*##%@@@@@@%%%%@@@@@@%##*++=-::. 
 ..:-==+**#%%@@@@@@@@@@@@@@%%#**+==-:.. 
 ..:--=++*##%%@@@@@@@@@@@@%%##*++=--:.. 
  .::-==+**##%%@@@@@@@@@@%%##**+==-::.  
  ..:--==+**##%%%%@@@@%%%%##**+==--:..  
  ..::--=++**###%%%%%%%%###**++=--::..  
   ..::--=++***##########***++=--::..   
```

!!! note "Observer basis shift"
    A 1° FOV increase places the outer ray angles slightly wider, reducing the fraction of rays that intersect the peak-bend region of the field. The core `@` hotspot narrows in angular extent compared to the canonical. The structural symmetry is preserved; the angular mapping shifts.

---

### 4 — `grin_radial_smoke_resolution_variant` · Resolution Variant (41 × 22)

| | |
|---|---|
| **Fixture** | `Fixtures/grin_radial_smoke_resolution_variant.json` |
| **Grid** | **41 × 22** (+1 column vs canonical) |
| **Rays** | **902** (+22 vs canonical) |
| **Field samples** | 28 864 |
| **GRIN amplitude** | 0.25 |
| **Observer FOV** | 60° |
| **Mean bend** | 9.60 × 10⁻⁴ (≈ same) |
| **Max bend** | 5.62 × 10⁻³ (≈ same) |
| **Validation** | ✅ PASS |
| **Channels** | `bend_magnitude_metric`, `traversal_step_count` |
| **Run ID** | `20260703T030308Z_grin_radial_smoke_resolution_variant` |

**Artifacts (site-published):**  
[manifest.json](../assets/glowing_heart/v3_3/grin_radial_smoke_resolution_variant/manifest.json) · [run_summary.md](../assets/glowing_heart/v3_3/grin_radial_smoke_resolution_variant/run_summary.md) · [ray_metrics.csv](../assets/glowing_heart/v3_3/grin_radial_smoke_resolution_variant/ray_metrics.csv)

**Resolution ladder:** [Baseline](../assets/glowing_heart/v3_4/grin_radial_smoke_resolution_variant/smoke/snapshot_ascii.txt) · [Compact](../assets/glowing_heart/v3_4/grin_radial_smoke_resolution_variant/mini/snapshot_ascii.txt) · [Gallery detail](../assets/glowing_heart/v3_4/grin_radial_smoke_resolution_variant/standard/snapshot_ascii.txt) · [Full ladder](project_glowing_heart_v3_4_resolution_ladder.md)

The +1-column resolution sensitivity is specific to this fixture's native 41x22 baseline. Compact and Gallery detail use the shared family footprints, so they show density scaling rather than preserving that one-column delta.

**ASCII preview:**
```
   ..::--=++***###%%%%%###***++=--::..   
   .::--=++**###%%%%%%%%%###**++=--::.   
  ..::-==+**##%%%%@@@@@%%%%##**+==-::..  
  .::--=+**##%%%@@@@@@@@@%%%##**+=--::.  
 ..::-=++*##%%@@@@@@@@@@@@@%%##*++=-::.. 
 ..:--=+**#%%@@@@@@@@@@@@@@@%%#**+=--:.. 
 .::-==+*##%%@@@@@%%%%%@@@@@%%##*+==-::. 
..::-=++*#%%@@@@@%%###%%@@@@@%%#*++=-::..
..::-=+**#%%@@@@%##*+*##%@@@@%%#**+=-::..
..:--=+**#%%@@@@%#+=-=+#%@@@@%%#**+=--:..
..:--=+*##%%@@@%%*+-.-+*%%@@@%%##*+=--:..
..:--=+*##%%@@@%%*+-.-+*%%@@@%%##*+=--:..
..:--=+**#%%@@@@%#+=-=+#%@@@@%%#**+=--:..
..::-=+**#%%@@@@%##*+*##%@@@@%%#**+=-::..
..::-=++*#%%@@@@@%%###%%@@@@@%%#*++=-::..
 .::-==+*##%%@@@@@%%%%%@@@@@%%##*+==-::. 
 ..:--=+**#%%@@@@@@@@@@@@@@@%%#**+=--:.. 
 ..::-=++*##%%@@@@@@@@@@@@@%%##*++=-::.. 
  .::--=+**##%%%@@@@@@@@@%%%##**+=--::.  
  ..::-==+**##%%%%@@@@@%%%%##**+==-::..  
   .::--=++**###%%%%%%%%%###**++=--::.   
   ..::--=++***###%%%%%###***++=--::.   
```

---

## Deferred Fixture Classes

The following fixture categories exist in the repository but cannot currently run through the Core CLI testbench.

| Category | Count | Reason |
|---|---|---|
| Godot-only scene fixtures (`.tscn`) | 50 | Require Godot runtime · Hermetic closure, black hole, Einstein ring, GRIN visual, wormhole, boundary, metric, atomic orbital, overspace families |
| Shared metadata bridges | 2 | Schema/contract definition files; not runnable transport fixtures |

These are deferred without prejudice. As Core fixture format support expands to cover these transport configurations, they will graduate to the runnable gallery.

---

## Site Publishing Notes

All artifacts linked from this gallery are under `Docs/assets/glowing_heart/v3_3/` and are included in the MkDocs site build. Binary `.ppm` snapshot images are retained in `output/glowing_heart_library_v3_3/` (tracked in git for text artifacts) but are not published to the site (binary format, large). ASCII previews embedded above serve as the browsable snapshot.

Raw run packets (full CSV, heatmap, traversal step count) are in:

```
output/glowing_heart_library_v3_3/
├── 20260703T030244Z_grin_radial_smoke/
├── 20260703T030247Z_grin_radial_smoke_variant/
├── 20260703T030253Z_grin_radial_smoke_observer_variant/
└── 20260703T030308Z_grin_radial_smoke_resolution_variant/
```

---

## Index Sources

- Candidates: `reports/glowing_heart_v3_3_fixture_library_candidates.preview.json`
- Selection: `reports/glowing_heart_v3_3_fixture_library_selection.preview.json`
- Index JSON: `reports/glowing_heart_v3_3_fixture_library_index.preview.json`
- Index schema: `schemas/glowing_heart/fixture_library_index.v0.preview.json`
- Health report: `reports/glowing_heart_v3_3_fixture_library_health.preview.md`
