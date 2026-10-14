# 0.116: the cover strokes that hide Granite Creek Cut and Laurel Switchbacks (Street Loop Reverse's shortcuts, painted on the world map
# artwork because Free Roam has those trails) on Leviathan's maps. For each trail (StreetLoopReverse.unity's route points), a point every
# 4 m and the artwork's own colour beside it (the median of samples 8-14 m either side), so the stroke reads as the ground around it.
# Writes Assets/Resources/Leviathan/MapCover.json and a before/after preview.
import json, math, sys
from PIL import Image, ImageDraw
src, out, preview = sys.argv[1], sys.argv[2], sys.argv[3]
im = Image.open('Assets/Resources/WorldMaps/PermanentWorld.png').convert('RGB'); W, H = im.size
bx, bz, bw, bh = -1350, -850, 3000, 1800
def px(x, z): return ((x - bx) / bw * W, H - (z - bz) / bh * H)
trails = json.load(open(src)); paths = []
for title, pts in trails.items():
    res = [pts[0]]; acc = 0
    for a, b in zip(pts, pts[1:]):
        d = math.dist(a, b); acc += d
        if acc >= 4: res.append(b); acc = 0
    if res[-1] != pts[-1]: res.append(pts[-1])
    res = res[3:-3]  # not the last 12 m at either end, where the trail meets the road (the road stays as painted)
    flat, cols = [], []
    for k, p in enumerate(res):
        a = res[max(0, k - 1)]; b = res[min(len(res) - 1, k + 1)]; fx, fz = b[0] - a[0], b[1] - a[1]; L = math.hypot(fx, fz) or 1; nx, nz = -fz / L, fx / L
        samples = []
        for side in (-1, 1):
            for off in (8, 10, 12, 14):
                for along in (-4, 0, 4):
                    q = px(p[0] + side * nx * off + fx / L * along, p[1] + side * nz * off + fz / L * along)
                    if 0 <= q[0] < W and 0 <= q[1] < H: samples.append(im.getpixel((int(q[0]), int(q[1]))))
        samples.sort(key=sum); c = samples[len(samples) // 2]
        flat += [round(p[0], 2), round(p[1], 2)]; cols += [round(c[0] / 255, 3), round(c[1] / 255, 3), round(c[2] / 255, 3)]
    paths.append({'title': title, 'pts': flat, 'colors': cols})
json.dump({'width': 11, 'paths': paths}, open(out, 'w'))
# preview: the strokes painted on the artwork around Granite Creek Cut and Laurel Switchbacks
after = im.copy(); d = ImageDraw.Draw(after); wpx = 11 / bw * W
for p in paths:
    pts = [(p['pts'][i], p['pts'][i + 1]) for i in range(0, len(p['pts']), 2)]
    for k in range(len(pts) - 1):
        c = tuple(int(255 * v) for v in p['colors'][3 * k:3 * k + 3]); d.line([px(*pts[k]), px(*pts[k + 1])], fill=c, width=int(wpx + .5))
x0, y0 = px(-700, 600); x1, y1 = px(620, -420); box = (int(x0), int(y0), int(x1), int(y1))
a = im.crop(box); b = after.crop(box); both = Image.new('RGB', (a.width * 2 + 10, a.height), (0, 0, 0)); both.paste(a, (0, 0)); both.paste(b, (a.width + 10, 0)); both.save(preview, quality=85)
print('paths', [(p['title'], len(p['pts']) // 2) for p in paths])
