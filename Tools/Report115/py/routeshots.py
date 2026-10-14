# 0.115: the Leviathan route over a top-down render of its own scene (Tools/Report115/Report115Top.cs): the line (magenta, arrows every 250 m),
# the gates (numbered; lake and mountain gates red), the Street Loop's shortcuts (gold), the flights (dashed), and the 1 km scale.
# python Tools/Report115/py/routeshots.py <course json> <render png> <cx,cz,half> <out jpg> [x0,x1,z0,z1]
import sys, json, math
import matplotlib; matplotlib.use('Agg'); import matplotlib.pyplot as plt, matplotlib.image as mpimg
c = json.load(open(sys.argv[1])); img = mpimg.imread(sys.argv[2]); cx, cz, half = map(float, sys.argv[3].split(',')); out = sys.argv[4]
box = [float(v) for v in sys.argv[5].split(',')] if len(sys.argv) > 5 else None
f = c['pts']; pts = [f[i:i + 3] for i in range(0, len(f), 3)]
fig = plt.figure(figsize=(16, 16 if not box else 16 * (box[3] - box[2]) / (box[1] - box[0]))); ax = fig.gca(); ax.imshow(img, extent=[cx - half, cx + half, cz - half, cz + half])
ax.plot([p[0] for p in pts] + [pts[0][0]], [p[2] for p in pts] + [pts[0][2]], '-', color='magenta', lw=2.2, label=f"{c['title']}  ·  lap {c['length'] / 1000:.2f} km")
s = 0; nxt = 0; step = 250 if not box else 60
for i in range(1, len(pts)):
    s += math.dist(pts[i], pts[i - 1])
    if s >= nxt:
        nxt = s + step
        if not box or (box[0] < pts[i][0] < box[1] and box[2] < pts[i][2] < box[3]):
            ax.annotate('', xy=(pts[i][0], pts[i][2]), xytext=(pts[i - 1][0], pts[i - 1][2]), arrowprops=dict(arrowstyle='-|>', color='white', lw=1.5, mutation_scale=18))
for b in c['branches']:
    q = b['pts']; q = [q[i:i + 3] for i in range(0, len(q), 3)]; ax.plot([p[0] for p in q], [p[2] for p in q], '-', color='gold', lw=1.8)
for fl in c['flights']: ax.plot([fl['lip']['x'], fl['landingEnd']['x']], [fl['lip']['z'], fl['landingEnd']['z']], '--', color='cyan', lw=2)
for k, g in enumerate(c['gates']):
    p = g['pos']; yaw = math.radians(g['rot'][1]); r = (math.cos(yaw), -math.sin(yaw)); hw = g['halfWidth']
    ax.plot([p[0] - r[0] * hw, p[0] + r[0] * hw], [p[2] - r[1] * hw, p[2] + r[1] * hw], '-', color='red' if g['mandatory'] else 'lime', lw=3)
    ax.annotate('S/F' if k == 0 else str(k), (p[0], p[2]), xytext=(6, 6), textcoords='offset points', fontsize=11 if box else 9, color='white', weight='bold')
if box: ax.set_xlim(box[0], box[1]); ax.set_ylim(box[2], box[3])
ax.plot([], [], '-', color='gold', label='Street Loop shortcuts'); ax.plot([], [], '--', color='cyan', label='flights'); ax.plot([], [], '-', color='red', lw=3, label='lake and mountain gates (in order)')
ax.legend(loc='upper left', fontsize=11); ax.set_xlabel('x (m)'); ax.set_ylabel('z (m, north up)'); ax.set_title(c['title'] + ' — top-down (scene ' + c['scene'] + ')')
fig.savefig(out, dpi=70, bbox_inches='tight', pil_kwargs={'quality': 85})
