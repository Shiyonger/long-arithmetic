#!/usr/bin/env python3

import csv
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
DATA_DIR = ROOT / "load-test-artifacts" / "chart-data"
OUTPUT_DIR = ROOT / "load-test-artifacts" / "charts"


def load_main_results():
    with (DATA_DIR / "main-results.csv").open(newline="", encoding="utf-8") as file:
        return list(csv.DictReader(file))


def load_stage_results():
    with (DATA_DIR / "stage-profiling.csv").open(newline="", encoding="utf-8") as file:
        return list(csv.DictReader(file))


def save_svg(path: Path, content: str):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content, encoding="utf-8")


def escape(text: str) -> str:
    return (
        text.replace("&", "&amp;")
        .replace("<", "&lt;")
        .replace(">", "&gt;")
        .replace('"', "&quot;")
    )


def chart_header(width: int, height: int, title: str) -> list[str]:
    return [
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">',
        '<rect width="100%" height="100%" fill="#f8f5ef"/>',
        f'<text x="{width / 2}" y="34" text-anchor="middle" font-family="Georgia, serif" font-size="24" fill="#1f2933">{escape(title)}</text>',
    ]


def chart_footer() -> list[str]:
    return ["</svg>"]


def scale_y(value: float, max_value: float, chart_top: int, chart_height: int) -> float:
    usable = chart_height
    return chart_top + usable - (value / max_value) * usable


def solve_time_line_chart(rows):
    width, height = 980, 620
    left, right, top, bottom = 90, 40, 90, 80
    plot_width = width - left - right
    plot_height = height - top - bottom
    max_value = max(float(row["solve_ms"]) for row in rows) * 1.1
    batches = sorted({int(row["batch"]) for row in rows})
    x_positions = {
        batch: left + index * (plot_width / (len(batches) - 1))
        for index, batch in enumerate(batches)
    }
    palette = {
        ("classic", "dense"): "#b23a48",
        ("classic", "sparse"): "#e07a5f",
        ("karatsuba", "dense"): "#2a6f97",
        ("karatsuba", "sparse"): "#6c9a8b",
    }

    svg = chart_header(width, height, "Solve Time vs Batch Size")
    svg.extend(
        [
            f'<line x1="{left}" y1="{top}" x2="{left}" y2="{top + plot_height}" stroke="#334e68" stroke-width="2"/>',
            f'<line x1="{left}" y1="{top + plot_height}" x2="{left + plot_width}" y2="{top + plot_height}" stroke="#334e68" stroke-width="2"/>',
        ]
    )

    for tick in range(6):
        value = max_value * tick / 5
        y = scale_y(value, max_value, top, plot_height)
        svg.append(f'<line x1="{left}" y1="{y:.2f}" x2="{left + plot_width}" y2="{y:.2f}" stroke="#d9d2c5" stroke-width="1"/>')
        svg.append(f'<text x="{left - 12}" y="{y + 5:.2f}" text-anchor="end" font-family="Arial, sans-serif" font-size="12" fill="#334e68">{value / 1000:.1f}s</text>')

    for batch, x in x_positions.items():
        svg.append(f'<line x1="{x:.2f}" y1="{top + plot_height}" x2="{x:.2f}" y2="{top + plot_height + 6}" stroke="#334e68" stroke-width="2"/>')
        svg.append(f'<text x="{x:.2f}" y="{top + plot_height + 28}" text-anchor="middle" font-family="Arial, sans-serif" font-size="14" fill="#334e68">{batch}</text>')

    groups = {}
    for row in rows:
        key = (row["algorithm"], row["profile"])
        groups.setdefault(key, []).append(row)

    for key, group_rows in groups.items():
        group_rows.sort(key=lambda row: int(row["batch"]))
        points = []
        for row in group_rows:
            x = x_positions[int(row["batch"])]
            y = scale_y(float(row["solve_ms"]), max_value, top, plot_height)
            points.append((x, y))

        color = palette[key]
        polyline = " ".join(f"{x:.2f},{y:.2f}" for x, y in points)
        svg.append(f'<polyline fill="none" stroke="{color}" stroke-width="4" points="{polyline}"/>')
        for x, y in points:
            svg.append(f'<circle cx="{x:.2f}" cy="{y:.2f}" r="5" fill="{color}"/>')

    legend_x = width - 280
    legend_y = 110
    for index, key in enumerate(sorted(groups.keys())):
        y = legend_y + index * 26
        label = f"{key[0]} / {key[1]}"
        svg.append(f'<line x1="{legend_x}" y1="{y}" x2="{legend_x + 24}" y2="{y}" stroke="{palette[key]}" stroke-width="4"/>')
        svg.append(f'<text x="{legend_x + 34}" y="{y + 5}" font-family="Arial, sans-serif" font-size="14" fill="#1f2933">{escape(label)}</text>')

    svg.append(f'<text x="{width / 2}" y="{height - 24}" text-anchor="middle" font-family="Arial, sans-serif" font-size="16" fill="#334e68">Batch size</text>')
    svg.append(f'<text transform="translate(26 {height / 2}) rotate(-90)" text-anchor="middle" font-family="Arial, sans-serif" font-size="16" fill="#334e68">Solve time</text>')
    svg.extend(chart_footer())
    return "\n".join(svg)


def speedup_bar_chart(rows):
    width, height = 900, 520
    left, right, top, bottom = 80, 40, 90, 80
    plot_width = width - left - right
    plot_height = height - top - bottom

    buckets = {}
    for row in rows:
        key = (row["profile"], int(row["batch"]))
        buckets.setdefault(key, {})[row["algorithm"]] = float(row["solve_ms"])

    series = []
    for key in sorted(buckets.keys(), key=lambda item: (item[0], item[1])):
        classic = buckets[key]["classic"]
        karatsuba = buckets[key]["karatsuba"]
        series.append((key, classic / karatsuba))

    max_value = max(value for _, value in series) * 1.15
    bar_width = plot_width / (len(series) * 1.8)
    start_x = left + bar_width

    svg = chart_header(width, height, "Classic / Karatsuba Speedup")
    svg.extend(
        [
            f'<line x1="{left}" y1="{top}" x2="{left}" y2="{top + plot_height}" stroke="#334e68" stroke-width="2"/>',
            f'<line x1="{left}" y1="{top + plot_height}" x2="{left + plot_width}" y2="{top + plot_height}" stroke="#334e68" stroke-width="2"/>',
        ]
    )

    for tick in range(6):
        value = max_value * tick / 5
        y = scale_y(value, max_value, top, plot_height)
        svg.append(f'<line x1="{left}" y1="{y:.2f}" x2="{left + plot_width}" y2="{y:.2f}" stroke="#d9d2c5" stroke-width="1"/>')
        svg.append(f'<text x="{left - 12}" y="{y + 5:.2f}" text-anchor="end" font-family="Arial, sans-serif" font-size="12" fill="#334e68">{value:.1f}x</text>')

    for index, (key, value) in enumerate(series):
        x = start_x + index * bar_width * 1.8
        bar_height = (value / max_value) * plot_height
        y = top + plot_height - bar_height
        color = "#c08497" if key[0] == "dense" else "#84a59d"
        label = f"{key[0]} / batch {key[1]}"
        svg.append(f'<rect x="{x:.2f}" y="{y:.2f}" width="{bar_width:.2f}" height="{bar_height:.2f}" fill="{color}"/>')
        svg.append(f'<text x="{x + bar_width / 2:.2f}" y="{y - 8:.2f}" text-anchor="middle" font-family="Arial, sans-serif" font-size="13" fill="#1f2933">{value:.2f}x</text>')
        svg.append(f'<text x="{x + bar_width / 2:.2f}" y="{top + plot_height + 24}" text-anchor="middle" font-family="Arial, sans-serif" font-size="12" fill="#334e68">{escape(label)}</text>')

    svg.append(f'<text x="{width / 2}" y="{height - 24}" text-anchor="middle" font-family="Arial, sans-serif" font-size="16" fill="#334e68">Profile and batch size</text>')
    svg.append(f'<text transform="translate(26 {height / 2}) rotate(-90)" text-anchor="middle" font-family="Arial, sans-serif" font-size="16" fill="#334e68">How many times classic is slower</text>')
    svg.extend(chart_footer())
    return "\n".join(svg)


def stage_breakdown_chart(rows):
    width, height = 980, 560
    left, right, top, bottom = 110, 40, 90, 90
    plot_width = width - left - right
    plot_height = height - top - bottom
    stages = ["generation", "input_write", "load", "save", "solve"]
    pretty = {
        "generation": "generation",
        "input_write": "input write",
        "load": "load",
        "save": "save",
        "solve": "solve",
    }
    algorithms = ["classic", "karatsuba"]
    colors = {"classic": "#b23a48", "karatsuba": "#2a6f97"}

    values = {
        (row["algorithm"], row["stage"]): float(row["time_ms"])
        for row in rows
    }
    max_value = max(values.values()) * 1.1
    group_width = plot_width / len(stages)
    bar_width = group_width / 3

    svg = chart_header(width, height, "Stage Breakdown on Maximum Dataset")
    svg.extend(
        [
            f'<line x1="{left}" y1="{top}" x2="{left}" y2="{top + plot_height}" stroke="#334e68" stroke-width="2"/>',
            f'<line x1="{left}" y1="{top + plot_height}" x2="{left + plot_width}" y2="{top + plot_height}" stroke="#334e68" stroke-width="2"/>',
        ]
    )

    for tick in range(6):
        value = max_value * tick / 5
        y = scale_y(value, max_value, top, plot_height)
        svg.append(f'<line x1="{left}" y1="{y:.2f}" x2="{left + plot_width}" y2="{y:.2f}" stroke="#d9d2c5" stroke-width="1"/>')
        svg.append(f'<text x="{left - 12}" y="{y + 5:.2f}" text-anchor="end" font-family="Arial, sans-serif" font-size="12" fill="#334e68">{value / 1000:.1f}s</text>')

    for stage_index, stage in enumerate(stages):
        group_center = left + group_width * (stage_index + 0.5)
        for algo_index, algorithm in enumerate(algorithms):
            value = values[(algorithm, stage)]
            x = group_center + (algo_index - 0.5) * bar_width * 1.2 - bar_width / 2
            bar_height = (value / max_value) * plot_height
            y = top + plot_height - bar_height
            svg.append(f'<rect x="{x:.2f}" y="{y:.2f}" width="{bar_width:.2f}" height="{bar_height:.2f}" fill="{colors[algorithm]}"/>')

        svg.append(f'<text x="{group_center:.2f}" y="{top + plot_height + 28}" text-anchor="middle" font-family="Arial, sans-serif" font-size="13" fill="#334e68">{escape(pretty[stage])}</text>')

    legend_x = width - 220
    legend_y = 110
    for index, algorithm in enumerate(algorithms):
        y = legend_y + index * 28
        svg.append(f'<rect x="{legend_x}" y="{y - 12}" width="20" height="20" fill="{colors[algorithm]}"/>')
        svg.append(f'<text x="{legend_x + 30}" y="{y + 3}" font-family="Arial, sans-serif" font-size="14" fill="#1f2933">{escape(algorithm)}</text>')

    svg.append(f'<text x="{width / 2}" y="{height - 24}" text-anchor="middle" font-family="Arial, sans-serif" font-size="16" fill="#334e68">Measured stage</text>')
    svg.append(f'<text transform="translate(28 {height / 2}) rotate(-90)" text-anchor="middle" font-family="Arial, sans-serif" font-size="16" fill="#334e68">Stage time</text>')
    svg.extend(chart_footer())
    return "\n".join(svg)


def main():
    main_rows = load_main_results()
    stage_rows = load_stage_results()

    save_svg(OUTPUT_DIR / "solve-time-vs-batch.svg", solve_time_line_chart(main_rows))
    save_svg(OUTPUT_DIR / "classic-vs-karatsuba-speedup.svg", speedup_bar_chart(main_rows))
    save_svg(OUTPUT_DIR / "stage-breakdown-maximum-dataset.svg", stage_breakdown_chart(stage_rows))


if __name__ == "__main__":
    main()
