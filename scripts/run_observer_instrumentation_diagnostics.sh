#!/usr/bin/env bash
# OI-013A: Observer Instrumentation Diagnostics bundle.
# Runs existing OI headless scenes and writes a timestamped evidence bundle.
#
# Usage: bash scripts/run_observer_instrumentation_diagnostics.sh <timestamp>
#
# Guardrail: does not modify the renderer, transport pipeline, or
# reports/observatory_catalog.json. PNG artifacts encode instrument
# observations, not rendered pixels.

set -u -o pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
GODOT_BIN="${GODOT_BIN:-$ROOT/scripts/godot_local.sh}"
TIMESTAMP="${1:-$(date -u +%Y%m%dT%H%M%SZ)}"
BUNDLE_DIR="$ROOT/output/observer_instrumentation/runs/$TIMESTAMP"

mkdir -p "$BUNDLE_DIR/artifacts" "$BUNDLE_DIR/logs"

LOG_MAIN="$BUNDLE_DIR/logs/bundle.log"
exec > >(tee -a "$LOG_MAIN") 2>&1

echo "[OI-013A] Observer Instrumentation Diagnostics"
echo "[OI-013A] timestamp=$TIMESTAMP"
echo "[OI-013A] bundle=$BUNDLE_DIR"
echo "[OI-013A] guardrail=diagnostic-only; no transport change; no catalog write"

# Fixture definitions (parallel arrays — bash 3 compat, bash 4 safe).
OI_IDS=(    "oi_001"                          "oi_005"                              "oi_006"                                "oi_011"                                    "oi_012"                                      )
OI_LABELS=( "OI-001"                          "OI-005"                              "OI-006"                                "OI-011"                                    "OI-012"                                      )
OI_NAMES=(  "Equator UV Band Probe"            "Checker Chain Verification"           "Checker Diagnostic PNG"                "Texture CPU Source Parity"                 "Texture Sample Diagnostic PNG"                )
OI_SCENES=( "res://test-oi-001-equator.tscn"  "res://test-oi-005-checkerboard.tscn" "res://test-oi-006-checker-diagnostic.tscn" "res://test-oi-011-texture-source-parity.tscn" "res://test-oi-012-texture-sample-diagnostic.tscn" )
# OI_CORE=1 → failure affects overall verdict; 0 → informational only.
OI_CORE=(   "1"                               "1"                                   "1"                                     "0"                                         "0"                                           )

OI_VERDICTS=()
OI_EXIT_CODES=()
for _i in "${!OI_IDS[@]}"; do
    OI_VERDICTS+=("SKIP")
    OI_EXIT_CODES+=("na")
done

RUN_START_EPOCH="$(date +%s)"

# ---------------------------------------------------------------------------
# Run each OI headless scene.
# ---------------------------------------------------------------------------
for idx in "${!OI_IDS[@]}"; do
    oi_id="${OI_IDS[$idx]}"
    label="${OI_LABELS[$idx]}"
    scene="${OI_SCENES[$idx]}"
    scene_fs="$ROOT/${scene#res://}"

    echo ""
    echo "[$label] --- ${OI_NAMES[$idx]} ---"

    if [[ ! -f "$scene_fs" ]]; then
        echo "[$label] SKIP scene file not found: $scene_fs"
        OI_VERDICTS[$idx]="SKIP"
        OI_EXIT_CODES[$idx]="na"
        continue
    fi

    log_path="$BUNDLE_DIR/logs/${oi_id}.log"

    set +e
    "$GODOT_BIN" --headless --path "$ROOT" --scene "$scene" \
        > "$log_path" 2>&1
    oi_exit=$?
    set -e

    if [[ "$oi_exit" -eq 0 ]]; then
        verdict="PASS"
    else
        verdict="FAIL"
    fi

    OI_VERDICTS[$idx]="$verdict"
    OI_EXIT_CODES[$idx]="$oi_exit"

    echo "[$label] $verdict exit=$oi_exit log=$log_path"

    # Echo the fixture's own summary line from its log.
    if [[ -s "$log_path" ]]; then
        grep -E "^\[$label\]" "$log_path" | tail -3 || true
    fi
done

# ---------------------------------------------------------------------------
# Copy PNG artifacts into the bundle.
# ---------------------------------------------------------------------------
echo ""
echo "[OI-013A] --- copying artifacts ---"

copy_artifact() {
    local rel_src="$1"
    local dest_name="$2"
    local src="$ROOT/$rel_src"
    local dest="$BUNDLE_DIR/artifacts/$dest_name"
    if [[ -f "$src" ]]; then
        cp -f "$src" "$dest"
        echo "[OI-013A] copied: $dest_name"
    else
        echo "[OI-013A] not found (skipped): $rel_src"
    fi
}

copy_artifact "output/observer_instrumentation/oi_006_checker_diagnostic.png"                "oi_006_checker_diagnostic.png"
copy_artifact "output/observer_instrumentation/oi_007_checker_diagnostic_upscaled.png"       "oi_007_checker_diagnostic_upscaled.png"
copy_artifact "output/observer_instrumentation/oi_012_texture_sample_diagnostic.png"         "oi_012_texture_sample_diagnostic.png"
copy_artifact "output/observer_instrumentation/oi_012_texture_sample_diagnostic_upscaled.png" "oi_012_texture_sample_diagnostic_upscaled.png"

# ---------------------------------------------------------------------------
# Compute overall verdict.
# Core fixtures (OI-001, OI-005, OI-006) must all PASS.
# Extended fixtures (OI-011, OI-012) are informational.
# ---------------------------------------------------------------------------
overall="PASS"
core_pass=0
core_fail=0
ext_pass=0
ext_fail=0
skip_count=0

for idx in "${!OI_IDS[@]}"; do
    v="${OI_VERDICTS[$idx]}"
    is_core="${OI_CORE[$idx]}"
    if [[ "$v" == "SKIP" ]]; then
        skip_count=$((skip_count + 1))
    elif [[ "$is_core" == "1" ]]; then
        if [[ "$v" == "PASS" ]]; then
            core_pass=$((core_pass + 1))
        else
            core_fail=$((core_fail + 1))
            overall="FAIL"
        fi
    else
        if [[ "$v" == "PASS" ]]; then
            ext_pass=$((ext_pass + 1))
        else
            ext_fail=$((ext_fail + 1))
        fi
    fi
done

RUN_END_EPOCH="$(date +%s)"
DURATION_SECONDS=$(( RUN_END_EPOCH - RUN_START_EPOCH ))
TIMESTAMP_ISO="$(date -u +%Y-%m-%dT%H:%M:%SZ)"

# ---------------------------------------------------------------------------
# Write summary.json
# ---------------------------------------------------------------------------
echo ""
echo "[OI-013A] --- writing summary.json ---"

# Build the fixtures object one entry per fixture.
fixtures_json=""
for idx in "${!OI_IDS[@]}"; do
    oi_id="${OI_IDS[$idx]}"
    label="${OI_LABELS[$idx]}"
    name="${OI_NAMES[$idx]}"
    is_core="${OI_CORE[$idx]}"
    verdict="${OI_VERDICTS[$idx]}"
    exit_code="${OI_EXIT_CODES[$idx]}"

    core_bool="false"
    [[ "$is_core" == "1" ]] && core_bool="true"

    exit_json="null"
    [[ "$exit_code" != "na" ]] && exit_json="$exit_code"

    entry=$(printf '    "%s": { "label": "%s", "name": "%s", "core": %s, "verdict": "%s", "exit_code": %s, "log": "logs/%s.log" }' \
        "$oi_id" "$label" "$name" "$core_bool" "$verdict" "$exit_json" "$oi_id")

    if [[ -n "$fixtures_json" ]]; then
        fixtures_json="${fixtures_json},"$'\n'
    fi
    fixtures_json="${fixtures_json}${entry}"
done

cat > "$BUNDLE_DIR/summary.json" <<SUMMARY_EOF
{
  "schema": "xprimeray.observer_instrumentation.diagnostics.v1",
  "run_id": "oi_diagnostics_${TIMESTAMP}",
  "timestamp": "${TIMESTAMP_ISO}",
  "duration_seconds": ${DURATION_SECONDS},
  "bundle": "${BUNDLE_DIR}",
  "overall_verdict": "${overall}",
  "core_pass": ${core_pass},
  "core_fail": ${core_fail},
  "extended_pass": ${ext_pass},
  "extended_fail": ${ext_fail},
  "skipped": ${skip_count},
  "guardrail": "diagnostic-only; no transport change; no catalog write; PNG artifacts encode instrument observations not rendered pixels",
  "fixtures": {
${fixtures_json}
  }
}
SUMMARY_EOF

echo "[OI-013A] summary.json written"

# ---------------------------------------------------------------------------
# Write observer_instrumentation_report.md
# ---------------------------------------------------------------------------
echo "[OI-013A] --- writing observer_instrumentation_report.md ---"

# Use BT for literal backtick characters in markdown.
BT='`'

# Build fixture table rows.
table_rows=""
for idx in "${!OI_IDS[@]}"; do
    label="${OI_LABELS[$idx]}"
    name="${OI_NAMES[$idx]}"
    verdict="${OI_VERDICTS[$idx]}"
    exit_code="${OI_EXIT_CODES[$idx]}"
    category="Extended"
    [[ "${OI_CORE[$idx]}" == "1" ]] && category="Core"

    table_rows+="| ${label} | ${name} | ${category} | **${verdict}** | ${exit_code} |"$'\n'
done

# Build artifacts list.
artifacts_section=""
add_artifact_link() {
    local dest_name="$1"
    local description="$2"
    if [[ -f "$BUNDLE_DIR/artifacts/$dest_name" ]]; then
        artifacts_section+="- [${dest_name}](artifacts/${dest_name}) — ${description}"$'\n'
    fi
}

add_artifact_link "oi_006_checker_diagnostic.png"                "OI-006 source 40x22 RGBA8; black=dark checker, white=light checker"
add_artifact_link "oi_007_checker_diagnostic_upscaled.png"       "OI-007 nearest-neighbor 16x upscale 640x352; pixel classes preserved"
add_artifact_link "oi_012_texture_sample_diagnostic.png"         "OI-012 texture sample source; pixels encode sampled RGBA texel color"
add_artifact_link "oi_012_texture_sample_diagnostic_upscaled.png" "OI-012 texture sample 16x upscale"

[[ -z "$artifacts_section" ]] && artifacts_section="_No PNG artifacts produced in this run._"

# Build per-fixture log list.
logs_list=""
for idx in "${!OI_IDS[@]}"; do
    logs_list+="- ${BT}logs/${OI_IDS[$idx]}.log${BT} — ${OI_LABELS[$idx]} ${OI_NAMES[$idx]}"$'\n'
done

cat > "$BUNDLE_DIR/observer_instrumentation_report.md" <<REPORT_EOF
# Observer Instrumentation Diagnostics

**Run:** ${BT}${TIMESTAMP}${BT}
**Bundle:** ${BT}${BUNDLE_DIR}${BT}
**Overall verdict:** **${overall}**
**Duration:** ${DURATION_SECONDS}s

---

## Guardrail

This report is produced by Observer Instrumentation headless diagnostic fixtures.
It does not modify the renderer, transport pipeline, or observatory catalog.
PNG artifacts encode instrument observations, not rendered pixels.

---

## Fixture Verdicts

| Fixture | Name | Category | Verdict | Exit |
|---------|------|----------|---------|------|
${table_rows}
Core fixtures (OI-001, OI-005, OI-006) determine the overall verdict.
Extended fixtures (OI-011, OI-012) are informational; their failure does not affect the overall verdict.

Summary: core_pass=${core_pass} core_fail=${core_fail} extended_pass=${ext_pass} extended_fail=${ext_fail} skipped=${skip_count}

---

## Artifacts

${artifacts_section}
---

## Logs

Raw per-fixture command output:

${logs_list}
---

## Observer PNG Doctrine

Pixel color semantics (canonical RGBA8):

| Class | Hex | Meaning |
|-------|-----|---------|
| Black | ${BT}000000FF${BT} | Dark checker sample (SampleValue=true) |
| White | ${BT}FFFFFFFF${BT} | Light checker sample (SampleValue=false) |
| Magenta | ${BT}FF00FFFF${BT} | Unresolved probe hit — failure signal; zero required |
| Gray | ${BT}808080FF${BT} | Non-surface ray (miss, absorbed, other geometry) |
| Transparent | ${BT}00000000${BT} | No debug ray slot mapped to this film coordinate |

---

*OI-013A diagnostic bundle. Not a transport validation result. Not a renderer comparison.*
REPORT_EOF

echo "[OI-013A] observer_instrumentation_report.md written"

# ---------------------------------------------------------------------------
# Final summary.
# ---------------------------------------------------------------------------
echo ""
echo "[OI-013A] overall=${overall} core_pass=${core_pass} core_fail=${core_fail} extended_pass=${ext_pass} extended_fail=${ext_fail} skipped=${skip_count} duration=${DURATION_SECONDS}s"
echo "[OI-013A] done bundle=${BUNDLE_DIR}"

if [[ "$overall" == "PASS" ]]; then
    exit 0
else
    exit 1
fi
