#!/usr/bin/env python3
"""Build a preview Observatory catalog from Glowing Heart CLI entries.

This is a dry-run utility. It reads generated
output/glowing_heart/*/observatory_entry.json files and writes a preview JSON
catalog. It never modifies reports/observatory_catalog.json.
"""

from __future__ import annotations

import argparse
import json
from datetime import datetime
from pathlib import Path
from typing import Any


DEFAULT_ENTRIES_ROOT = Path("output/glowing_heart")
DEFAULT_OUTPUT = Path("reports/glowing_heart_observatory_catalog.preview.json")
REQUIRED_FIELDS = (
    "category",
    "fixture",
    "run_id",
    "artifact_type",
    "coverage",
    "closure",
    "verdict",
    "timestamp",
    "source_path",
)


class ValidationError(Exception):
    pass


def load_json(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:
        raise ValidationError(f"{path}: invalid JSON: {exc}") from exc

    if not isinstance(value, dict):
        raise ValidationError(f"{path}: expected a JSON object")
    return value


def parse_timestamp(value: str, path: Path) -> datetime:
    if not value.endswith("Z"):
        raise ValidationError(f"{path}: timestamp must be UTC and end with Z")

    try:
        return datetime.strptime(value, "%Y-%m-%dT%H:%M:%SZ")
    except ValueError as exc:
        raise ValidationError(f"{path}: timestamp must match YYYY-MM-DDTHH:MM:SSZ") from exc


def validate_entry(entry: dict[str, Any], path: Path) -> dict[str, str]:
    normalized: dict[str, str] = {}
    for field in REQUIRED_FIELDS:
        value = entry.get(field)
        if not isinstance(value, str) or not value.strip():
            raise ValidationError(f"{path}: required field '{field}' must be a non-empty string")
        normalized[field] = value

    # Preserve known optional metadata when present, but keep the required
    # catalog row fields first and string-valued.
    for field in ("source", "phase"):
        value = entry.get(field)
        if value is not None:
            if not isinstance(value, str) or not value.strip():
                raise ValidationError(f"{path}: optional field '{field}' must be a non-empty string when present")
            normalized[field] = value

    parse_timestamp(normalized["timestamp"], path)
    return normalized


def discover_entries(entries_root: Path) -> list[tuple[Path, dict[str, str]]]:
    if not entries_root.exists():
        return []

    discovered: list[tuple[Path, dict[str, str]]] = []
    for path in sorted(entries_root.glob("*/observatory_entry.json")):
        discovered.append((path, validate_entry(load_json(path), path)))
    return discovered


def dedupe_by_run_id(entries: list[tuple[Path, dict[str, str]]]) -> tuple[list[dict[str, str]], int]:
    by_run_id: dict[str, tuple[Path, dict[str, str]]] = {}
    duplicate_count = 0

    # Deterministic winner: newest timestamp, then lexicographically later path.
    for path, entry in entries:
        run_id = entry["run_id"]
        current = by_run_id.get(run_id)
        if current is None:
            by_run_id[run_id] = (path, entry)
            continue

        duplicate_count += 1
        _, current_entry = current
        candidate_key = (entry["timestamp"], path.as_posix())
        current_key = (current_entry["timestamp"], current[0].as_posix())
        if candidate_key >= current_key:
            by_run_id[run_id] = (path, entry)

    records = [entry for _, entry in by_run_id.values()]
    records.sort(key=lambda entry: (entry["timestamp"], entry["run_id"]))
    return records, duplicate_count


def write_preview(records: list[dict[str, str]], output: Path) -> None:
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(records, indent=2, sort_keys=True) + "\n", encoding="utf-8")


def main() -> int:
    parser = argparse.ArgumentParser(description="Dry-run merge Glowing Heart Observatory entries into a preview catalog.")
    parser.add_argument("--entries-root", type=Path, default=DEFAULT_ENTRIES_ROOT)
    parser.add_argument("--out", type=Path, default=DEFAULT_OUTPUT)
    args = parser.parse_args()

    try:
        discovered = discover_entries(args.entries_root)
        records, duplicate_count = dedupe_by_run_id(discovered)
        write_preview(records, args.out)
    except ValidationError as exc:
        print(f"[glowing-heart-catalog-merge] ERROR: {exc}")
        return 1

    print(f"[glowing-heart-catalog-merge] entries_root={args.entries_root}")
    print(f"[glowing-heart-catalog-merge] discovered={len(discovered)}")
    print(f"[glowing-heart-catalog-merge] duplicates={duplicate_count}")
    print(f"[glowing-heart-catalog-merge] preview_records={len(records)}")
    print(f"[glowing-heart-catalog-merge] wrote={args.out}")
    print("[glowing-heart-catalog-merge] dry_run=true")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
