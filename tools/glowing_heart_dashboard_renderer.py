#!/usr/bin/env python3
"""Render the Glowing Heart dashboard seed as Markdown and a safe SVG overview."""

from __future__ import annotations

import html
import json
import os
import sys
import tempfile
import textwrap
from pathlib import Path
from typing import Any


STATUS_STYLE = {
    "Comparable": ("#166534", "#dcfce7", "#86efac"),
    "Unknown": ("#92400e", "#fef3c7", "#fcd34d"),
    "NotComparable": ("#7f1d1d", "#fee2e2", "#fca5a5"),
    "RequiresTransform": ("#1e40af", "#dbeafe", "#93c5fd"),
}
METRICS = ("countCompared", "maxAbsDifference", "meanAbsDifference", "nonZeroCount")
STATUS_ORDER = ("Comparable", "Unknown", "NotComparable", "RequiresTransform")


def load_json(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except FileNotFoundError as exc:
        raise ValueError(f"JSON not found: {path}") from exc
    except json.JSONDecodeError as exc:
        raise ValueError(f"{path}:{exc.lineno}:{exc.colno}: invalid JSON: {exc.msg}") from exc
    if not isinstance(value, dict):
        raise ValueError(f"{path}: expected an object")
    return value


def validate_schema(seed: dict[str, Any], schema: dict[str, Any]) -> None:
    try:
        from jsonschema import Draft202012Validator, FormatChecker
    except ImportError:
        return
    errors = sorted(
        Draft202012Validator(schema, format_checker=FormatChecker()).iter_errors(seed),
        key=lambda error: list(error.absolute_path),
    )
    if errors:
        error = errors[0]
        location = ".".join(str(part) for part in error.absolute_path) or "<root>"
        raise ValueError(f"seed schema error at {location}: {error.message}")


def validate_seed(seed: dict[str, Any]) -> dict[str, Any]:
    if seed.get("comparisonMode") != "core_vs_core":
        raise ValueError("comparisonMode must be 'core_vs_core'")
    if seed.get("parityClaim") != "NONE":
        raise ValueError("parityClaim must be 'NONE'")
    if seed.get("runtimeExecuted") is not False:
        raise ValueError("runtimeExecuted must be false")
    groups = seed.get("groups")
    if not isinstance(groups, list) or len(groups) != 1:
        raise ValueError(f"groups must contain exactly one dashboard group; found {len(groups) if isinstance(groups, list) else 0}")
    group = groups[0]
    if not isinstance(group, dict):
        raise ValueError("groups[0] must be an object")
    exhibits = group.get("exhibits")
    if not isinstance(exhibits, list) or len(exhibits) != 5:
        raise ValueError(f"groups[0].exhibits must contain exactly five entries; found {len(exhibits) if isinstance(exhibits, list) else 0}")
    observed = {status: 0 for status in STATUS_ORDER}
    seen: set[str] = set()
    for position, exhibit in enumerate(exhibits):
        context = f"groups[0].exhibits[{position}]"
        if not isinstance(exhibit, dict):
            raise ValueError(f"{context} must be an object")
        exhibit_id = exhibit.get("exhibitId")
        if not isinstance(exhibit_id, str) or not exhibit_id or exhibit_id in seen:
            raise ValueError(f"{context}.exhibitId must be present and unique")
        seen.add(exhibit_id)
        status = exhibit.get("status")
        if status not in STATUS_STYLE:
            raise ValueError(f"{context}.status has unsupported value {status!r}")
        observed[status] += 1
        for field in METRICS:
            value = exhibit.get(field)
            if not isinstance(value, (int, float)) or isinstance(value, bool) or value < 0:
                raise ValueError(f"{context}.{field} must be a non-negative number")
        boundary = exhibit.get("claimBoundary")
        if not isinstance(boundary, list) or not boundary or not all(isinstance(item, str) and item for item in boundary):
            raise ValueError(f"{context}.claimBoundary must be a non-empty string array")
    if group.get("statusCounts") != observed:
        raise ValueError(f"groups[0].statusCounts does not match exhibit statuses: {observed}")
    return group


def esc(value: Any) -> str:
    return html.escape(str(value), quote=True)


def lines(value: str, width: int, limit: int) -> list[str]:
    wrapped = textwrap.wrap(value, width=width, break_long_words=False, break_on_hyphens=False)
    if len(wrapped) > limit:
        wrapped = wrapped[:limit]
        wrapped[-1] = wrapped[-1].rstrip(" .") + "..."
    return wrapped


def metric_summary(exhibit: dict[str, Any]) -> tuple[str, str]:
    return (
        f"{exhibit['countCompared']} compared / {exhibit['nonZeroCount']} non-zero",
        f"max {exhibit['maxAbsDifference']} / mean {exhibit['meanAbsDifference']}",
    )


def render_svg(seed: dict[str, Any], group: dict[str, Any]) -> str:
    width, height = 1400, 900
    card_w, card_h, gap, start_x, card_y = 250, 430, 25, 25, 335
    parts = [
        '<?xml version="1.0" encoding="UTF-8"?>',
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}" role="img" aria-labelledby="dashboard-title dashboard-desc">',
        '<title id="dashboard-title">Glowing Heart Observer Fixture Dashboard</title>',
        '<desc id="dashboard-desc">One Core smoke observer group with five recorded comparison-decision exhibits.</desc>',
        '<rect width="1400" height="900" fill="#f8fafc"/>',
        '<text x="36" y="38" font-family="system-ui, sans-serif" font-size="13" font-weight="700" fill="#475569">PROJECT GLOWING HEART · DASHBOARD SEED VIEW</text>',
        f'<text x="36" y="78" font-family="system-ui, sans-serif" font-size="28" font-weight="700" fill="#0f172a">{esc(seed["title"])}</text>',
        f'<g id="{esc(group["groupId"])}" data-node-kind="dashboard-group">',
        '<rect x="36" y="105" width="1328" height="178" rx="7" fill="#ffffff" stroke="#94a3b8" stroke-width="1.5"/>',
        f'<text x="60" y="143" font-family="system-ui, sans-serif" font-size="20" font-weight="700" fill="#0f172a">{esc(group["title"])}</text>',
        f'<text x="60" y="177" font-family="system-ui, sans-serif" font-size="13" fill="#475569">Observer: {esc(group["observerBasis"])}</text>',
        f'<text x="60" y="202" font-family="system-ui, sans-serif" font-size="13" fill="#475569">Fixture family: {esc(group["fixtureFamily"])}</text>',
        f'<text x="60" y="227" font-family="system-ui, sans-serif" font-size="13" fill="#475569">Channels: {esc(" · ".join(group["channelIds"]))}</text>',
    ]
    badge_x = 760
    for status in STATUS_ORDER:
        foreground, fill, border = STATUS_STYLE[status]
        count = group["statusCounts"][status]
        parts.extend([
            f'<rect x="{badge_x}" y="160" width="135" height="48" rx="5" fill="{fill}" stroke="{border}"/>',
            f'<text x="{badge_x + 67.5:g}" y="180" text-anchor="middle" font-family="system-ui, sans-serif" font-size="11" font-weight="700" fill="{foreground}">{esc(status)}</text>',
            f'<text x="{badge_x + 67.5:g}" y="199" text-anchor="middle" font-family="system-ui, sans-serif" font-size="16" font-weight="700" fill="{foreground}">{count}</text>',
        ])
        badge_x += 145
    parts.extend([
        '<rect x="760" y="226" width="570" height="34" rx="4" fill="#f1f5f9" stroke="#cbd5e1"/>',
        '<text x="1045" y="248" text-anchor="middle" font-family="system-ui, sans-serif" font-size="11" font-weight="700" fill="#334155">CLAIM-BOUNDED · RECORDED EVIDENCE ONLY</text>',
        '</g>',
    ])

    for index, exhibit in enumerate(group["exhibits"]):
        x = start_x + index * (card_w + gap)
        foreground, fill, border = STATUS_STYLE[exhibit["status"]]
        first_metric, second_metric = metric_summary(exhibit)
        boundary_text = " ".join(exhibit["claimBoundary"])
        parts.extend([
            f'<g id="{esc(exhibit["exhibitId"])}" data-node-kind="exhibit" data-status="{esc(exhibit["status"])}">',
            f'<desc>{esc(boundary_text)}</desc>',
            f'<rect x="{x}" y="{card_y}" width="{card_w}" height="{card_h}" rx="7" fill="#ffffff" stroke="{border}" stroke-width="2"/>',
            f'<rect x="{x + 16}" y="{card_y + 18}" width="{card_w - 32}" height="32" rx="4" fill="{fill}"/>',
            f'<text x="{x + card_w / 2:g}" y="{card_y + 40}" text-anchor="middle" font-family="system-ui, sans-serif" font-size="13" font-weight="700" fill="{foreground}">{esc(exhibit["status"])}</text>',
        ])
        for line_index, line in enumerate(lines(exhibit["title"], 23, 2)):
            parts.append(f'<text x="{x + 16}" y="{card_y + 82 + line_index * 23}" font-family="system-ui, sans-serif" font-size="18" font-weight="700" fill="#0f172a">{esc(line)}</text>')
        parts.append(f'<text x="{x + 16}" y="{card_y + 140}" font-family="system-ui, sans-serif" font-size="11" font-weight="700" fill="#64748b">RULE</text>')
        for line_index, line in enumerate(lines(exhibit["rule"], 32, 3)):
            parts.append(f'<text x="{x + 16}" y="{card_y + 159 + line_index * 16}" font-family="ui-monospace, monospace" font-size="11" fill="#334155">{esc(line)}</text>')
        parts.extend([
            f'<line x1="{x + 16}" y1="{card_y + 220}" x2="{x + card_w - 16}" y2="{card_y + 220}" stroke="#e2e8f0"/>',
            f'<text x="{x + 16}" y="{card_y + 247}" font-family="system-ui, sans-serif" font-size="11" font-weight="700" fill="#64748b">SEED METRICS</text>',
            f'<text x="{x + 16}" y="{card_y + 273}" font-family="system-ui, sans-serif" font-size="13" font-weight="650" fill="#0f172a">{esc(first_metric)}</text>',
        ])
        for line_index, line in enumerate(lines(second_metric, 32, 2)):
            parts.append(f'<text x="{x + 16}" y="{card_y + 297 + line_index * 15}" font-family="system-ui, sans-serif" font-size="11" fill="#475569">{esc(line)}</text>')
        parts.extend([
            f'<text x="{x + 16}" y="{card_y + 337}" font-family="system-ui, sans-serif" font-size="10" fill="#64748b">{esc(exhibit["leftChannel"])}</text>',
            f'<text x="{x + 16}" y="{card_y + 355}" font-family="system-ui, sans-serif" font-size="10" fill="#64748b">to {esc(exhibit["rightChannel"])}</text>',
            f'<rect x="{x + 16}" y="{card_y + 374}" width="{card_w - 32}" height="34" rx="4" fill="#f1f5f9" stroke="#cbd5e1"/>',
            f'<text x="{x + card_w / 2:g}" y="{card_y + 396}" text-anchor="middle" font-family="system-ui, sans-serif" font-size="11" font-weight="700" fill="#334155">CLAIM BOUNDARY RECORDED</text>',
            '</g>',
        ])
    parts.extend([
        '<text x="36" y="820" font-family="system-ui, sans-serif" font-size="12" fill="#475569">Core-vs-Core only · not a Godot, image, or pixel comparison · no parity claim</text>',
        '<text x="1364" y="820" text-anchor="end" font-family="system-ui, sans-serif" font-size="12" fill="#475569">Status and metrics sourced from dashboard seed</text>',
        '<text x="36" y="854" font-family="system-ui, sans-serif" font-size="11" fill="#64748b">Dashboard view organizes recorded evidence; it does not establish scientific correctness.</text>',
        '</svg>',
        '',
    ])
    return "\n".join(parts)


def render_markdown(seed_path: Path, seed: dict[str, Any], group: dict[str, Any], svg_path: Path) -> str:
    lines_out = [
        "# Glowing Heart v3.1 Observer Fixture Dashboard Preview",
        "",
        "<!-- Generated by tools/glowing_heart_dashboard_renderer.py. Do not edit exhibit values by hand. -->",
        "",
        f"Rendered from `{seed_path.as_posix()}` version `{seed['version']}`.",
        "",
        f"![Glowing Heart Observer Fixture Dashboard]({svg_path.name})",
        "",
        "## Dashboard Group",
        "",
        "| Observer | Fixture family | Channels | Exhibits |",
        "|---|---|---|---:|",
        f"| {group['observerBasis']} | `{group['fixtureFamily']}` | {', '.join(f'`{item}`' for item in group['channelIds'])} | {len(group['exhibits'])} |",
        "",
        "## Status Counts",
        "",
        "| Status | Count |",
        "|---|---:|",
    ]
    lines_out.extend(f"| `{status}` | {group['statusCounts'][status]} |" for status in STATUS_ORDER)
    lines_out += ["", "## Exhibits", "", "| Exhibit | Status | Rule | Compared | Maximum difference | Mean difference | Non-zero |", "|---|---|---|---:|---:|---:|---:|"]
    for exhibit in group["exhibits"]:
        lines_out.append(f"| {exhibit['title']} | `{exhibit['status']}` | `{exhibit['rule']}` | {exhibit['countCompared']} | {exhibit['maxAbsDifference']} | {exhibit['meanAbsDifference']} | {exhibit['nonZeroCount']} |")
    lines_out += ["", "## Exhibit Claim Boundaries", ""]
    for exhibit in group["exhibits"]:
        lines_out += [f"### {exhibit['title']}", ""]
        lines_out.extend(f"- {item}" for item in exhibit["claimBoundary"])
        lines_out.append("")
    lines_out += ["## Dashboard Claim Boundary", ""]
    lines_out.extend(f"- {item}" for item in seed["claimBoundary"])
    lines_out += ["", "Status and metric values are rendered directly from the dashboard seed. No status is inferred from color, text, or topology.", ""]
    return "\n".join(lines_out)


def atomic_write(path: Path, content: str) -> None:
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
    if len(argv) != 5:
        print(f"Usage: {argv[0]} <seed.json> <schema.json> <output.md> <output.svg>", file=sys.stderr)
        return 2
    seed_path, schema_path, markdown_path, svg_path = map(Path, argv[1:])
    if len({path.resolve() for path in (seed_path, schema_path, markdown_path, svg_path)}) != 4:
        print("FAIL: input and output paths must be distinct", file=sys.stderr)
        return 2
    try:
        seed = load_json(seed_path)
        schema = load_json(schema_path)
        validate_schema(seed, schema)
        group = validate_seed(seed)
        markdown = render_markdown(seed_path, seed, group, svg_path)
        svg = render_svg(seed, group)
        atomic_write(markdown_path, markdown)
        atomic_write(svg_path, svg)
    except (OSError, TypeError, ValueError) as exc:
        print(f"FAIL: {exc}", file=sys.stderr)
        return 1
    print(f"PASS: rendered one group and five exhibits to {markdown_path} and {svg_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))
