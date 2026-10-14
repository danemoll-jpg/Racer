import sys,collections,heapq,os,math
import numpy as np
x0,z0=440,-150
name=sys.argv[1]; STEP=float(os.environ.get('STEP','0.45')); MINN=float(os.environ.get('MINN','0.87'))
nodes={}
for l in open(f'grid/grid-{name}.txt',encoding='utf-8'):
    p=l.rstrip('\n').split('|'); x,z=map(float,p[0].split()); nodes[(int(round(x-x0)),int(round(z-z0)))]=[(float(q.split(' ',2)[0]),float(q.split(' ',2)[1]),q.split(' ',2)[2]) for q in p[1:]]
def top(ix,iz):
    h=nodes.get((ix,iz)); return h[0] if h else None
def good(ix,iz):
    h=top(ix,iz)
    if not h or h[1]<MINN or 'trunk' in h[2].lower() or 'architecture' in h[2]: return False
    if (x0+ix-642)**2+(z0+iz+20)**2<67**2 and h[0]<78.05: return False
    return True
W,H=351,261
ok=np.zeros((W,H),bool)
for ix in range(W):
    for iz in range(H): ok[ix,iz]=good(ix,iz)
# a cell is passable if good and its step to each good neighbour is small; clearance = distance to the nearest bad cell or big step
bad=~ok.copy()
for ix in range(W-1):
    for iz in range(H-1):
        if ok[ix,iz] and ok[ix+1,iz] and abs(top(ix,iz)[0]-top(ix+1,iz)[0])>STEP: bad[ix,iz]=bad[ix+1,iz]=True
        if ok[ix,iz] and ok[ix,iz+1] and abs(top(ix,iz)[0]-top(ix,iz+1)[0])>STEP: bad[ix,iz]=bad[ix,iz+1]=True
from scipy import ndimage
clear=ndimage.distance_transform_edt(~bad)
a=tuple(int(round(v)) for v in (float(sys.argv[2].split(',')[0])-x0,float(sys.argv[2].split(',')[1])-z0)); b=tuple(int(round(v)) for v in (float(sys.argv[3].split(',')[0])-x0,float(sys.argv[3].split(',')[1])-z0))
dist={a:0};prev={};pq=[(0,a)]
while pq:
    d,u=heapq.heappop(pq)
    if u==b: break
    if d>dist[u]: continue
    for dx,dz in ((1,0),(-1,0),(0,1),(0,-1),(1,1),(1,-1),(-1,1),(-1,-1)):
        v=(u[0]+dx,u[1]+dz)
        if not(0<=v[0]<W and 0<=v[1]<H) or bad[v]: continue
        c=math.hypot(dx,dz)*(1+40/max(.5,clear[v])**2); nd=d+c
        if nd<dist.get(v,1e18): dist[v]=nd; prev[v]=u; heapq.heappush(pq,(nd,v))
if b not in prev: print('no path'); sys.exit()
path=[b]
while path[-1]!=a: path.append(prev[path[-1]])
path=path[::-1]
print(name,'widest path',len(path),'cells; min clearance (m, distance to an edge)',min(clear[p] for p in path))
for p in path[::6]: print(f'   ({x0+p[0]},{z0+p[1]}) y {top(*p)[0]:.1f} clearance {clear[p]:.1f}')
import json
pts=[(x0+p[0],top(*p)[0],z0+p[1]) for p in path]
# smooth: moving average over 9 cells, then keep every 2nd
sm=[]
for i in range(len(pts)):
    a=max(0,i-4); b=min(len(pts),i+5); seg=pts[a:b]; sm.append(tuple(sum(q[k] for q in seg)/len(seg) for k in range(3)))
out=sys.argv[4] if len(sys.argv)>4 else None
if out: json.dump({'points':sm[::2]+[sm[-1]],'clearance':[float(clear[p]) for p in path][::2]},open(out,'w'))
