#!/usr/bin/env python3
"""Validate Atlas Graph structure and references without runtime execution."""

from __future__ import annotations

import json
import sys
from pathlib import Path
from typing import Any


TOP_LEVEL_FIELDS = (
    "graphId",
    "title",
    "description",
    "version",
    "nodes",
    "edges",
    "groups",
    "claimBoundary",
    "evidence",
    "renderHints",
)
NODE_FIELDS = (
    "id",
    "label",
    "type",
    "category",
    "maturity",
    "description",
    "claimBoundary",
    "links",
    "tags",
)
EDGE_FIELDS = ("id", "from", "to", "relationship", "label", "claimBoundary")
GROUP_FIELDS = ("id", "label", "description", "nodeIds")
LINK_FIELDS = ("label", "href")
EVIDENCE_FIELDS = ("label", "reference", "kind")
RENDER_HINT_FIELDS = ("direction", "layout", "showGroups")
NODE_TYPES = {
    "observer",
    "grammar_node",
    "artifact",
    "contract",
    "measurement",
    "representation",
    "project",
    "future",
    "territory",
}
MATURITY_LABELS = {
    "Not Applicable",
    "Vision",
    "Prototype",
    "Experimental",
    "Internal Validation",
    "Public Demo",
    "Research Ready",
    "Production Ready",
}
RELATIONSHIPS = {
    "reading_path",
    "describes",
    "produces",
    "requires",
    "contextualizes",
    "measures",
    "represents",
    "planned",
    "related_to",
}
EVIDENCE_KINDS = {
    "repository_doc",
    "schema",
    "report",
    "artifact",
    "external_source",
    "other",
}
RENDER_DIRECTIONS = {"LR", "RL", "TB", "BT"}
RENDER_LAYOUTS = {"flowchart", "layered", "radial", "manual"}
RELATIONSHIP_LABEL_TERMS = {
    "reading_path": ("reading", "route", "journey"),
    "describes": ("describe",),
    "produces": ("produce", "emit", "output", "support"),
    "requires": ("require", "depend", "need"),
    "contextualizes": ("context", "situate", "orient"),
    "measures": ("measure",),
    "represents": ("represent",),
    "planned": ("plan", "future"),
    "related_to": ("related", "associate", "connect"),
}


def load_graph(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        raise ValueError(f"could not load JSON: {exc}") from exc
    if not isinstance(value, dict):
        raise ValueError("top-level value must be an object")
    return value


def missing_fields(value: dict[str, Any], required: tuple[str, ...]) -> list[str]:
    return [field for field in required if field not in value]


def validate_graph(graph: dict[str, Any]) -> list[str]:
    errors: list[str] = []
    missing = missing_fields(graph, TOP_LEVEL_FIELDS)
    if missing:
        errors.append(f"graph missing fields: {', '.join(missing)}")

    nodes = graph.get("nodes")
    edges = graph.get("edges")
    groups = graph.get("groups")
    if not isinstance(nodes, list) or not nodes:
        errors.append("nodes must be a non-empty array")
        nodes = []
    if not isinstance(edges, list):
        errors.append("edges must be an array")
        edges = []
    if not isinstance(groups, list):
        errors.append("groups must be an array")
        groups = []

    node_ids: set[str] = set()
    for index, node in enumerate(nodes):
        label = f"nodes[{index}]"
        if not isinstance(node, dict):
            errors.append(f"{label} must be an object")
            continue
        node_missing = missing_fields(node, NODE_FIELDS)
        if node_missing:
            errors.append(f"{label} missing fields: {', '.join(node_missing)}")
        node_id = node.get("id")
        if not isinstance(node_id, str) or not node_id:
            errors.append(f"{label}.id must be a non-empty string")
        elif node_id in node_ids:
            errors.append(f"duplicate node id: {node_id}")
        else:
            node_ids.add(node_id)
        if node.get("type") not in NODE_TYPES:
            errors.append(f"{label}.type has unsupported value: {node.get('type')!r}")
        if node.get("maturity") not in MATURITY_LABELS:
            errors.append(f"{label}.maturity has unsupported value: {node.get('maturity')!r}")
        if not isinstance(node.get("links"), list):
            errors.append(f"{label}.links must be an array")
        else:
            for link_index, link in enumerate(node["links"]):
                link_label = f"{label}.links[{link_index}]"
                if not isinstance(link, dict):
                    errors.append(f"{link_label} must be an object")
                    continue
                link_missing = missing_fields(link, LINK_FIELDS)
                if link_missing:
                    errors.append(f"{link_label} missing fields: {', '.join(link_missing)}")
        if not isinstance(node.get("tags"), list):
            errors.append(f"{label}.tags must be an array")

    edge_ids: set[str] = set()
    for index, edge in enumerate(edges):
        label = f"edges[{index}]"
        if not isinstance(edge, dict):
            errors.append(f"{label} must be an object")
            continue
        edge_missing = missing_fields(edge, EDGE_FIELDS)
        if edge_missing:
            errors.append(f"{label} missing fields: {', '.join(edge_missing)}")
        edge_id = edge.get("id")
        if not isinstance(edge_id, str) or not edge_id:
            errors.append(f"{label}.id must be a non-empty string")
        elif edge_id in edge_ids:
            errors.append(f"duplicate edge id: {edge_id}")
        else:
            edge_ids.add(edge_id)
        if edge.get("from") not in node_ids:
            errors.append(f"{label}.from references unknown node: {edge.get('from')!r}")
        if edge.get("to") not in node_ids:
            errors.append(f"{label}.to references unknown node: {edge.get('to')!r}")
        if edge.get("relationship") not in RELATIONSHIPS:
            errors.append(f"{label}.relationship has unsupported value: {edge.get('relationship')!r}")

    group_ids: set[str] = set()
    for index, group in enumerate(groups):
        label = f"groups[{index}]"
        if not isinstance(group, dict):
            errors.append(f"{label} must be an object")
            continue
        group_missing = missing_fields(group, GROUP_FIELDS)
        if group_missing:
            errors.append(f"{label} missing fields: {', '.join(group_missing)}")
        group_id = group.get("id")
        if not isinstance(group_id, str) or not group_id:
            errors.append(f"{label}.id must be a non-empty string")
        elif group_id in group_ids:
            errors.append(f"duplicate group id: {group_id}")
        else:
            group_ids.add(group_id)
        group_nodes = group.get("nodeIds")
        if not isinstance(group_nodes, list):
            errors.append(f"{label}.nodeIds must be an array")
            continue
        for node_id in group_nodes:
            if node_id not in node_ids:
                errors.append(f"{label}.nodeIds references unknown node: {node_id!r}")

    if graph.get("version") != "v0.1":
        errors.append("version must be 'v0.1'")
    if not isinstance(graph.get("claimBoundary"), str) or not graph.get("claimBoundary"):
        errors.append("claimBoundary must be a non-empty string")
    evidence = graph.get("evidence")
    if not isinstance(evidence, list):
        errors.append("evidence must be an array")
    else:
        for index, item in enumerate(evidence):
            label = f"evidence[{index}]"
            if not isinstance(item, dict):
                errors.append(f"{label} must be an object")
                continue
            item_missing = missing_fields(item, EVIDENCE_FIELDS)
            if item_missing:
                errors.append(f"{label} missing fields: {', '.join(item_missing)}")
            if item.get("kind") not in EVIDENCE_KINDS:
                errors.append(f"{label}.kind has unsupported value: {item.get('kind')!r}")

    render_hints = graph.get("renderHints")
    if not isinstance(render_hints, dict):
        errors.append("renderHints must be an object")
    else:
        hints_missing = missing_fields(render_hints, RENDER_HINT_FIELDS)
        if hints_missing:
            errors.append(f"renderHints missing fields: {', '.join(hints_missing)}")
        if render_hints.get("direction") not in RENDER_DIRECTIONS:
            errors.append(f"renderHints.direction has unsupported value: {render_hints.get('direction')!r}")
        if render_hints.get("layout") not in RENDER_LAYOUTS:
            errors.append(f"renderHints.layout has unsupported value: {render_hints.get('layout')!r}")
        if not isinstance(render_hints.get("showGroups"), bool):
            errors.append("renderHints.showGroups must be a boolean")
    return errors


def graph_warnings(graph: dict[str, Any]) -> list[str]:
    warnings: list[str] = []
    boundaries: list[tuple[str, Any]] = [("claimBoundary", graph.get("claimBoundary"))]
    for collection_name in ("nodes", "edges"):
        collection = graph.get(collection_name)
        if not isinstance(collection, list):
            continue
        for index, item in enumerate(collection):
            if isinstance(item, dict):
                boundaries.append(
                    (f"{collection_name}[{index}].claimBoundary", item.get("claimBoundary"))
                )
    for label, value in boundaries:
        if isinstance(value, str) and len(value.strip()) < 20:
            warnings.append(f"{label} is shorter than 20 characters")

    edges = graph.get("edges")
    if isinstance(edges, list):
        for index, edge in enumerate(edges):
            if not isinstance(edge, dict):
                continue
            relationship = edge.get("relationship")
            edge_label = edge.get("label")
            terms = RELATIONSHIP_LABEL_TERMS.get(relationship)
            if terms is None or not isinstance(edge_label, str):
                continue
            normalized = edge_label.lower().replace("_", " ").replace("-", " ")
            if not any(term in normalized for term in terms):
                warnings.append(
                    f"edges[{index}].label {edge_label!r} may conflict with relationship {relationship!r}"
                )
    return warnings


def main(argv: list[str]) -> int:
    if len(argv) != 2:
        print("Usage: atlas_graph_validate.py <graph.json>", file=sys.stderr)
        return 2

    path = Path(argv[1])
    try:
        graph = load_graph(path)
    except ValueError as exc:
        print("[atlas-graph-validate] FAIL")
        print(f"graph={path.as_posix()}")
        print(f"error={exc}")
        return 1

    errors = validate_graph(graph)
    warnings = graph_warnings(graph)
    print(f"[atlas-graph-validate] {'PASS' if not errors else 'FAIL'}")
    print(f"graph={path.as_posix()}")
    print(f"nodes={len(graph.get('nodes', []))}")
    print(f"edges={len(graph.get('edges', []))}")
    print(f"groups={len(graph.get('groups', []))}")
    for error in errors:
        print(f"error={error}")
    for warning in warnings:
        print(f"warning={warning}")
    print(f"warnings={len(warnings)}")
    return 0 if not errors else 1


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
