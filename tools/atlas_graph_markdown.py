#!/usr/bin/env python3
"""Render an Atlas Graph JSON document as a deterministic Markdown preview."""

from __future__ import annotations

import sys
from pathlib import Path
from typing import Any

from atlas_graph_validate import load_graph, validate_graph


def markdown_cell(value: Any) -> str:
    return str(value).replace("|", "\\|").replace("\n", " ")


def mermaid_text(value: Any) -> str:
    return str(value).replace('"', "'").replace("\n", " ")


def render_markdown(graph: dict[str, Any]) -> str:
    nodes = graph["nodes"]
    edges = graph["edges"]
    direction = graph["renderHints"].get("direction", "LR")

    lines = [
        f"# {graph['title']} (Preview)",
        "",
        graph["description"],
        "",
        f"Graph ID: `{graph['graphId']}`",
        "",
        f"Version: `{graph['version']}`",
        "",
        "## Claim Boundary",
        "",
        graph["claimBoundary"],
        "",
        "## Nodes",
        "",
        "| ID | Label | Type | Category | Maturity | Description | Claim Boundary |",
        "|---|---|---|---|---|---|---|",
    ]
    for node in nodes:
        lines.append(
            "| "
            + " | ".join(
                markdown_cell(node[key])
                for key in ("id", "label", "type", "category", "maturity", "description", "claimBoundary")
            )
            + " |"
        )

    lines.extend(
        [
            "",
            "## Edges",
            "",
            "| ID | From | To | Relationship | Label | Claim Boundary |",
            "|---|---|---|---|---|---|",
        ]
    )
    for edge in edges:
        lines.append(
            "| "
            + " | ".join(
                markdown_cell(edge[key])
                for key in ("id", "from", "to", "relationship", "label", "claimBoundary")
            )
            + " |"
        )

    lines.extend(["", "## Diagram", "", "```mermaid", f"flowchart {direction}"])
    for node in nodes:
        lines.append(f"    {node['id']}[\"{mermaid_text(node['label'])}\"]")
    for edge in edges:
        lines.append(
            f"    {edge['from']} -->|{mermaid_text(edge['label'])}| {edge['to']}"
        )
    lines.extend(["```", "", "## Evidence", ""])

    if graph["evidence"]:
        lines.extend(["| Label | Reference | Kind |", "|---|---|---|"])
        for item in graph["evidence"]:
            lines.append(
                f"| {markdown_cell(item['label'])} | `{markdown_cell(item['reference'])}` | {markdown_cell(item['kind'])} |"
            )
    else:
        lines.append("No evidence references declared.")

    lines.extend(
        [
            "",
            "## Status",
            "",
            "This preview describes graph structure only.",
            "",
            "No parity claim.",
            "No scientific validation claim.",
            "No proof claim.",
            "",
        ]
    )
    return "\n".join(lines)


def main(argv: list[str]) -> int:
    if len(argv) != 3:
        print("Usage: atlas_graph_markdown.py <graph.json> <output.md>", file=sys.stderr)
        return 2

    source = Path(argv[1])
    output = Path(argv[2])
    try:
        graph = load_graph(source)
    except ValueError as exc:
        print(f"[atlas-graph-markdown] FAIL: {exc}", file=sys.stderr)
        return 1

    errors = validate_graph(graph)
    if errors:
        print("[atlas-graph-markdown] FAIL: graph is invalid", file=sys.stderr)
        for error in errors:
            print(f"error={error}", file=sys.stderr)
        return 1

    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(render_markdown(graph), encoding="utf-8")
    print("[atlas-graph-markdown] PASS")
    print(f"source={source.as_posix()}")
    print(f"output={output.as_posix()}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
