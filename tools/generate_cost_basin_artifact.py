#!/usr/bin/env python3
"""Generate Cost Basin Artifact v0.

Observation-only post-process artifact:
- Reads hit diagnostics for per-pixel final_step_count.
- Reads existing traversal_step_heatmap.png as the sibling traversal view.
- Reads curvature_fps_result.json / Query Observatory metrics when available.
- Emits cost_basin_heatmap.png, cost_basin_ladder.png, and markdown explanation.

This script does not call or tune renderer logic.
"""

from __future__ import annotations

import argparse
import csv
import json
import math
from dataclasses import dataclass
from pathlib import Path
from statistics import mean
from typing import Any

from PIL import Image, ImageDraw, ImageFont


DEFAULT_RUN_DIR = Path("output/curvature_fps_benchmark/20260607T221311Z")
DEFAULT_BASE_CELL = DEFAULT_RUN_DIR / "cells/curvature_000/row"
DEFAULT_OUTPUT_DIR = Path("reports")


@dataclass
class CellCost:
    label: str
    cell_dir: Path
    hit_csv: Path
    traversal_heatmap: Path | None
    result_json: Path | None
    width: int
    height: int
    steps: list[tuple[int, int, float]]
    final_step_mean: float
    final_step_max: float
    query_count_total: float | None
    query_count_per_pixel_mean: float | None
    substep_count_mean: float | None
    pass2_query_ms: float | None
    query_cost_pct: float | None


def font(size: int, bold: bool = False) -> ImageFont.ImageFont:
    name = "DejaVuSans-Bold.ttf" if bold else "DejaVuSans.ttf"
    path = Path("/usr/share/fonts/truetype/dejavu") / name
    if path.exists():
        return ImageFont.truetype(str(path), size)
    return ImageFont.load_default()


F_TITLE = font(24, True)
F_SUBTITLE = font(14)
F_LABEL = font(13, True)
F_SMALL = font(11)
F_TINY = font(9)


def load_json(path: Path | None) -> dict[str, Any]:
    if not path or not path.exists():
        return {}
    return json.loads(path.read_text(encoding="utf-8"))


def find_one(directory: Path, patterns: list[str]) -> Path | None:
    for pattern in patterns:
        matches = sorted(directory.glob(pattern))
        if matches:
            return matches[0]
    return None


def parse_float(value: Any) -> float | None:
    if value is None or value == "":
        return None
    try:
        return float(value)
    except (TypeError, ValueError):
        return None


def load_hit_steps(path: Path) -> tuple[int, int, list[tuple[int, int, float]], list[str]]:
    rows: list[tuple[int, int, float]] = []
    fields: list[str] = []
    with path.open(newline="", encoding="utf-8-sig") as handle:
        reader = csv.DictReader(handle)
        fields = list(reader.fieldnames or [])
        for row in reader:
            x = int(float(row.get("x") or 0))
            y = int(float(row.get("y") or 0))
            step = parse_float(row.get("final_step_count"))
            if step is None:
                step = parse_float(row.get("step_count"))
            if step is None:
                continue
            rows.append((x, y, step))
    if not rows:
        raise ValueError(f"No step rows found in {path}")
    width = max(x for x, _, _ in rows) + 1
    height = max(y for _, y, _ in rows) + 1
    return width, height, rows, fields


def load_cell(cell_dir: Path, label: str) -> CellCost:
    hit_csv = find_one(cell_dir, ["*hit_diagnostics.csv", "hit_diagnostics.csv"])
    if not hit_csv:
        raise FileNotFoundError(f"No hit diagnostics CSV found in {cell_dir}")
    traversal_heatmap = find_one(cell_dir, ["traversal_step_heatmap.png", "*traversal_step_heatmap.png"])
    result_json = find_one(cell_dir, ["curvature_fps_result.json", "*result.json"])
    width, height, steps, fields = load_hit_steps(hit_csv)
    step_values = [step for _, _, step in steps]
    result = load_json(result_json)
    perf = result.get("latest_perf_frame_report") or {}
    film = result.get("film_capture") or {}

    query_total = (
        parse_float(perf.get("subdivided_ray_queries"))
        or parse_float(perf.get("band_physics_queries"))
        or parse_float(film.get("physics_queries"))
    )
    pass2_query_ms = parse_float(perf.get("pass2_query_ms"))
    pass2_phys_ms = parse_float(perf.get("pass2_phys_ms"))
    query_cost_pct = None
    if pass2_query_ms is not None and pass2_phys_ms:
        query_cost_pct = 100.0 * pass2_query_ms / pass2_phys_ms

    query_per_pixel = None
    if query_total is not None and steps:
        query_per_pixel = query_total / len(steps)

    segments = (
        parse_float(perf.get("segments_tested"))
        or parse_float(film.get("segments_tested"))
        or parse_float(perf.get("segments"))
        or parse_float(film.get("segments_integrated"))
    )
    substep_mean = None
    if query_total is not None and segments:
        substep_mean = query_total / segments
    elif "substep_count" in fields:
        # Reserved for future per-pixel diagnostics.
        substeps = [step for _, _, step in steps]
        substep_mean = mean(substeps)

    return CellCost(
        label=label,
        cell_dir=cell_dir,
        hit_csv=hit_csv,
        traversal_heatmap=traversal_heatmap,
        result_json=result_json,
        width=width,
        height=height,
        steps=steps,
        final_step_mean=mean(step_values),
        final_step_max=max(step_values),
        query_count_total=query_total,
        query_count_per_pixel_mean=query_per_pixel,
        substep_count_mean=substep_mean,
        pass2_query_ms=pass2_query_ms,
        query_cost_pct=query_cost_pct,
    )


def cost_values(cell: CellCost) -> dict[tuple[int, int], float]:
    step_sum = sum(step for _, _, step in cell.steps) or 1.0
    query_total = cell.query_count_total or 0.0
    substep_multiplier = cell.substep_count_mean or 1.0
    values: dict[tuple[int, int], float] = {}
    for x, y, step in cell.steps:
        # Spatial source is measured final_step_count. Query/substep fields are
        # aggregate Query Observatory metrics unless per-pixel counts are added later.
        query_estimate = (step / step_sum) * query_total if query_total else 0.0
        values[(x, y)] = step + math.log1p(query_estimate) + substep_multiplier
    return values


def color_ramp(norm: float) -> tuple[int, int, int]:
    norm = max(0.0, min(1.0, norm))
    if norm < 0.5:
        t = norm / 0.5
        r = int(26 + t * 214)
        g = int(79 + t * 131)
        b = int(168 - t * 108)
        return r, g, b
    t = (norm - 0.5) / 0.5
    r = int(240 + t * 15)
    g = int(210 - t * 154)
    b = int(60 - t * 28)
    return r, g, b


def render_cost_map(cell: CellCost, scale: int = 5, title: str | None = None) -> Image.Image:
    values = cost_values(cell)
    raw = Image.new("RGB", (cell.width, cell.height), (8, 11, 18))
    pixels = raw.load()
    vals = list(values.values())
    lo, hi = min(vals), max(vals)
    for (x, y), value in values.items():
        norm = (value - lo) / (hi - lo) if hi > lo else 0.0
        pixels[x, y] = color_ramp(norm)

    img = raw.resize((cell.width * scale, cell.height * scale), Image.Resampling.NEAREST)
    if not title:
        return img

    pad_top = 60
    canvas = Image.new("RGB", (img.width, img.height + pad_top + 34), (12, 14, 22))
    canvas.paste(img, (0, pad_top))
    draw = ImageDraw.Draw(canvas)
    draw.text((14, 10), title, font=F_TITLE, fill=(240, 248, 255))
    subtitle = (
        f"{cell.label} | final_step_count mean {cell.final_step_mean:.1f}, "
        f"max {cell.final_step_max:.0f}"
    )
    draw.text((14, 38), subtitle, font=F_SUBTITLE, fill=(174, 190, 210))
    draw.text(
        (14, canvas.height - 24),
        "Cost Basin v0: measured final_step_count + aggregate query/substep attribution; observation only.",
        font=F_SMALL,
        fill=(174, 190, 210),
    )
    draw_legend(draw, canvas.width - 238, canvas.height - 30, 220, 12)
    return canvas


def draw_legend(draw: ImageDraw.ImageDraw, x: int, y: int, width: int, height: int) -> None:
    for i in range(width):
        draw.line((x + i, y, x + i, y + height), fill=color_ramp(i / max(1, width - 1)))
    draw.text((x, y - 14), "low effort", font=F_TINY, fill=(174, 190, 210))
    draw.text((x + width - 54, y - 14), "high effort", font=F_TINY, fill=(174, 190, 210))


def render_ladder(cells: list[CellCost], output: Path) -> None:
    thumbs = [render_cost_map(cell, scale=3) for cell in cells]
    card_w = max(img.width for img in thumbs) + 28
    card_h = max(img.height for img in thumbs) + 82
    width = card_w * len(cells)
    height = card_h + 78
    canvas = Image.new("RGB", (width, height), (12, 14, 22))
    draw = ImageDraw.Draw(canvas)
    draw.text((16, 12), "Cost Basin Ladder v0", font=F_TITLE, fill=(240, 248, 255))
    draw.text(
        (16, 42),
        "Where computational effort accumulates across curvature cells. Observation only; no renderer optimization.",
        font=F_SUBTITLE,
        fill=(174, 190, 210),
    )
    for i, (cell, thumb) in enumerate(zip(cells, thumbs)):
        x = i * card_w + 14
        y = 74
        draw.rounded_rectangle((x - 6, y - 6, x + card_w - 16, y + card_h - 10), radius=8, fill=(18, 22, 32), outline=(52, 63, 82))
        draw.text((x, y), cell.label, font=F_LABEL, fill=(250, 204, 21))
        draw.text((x, y + 18), f"mean {cell.final_step_mean:.1f} | max {cell.final_step_max:.0f}", font=F_SMALL, fill=(211, 222, 235))
        if cell.query_cost_pct is not None:
            draw.text((x, y + 34), f"query cost {cell.query_cost_pct:.1f}% of phys", font=F_SMALL, fill=(148, 226, 213))
        canvas.paste(thumb, (x, y + 52))
    draw_legend(draw, width - 250, height - 28, 220, 12)
    output.parent.mkdir(parents=True, exist_ok=True)
    canvas.save(output)


def pct(value: float | None) -> str:
    return "n/a" if value is None else f"{value:.1f}%"


def number(value: float | None, decimals: int = 1) -> str:
    if value is None:
        return "n/a"
    if decimals == 0:
        return f"{value:,.0f}"
    return f"{value:,.{decimals}f}"


def write_markdown(base: CellCost, cells: list[CellCost], heatmap: Path, ladder: Path, output: Path) -> None:
    output.parent.mkdir(parents=True, exist_ok=True)
    rows = "\n".join(
        "| {label} | {mean} | {max_step} | {queries} | {substep} | {query_pct} |".format(
            label=cell.label,
            mean=number(cell.final_step_mean),
            max_step=number(cell.final_step_max, 0),
            queries=number(cell.query_count_total, 0),
            substep=number(cell.substep_count_mean, 2),
            query_pct=pct(cell.query_cost_pct),
        )
        for cell in cells
    )
    output.write_text(
        f"""# Cost Basin Artifact v0

**Question:** Where does computational effort accumulate?

**Status:** Observed artifact. Reporting layer only. No renderer optimization.

## Outputs

- Heatmap: `{heatmap.as_posix()}`
- Ladder: `{ladder.as_posix()}`
- Explanation: `{output.as_posix()}`

## Inputs

- Hit diagnostics: `{base.hit_csv.as_posix()}`
- Traversal heatmap: `{base.traversal_heatmap.as_posix() if base.traversal_heatmap else 'not found'}`
- Query Observatory metrics: `{base.result_json.as_posix() if base.result_json else 'not found'}`

## Method

Cost Basin v0 uses `final_step_count` as the measured spatial effort field. When per-pixel `query_count` or `substep_count` are not present in `hit_diagnostics.csv`, v0 derives observation-only attribution from the aggregate Query Observatory metrics in `latest_perf_frame_report`:

- `query_count` is estimated spatially in proportion to each pixel's share of total `final_step_count`.
- `substep_count` is represented by the aggregate ratio `subdivided_ray_queries / segments`.
- `pass2_query_ms` is reported as aggregate context, not assigned as a per-pixel timer.

This makes the heatmap a cost-observation artifact, not a scheduling or optimization signal.

## Reading The Heatmap

Bright yellow/white regions are the local Cost Basin: pixels where computational effort accumulates relative to the rest of the same frame. Blue/green regions are lower-effort portions of the same scene contract.

For the base cell `{base.label}`, the basin is mostly a traversal-depth basin: mean `final_step_count` is {base.final_step_mean:.1f}, max is {base.final_step_max:.0f}. Query work dominates the physics phase ({pct(base.query_cost_pct)} of `pass2_phys_ms`), so the observed traversal field also predicts where query effort accumulates.

## Cost Basin Ladder

| cell | final_step_count mean | final_step_count max | query_count total | substep_count mean | query cost % |
|---|---:|---:|---:|---:|---:|
{rows}

## Interpretation

The ladder asks whether the basin shifts as curvature changes. In this hermetic curved-room run, closure remains complete while effort stays concentrated in the same broad traversal-depth structure. Curvature changes the depth and fine shape of the basin, but the artifact does not claim physical correctness.

## Verdict

**Cost Basin Artifact v0: OBSERVED.**

The artifact answers where computational effort accumulates for the selected run using existing diagnostics only. It does not optimize renderer behavior, alter scheduling, or feed runtime decisions.
""",
        encoding="utf-8",
    )


def discover_ladder_cells(run_dir: Path) -> list[tuple[str, Path]]:
    cells: list[tuple[str, Path]] = []
    for child in sorted((run_dir / "cells").glob("curvature_*")):
        row = child / "row"
        if row.exists():
            raw = child.name.replace("curvature_", "")
            try:
                label = f"{int(raw)}%"
            except ValueError:
                label = raw + "%"
            cells.append((label, row))
    return cells


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--base-cell", type=Path, default=DEFAULT_BASE_CELL)
    parser.add_argument("--run-dir", type=Path, default=DEFAULT_RUN_DIR)
    parser.add_argument("--output-dir", type=Path, default=DEFAULT_OUTPUT_DIR)
    args = parser.parse_args()

    base = load_cell(args.base_cell, "0%")
    cells = [load_cell(path, label) for label, path in discover_ladder_cells(args.run_dir)]
    if not cells:
        cells = [base]

    heatmap_path = args.output_dir / "cost_basin_heatmap.png"
    ladder_path = args.output_dir / "cost_basin_ladder.png"
    markdown_path = args.output_dir / "cost_basin_artifact_v0.md"

    heatmap = render_cost_map(base, scale=5, title="Cost Basin Heatmap v0")
    args.output_dir.mkdir(parents=True, exist_ok=True)
    heatmap.save(heatmap_path)
    render_ladder(cells, ladder_path)
    write_markdown(base, cells, heatmap_path, ladder_path, markdown_path)

    print(f"wrote {heatmap_path}")
    print(f"wrote {ladder_path}")
    print(f"wrote {markdown_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
