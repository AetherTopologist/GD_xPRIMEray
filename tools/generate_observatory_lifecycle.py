#!/usr/bin/env python3
"""Generate the Observatory lifecycle information-architecture report."""

from __future__ import annotations

import json
from collections import defaultdict
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


CATALOG_PATH = Path("reports/observatory_catalog.json")
GRAPH_PATH = Path("reports/observatory_graph.json")
MATURITY_PATH = Path("reports/observatory_maturity_ladder.json")
TRUST_MODEL_PATH = Path("reports/observatory_trust_model.json")
JSON_OUTPUT = Path("reports/observatory_lifecycle.json")
MD_OUTPUT = Path("reports/observatory_lifecycle.md")

REPO_BLOB_BASE = "https://github.com/AetherTopologist/GD_xPRIMEray/blob/main"


REVIEW_LINKS = {
    "cost_basin": {
        "level": "specific",
        "path": "reports/cost_basin_survival_critique.md",
        "label": "Cost Basin Survival Critique",
        "outcome": "Survived as an Observed artifact; promotion is held pending null-model and failure-regime gates.",
    },
    "curvature_signature_ladder": {
        "level": "framework_dossier",
        "path": "reports/observatory_adversarial_framework.md",
        "label": "Sensitivity Basin dossier",
        "outcome": "Curvature Signature instance is characterized; generalized Sensitivity Basin remains gated.",
    },
    "hermetic_storyboard_v2": {
        "level": "framework_dossier",
        "path": "reports/observatory_adversarial_framework.md",
        "label": "Closure Basin dossier",
        "outcome": "Closure language survives within scene-contract and coverage caveats.",
    },
    "query_observatory": {
        "level": "framework_dossier",
        "path": "reports/observatory_adversarial_framework.md",
        "label": "Query Observatory dossier",
        "outcome": "Run-scoped aggregate attribution; not a spatial hotspot map until per-pixel query attribution exists.",
    },
}

ARTIFACT_TRUST_NOTES = {
    "query_observatory": {
        "scope": "run-scoped aggregate attribution",
        "caveat": "Presentation artifact; not an architectural concept; aggregate-only unless per-pixel query data is added.",
    },
}

GALLERY_LINKS = {
    "cost_basin": "Docs/Observatory_Gallery/basin_atlas.md",
    "curvature_signature_ladder": "Docs/Observatory_Gallery/curvature_benchmark.md",
    "hermetic_storyboard_v2": "Docs/Observatory_Gallery/canonical_fixtures.md",
    "observatory_story_reference": "Docs/Observatory_Gallery/what_the_observatory_measures.md",
    "observer_storyboard": "Docs/Observatory_Gallery/what_the_observatory_measures.md",
    "query_observatory": "Docs/Observatory_Gallery/basin_atlas.md",
    "renderer_storyboard_v1": "Docs/Observatory_Gallery/what_the_observatory_measures.md",
}

REVIEW_LINKS.update(
    {
        "observatory_story_reference": {
            "level": "specific_stub",
            "path": "reports/observatory_story_reference_adversarial_review.md",
            "label": "Observatory Story Reference adversarial review",
            "outcome": "Minimal review stub exists; experiments are pending.",
        },
        "observer_storyboard": {
            "level": "specific_stub",
            "path": "reports/observer_storyboard_adversarial_review.md",
            "label": "Observer Storyboard adversarial review",
            "outcome": "Minimal review stub exists; experiments are pending.",
        },
        "renderer_storyboard_v1": {
            "level": "specific_stub",
            "path": "reports/renderer_storyboard_v1_adversarial_review.md",
            "label": "Renderer Storyboard v1 adversarial review",
            "outcome": "Minimal review stub exists; experiments are pending.",
        },
    }
)

SYSTEM_LINKS = {
    "gallery": "Docs/Observatory_Gallery/index.md",
    "trust_model": "Docs/Observatory_Gallery/observatory_trust_model.md",
    "maturity_ladder": "Docs/Observatory_Gallery/observatory_maturity_ladder.md",
    "basin_atlas": "Docs/Observatory_Gallery/basin_atlas.md",
    "adversarial_reviews": "reports/observatory_adversarial_framework.md",
}


def utc_now() -> str:
    return datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")


def load_json(path: Path) -> Any:
    return json.loads(path.read_text(encoding="utf-8"))


def load_catalog(path: Path) -> list[dict[str, Any]]:
    data = load_json(path)
    if isinstance(data, list):
        return [item for item in data if isinstance(item, dict)]
    if isinstance(data, dict):
        for key in ("artifacts", "records", "items"):
            value = data.get(key)
            if isinstance(value, list):
                return [item for item in value if isinstance(item, dict)]
    return []


def stage_axis(path: Path) -> dict[str, dict[str, Any]]:
    data = load_json(path)
    return {
        str(item["stage"]): item
        for item in data.get("master_axis", [])
        if isinstance(item, dict) and item.get("stage")
    }


def artifact_nodes(path: Path) -> dict[str, dict[str, Any]]:
    data = load_json(path)
    nodes: dict[str, dict[str, Any]] = {}
    for node in data.get("nodes", []):
        if node.get("type") != "Artifact":
            continue
        node_id = str(node.get("id", ""))
        key = node_id.split("artifact:", 1)[-1]
        nodes[key] = node
    return nodes


def maturity_entries(path: Path) -> list[dict[str, Any]]:
    data = load_json(path)
    return [item for item in data.get("entries", []) if isinstance(item, dict)]


def best_maturity_for(key: str, records: list[dict[str, Any]], entries: list[dict[str, Any]]) -> dict[str, Any] | None:
    source_paths = {str(record.get("source_path")) for record in records if record.get("source_path")}
    matches: list[dict[str, Any]] = []
    for entry in entries:
        entry_id = str(entry.get("id", ""))
        artifact_type = str(entry.get("artifact_type", ""))
        source_path = str(entry.get("source_path", ""))
        if entry_id == f"artifact:{key}" or artifact_type == key or source_path in source_paths:
            matches.append(entry)
    if not matches:
        return None
    return sorted(matches, key=lambda item: (int(item.get("score", -1)), str(item.get("name", ""))), reverse=True)[0]


def status_values(records: list[dict[str, Any]], field: str) -> list[str]:
    values = {
        str(record.get(field)).upper()
        for record in records
        if record.get(field)
    }
    return sorted(values)


def source_paths_for(records: list[dict[str, Any]], node: dict[str, Any] | None) -> list[str]:
    paths: list[str] = []
    if node:
        paths.extend(str(path) for path in node.get("metadata", {}).get("source_paths", []) if path)
    paths.extend(str(record.get("source_path")) for record in records if record.get("source_path"))

    seen: set[str] = set()
    ordered: list[str] = []
    for path in paths:
        if path in seen:
            continue
        seen.add(path)
        ordered.append(path)
    return ordered


def link_for(path: str) -> str:
    if path.startswith("Docs/"):
        return f"../{path}"
    return f"{REPO_BLOB_BASE}/{path}"


def review_for(key: str) -> dict[str, Any]:
    review = REVIEW_LINKS.get(key)
    if review:
        return {
            **review,
            "reviewed": review["level"] in {"specific", "framework_dossier", "specific_stub"},
        }
    return {
        "reviewed": False,
        "level": "missing",
        "path": "",
        "label": "No artifact-specific adversarial review linked",
        "outcome": "Missing lifecycle stage.",
    }


def trust_for(maturity: dict[str, Any] | None, axis: dict[str, dict[str, Any]]) -> dict[str, Any]:
    if not maturity:
        return {
            "trusted": False,
            "stage": "UNLABELED",
            "score": None,
            "interpretation": "Not scored yet.",
        }
    stage = str(maturity.get("stage", "UNLABELED"))
    entry = axis.get(stage)
    if not entry:
        return {
            "trusted": False,
            "stage": stage,
            "score": maturity.get("score"),
            "interpretation": "No Trust Model interpretation found for this stage.",
        }
    return {
        "trusted": True,
        "stage": stage,
        "score": entry.get("score"),
        "interpretation": str(entry.get("meaning", "")),
    }


def missing_stages(record: dict[str, Any]) -> list[str]:
    missing: list[str] = []
    if not record["observed"]:
        missing.append("Observation")
    if not record["source_paths"]:
        missing.append("Artifact")
    if record["maturity"]["stage"] == "UNLABELED":
        missing.append("Maturity")
    if not record["trust"]["trusted"]:
        missing.append("Trust")
    if not record["adversarial_review"]["reviewed"]:
        missing.append("Adversarial Review")
    if record["status"] == "UNRECORDED":
        missing.append("Status")
    return missing


def build_payload() -> dict[str, Any]:
    catalog = load_catalog(CATALOG_PATH)
    graph_nodes = artifact_nodes(GRAPH_PATH)
    entries = maturity_entries(MATURITY_PATH)
    axis = stage_axis(TRUST_MODEL_PATH)

    records_by_type: dict[str, list[dict[str, Any]]] = defaultdict(list)
    for record in catalog:
        artifact_type = str(record.get("artifact_type", "unknown"))
        records_by_type[artifact_type].append(record)

    artifact_keys = sorted(set(graph_nodes) | set(records_by_type))
    artifacts: list[dict[str, Any]] = []
    for key in artifact_keys:
        records = records_by_type.get(key, [])
        node = graph_nodes.get(key)
        maturity = best_maturity_for(key, records, entries)
        trust = trust_for(maturity, axis)
        paths = source_paths_for(records, node)
        verdicts = status_values(records, "verdict")
        coverage = status_values(records, "coverage")
        closure = status_values(records, "closure")
        status = ", ".join(verdicts) if verdicts else "UNRECORDED"
        artifact = {
            "id": f"artifact:{key}",
            "artifact_type": key,
            "artifact": node.get("label") if node else key,
            "observation": "Observed output with source paths." if paths else "No source path discovered.",
            "observed": bool(paths),
            "source_paths": paths,
            "status": status,
            "coverage": coverage,
            "closure": closure,
            "maturity": {
                "stage": trust["stage"],
                "score": trust["score"],
                "basis": maturity.get("basis", "") if maturity else "",
                "source_path": maturity.get("source_path", "") if maturity else "",
            },
            "trust": trust,
            "trust_note": ARTIFACT_TRUST_NOTES.get(key, {}),
            "adversarial_review": review_for(key),
            "canonicality": "Canonical" if trust["stage"] == "Canonical" else "Not canonical",
            "gallery_link": GALLERY_LINKS.get(key, ""),
        }
        artifact["missing_lifecycle_stages"] = missing_stages(artifact)
        artifacts.append(artifact)

    return {
        "schema": "xprimeray.observatory_lifecycle.v1",
        "generated_at": utc_now(),
        "generated_from": [
            CATALOG_PATH.as_posix(),
            GRAPH_PATH.as_posix(),
            MATURITY_PATH.as_posix(),
            TRUST_MODEL_PATH.as_posix(),
            "reports/observatory_adversarial_framework.md",
            "reports/cost_basin_survival_critique.md",
        ],
        "lifecycle_fields": [
            "Observation",
            "Artifact",
            "Maturity",
            "Trust",
            "Adversarial Review",
            "Status",
            "Canonicality",
        ],
        "artifacts": artifacts,
        "missing_lifecycle_stages": {
            artifact["artifact"]: artifact["missing_lifecycle_stages"]
            for artifact in artifacts
            if artifact["missing_lifecycle_stages"]
        },
        "automatic_link_recommendations": [
            {
                "from": "Gallery artifact cards",
                "to": "Lifecycle report row, Trust Model, Maturity Ladder, Adversarial Review, Basin Atlas when applicable",
                "reason": "Visitors should move from visual exhibit to evidence strength and critique without guessing the vocabulary.",
            },
            {
                "from": "Maturity Ladder entries",
                "to": "Adversarial Review and lifecycle status",
                "reason": "Every score should show the attack path or explicitly mark review missing.",
            },
            {
                "from": "Trust Model stages",
                "to": "Lifecycle report examples at each stage",
                "reason": "Trust language becomes concrete when each stage has current artifacts.",
            },
            {
                "from": "Adversarial Reviews",
                "to": "Catalog artifact, Gallery page, Maturity Ladder entry, and Basin Atlas card",
                "reason": "Survival critiques should narrow claims at the exact artifact they attack.",
            },
            {
                "from": "Basin Atlas cards",
                "to": "Lifecycle report rows and review gates",
                "reason": "Basin terms are easy to over-read; their cards should expose maturity, trust, and attack status.",
            },
        ],
        "recommended_system_links": SYSTEM_LINKS,
    }


def md_link(label: str, path: str) -> str:
    if not path:
        return "Missing"
    return f"[{label}]({link_for(path)})"


def yes_no(value: bool) -> str:
    return "Yes" if value else "No"


def render_markdown(payload: dict[str, Any]) -> str:
    lines = [
        "# Observatory Lifecycle",
        "",
        "Generated information architecture for connecting Observatory artifacts from observation through canonicality.",
        "",
        "**Guardrail:** This report changes no renderer logic. It joins the existing Artifact Catalog, Knowledge Graph, Maturity Ladder, Trust Model, Basin Atlas, Storyboards, and Adversarial Reviews.",
        "",
        "## Lifecycle Pipeline",
        "",
        "Observation -> Artifact -> Maturity -> Trust -> Adversarial Review -> Status -> Canonicality",
        "",
        "A visitor should be able to start at any artifact and answer: what was observed, how strong the evidence is, how the claim was attacked, whether it survived, and why it is trusted.",
        "",
        "## Status Table",
        "",
        "| Artifact | Observed? | Reviewed? | Trusted? | Canonical? | Status | Maturity | Review | Gallery |",
        "|---|---:|---:|---:|---:|---|---|---|---|",
    ]
    for artifact in payload["artifacts"]:
        review = artifact["adversarial_review"]
        review_cell = md_link(review["label"], review["path"]) if review["path"] else review["label"]
        gallery_cell = md_link("Gallery", artifact["gallery_link"]) if artifact["gallery_link"] else "Missing"
        maturity = artifact["maturity"]
        lines.append(
            f"| **{artifact['artifact']}** | {yes_no(artifact['observed'])} | "
            f"{review['level']} | {yes_no(artifact['trust']['trusted'])} | "
            f"{yes_no(artifact['canonicality'] == 'Canonical')} | `{artifact['status']}` | "
            f"{maturity['stage']} ({maturity['score'] if maturity['score'] is not None else 'unscored'}) | "
            f"{review_cell} | {gallery_cell} |"
        )

    lines += [
        "",
        "## Lifecycle Details",
        "",
    ]
    for artifact in payload["artifacts"]:
        review = artifact["adversarial_review"]
        source_links = ", ".join(md_link(Path(path).name, path) for path in artifact["source_paths"][:4])
        if len(artifact["source_paths"]) > 4:
            source_links += f", `+{len(artifact['source_paths']) - 4} more`"
        if not source_links:
            source_links = "Missing"
        missing = ", ".join(artifact["missing_lifecycle_stages"]) or "None"
        lines += [
            f"### {artifact['artifact']}",
            "",
            f"- **Observation:** {artifact['observation']}",
            f"- **Artifact:** {source_links}",
            f"- **Maturity:** {artifact['maturity']['stage']} ({artifact['maturity']['score'] if artifact['maturity']['score'] is not None else 'unscored'})",
            f"- **Trust:** {artifact.get('trust_note', {}).get('scope') or artifact['trust']['interpretation']}",
        ]
        if artifact.get("trust_note", {}).get("caveat"):
            lines.append(f"- **Caveat:** {artifact['trust_note']['caveat']}")
        lines += [
            f"- **Adversarial Review:** {review['label']} — {review['outcome']}",
            f"- **Status:** verdict `{artifact['status']}`, coverage `{', '.join(artifact['coverage']) or 'UNRECORDED'}`, closure `{', '.join(artifact['closure']) or 'UNRECORDED'}`",
            f"- **Canonicality:** {artifact['canonicality']}",
            f"- **Missing lifecycle stages:** {missing}",
            "",
        ]

    lines += [
        "## Missing Lifecycle Stages",
        "",
    ]
    missing_items = payload["missing_lifecycle_stages"]
    if missing_items:
        lines += [
            "| Artifact | Missing stages |",
            "|---|---|",
        ]
        for artifact, stages in missing_items.items():
            lines.append(f"| {artifact} | {', '.join(stages)} |")
        lines.append("")
    else:
        lines.append("No missing lifecycle stages detected.")
        lines.append("")

    lines += [
        "## Automatic Link Recommendations",
        "",
        "| From | To | Reason |",
        "|---|---|---|",
    ]
    for recommendation in payload["automatic_link_recommendations"]:
        lines.append(
            f"| {recommendation['from']} | {recommendation['to']} | {recommendation['reason']} |"
        )

    lines += [
        "",
        "## Recommended System Links",
        "",
    ]
    for name, path in payload["recommended_system_links"].items():
        lines.append(f"- **{name}:** {md_link(path, path)}")

    lines += [
        "",
        f"Generated at `{payload['generated_at']}` from existing Observatory outputs.",
        "",
    ]
    return "\n".join(lines)


def main() -> int:
    payload = build_payload()
    JSON_OUTPUT.write_text(json.dumps(payload, indent=2, sort_keys=True) + "\n", encoding="utf-8")
    MD_OUTPUT.write_text(render_markdown(payload), encoding="utf-8")
    print(f"wrote {JSON_OUTPUT} and {MD_OUTPUT} ({len(payload['artifacts'])} artifacts)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
