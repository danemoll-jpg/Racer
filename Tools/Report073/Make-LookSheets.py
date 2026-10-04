"""0.73 Part C: copy the bench views into Docs/Report073/Look and make one 3x3 sheet per course family (time x weather)
plus the Free Roam dusk-to-night strip. Bench output: %LOCALAPPDATA%/Temp/report073/bench/run."""
import os, pathlib, shutil
from PIL import Image, ImageDraw
src=pathlib.Path(os.environ['LOCALAPPDATA'])/'Temp/report073/bench/run'; out=pathlib.Path(__file__).resolve().parents[2]/'Docs/Report073/Look'
models=out.parent/'Models/game'
for f in src.glob('*.jpg'):
    shutil.copy2(f, (models if f.name.startswith('moto-') else out)/f.name)
shutil.copy2(src/'conditions.txt', out.parent/'frame-rate.txt')
def sheet(names,cols,path):
    W,H=640,360;rows=(len(names)+cols-1)//cols;s=Image.new('RGB',(W*cols,H*rows));d=ImageDraw.Draw(s)
    for i,n in enumerate(names):
        s.paste(Image.open(out/(n+'.jpg')).resize((W,H)),((i%cols)*W,(i//cols)*H));d.text(((i%cols)*W+8,(i//cols)*H+6),n,fill=(255,255,0))
    s.save(path,quality=88)
for fam in ['street','forest','backyard','mountain']:
    sheet([f'{fam}-{t}-{w}' for t in ['day','dusk','night'] for w in ['clear','rain','snow']],3,out/f'sheet-{fam}.jpg')
sheet([f'freeroam-cycle-{h}' for h in ['1800','1900','2000','2100','2200','2300']],3,out/'sheet-freeroam-dusk-to-night.jpg')
print('ok')
