# Side-by-side before/after JPGs for Docs/Report068/views (renders from Report068Views.cs).
import os,sys
from PIL import Image,ImageDraw
L=os.path.join(os.environ['LOCALAPPDATA'],'Temp','report068')
b=os.path.join(L,'inventory','views'); a=os.path.join(L,'views-after','views')
ids=['BUG-003','BUG-003-wide','BUG-004','BUG-005','BUG-005-top','BUG-006','BUG-006-wide','BUG-007','BUG-008','BUG-008-landing','BUG-009','TOP-landing-turn','TOP-downhill-ridge','TOP-southface-turn']
for i in ids:
    pb=os.path.join(b,i+'-before.png'); pa=os.path.join(a,i+'-after.png')
    if not (os.path.exists(pb) and os.path.exists(pa)): print('missing',i); continue
    x=Image.open(pb).convert('RGB').resize((640,360)); y=Image.open(pa).convert('RGB').resize((640,360))
    c=Image.new('RGB',(1290,380),(20,20,20)); c.paste(x,(0,20)); c.paste(y,(650,20)); d=ImageDraw.Draw(c); d.text((5,3),i+'  BEFORE (0.67)',fill=(255,255,255)); d.text((655,3),'AFTER (0.68)',fill=(255,255,255))
    c.save(os.path.join('Docs','Report068','views',i+'-before-after.jpg'),quality=82)
for i in ['E-bend-side','E-highridge','E-southface','BUG-006-left']:
    pa=os.path.join(a,i+'-after.png')
    if os.path.exists(pa): Image.open(pa).convert('RGB').resize((960,540)).save(os.path.join('Docs','Report068','views',i+'-after.jpg'),quality=82)
