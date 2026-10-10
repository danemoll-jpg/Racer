# Real Hwy 92 (OSM, local metres) <-> the game's Hwy 92 (Unity x/z), anchored at the Trickum Rd and S Cherokee Ln junctions.
# A real point becomes (s along the real centreline, d across it); the game point is the game centreline at the matching s
# (scaled by k between the junctions, the same k beyond them) moved d across, with the real road's half width replaced by the game's.
import json,math,numpy as np
ROOT='C:/Users/danmo/Racer'
LAT0,LON0=34.086,-84.48
def ll2xy(lat,lon):return ((lon-LON0)*111320*math.cos(math.radians(LAT0)),(lat-LAT0)*110574)
def xy2ll(x,y):return (LAT0+y/110574,LON0+x/(111320*math.cos(math.radians(LAT0))))
osm=json.load(open(ROOT+'/SourceArt/Reference/Hwy92/osm-hwy92.json'))
WAYS=[e for e in osm['elements'] if e['type']=='way']
def wxy(w):return np.array([ll2xy(g['lat'],g['lon']) for g in w['geometry']])
HWY=[w for w in WAYS if w.get('tags',{}).get('ref','').startswith('GA 92') and w['tags'].get('highway') in('primary',)]
def centreline_real():
    xs=np.arange(-1840,1841,5.0);out=[]
    segs=[]
    for w in HWY:
        p=wxy(w);segs+=list(zip(p[:-1],p[1:]))
    for x in xs:
        ys=[]
        for a,b in segs:
            if (a[0]-x)*(b[0]-x)<=0 and a[0]!=b[0]:
                t=(x-a[0])/(b[0]-a[0]);ys.append(a[1]+t*(b[1]-a[1]))
        if ys:out.append((x,(min(ys)+max(ys))/2,(max(ys)-min(ys))))
    return np.array(out)
RC=centreline_real()
def smooth(p,n=3):
    q=p.copy()
    for _ in range(n):q[1:-1]=(q[:-2]+2*q[1:-1]+q[2:])/4
    return q
RCL=smooth(RC[:,:2],6)
GC=np.array([[float(v) for v in l.split()] for l in open(ROOT+'/Tools/Report104/py/hwy92-game.txt')])
GCL=GC[:,[0,2]]
def arclen(p):return np.concatenate([[0],np.cumsum(np.linalg.norm(np.diff(p,axis=0),axis=1))])
RS=arclen(RCL);GS=arclen(GCL)
def project(poly,s,pt):
    d=poly[1:]-poly[:-1];L=(d**2).sum(1);t=np.clip(((pt-poly[:-1])*d).sum(1)/np.maximum(L,1e-9),0,1)
    q=poly[:-1]+d*t[:,None];dist=np.linalg.norm(q-pt,axis=1);i=int(np.argmin(dist))
    tan=d[i]/math.sqrt(L[i]);nrm=np.array([-tan[1],tan[0]]) # left of travel (+x travel -> +y/north)
    return s[i]+t[i]*math.sqrt(L[i]),float(np.dot(pt-q[i],nrm))
def at(poly,s,sv):
    sv=min(max(sv,s[0]),s[-1]);i=min(np.searchsorted(s,sv)-1,len(poly)-2);i=max(i,0)
    t=(sv-s[i])/max(s[i+1]-s[i],1e-9);p=poly[i]+(poly[i+1]-poly[i])*t;tan=(poly[i+1]-poly[i])/np.linalg.norm(poly[i+1]-poly[i])
    return p,tan,np.array([-tan[1],tan[0]])
# junctions: real from OSM shared nodes; game where the Street Loop route on Trickum Rd / S Cherokee Ln meets the centreline
R_T=np.array(ll2xy(*[(g['lat'],g['lon']) for w in WAYS for n,g in zip(w['nodes'],w['geometry']) if n==67300077][0]))
R_C=np.array(ll2xy(*[(g['lat'],g['lon']) for w in WAYS for n,g in zip(w['nodes'],w['geometry']) if n==67331769][0]))
G_T=np.array([-606.0,541.0]);G_C=np.array([322.0,557.5])
rsT,_=project(RCL,RS,R_T);rsC,_=project(RCL,RS,R_C);gsT,_=project(GCL,GS,G_T);gsC,_=project(GCL,GS,G_C)
K=(gsC-gsT)/(rsC-rsT)
REAL_HALF=None  # set below from the carriageway spread
GAME_HALF=8.2
def real_half_at(x):
    i=int(np.clip(np.searchsorted(RC[:,0],x),0,len(RC)-1));return max(RC[i,2]/2+5.5,9.0)  # carriageway centres +- half a carriageway (~5.5 m)
def r2g(pt):
    pt=np.asarray(pt,float);s,d=project(RCL,RS,pt);rh=real_half_at(pt[0])
    gs=gsT+(s-rsT)*K;p,tan,nrm=at(GCL,GS,gs)
    if abs(d)<=rh:dg=d*GAME_HALF/rh
    else:dg=math.copysign(GAME_HALF+(abs(d)-rh)*K,d)
    return p+nrm*dg
def g2r(pt):
    pt=np.asarray(pt,float);gs,dg=project(GCL,GS,pt);s=rsT+(gs-gsT)/K;p,tan,nrm=at(RCL,RS,s);rh=real_half_at(p[0])
    if abs(dg)<=GAME_HALF:d=dg*rh/GAME_HALF
    else:d=math.copysign(rh+(abs(dg)-GAME_HALF)/K,dg)
    return p+nrm*d
def heading_r2g(pt,ang):  # a real direction (radians, x east y north) to the game frame (x,z)
    a=np.asarray(pt,float);b=a+np.array([math.cos(ang),math.sin(ang)])*5
    ga,gb=r2g(a),r2g(b);return math.atan2(gb[1]-ga[1],gb[0]-ga[0])
if __name__=='__main__':
    print('real junctions',R_T,R_C,'dist',np.linalg.norm(R_C-R_T),'along',rsC-rsT)
    print('game junctions along',gsC-gsT,'K',K)
    print('real centre width samples',[(int(x),round(w,1)) for x,_,w in RC[::40]])
    for nm,pt in (('T',R_T),('C',R_C)):print(nm,r2g(pt))
