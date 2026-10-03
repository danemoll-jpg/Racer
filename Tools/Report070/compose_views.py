# Side-by-side before/after JPGs for Docs/Report070/views (renders from Report070Views.cs; 'before' = the 0.69 scenes).
import os
from PIL import Image,ImageDraw
L=os.path.join(os.environ['LOCALAPPDATA'],'Temp','report070')
before=os.path.join(L,'views-before','views')
after={  # id -> (folder, tag) of the final 'after' render
 'BUG-003':('views-f2','after2'),'BUG-004':('views-f2','after2'),
 'BUG-005':('views-c2','after2'),'BUG-005-over':('views-c2','after2'),'BUG-006':('views-c2','after2'),
 'BUG-007':('views-d1','after1'),'BUG-007-camp':('views-d1','after1'),
 'BUG-008':('views-final','after'),'BUG-008-wide':('views-final','after'),
 'BUG-009':('views-b009e','after2'),'BUG-009-wide':('views-final','after')}
out=os.path.join('Docs','Report070','views');os.makedirs(out,exist_ok=True)
for i in ['BUG-001','BUG-001-over','BUG-002']:
    p=os.path.join(before,i+'-before.png')
    if os.path.exists(p):Image.open(p).convert('RGB').resize((960,540)).save(os.path.join(out,i+'-unchanged.jpg'),quality=82)
for i,(folder,tag) in after.items():
    pb=os.path.join(before,i+'-before.png');pa=os.path.join(L,folder,'views',i+'-'+tag+'.png')
    if not(os.path.exists(pb) and os.path.exists(pa)):print('missing',i);continue
    x=Image.open(pb).convert('RGB').resize((640,360));y=Image.open(pa).convert('RGB').resize((640,360))
    c=Image.new('RGB',(1290,380),(20,20,20));c.paste(x,(0,20));c.paste(y,(650,20));d=ImageDraw.Draw(c);d.text((5,3),i+'  BEFORE (0.69)',fill=(255,255,255));d.text((655,3),'AFTER (0.70)',fill=(255,255,255))
    c.save(os.path.join(out,i+'-before-after.jpg'),quality=82)
for i,(folder,tag) in {'BUG-007-reverse':('views-d2','after1'),'BUG-009-left-gore':('views-b009e','after2')}.items():
    src={'BUG-007-reverse':'BUG-007-rev','BUG-009-left-gore':'BUG-009-L'}[i]
    pa=os.path.join(L,folder,'views',src+'-'+tag+'.png')
    if os.path.exists(pa):Image.open(pa).convert('RGB').resize((960,540)).save(os.path.join(out,i+'-after.jpg'),quality=82)
print('done')
