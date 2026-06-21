#!/usr/bin/env python3
"""Generate a preview Markdown gallery for Glowing Heart output packets.

This is a preview-only browser layer. It reads complete packet folders from
output/glowing_heart and writes reports/glowing_heart_gallery.preview.md.
It does not modify the production Observatory catalog or website code.
"""

from __future__ import annotations

import argparse
import json
from collections import Counter
from dataclasses import dataclass
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


DEFAULT_PACKETS_ROOT = Path("output/glowing_heart")
DEFAULT_OUTPUT = Path("reports/glowing_heart_gallery.preview.md")
REQUIRED_PACKET_FILES = ("manifest.json", "observatory_entry.json", "snapshot_ascii.txt")


class PacketError(Exception):
    pass


@dataclass(frozen=True)
class GalleryRun:
    run_id: str
    timestamp: str
    fixture: str
    phase: str
    validation: str
    artifact_type: str
    packet_dir: Path
    manifest_path: Path
    summary_path: Path
    ascii_preview: list[str]


def load_json(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:
        raise PacketError(f"{path}: invalid JSON: {exc}") from exc

    if not isinstance(value, dict):
        raise PacketError(f"{path}: expected JSON object")
    return value


def required_str(value: Any, label: str, path: Path) -> str:
    if not isinstance(value, str) or not value.strip():
        raise PacketError(f"{path}: required field {label} must be a non-empty string")
    return value


def parse_timestamp(value: str, path: Path) -> datetime:
    if not value.endswith("Z"):
        raise PacketError(f"{path}: timestamp must be UTC and end with Z")
    try:
        return datetime.strptime(value, "%Y-%m-%dT%H:%M:%SZ")
    except ValueError as exc:
        raise PacketError(f"{path}: timestamp must match YYYY-MM-DDTHH:MM:SSZ") from exc


def validate_manifest(manifest: dict[str, Any], path: Path) -> tuple[str, str, str]:
    run_id = required_str(manifest.get("runId"), "runId", path)
    phase = required_str(manifest.get("phase"), "phase", path)
    result = manifest.get("result")
    if not isinstance(result, dict):
        raise PacketError(f"{path}: required field result must be an object")
    validation = required_str(result.get("validation"), "result.validation", path)
    return run_id, phase, validation


def validate_entry(entry: dict[str, Any], path: Path) -> tuple[str, str, str]:
    fixture = required_str(entry.get("fixture"), "fixture", path)
    artifact_type = required_str(entry.get("artifact_type"), "artifact_type", path)
    timestamp = required_str(entry.get("timestamp"), "timestamp", path)
    parse_timestamp(timestamp, path)
    return fixture, artifact_type, timestamp


def read_ascii_preview(path: Path, max_rows: int = 12) -> list[str]:
    lines = path.read_text(encoding="utf-8").splitlines()
    visual_start = 0
    for index, line in enumerate(lines):
        if not line.strip():
            visual_start = index + 1
            break

    visual_rows = [line.rstrip() for line in lines[visual_start:] if line.strip()]
    return visual_rows[:max_rows]


def discover_packet_dirs(root: Path) -> list[Path]:
    if not root.exists():
        return []
    return sorted(path for path in root.iterdir() if path.is_dir())


def load_run(packet_dir: Path) -> GalleryRun:
    missing = [name for name in REQUIRED_PACKET_FILES if not (packet_dir / name).is_file()]
    if missing:
        raise PacketError(f"{packet_dir}: missing {', '.join(missing)}")

    manifest_path = packet_dir / "manifest.json"
    entry_path = packet_dir / "observatory_entry.json"
    ascii_path = packet_dir / "snapshot_ascii.txt"
    run_id, phase, validation = validate_manifest(load_json(manifest_path), manifest_path)
    fixture, artifact_type, timestamp = validate_entry(load_json(entry_path), entry_path)
    preview = read_ascii_preview(ascii_path)
    if not preview:
        raise PacketError(f"{ascii_path}: no visual preview rows found")

    return GalleryRun(
        run_id=run_id,
        timestamp=timestamp,
        fixture=fixture,
        phase=phase,
        validation=validation,
        artifact_type=artifact_type,
        packet_dir=packet_dir,
        manifest_path=manifest_path,
        summary_path=packet_dir / "run_summary.md",
        ascii_preview=preview,
    )


def build_gallery(runs: list[GalleryRun], skipped: int, generated: datetime) -> str:
    validation_counts = Counter(run.validation for run in runs)
    fixture_counts = Counter(run.fixture for run in runs)
    generated_text = generated.strftime("%Y-%m-%dT%H:%M:%SZ")

    lines: list[str] = [
        "# Glowing Heart Observatory Gallery (Preview)",
        "",
        f"Generated: {generated_text}",
        "",
        f"Runs discovered: {len(runs)}",
        "",
        "---",
        "",
        "## Statistics",
        "",
        f"Runs: {len(runs)}",
        "",
        f"PASS: {validation_counts.get('PASS', 0)}",
        "",
        f"FAIL: {validation_counts.get('FAIL', 0)}",
        "",
        f"Skipped packets: {skipped}",
        "",
        "Fixtures:",
    ]

    if fixture_counts:
        for fixture, count in sorted(fixture_counts.items()):
            lines.append(f"- {fixture}: {count}")
    else:
        lines.append("- none: 0")

    for run in runs:
        lines.extend(
            [
                "",
                "---",
                "",
                f"## {run.run_id}",
                "",
                f"Fixture: {run.fixture}",
                "",
                f"Phase: {run.phase}",
                "",
                f"Validation: {run.validation}",
                "",
                "Artifact Type:",
                run.artifact_type,
                "",
                "Manifest:",
                run.manifest_path.as_posix(),
                "",
                "Summary:",
                run.summary_path.as_posix(),
                "",
                "Snapshot:",
                "",
                "```txt",
                *run.ascii_preview,
                "```",
            ]
        )

    return "\n".join(lines) + "\n"


def main() -> int:
    parser = argparse.ArgumentParser(description="Generate a preview Markdown gallery for Glowing Heart output packets.")
    parser.add_argument("--packets-root", type=Path, default=DEFAULT_PACKETS_ROOT)
    parser.add_argument("--out", type=Path, default=DEFAULT_OUTPUT)
    args = parser.parse_args()

    packet_dirs = discover_packet_dirs(args.packets_root)
    runs: list[GalleryRun] = []
    skipped = 0
    for packet_dir in packet_dirs:
        try:
            runs.append(load_run(packet_dir))
        except PacketError:
            skipped += 1

    runs.sort(key=lambda run: (run.timestamp, run.run_id), reverse=True)
    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text(build_gallery(runs, skipped, datetime.now(timezone.utc)), encoding="utf-8")

    print("[glowing-heart-gallery]")
    print(f"discovered={len(packet_dirs)}")
    print(f"valid={len(runs)}")
    print(f"skipped={skipped}")
    print()
    print(f"wrote={args.out}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
