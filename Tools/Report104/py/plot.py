# The stunt-course plot: the Weatherstone subdivision directly across Hwy 92 from S Cherokee Ln (entered by Weatherstone Place,
# the continuation of S Cherokee Ln). Its outline = the hull of its streets and of the homes within 45 m of them.
import numpy as np,json
from hwymap import *
def hull(P):
    P=sorted(map(tuple,P))
    def cross(o,a,b):return (a[0]-o[0])*(b[1]-o[1])-(a[1]-o[1])*(b[0]-o[0])
    lo=[];up=[]
    for p in P:
        while len(lo)>=2 and cross(lo[-2],lo[-1],p)<=0:lo.pop()
        lo.append(p)
    for p in reversed(P):
        while len(up)>=2 and cross(up[-2],up[-1],p)<=0:up.pop()
        up.append(p)
    return np.array(lo[:-1]+up[:-1])
STREETS=[w for w in WAYS if w.get('tags',{}).get('highway') and w['tags'].get('name','') in ('Weatherstone Place','Weatherstone Drive','Weatherstone Trace','Weatherstone Parkway','Weatherstone Court','Weatherstone Crossing','Weatherstone Circle')]
SP=np.vstack([wxy(w) for w in STREETS])
def near_street(p,r=45):
    for w in STREETS:
        q=wxy(w);d=q[1:]-q[:-1];L=(d**2).sum(1);t=np.clip(((p-q[:-1])*d).sum(1)/np.maximum(L,1e-9),0,1)
        if np.min(np.linalg.norm(q[:-1]+d*t[:,None]-p,axis=1))<r:return True
    return False
homes=[]
for w in WAYS:
    t=w.get('tags',{})
    if t.get('building') in('house','yes','detached','residential','terrace','semidetached_house','apartments'):
        p=wxy(w);c=p.mean(0)
        if c[1]>R_C[1]+15 and c[0]<805 and near_street(c) and not (abs(c[0]-706)<95 and c[1]<300):homes.append(p)
pts=np.vstack([SP[SP[:,1]>R_C[1]+20]]+homes)
from shapely.geometry import Polygon
OFFICE=Polygon([tuple(p) for w in WAYS if w['id']==695831810 for p in wxy(w)])
from shapely.geometry import box
# the office park on the corner (and the strip between it and Weatherstone Place) is not part of the subdivision
POLY=Polygon(hull(pts)).difference(OFFICE.buffer(6).union(box(OFFICE.bounds[0]-6,190,812,OFFICE.bounds[3]+6))).simplify(3)
if POLY.geom_type!='Polygon':POLY=max(POLY.geoms,key=lambda g:g.area)
REAL=np.array(POLY.exterior.coords)[:-1]
def game_outline():return np.array([r2g(p) for p in REAL])
if __name__=='__main__':
    print(len(homes),'homes; real area m2',round(POLY.area));G=game_outline()
    print('real outline',REAL.round(0).tolist());print('game outline',G.round(1).tolist())
    json.dump({'real':REAL.round(2).tolist(),'game':G.round(2).tolist(),'homes':len(homes)},open('plot.json','w'))
