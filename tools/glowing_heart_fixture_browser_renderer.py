#!/usr/bin/env python3
"""Render the public fixture browser from retained v3.3 and v3.4 reports."""

from __future__ import annotations

import json
from pathlib import Path
from typing import Any


LIBRARY_INDEX = Path("reports/glowing_heart_v3_3_fixture_library_index.preview.json")
LADDER_INDEX = Path("reports/glowing_heart_v3_4_resolution_ladder.preview.json")
DOC_OUTPUT = Path("Docs/xPRIMEray/glowing_heart_fixture_browser.md")
REPORT_OUTPUT = Path("reports/glowing_heart_v3_5_fixture_browser.preview.md")
TIER_ORDER = ("smoke", "mini", "standard", "high")
TIER_LABELS = {
    "smoke": "Baseline",
    "mini": "Compact",
    "standard": "Gallery detail",
    "high": "Extended",
}
ROLE_LABELS = {
    "canonical_primary": "Canonical",
    "sensitivity_variant": "Amplitude sensitivity",
    "observer_variant": "Observer sensitivity",
    "resolution_variant": "Grid sensitivity",
}
SOURCE_BASE = "https://github.com/xPRIMEray/GD_xPRIMEray/blob/main"


def load_json(path: Path) -> dict[str, Any]:
    value = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(value, dict):
        raise ValueError(f"{path}: root must be an object")
    if value.get("parityClaim") != "NONE":
        raise ValueError(f"{path}: parityClaim must be NONE")
    return value


def artifact_link(path: str, docs: bool) -> str:
    asset = Path(path)
    if not asset.is_file():
        raise ValueError(f"publishable artifact is missing: {path}")
    if docs:
        return "../" + asset.relative_to("Docs").as_posix()
    return "../" + asset.as_posix()


def render(library: dict[str, Any], ladder: dict[str, Any], docs: bool) -> str:
    families = library.get("families", [])
    if len(families) != 1:
        raise ValueError("v3.5 expects exactly one published fixture family")
    family = families[0]
    records = ladder.get("records", [])
    by_fixture: dict[str, list[dict[str, Any]]] = {}
    for record in records:
        by_fixture.setdefault(record["fixtureName"], []).append(record)

    lines = [
        "# Glowing Heart Fixture Browser",
        "",
        "## Core-runnable GRIN smoke fixture artifacts",
        "",
        "**Health: 4 fixtures · 12 attempted resolution runs PASS**  ",
        "**Boundary: Core artifacts only · no Godot or image comparison**",
        "",
        "Browse the retained fixture family by experimental role and sampling density. The highest published passing tier is surfaced first; manifests and metric tables remain available for deeper inspection.",
        "",
        "## Reading Boundary",
        "",
        "These pages expose measurements produced by the Core smoke transport fixtures. They do not establish Godot parity, physical validation, renderer equivalence, or proof. Higher resolution means a denser sampling grid, not a different transport model.",
        "",
        f"## {family['familyId']}",
        "",
        family["description"],
        "",
        '<div class="grid cards" markdown>',
        "",
    ]
    for fixture in family["fixtures"]:
        fixture_records = by_fixture.get(fixture["name"], [])
        passed = [r for r in fixture_records if r["status"] == "PASS"]
        if not passed:
            raise ValueError(f"{fixture['name']}: no passing published tier")
        best = max(passed, key=lambda r: TIER_ORDER.index(r["tier"]))
        base = best.get("publishedAssetPath")
        if not base:
            raise ValueError(f"{fixture['name']}: best tier has no published asset path")
        links = {
            name: artifact_link(f"{base}/{filename}", docs)
            for name, filename in (
                ("Snapshot", "snapshot_ascii.txt"),
                ("Summary", "run_summary.md"),
                ("Manifest", "manifest.json"),
                ("Metrics CSV", "ray_metrics.csv"),
            )
        }
        channels = " · ".join(f"`{channel}`" for channel in fixture["channels"])
        role = ROLE_LABELS[fixture["role"]]
        lines.extend([
            f'-   **`{fixture["name"]}`**',
            "",
            f"    **{role}** · `{best['status']}`",
            "",
            f"    Best passing tier: **{TIER_LABELS[best['tier']]} {best['gridWidth']}x{best['gridHeight']}** · {best['rayCount']} rays",
            "",
            f"    Channels: {channels}",
            "",
            f"    [{TIER_LABELS[best['tier']]} preview]({links['Snapshot']}) · [Run summary]({links['Summary']})",
            "",
            f"    Raw data: [Manifest]({links['Manifest']}) · [Metrics CSV]({links['Metrics CSV']})",
            "",
            "    Evidence: [Fixture gallery](project_glowing_heart_v3_3_fixture_library_gallery.md) · [Resolution ladder](project_glowing_heart_v3_4_resolution_ladder.md)" if docs else "    Evidence: `Docs/xPRIMEray/project_glowing_heart_v3_3_fixture_library_gallery.md` · `Docs/xPRIMEray/project_glowing_heart_v3_4_resolution_ladder.md`",
            "",
        ])
    lines.extend(["</div>", "", "## Resolution Ladder", "", "| Fixture | Baseline | Compact | Gallery detail | Extended |", "|---|---|---|---|---|"])
    for fixture in family["fixtures"]:
        fixture_records = {r["tier"]: r for r in by_fixture[fixture["name"]]}
        cells = []
        for tier in TIER_ORDER:
            record = fixture_records[tier]
            if record["status"] == "DEFERRED":
                cells.append("Not run in v3.4 (scope stop)")
            else:
                snapshot = artifact_link(f"{record['publishedAssetPath']}/snapshot_ascii.txt", docs)
                cells.append(f"[{record['gridWidth']}x{record['gridHeight']} · PASS]({snapshot})")
        lines.append(f"| `{fixture['name']}` | {' | '.join(cells)} |")

    lines.extend([
        "",
        "Extended is a policy deferral, not a failed run. It was not run in v3.4 because Gallery detail was the declared stopping point.",
        "",
        "## Downloads and Developer Sources",
        "",
        f"- [Fixture library index JSON]({SOURCE_BASE}/{LIBRARY_INDEX.as_posix()})",
        f"- [Resolution ladder JSON]({SOURCE_BASE}/{LADDER_INDEX.as_posix()})",
        f"- [Fixture library schema]({SOURCE_BASE}/schemas/glowing_heart/fixture_library_index.v0.preview.json)",
        f"- [Resolution ladder schema]({SOURCE_BASE}/schemas/glowing_heart/fixture_resolution_ladder.v0.preview.json)",
        "- Each fixture card links its publishable manifest and metrics CSV.",
        "",
        "## Claim Boundary",
        "",
        "- Core fixture artifact browsing only.",
        "- No Godot comparison or Godot runtime execution.",
        "- No image or pixel comparison.",
        "- No parity claim.",
        "- No physical validation, renderer equivalence, or proof.",
        "- PASS describes the recorded fixture run checks only.",
        "",
    ])
    return "\n".join(lines)


def main() -> int:
    library = load_json(LIBRARY_INDEX)
    ladder = load_json(LADDER_INDEX)
    REPORT_OUTPUT.write_text(render(library, ladder, docs=False), encoding="utf-8")
    DOC_OUTPUT.write_text(render(library, ladder, docs=True), encoding="utf-8")
    print(f"PASS: rendered {len(library['families'][0]['fixtures'])} fixture cards")
    print(f"Docs: {DOC_OUTPUT}")
    print(f"Preview: {REPORT_OUTPUT}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
