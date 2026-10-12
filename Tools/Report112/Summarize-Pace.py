# 0.112 Part A/B: margins from pace112 check output (p106-check.txt files): per event the best rival per run, mean / min / max, Dan's margin, resets.
import re, sys, statistics
DAN = {"c1-first-lap": 230.837, "c1-night-shift": 360.209, "c1-wrong-way": 247.606, "c1-street-final": 352.485,
       "c4-first-ascent": 148.507, "c4-downhill": 154.999, "c4-whiteout": 161.571, "c4-summit-final": 203.211}
WINDOW = {"c1-first-lap": (20, 40), "c1-night-shift": (20, 40), "c1-wrong-way": (20, 40), "c1-street-final": (12, 25),
          "c4-first-ascent": (5, 15), "c4-downhill": (5, 15), "c4-whiteout": (5, 15), "c4-summit-final": (0, 8)}
rows = {}
for path in sys.argv[1:]:
    event = None
    for line in open(path, encoding="utf-8"):
        m = re.match(r"== (\S+) .*difficulty (\d) pace ([\d.]+) rivals (\S+)", line)
        if m: event = m.group(1); rows.setdefault(event, {"setup": f"difficulty {m.group(2)}, pace {m.group(3)}, rivals {m.group(4)}", "runs": {}}); continue
        m = re.match(r"\s+run (\d+) (\S+)\s+(\S+) resets (\d+)", line)
        if m and event:
            run = rows[event]["runs"].setdefault(int(m.group(1)), [])
            t = float(m.group(3)) if re.match(r"[\d.]+$", m.group(3)) else None
            run.append((m.group(2), t, int(m.group(4))))
print("| Event | Dan | Rivals' setup | Best rival per run (median / min / max) | Margin on the median (window) | Rival resets per run | DNF |")
print("|---|---|---|---|---|---|---|")
for event, r in rows.items():
    best = [min(t for _, t, _ in run if t is not None) for run in r["runs"].values() if any(t is not None for _, t, _ in run)]
    resets = [sum(x for _, _, x in run) for run in r["runs"].values()]
    dnf = sum(1 for run in r["runs"].values() for _, t, _ in run if t is None)
    mean = statistics.median(best); lo, hi = WINDOW[event]; margin = mean - DAN[event]
    print(f"| {event} | {DAN[event]:.1f} | {r['setup']} | {mean:.1f} / {min(best):.1f} / {max(best):.1f} | {margin:+.1f} ({lo}-{hi}{' OK' if lo <= margin <= hi else ''}) | {', '.join(map(str, resets))} | {dnf} |")
