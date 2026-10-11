"""0.106: copies the built-player checks' lists and shots (from the scratch folder) into Docs/Report106 (shots as JPEG, at most 1920 wide)."""
import pathlib, shutil, sys
from PIL import Image
root = pathlib.Path(__file__).resolve().parents[3]; src = pathlib.Path.home() / 'AppData/Local/Temp/report106'; out = root / 'Docs/Report106'
(out / 'Player').mkdir(parents=True, exist_ok=True); (out / 'Shots').mkdir(parents=True, exist_ok=True)
for run in (sys.argv[1:] or [p.name for p in src.iterdir() if p.is_dir()]):
    d = src / run
    if not (d / 'p106-check.txt').exists(): continue
    shutil.copy2(d / 'p106-check.txt', out / 'Player' / f'{run}.txt')
    for t in d.glob('*.txt'):
        if t.name not in ('p106-check.txt', 'done.txt'): shutil.copy2(t, out / 'Player' / f'{run}-{t.name}')
    for p in d.glob('*.png'):
        im = Image.open(p).convert('RGB')
        if im.width > 1920: im = im.resize((1920, round(im.height * 1920 / im.width)), Image.LANCZOS)
        im.save(out / 'Shots' / (p.stem + '.jpg'), quality=86)
    print('copied', run)
