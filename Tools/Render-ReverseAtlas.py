"""Render two separately viewable actual-geometry Backyard atlas sheets."""
import json,math
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
r=Path(__file__).resolve().parents[1];out=r/'Docs/BackyardReverse'
def font(n,b=False):return ImageFont.truetype('C:/Windows/Fonts/'+('arialbd.ttf' if b else 'arial.ttf'),n)
def xy(p):return 120+(p['x']+40)*7.5,220+(206.666667-p['z'])*7.5
for direction in ['Forward','Reverse']:
 data=json.loads((out/f'{direction}-map.json').read_text())
 im=Image.new('RGB',(5040,3750),'#10262b');im.paste(Image.open(out/f'{direction}-base.png'),(120,220));d=ImageDraw.Draw(im,'RGBA')
 d.text((120,40),f"DAN'S BACKYARD LOOP — {direction.upper()}",font=font(66,True),fill='white')
 d.text((120,128),'Actual saved scene geometry  /  Unity X/Z metres  /  North +Z up',font=font(32),fill='#b6d1d4')
 for x in range(0,601,50):
  a=xy({'x':x,'z':-220});b=xy({'x':x,'z':206.666667});d.line([a,b],fill=(235,255,255,60),width=2);d.text((a[0]-35,3440),f'X {x}',font=font(25),fill='#d7eeee')
 for z in range(-200,201,50):
  a=xy({'x':-40,'z':z});b=xy({'x':600,'z':z});d.line([a,b],fill=(235,255,255,60),width=2);d.text((12,a[1]-15),f'Z {z}',font=font(23),fill='#d7eeee')
 def line(points,color,closed=False):
  pts=[xy(p) for p in points];pts+=pts[:1] if closed else [];d.line(pts,fill='#10262b',width=23,joint='curve');d.line(pts,fill=color,width=12,joint='curve');distance=0
  for a,b in zip(pts,pts[1:]):
   length=math.dist(a,b);distance+=length
   if distance<260 or length<.01:continue
   distance=0;ux=(b[0]-a[0])/length;uy=(b[1]-a[1])/length;cx,cy=b;d.polygon([(cx+ux*26,cy+uy*26),(cx-ux*18-uy*14,cy-uy*18+ux*14),(cx-ux*18+uy*14,cy-uy*18-ux*14)],fill='white',outline='#10262b',width=3)
 line(data['route'],'#4ff4e3',True)
 for branch in data['branches']:line(branch['points'],'#ffb942')
 def label(text,p,dx=15,dy=15,color='white',size=29):
  x,y=xy(p);x+=dx;y+=dy;box=d.multiline_textbbox((x,y),text,font=font(size,True),spacing=5);d.rounded_rectangle((box[0]-10,box[1]-8,box[2]+10,box[3]+8),radius=8,fill=(10,30,35,225));d.multiline_text((x,y),text,font=font(size,True),fill=color,spacing=5)
 for i,g in enumerate(data['gates']):
  x,y=xy(g['position']);d.ellipse((x-17,y-17,x+17,y+17),fill='white' if i==0 else '#438fe7',outline='white',width=3);label('START / FINISH' if i==0 else f'CP {i}',g['position'],20,16,size=24)
 names=['Dump jump','South gully jump','North gully jump'] if direction=='Forward' else ['Reverse north gully','Reverse south gully','Pool-house flight']
 for name,f in zip(names,data['flights']):
  x,y=xy(f['launch']);d.polygon([(x,y-23),(x+23,y),(x,y+23),(x-23,y)],fill='#ffbd42',outline='#15292b',width=3);label(name+'\n'+f"X {f['launch']['x']:.1f} / Z {f['launch']['z']:.1f}",f['launch'],-40,-120,color='#ffce68')
 for title,p in [('Garbage dump',{'x':281,'z':10}),('Continuous gully',{'x':96,'z':-15}),("Roger's",{'x':440,'z':-82}),("McFadden's",{'x':418,'z':-160}),("Moll's",{'x':415.8,'z':-33}),('Pool',{'x':416,'z':-197}),('Lake',{'x':384,'z':-194.1}),('South Cherokee Lane',{'x':473,'z':145})]:label(title,p,size=28)
 for b in data['branches']:label(b['title'],b['points'][len(b['points'])//2],20,-65,color='#ffce68')
 if direction=='Reverse':label('Wooded dump bypass',{'x':275,'z':-23},0,45);label('No optional Reverse shortcuts',{'x':200,'z':-170},0,0,size=30)
 d.text((120,3525),'TEAL: MAIN ROUTE     GOLD: OPTIONAL FORWARD SHORTCUTS     BLUE: CHECKPOINTS     AMBER: MAJOR JUMPS',font=font(30,True),fill='#d7eeee')
 d.text((120,3600),'Terrain, buildings, water, forest and physical trails are rendered from the saved Unity scene.',font=font(30),fill='#b6d1d4')
 d.text((120,3660),'Use ATLAS.html to switch directions. Forward remains approved; new Reverse gameplay awaits Dan review.',font=font(30),fill='#b6d1d4')
 im.save(out/f'{direction}.png');im.resize((1400,1042)).save(out/f'{direction}-preview.png')
(out/'ATLAS.html').write_text('''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Dan’s Backyard route atlas</title><style>body{margin:0;background:#10262b;color:#e9f5f4;font:16px system-ui}header{padding:20px 28px;position:sticky;top:0;background:#10262bf0}h1{font-size:22px;margin:0 0 12px}button,a{font:inherit;color:inherit}button{border:1px solid #54716e;border-radius:7px;background:#234341;padding:10px 18px;cursor:pointer;margin-right:10px}button[aria-pressed=true]{background:#80e4cb;color:#10262b}img{display:block;width:100%;height:auto}p{margin:12px 0 0;color:#b6d1d4}</style><header><h1>Dan’s Backyard • route atlas</h1><button aria-pressed="false" onclick="select('Forward',this)">Approved Forward + shortcuts</button><button aria-pressed="true" onclick="select('Reverse',this)">Reverse main</button><a id="full" href="Reverse.png" target="_blank">Open full resolution</a><p>Actual saved geometry · north is +Z · coordinate grid in metres · separate views keep the routes readable.</p></header><img id="map" src="Reverse.png" alt="Backyard Reverse main route, jumps, checkpoints and permanent geography"><script>function select(direction,button){document.querySelectorAll('button').forEach(b=>b.setAttribute('aria-pressed',b===button));document.getElementById('map').src=direction+'.png';document.getElementById('map').alt='Backyard '+direction+' actual route atlas';document.getElementById('full').href=direction+'.png'}</script></html>''',encoding='utf-8')
print('Forward/Reverse images and selectable actual-geometry atlas generated.')
