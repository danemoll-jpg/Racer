"""0.86 Part C: shared loaders (height grid, routes, water, obstacles) for the survey scripts; exec'd with sdir set."""
import math,csv,os
import numpy as np
# heights
lines=open(os.path.join(sdir,'map-lap-LakeWoods.csv')).read().splitlines()
hdr=lines[0].split();X0=float(hdr[2]);Z0=float(hdr[4]);CELL=float(hdr[6]);W=int(hdr[8]);H=int(hdr[10])
grid=np.array([[float(v) if v else np.nan for v in l.split(',')] for l in lines[1:]])
def height(x,z):
    fx=(x-X0)/CELL-.5;fz=(z-Z0)/CELL-.5;i=int(math.floor(fx));k=int(math.floor(fz));tx=fx-i;tz=fz-k
    i=max(0,min(W-2,i));k=max(0,min(H-2,k))
    a=grid[k,i];b=grid[k,i+1];c=grid[k+1,i];d=grid[k+1,i+1]
    return (a*(1-tx)+b*tx)*(1-tz)+(c*(1-tx)+d*tx)*tz
# routes
routes={}
for l in open(os.path.join(sdir,'branches-LakeWoods.tsv')):
    name,pts=l.rstrip('\n').split('\t');routes[name]=[tuple(map(float,p.split(','))) for p in pts.split()]
main=routes['Main'];STEP=2.0;LEN=len(main)*STEP
def at(s):i=int(round(s/STEP))%len(main);return main[i]
echo=routes['Echo Cave']
# water
water=[]
for l in list(open(os.path.join(sdir,'water-LakeWoods.tsv')))[1:]:
    p=l.rstrip('\n').split('\t');name=p[0].split('/')[-1];rnd=p[1]=='True';surf=float(p[2]);cx,cz=map(float,p[4].split(','));sx,sz=map(float,p[5].split('x'));rot=math.radians(float(p[6]))
    water.append((name,rnd,surf,cx,cz,sx,sz,rot))
def in_water(x,z):
    for name,rnd,surf,cx,cz,sx,sz,rot in water:
        dx,dz=x-cx,z-cz;lx=dx*math.cos(rot)-dz*math.sin(rot);lz=dx*math.sin(rot)+dz*math.cos(rot)
        if rnd:
            if (lx/(sx/2))**2+(lz/(sz/2))**2<=1:return name
        elif abs(lx)<=sx/2 and abs(lz)<=sz/2:return name
    return None
# obstacles
obs={'trunk':[],'building':[],'prop':[]}
for r in csv.DictReader(open(os.path.join(sdir,'obstacles-LakeWoods.tsv')),delimiter='\t'):
    if r.get('sz') is None or r.get('kind') not in obs:continue
    obs[r['kind']].append((float(r['cx']),float(r['cz']),float(r['sx']),float(r['sz']),r['path']))
mainarr=np.array(main)
def near_main(x,z):
    d=np.hypot(mainarr[:,0]-x,mainarr[:,2]-z);i=int(np.argmin(d));return i*STEP,float(d[i])
