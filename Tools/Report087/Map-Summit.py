"""0.87 Part B: top-down map of the Summit Climb hillside: heights, main, gates, trunk colliders, drawn pieces, optional line.
Usage: python Map-Summit.py <survey dir> <out.png> [line file: x,z per line]"""
import sys,os
import numpy as np,matplotlib;matplotlib.use('Agg');import matplotlib.pyplot as plt
sdir=sys.argv[1];exec(open(os.path.join(os.path.dirname(__file__),'summit_common.py')).read())
fig,ax=plt.subplots(figsize=(12,16))
ext=[X0,X0+W,Z0,Z0+H];ax.imshow(grid,origin='lower',extent=ext,cmap='terrain',alpha=.8)
cs=ax.contour(np.arange(W)+X0+.5,np.arange(H)+Z0+.5,grid,levels=range(20,100,2),colors='k',linewidths=.3);ax.clabel(cs,cs.levels[::5],fontsize=6)
m=mainarr[(mainarr[:,0]>1700)&(mainarr[:,0]<2200)];ax.plot(m[:,1],m[:,3],'w-',lw=2)
for r in m[::10]:ax.annotate(f"{r[0]:.0f}",(r[1],r[3]),fontsize=7,color='w')
for g in gates:ax.plot(g[1],g[3],'rs')
for t in trunks:ax.add_patch(plt.Circle((t[0],t[1]),max(t[2],.3),color='k' if 'off' not in t[5] else 'grey'))
d=np.array([(x,z,sx) for x,z,sx,sy,by,r,v in drawn if sx>1.5]);ax.scatter(d[:,0],d[:,1],s=d[:,2]*3,facecolors='none',edgecolors='g',lw=.5)
if len(sys.argv)>3:
    for f in sys.argv[3:]:
        p=np.array([list(map(float,l.split(',')[:2])) for l in open(f) if l.strip()]);ax.plot(p[:,0],p[:,1],'m-',lw=1.5)
ax.set_xlim(490,625);ax.set_ylim(-275,-35);ax.set_aspect('equal');ax.grid(alpha=.3)
plt.savefig(sys.argv[2],dpi=110,bbox_inches='tight')
