#!/usr/bin/env python3
"""Export static metadata for the selected Glowing Heart Godot fixture.

This is a text scan only. It does not execute Godot, instantiate scenes, modify
Godot files, modify Core transport, or touch the production Observatory catalog.
"""

from __future__ import annotations

import argparse
import json
import re
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


DEFAULT_SELECTED_FROM = Path("reports/glowing_heart_shared_fixture_candidate.preview.json")
DEFAULT_OUTPUT_JSON = Path("reports/glowing_heart_godot_fixture_export.preview.json")
DEFAULT_OUTPUT_MD = Path("reports/glowing_heart_godot_fixture_export.preview.md")
INTERESTING_TERMS = (
    "FieldSource3D",
    "Camera3D",
    "Grin",
    "GRIN",
    "Hermetic",
    "Observatory",
    "Ray",
    "Receiver",
    "Closure",
    "Boundary",
    "Metric",
    "Transport",
    "Renderer",
    "Probe",
    "HUD",
)
LIMITATIONS = [
    "Static text scan only",
    "Godot runtime was not executed",
    "Scene graph was not instantiated",
    "Exported values are metadata hints, not validated runtime state",
    "No parity or closure claim is made",
]


class ExportError(Exception):
    pass


@dataclass(frozen=True)
class Resource:
    type: str
    path: str | None
    id: str | None


@dataclass(frozen=True)
class Node:
    name: str
    type: str
    parent: str | None


def load_object(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:
        raise ExportError(f"{path}: failed to load JSON: {exc}") from exc
    if not isinstance(value, dict):
        raise ExportError(f"{path}: expected JSON object")
    return value


def selected_fixture_path(selected_from: Path) -> Path:
    data = load_object(selected_from)
    candidate = data.get("godotCandidate")
    if not isinstance(candidate, dict):
        raise ExportError(f"{selected_from}: expected godotCandidate object")
    value = candidate.get("path")
    if not isinstance(value, str) or not value.strip():
        raise ExportError(f"{selected_from}: expected godotCandidate.path")
    return Path(value)


def parse_attrs(line: str) -> dict[str, str]:
    return {match.group(1): match.group(2) for match in re.finditer(r'(\w+)="([^"]*)"', line)}


def parse_external_resources(lines: list[str]) -> list[Resource]:
    resources: list[Resource] = []
    for line in lines:
        if not line.startswith("[ext_resource"):
            continue
        attrs = parse_attrs(line)
        resources.append(
            Resource(
                type=attrs.get("type", ""),
                path=attrs.get("path"),
                id=attrs.get("id"),
            )
        )
    return resources


def parse_sub_resources(lines: list[str]) -> list[dict[str, str | None]]:
    resources: list[dict[str, str | None]] = []
    for line in lines:
        if not line.startswith("[sub_resource"):
            continue
        attrs = parse_attrs(line)
        resources.append(
            {
                "type": attrs.get("type"),
                "id": attrs.get("id"),
            }
        )
    return resources


def parse_nodes(lines: list[str]) -> list[Node]:
    nodes: list[Node] = []
    for line in lines:
        if not line.startswith("[node"):
            continue
        attrs = parse_attrs(line)
        nodes.append(
            Node(
                name=attrs.get("name", ""),
                type=attrs.get("type", ""),
                parent=attrs.get("parent"),
            )
        )
    return nodes


def interesting_references(lines: list[str]) -> dict[str, dict[str, Any]]:
    result: dict[str, dict[str, Any]] = {}
    for term in INTERESTING_TERMS:
        examples: list[str] = []
        count = 0
        term_lower = term.lower()
        for line in lines:
            if term_lower in line.lower():
                count += 1
                if len(examples) < 5:
                    examples.append(line)
        result[term] = {"count": count, "examples": examples}
    return result


def contains_any(text: str, terms: tuple[str, ...]) -> bool:
    lower = text.lower()
    return any(term.lower() in lower for term in terms)


def classified_hints(text: str) -> dict[str, bool]:
    return {
        "hasCamera": contains_any(text, ("Camera3D",)),
        "hasFieldSource": contains_any(text, ("FieldSource3D",)),
        "hasGrinSignal": contains_any(text, ("grin", "ROuter", "CanonicalGamma", "Amp")),
        "hasHermeticSignal": contains_any(text, ("hermetic",)),
        "hasObservatorySignal": contains_any(text, ("observatory",)),
        "hasReceiverSignal": contains_any(text, ("receiver", "hermetic_receiver")),
        "hasClosureSignal": contains_any(text, ("closure", "missHits == 0")),
        "hasBoundarySignal": contains_any(text, ("boundary",)),
        "hasWormholeSignal": contains_any(text, ("wormhole",)),
    }


def resource_to_json(resource: Resource) -> dict[str, str | None]:
    return {
        "type": resource.type,
        "path": resource.path,
        "id": resource.id,
    }


def node_to_json(node: Node) -> dict[str, str | None]:
    return {
        "name": node.name,
        "type": node.type,
        "parent": node.parent,
    }


def build_export(selected_from: Path, fixture_path: Path, generated: datetime) -> dict[str, Any]:
    if not fixture_path.is_file():
        raise ExportError(f"{fixture_path}: selected fixture file not found")

    text = fixture_path.read_text(encoding="utf-8", errors="ignore")
    lines = text.splitlines()
    gd_scene_header = next((line for line in lines if line.startswith("[gd_scene")), None)
    external_resources = parse_external_resources(lines)
    sub_resources = parse_sub_resources(lines)
    nodes = parse_nodes(lines)

    return {
        "schema": "xprimeray.glowing_heart.godot_fixture_export.v1.1",
        "generatedUtc": generated.strftime("%Y-%m-%dT%H:%M:%SZ"),
        "runtimeExecuted": False,
        "parityClaim": "NONE",
        "source": "static_tscn_text_scan",
        "selectedFrom": selected_from.as_posix(),
        "fixture": {
            "name": fixture_path.stem,
            "path": fixture_path.as_posix(),
            "fileSizeBytes": fixture_path.stat().st_size,
            "lineCount": len(lines),
            "gdSceneHeader": gd_scene_header,
        },
        "externalResources": [resource_to_json(resource) for resource in external_resources],
        "subResources": sub_resources,
        "nodes": [node_to_json(node) for node in nodes],
        "interestingReferences": interesting_references(lines),
        "classifiedHints": classified_hints(text),
        "limitations": LIMITATIONS,
    }


def bool_text(value: bool) -> str:
    return "true" if value else "false"


def table_value(value: Any) -> str:
    if value is None:
        return ""
    return str(value)


def build_markdown(export: dict[str, Any]) -> str:
    fixture = export["fixture"]
    hints = export["classifiedHints"]
    lines = [
        "# Glowing Heart Godot Fixture Export (Preview)",
        "",
        f"Generated: {export['generatedUtc']}",
        "",
        "Runtime executed: false",
        "",
        "Parity claim: NONE",
        "",
        "Source: static_tscn_text_scan",
        "",
        "## Fixture",
        "",
        "| Field | Value |",
        "|---|---|",
        f"| Name | {fixture['name']} |",
        f"| Path | {fixture['path']} |",
        f"| File Size | {fixture['fileSizeBytes']} |",
        f"| Lines | {fixture['lineCount']} |",
        "",
        "## Scene Header",
        "",
        "```txt",
        table_value(fixture.get("gdSceneHeader")),
        "```",
        "",
        "## Classified Hints",
        "",
        "| Hint | Value |",
        "|---|---|",
    ]

    for key in (
        "hasCamera",
        "hasFieldSource",
        "hasGrinSignal",
        "hasHermeticSignal",
        "hasObservatorySignal",
        "hasReceiverSignal",
        "hasClosureSignal",
        "hasBoundarySignal",
        "hasWormholeSignal",
    ):
        lines.append(f"| {key} | {bool_text(bool(hints[key]))} |")

    lines.extend(["", "## External Resources", "", "| Type | Path | Id |", "|---|---|---|"])
    for resource in export["externalResources"]:
        lines.append(f"| {resource['type']} | {resource.get('path') or ''} | {resource.get('id') or ''} |")

    lines.extend(["", "## Nodes", "", "| Name | Type | Parent |", "|---|---|---|"])
    for node in export["nodes"]:
        lines.append(f"| {node['name']} | {node['type']} | {node.get('parent') or ''} |")

    lines.extend(["", "## Interesting References", ""])
    for term, data in export["interestingReferences"].items():
        if data["count"] <= 0:
            continue
        lines.extend([f"### {term}", "", f"Count: {data['count']}", "", "```txt"])
        lines.extend(data["examples"])
        lines.extend(["```", ""])

    lines.extend(["## Limitations", ""])
    lines.extend(f"- {item}." for item in export["limitations"])
    return "\n".join(lines) + "\n"


def main() -> int:
    parser = argparse.ArgumentParser(description="Export static metadata for the selected Glowing Heart Godot fixture.")
    parser.add_argument("--selected-from", type=Path, default=DEFAULT_SELECTED_FROM)
    parser.add_argument("--out-json", type=Path, default=DEFAULT_OUTPUT_JSON)
    parser.add_argument("--out-md", type=Path, default=DEFAULT_OUTPUT_MD)
    args = parser.parse_args()

    if not args.selected_from.is_file():
        raise ExportError(f"{args.selected_from}: selected fixture packet not found")

    fixture_path = selected_fixture_path(args.selected_from)
    export = build_export(args.selected_from, fixture_path, datetime.now(timezone.utc))
    args.out_json.parent.mkdir(parents=True, exist_ok=True)
    args.out_md.parent.mkdir(parents=True, exist_ok=True)
    args.out_json.write_text(json.dumps(export, indent=2) + "\n", encoding="utf-8")
    args.out_md.write_text(build_markdown(export), encoding="utf-8")

    print("[glowing-heart-godot-fixture-export]")
    print(f"fixture={fixture_path.as_posix()}")
    print(f"nodes={len(export['nodes'])}")
    print(f"external_resources={len(export['externalResources'])}")
    print(f"sub_resources={len(export['subResources'])}")
    print("runtime_executed=false")
    print("parity_claim=NONE")
    print()
    print(f"wrote={args.out_json}")
    print(f"wrote={args.out_md}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except ExportError as exc:
        print(f"[glowing-heart-godot-fixture-export] ERROR: {exc}")
        raise SystemExit(1)
