#!/usr/bin/env python3
"""Derive the v3.0 dashboard seed from the frozen v2.x evidence chain."""

from __future__ import annotations

import json
import os
import sys
import tempfile
from collections import Counter
from pathlib import Path
from typing import Any


PACKET_INDEX = Path("reports/glowing_heart_v2_6_difference_packet_index.preview.json")
MAP_INDEX = Path("reports/glowing_heart_v2_10_evidence_map_index.preview.json")
GRAPH = Path("Docs/Observatory/Observation_Atlas/Atlas_Graph/glowing_heart_difference_packet_exhibits.graph.json")
EVIDENCE_MAP = Path("reports/glowing_heart_v2_9_evidence_map.svg")
GALLERY = Path("Docs/xPRIMEray/project_glowing_heart_v2_4_difference_packet_gallery.md")
RELEASE_CANDIDATE = Path("Docs/xPRIMEray/project_glowing_heart_v2_13_release_candidate.md")
OUTPUT = Path("reports/glowing_heart_v3_0_dashboard_seed.preview.json")
STATUSES = ("Comparable", "Unknown", "NotComparable", "RequiresTransform")
METRICS = ("countCompared", "maxAbsDifference", "meanAbsDifference", "nonZeroCount")


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


def source_guards(entry: dict[str, Any], context: str) -> None:
    if entry.get("comparisonMode") != "core_vs_core":
        raise ValueError(f"{context}.comparisonMode: expected 'core_vs_core'")
    if entry.get("parityClaim") != "NONE":
        raise ValueError(f"{context}.parityClaim: expected 'NONE'")
    if entry.get("runtimeExecuted") is not False:
        raise ValueError(f"{context}.runtimeExecuted: expected false")


def build(packet: dict[str, Any], map_index: dict[str, Any], graph: dict[str, Any]) -> dict[str, Any]:
    entries = packet.get("entries")
    if not isinstance(entries, list) or len(entries) != 5:
        raise ValueError("Difference Packet Index must contain exactly five entries")
    map_entries = {item.get("id"): item for item in map_index.get("exhibits", []) if isinstance(item, dict)}
    graph_entries = {
        item.get("id"): item
        for item in graph.get("nodes", [])
        if isinstance(item, dict) and item.get("type") == "artifact"
    }
    if graph.get("graphId") != "glowing_heart.difference_packet_exhibits":
        raise ValueError("Atlas Graph graphId is not the frozen v2.x graph")

    fixtures: list[str] = []
    channels: list[str] = []
    exhibits: list[dict[str, Any]] = []
    counts: Counter[str] = Counter()
    for position, entry in enumerate(entries):
        if not isinstance(entry, dict):
            raise ValueError(f"entries[{position}]: expected an object")
        context = f"entries[{position}] id='{entry.get('id', '<missing>')}'"
        required = ("id", "title", "status", "rule", "leftFixture", "rightFixture", "leftChannel", "rightChannel", "metrics", "claimBoundary")
        missing = [field for field in required if field not in entry]
        if missing:
            raise ValueError(f"{context}: missing {', '.join(missing)}")
        source_guards(entry, context)
        if entry["status"] not in STATUSES:
            raise ValueError(f"{context}.status: unsupported value {entry['status']!r}")
        metrics = entry["metrics"]
        if not isinstance(metrics, dict) or any(field not in metrics for field in METRICS):
            raise ValueError(f"{context}.metrics: incomplete metric summary")
        mapped = map_entries.get(entry["id"])
        node = graph_entries.get(entry["id"])
        metadata = node.get("metadata") if isinstance(node, dict) else None
        if not isinstance(mapped, dict) or not isinstance(metadata, dict):
            raise ValueError(f"{context}: missing map-index or graph exhibit")
        if any(mapped.get(field) != entry[field] for field in ("status", "rule", "claimBoundary")):
            raise ValueError(f"{context}: Evidence Map Index metadata drift")
        if metadata.get("status") != entry["status"] or metadata.get("rule") != entry["rule"]:
            raise ValueError(f"{context}: Atlas Graph status/rule drift")
        if any(metadata.get(field) != metrics[field] for field in METRICS):
            raise ValueError(f"{context}: Atlas Graph metric drift")
        for path in (entry["leftFixture"], entry["rightFixture"]):
            if path not in fixtures:
                fixtures.append(path)
        for channel in (entry["leftChannel"], entry["rightChannel"]):
            if channel not in channels:
                channels.append(channel)
        counts[entry["status"]] += 1
        exhibits.append({
            "exhibitId": entry["id"],
            "title": entry["title"],
            "status": entry["status"],
            "rule": entry["rule"],
            "leftFixture": entry["leftFixture"],
            "rightFixture": entry["rightFixture"],
            "leftChannel": entry["leftChannel"],
            "rightChannel": entry["rightChannel"],
            **{field: metrics[field] for field in METRICS},
            "claimBoundary": entry["claimBoundary"],
        })

    expected = {"Comparable": 2, "Unknown": 2, "NotComparable": 1, "RequiresTransform": 0}
    observed = {status: counts.get(status, 0) for status in STATUSES}
    if observed != expected:
        raise ValueError(f"unexpected status counts: {observed}")
    generated = packet.get("generatedUtc")
    if not isinstance(generated, str) or not generated:
        raise ValueError("Difference Packet Index generatedUtc is missing")
    return {
        "dashboardId": "xprimeray.glowing_heart.observer_fixture_dashboard_seed.v0.preview",
        "title": "Project Glowing Heart Observer Fixture Dashboard Seed",
        "version": "v0.preview",
        "generatedUtc": generated,
        "sourceReleaseCandidate": RELEASE_CANDIDATE.as_posix(),
        "comparisonMode": "core_vs_core",
        "parityClaim": "NONE",
        "runtimeExecuted": False,
        "claimBoundary": [
            "Core-vs-Core only.",
            "Not a Godot comparison.",
            "Not image or pixel comparison.",
            "Not parity.",
            "Not physical validation.",
            "Not renderer equivalence.",
            "Dashboard seed organizes recorded evidence only; it does not validate scientific correctness."
        ],
        "groups": [{
            "groupId": "core_smoke_observer_grin_radial_smoke_family_v1",
            "title": "Core Smoke Observer / GRIN Radial Smoke Family v1",
            "observerBasis": "Core smoke observer",
            "fixtureFamily": "grin_radial_smoke_family_v1",
            "fixturePaths": fixtures,
            "channelIds": channels,
            "sourceIndexPath": PACKET_INDEX.as_posix(),
            "sourceGraphPath": GRAPH.as_posix(),
            "sourceEvidenceMapPath": EVIDENCE_MAP.as_posix(),
            "sourceGalleryPath": GALLERY.as_posix(),
            "statusCounts": observed,
            "exhibits": exhibits,
        }],
    }


def atomic_write(path: Path, value: dict[str, Any]) -> None:
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
        for path in (EVIDENCE_MAP, GALLERY, RELEASE_CANDIDATE):
            if not path.is_file():
                raise ValueError(f"required artifact not found: {path}")
        value = build(load(PACKET_INDEX), load(MAP_INDEX), load(GRAPH))
        atomic_write(output, value)
    except (OSError, TypeError, ValueError) as exc:
        print(f"FAIL: {exc}", file=sys.stderr)
        return 1
    print(f"PASS: wrote {output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
