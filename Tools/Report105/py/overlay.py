import json,numpy as np,sys
from PIL import Image,ImageDraw
from hwymap import *
src=sys.argv[1]
z=np.load(f'{src}/grid-StreetLoopReverse.npz');g=z['g'];H,W=g.shape
base=Image.open(f'{src}/map-SLR.png').convert('RGB');d=ImageDraw.Draw(base,'RGBA')
def px(p):return ((p[0]+1260),(H-1-(p[1]-380)/2)*2)
n=0
for w in WAYS:
    t=w.get('tags',{});p=wxy(w)
    if len(p)<2 or abs(p[:,0].mean())>2000:continue
    s,dd=project(RCL,RS,p.mean(0))
    if abs(dd)>330:continue
    gp=[px(r2g(q)) for q in p]
    if 'building' in t:d.polygon(gp,fill=(255,255,255,200),outline=(0,0,0));n+=1
    elif t.get('amenity')=='parking':d.polygon(gp,fill=(60,60,200,110))
    elif t.get('landuse') in('residential',):d.polygon(gp,outline=(255,0,255))
    elif t.get('landuse') in('commercial','retail'):d.polygon(gp,outline=(255,255,0))
    elif t.get('highway') in('service',):d.line(gp,fill=(40,40,40),width=2)
    elif t.get('highway') in('residential','tertiary','primary_link','unclassified'):d.line(gp,fill=(255,255,255),width=3)
for e in osm['elements']:
    if e['type']=='node' and e.get('tags',{}).get('highway')=='traffic_signals':
        q=r2g(ll2xy(e['lat'],e['lon']));x,y=px(q);d.ellipse((x-4,y-4,x+4,y+4),fill=(255,0,0))
base.save(f'{src}/overlay.png');print(n)
w=base.size[0];base.crop((400,0,1300,base.size[1])).save(f'{src}/ovA.png');base.crop((1250,0,2100,base.size[1])).save(f'{src}/ovB.png')
