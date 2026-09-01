#!/usr/bin/env bash
set -euo pipefail

log_file="$(mktemp)"
trap 'rm -f "$log_file"' EXIT

timeout 120s godot4-mono --path . --headless \
  --script res://scripts/test_live_tlas_scene_context.gd >"$log_file" 2>&1

rg -q 'LIVE_TLAS_SCENE_CONTEXT PASS current_scene=ObservatoryWorkbench' "$log_file"
rg -q '\[LiveTLAS\]\[SnapshotBuild\] geometryEntities=[1-9][0-9]* tlasNodes=[1-9][0-9]* tlasRoot=[0-9]+' "$log_file"

echo "LIVE TLAS SCENE CONTEXT PASS"
rg '\[SNAPSHOT\] geomCount=|\[LiveTLAS\]\[SnapshotBuild\]|LIVE_TLAS_SCENE_CONTEXT' "$log_file" | head -6
