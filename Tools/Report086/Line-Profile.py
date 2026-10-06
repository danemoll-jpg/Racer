"""0.86 Part C (read only): ground profile along a polyline in LakeWoods from the survey height grid, with water, trunk
colliders within 3 m and buildings within 6 m, every STEP m. Usage: python Line-Profile.py <survey dir> step x,z x,z ..."""
import sys,math,csv,os
import numpy as np
sdir=sys.argv[1];step=float(sys.argv[2]);LP=[tuple(map(float,p.split(','))) for p in sys.argv[3:]]
exec(open(os.path.join(os.path.dirname(__file__),'survey_common.py')).read())
d=0;prev=None;out=[]
for (x0,z0),(x1,z1) in zip(LP,LP[1:]):
    L=math.hypot(x1-x0,z1-z0);n=max(1,int(L/step))
    for i in range(n+(1 if (x1,z1)==LP[-1] else 0)):
        t=i/n;x=x0+(x1-x0)*t;z=z0+(z1-z0)*t
        if prev:d+=math.hypot(x-prev[0],z-prev[1])
        prev=(x,z);y=height(x,z);w=in_water(x,z)
        tr=[o for o in obs['trunk'] if math.hypot(o[0]-x,o[1]-z)<3+max(o[2],o[3])/2]
        bu=[o[4].split('/')[-3] if o[4].count('/')>2 else o[4] for o in obs['building'] if math.hypot(o[0]-x,o[1]-z)<6+max(o[2],o[3])/2]
        out.append((d,x,z,y))
        g='' if len(out)<2 else f'{(y-out[-2][3])/max(.01,d-out[-2][0])*100:+5.0f}%'
        ms,md=near_main(x,z)
        print(f'{d:6.1f} m  ({x:6.1f},{z:7.1f})  y {y:6.2f} {g}  main s {ms:.0f} at {md:.0f} m{"  WATER "+w if w else ""}{"  trunks "+str(len(tr)) if tr else ""}{"  near "+",".join(set(bu)) if bu else ""}')
