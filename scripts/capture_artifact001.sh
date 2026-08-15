#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUTPUT="${ARTIFACT001_OUTPUT:-${1:-$ROOT/output/cathedral_probe/artifact001}}"
ENGINE_COMMIT="${ARTIFACT001_ENGINE_COMMIT:-$(git -C "$ROOT" rev-parse HEAD)}"
GODOT_BIN="${GODOT_BIN:-godot4-mono}"

echo "[Artifact001] output=$OUTPUT"
echo "[Artifact001] engine_commit=$ENGINE_COMMIT"
ARTIFACT001_OUTPUT="$OUTPUT" \
ARTIFACT001_ENGINE_COMMIT="$ENGINE_COMMIT" \
"$GODOT_BIN" --path "$ROOT" --benchmark-lock=true --benchmark-seed=1 --script res://scripts/capture_artifact001.gd
