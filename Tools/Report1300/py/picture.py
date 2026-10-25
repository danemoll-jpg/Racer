"""1.3: the numbered map picture. Joins the 3 x 3 top-down tiles (<dir>/<name>-00..22.png: x -1400..1900, z -900..1500, 0.5 m per pixel), crops the
world (x -1350..1650, z -850..950) at 1 m per pixel and marks every item of Assets/Resources/RoamWorld.json (or the file given) with its number and kind.
Usage: picture.py <shots dir> <name> <out.png> [items.json]"""
import json, sys, pathlib, math
from PIL import Image, ImageDraw, ImageFont

root = pathlib.Path(__file__).resolve().parents[3]
d, name, out = pathlib.Path(sys.argv[1]), sys.argv[2], pathlib.Path(sys.argv[3])
items = json.loads(pathlib.Path(sys.argv[4] if len(sys.argv) > 4 else root / 'Assets/Resources/RoamWorld.json').read_text())['items']
full = Image.new('RGB', (3300, 2400))
for qx in range(3):
    for qz in range(3):
        q = Image.open(d / f'{name}-{qx}{qz}.png').convert('RGB').resize((1100, 800), Image.LANCZOS)
        full.paste(q, (qx * 1100, (2 - qz) * 800))  # north (z+) up
XMIN, ZMAX = -1350, 950
img = full.crop((XMIN + 1400, 1500 - ZMAX, 1650 + 1400, 1500 + 850)); W, H = img.size
draw = ImageDraw.Draw(img)
try: font = ImageFont.truetype('arialbd.ttf', 26); small = ImageFont.truetype('arial.ttf', 20)
except OSError: font = small = ImageFont.load_default()
def px(x, z): return x - XMIN, ZMAX - z
colours = {'table': (255, 150, 40), 'kicker': (255, 190, 60), 'gap': (255, 120, 30), 'house': (255, 90, 30), 'through': (255, 60, 60), 'smash': (230, 40, 40), 'flip': (200, 110, 255), 'loop': (90, 200, 255)}
for i in items:
    x, z = i['pos']; c = colours.get(i['kind'], (255, 255, 255)); p = px(x, z); yaw = math.radians(i.get('yaw', 0))
    tip = (p[0] + math.sin(yaw) * 26, p[1] - math.cos(yaw) * 26)
    draw.line([p, tip], fill=(0, 0, 0), width=7); draw.line([p, tip], fill=c, width=3)
    draw.ellipse([p[0] - 15, p[1] - 15, p[0] + 15, p[1] + 15], fill=c, outline=(0, 0, 0), width=3)
    draw.text((p[0], p[1]), i['id'], font=font, fill=(0, 0, 0), anchor='mm')
    label = f"{i['id']} {i['kind']}" + (f" ({i['house']})" if i.get('house') else '')
    draw.text((p[0] + 20, p[1] + 14), label, font=small, fill=(255, 255, 255), stroke_width=3, stroke_fill=(0, 0, 0))
key = '  '.join(f'{k}' for k in colours)
draw.rectangle([10, H - 46, 1040, H - 10], fill=(0, 0, 0))
x = 20
for k, c in colours.items():
    draw.ellipse([x, H - 38, x + 20, H - 18], fill=c); draw.text((x + 26, H - 40), k, font=small, fill=(255, 255, 255)); x += 128
draw.text((W - 20, 20), 'N ↑   1 px = 1 m   x -1350..1650, z -850..950', font=small, fill=(255, 255, 255), anchor='ra', stroke_width=3, stroke_fill=(0, 0, 0))
img.save(out); print('wrote', out, len(items), 'items')
