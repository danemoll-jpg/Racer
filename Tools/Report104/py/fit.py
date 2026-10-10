# How well the real roads fit the game's: the real Trickum Rd and S Cherokee Ln (first 300 m south of Hwy 92) mapped into the game
# against the Street Loop route on those roads (the route keeps to one lane, about 2-3 m off the road's centre).
import json,numpy as np,math
from hwymap import *
co=[c for c in json.load(open(ROOT+'/Assets/Resources/WorldMaps/CoursePreviews.json'))['courses'] if c['scene']=='StreetLoopGreybox'][0]
m=np.array([[p['x'],p['z']] for p in co['main']])
out=[]
def dist_to(poly,p):
    d=poly[1:]-poly[:-1];L=(d**2).sum(1);t=np.clip(((p-poly[:-1])*d).sum(1)/np.maximum(L,1e-9),0,1);return np.min(np.linalg.norm(poly[:-1]+d*t[:,None]-p,axis=1))
for nm,ids,anchor in(('Trickum Rd',[132057972,400647815,400647839,695831807,1062084869],R_T),('S Cherokee Ln',[9105236,1082917501],R_C)):
    pts=np.vstack([wxy(w) for w in WAYS if w['id'] in ids])
    pts=pts[(np.linalg.norm(pts-anchor,axis=1)<390)&(pts[:,1]<anchor[1]+5)]
    gm=np.array([r2g(p) for p in pts]);route=m[(m[:,1]<540)&(m[:,1]>200)&(abs(m[:,0]-gm[:,0].mean())<120)]
    ds=[dist_to(route,p) for p in gm if p[1]>360]
    out.append(f'{nm}: {len(ds)} real points 25-300 m south of Hwy 92 mapped into the game: distance to the Street Loop route on that road mean {np.mean(ds):.1f} m, max {np.max(ds):.1f} m')
gj=[r2g(R_T),r2g(R_C)]
out.append(f'junctions: real Trickum {R_T.round(1)} -> game {gj[0].round(1)} (anchor {G_T}); real S Cherokee {R_C.round(1)} -> game {gj[1].round(1)} (anchor {G_C})')
out.append(f'scale along Hwy 92 between the junctions: game {gsC-gsT:.1f} m / real {rsC-rsT:.1f} m = {K:.3f}')
print('\n'.join(out));open('fit.txt','w').write('\n'.join(out)+'\n')
