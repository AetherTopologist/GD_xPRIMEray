#!/usr/bin/env python3
"""Check synchronization across the Glowing Heart evidence artifact chain."""

from __future__ import annotations

import argparse
import json
import os
import re
import sys
import tempfile
import xml.etree.ElementTree as ET
from collections import Counter
from pathlib import Path
from typing import Any


EXPECTED_GRAPH_ID = "glowing_heart.difference_packet_exhibits"
EXPECTED_COUNTS = {"Comparable": 2, "Unknown": 2, "NotComparable": 1, "RequiresTransform": 0}
METRIC_FIELDS = ("countCompared", "maxAbsDifference", "meanAbsDifference", "nonZeroCount")
BOUNDARY = [
    "Core-vs-Core only.",
    "Not a Godot comparison.",
    "Not image or pixel comparison.",
    "Not parity.",
    "Not physical validation.",
    "Not renderer equivalence.",
    "Health check validates artifact synchronization, not scientific correctness.",
]


def parser() -> argparse.ArgumentParser:
    value = argparse.ArgumentParser(description=__doc__)
    value.add_argument("--packet-index", type=Path, default=Path("reports/glowing_heart_v2_6_difference_packet_index.preview.json"))
    value.add_argument("--packet-schema", type=Path, default=Path("schemas/glowing_heart/difference_packet_index.v0.preview.json"))
    value.add_argument("--graph", type=Path, default=Path("Docs/Observatory/Observation_Atlas/Atlas_Graph/glowing_heart_difference_packet_exhibits.graph.json"))
    value.add_argument("--graph-schema", type=Path, default=Path("schemas/atlas_graph/atlas_graph_schema.v0.preview.json"))
    value.add_argument("--svg", type=Path, default=Path("reports/glowing_heart_v2_9_evidence_map.svg"))
    value.add_argument("--gallery-preview", type=Path, default=Path("reports/glowing_heart_v2_4_difference_packet_gallery.preview.md"))
    value.add_argument("--gallery-docs", type=Path, default=Path("Docs/xPRIMEray/project_glowing_heart_v2_4_difference_packet_gallery.md"))
    value.add_argument("--map-index", type=Path, default=Path("reports/glowing_heart_v2_10_evidence_map_index.preview.json"))
    value.add_argument("--map-schema", type=Path, default=Path("schemas/glowing_heart/evidence_map_index.v0.preview.json"))
    value.add_argument("--output-json", type=Path, default=Path("reports/glowing_heart_v2_11_evidence_chain_health.preview.json"))
    value.add_argument("--output-md", type=Path, default=Path("reports/glowing_heart_v2_11_evidence_chain_health.preview.md"))
    return value


class HealthCheck:
    def __init__(self, args: argparse.Namespace) -> None:
        self.args = args
        self.checks: list[dict[str, Any]] = []
        self.exhibits: list[dict[str, Any]] = []

    def add(self, group: str, ok: bool, message: str, path: Path, *, exhibit_id: str | None = None,
            warning: bool = False) -> None:
        item: dict[str, Any] = {
            "group": group,
            "severity": "info" if ok else ("warning" if warning else "error"),
            "message": message,
            "artifactPath": path.as_posix(),
        }
        if exhibit_id:
            item["exhibitId"] = exhibit_id
        self.checks.append(item)

    def load_json(self, group: str, path: Path) -> dict[str, Any] | None:
        try:
            value = json.loads(path.read_text(encoding="utf-8"))
            if not isinstance(value, dict):
                raise ValueError("top-level value is not an object")
        except (OSError, json.JSONDecodeError, ValueError) as exc:
            self.add(group, False, f"JSON load failed: {exc}", path)
            return None
        self.add(group, True, "JSON parses as an object.", path)
        return value

    def schema(self, group: str, instance: dict[str, Any], schema_path: Path, artifact: Path) -> None:
        schema = self.load_json(group, schema_path)
        if schema is None:
            return
        try:
            from jsonschema import Draft202012Validator, FormatChecker
        except ImportError:
            self.add(group, False, "jsonschema unavailable; schema check skipped.", artifact, warning=True)
            return
        errors = sorted(
            Draft202012Validator(schema, format_checker=FormatChecker()).iter_errors(instance),
            key=lambda error: list(error.absolute_path),
        )
        if errors:
            first = errors[0]
            location = ".".join(str(part) for part in first.absolute_path) or "<root>"
            self.add(group, False, f"Schema check failed at {location}: {first.message}", artifact)
        else:
            self.add(group, True, f"Validates against {schema_path.as_posix()}.", artifact)

    def packet_index(self) -> tuple[dict[str, Any] | None, dict[str, dict[str, Any]]]:
        group = "difference_packet_index"
        data = self.load_json(group, self.args.packet_index)
        if data is None:
            return None, {}
        self.schema(group, data, self.args.packet_schema, self.args.packet_index)
        entries = data.get("entries")
        if not isinstance(entries, list):
            self.add(group, False, "entries must be an array.", self.args.packet_index)
            return data, {}
        self.add(group, len(entries) == 5, f"Exhibit count is {len(entries)}; expected 5.", self.args.packet_index)
        indexed: dict[str, dict[str, Any]] = {}
        counts: Counter[str] = Counter()
        required = ("id", "title", "status", "rule", "metrics", "claimBoundary", "parityClaim", "runtimeExecuted", "comparisonMode")
        for position, entry in enumerate(entries):
            if not isinstance(entry, dict):
                self.add(group, False, f"entries[{position}] is not an object.", self.args.packet_index)
                continue
            exhibit_id = entry.get("id") if isinstance(entry.get("id"), str) else None
            missing = [field for field in required if field not in entry]
            unique = exhibit_id is not None and exhibit_id not in indexed
            guards = entry.get("parityClaim") == "NONE" and entry.get("runtimeExecuted") is False and entry.get("comparisonMode") == "core_vs_core"
            metrics = entry.get("metrics")
            metrics_ok = isinstance(metrics, dict) and all(field in metrics for field in METRIC_FIELDS)
            boundary_ok = isinstance(entry.get("claimBoundary"), list) and bool(entry["claimBoundary"])
            ok = not missing and unique and guards and metrics_ok and boundary_ok
            self.add(group, ok, f"Entry fields and claim guards {'pass' if ok else 'fail'}.", self.args.packet_index, exhibit_id=exhibit_id)
            if exhibit_id and unique:
                indexed[exhibit_id] = entry
            if isinstance(entry.get("status"), str):
                counts[entry["status"]] += 1
        observed = {key: counts.get(key, 0) for key in EXPECTED_COUNTS}
        self.add(group, observed == EXPECTED_COUNTS, f"Status counts: {observed}.", self.args.packet_index)
        return data, indexed

    def graph(self, packet: dict[str, dict[str, Any]]) -> tuple[dict[str, Any] | None, dict[str, dict[str, Any]]]:
        group = "atlas_graph"
        graph = self.load_json(group, self.args.graph)
        if graph is None:
            return None, {}
        try:
            from atlas_graph_validate import validate_graph
            errors = validate_graph(graph)
        except (ImportError, TypeError, ValueError) as exc:
            errors = [str(exc)]
        self.add(group, not errors, "Atlas Graph shared validation passed." if not errors else f"Atlas Graph errors: {'; '.join(errors[:3])}", self.args.graph)
        self.add(group, graph.get("graphId") == EXPECTED_GRAPH_ID, f"graphId is {graph.get('graphId')!r}.", self.args.graph)
        nodes = graph.get("nodes") if isinstance(graph.get("nodes"), list) else []
        roots = [node for node in nodes if isinstance(node, dict) and node.get("type") == "project"]
        artifacts = [node for node in nodes if isinstance(node, dict) and node.get("type") == "artifact"]
        self.add(group, len(roots) == 1, f"Project/root node count is {len(roots)}; expected 1.", self.args.graph)
        self.add(group, len(artifacts) == 5, f"Artifact node count is {len(artifacts)}; expected 5.", self.args.graph)
        graph_entries = {node.get("id"): node for node in artifacts if isinstance(node.get("id"), str)}
        self.add(group, set(graph_entries) == set(packet), "Graph exhibit IDs match the Difference Packet Index.", self.args.graph)
        for exhibit_id in sorted(set(packet) | set(graph_entries)):
            entry, node = packet.get(exhibit_id), graph_entries.get(exhibit_id)
            if entry is None or node is None:
                continue
            metadata = node.get("metadata") if isinstance(node.get("metadata"), dict) else {}
            expected_metrics = entry.get("metrics", {})
            metrics_ok = all(metadata.get(field) == expected_metrics.get(field) for field in METRIC_FIELDS)
            values_ok = metadata.get("status") == entry.get("status") and metadata.get("rule") == entry.get("rule") and metrics_ok
            boundary_ok = bool(node.get("claimBoundary")) and metadata.get("claimBoundary") == entry.get("claimBoundary")
            self.add(group, values_ok and boundary_ok, "Status, rule, metrics, and claim boundary match the source entry.", self.args.graph, exhibit_id=exhibit_id)
        root_id = roots[0].get("id") if len(roots) == 1 else None
        edges = graph.get("edges") if isinstance(graph.get("edges"), list) else []
        root_edges = [edge for edge in edges if isinstance(edge, dict) and edge.get("from") == root_id and edge.get("to") in graph_entries]
        inter_edges = [edge for edge in edges if isinstance(edge, dict) and edge.get("from") in graph_entries and edge.get("to") in graph_entries]
        targets = {edge.get("to") for edge in root_edges}
        edges_ok = len(edges) == 5 and len(root_edges) == 5 and targets == set(graph_entries) and not inter_edges
        self.add(group, edges_ok, f"Root-to-exhibit edges: {len(root_edges)}; inter-exhibit edges: {len(inter_edges)}.", self.args.graph)
        return graph, graph_entries

    def svg(self, packet: dict[str, dict[str, Any]]) -> set[str]:
        group = "evidence_map_svg"
        try:
            root = ET.parse(self.args.svg).getroot()
        except (OSError, ET.ParseError) as exc:
            self.add(group, False, f"SVG load failed: {exc}", self.args.svg)
            return set()
        self.add(group, True, "SVG parses as XML.", self.args.svg)
        self.add(group, root.get("viewBox") == "0 0 1400 900", f"viewBox is {root.get('viewBox')!r}.", self.args.svg)
        groups = [node for node in root.iter() if node.tag.endswith("g") and node.get("data-node-kind") == "exhibit"]
        ids = {node.get("id") for node in groups if node.get("id")}
        self.add(group, len(groups) == 5 and ids == set(packet), f"Found {len(groups)} exhibit groups; IDs match source index.", self.args.svg)
        text = " ".join("".join(node.itertext()) for node in root.iter()).upper()
        labels_ok = all(label in text for label in ("COMPARABLE", "UNKNOWN", "NOTCOMPARABLE", "CLAIM BOUNDARY"))
        titles_ok = all(str(entry.get("title", "")).upper() in text for entry in packet.values())
        self.add(group, labels_ok and titles_ok, "Status, claim-boundary, and exhibit title labels are present.", self.args.svg)
        scripts = [node for node in root.iter() if node.tag.split("}")[-1].lower() == "script"]
        external = []
        for node in root.iter():
            for name, value in node.attrib.items():
                if name.endswith("href") and value.lower().startswith(("http://", "https://", "//")):
                    external.append(value)
        self.add(group, not scripts and not external, f"Scripts: {len(scripts)}; external hrefs: {len(external)}.", self.args.svg)
        return ids

    def gallery(self, packet: dict[str, dict[str, Any]]) -> None:
        group = "gallery_markdown"
        contents: list[tuple[Path, str]] = []
        for path in (self.args.gallery_preview, self.args.gallery_docs):
            try:
                text = path.read_text(encoding="utf-8")
            except OSError as exc:
                self.add(group, False, f"Markdown load failed: {exc}", path)
                continue
            contents.append((path, text))
            self.add(group, True, "Markdown artifact exists and is readable.", path)
            heading = text.startswith("# Project Glowing Heart v2.4 Difference Packet Gallery")
            generated = "<!-- Generated by tools/glowing_heart_gallery_renderer.py." in text
            stale = bool(re.search(r"deferred\s+(?:case\s+e|incompatible\s+channel)|case\s+e.{0,80}deferred", text, re.I | re.S))
            entries_ok = all(f"`{exhibit_id}`" in text and f"## {entry['title']}" in text for exhibit_id, entry in packet.items())
            counts_ok = all(f"| `{status}` | {count} |" in text for status, count in EXPECTED_COUNTS.items())
            self.add(group, heading and generated and not stale and entries_ok and counts_ok,
                     "Heading, generator provenance, exhibits, status counts, and Case E state are current.", path)

    def map_index(self, packet: dict[str, dict[str, Any]], graph: dict[str, dict[str, Any]]) -> tuple[dict[str, Any] | None, dict[str, dict[str, Any]]]:
        group = "evidence_map_index"
        data = self.load_json(group, self.args.map_index)
        if data is None:
            return None, {}
        self.schema(group, data, self.args.map_schema, self.args.map_index)
        expected_paths = {
            "sources.differencePacketIndex": self.args.packet_index.as_posix(),
            "sources.atlasGraph": self.args.graph.as_posix(),
            "generatedArtifacts.evidenceMapSvg": self.args.svg.as_posix(),
            "generatedArtifacts.evidenceMapPreview": "reports/glowing_heart_v2_9_evidence_map.preview.md",
            "generatedArtifacts.galleryDocs": self.args.gallery_docs.as_posix(),
            "generatedArtifacts.galleryPreview": self.args.gallery_preview.as_posix(),
        }
        actual_paths = {
            "sources.differencePacketIndex": (data.get("sources") or {}).get("differencePacketIndex"),
            "sources.atlasGraph": (data.get("sources") or {}).get("atlasGraph"),
            "generatedArtifacts.evidenceMapSvg": (data.get("generatedArtifacts") or {}).get("evidenceMapSvg"),
            "generatedArtifacts.evidenceMapPreview": (data.get("generatedArtifacts") or {}).get("evidenceMapPreview"),
            "generatedArtifacts.galleryDocs": (data.get("generatedArtifacts") or {}).get("galleryDocs"),
            "generatedArtifacts.galleryPreview": (data.get("generatedArtifacts") or {}).get("galleryPreview"),
        }
        self.add(group, actual_paths == expected_paths, "All recorded source and generated artifact paths are canonical.", self.args.map_index)
        guards = data.get("parityClaim") == "NONE" and data.get("runtimeExecuted") is False and data.get("comparisonMode") == "core_vs_core"
        self.add(group, guards, "Fixed claim guards hold.", self.args.map_index)
        self.add(group, data.get("statusCounts") == EXPECTED_COUNTS, f"Status counts: {data.get('statusCounts')}.", self.args.map_index)
        exhibits = data.get("exhibits") if isinstance(data.get("exhibits"), list) else []
        indexed = {item.get("id"): item for item in exhibits if isinstance(item, dict) and isinstance(item.get("id"), str)}
        self.add(group, set(indexed) == set(packet) == set(graph), "Evidence Map Index exhibit IDs match packet and graph IDs.", self.args.map_index)
        for exhibit_id in sorted(set(packet) & set(indexed) & set(graph)):
            entry, item = packet[exhibit_id], indexed[exhibit_id]
            ok = item.get("status") == entry.get("status") and item.get("rule") == entry.get("rule") and item.get("claimBoundary") == entry.get("claimBoundary")
            self.add(group, ok, "Status, rule, and claim boundary match source artifacts.", self.args.map_index, exhibit_id=exhibit_id)
        return data, indexed

    def paths(self, artifacts: list[tuple[Path, Any]]) -> None:
        group = "cross_chain"
        lowercase: list[str] = []
        durable_tmp: list[str] = []

        def walk(value: Any, location: str = "", key: str = "") -> None:
            if isinstance(value, dict):
                for child_key, child in value.items():
                    walk(child, f"{location}.{child_key}" if location else child_key, child_key)
            elif isinstance(value, list):
                for index, child in enumerate(value):
                    walk(child, f"{location}[{index}]", key)
            elif isinstance(value, str):
                if re.search(r"(^|[^A-Za-z0-9_])fixtures/", value):
                    lowercase.append(location)
                if value.startswith("/tmp") and key != "lastVerifiedOutputPath":
                    durable_tmp.append(location)

        for path, value in artifacts:
            if value is not None:
                walk(value, path.as_posix())
        self.add(group, not lowercase, f"Standalone lowercase fixture paths: {lowercase or 'none'}.", self.args.packet_index)
        self.add(group, not durable_tmp, f"Durable /tmp paths: {durable_tmp or 'none'}; lastVerifiedOutputPath is treated as a non-durable trace.", self.args.packet_index)

    def run(self) -> dict[str, Any]:
        packet_data, packet = self.packet_index()
        graph_data, graph = self.graph(packet)
        svg_ids = self.svg(packet)
        self.gallery(packet)
        map_data, map_entries = self.map_index(packet, graph)
        same_ids = bool(packet) and set(packet) == set(graph) == svg_ids == set(map_entries)
        self.add("cross_chain", same_ids, "The same five exhibit IDs occur across structured indexes, graph, and SVG.", self.args.map_index)
        consistency = []
        for exhibit_id in sorted(packet):
            graph_meta = (graph.get(exhibit_id, {}).get("metadata") or {}) if exhibit_id in graph else {}
            map_item = map_entries.get(exhibit_id, {})
            ok = exhibit_id in svg_ids and graph_meta.get("status") == packet[exhibit_id].get("status") == map_item.get("status") and graph_meta.get("rule") == packet[exhibit_id].get("rule") == map_item.get("rule")
            consistency.append({"id": exhibit_id, "title": packet[exhibit_id].get("title", ""), "status": packet[exhibit_id].get("status", ""), "rule": packet[exhibit_id].get("rule", ""), "consistent": ok})
        self.exhibits = consistency
        self.paths([(self.args.packet_index, packet_data), (self.args.graph, graph_data), (self.args.map_index, map_data)])
        errors = sum(check["severity"] == "error" for check in self.checks)
        warnings = sum(check["severity"] == "warning" for check in self.checks)
        infos = sum(check["severity"] == "info" for check in self.checks)
        groups = []
        for name in dict.fromkeys(check["group"] for check in self.checks):
            selected = [check for check in self.checks if check["group"] == name]
            groups.append({"id": name, "status": "FAIL" if any(check["severity"] == "error" for check in selected) else "PASS", "checkCount": len(selected), "errorCount": sum(check["severity"] == "error" for check in selected), "warningCount": sum(check["severity"] == "warning" for check in selected)})
        generated = packet_data.get("generatedUtc") if isinstance(packet_data, dict) else "unknown"
        return {"reportId": "xprimeray.glowing_heart.evidence_chain_health.v0.preview", "title": "Project Glowing Heart Evidence Chain Health", "version": "v0.preview", "generatedUtc": generated, "overallStatus": "FAIL" if errors else "PASS", "checkGroups": groups, "checks": self.checks, "exhibitConsistency": consistency, "summary": {"total": len(self.checks), "info": infos, "warning": warnings, "error": errors}, "claimBoundary": BOUNDARY}


def markdown(report: dict[str, Any]) -> str:
    lines = ["# Glowing Heart v2.11 Evidence Chain Health Preview", "", "The health check walks the recorded Difference Packet Index -> Atlas Graph -> Evidence Map -> Gallery -> Evidence Map Index chain.", "", f"## {report['overallStatus']} Summary", "", f"- Checks: {report['summary']['total']}", f"- Errors: {report['summary']['error']}", f"- Warnings: {report['summary']['warning']}", "", "## Check Groups", "", "| Group | Status | Checks | Errors | Warnings |", "|---|---|---:|---:|---:|"]
    lines.extend(f"| `{group['id']}` | `{group['status']}` | {group['checkCount']} | {group['errorCount']} | {group['warningCount']} |" for group in report["checkGroups"])
    lines += ["", "## Exhibit Consistency", "", "| Exhibit | Status | Rule | Consistent |", "|---|---|---|---|"]
    lines.extend(f"| {item['title']} (`{item['id']}`) | `{item['status']}` | `{item['rule']}` | {'Yes' if item['consistent'] else 'No'} |" for item in report["exhibitConsistency"])
    issues = [check for check in report["checks"] if check["severity"] != "info"]
    lines += ["", "## Warnings and Errors", ""]
    lines.extend(f"- `{item['severity']}` `{item['group']}`: {item['message']}" for item in issues)
    if not issues:
        lines.append("None.")
    lines += ["", "## Claim Boundary", ""] + [f"- {item}" for item in report["claimBoundary"]]
    lines += ["", "## Next Milestone", "", "Glowing Heart v2.12 can add this command to a local preflight or CI-ready workflow.", ""]
    return "\n".join(lines)


def atomic_text(path: Path, value: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    descriptor, temporary = tempfile.mkstemp(prefix=f".{path.name}.", dir=path.parent, text=True)
    try:
        with os.fdopen(descriptor, "w", encoding="utf-8", newline="\n") as handle:
            handle.write(value)
        os.replace(temporary, path)
    except Exception:
        try:
            os.unlink(temporary)
        except FileNotFoundError:
            pass
        raise


def main(argv: list[str]) -> int:
    args = parser().parse_args(argv)
    report = HealthCheck(args).run()
    atomic_text(args.output_json, json.dumps(report, indent=2, ensure_ascii=True) + "\n")
    atomic_text(args.output_md, markdown(report))
    print(f"{report['overallStatus']}: {report['summary']['total']} checks, {report['summary']['error']} errors, {report['summary']['warning']} warnings")
    print(f"JSON: {args.output_json}")
    print(f"Markdown: {args.output_md}")
    return 0 if report["overallStatus"] == "PASS" else 1


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
