#!/usr/bin/env python3
"""Run the Glowing Heart evidence-chain preflight checks."""

from __future__ import annotations

import argparse
import json
import os
import shlex
import subprocess
import sys
import tempfile
import time
import xml.etree.ElementTree as ET
from dataclasses import dataclass
from pathlib import Path


PYTHON_FILES = (
    "tools/glowing_heart_evidence_chain_health.py",
    "tools/glowing_heart_evidence_map_index.py",
    "tools/atlas_graph_evidence_map_renderer.py",
    "tools/glowing_heart_index_to_atlas_graph.py",
    "tools/glowing_heart_gallery_renderer.py",
    "tools/atlas_graph_validate.py",
    "tools/atlas_graph_markdown.py",
)
JSON_FILES = (
    "reports/glowing_heart_v2_6_difference_packet_index.preview.json",
    "reports/glowing_heart_v2_10_evidence_map_index.preview.json",
    "Docs/Observatory/Observation_Atlas/Atlas_Graph/glowing_heart_difference_packet_exhibits.graph.json",
    "schemas/glowing_heart/difference_packet_index.v0.preview.json",
    "schemas/glowing_heart/evidence_map_index.v0.preview.json",
    "schemas/atlas_graph/atlas_graph_schema.v0.preview.json",
)
GRAPH = "Docs/Observatory/Observation_Atlas/Atlas_Graph/glowing_heart_difference_packet_exhibits.graph.json"
SVG = "reports/glowing_heart_v2_9_evidence_map.svg"
REPORT = Path("reports/glowing_heart_v2_12_preflight.preview.md")


@dataclass
class Result:
    name: str
    status: str
    duration: float
    command: str
    detail: str = ""


def arguments() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--quick", action="store_true", help="Skip .NET and MkDocs checks.")
    parser.add_argument("--skip-dotnet", action="store_true", help="Skip both .NET builds.")
    parser.add_argument("--skip-mkdocs", action="store_true", help="Skip the MkDocs build.")
    parser.add_argument("--output-md", type=Path, default=REPORT, help="Markdown report path.")
    return parser.parse_args()


def run(name: str, command: list[str]) -> Result:
    started = time.monotonic()
    completed = subprocess.run(command, text=True, capture_output=True)
    duration = time.monotonic() - started
    detail = ""
    if completed.returncode:
        detail = (completed.stdout + completed.stderr).strip()
    return Result(name, "PASS" if completed.returncode == 0 else "FAIL", duration, shlex.join(command), detail)


def skip(name: str, reason: str) -> Result:
    return Result(name, "SKIP", 0.0, reason)


def check_json() -> Result:
    started = time.monotonic()
    failures: list[str] = []
    for name in JSON_FILES:
        try:
            json.loads(Path(name).read_text(encoding="utf-8"))
        except (OSError, json.JSONDecodeError) as exc:
            failures.append(f"{name}: {exc}")
    return Result(
        "JSON syntax",
        "FAIL" if failures else "PASS",
        time.monotonic() - started,
        "parse required JSON files",
        "\n".join(failures),
    )


def check_svg() -> Result:
    started = time.monotonic()
    failures: list[str] = []
    try:
        root = ET.parse(SVG).getroot()
        groups = [node for node in root.iter() if node.tag.endswith("g") and node.get("data-node-kind") == "exhibit"]
        if len(groups) != 5:
            failures.append(f"expected 5 exhibit groups, found {len(groups)}")
        scripts = [node for node in root.iter() if node.tag.split("}")[-1].lower() == "script"]
        if scripts:
            failures.append(f"found {len(scripts)} script tags")
        for node in root.iter():
            for name, value in node.attrib.items():
                if name.endswith("href") and value.lower().startswith(("http://", "https://", "//")):
                    failures.append(f"external href: {value}")
    except (OSError, ET.ParseError) as exc:
        failures.append(str(exc))
    return Result("SVG safety", "FAIL" if failures else "PASS", time.monotonic() - started, f"parse and inspect {SVG}", "\n".join(failures))


def report_text(command: str, results: list[Result], duration: float) -> str:
    failed = [result for result in results if result.status == "FAIL"]
    overall = "FAIL" if failed else "PASS"
    lines = [
        "# Glowing Heart v2.12 Preflight Preview",
        "",
        f"Overall status: **{overall}**",
        "",
        f"Command: `{command}`",
        "",
        f"Total duration: {duration:.2f} seconds",
        "",
        "## Sections",
        "",
        "| Section | Status | Duration |",
        "|---|---|---:|",
    ]
    lines.extend(f"| {result.name} | `{result.status}` | {result.duration:.2f}s |" for result in results)
    lines += ["", "## Failure Details", ""]
    if failed:
        for result in failed:
            lines += [f"### {result.name}", "", f"Command: `{result.command}`", "", "```text", result.detail or "Command failed without output.", "```", ""]
    else:
        lines += ["None.", ""]
    lines += [
        "## Claim Boundary",
        "",
        "- Core-vs-Core only.",
        "- Not a Godot comparison.",
        "- Not image or pixel comparison.",
        "- Not parity.",
        "- Not physical validation.",
        "- Not renderer equivalence.",
        "- Preflight verifies artifact synchronization and tooling health, not scientific correctness.",
        "",
    ]
    return "\n".join(lines)


def atomic_write(path: Path, value: str) -> None:
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


def main() -> int:
    args = arguments()
    started = time.monotonic()
    skip_dotnet = args.quick or args.skip_dotnet
    skip_mkdocs = args.quick or args.skip_mkdocs
    results = [
        run("Python syntax", [sys.executable, "-m", "py_compile", *PYTHON_FILES]),
        check_json(),
        run("Atlas Graph validation", [sys.executable, "tools/atlas_graph_validate.py", GRAPH]),
        run(
            "Evidence Chain health",
            [
                sys.executable,
                "tools/glowing_heart_evidence_chain_health.py",
                "--output-json",
                "reports/glowing_heart_v2_11_evidence_chain_health.preview.json",
                "--output-md",
                "reports/glowing_heart_v2_11_evidence_chain_health.preview.md",
            ],
        ),
        check_svg(),
    ]
    if skip_dotnet:
        results.append(skip(".NET builds", "skipped by --quick or --skip-dotnet"))
    else:
        core = run(".NET Core build", ["dotnet", "build", "src/XPrimeRay.Core/XPrimeRay.Core.csproj"])
        cli = run(".NET CLI build", ["dotnet", "build", "src/XPrimeRay.Testbench.Cli/XPrimeRay.Testbench.Cli.csproj"])
        combined = Result(
            ".NET builds",
            "PASS" if core.status == cli.status == "PASS" else "FAIL",
            core.duration + cli.duration,
            f"{core.command}; {cli.command}",
            "\n".join(part for part in (core.detail, cli.detail) if part),
        )
        results.append(combined)
    results.append(skip("MkDocs", "skipped by --quick or --skip-mkdocs") if skip_mkdocs else run("MkDocs", ["mkdocs", "build"]))

    duration = time.monotonic() - started
    invoked = shlex.join([sys.executable, "tools/glowing_heart_preflight.py", *sys.argv[1:]])
    atomic_write(args.output_md, report_text(invoked, results, duration))
    for result in results:
        print(f"{result.status:4}  {result.name} ({result.duration:.2f}s)")
    failures = sum(result.status == "FAIL" for result in results)
    print(f"{'FAIL' if failures else 'PASS'}: {len(results)} sections, {failures} failures")
    print(f"Report: {args.output_md}")
    return 1 if failures else 0


if __name__ == "__main__":
    raise SystemExit(main())
