"""0.75 evidence sheets in Docs/Report075/Models/ (JPEG).
  vehicle-<id>-old-vs-new.jpg  row 1: the new model rendered in Blender (final revision, default rider);
                               rows 2-3: the classic and the new model as the game draws them (garage-style isolated render).
  rider-*.jpg                  the rider options rendered in Blender (Moto pose; bodies also in the Atv and Car poses).
  rider-ingame-hat-hair.jpg    the in-game Rider page preview for every hat with every hair style (man) and every hair (woman).
Usage: Make-ModelSheets.py <blender renders dir> <unity models dir> <unity rider dir>"""
import sys, pathlib, glob
from PIL import Image, ImageDraw, ImageFont
blender, unity, ridergame = map(pathlib.Path, sys.argv[1:4])
out = pathlib.Path(__file__).resolve().parents[2] / 'Docs/Report075/Models'; out.mkdir(parents=True, exist_ok=True)
try: font = ImageFont.truetype('arial.ttf', 22); small = ImageFont.truetype('arial.ttf', 17)
except Exception: font = small = ImageFont.load_default()


def sheet(name, rows, W=420, labels=True):
    """rows: [(row label, [(caption, file), ...]), ...]"""
    first = Image.open(rows[0][1][0][1]); H = int(W * first.height / first.width); L = 34; C = max(len(r[1]) for r in rows)
    im = Image.new('RGB', (W * C, (H + L) * len(rows)), (235, 237, 240)); d = ImageDraw.Draw(im)
    for r, (label, cells) in enumerate(rows):
        y = r * (H + L); d.text((10, y + 6), label, fill=(20, 20, 20), font=font)
        for c, (cap, f) in enumerate(cells):
            pic = Image.open(f).convert('RGB'); h = int(W * pic.height / pic.width)
            im.paste(pic.resize((W, h)).crop((0, 0, W, H)), (c * W, y + L))
            if labels and cap: d.text((c * W + 8, y + L + 6), cap, fill=(20, 20, 20), font=small)
    im.save(out / name, quality=86); print(out / name, im.size)


VIEWS = ['front', 'side', 'three-quarter', 'top']
for vid, name in (('moto', 'Needle 600'), ('atv', 'Trail Four'), ('original', 'Street Classic'), ('tourer', 'Longroof GT')):
    sheet(f'vehicle-{vid}-old-vs-new.jpg', [(f'{name}: new, Blender (final revision)', [(v, blender / 'veh' / f'{vid}-{v}.png') for v in VIEWS]),
                                           (f'{name}: classic, in game', [(v, unity / f'unity-{vid}-classic-{v}.png') for v in VIEWS]),
                                           (f'{name}: new, in game', [(v, unity / f'unity-{vid}-new-{v}.png') for v in VIEWS])])
    extra = 'cabin' if vid in ('original', 'tourer') else 'rider-close'
    sheet(f'vehicle-{vid}-detail.jpg', [(f'{name}: new, Blender', [(v, blender / 'veh' / f'{vid}-{v}.png') for v in ('rear-three-quarter', extra)])], W=700)
R = blender / 'rider'
def files(pat): return sorted(glob.glob(str(R / pat)))
def cap(f): return pathlib.Path(f).stem.split('-', 3)[-1]
sheet('rider-bodies.jpg', [(f'{pose} pose', [(cap(f), f) for f in files(f'{pose}-bodies-*')]) for pose in ('Moto', 'Atv', 'Car')], W=360)
hair = files('Moto-hair-*')
sheet('rider-hair.jpg', [('Hair styles, man (front / rear)', [(cap(f), f) for f in hair[:10]]), ('Hair styles, woman (front / rear)', [(cap(f), f) for f in hair[10:]])], W=260)
hats = files('Moto-hats-*'); sheet('rider-hats.jpg', [('Hats (front / rear)', [(cap(f), f) for f in hats])], W=260)
hh = files('Moto-hathair-*a-*')
sheet('rider-hats-with-hair.jpg', [(f'{h} with each hair style', [(cap(f), f) for f in hh[i * 5:(i + 1) * 5]]) for i, h in enumerate(('Flat cap', 'Baseball cap', 'Beanie', 'Cowboy hat'))], W=300)
sheet('rider-shirts-pants.jpg', [('Shirts', [(cap(f), f) for f in files('Moto-shirts-*')]), ('Pants', [(cap(f), f) for f in files('Moto-pants-*')])], W=300)
G = ridergame
rows = [(f'In game, hat {h}', [(f'hair {r}', G / f'rider-hat{h}-hair{r}-man.png') for r in range(4)]) for h in range(5)]
rows.append(('In game, woman, no hat', [(f'hair {r}', G / f'rider-hat0-hair{r}-woman.png') for r in range(5)]))
sheet('rider-ingame-hat-hair.jpg', rows, W=380)
