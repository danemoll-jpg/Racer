"""Summarize measured release-player trials; never substitute estimated times."""
import csv
import statistics
import sys
from pathlib import Path

root = Path(sys.argv[1])
rows = []
sources = []
for course in ("street", "forest"):
    source = Path(sys.argv[2]) if course == "street" and len(sys.argv)>2 else root / f"routes-{course}" / "times.csv"
    sources.append(str(source))
    with source.open(newline="") as file:
        rows.extend(csv.DictReader(file))
lines = [
    "| Course / shortcut | Vehicle | Normal | Clean | Imperfect | Local recovery | Clean gain |",
    "|---|---|---:|---:|---:|---:|---:|",
]
failures = []
for key in dict.fromkeys((r["course"], r["route"], r["vehicle"]) for r in rows):
    group = [r for r in rows if (r["course"], r["route"], r["vehicle"]) == key]
    groups = {mode: [r for r in group if r["mode"] == mode] for mode in ("normal", "clean", "imperfect", "recovery")}
    medians = {mode: statistics.median(float(r["seconds"]) for r in values) for mode, values in groups.items() if values}
    for mode, values in groups.items():
        if len(values) < 2:
            failures.append(f"{key}: {mode} has fewer than two trials")
        if any(r["finished"] != "True" or int(r["misses"]) for r in values):
            failures.append(f"{key}: {mode} has a timeout or missed gate")
    if any(int(r["recoveries"]) for r in groups["clean"]):
        failures.append(f"{key}: clean trial required recovery")
    gain = medians.get("normal", 0) - medians.get("clean", 0)
    if gain <= 0:
        failures.append(f"{key}: no measured clean advantage")
    values = [f"{medians[m]:.2f}" if m in medians else "missing" for m in groups]
    lines.append(f"| {key[0]} / {key[1]} | {key[2]} | " + " | ".join(values) + f" | {gain:.2f} |")
lines += ["", "Seconds; median of repeated physical motor trials. Countdown is excluded.",
          "Imperfect input includes entry offset/yaw and three seconds of actual braking/steering after entry. Local recovery uses the game reset implementation.",
          "These scripted driving comparisons do not establish human fun, difficulty or controller acceptance.", ""]
lines += ["Raw sources: " + "; ".join(sources), ""]
lines += ["Verification: " + ("all repeated completion/credit/clean-advantage checks passed" if not failures else "issues below")]
lines += ["- " + issue for issue in failures]
(root / "timing-comparison.md").write_text("\n".join(lines) + "\n", encoding="utf-8")
print("\n".join(lines))
