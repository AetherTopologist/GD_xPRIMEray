#!/usr/bin/env python3
"""Generate the Glowing Heart fixture gallery discovery index."""

from __future__ import annotations

import json
import os
import sys
import tempfile
from pathlib import Path
from typing import Any


SEED = Path("reports/glowing_heart_v3_0_dashboard_seed.preview.json")
OUTPUT = Path("reports/glowing_heart_v3_2_fixture_gallery_index.preview.json")
EVIDENCE_INDEX = Path("reports/glowing_heart_v2_10_evidence_map_index.preview.json")
DASHBOARD_MARKDOWN = Path("reports/glowing_heart_v3_1_dashboard.preview.md")
HEALTH_REPORT = Path("reports/glowing_heart_v2_11_evidence_chain_health.preview.md")
EVIDENCE_MAP = Path("reports/glowing_heart_v2_9_evidence_map.svg")
DASHBOARD_SVG = Path("reports/glowing_heart_v3_1_dashboard.svg")
GALLERY = Path("Docs/xPRIMEray/project_glowing_heart_v2_4_difference_packet_gallery.md")


def load(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except FileNotFoundError as exc:
        raise ValueError(f"required source not found: {path}") from exc
    except json.JSONDecodeError as exc:
        raise ValueError(f"{path}:{exc.lineno}:{exc.colno}: invalid JSON: {exc.msg}") from exc
    if not isinstance(value, dict):
        raise ValueError(f"{path}: expected an object")
    return value


def build(seed: dict[str, Any]) -> dict[str, Any]:
    if seed.get("comparisonMode") != "core_vs_core" or seed.get("parityClaim") != "NONE" or seed.get("runtimeExecuted") is not False:
        raise ValueError("dashboard seed claim guards are invalid")
    groups = seed.get("groups")
    if not isinstance(groups, list) or len(groups) != 1 or not isinstance(groups[0], dict):
        raise ValueError("dashboard seed must contain exactly one group")
    group = groups[0]
    required = ("fixtureFamily", "fixturePaths", "observerBasis", "channelIds", "statusCounts")
    missing = [field for field in required if field not in group]
    if missing:
        raise ValueError(f"dashboard group missing fields: {', '.join(missing)}")
    fixture_paths = group["fixturePaths"]
    if not isinstance(fixture_paths, list) or not fixture_paths or "Fixtures/grin_radial_smoke.json" not in fixture_paths:
        raise ValueError("dashboard group lacks the canonical grin radial smoke fixture")
    if group["fixtureFamily"] != "grin_radial_smoke_family_v1":
        raise ValueError("unexpected fixture family")
    expected_counts = {"Comparable": 2, "Unknown": 2, "NotComparable": 1, "RequiresTransform": 0}
    if group["statusCounts"] != expected_counts:
        raise ValueError("dashboard status counts do not match the retained evidence set")
    generated = seed.get("generatedUtc")
    if not isinstance(generated, str) or not generated:
        raise ValueError("dashboard seed generatedUtc is missing")
    return {
        "indexId": "xprimeray.glowing_heart.fixture_gallery_index.v0.preview",
        "title": "Project Glowing Heart Fixture Gallery Index",
        "version": "v0.preview",
        "generatedUtc": generated,
        "comparisonMode": "core_vs_core",
        "parityClaim": "NONE",
        "runtimeExecuted": False,
        "entries": [{
            "fixtureFamily": group["fixtureFamily"],
            "fixturePath": "Fixtures/grin_radial_smoke.json",
            "observerBasis": group["observerBasis"],
            "channels": group["channelIds"],
            "artifactLinks": {
                "evidenceIndex": EVIDENCE_INDEX.as_posix(),
                "dashboardSeed": SEED.as_posix(),
                "dashboardMarkdown": DASHBOARD_MARKDOWN.as_posix(),
                "healthReport": HEALTH_REPORT.as_posix()
            },
            "evidenceMap": EVIDENCE_MAP.as_posix(),
            "dashboardSvg": DASHBOARD_SVG.as_posix(),
            "galleryMarkdown": GALLERY.as_posix(),
            "statusCounts": group["statusCounts"],
            "claimBoundary": seed["claimBoundary"],
            "maturity": "Experimental",
            "curiosityTier": "visitor"
        }],
        "claimBoundary": [
            "Core-vs-Core recorded artifacts only.",
            "Not a Godot comparison.",
            "Not image or pixel comparison.",
            "Not parity.",
            "Not physical validation.",
            "Not renderer equivalence.",
            "Fixture gallery indexing organizes existing artifacts; it does not establish scientific correctness."
        ]
    }


def write_atomic(path: Path, value: dict[str, Any]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    descriptor, temporary = tempfile.mkstemp(prefix=f".{path.name}.", dir=path.parent, text=True)
    try:
        with os.fdopen(descriptor, "w", encoding="utf-8", newline="\n") as handle:
            json.dump(value, handle, indent=2, ensure_ascii=True)
            handle.write("\n")
        os.replace(temporary, path)
    except Exception:
        try:
            os.unlink(temporary)
        except FileNotFoundError:
            pass
        raise


def main(argv: list[str]) -> int:
    if len(argv) > 2:
        print(f"Usage: {argv[0]} [output.json]", file=sys.stderr)
        return 2
    output = Path(argv[1]) if len(argv) == 2 else OUTPUT
    try:
        for path in (EVIDENCE_INDEX, DASHBOARD_MARKDOWN, HEALTH_REPORT, EVIDENCE_MAP, DASHBOARD_SVG, GALLERY):
            if not path.is_file():
                raise ValueError(f"required artifact not found: {path}")
        write_atomic(output, build(load(SEED)))
    except (OSError, TypeError, ValueError) as exc:
        print(f"FAIL: {exc}", file=sys.stderr)
        return 1
    print(f"PASS: wrote {output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
