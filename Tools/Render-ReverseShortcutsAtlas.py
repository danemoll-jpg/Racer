import json,math,html,shutil
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
root=Path(__file__).resolve().parents[1];out=root/'Docs/ReverseShortcuts';Image.MAX_IMAGE_PIXELS=160_000_000
font=lambda n:ImageFont.truetype('C:/Windows/Fonts/arial.ttf',n)
x0,x1,z0,z1=-15,505,-130,170;w,h=1820,1050
def xy(p):return ((p['x']-x0)/(x1-x0)*w,(z1-p['z'])/(z1-z0)*h)
for suffix in ['Forward','Reverse']:
 name='DansBackyard'+suffix;data=json.loads((root/'Docs/WorldMap'/f'{name}.json').read_text());b=data['bounds'];source=Image.open(root/'Assets/Resources/WorldMaps'/f'{name}.png').convert('RGB')
 crop=((x0-b['x'])/b['width']*source.width,(b['y']+b['height']-z1)/b['height']*source.height,(x1-b['x'])/b['width']*source.width,(b['y']+b['height']-z0)/b['height']*source.height)
 image=source.crop(crop).resize((w,h),Image.Resampling.LANCZOS);draw=ImageDraw.Draw(image)
 def route(points,color,closed=False,start=0,end=0):
  station=0
  for a,c in zip(points,points[1:]+points[:1] if closed else points[1:]):
   underground=start<=station<end and end>start
   if not underground or int(station/4)%2==0:
    draw.line((xy(a),xy(c)),fill='#12292b',width=8);draw.line((xy(a),xy(c)),fill=color,width=4)
   station+=math.hypot(c['x']-a['x'],c['z']-a['z'])
 route(data['route'],'#43edda',True)
 for branch in data['branches']:route(branch['points'],'#ffbd48',start=branch.get('undergroundStart',0),end=branch.get('undergroundEnd',0))
 for i,gate in enumerate(data['gates']):
  x,y=xy(gate['position']);draw.ellipse((x-5,y-5,x+5,y+5),fill='white' if i==0 else '#4389ff');draw.text((x+9,y-14),'START' if i==0 else f'CP{i}',font=font(19),fill='white',stroke_width=2,stroke_fill='#102328')
 if suffix=='Reverse':
  for label,p in [('Logging Ridge',{'x':347,'z':111}),('Storm Drain — underground',{'x':148,'z':48}),('Gully jump',{'x':95.8,'z':-24})]:
   x,y=xy(p);draw.text((x+12,y+7),label,font=font(23),fill='#ffdc91',stroke_width=2,stroke_fill='#102328')
 canvas=Image.new('RGB',(w,h+125),'#11292d');canvas.paste(image,(0,110));q=ImageDraw.Draw(canvas);q.text((24,18),"DAN'S BACKYARD LOOP — "+suffix.upper(),font=font(32),fill='white');q.text((24,65),'Actual saved geometry  •  Teal main  •  Gold optional  •  Dashed gold underground  •  North / +Z up',font=font(23),fill='#c8dfdf');canvas.save(out/f'{suffix}.png')
page='''<!doctype html><meta charset="utf-8"><title>Backyard route atlas</title><style>body{margin:24px;background:#11292d;color:#e6f3f3;font:17px system-ui}button{padding:10px 20px;margin:0 8px 16px 0;background:#24474c;color:white;border:1px solid #5d8589;border-radius:6px;cursor:pointer}img{width:100%;max-width:1820px}p{max-width:1000px;line-height:1.5}a{color:#ffdc91}</style><h1>Dan's Backyard Loop</h1><button onclick="document.querySelector('img').src='Reverse.png'">Reverse</button><button onclick="document.querySelector('img').src='Forward.png'">Forward</button><p>Teal is the accepted main route. Gold marks optional shortcuts; dashes mark the underground drain beneath the existing surface trail. Reverse has Logging Ridge and Storm Drain / Gully Jump. Forward retains its original Tree-Top Trail and Cabin Jump; the new drain grate and logging gate close during Forward races. Free roam opens them.</p><img src="Reverse.png" alt="Actual geometry and routes for Backyard Reverse"><p>Original anchor coordinates and validation: <a href="REQUEST.md">request</a> · <a href="VALIDATION.md">technical evidence and limitations</a>. Both new branches are player-only. Existing main routes remain unchanged.</p>'''
(out/'ATLAS.html').write_text(page,encoding='utf-8')
for name in ['Forward.png','Reverse.png','ATLAS.html']:
 target=root/'Docs/BackyardReverse'/name;tmp=target.with_name(name+'.task-tmp');shutil.copy2(out/name,tmp);tmp.replace(target)
(root/'Docs/BackyardReverse/ATLAS.html').write_text(page.replace('href="REQUEST.md"','href="../ReverseShortcuts/REQUEST.md"').replace('href="VALIDATION.md"','href="../ReverseShortcuts/VALIDATION.md"'),encoding='utf-8')
print('Updated actual-geometry Forward/Reverse atlas with underground convention.')
