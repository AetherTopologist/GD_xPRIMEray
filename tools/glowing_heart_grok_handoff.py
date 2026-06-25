#!/usr/bin/env python3
"""Generate the v1.6 Grok public demo handoff packet preview.

This is communication and presentation observability only. It does not execute
Godot, modify scenes, touch Core transport, or claim parity.
"""

from __future__ import annotations

import json
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


INPUTS = [
    Path("reports/glowing_heart_public_demo_readiness.preview.json"),
    Path("reports/glowing_heart_public_demo_readiness.preview.md"),
    Path("reports/glowing_heart_gap_matrix.preview.md"),
    Path("reports/glowing_heart_bridge.preview.md"),
    Path("reports/glowing_heart_shared_fixture_instance.preview.md"),
    Path("reports/glowing_heart_godot_fixture_candidates.preview.json"),
]
READINESS_JSON = Path("reports/glowing_heart_public_demo_readiness.preview.json")
GODOT_CANDIDATES = Path("reports/glowing_heart_godot_fixture_candidates.preview.json")
OUTPUT_JSON = Path("reports/glowing_heart_grok_handoff.preview.json")
OUTPUT_MD = Path("reports/glowing_heart_grok_handoff.preview.md")
PREFERRED_CANDIDATE = "Fixtures/fixture_hermetic_observatory_grin.tscn"
PREFERRED_CANDIDATE_NAME = "fixture_hermetic_observatory_grin"

EXAMPLE_SAFE_CAPTIONS = [
    "Experimental observatory visualization.",
    "Perceptual demonstration of field-driven transport concepts.",
    "Active engineering prototype.",
    "Preview artifact from Project Glowing Heart.",
    "Visual exploration environment.",
    "Work in progress.",
]

EXAMPLE_UNSAFE_CAPTIONS = [
    "Proof of curved-light transport.",
    "Verified wormhole simulation.",
    "Physics validation complete.",
    "Pixel parity achieved.",
    "Scientifically confirmed closure.",
    "Matches reality.",
]

GROK_TASKS = [
    "Review public-facing language for normative safety and clarity.",
    "Rewrite demo captions to avoid parity, validation, and physics-proof claims.",
    "Suggest UI labels for the Project Glowing Heart Godot demo.",
    "Audit whether a casual viewer could misunderstand the demo as proof.",
    "Propose public page layout for active engineering initiative framing.",
    "Check readability of overlay text, captions, and artifact labels.",
    "Flag any wording that implies endorsement, closure, or pixel equivalence.",
]

UI_AUDIT_CHECKLIST = [
    "Can a casual visitor misunderstand this as proof?",
    "Is parity implied?",
    "Are labels visible?",
    "Is experimental status visible?",
    "Are screenshots dated?",
    "Are artifact sources linked?",
    "Can users distinguish Core vs Godot outputs?",
    "Does any wording imply validation?",
]

NEXT_MILESTONE = "v1.7 Demo Presentation Packet"


class HandoffError(Exception):
    pass


def utc_now() -> str:
    return datetime.now(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")


def load_json_object(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:
        raise HandoffError(f"{path}: failed to load JSON: {exc}") from exc
    if not isinstance(value, dict):
        raise HandoffError(f"{path}: expected JSON object")
    return value


def require_inputs(paths: list[Path]) -> None:
    for path in paths:
        if not path.is_file():
            raise HandoffError(f"{path}: required input not found")


def string_list(value: Any) -> list[str]:
    if not isinstance(value, list):
        return []
    return [item for item in value if isinstance(item, str)]


def select_candidate(index: dict[str, Any], readiness: dict[str, Any]) -> dict[str, Any]:
    candidates = readiness.get("bestDemoCandidates")
    selected_path = PREFERRED_CANDIDATE
    if isinstance(candidates, list) and candidates:
        first = candidates[0]
        if isinstance(first, dict) and isinstance(first.get("path"), str):
            selected_path = first["path"]

    godot_candidates = index.get("candidates")
    if not isinstance(godot_candidates, list):
        raise HandoffError(f"{GODOT_CANDIDATES}: expected candidates array")

    for candidate in godot_candidates:
        if isinstance(candidate, dict) and candidate.get("path") == selected_path:
            return candidate
    raise HandoffError(f"{GODOT_CANDIDATES}: selected candidate not found: {selected_path}")


def build_best_demo_candidate(readiness: dict[str, Any], candidate: dict[str, Any]) -> dict[str, Any]:
    ranked = readiness.get("bestDemoCandidates")
    ranked_entry: dict[str, Any] = {}
    if isinstance(ranked, list) and ranked and isinstance(ranked[0], dict):
        ranked_entry = ranked[0]

    tags = string_list(candidate.get("detected_tags"))
    return {
        "path": ranked_entry.get("path", PREFERRED_CANDIDATE),
        "name": PREFERRED_CANDIDATE_NAME,
        "rank": ranked_entry.get("rank", 1),
        "demoSafety": ranked_entry.get("demoSafety", "SAFE_WITH_LIMITS"),
        "claimBoundary": ranked_entry.get(
            "claimBoundary",
            "Visual / perceptual demo only; no parity or validation claim.",
        ),
        "reason": ranked_entry.get(
            "reason",
            "Selected by the shared fixture bridge as the Godot-side GRIN observatory candidate.",
        ),
        "detectedTags": tags,
        "transportHint": candidate.get("transport_hint", "unknown"),
        "closureHint": candidate.get("closure_hint", "unknown"),
        "godotRuntimeRequired": bool(candidate.get("godot_runtime_required", True)),
        "visualDemoOnly": True,
        "parityClaim": "NONE",
        "validationClaim": "NONE",
    }


def build_report(generated: str, readiness: dict[str, Any], candidate: dict[str, Any]) -> dict[str, Any]:
    return {
        "schema": "xprimeray.glowing_heart.grok_handoff.v1.6",
        "generatedUtc": generated,
        "runtimeExecuted": False,
        "parityClaim": "NONE",
        "projectStatus": {
            "phase": "Public Demo Preparation",
            "readiness": "SAFE_WITH_LIMITS",
        },
        "allowedClaims": string_list(readiness.get("allowedClaims")),
        "forbiddenClaims": string_list(readiness.get("forbiddenClaims")),
        "bestDemoCandidate": build_best_demo_candidate(readiness, candidate),
        "safeArtifacts": string_list(readiness.get("safeArtifactsToShow")),
        "unsafeArtifacts": string_list(readiness.get("unsafeArtifactsToAvoid")),
        "exampleSafeCaptions": EXAMPLE_SAFE_CAPTIONS,
        "exampleUnsafeCaptions": EXAMPLE_UNSAFE_CAPTIONS,
        "grokTasks": GROK_TASKS,
        "uiAuditChecklist": UI_AUDIT_CHECKLIST,
        "nextMilestone": NEXT_MILESTONE,
    }


def bullets(items: list[str]) -> str:
    return "\n".join(f"- {item}" for item in items)


def render_markdown(report: dict[str, Any]) -> str:
    status = report["projectStatus"]
    candidate = report["bestDemoCandidate"]
    return f"""# Project Glowing Heart Grok Handoff Packet

Runtime executed: false

Parity claim: NONE

## Project Status

- phase: {status["phase"]}
- readiness: {status["readiness"]}
- runtimeExecuted: {str(report["runtimeExecuted"]).lower()}
- parityClaim: {report["parityClaim"]}

## Safe Language

### Allowed Claims

{bullets(report["allowedClaims"])}

### Example Safe Captions

{bullets(report["exampleSafeCaptions"])}

### Safe Artifacts

{bullets(report["safeArtifacts"])}

## Unsafe Language

### Forbidden Claims

{bullets(report["forbiddenClaims"])}

### Example Unsafe Captions

{bullets(report["exampleUnsafeCaptions"])}

### Unsafe Artifacts

{bullets(report["unsafeArtifacts"])}

## Demo Candidate

- path: {candidate["path"]}
- name: {candidate["name"]}
- rank: {candidate["rank"]}
- demoSafety: {candidate["demoSafety"]}
- claimBoundary: {candidate["claimBoundary"]}
- visualDemoOnly: {str(candidate["visualDemoOnly"]).lower()}
- parityClaim: {candidate["parityClaim"]}
- validationClaim: {candidate["validationClaim"]}
- reason: {candidate["reason"]}

Visual / perceptual demo only.
No parity claim.
No validation claim.

## UI Audit Checklist

{bullets(report["uiAuditChecklist"])}

## Grok Mission

Review language.
Review layout.
Review readability.
Review misunderstanding risk.

Do not review scientific validity.
Do not create parity claims.
Do not create validation claims.

### Grok Tasks

{bullets(report["grokTasks"])}

## Next Milestone

{report["nextMilestone"]}
"""


def main() -> int:
    try:
        require_inputs(INPUTS)
        readiness = load_json_object(READINESS_JSON)
        candidate_index = load_json_object(GODOT_CANDIDATES)
        candidate = select_candidate(candidate_index, readiness)
        report = build_report(utc_now(), readiness, candidate)

        OUTPUT_JSON.parent.mkdir(parents=True, exist_ok=True)
        OUTPUT_MD.parent.mkdir(parents=True, exist_ok=True)
        OUTPUT_JSON.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        OUTPUT_MD.write_text(render_markdown(report), encoding="utf-8")
    except HandoffError as exc:
        print(f"[glowing-heart-grok-handoff] ERROR: {exc}")
        return 1

    print("[glowing-heart-grok-handoff]")
    print()
    print(f"status={report['projectStatus']['readiness']}")
    print()
    print(f"demo_candidate={report['bestDemoCandidate']['name']}")
    print()
    print(f"parity_claim={report['parityClaim']}")
    print()
    print(f"runtime_executed={str(report['runtimeExecuted']).lower()}")
    print()
    print(f"wrote={OUTPUT_JSON.as_posix()}")
    print(f"wrote={OUTPUT_MD.as_posix()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())