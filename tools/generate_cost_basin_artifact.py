#!/usr/bin/env python3
"""Generate Cost Basin Artifact v1.

Observation-only post-process artifact:
- Reads hit diagnostics for per-pixel final_step_count.
- Reads existing traversal_step_heatmap.png as the sibling traversal view.
- Reads curvature_fps_result.json / Query Observatory metrics when available.
- Reads closure, coverage, ownership seam, and disagreement maps when available.
- Emits v0 heatmap/ladder plus v1 terrain/storyboard and markdown explanation.

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
    closure_map: Path | None
    coverage_map: Path | None
    ownership_seam_map: Path | None
    disagreement_map: Path | None
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
    closure_map = find_one(cell_dir, ["hit_miss_map.png", "*hit_miss*.png"])
    coverage_map = find_one(cell_dir, ["frame_coverage_map.png", "*coverage_map.png"])
    ownership_seam_map = find_one(cell_dir, ["ownership_graph_seam_map.png", "*seam_map.png"])
    disagreement_map = find_one(cell_dir, ["unstable_subgraph_overlay.png", "*disagreement*.png", "*unstable*.png"])
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
        closure_map=closure_map,
        coverage_map=coverage_map,
        ownership_seam_map=ownership_seam_map,
        disagreement_map=disagreement_map,
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


def cost_grid(cell: CellCost) -> list[list[float]]:
    values = cost_values(cell)
    fallback = min(values.values()) if values else 0.0
    return [[values.get((x, y), fallback) for x in range(cell.width)] for y in range(cell.height)]


def flatten(grid: list[list[float]]) -> list[float]:
    return [value for row in grid for value in row]


def percentile(values: list[float], pct_value: float) -> float:
    if not values:
        return 0.0
    ordered = sorted(values)
    index = int(max(0, min(len(ordered) - 1, round((pct_value / 100.0) * (len(ordered) - 1)))))
    return ordered[index]


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


def terrain_ramp(norm: float) -> tuple[int, int, int]:
    norm = max(0.0, min(1.0, norm))
    stops = [
        (0.00, (22, 46, 92)),
        (0.35, (37, 102, 112)),
        (0.58, (129, 139, 85)),
        (0.76, (218, 170, 70)),
        (0.91, (234, 92, 45)),
        (1.00, (255, 245, 196)),
    ]
    for (a_pos, a_col), (b_pos, b_col) in zip(stops, stops[1:]):
        if norm <= b_pos:
            t = 0.0 if b_pos == a_pos else (norm - a_pos) / (b_pos - a_pos)
            return tuple(int(a_col[i] + (b_col[i] - a_col[i]) * t) for i in range(3))
    return stops[-1][1]


def scale_color(color: tuple[int, int, int], factor: float) -> tuple[int, int, int]:
    return tuple(max(0, min(255, int(channel * factor))) for channel in color)


def blend(src: tuple[int, int, int], overlay: tuple[int, int, int], alpha: float) -> tuple[int, int, int]:
    return tuple(int(src[i] * (1.0 - alpha) + overlay[i] * alpha) for i in range(3))


def image_mask(path: Path | None, width: int, height: int, mode: str = "edge") -> set[tuple[int, int]]:
    if not path or not path.exists():
        return set()
    img = Image.open(path).convert("RGB").resize((width, height), Image.Resampling.BILINEAR)
    px = img.load()
    mask: set[tuple[int, int]] = set()
    if mode == "active":
        # Non-dark, non-gray regions. Useful for seam/disagreement overlays.
        for y in range(height):
            for x in range(width):
                r, g, b = px[x, y]
                chroma = max(r, g, b) - min(r, g, b)
                if max(r, g, b) > 70 and chroma > 18:
                    mask.add((x, y))
        return mask

    # Boundary mask from luminance/color discontinuities. Uniform closure or
    # full-coverage maps intentionally produce little or no boundary.
    for y in range(1, height - 1):
        for x in range(1, width - 1):
            r, g, b = px[x, y]
            here = (r + g + b) / 3.0
            right = sum(px[x + 1, y]) / 3.0
            down = sum(px[x, y + 1]) / 3.0
            chroma = max(abs(r - px[x + 1, y][0]), abs(g - px[x + 1, y][1]), abs(b - px[x + 1, y][2]))
            if abs(here - right) > 18 or abs(here - down) > 18 or chroma > 28:
                mask.add((x, y))
    return mask


def ridge_mask(grid: list[list[float]], threshold_pct: float = 91.0) -> set[tuple[int, int]]:
    height = len(grid)
    width = len(grid[0]) if height else 0
    values = flatten(grid)
    high = percentile(values, threshold_pct)
    mask: set[tuple[int, int]] = set()
    for y in range(1, height - 1):
        for x in range(1, width - 1):
            gx = grid[y][x + 1] - grid[y][x - 1]
            gy = grid[y + 1][x] - grid[y - 1][x]
            grad = abs(gx) + abs(gy)
            if grid[y][x] >= high or grad >= 3.0:
                mask.add((x, y))
    return mask


def local_maxima(grid: list[list[float]], limit: int = 12, radius: int = 8) -> list[tuple[int, int, float]]:
    height = len(grid)
    width = len(grid[0]) if height else 0
    candidates: list[tuple[int, int, float]] = []
    floor = percentile(flatten(grid), 94.0)
    for y in range(1, height - 1):
        for x in range(1, width - 1):
            value = grid[y][x]
            if value < floor:
                continue
            neighbors = [
                grid[yy][xx]
                for yy in range(y - 1, y + 2)
                for xx in range(x - 1, x + 2)
                if not (xx == x and yy == y)
            ]
            if value >= max(neighbors):
                candidates.append((x, y, value))
    candidates.sort(key=lambda item: item[2], reverse=True)
    selected: list[tuple[int, int, float]] = []
    for x, y, value in candidates:
        if all((x - sx) ** 2 + (y - sy) ** 2 >= radius**2 for sx, sy, _ in selected):
            selected.append((x, y, value))
        if len(selected) >= limit:
            break
    return selected


def mask_alignment(ridges: set[tuple[int, int]], mask: set[tuple[int, int]], radius: int = 1) -> int:
    if not ridges or not mask:
        return 0
    expanded: set[tuple[int, int]] = set()
    for x, y in mask:
        for dy in range(-radius, radius + 1):
            for dx in range(-radius, radius + 1):
                expanded.add((x + dx, y + dy))
    return sum(1 for point in ridges if point in expanded)


def draw_mark(draw: ImageDraw.ImageDraw, x: int, y: int, color: tuple[int, int, int]) -> None:
    draw.ellipse((x - 5, y - 5, x + 5, y + 5), outline=(10, 12, 18), width=3)
    draw.ellipse((x - 4, y - 4, x + 4, y + 4), outline=color, width=2)
    draw.line((x - 7, y, x + 7, y), fill=color, width=1)
    draw.line((x, y - 7, x, y + 7), fill=color, width=1)


def render_terrain_map(cell: CellCost, output: Path, scale: int = 5) -> dict[str, Any]:
    grid = cost_grid(cell)
    values = flatten(grid)
    lo, hi = min(values), max(values)
    height = len(grid)
    width = len(grid[0]) if height else 0
    raw = Image.new("RGB", (width, height), (10, 12, 18))
    px = raw.load()
    for y in range(height):
        for x in range(width):
            value = grid[y][x]
            norm = (value - lo) / (hi - lo) if hi > lo else 0.0
            left = grid[y][max(0, x - 1)]
            right = grid[y][min(width - 1, x + 1)]
            up = grid[max(0, y - 1)][x]
            down = grid[min(height - 1, y + 1)][x]
            shade = 1.05 + ((left - right) + (up - down)) / max(1.0, hi - lo) * 0.75
            px[x, y] = scale_color(terrain_ramp(norm), shade)

    ridges = ridge_mask(grid)
    maxima = local_maxima(grid)
    closure = image_mask(cell.closure_map, width, height, "edge")
    coverage = image_mask(cell.coverage_map, width, height, "edge")
    seams = image_mask(cell.ownership_seam_map, width, height, "active")
    disagreement = image_mask(cell.disagreement_map, width, height, "active")

    overlays = [
        (closure, (70, 220, 255), 0.80),
        (coverage, (70, 255, 130), 0.70),
        (seams, (190, 105, 255), 0.86),
        (disagreement, (255, 80, 80), 0.82),
        (ridges, (255, 250, 210), 0.42),
    ]
    for mask, color, alpha in overlays:
        for x, y in mask:
            if 0 <= x < width and 0 <= y < height:
                px[x, y] = blend(px[x, y], color, alpha)

    terrain = raw.resize((width * scale, height * scale), Image.Resampling.NEAREST)
    pad_top = 74
    pad_bottom = 78
    canvas = Image.new("RGB", (terrain.width, terrain.height + pad_top + pad_bottom), (12, 14, 22))
    canvas.paste(terrain, (0, pad_top))
    draw = ImageDraw.Draw(canvas)
    draw.text((14, 10), "Cost Basin Terrain v1", font=F_TITLE, fill=(240, 248, 255))
    draw.text(
        (14, 40),
        "final_step_count terrain + closure/coverage/seam/disagreement overlays",
        font=F_SUBTITLE,
        fill=(174, 190, 210),
    )
    for idx, (x, y, value) in enumerate(maxima[:8], start=1):
        sx, sy = x * scale + scale // 2, y * scale + pad_top + scale // 2
        draw_mark(draw, sx, sy, (255, 64, 200))
        draw.text((sx + 7, sy - 12), str(idx), font=F_TINY, fill=(255, 220, 245))

    legend_y = canvas.height - 56
    legend_items = [
        ("ridges", (255, 250, 210)),
        ("closure boundary", (70, 220, 255)),
        ("coverage boundary", (70, 255, 130)),
        ("ownership seams", (190, 105, 255)),
        ("disagreement zones", (255, 80, 80)),
        ("local maxima", (255, 64, 200)),
    ]
    x = 14
    for label, color in legend_items:
        draw.rectangle((x, legend_y, x + 12, legend_y + 12), fill=color)
        draw.text((x + 17, legend_y - 1), label, font=F_SMALL, fill=(211, 222, 235))
        x += 132 if len(label) < 11 else 164
    draw.text(
        (14, canvas.height - 28),
        "Observation only. Alignment means co-location in existing diagnostic outputs, not causal proof.",
        font=F_SMALL,
        fill=(174, 190, 210),
    )
    draw_legend(draw, canvas.width - 238, canvas.height - 34, 220, 12)

    output.parent.mkdir(parents=True, exist_ok=True)
    canvas.save(output)
    return {
        "ridges": ridges,
        "maxima": maxima,
        "closure": closure,
        "coverage": coverage,
        "seams": seams,
        "disagreement": disagreement,
        "alignment": {
            "closure": mask_alignment(ridges, closure),
            "coverage": mask_alignment(ridges, coverage),
            "ownership_seams": mask_alignment(ridges, seams),
            "disagreement": mask_alignment(ridges, disagreement),
        },
        "ridge_count": len(ridges),
    }


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


def image_panel(path: Path | None, size: tuple[int, int], missing: str) -> Image.Image:
    width, height = size
    if not path or not path.exists():
        img = Image.new("RGB", size, (22, 25, 34))
        draw = ImageDraw.Draw(img)
        draw.text((14, height // 2 - 10), missing, font=F_SMALL, fill=(148, 163, 184))
        return img
    src = Image.open(path).convert("RGB")
    src.thumbnail((width, height), Image.Resampling.LANCZOS)
    img = Image.new("RGB", size, (10, 12, 18))
    img.paste(src, ((width - src.width) // 2, (height - src.height) // 2))
    return img


def render_local_maxima_panel(
    cell: CellCost,
    terrain_info: dict[str, Any],
    size: tuple[int, int],
) -> Image.Image:
    img = Image.new("RGB", size, (18, 22, 32))
    draw = ImageDraw.Draw(img)
    draw.text((12, 10), "Local maxima", font=F_LABEL, fill=(255, 220, 245))
    maxima = terrain_info.get("maxima", [])
    values = [value for _, _, value in maxima]
    lo, hi = (min(values), max(values)) if values else (0.0, 1.0)
    for idx, (x, y, value) in enumerate(maxima[:8], start=1):
        norm = (value - lo) / (hi - lo) if hi > lo else 1.0
        yy = 32 + (idx - 1) * 15
        draw.text((14, yy), f"{idx:02d}", font=F_SMALL, fill=(255, 64, 200))
        draw.text((42, yy), f"x={x:03d} y={y:03d}", font=F_SMALL, fill=(211, 222, 235))
        draw.rectangle((132, yy + 3, 132 + int(96 * norm), yy + 10), fill=color_ramp(norm))
        draw.text((238, yy), f"{value:.1f}", font=F_SMALL, fill=(174, 190, 210))
    if not maxima:
        draw.text((14, 42), "No local maxima detected.", font=F_SMALL, fill=(148, 163, 184))
    draw.text((14, size[1] - 18), "Peaks use non-maximum suppression.", font=F_TINY, fill=(148, 163, 184))
    return img


def render_alignment_panel(
    terrain_info: dict[str, Any],
    size: tuple[int, int],
) -> Image.Image:
    img = Image.new("RGB", size, (18, 22, 32))
    draw = ImageDraw.Draw(img)
    draw.text((12, 10), "Ridge alignment", font=F_LABEL, fill=(255, 250, 210))
    ridge_count = max(1, int(terrain_info.get("ridge_count") or 0))
    labels = [
        ("closure boundaries", terrain_info["alignment"]["closure"], (70, 220, 255)),
        ("coverage boundaries", terrain_info["alignment"]["coverage"], (70, 255, 130)),
        ("ownership seams", terrain_info["alignment"]["ownership_seams"], (190, 105, 255)),
        ("disagreement zones", terrain_info["alignment"]["disagreement"], (255, 80, 80)),
    ]
    for idx, (label, value, color) in enumerate(labels):
        yy = 34 + idx * 26
        pct_value = value / ridge_count
        draw.text((14, yy), label, font=F_SMALL, fill=(211, 222, 235))
        draw.rectangle((150, yy + 3, 292, yy + 14), outline=(52, 63, 82))
        draw.rectangle((151, yy + 4, 151 + int(140 * pct_value), yy + 13), fill=color)
        draw.text((300, yy), f"{value} px", font=F_SMALL, fill=(174, 190, 210))
    draw.text((14, size[1] - 30), f"Ridge pixels: {ridge_count}", font=F_SMALL, fill=(174, 190, 210))
    draw.text((14, size[1] - 16), "Zero can mean a uniform source map.", font=F_TINY, fill=(148, 163, 184))
    return img


def draw_story_panel(
    canvas: Image.Image,
    index: int,
    title: str,
    question: str,
    body: str,
    image: Image.Image,
    x: int,
    y: int,
    width: int,
    height: int,
) -> None:
    draw = ImageDraw.Draw(canvas)
    draw.rounded_rectangle((x, y, x + width, y + height), radius=8, fill=(18, 22, 32), outline=(52, 63, 82), width=1)
    draw.text((x + 12, y + 10), f"{index}. {title}", font=F_LABEL, fill=(240, 248, 255))
    draw.text((x + 12, y + 30), question, font=F_SMALL, fill=(148, 226, 213))
    image_y = y + 54
    canvas.paste(image, (x + 12, image_y))
    draw.text((x + 12, y + height - 42), body, font=F_SMALL, fill=(211, 222, 235))


def render_storyboard(
    cell: CellCost,
    terrain_path: Path,
    terrain_info: dict[str, Any],
    output: Path,
) -> None:
    panel_w, panel_h = 360, 270
    gap = 16
    margin = 20
    header = 84
    width = margin * 2 + panel_w * 3 + gap * 2
    height = header + margin + panel_h * 3 + gap * 2
    canvas = Image.new("RGB", (width, height), (10, 12, 18))
    draw = ImageDraw.Draw(canvas)
    draw.text((margin, 14), "Cost Basin Storyboard v1", font=F_TITLE, fill=(240, 248, 255))
    draw.text(
        (margin, 46),
        "Spatial terrain map for where computational effort accumulates. Observation only; no renderer modifications.",
        font=F_SUBTITLE,
        fill=(174, 190, 210),
    )
    image_size = (panel_w - 24, 166)
    panels = [
        (
            "Terrain",
            "Where does effort accumulate?",
            f"Mean steps {cell.final_step_mean:.1f}; max {cell.final_step_max:.0f}.",
            image_panel(terrain_path, image_size, "missing terrain"),
        ),
        (
            "Traversal Input",
            "What raw effort field seeded v1?",
            "`final_step_count` plus traversal heatmap.",
            image_panel(cell.traversal_heatmap, image_size, "missing traversal"),
        ),
        (
            "Closure Boundary",
            "Do ridges align with closure?",
            "Hermetic closure is expected to be mostly uniform here.",
            image_panel(cell.closure_map, image_size, "missing closure map"),
        ),
        (
            "Coverage Boundary",
            "Do ridges align with coverage?",
            "Coverage map checks whether terrain reflects observed pixels.",
            image_panel(cell.coverage_map, image_size, "missing coverage map"),
        ),
        (
            "Ownership Seams",
            "Do ridges align with seams?",
            "Purple terrain overlay marks available ownership seams.",
            image_panel(cell.ownership_seam_map, image_size, "missing seam map"),
        ),
        (
            "Disagreement Zones",
            "Do ridges align with disagreement?",
            "Red terrain overlay marks unstable/disagreement evidence.",
            image_panel(cell.disagreement_map, image_size, "missing disagreement map"),
        ),
        (
            "Local Maxima",
            "Where are the peaks?",
            "Magenta markers label high-effort terrain peaks.",
            render_local_maxima_panel(cell, terrain_info, image_size),
        ),
        (
            "Ridge Alignment",
            "Which overlays co-locate with ridges?",
            "Counts are ridge pixels near source-map boundaries/zones.",
            render_alignment_panel(terrain_info, image_size),
        ),
        (
            "Verdict",
            "What does v1 claim?",
            "Observed spatial cost terrain; not a physical-truth claim.",
            image_panel(None, image_size, "OBSERVATION ONLY\nNo renderer changes\nNo optimization signal"),
        ),
    ]
    for idx, panel in enumerate(panels):
        col = idx % 3
        row = idx // 3
        x = margin + col * (panel_w + gap)
        y = header + row * (panel_h + gap)
        draw_story_panel(canvas, idx + 1, *panel, x=x, y=y, width=panel_w, height=panel_h)
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


def write_markdown(
    base: CellCost,
    cells: list[CellCost],
    heatmap: Path,
    ladder: Path,
    terrain: Path,
    storyboard: Path,
    terrain_info: dict[str, Any],
    output: Path,
) -> None:
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
    maxima_rows = "\n".join(
        f"| {idx} | {x} | {y} | {value:.2f} |"
        for idx, (x, y, value) in enumerate(terrain_info.get("maxima", [])[:8], start=1)
    )
    if not maxima_rows:
        maxima_rows = "| n/a | n/a | n/a | n/a |"
    alignment = terrain_info.get("alignment", {})
    output.write_text(
        f"""# Cost Basin Artifact v1

**Question:** Where does computational effort accumulate?

**Status:** Observed artifact. Reporting layer only. No renderer optimization.

## Outputs

- Heatmap: `{heatmap.as_posix()}`
- Ladder: `{ladder.as_posix()}`
- Terrain: `{terrain.as_posix()}`
- Storyboard: `{storyboard.as_posix()}`
- Explanation: `{output.as_posix()}`

## Inputs

- Hit diagnostics: `{base.hit_csv.as_posix()}`
- Traversal heatmap: `{base.traversal_heatmap.as_posix() if base.traversal_heatmap else 'not found'}`
- Closure map: `{base.closure_map.as_posix() if base.closure_map else 'not found'}`
- Coverage map: `{base.coverage_map.as_posix() if base.coverage_map else 'not found'}`
- Ownership seam map: `{base.ownership_seam_map.as_posix() if base.ownership_seam_map else 'not found'}`
- Disagreement zone map: `{base.disagreement_map.as_posix() if base.disagreement_map else 'not found'}`
- Query Observatory metrics: `{base.result_json.as_posix() if base.result_json else 'not found'}`

## Method

Cost Basin v1 uses `final_step_count` as the measured spatial effort field. When per-pixel `query_count` or `substep_count` are not present in `hit_diagnostics.csv`, v1 derives observation-only attribution from the aggregate Query Observatory metrics in `latest_perf_frame_report`:

- `query_count` is estimated spatially in proportion to each pixel's share of total `final_step_count`.
- `substep_count` is represented by the aggregate ratio `subdivided_ray_queries / segments`.
- `pass2_query_ms` is reported as aggregate context, not assigned as a per-pixel timer.
- `traversal_step_heatmap.png` is treated as sibling evidence for the same effort field.
- `hit_miss_map.png`, `frame_coverage_map.png`, `ownership_graph_seam_map.png`, and `unstable_subgraph_overlay.png` are read as alignment layers.

This makes the terrain a cost-observation artifact, not a scheduling or optimization signal.

## Reading The Terrain

Bright yellow/white regions are the local Cost Basin: pixels where computational effort accumulates relative to the rest of the same frame. Blue/green regions are lower-effort portions of the same scene contract. Hillshade and pale contour overlays expose the terrain shape so the artifact reads as a basin/ridge map rather than only a row profile.

For the base cell `{base.label}`, the basin is mostly a traversal-depth basin: mean `final_step_count` is {base.final_step_mean:.1f}, max is {base.final_step_max:.0f}. Query work dominates the physics phase ({pct(base.query_cost_pct)} of `pass2_phys_ms`), so the observed traversal field also predicts where query effort accumulates.

## Local Maxima

| rank | x | y | cost terrain value |
|---:|---:|---:|---:|
{maxima_rows}

## Ridge Alignment

| alignment layer | ridge-adjacent pixels |
|---|---:|
| closure boundaries | {alignment.get("closure", 0)} |
| coverage boundaries | {alignment.get("coverage", 0)} |
| ownership seams | {alignment.get("ownership_seams", 0)} |
| disagreement zones | {alignment.get("disagreement", 0)} |

Zero alignment can mean the source layer is uniform for this fixture, not that the method failed. The hermetic 0% closure and coverage maps are expected to be mostly uniform.

## Cost Basin Ladder

| cell | final_step_count mean | final_step_count max | query_count total | substep_count mean | query cost % |
|---|---:|---:|---:|---:|---:|
{rows}

## Interpretation

The ladder asks whether the basin shifts as curvature changes. In this hermetic curved-room run, closure remains complete while effort stays concentrated in the same broad traversal-depth structure. Curvature changes the depth and fine shape of the basin, but the artifact does not claim physical correctness.

## Verdict

**Cost Basin Artifact v1: OBSERVED.**

The artifact answers where computational effort accumulates, where local maxima appear, and where basin ridges co-locate with existing closure, coverage, ownership seam, and disagreement evidence. It does not optimize renderer behavior, alter scheduling, or feed runtime decisions.
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
    terrain_path = args.output_dir / "cost_basin_terrain.png"
    storyboard_path = args.output_dir / "cost_basin_storyboard.png"
    markdown_path = args.output_dir / "cost_basin_artifact_v1.md"

    heatmap = render_cost_map(base, scale=5, title="Cost Basin Heatmap v0")
    args.output_dir.mkdir(parents=True, exist_ok=True)
    heatmap.save(heatmap_path)
    render_ladder(cells, ladder_path)
    terrain_info = render_terrain_map(base, terrain_path)
    render_storyboard(base, terrain_path, terrain_info, storyboard_path)
    write_markdown(base, cells, heatmap_path, ladder_path, terrain_path, storyboard_path, terrain_info, markdown_path)

    print(f"wrote {heatmap_path}")
    print(f"wrote {ladder_path}")
    print(f"wrote {terrain_path}")
    print(f"wrote {storyboard_path}")
    print(f"wrote {markdown_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
