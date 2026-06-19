#!/usr/bin/env python3
"""Generate the Observatory Gallery Basin Atlas page."""

from __future__ import annotations

import json
from dataclasses import dataclass
from pathlib import Path
from typing import Any


CATALOG_PATH = Path("reports/observatory_catalog.json")
MATURITY_PATH = Path("reports/observatory_maturity_ladder.json")
TRUST_MODEL_PATH = Path("reports/observatory_trust_model.json")
OUTPUT_PATH = Path("Docs/Observatory_Gallery/basin_atlas.md")
REPO_BLOB_BASE = "https://github.com/AetherTopologist/GD_xPRIMEray/blob/main"


@dataclass(frozen=True)
class Basin:
    name: str
    relation: str
    status_field: str
    artifact_types: tuple[str, ...]
    source_globs: tuple[str, ...]
    gallery_link: str


BASINS = (
    Basin(
        name="Closure Basin",
        relation="Domain where evaluation reaches terminal classification.",
        status_field="closure",
        artifact_types=("hermetic_storyboard_v2",),
        source_globs=(
            "reports/hermetic_storyboard_v2.png",
            "Docs/assets/observatory/hermetic_storyboard_v2.png",
            "reports/observatory_fixtures/oracle_closure/diagnostic_contact_sheet.png",
        ),
        gallery_link="./closure_diagnostics.md",
    ),
    Basin(
        name="Coverage Basin",
        relation="Domain that was actually evaluated/written.",
        status_field="coverage",
        artifact_types=("curvature_signature_ladder", "hermetic_storyboard_v2"),
        source_globs=(
            "reports/weekend_fps_curvature_sweep_assets/*frame_coverage_map.png",
            "reports/curvature_full_coverage_experiment.md",
            "Docs/Observatory_Gallery/curvature_full_coverage_experiment.md",
        ),
        gallery_link="./curvature_full_coverage_experiment.md",
    ),
    Basin(
        name="Cost Basin",
        relation="Effort field inside completed evaluation.",
        status_field="verdict",
        artifact_types=("cost_basin",),
        source_globs=(
            "reports/cost_basin_artifact_v1.md",
            "reports/cost_basin_terrain.png",
            "reports/cost_basin_storyboard.png",
            "reports/cost_basin_ladder.png",
        ),
        gallery_link="./observatory_maturity_ladder.md",
    ),
    Basin(
        name="Ownership Basin",
        relation="Which domain/receiver claimed each evaluated ray.",
        status_field="verdict",
        artifact_types=(),
        source_globs=(
            "reports/weekend_fps_curvature_sweep_assets/*transport_ownership.png",
            "reports/weekend_fps_curvature_sweep_assets/*ownership_seams.png",
            "output/transport_ownership_graph_precision_sweep/*/transport_ownership_graph_summary.md",
        ),
        gallery_link="../diagnostics/domain_ownership.md",
    ),
    Basin(
        name="Disagreement Basin",
        relation="Where two observer/model assignments differ.",
        status_field="verdict",
        artifact_types=(),
        source_globs=(
            "misterylabs_artifacts/visuals/observer-disagreement-contact-sheet.png",
            "misterylabs_artifacts/datasets/observer-disagreement.json",
            "observatory_atlas/chapters/chapter_02_observer_disagreement/chapter.md",
        ),
        gallery_link="../Observatory/chapters/chapter_02.md",
    ),
    Basin(
        name="Sensitivity Basin",
        relation="Where activation changes the measured field relative to baseline.",
        status_field="verdict",
        artifact_types=("curvature_signature_ladder",),
        source_globs=(
            "reports/curvature_signature_ladder.png",
            "reports/weekend_fps_curvature_sweep_assets/curvature_signature_ladder.png",
            "reports/weekend_fps_curvature_sweep_assets/*curvature_signature.png",
        ),
        gallery_link="./curvature_benchmark.md",
    ),
)


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


def trust_by_stage(path: Path) -> dict[str, dict[str, Any]]:
    data = load_json(path)
    return {
        str(item.get("stage")): item
        for item in data.get("master_axis", [])
        if isinstance(item, dict) and item.get("stage")
    }


def maturity_by_name(path: Path) -> dict[str, dict[str, Any]]:
    data = load_json(path)
    return {
        str(item.get("name")): item
        for item in data.get("entries", [])
        if isinstance(item, dict) and item.get("name")
    }


def matching_catalog_records(basin: Basin, catalog: list[dict[str, Any]]) -> list[dict[str, Any]]:
    return [
        record
        for record in catalog
        if str(record.get("artifact_type") or "") in basin.artifact_types
    ]


def status_for(basin: Basin, records: list[dict[str, Any]]) -> str:
    values = sorted(
        {
            str(record.get(basin.status_field) or "").upper()
            for record in records
            if record.get(basin.status_field)
        }
    )
    if not values:
        return "UNRECORDED"
    if len(values) == 1:
        return values[0]
    priority = ["FAIL", "PARTIAL", "OBSERVED", "PASS", "MISSING"]
    ordered = sorted(values, key=lambda value: priority.index(value) if value in priority else len(priority))
    return ", ".join(ordered)


def maturity_for(basin: Basin, maturity_entries: dict[str, dict[str, Any]], trust_axis: dict[str, dict[str, Any]]) -> tuple[str, str]:
    entry = maturity_entries.get(basin.name)
    if not entry:
        return "UNLABELED", "Not scored yet"

    stage = str(entry.get("stage") or "UNLABELED")
    trust_entry = trust_axis.get(stage)
    if not trust_entry:
        return stage, "Not scored yet"
    score = trust_entry.get("score")
    meaning = str(trust_entry.get("meaning") or "").rstrip(".")
    return stage, f"{stage}, score {score}: {meaning}."


def catalog_paths(records: list[dict[str, Any]]) -> list[str]:
    return [
        str(record.get("source_path"))
        for record in records
        if record.get("source_path")
    ]


def glob_paths(patterns: tuple[str, ...]) -> list[str]:
    paths: list[str] = []
    for pattern in patterns:
        matches = sorted(Path(".").glob(pattern))
        paths.extend(path.as_posix() for path in matches if path.is_file())
    return paths


def artifact_paths(basin: Basin, records: list[dict[str, Any]]) -> list[str]:
    seen: set[str] = set()
    ordered: list[str] = []
    for path in [*catalog_paths(records), *glob_paths(basin.source_globs)]:
        if path not in seen:
            seen.add(path)
            ordered.append(path)
    return ordered


def last_run(records: list[dict[str, Any]]) -> str:
    timestamps = sorted(
        str(record.get("timestamp"))
        for record in records
        if record.get("timestamp")
    )
    return timestamps[-1] if timestamps else "unrecorded"


def artifact_links(paths: list[str], limit: int = 4) -> str:
    if not paths:
        return "No linked artifacts found."

    links = []
    for path in paths[:limit]:
        label = Path(path).name
        links.append(f"[`{label}`]({REPO_BLOB_BASE}/{path})")
    if len(paths) > limit:
        links.append(f"`+{len(paths) - limit} more`")
    return ", ".join(links)


def render_card(
    basin: Basin,
    catalog: list[dict[str, Any]],
    maturity_entries: dict[str, dict[str, Any]],
    trust_axis: dict[str, dict[str, Any]],
) -> list[str]:
    records = matching_catalog_records(basin, catalog)
    maturity, trust = maturity_for(basin, maturity_entries, trust_axis)
    paths = artifact_paths(basin, records)
    return [
        f"-   **{basin.name}**",
        "",
        f"    {basin.relation}",
        "",
        f"    **Status:** `{status_for(basin, records)}`  ",
        f"    **Maturity:** `{maturity}`  ",
        f"    **Trust:** {trust}  ",
        f"    **Artifacts:** {artifact_links(paths)}  ",
        f"    **Last run:** `{last_run(records)}`  ",
        f"    **Gallery link:** [Open]({basin.gallery_link})",
        "",
    ]


def render_page() -> str:
    catalog = load_catalog(CATALOG_PATH)
    maturity_entries = maturity_by_name(MATURITY_PATH)
    trust_axis = trust_by_stage(TRUST_MODEL_PATH)

    lines = [
        "# Basin Atlas",
        "",
        "The Basin Atlas is the Observatory's periodic-table view of spatial diagnostic domains. Each card names a basin, points to the available artifacts, and separates local run status from maturity and trust language.",
        "",
        "**Reading rule:** Status is local artifact/run status only. Maturity is the Observatory Maturity Ladder stage. Trust is the evidence-strength interpretation from the Observatory Trust Model. Evidence strength is distinct from physical truth.",
        "",
        "If no Observatory Maturity Ladder entry exists for a basin, the card shows `Maturity: UNLABELED` and `Trust: Not scored yet`, not `Proposed`.",
        "",
        '<div class="grid cards" markdown>',
        "",
    ]
    for basin in BASINS:
        lines.extend(render_card(basin, catalog, maturity_entries, trust_axis))
    lines.extend(
        [
            "</div>",
            "",
            "Generated by `tools/generate_basin_atlas.py` from `reports/observatory_catalog.json`, `reports/observatory_maturity_ladder.json`, and `reports/observatory_trust_model.json`.",
            "",
        ]
    )
    return "\n".join(lines)


def main() -> int:
    OUTPUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT_PATH.write_text(render_page(), encoding="utf-8")
    print(f"wrote {OUTPUT_PATH}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
