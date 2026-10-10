# The real roadside along the game's stretch: every building within DEPTH m (real) of Hwy 92 whose game position is on the
# game's ground (x -800..800), with its OSM tags and the shop/amenity points inside or beside it.
import json,numpy as np,math
from shapely.geometry import Polygon,Point,LineString
from hwymap import *
DEPTH=230
POIS=[e for e in json.load(open(ROOT+'/SourceArt/Reference/Hwy92/osm-hwy92-pois.json'))['elements'] if e.get('tags')]
for p in POIS:p['xy']=np.array(ll2xy(p['lat'],p['lon']))
def poi_kind(t):
    return t.get('amenity') or t.get('shop') or t.get('office') or t.get('leisure') or ''
def roadside():
    out=[]
    for w in WAYS:
        t=w.get('tags',{})
        if 'building' not in t and t.get('amenity')!='fuel':continue
        p=wxy(w)
        if len(p)<4:continue
        poly=Polygon(p)
        if not poly.is_valid:poly=poly.buffer(0)
        c=np.array(poly.centroid.coords[0]);s,d=project(RCL,RS,c)
        if abs(d)>DEPTH:continue
        g=r2g(c)
        if not(-800<=g[0]<=800):continue
        pk=[poi_kind(q['tags']) for q in POIS if poly.buffer(4).contains(Point(q['xy']))]
        out.append(dict(id=w['id'],tags=t,poly=p,area=poly.area,c=c,s=s,d=d,g=g,pois=pk))
    return out
if __name__=='__main__':
    L=roadside();print(len(L))
    from collections import Counter
    print(Counter(l['tags'].get('building') for l in L))
    for l in sorted(L,key=lambda l:(l['d']>0,l['s'])):
        if l['area']<120 and l['tags'].get('building') in('house','detached','yes') and not l['pois']:continue
        t={k:v for k,v in l['tags'].items() if not k.startswith('addr') and k not in('source','check_date')}
        print(f"{'N' if l['d']>0 else 'S'} g({l['g'][0]:7.1f},{l['g'][1]:6.1f}) d{l['d']:6.0f} area{l['area']:7.0f} {t} {l['pois']}")
