# Driver's-view stations along the stretch, both directions: the vehicle in the outer lane (4.4 m off the centre to its right).
import numpy as np,math,sys
from hwymap import GCL,GS,at,project
out=[]
for d,name in((1,'east'),(-1,'west')):
    xs=range(-780,781,130) if d>0 else range(780,-781,-130)
    for x in xs:
        s,_=project(GCL,GS,np.array([x,545.0]));p,tan,nrm=at(GCL,GS,s);t=tan*d;right=np.array([t[1],-t[0]])
        q=p+right*4.4;yaw=math.degrees(math.atan2(t[0],t[1]))
        out.append(f"{name}{x:+d},{q[0]:.1f},{q[1]:.1f},{yaw:.1f}")
print(';'.join(out))
