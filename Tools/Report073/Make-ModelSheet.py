"""0.73 Part D: one render sheet, old vs new. Row 1: the new Needle 600 rendered in Blender (final revision 3).
Rows 2-3: the classic (generated in code, so it has no Blender source) and the new model as the game draws them
(Unity, same four views, garage-style isolated render). Written to Docs/Report073/Models/render-sheet-old-vs-new.png."""
import pathlib
from PIL import Image, ImageDraw, ImageFont
d=pathlib.Path(__file__).resolve().parents[2]/'Docs/Report073/Models'
views=['front','side','three-quarter','top']
rows=[('Blender, new (rev 3)',[d/f'renders/rev3-{v}.png' for v in views]),('Game, classic',[d/f'unity/unity-classic-{v}.png' for v in views]),('Game, new',[d/f'unity/unity-new-{v}.png' for v in views])]
W,H,L=500,380,40
sheet=Image.new('RGB',(W*4,(H+L)*3),(235,237,240));dr=ImageDraw.Draw(sheet)
try: font=ImageFont.truetype('arial.ttf',24)
except Exception: font=ImageFont.load_default()
for r,(label,files) in enumerate(rows):
    y=r*(H+L);dr.text((12,y+8),label,fill=(20,20,20),font=font)
    for c,f in enumerate(files):
        im=Image.open(f).convert('RGB').resize((W,H));sheet.paste(im,(c*W,y+L));dr.text((c*W+12,y+L+8),views[c],fill=(20,20,20),font=font)
out=d/'render-sheet-old-vs-new.png';sheet.save(out);print(out)
