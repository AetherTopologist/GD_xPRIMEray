#!/usr/bin/env python3
"""Generate the v1.5 public demo readiness gate preview.

This is claim-safety and demo-readiness observability only. It does not execute
Godot, modify scenes, touch Core transport, or claim parity.
"""

from __future__ import annotations

import json
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


INPUTS = [
    Path("reports/glowing_heart_bridge.preview.md"),
    Path("reports/glowing_heart_gap_matrix.preview.md"),
    Path("reports/glowing_heart_shared_fixture_candidate.preview.md"),
    Path("reports/glowing_heart_godot_fixture_candidates.preview.json"),
    Path("reports/glowing_heart_shared_fixture_instance.preview.md"),
    Path("Docs/xPRIMEray/project_glowing_heart_public_proposal.md"),
]
GODOT_CANDIDATES = Path("reports/glowing_heart_godot_fixture_candidates.preview.json")
SHARED_INSTANCE_CANDIDATES = [
    Path("fixtures/shared/glowing_heart_grin_bridge.v0.preview.json"),
    Path("Fixtures/shared/glowing_heart_grin_bridge.v0.preview.json"),
]
OUTPUT_JSON = Path("reports/glowing_heart_public_demo_readiness.preview.json")
OUTPUT_MD = Path("reports/glowing_heart_public_demo_readiness.preview.md")
PREFERRED_CANDIDATE = "Fixtures/fixture_hermetic_observatory_grin.tscn"

ALLOWED_CLAIMS = [
    "Project Glowing Heart is an active engineering initiative.",
    "xPRIMEray-Core has a standalone CLI artifact.",
    "The Core can load simplified JSON fixtures.",
    "The Core can emit deterministic field-driven bend-magnitude snapshots.",
    "The Core can generate observatory-compatible preview artifacts.",
    "A shared fixture bridge candidate has been identified.",
    "Godot parity is not claimed.",
]

FORBIDDEN_CLAIMS = [
    "xPRIMEray-Core matches Godot output.",
    "The Core proves hermetic closure.",
    "The Core validates wormhole physics.",
    "The Core is physically complete.",
    "The Core and Godot are pixel-equivalent.",
    "The selected Godot fixture has been executed by the Core.",
    "The public demo is a scientific validation.",
    "Any researcher or institution endorses the demo.",
]

SAFE_ARTIFACTS = [
    "snapshot_ascii.txt",
    "snapshot.ppm",
    "run_summary.md",
    "glowing_heart_gallery.preview.md",
    "glowing_heart_bridge.preview.md",
    "glowing_heart_gap_matrix.preview.md",
    "shared fixture instance preview",
]

UNSAFE_ARTIFACTS = [
    "Raw Godot scene claims without runtime screenshots",
    "Parity language",
    "Closure language unless explicitly marked missing/unknown",
    'Wormhole demo claims without "perceptual demonstration" label',
    "Physics proof language",
    "Endorsement language",
]

BLOCKING_DELTAS = [
    "Need one current Godot screenshot/output packet for selected candidate.",
    "Need claim-safe overlay text.",
    "Need visual/UI audit for readability.",
    'Need explicit "perceptual demonstration, not validation" label.',
    "Need mapping from public page to preview artifacts.",
    "Need decision on whether to show Core snapshot beside Godot screenshot.",
]

GROK_TASKS = [
    "Review public-facing language for normative safety and clarity.",
    "Rewrite demo captions to avoid parity/physics-proof claims.",
    "Suggest UI labels for Project Glowing Heart Godot demo.",
    "Audit whether a casual viewer could misunderstand the demo.",
    'Propose public page layout for "active engineering initiative" framing.',
]

CLAUDE_TASKS = [
    "Verify claims match artifacts.",
    "Verify no forbidden claims appear.",
    "Verify demo candidate selection is grounded in metadata.",
    "Verify glossary terms are used consistently.",
    "Verify parityClaim remains NONE.",
]


class ReadinessError(Exception):
    pass


def utc_now() -> str:
    return datetime.now(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")


def load_json_object(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:
        raise ReadinessError(f"{path}: failed to load JSON: {exc}") from exc
    if not isinstance(value, dict):
        raise ReadinessError(f"{path}: expected JSON object")
    return value


def require_inputs(paths: list[Path]) -> None:
    for path in paths:
        if not path.is_file():
            raise ReadinessError(f"{path}: required input not found")


def first_existing(paths: list[Path], label: str) -> Path:
    for path in paths:
        if path.is_file():
            return path
    choices = ", ".join(path.as_posix() for path in paths)
    raise ReadinessError(f"{label}: required input not found; checked {choices}")


def string_list(value: Any) -> list[str]:
    if not isinstance(value, list):
        return []
    return [item for item in value if isinstance(item, str)]


def select_candidate(index: dict[str, Any], shared_instance: dict[str, Any]) -> dict[str, Any]:
    source_links = shared_instance.get("sourceLinks") if isinstance(shared_instance.get("sourceLinks"), dict) else {}
    selected_path = source_links.get("godotFixturePath") if isinstance(source_links.get("godotFixturePath"), str) else PREFERRED_CANDIDATE
    candidates = index.get("candidates")
    if not isinstance(candidates, list):
        raise ReadinessError(f"{GODOT_CANDIDATES}: expected candidates array")

    for candidate in candidates:
        if isinstance(candidate, dict) and candidate.get("path") == selected_path:
            return candidate
    raise ReadinessError(f"{GODOT_CANDIDATES}: selected candidate not found: {selected_path}")


def build_candidate_entry(candidate: dict[str, Any]) -> dict[str, Any]:
    tags = string_list(candidate.get("detected_tags"))
    reason_bits = [
        "Selected by the shared fixture bridge as the Godot-side GRIN observatory candidate.",
        f"Static metadata tags: {', '.join(tags) if tags else 'none'}.",
        f"Transport hint: {candidate.get('transport_hint', 'unknown')}.",
        f"Closure hint: {candidate.get('closure_hint', 'unknown')}.",
        "Godot runtime is still required before screenshots, parity, or validation claims.",
    ]
    return {
        "path": candidate.get("path", PREFERRED_CANDIDATE),
        "rank": 1,
        "reason": " ".join(reason_bits),
        "demoSafety": "SAFE_WITH_LIMITS",
        "claimBoundary": "Visual / perceptual demo only; no parity or validation claim.",
    }


def build_report(generated: str, candidate: dict[str, Any]) -> dict[str, Any]:
    return {
        "schema": "xprimeray.glowing_heart.public_demo_readiness.v1.5",
        "generatedUtc": generated,
        "runtimeExecuted": False,
        "parityClaim": "NONE",
        "readiness": {
            "status": "NOT_READY_FOR_PARITY_DEMO",
            "safeForPublicProgressPage": True,
            "safeForGodotVisualDemoFraming": True,
            "safeForPhysicsValidationClaims": False,
            "safeForPixelParityClaims": False,
        },
        "allowedClaims": ALLOWED_CLAIMS,
        "forbiddenClaims": FORBIDDEN_CLAIMS,
        "safeArtifactsToShow": SAFE_ARTIFACTS,
        "unsafeArtifactsToAvoid": UNSAFE_ARTIFACTS,
        "bestDemoCandidates": [build_candidate_entry(candidate)],
        "blockingDeltas": BLOCKING_DELTAS,
        "recommendedGrokTasks": GROK_TASKS,
        "recommendedClaudeAuditTasks": CLAUDE_TASKS,
    }


def bullets(items: list[str]) -> str:
    return "\n".join(f"- {item}" for item in items)


def render_markdown(report: dict[str, Any]) -> str:
    readiness = report["readiness"]
    candidate = report["bestDemoCandidates"][0]
    return f"""# Project Glowing Heart Public Demo Readiness Gate (Preview)

## Generated

{report["generatedUtc"]}

## Runtime / Parity

- runtimeExecuted: {str(report["runtimeExecuted"]).lower()}
- parityClaim: {report["parityClaim"]}

## Readiness Verdict

- status: {readiness["status"]}
- safeForPublicProgressPage: {str(readiness["safeForPublicProgressPage"]).lower()}
- safeForGodotVisualDemoFraming: {str(readiness["safeForGodotVisualDemoFraming"]).lower()}
- safeForPhysicsValidationClaims: {str(readiness["safeForPhysicsValidationClaims"]).lower()}
- safeForPixelParityClaims: {str(readiness["safeForPixelParityClaims"]).lower()}

## Allowed Claims

{bullets(report["allowedClaims"])}

## Forbidden Claims

{bullets(report["forbiddenClaims"])}

## Safe Artifacts To Show

{bullets(report["safeArtifactsToShow"])}

## Unsafe Artifacts To Avoid

{bullets(report["unsafeArtifactsToAvoid"])}

## Best Candidates

- rank: {candidate["rank"]}
- path: {candidate["path"]}
- demoSafety: {candidate["demoSafety"]}
- claimBoundary: {candidate["claimBoundary"]}
- reason: {candidate["reason"]}

## Blocking Deltas

{bullets(report["blockingDeltas"])}

## Grok Handoff Notes

{bullets(report["recommendedGrokTasks"])}

## Claude Audit Notes

{bullets(report["recommendedClaudeAuditTasks"])}

## Next Milestone

v1.6 should create the first Grok handoff packet for public-facing demo/interface language and layout, while preserving the current no-parity, no-validation boundary.
"""


def main() -> int:
    try:
        require_inputs(INPUTS)
        candidate_index = load_json_object(GODOT_CANDIDATES)
        shared_instance_path = first_existing(SHARED_INSTANCE_CANDIDATES, "shared fixture instance")
        shared_instance = load_json_object(shared_instance_path)
        candidate = select_candidate(candidate_index, shared_instance)
        report = build_report(utc_now(), candidate)

        OUTPUT_JSON.parent.mkdir(parents=True, exist_ok=True)
        OUTPUT_MD.parent.mkdir(parents=True, exist_ok=True)
        OUTPUT_JSON.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        OUTPUT_MD.write_text(render_markdown(report), encoding="utf-8")
    except ReadinessError as exc:
        print(f"[glowing-heart-public-demo-readiness] ERROR: {exc}")
        return 1

    readiness = report["readiness"]
    print("[glowing-heart-public-demo-readiness]")
    print(f"status={readiness['status']}")
    print(f"safe_public_progress={str(readiness['safeForPublicProgressPage']).lower()}")
    print(f"safe_visual_demo_framing={str(readiness['safeForGodotVisualDemoFraming']).lower()}")
    print(f"safe_physics_validation={str(readiness['safeForPhysicsValidationClaims']).lower()}")
    print(f"safe_pixel_parity={str(readiness['safeForPixelParityClaims']).lower()}")
    print(f"parity_claim={report['parityClaim']}")
    print()
    print(f"wrote={OUTPUT_JSON.as_posix()}")
    print(f"wrote={OUTPUT_MD.as_posix()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
