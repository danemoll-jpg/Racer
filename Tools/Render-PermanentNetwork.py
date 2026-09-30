"""Draw only exported permanent road/trail geometry over the registered scene composite."""
import json,math,os
from pathlib import Path
from PIL import Image,ImageDraw
r=Path(__file__).resolve().parents[1];out=r/'Docs/WorldMap';Image.MAX_IMAGE_PIXELS=160_000_000
def save(image,path):
 temp=path.with_name(path.name+'.render-tmp');image.save(temp,format='PNG');os.replace(temp,path)
b=json.loads((out/'MountainLoop.json').read_text())['bounds'];image=Image.open(out/'WORLD_BASE.png').convert('RGB');draw=ImageDraw.Draw(image)
data=json.loads((r/'Docs/WorldCleanup/audit.json').read_text());seen=set();segments=[]
def xy(p):return ((p['x']-b['x'])/b['width']*image.width,(b['y']+b['height']-p['z'])/b['height']*image.height)
def add(points,kind,title,closed=False):
 pairs=list(zip(points,points[1:]+points[:1] if closed else points[1:]))
 for a,c in pairs:
  if math.dist((a['x'],a['z']),(c['x'],c['z']))<.05:continue
  key=tuple(sorted(((round(a['x'],1),round(a['z'],1)),(round(c['x'],1),round(c['z'],1)))))
  if key in seen:continue
  seen.add(key);segments.append((a,c,kind,title))
for scene in data:
 for road in scene['roads']:
  # Open driveways and spurs must never be joined end-to-start across terrain.
  closed=road['name'] in ('Phase 3 - Race Systems','Reverse main route','Forest race route','Mountain racing line','CR117 mandatory two-flight racing line','Forward navigation only - no road mesh')
  kind='dirt' if road['forestTrail'] or any(s in road['name'].lower() for s in ('driveway','trail','summit','shore','navigation')) else 'paved'
  add(road['points'],kind,road['name'],closed)
 for branch in scene['branches']:add(branch['points'],'shortcut',branch['title'])
for a,c,kind,title in sorted(segments,key=lambda s:s[2]!='paved'):
 width=12 if kind=='paved' else 9;color='#bdb8a8' if kind=='paved' else '#b99d70'
 draw.line((xy(a),xy(c)),fill='#374139',width=width+3)
 draw.line((xy(a),xy(c)),fill=color,width=width)
save(image,out/'PERMANENT_NETWORK.png')
save(image.resize((5200,round(image.height*5200/image.width)),Image.Resampling.LANCZOS),r/'Assets/Resources/WorldMaps/PermanentWorld.png')
# The atlas uses the same permanent network and corrected landmarks as the menu.
atlas=Image.open(out/'WORLD_MAP.png').convert('RGB');atlas.paste(image,(150,240))
# Preserve the coordinate grid/landmark layer by drawing the new network on the original atlas instead.
atlas=Image.open(out/'WORLD_MAP.png').convert('RGB');d=ImageDraw.Draw(atlas)
for a,c,kind,title in sorted(segments,key=lambda s:s[2]!='paved'):
 pts=[(xy(p)[0]+150,xy(p)[1]+240) for p in (a,c)];d.line(pts,fill='#bdb8a8' if kind=='paved' else '#b99d70',width=4 if kind=='paved' else 3)
save(atlas,out/'WORLD_MAP.png');save(atlas.resize((1500,round(atlas.height*1500/atlas.width))),out/'WORLD_MAP-preview.png')
(r/'Docs/WorldCleanup/network-inventory.json').write_text(json.dumps({'bounds':b,'segments':len(segments),'sources':sorted(set(s[3] for s in segments))},indent=2))
print(f'{len(segments)} unique physical network segments; shared menu base 5200px.')
