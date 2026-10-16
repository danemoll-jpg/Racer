# 0.120 Part C: a top-down of each Stunt Race course with its bumps marked (the course line from the probe, the gates, the big jumps' lips, each
# bump numbered as in bumps.md, an arrow the way you drive over it).
#   python topdown.py <probe root> <bumps json> <out dir>
import csv, json, math, os, sys
import matplotlib; matplotlib.use('Agg')
import matplotlib.pyplot as plt

COURSES = [("probe4", "mountain-forward", "Mountain Loop Forward"), ("probe5", "mountain-reverse", "Mountain Loop Reverse"),
           ("probe8", "leviathan-forward", "Leviathan Forward"), ("probe9", "leviathan-reverse", "Leviathan Reverse")]

def rows(p):
    with open(p, newline='') as f: return list(csv.DictReader(f))

def main():
    root, bumps_json, out = sys.argv[1:4]; os.makedirs(out, exist_ok=True)
    data = {c['course']: c['bumps'] for c in json.load(open(bumps_json))['courses']}
    for folder, key, title in COURSES:
        f = os.path.join(root, folder)
        if not os.path.exists(os.path.join(f, 'line.csv')): continue
        line = rows(os.path.join(f, 'line.csv')); gates = rows(os.path.join(f, 'gates.csv')); flights = rows(os.path.join(f, 'flights.csv'))
        fig, ax = plt.subplots(figsize=(11, 11), dpi=110)
        ax.plot([float(r['x']) for r in line], [float(r['z']) for r in line], color='#2a7fbf', lw=2.2, label='course line', zorder=1)
        ax.scatter([float(g['x']) for g in gates], [float(g['z']) for g in gates], marker='s', s=26, color='#888888', label='gates', zorder=2)
        ax.scatter([float(fl['lipx']) for fl in flights], [float(fl['lipz']) for fl in flights], marker='^', s=110, color='#7a3fbf', label='big jumps (lips)', zorder=3)
        x0, z0 = float(line[0]['x']), float(line[0]['z']); ax.scatter([x0], [z0], marker='*', s=200, color='#2a9d3a', label='start', zorder=3)
        for i, b in enumerate(data.get(key, []), 1):
            x, z = b['pos'][0], b['pos'][1]; big = b['height'] > 1.6; yaw = math.radians(b['yaw'])
            ax.scatter([x], [z], s=170 if big else 110, color='#e0531f' if big else '#f2a01f', edgecolor='black', zorder=4)
            ax.annotate('', xy=(x + math.sin(yaw) * 70, z + math.cos(yaw) * 70), xytext=(x, z), arrowprops=dict(arrowstyle='->', color='black', lw=1.2), zorder=4)
            ax.annotate(f"{i}{' (big)' if big else ''}\n{x:.0f}, {z:.0f}", (x, z), xytext=(10, 8), textcoords='offset points', fontsize=10, weight='bold', zorder=5)
        ax.set_aspect('equal'); ax.set_xlabel('X'); ax.set_ylabel('Z'); ax.grid(alpha=.25); ax.legend(loc='best', fontsize=9)
        ax.set_title(f"Stunt Race bumps: {title}  (orange: kicker, red: big kicker; arrow: the way you drive)")
        fig.tight_layout(); fig.savefig(os.path.join(out, f"topdown-{key}.png")); plt.close(fig)

if __name__ == '__main__': main()
