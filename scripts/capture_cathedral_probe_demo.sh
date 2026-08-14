#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SCENE_ID="portable_observatory.gallery.v1"
FILM="80x45"
FIELD="0.0"
STEPS="80"
STEP_LENGTH="0.07"
OUTPUT_ROOT="${ROOT_DIR}/output/cathedral_probe"
RUN_ID="gallery-v1-field-${FIELD}-${FILM}"
ENGINE_COMMIT="$(git -C "$ROOT_DIR" rev-parse HEAD 2>/dev/null || echo unknown)"

while [[ $# -gt 0 ]]; do
	case "$1" in
		--scene) SCENE_ID="$2"; shift 2 ;;
		--film) FILM="$2"; shift 2 ;;
		--field) FIELD="$2"; RUN_ID="gallery-v1-field-${FIELD}-${FILM}"; shift 2 ;;
		--steps) STEPS="$2"; shift 2 ;;
		--step-length) STEP_LENGTH="$2"; shift 2 ;;
		--output) OUTPUT_ROOT="$2"; shift 2 ;;
		--engine-commit) ENGINE_COMMIT="$2"; shift 2 ;;
		*) echo "unknown option: $1" >&2; exit 2 ;;
	esac
done

OUTPUT_DIR="${OUTPUT_ROOT%/}/${RUN_ID}"
mkdir -p "$OUTPUT_DIR"
export CAPTURE_SCENE_ID="$SCENE_ID"
export CAPTURE_FILM="$FILM"
export CAPTURE_FIELD="$FIELD"
export CAPTURE_STEPS="$STEPS"
export CAPTURE_STEP_LENGTH="$STEP_LENGTH"
export CAPTURE_OUTPUT="$OUTPUT_DIR"
export CAPTURE_RUN_ID="$RUN_ID"
export CAPTURE_ENGINE_COMMIT="$ENGINE_COMMIT"

exec "$ROOT_DIR/scripts/godot_local.sh" --headless --path "$ROOT_DIR" --script res://scripts/capture_cathedral_probe_demo.gd
