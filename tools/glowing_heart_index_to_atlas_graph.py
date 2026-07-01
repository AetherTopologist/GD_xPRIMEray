#!/usr/bin/env python3
"""Bridge the Glowing Heart Difference Packet Index into Atlas Graph JSON."""

from __future__ import annotations

import json
import os
import sys
import tempfile
from pathlib import Path
from typing import Any


REQUIRED_TOP = ("indexId", "title", "version", "entries", "claimBoundary")
REQUIRED_ENTRY = (
    "id",
    "title",
    "sourceCase",
    "status",
    "rule",
    "reason",
    "leftFixture",
    "rightFixture",
    "leftChannel",
    "rightChannel",
    "comparisonMode",
    "parityClaim",
    "runtimeExecuted",
    "metrics",
    "claimBoundary",
    "galleryDocPath",
    "sourceReportPath",
)
REQUIRED_METRICS = ("countCompared", "maxAbsDifference", "meanAbsDifference", "nonZeroCount")
ALLOWED_STATUSES = {"Comparable", "NotComparable", "RequiresTransform", "Unknown"}


def require_fields(value: dict[str, Any], fields: tuple[str, ...], context: str) -> None:
    missing = [field for field in fields if field not in value]
    if missing:
        raise ValueError(f"{context}: missing required field(s): {', '.join(missing)}")


def load_index(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except FileNotFoundError as exc:
        raise ValueError(f"index not found: {path}") from exc
    except json.JSONDecodeError as exc:
        raise ValueError(f"{path}:{exc.lineno}:{exc.colno}: invalid JSON: {exc.msg}") from exc

    if not isinstance(value, dict):
        raise ValueError("index: expected an object")
    require_fields(value, REQUIRED_TOP, "index")
    entries = value["entries"]
    if not isinstance(entries, list) or not entries:
        raise ValueError("index.entries: expected a non-empty array")
    if not isinstance(value["claimBoundary"], list) or not value["claimBoundary"]:
        raise ValueError("index.claimBoundary: expected a non-empty array")

    seen: set[str] = set()
    for position, entry in enumerate(entries):
        context = f"index.entries[{position}]"
        if not isinstance(entry, dict):
            raise ValueError(f"{context}: expected an object")
        require_fields(entry, REQUIRED_ENTRY, context)
        entry_context = f"{context} id='{entry.get('id', '<missing>')}'"
        if entry["id"] in seen:
            raise ValueError(f"{entry_context}.id: duplicate exhibit id '{entry['id']}'")
        seen.add(entry["id"])
        if entry["status"] not in ALLOWED_STATUSES:
            raise ValueError(f"{entry_context}.status: invalid status {entry['status']!r}")
        if entry["parityClaim"] != "NONE":
            raise ValueError(f"{entry_context}.parityClaim: expected 'NONE', got {entry['parityClaim']!r}")
        if entry["runtimeExecuted"] is not False:
            raise ValueError(f"{entry_context}.runtimeExecuted: expected false, got {entry['runtimeExecuted']!r}")
        if entry["comparisonMode"] != "core_vs_core":
            raise ValueError(
                f"{entry_context}.comparisonMode: expected 'core_vs_core', got {entry['comparisonMode']!r}"
            )
        if not isinstance(entry["claimBoundary"], list) or not entry["claimBoundary"]:
            raise ValueError(f"{entry_context}.claimBoundary: expected a non-empty array")
        if not isinstance(entry["metrics"], dict):
            raise ValueError(f"{entry_context}.metrics: expected an object")
        require_fields(entry["metrics"], REQUIRED_METRICS, f"{entry_context}.metrics")

    return value


def claim_text(boundaries: list[str]) -> str:
    return " ".join(boundaries)


def exhibit_node(entry: dict[str, Any]) -> dict[str, Any]:
    metrics = entry["metrics"]
    return {
        "id": entry["id"],
        "label": entry["title"],
        "type": "artifact",
        "category": "Difference Packet Exhibit",
        "maturity": "Experimental",
        "description": entry["reason"],
        "claimBoundary": claim_text(entry["claimBoundary"]),
        "links": [
            {"label": "Gallery", "href": entry["galleryDocPath"]},
            {"label": "Source report", "href": entry["sourceReportPath"]},
        ],
        "tags": [
            f"status:{entry['status']}",
            f"rule:{entry['rule']}",
            f"leftChannel:{entry['leftChannel']}",
            f"rightChannel:{entry['rightChannel']}",
        ],
        "metadata": {
            "status": entry["status"],
            "rule": entry["rule"],
            "sourceCase": entry["sourceCase"],
            "leftFixture": entry["leftFixture"],
            "rightFixture": entry["rightFixture"],
            "leftChannel": entry["leftChannel"],
            "rightChannel": entry["rightChannel"],
            "countCompared": metrics["countCompared"],
            "maxAbsDifference": metrics["maxAbsDifference"],
            "meanAbsDifference": metrics["meanAbsDifference"],
            "nonZeroCount": metrics["nonZeroCount"],
            "parityClaim": entry["parityClaim"],
            "runtimeExecuted": entry["runtimeExecuted"],
            "comparisonMode": entry["comparisonMode"],
            "claimBoundary": entry["claimBoundary"],
        },
    }


def build_graph(index: dict[str, Any]) -> dict[str, Any]:
    root_id = "glowing_heart_difference_packet_gallery"
    root = {
        "id": root_id,
        "label": "Project Glowing Heart Difference Packet Gallery",
        "type": "project",
        "category": "Glowing Heart",
        "maturity": "Experimental",
        "description": "Structured gallery of retained Core Difference Packet decision exhibits.",
        "claimBoundary": "Core-vs-Core artifact gallery only; not parity, validation, physical correctness, or renderer equivalence.",
        "links": [
            {
                "label": "Difference Packet Gallery",
                "href": "Docs/xPRIMEray/project_glowing_heart_v2_4_difference_packet_gallery.md",
            }
        ],
        "tags": ["glowing_heart", "difference_packet", "gallery"],
        "metadata": {
            "sourceIndex": index["indexId"],
            "exhibitCount": len(index["entries"]),
            "claimBoundary": index["claimBoundary"],
        },
    }
    nodes = [root, *(exhibit_node(entry) for entry in index["entries"])]
    edges = [
        {
            "id": f"gallery_produces_{entry['id']}",
            "from": root_id,
            "to": entry["id"],
            "relationship": "produces",
            "label": "produces exhibit",
            "claimBoundary": "Artifact relationship only; it does not imply ranking, correctness, or comparison eligibility beyond the recorded matrix decision.",
        }
        for entry in index["entries"]
    ]
    return {
        "graphId": "glowing_heart.difference_packet_exhibits.v2_8",
        "title": "Glowing Heart Difference Packet Exhibits",
        "description": "Atlas Graph representation of the five structured Difference Packet Gallery exhibits.",
        "version": "v0.1",
        "notes": [
            "Node maturity describes documentation evidence depth, not exhibit quality.",
            "Difference Packet runtimeExecuted=false describes the comparison stage, not Core artifact generation.",
        ],
        "plainTerms": "Read this graph as an evidence map of comparison decisions, not as a ranking or correctness ladder.",
        "nodes": nodes,
        "edges": edges,
        "groups": [
            {
                "id": "difference_packet_exhibits",
                "label": "Difference Packet Exhibits",
                "description": "Five retained Core comparison-decision exhibits generated from the v2.6 index.",
                "nodeIds": [entry["id"] for entry in index["entries"]],
            }
        ],
        "claimBoundary": claim_text(index["claimBoundary"]),
        "evidence": [
            {
                "label": "Difference Packet Index",
                "reference": "reports/glowing_heart_v2_6_difference_packet_index.preview.json",
                "kind": "report",
            },
            {
                "label": "Difference Packet Index Schema",
                "reference": "schemas/glowing_heart/difference_packet_index.v0.preview.json",
                "kind": "schema",
            },
            {
                "label": "Channel Registry",
                "reference": "Docs/xPRIMEray/glowing_heart/channel_registry.preview.json",
                "kind": "artifact",
            },
            {
                "label": "Compatibility Matrix",
                "reference": "Docs/xPRIMEray/glowing_heart/channel_compatibility_matrix.preview.json",
                "kind": "artifact",
            },
        ],
        "renderHints": {
            "direction": "LR",
            "layout": "layered",
            "showGroups": True,
            "nodeShapes": {"project": "rounded", "artifact": "rectangle"},
            "edgeStyles": {"produces": "solid"},
        },
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
    if len(argv) != 3:
        print(f"Usage: {argv[0]} <difference-packet-index.json> <atlas-graph.json>", file=sys.stderr)
        return 2
    source, output = map(Path, argv[1:])
    if source.resolve() == output.resolve():
        print("FAIL: input and output paths must be distinct", file=sys.stderr)
        return 2
    try:
        write_atomic(output, build_graph(load_index(source)))
    except (OSError, TypeError, ValueError) as exc:
        print(f"FAIL: {exc}", file=sys.stderr)
        return 1
    print(f"PASS: generated {output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
