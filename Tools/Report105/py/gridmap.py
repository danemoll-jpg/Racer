# Draws the game's Hwy 92 band from the Unity grid export (2 m cells): ground by height, roads, trees, old businesses, routes.
import sys,json,numpy as np
from PIL import Image,ImageDraw
src=sys.argv[1];scene=sys.argv[2];out=sys.argv[3]
cats=dict(l.rstrip('\n').split('\t') for l in open(f'{src}/cats-{scene}.txt'))
rows=[l.split() for l in open(f'{src}/grid-{scene}.txt')]
H=len(rows);W=len(rows[0]);g=np.full((H,W),np.nan);top=np.full((H,W),np.nan);c=np.zeros((H,W),int)
for i,r in enumerate(rows):
  for j,cell in enumerate(r):
    a,b,k=cell.split(',');g[i,j]=float(a);top[i,j]=float(b);c[i,j]=int(k)
np.savez_compressed(f'{src}/grid-{scene}.npz',g=g,top=top,c=c)
img=np.zeros((H,W,3),np.uint8)
lo,hi=np.nanmin(g),np.nanmax(g)
for i in range(H):
  for j in range(W):
    name=cats[str(c[i,j])]
    if name=='-':col=(0,0,0)
    elif 'trunk' in name:col=(10,70,10)
    elif 'business' in name:col=(220,40,40)
    elif 'CR113' in name or 'Decorative' in name or 'apron' in name:col=(90,90,90)
    elif 'Edge' in name:col=(120,60,160)
    elif 'Memory loop' in name:
      t=(g[i,j]-lo)/(hi-lo+1e-6);col=(int(60+120*t),int(140+80*t),int(60+60*t))
    else:col=(230,200,0)
    img[H-1-i,j]=col
im=Image.fromarray(img).resize((W*2,H*2),Image.NEAREST);d=ImageDraw.Draw(im)
def px(x,z):return ((x+1260)/2*2,(H-1-(z-380)/2)*2)
d0=json.load(open('C:/Users/danmo/Racer/Assets/Resources/WorldMaps/CoursePreviews.json'))
for co in d0['courses']:
  if co['scene'] not in('StreetLoopReverse',):continue
  d.line([px(p['x'],p['z']) for p in co['main']],fill=(0,200,255),width=2)
  for b in co['branches']:d.line([px(p['x'],p['z']) for p in b['points']],fill=(255,140,0),width=2)
pts=[tuple(map(float,l.split())) for l in open(f'{src}/hwy92-StreetLoopReverse.txt')]
d.line([px(p[0],p[2]) for p in pts],fill=(255,255,255),width=1)
for x in range(-1200,1000,100):d.text(px(x,385),str(x),fill=(255,255,255))
for z in range(400,761,50):d.text(px(-1258,z),str(z),fill=(255,255,255))
im.save(out);print(lo,hi,im.size)
