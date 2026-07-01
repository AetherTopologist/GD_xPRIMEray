#!/usr/bin/env python3
"""Build the Glowing Heart evidence-chain discovery index."""

from __future__ import annotations

import json
import os
import re
import sys
import tempfile
import xml.etree.ElementTree as ET
from collections import Counter
from pathlib import Path
from typing import Any


PACKET_INDEX_PATH = Path("reports/glowing_heart_v2_6_difference_packet_index.preview.json")
GRAPH_PATH = Path("Docs/Observatory/Observation_Atlas/Atlas_Graph/glowing_heart_difference_packet_exhibits.graph.json")
SVG_PATH = Path("reports/glowing_heart_v2_9_evidence_map.svg")
MAP_PREVIEW_PATH = Path("reports/glowing_heart_v2_9_evidence_map.preview.md")
GALLERY_DOCS_PATH = Path("Docs/xPRIMEray/project_glowing_heart_v2_4_difference_packet_gallery.md")
GALLERY_PREVIEW_PATH = Path("reports/glowing_heart_v2_4_difference_packet_gallery.preview.md")
OUTPUT_PATH = Path("reports/glowing_heart_v2_10_evidence_map_index.preview.json")
STATUS_ORDER = ("Comparable", "Unknown", "NotComparable", "RequiresTransform")


def load_object(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except FileNotFoundError as exc:
        raise ValueError(f"required JSON not found: {path}") from exc
    except json.JSONDecodeError as exc:
        raise ValueError(f"{path}:{exc.lineno}:{exc.colno}: invalid JSON: {exc.msg}") from exc
    if not isinstance(value, dict):
        raise ValueError(f"{path}: expected an object")
    return value


def markdown_anchor(title: str) -> str:
    value = re.sub(r"[^a-z0-9 -]", "", title.lower())
    return "#" + re.sub(r"[ -]+", "-", value).strip("-")


def verify_files() -> None:
    for path in (SVG_PATH, MAP_PREVIEW_PATH, GALLERY_DOCS_PATH, GALLERY_PREVIEW_PATH):
        if not path.is_file():
            raise ValueError(f"required artifact not found: {path}")


def verify_svg(exhibit_ids: set[str]) -> None:
    try:
        root = ET.parse(SVG_PATH).getroot()
    except ET.ParseError as exc:
        raise ValueError(f"{SVG_PATH}: invalid SVG XML: {exc}") from exc
    namespace = {"svg": "http://www.w3.org/2000/svg"}
    card_ids = {
        group.attrib["id"]
        for group in root.findall(".//svg:g", namespace)
        if group.attrib.get("data-node-kind") == "exhibit"
    }
    if card_ids != exhibit_ids:
        raise ValueError(f"{SVG_PATH}: exhibit card IDs do not match the source index")


def build_index(packet_index: dict[str, Any], graph: dict[str, Any]) -> dict[str, Any]:
    generated_utc = packet_index.get("generatedUtc")
    if not isinstance(generated_utc, str) or not generated_utc:
        raise ValueError("difference packet index generatedUtc must be a non-empty string")
    entries = packet_index.get("entries")
    if not isinstance(entries, list) or not entries:
        raise ValueError("difference packet index entries must be a non-empty array")
    graph_id = graph.get("graphId")
    if not isinstance(graph_id, str) or not graph_id:
        raise ValueError("Atlas Graph graphId must be a non-empty string")
    nodes = graph.get("nodes")
    if not isinstance(nodes, list):
        raise ValueError("Atlas Graph nodes must be an array")
    graph_nodes = {node.get("id"): node for node in nodes if isinstance(node, dict)}
    exhibit_ids = {entry.get("id") for entry in entries}
    if None in exhibit_ids or len(exhibit_ids) != len(entries):
        raise ValueError("difference packet index exhibit IDs must be present and unique")
    verify_svg(exhibit_ids)

    exhibits = []
    counts = Counter()
    for position, entry in enumerate(entries):
        context = f"entries[{position}] id='{entry.get('id', '<missing>')}'"
        for field in ("id", "title", "status", "rule", "claimBoundary", "comparisonMode", "parityClaim", "runtimeExecuted"):
            if field not in entry:
                raise ValueError(f"{context}.{field}: required field is missing")
        if entry["parityClaim"] != "NONE" or entry["runtimeExecuted"] is not False or entry["comparisonMode"] != "core_vs_core":
            raise ValueError(f"{context}: fixed comparison claims are invalid")
        if entry["status"] not in STATUS_ORDER:
            raise ValueError(f"{context}.status: unsupported value '{entry['status']}'")
        node = graph_nodes.get(entry["id"])
        if node is None:
            raise ValueError(f"{context}: matching Atlas Graph node is missing")
        metadata = node.get("metadata")
        if not isinstance(metadata, dict):
            raise ValueError(f"{context}: graph node metadata is missing")
        for field in ("status", "rule", "claimBoundary"):
            expected = entry[field]
            actual = metadata.get(field)
            if actual != expected:
                raise ValueError(f"{context}: graph metadata {field} does not match the source index")
        counts[entry["status"]] += 1
        exhibits.append(
            {
                "id": entry["id"],
                "title": entry["title"],
                "status": entry["status"],
                "rule": entry["rule"],
                "graphNodeId": node["id"],
                "gallerySectionAnchor": markdown_anchor(entry["title"]),
                "evidenceMapCardLabel": node["label"],
                "claimBoundary": entry["claimBoundary"],
            }
        )

    claim_boundary = packet_index.get("claimBoundary")
    if not isinstance(claim_boundary, list) or not claim_boundary:
        raise ValueError("difference packet index claimBoundary must be a non-empty array")
    return {
        "indexId": "xprimeray.glowing_heart.evidence_map_index.v0.preview",
        "title": "Project Glowing Heart Evidence Map Discovery Index",
        "version": "v0.preview",
        "generatedUtc": generated_utc,
        "sources": {
            "differencePacketIndex": PACKET_INDEX_PATH.as_posix(),
            "atlasGraph": GRAPH_PATH.as_posix(),
        },
        "generatedArtifacts": {
            "evidenceMapSvg": SVG_PATH.as_posix(),
            "evidenceMapPreview": MAP_PREVIEW_PATH.as_posix(),
            "galleryDocs": GALLERY_DOCS_PATH.as_posix(),
            "galleryPreview": GALLERY_PREVIEW_PATH.as_posix(),
        },
        "renderers": {
            "atlasGraphBridge": "tools/glowing_heart_index_to_atlas_graph.py",
            "evidenceMap": "tools/atlas_graph_evidence_map_renderer.py",
            "gallery": "tools/glowing_heart_gallery_renderer.py",
        },
        "artifactChain": [
            {"stage": "difference_packet_index", "path": PACKET_INDEX_PATH.as_posix(), "kind": "json"},
            {"stage": "atlas_graph", "path": GRAPH_PATH.as_posix(), "kind": "json", "renderer": "tools/glowing_heart_index_to_atlas_graph.py"},
            {"stage": "evidence_map", "path": SVG_PATH.as_posix(), "kind": "svg", "renderer": "tools/atlas_graph_evidence_map_renderer.py"},
            {"stage": "gallery", "path": GALLERY_DOCS_PATH.as_posix(), "kind": "markdown", "renderer": "tools/glowing_heart_gallery_renderer.py"},
        ],
        "graphId": graph_id,
        "exhibitCount": len(exhibits),
        "statusCounts": {status: counts.get(status, 0) for status in STATUS_ORDER},
        "comparisonMode": "core_vs_core",
        "parityClaim": "NONE",
        "runtimeExecuted": False,
        "exhibits": exhibits,
        "claimBoundary": claim_boundary,
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
    output = Path(argv[1]) if len(argv) == 2 else OUTPUT_PATH
    try:
        verify_files()
        value = build_index(load_object(PACKET_INDEX_PATH), load_object(GRAPH_PATH))
        write_atomic(output, value)
    except (OSError, TypeError, ValueError) as exc:
        print(f"FAIL: {exc}", file=sys.stderr)
        return 1
    print(f"PASS: wrote {output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
