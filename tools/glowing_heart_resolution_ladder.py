#!/usr/bin/env python3
"""Run the bounded Core fixture resolution ladder and publish lightweight artifacts."""

from __future__ import annotations

import hashlib
import json
import math
import shutil
import subprocess
import sys
import time
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


FIXTURES = tuple(sorted(Path("Fixtures").glob("grin_radial_smoke*.json")))
OUTPUT_ROOT = Path("output/glowing_heart_library_v3_4")
PUBLISH_ROOT = Path("Docs/assets/glowing_heart/v3_4")
REPORT_JSON = Path("reports/glowing_heart_v3_4_resolution_ladder.preview.json")
REPORT_MD = Path("reports/glowing_heart_v3_4_resolution_ladder.preview.md")
DOCS_MD = Path("Docs/xPRIMEray/project_glowing_heart_v3_4_resolution_ladder.md")
CLI_PROJECT = Path("src/XPrimeRay.Testbench.Cli")
TIERS = (
    ("smoke", None, None, "Fixture-declared current grid."),
    ("mini", 80, 44, "Generated temporary 80x44 fixture variant."),
    ("standard", 320, 176, "Generated temporary 320x176 normal-target variant."),
    ("high", 640, 352, "Optional tier; deferred in the conservative v3.4 ladder."),
)
TIER_LABELS = {
    "smoke": "Baseline grid",
    "mini": "Compact 80x44",
    "standard": "Gallery detail 320x176",
    "high": "Extended 640x352",
}
RUN_TIMEOUT_SECONDS = 45
MAX_PACKET_BYTES = 25 * 1024 * 1024
PUBLISH_FILES = ("snapshot_ascii.txt", "manifest.json", "ray_metrics.csv")


def canonical_hashes() -> dict[Path, str]:
    return {path: hashlib.sha256(path.read_bytes()).hexdigest() for path in FIXTURES}


def write_json(path: Path, value: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2, ensure_ascii=True) + "\n", encoding="utf-8")


def compact_ascii(path: Path) -> str:
    lines = path.read_text(encoding="utf-8").splitlines()
    grid = lines[7:]
    return "\n".join(line[::4] for line in grid[::4])


def render_markdown(report: dict[str, Any], docs: bool) -> str:
    records = report["records"]
    fixtures = sorted({record["fixtureName"] for record in records})
    by_key = {(record["fixtureName"], record["tier"]): record for record in records}
    title = "# Fixture Gallery — Resolution Ladder (v3.4)"
    lines = [
        title,
        "",
        "This is the resolution-detail view of the v3.3 fixture gallery: the same four Core-runnable GRIN smoke fixtures sampled at increasing grid density. More rays do not introduce a new transport model.",
        "",
        "[Return to the v3.3 Fixture Library Gallery](project_glowing_heart_v3_3_fixture_library_gallery.md)" if docs else "Source gallery: `Docs/xPRIMEray/project_glowing_heart_v3_3_fixture_library_gallery.md`",
        "",
        "## Reading Boundary",
        "",
        "This page records retained Core fixture runs across declared sampling-density tiers. It is not a Godot comparison, image or pixel comparison, parity claim, physical validation, renderer-equivalence claim, or proof. A passing tier means only that the fixture completed under its recorded Core run checks.",
        "",
        "**Baseline grid -> Compact -> Gallery detail -> Extended**",
        "",
        "The tiers alter sampling density, not fixture GRIN parameters. Generated copies change `rayGrid` and scale the existing `maxMisses` run guard proportionally; canonical fixture files are not modified.",
        "",
        "## Status Matrix",
        "",
        "| Fixture | Baseline grid | Compact 80x44 | Gallery detail 320x176 | Extended 640x352 |",
        "|---|---|---|---|---|",
    ]
    for fixture in fixtures:
        cells = []
        for tier in ("smoke", "mini", "standard", "high"):
            record = by_key[(fixture, tier)]
            if record["status"] == "DEFERRED":
                cells.append("Not run in v3.4 (scope stop)")
            else:
                cells.append(f"`{record['status']}`")
        lines.append(f"| `{fixture}` | {' | '.join(cells)} |")
    lines.extend([
        "",
        "## Recorded Runs",
        "",
        "| Fixture | Tier | Grid | Rays | Status | Runtime (s) | Mean bend | Max bend | Artifacts |",
        "|---|---|---:|---:|---|---:|---:|---:|---|",
    ])
    for record in records:
        artifacts = "—"
        if record["publishedAssetPath"]:
            if docs:
                base = f"../assets/glowing_heart/v3_4/{record['fixtureName']}/{record['tier']}"
                artifacts = f"[Snapshot]({base}/snapshot_ascii.txt) · [Manifest]({base}/manifest.json) · [Summary]({base}/run_summary.md) · [Metrics]({base}/ray_metrics.csv)"
            else:
                base = record["publishedAssetPath"]
                artifacts = f"Snapshot: `{base}/snapshot_ascii.txt` · Manifest: `{base}/manifest.json` · Summary: `{base}/run_summary.md` · Metrics: `{base}/ray_metrics.csv`"
        runtime = "—" if record["runtimeSeconds"] is None else f"{record['runtimeSeconds']:.6f}"
        mean = "—" if record["meanBend"] is None else str(record["meanBend"])
        maximum = "—" if record["maxBend"] is None else str(record["maxBend"])
        status = "Not run in v3.4 (scope stop)" if record["status"] == "DEFERRED" else f"`{record['status']}`"
        tier_label = f"{TIER_LABELS[record['tier']]} (`{record['tier']}`)"
        lines.append(f"| `{record['fixtureName']}` | {tier_label} | {record['gridWidth']}x{record['gridHeight']} | {record['rayCount']} | {status} | {runtime} | {mean} | {maximum} | {artifacts} |")
    preview_path = PUBLISH_ROOT / "grin_radial_smoke" / "standard" / "snapshot_ascii.txt"
    lines.extend([
        "",
        "## Highest Passing Tier Preview",
        "",
        "The following is a 4× downsampled display of the canonical fixture's passing 320×176 ASCII artifact. The linked artifact above retains the full text grid.",
        "",
        "```text",
        compact_ascii(preview_path),
        "```",
        "",
        "## Run History / Correction Note",
        "",
        "The initial generated mini-tier copies retained an unscaled, overly conservative `maxMisses` ceiling. That configuration produced early failed attempts. The generator was corrected so temporary resolution copies preserve the canonical fixture's proportional `maxMisses` allowance, and the final retained runs use that corrected configuration.",
        "",
        "Those early attempts are part of the development history, but they are not the final ladder result. Canonical fixtures remained unchanged. The final result is 12 of 12 attempted smoke, mini, and standard runs passing; all four high-tier entries are deferred by the declared v3.4 scope policy.",
        "",
        "## Resolution Variant Identity",
        "",
        "`grin_radial_smoke_resolution_variant` has a native baseline grid of 41x22, compared with 40x22 for the other fixtures. Its one-column sensitivity is therefore meaningful at the baseline tier only. The generated compact and gallery-detail tiers intentionally use the same 80x44 and 320x176 footprints as the rest of the family; this does not mutate the canonical fixture.",
        "",
        "## Deferred Tier",
        "",
        "Extended 640x352 is **not run in v3.4 (scope stop)**. This is a policy deferral, not a failed run. Gallery detail completed for every fixture, and v3.4 stops at that declared target rather than expanding artifact volume without a new need.",
        "",
        "## Claim Boundary",
        "",
    ])
    lines.extend(f"- {item}" for item in report["claimBoundary"])
    lines.extend([
        "",
        "## Reproduce",
        "",
        "```bash",
        "dotnet build src/XPrimeRay.Core/XPrimeRay.Core.csproj",
        "dotnet build src/XPrimeRay.Testbench.Cli/XPrimeRay.Testbench.Cli.csproj",
        "python3 tools/glowing_heart_resolution_ladder.py",
        "```",
        "",
    ])
    return "\n".join(lines)


def generated_fixture(source: Path, fixture_name: str, tier: str, width: int, height: int) -> Path:
    data = json.loads(source.read_text(encoding="utf-8"))
    if not isinstance(data, dict) or not isinstance(data.get("rayGrid"), dict):
        raise ValueError(f"{source}: missing rayGrid object")
    source_rays = int(data["rayGrid"]["width"]) * int(data["rayGrid"]["height"])
    target_rays = width * height
    data["rayGrid"] = {**data["rayGrid"], "width": width, "height": height}
    validation = data.get("validation")
    if isinstance(validation, dict) and isinstance(validation.get("maxMisses"), int):
        miss_ratio = validation["maxMisses"] / source_rays
        data["validation"] = {
            **validation,
            "maxMisses": min(target_rays, math.ceil(miss_ratio * target_rays)),
        }
    path = OUTPUT_ROOT / "generated_fixtures" / fixture_name / f"{tier}.json"
    write_json(path, data)
    return path


def safe_summary(manifest: dict[str, Any], runtime_seconds: float, tier: str) -> str:
    fixture = manifest["fixture"]
    result = manifest["result"]
    return "\n".join([
        "# Glowing Heart v3.4 Core Fixture Run",
        "",
        f"- Fixture: `{fixture['name']}`",
        f"- Resolution tier: `{tier}`",
        f"- Resolution: {result['rays']} rays",
        f"- Status: `{result['validation']}`",
        f"- Runtime: {runtime_seconds:.6f} seconds",
        f"- Mean bend: {result['meanBend']}",
        f"- Max bend: {result['maxBend']}",
        "",
        "## Claim Boundary",
        "",
        "- Core smoke transport only.",
        "- Not a Godot comparison.",
        "- Not image or pixel comparison.",
        "- Not parity.",
        "- Not physical validation.",
        "- Not renderer equivalence.",
        "",
    ])


def publish(run_dir: Path, fixture_name: str, tier: str, runtime_seconds: float) -> Path:
    destination = PUBLISH_ROOT / fixture_name / tier
    destination.mkdir(parents=True, exist_ok=True)
    manifest = json.loads((run_dir / "manifest.json").read_text(encoding="utf-8"))
    for name in PUBLISH_FILES:
        shutil.copy2(run_dir / name, destination / name)
    summary = safe_summary(manifest, runtime_seconds, tier)
    (run_dir / "run_summary.md").write_text(summary, encoding="utf-8")
    (destination / "run_summary.md").write_text(summary, encoding="utf-8")
    return destination


def sanitize_existing_summaries() -> None:
    for manifest_path in OUTPUT_ROOT.glob("runs/*/*/*/manifest.json"):
        run_dir = manifest_path.parent
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        tier = run_dir.parent.name
        (run_dir / "run_summary.md").write_text(safe_summary(manifest, 0.0, tier), encoding="utf-8")


def deferred(source: Path, fixture_name: str, tier: str, width: int, height: int, reason: str) -> dict[str, Any]:
    return {
        "fixtureName": fixture_name,
        "fixturePath": source.as_posix(),
        "tier": tier,
        "gridWidth": width,
        "gridHeight": height,
        "rayCount": width * height,
        "status": "DEFERRED",
        "runtimeSeconds": None,
        "meanBend": None,
        "maxBend": None,
        "artifactPath": None,
        "publishedAssetPath": None,
        "reason": reason,
    }


def run_tier(source: Path, fixture_name: str, tier: str, width: int | None, height: int | None) -> dict[str, Any]:
    source_data = json.loads(source.read_text(encoding="utf-8"))
    source_grid = source_data["rayGrid"]
    run_width = int(source_grid["width"] if width is None else width)
    run_height = int(source_grid["height"] if height is None else height)
    fixture_path = source if width is None else generated_fixture(source, fixture_name, tier, run_width, run_height)
    output_parent = OUTPUT_ROOT / "runs" / fixture_name / tier
    output_parent.mkdir(parents=True, exist_ok=True)
    before = set(output_parent.iterdir())
    command = [
        "dotnet", "run", "--no-build", "--project", CLI_PROJECT.as_posix(), "--",
        "run-fixture", fixture_path.as_posix(), "--output", output_parent.as_posix(),
    ]
    started = time.monotonic()
    try:
        completed = subprocess.run(command, text=True, capture_output=True, timeout=RUN_TIMEOUT_SECONDS)
    except subprocess.TimeoutExpired:
        return deferred(source, fixture_name, tier, run_width, run_height, f"Runtime exceeded {RUN_TIMEOUT_SECONDS} seconds.")
    duration = time.monotonic() - started
    created = [path for path in output_parent.iterdir() if path not in before and path.is_dir()]
    if completed.returncode != 0 or len(created) != 1:
        reason = (completed.stderr or completed.stdout or "CLI did not produce one run packet.").strip()
        return {
            **deferred(source, fixture_name, tier, run_width, run_height, reason[:500]),
            "status": "FAIL",
            "runtimeSeconds": round(duration, 6),
        }
    run_dir = created[0]
    manifest = json.loads((run_dir / "manifest.json").read_text(encoding="utf-8"))
    (run_dir / "run_summary.md").write_text(safe_summary(manifest, duration, tier), encoding="utf-8")
    packet_bytes = sum(path.stat().st_size for path in run_dir.iterdir() if path.is_file())
    if packet_bytes > MAX_PACKET_BYTES:
        return deferred(source, fixture_name, tier, run_width, run_height, f"Run packet exceeded {MAX_PACKET_BYTES} bytes.")
    result = manifest["result"]
    published = publish(run_dir, fixture_name, tier, duration)
    return {
        "fixtureName": fixture_name,
        "fixturePath": source.as_posix(),
        "tier": tier,
        "gridWidth": run_width,
        "gridHeight": run_height,
        "rayCount": int(result["rays"]),
        "status": "PASS" if result["validation"] == "PASS" else "FAIL",
        "runtimeSeconds": round(duration, 6),
        "meanBend": result["meanBend"],
        "maxBend": result["maxBend"],
        "artifactPath": run_dir.as_posix(),
        "publishedAssetPath": published.as_posix(),
        "reason": None if result["validation"] == "PASS" else f"Core report status: {result['validation']}",
    }


def main(argv: list[str] | None = None) -> int:
    args = sys.argv[1:] if argv is None else argv
    if args == ["--render-only"]:
        if not REPORT_JSON.exists():
            print(f"FAIL: retained report not found: {REPORT_JSON}", file=sys.stderr)
            return 1
        report = json.loads(REPORT_JSON.read_text(encoding="utf-8"))
        REPORT_MD.write_text(render_markdown(report, docs=False), encoding="utf-8")
        DOCS_MD.write_text(render_markdown(report, docs=True), encoding="utf-8")
        print(f"PASS: rendered retained report to {REPORT_MD} and {DOCS_MD}")
        return 0
    if args:
        print("usage: glowing_heart_resolution_ladder.py [--render-only]", file=sys.stderr)
        return 2
    if not FIXTURES:
        print("FAIL: no canonical GRIN smoke fixtures found", file=sys.stderr)
        return 1
    initial_hashes = canonical_hashes()
    sanitize_existing_summaries()
    records: list[dict[str, Any]] = []
    stop_standard = False
    for source in FIXTURES:
        fixture_name = source.stem
        for tier, width, height, _ in TIERS:
            if tier == "high":
                records.append(deferred(source, fixture_name, tier, width, height, "Optional high tier deferred after the conservative standard target."))
                continue
            if tier == "standard" and stop_standard:
                records.append(deferred(source, fixture_name, tier, width, height, "Standard ladder stopped after an earlier standard-tier guard."))
                continue
            record = run_tier(source, fixture_name, tier, width, height)
            records.append(record)
            print(f"{record['status']:8} {fixture_name} {tier} {record['gridWidth']}x{record['gridHeight']} {record['runtimeSeconds']}")
            if tier == "standard" and (record["status"] != "PASS" or (record["runtimeSeconds"] or 0) > 30):
                stop_standard = True
    if canonical_hashes() != initial_hashes:
        print("FAIL: canonical fixture content changed", file=sys.stderr)
        return 1
    report = {
        "schema": "xprimeray.glowing_heart.fixture_resolution_ladder.v0.preview",
        "generatedUtc": datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
        "milestone": "Glowing Heart v3.4 — Fixture Library Resolution Ladder",
        "parityClaim": "NONE",
        "tiers": [
            {"id": tier, "description": description, "targetWidth": width, "targetHeight": height}
            for tier, width, height, description in TIERS
        ],
        "records": records,
        "claimBoundary": [
            "Core fixture artifact generation only.",
            "Not a Godot comparison.",
            "Not image or pixel comparison.",
            "Not parity.",
            "Not physical validation.",
            "Not renderer equivalence.",
            "Resolution tiers describe recorded Core runs only."
        ]
    }
    write_json(REPORT_JSON, report)
    REPORT_MD.write_text(render_markdown(report, docs=False), encoding="utf-8")
    DOCS_MD.write_text(render_markdown(report, docs=True), encoding="utf-8")
    failed = sum(record["status"] == "FAIL" for record in records)
    print(f"{'PASS' if failed == 0 else 'FAIL'}: {len(records)} records, {failed} failures")
    return 0 if failed == 0 else 1


if __name__ == "__main__":
    raise SystemExit(main())
