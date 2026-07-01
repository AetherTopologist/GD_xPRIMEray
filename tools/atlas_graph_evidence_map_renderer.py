#!/usr/bin/env python3
"""Render a claim-safe Glowing Heart evidence map from Atlas Graph JSON."""

from __future__ import annotations

import html
import json
import os
import sys
import tempfile
import textwrap
from collections import Counter
from pathlib import Path
from typing import Any

from atlas_graph_validate import validate_graph


STATUS_STYLE = {
    "Comparable": ("#166534", "#dcfce7", "#86efac"),
    "Unknown": ("#92400e", "#fef3c7", "#fcd34d"),
    "NotComparable": ("#7f1d1d", "#fee2e2", "#fca5a5"),
    "RequiresTransform": ("#1e40af", "#dbeafe", "#93c5fd"),
}
METRIC_FIELDS = ("countCompared", "maxAbsDifference", "meanAbsDifference", "nonZeroCount")


def load_graph(path: Path) -> dict[str, Any]:
    try:
        graph = json.loads(path.read_text(encoding="utf-8"))
    except FileNotFoundError as exc:
        raise ValueError(f"graph not found: {path}") from exc
    except json.JSONDecodeError as exc:
        raise ValueError(f"{path}:{exc.lineno}:{exc.colno}: invalid JSON: {exc.msg}") from exc
    if not isinstance(graph, dict):
        raise ValueError("graph: expected an object")
    structural_errors = validate_graph(graph)
    if structural_errors:
        raise ValueError("graph is invalid: " + "; ".join(structural_errors))
    return graph


def validate_evidence_map(graph: dict[str, Any]) -> tuple[dict[str, Any], list[dict[str, Any]]]:
    roots = [node for node in graph["nodes"] if node.get("type") == "project"]
    exhibits = [node for node in graph["nodes"] if node.get("type") == "artifact"]
    if len(roots) != 1:
        raise ValueError(f"nodes: expected exactly one project root, found {len(roots)}")
    if len(exhibits) != 5:
        raise ValueError(f"nodes: expected exactly five artifact exhibits, found {len(exhibits)}")

    root = roots[0]
    exhibit_ids = {node["id"] for node in exhibits}
    for position, node in enumerate(exhibits):
        context = f"exhibits[{position}] id='{node.get('id', '<missing>')}'"
        metadata = node.get("metadata")
        if not isinstance(metadata, dict):
            raise ValueError(f"{context}.metadata: expected an object")
        for field in ("status", "rule", *METRIC_FIELDS):
            if field not in metadata:
                raise ValueError(f"{context}.metadata.{field}: required field is missing")
        if metadata["status"] not in STATUS_STYLE:
            raise ValueError(f"{context}.metadata.status: unsupported value {metadata['status']!r}")
        if not isinstance(node.get("claimBoundary"), str) or not node["claimBoundary"].strip():
            raise ValueError(f"{context}.claimBoundary: required non-empty text is missing")

    if len(graph["edges"]) != 5:
        raise ValueError(f"edges: expected exactly five root-to-exhibit edges, found {len(graph['edges'])}")
    destinations: set[str] = set()
    for position, edge in enumerate(graph["edges"]):
        context = f"edges[{position}] id='{edge.get('id', '<missing>')}'"
        if edge["from"] in exhibit_ids or edge["to"] in exhibit_ids and edge["from"] != root["id"]:
            raise ValueError(f"{context}: inter-exhibit or non-root edge is not allowed")
        if edge["from"] != root["id"] or edge["to"] not in exhibit_ids:
            raise ValueError(f"{context}: expected root '{root['id']}' to an exhibit node")
        if edge["to"] in destinations:
            raise ValueError(f"{context}.to: duplicate exhibit edge target {edge['to']!r}")
        destinations.add(edge["to"])
    if destinations != exhibit_ids:
        raise ValueError("edges: every exhibit must have exactly one incoming edge from the root")

    return root, exhibits


def esc(value: Any) -> str:
    return html.escape(str(value), quote=True)


def text_lines(text: str, width: int, limit: int) -> list[str]:
    lines = textwrap.wrap(text, width=width, break_long_words=False, break_on_hyphens=False)
    if len(lines) > limit:
        lines = lines[:limit]
        lines[-1] = lines[-1].rstrip(" .") + "..."
    return lines


def metric_summary(metadata: dict[str, Any]) -> str:
    count = metadata["countCompared"]
    nonzero = metadata["nonZeroCount"]
    maximum = metadata["maxAbsDifference"]
    if count and nonzero == 0:
        return f"{count} compared · all zero"
    if nonzero:
        return f"{nonzero} non-zero · max {maximum}"
    return f"{count} compared"


def render_svg(graph: dict[str, Any], root: dict[str, Any], exhibits: list[dict[str, Any]]) -> str:
    width, height = 1400, 900
    card_w, card_h, gap, start_x, card_y = 250, 410, 25, 25, 330
    root_x, root_y, root_w, root_h = 400, 45, 600, 150
    root_center_x = root_x + root_w / 2
    parts = [
        '<?xml version="1.0" encoding="UTF-8"?>',
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}" role="img" aria-labelledby="map-title map-desc">',
        '<title id="map-title">Glowing Heart Difference Packet Evidence Map</title>',
        '<desc id="map-desc">One gallery root connected to five retained Core comparison-decision exhibits.</desc>',
        '<rect width="1400" height="900" fill="#f8fafc"/>',
        '<text x="48" y="34" font-family="system-ui, sans-serif" font-size="14" fill="#475569">ATLAS GRAPH · RECORDED COMPARISON DECISIONS</text>',
    ]

    for index in range(5):
        center_x = start_x + index * (card_w + gap) + card_w / 2
        parts.append(
            f'<path d="M {root_center_x:g} {root_y + root_h} C {root_center_x:g} 255, {center_x:g} 255, {center_x:g} {card_y}" fill="none" stroke="#94a3b8" stroke-width="2"/>'
        )

    parts.extend(
        [
            f'<g id="{esc(root["id"])}" data-node-kind="root">',
            f'<rect x="{root_x}" y="{root_y}" width="{root_w}" height="{root_h}" rx="8" fill="#ffffff" stroke="#334155" stroke-width="2"/>',
            f'<text x="{root_center_x:g}" y="92" text-anchor="middle" font-family="system-ui, sans-serif" font-size="24" font-weight="700" fill="#0f172a">{esc(root["label"])}</text>',
            '<text x="700" y="124" text-anchor="middle" font-family="system-ui, sans-serif" font-size="14" fill="#475569">Atlas Graph evidence root · five exhibit artifacts</text>',
            '<rect x="563" y="145" width="274" height="30" rx="4" fill="#f1f5f9" stroke="#cbd5e1"/>',
            '<text x="700" y="165" text-anchor="middle" font-family="system-ui, sans-serif" font-size="12" font-weight="600" fill="#334155">CLAIM-BOUNDED ARTIFACT MAP</text>',
            '</g>',
        ]
    )

    for index, node in enumerate(exhibits):
        x = start_x + index * (card_w + gap)
        metadata = node["metadata"]
        foreground, badge_fill, border = STATUS_STYLE[metadata["status"]]
        parts.extend(
            [
                f'<g id="{esc(node["id"])}" data-node-kind="exhibit" data-status="{esc(metadata["status"])}">',
                f'<rect x="{x}" y="{card_y}" width="{card_w}" height="{card_h}" rx="7" fill="#ffffff" stroke="{border}" stroke-width="2"/>',
                f'<rect x="{x + 16}" y="{card_y + 18}" width="{card_w - 32}" height="32" rx="4" fill="{badge_fill}"/>',
                f'<text x="{x + card_w / 2:g}" y="{card_y + 40}" text-anchor="middle" font-family="system-ui, sans-serif" font-size="13" font-weight="700" fill="{foreground}">{esc(metadata["status"])}</text>',
            ]
        )
        title_lines = text_lines(node["label"], 23, 2)
        for line_index, line in enumerate(title_lines):
            parts.append(
                f'<text x="{x + 16}" y="{card_y + 82 + line_index * 23}" font-family="system-ui, sans-serif" font-size="18" font-weight="700" fill="#0f172a">{esc(line)}</text>'
            )
        rule_y = card_y + 136
        parts.append(f'<text x="{x + 16}" y="{rule_y}" font-family="system-ui, sans-serif" font-size="11" font-weight="700" fill="#64748b">RULE</text>')
        for line_index, line in enumerate(text_lines(metadata["rule"], 32, 3)):
            parts.append(
                f'<text x="{x + 16}" y="{rule_y + 19 + line_index * 16}" font-family="ui-monospace, monospace" font-size="11" fill="#334155">{esc(line)}</text>'
            )
        parts.extend(
            [
                f'<line x1="{x + 16}" y1="{card_y + 220}" x2="{x + card_w - 16}" y2="{card_y + 220}" stroke="#e2e8f0"/>',
                f'<text x="{x + 16}" y="{card_y + 248}" font-family="system-ui, sans-serif" font-size="12" font-weight="700" fill="#64748b">KEY METRIC</text>',
                f'<text x="{x + 16}" y="{card_y + 276}" font-family="system-ui, sans-serif" font-size="15" font-weight="650" fill="#0f172a">{esc(metric_summary(metadata))}</text>',
                f'<rect x="{x + 16}" y="{card_y + 306}" width="{card_w - 32}" height="34" rx="4" fill="#f1f5f9" stroke="#cbd5e1"/>',
                f'<text x="{x + card_w / 2:g}" y="{card_y + 328}" text-anchor="middle" font-family="system-ui, sans-serif" font-size="11" font-weight="700" fill="#334155">▣ CLAIM BOUNDARY</text>',
                f'<text x="{x + 16}" y="{card_y + 370}" font-family="system-ui, sans-serif" font-size="11" fill="#64748b">{esc(len(node["claimBoundary"]))} chars recorded</text>',
                '</g>',
            ]
        )

    parts.extend(
        [
            '<g aria-label="Status legend">',
            '<text x="38" y="798" font-family="system-ui, sans-serif" font-size="12" font-weight="700" fill="#475569">STATUS LEGEND</text>',
        ]
    )
    legend_x = 160
    for status in ("Comparable", "Unknown", "NotComparable"):
        foreground, fill, border = STATUS_STYLE[status]
        parts.extend(
            [
                f'<rect x="{legend_x}" y="778" width="150" height="30" rx="4" fill="{fill}" stroke="{border}"/>',
                f'<text x="{legend_x + 75}" y="798" text-anchor="middle" font-family="system-ui, sans-serif" font-size="12" font-weight="700" fill="{foreground}">{status}</text>',
            ]
        )
        legend_x += 170
    parts.extend(
        [
            '</g>',
            f'<text x="38" y="854" font-family="system-ui, sans-serif" font-size="12" fill="#475569">Source graph: {esc(graph["graphId"])}</text>',
            '<text x="1362" y="854" text-anchor="end" font-family="system-ui, sans-serif" font-size="12" fill="#475569">Evidence structure only · no ranking or correctness inference</text>',
            '</svg>',
            '',
        ]
    )
    return "\n".join(parts)


def render_markdown(graph_path: Path, graph: dict[str, Any], exhibits: list[dict[str, Any]], svg_path: Path) -> str:
    counts = Counter(node["metadata"]["status"] for node in exhibits)
    lines = [
        "# Glowing Heart v2.9 Evidence Map Preview",
        "",
        "A compact Atlas Graph rendering of five recorded Difference Packet decisions.",
        "",
        f"![Glowing Heart Difference Packet evidence map]({svg_path.name})",
        "",
        "## Nodes",
        "",
        "| Exhibit | Status | Rule | Key metric | Claim boundary |",
        "|---|---|---|---|---|",
    ]
    for node in exhibits:
        metadata = node["metadata"]
        lines.append(
            f"| {node['label']} | `{metadata['status']}` | `{metadata['rule']}` | {metric_summary(metadata)} | Recorded |"
        )
    lines.extend(["", "## Status Legend", ""])
    for status in ("Comparable", "Unknown", "NotComparable", "RequiresTransform"):
        lines.append(f"- `{status}`: {counts.get(status, 0)} exhibit(s)")
    lines.extend(
        [
            "",
            "## Claim Boundary",
            "",
            "Every exhibit displays a Claim Boundary indicator. Boundary text remains in the source graph and is not parsed to infer status.",
            "",
            f"Source graph: `{graph_path.as_posix()}`",
            "",
            "## What This Does Not Show",
            "",
            "- Core-vs-Core only.",
            "- Not a Godot comparison.",
            "- Not image or pixel comparison.",
            "- Not parity.",
            "- Not physical validation.",
            "- Not renderer equivalence.",
            "- The evidence map renders recorded comparison decisions only; it does not validate scientific correctness.",
            "",
        ]
    )
    return "\n".join(lines)


def write_atomic(path: Path, content: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    descriptor, temporary = tempfile.mkstemp(prefix=f".{path.name}.", dir=path.parent, text=True)
    try:
        with os.fdopen(descriptor, "w", encoding="utf-8", newline="\n") as handle:
            handle.write(content)
        os.replace(temporary, path)
    except Exception:
        try:
            os.unlink(temporary)
        except FileNotFoundError:
            pass
        raise


def main(argv: list[str]) -> int:
    if len(argv) != 4:
        print(f"Usage: {argv[0]} <graph.json> <output.svg> <preview.md>", file=sys.stderr)
        return 2
    graph_path, svg_path, markdown_path = map(Path, argv[1:])
    if len({path.resolve() for path in (graph_path, svg_path, markdown_path)}) != 3:
        print("FAIL: input and output paths must be distinct", file=sys.stderr)
        return 2
    try:
        graph = load_graph(graph_path)
        root, exhibits = validate_evidence_map(graph)
        svg = render_svg(graph, root, exhibits)
        markdown = render_markdown(graph_path, graph, exhibits, svg_path)
        write_atomic(svg_path, svg)
        write_atomic(markdown_path, markdown)
    except (OSError, TypeError, ValueError) as exc:
        print(f"FAIL: {exc}", file=sys.stderr)
        return 1
    print(f"PASS: rendered {len(exhibits)} exhibits to {svg_path} and {markdown_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))

