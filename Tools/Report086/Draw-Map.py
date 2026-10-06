"""0.86 Part C (read only): top-down map of LakeWoods from the survey data: hillshade + contours, the main (white, station
labels), Echo Cave (gold), gates (red), water (blue), buildings (black), trunk colliders (dots), and candidate lines.
Usage: python Draw-Map.py <survey dir> <out.png> x0,z0,x1,z1 [contour step] [name=x,z;x,z;...;color ...]"""
import sys,os,math
import numpy as np
import matplotlib;matplotlib.use('Agg')
import matplotlib.pyplot as plt
from matplotlib.colors import LightSource
sdir=sys.argv[1];png=sys.argv[2];bx0,bz0,bx1,bz1=map(float,sys.argv[3].split(','));cstep=float(sys.argv[4]) if len(sys.argv)>4 else 2
exec(open(os.path.join(os.path.dirname(__file__),'survey_common.py')).read())
xs=X0+CELL*(np.arange(W)+.5);zs=Z0+CELL*(np.arange(H)+.5)
mx=(xs>=bx0)&(xs<=bx1);mz=(zs>=bz0)&(zs<=bz1);g=grid[np.ix_(mz,mx)];gx=xs[mx];gz=zs[mz]
gf=np.where(np.isnan(g),np.nanmin(g),g)
span=max(bx1-bx0,bz1-bz0);fig,ax=plt.subplots(figsize=(12*(bx1-bx0)/span+1.5,12*(bz1-bz0)/span+.6),dpi=110)
ls=LightSource(azdeg=315,altdeg=40);rgb=ls.shade(gf,cmap=plt.cm.gist_earth,vert_exag=2,blend_mode='soft',vmin=np.nanmin(g)-8,vmax=np.nanmax(g)+8)
ax.imshow(rgb,extent=(gx[0]-CELL/2,gx[-1]+CELL/2,gz[0]-CELL/2,gz[-1]+CELL/2),origin='lower')
lv=np.arange(math.floor(np.nanmin(g)/cstep)*cstep,np.nanmax(g)+cstep,cstep)
cs=ax.contour(gx,gz,gf,levels=lv,colors='k',linewidths=.35,alpha=.45)
major=[l for l in lv if abs(l/10-round(l/10))<1e-6]
cs2=ax.contour(gx,gz,gf,levels=major,colors='k',linewidths=.8,alpha=.7);ax.clabel(cs2,fmt='%d m',fontsize=7)
for name,rnd,surf,cx,cz,sx,sz,rot in water:
    if rnd:t=np.linspace(0,2*np.pi,90);px=cx+sx/2*np.cos(t);pz=cz+sz/2*np.sin(t)
    else:
        c=[(-.5,-.5),(.5,-.5),(.5,.5),(-.5,.5),(-.5,-.5)];px=[];pz=[]
        for u,v in c:lx,lz=u*sx,v*sz;px.append(cx+lx*math.cos(rot)+lz*math.sin(rot));pz.append(cz-lx*math.sin(rot)+lz*math.cos(rot))
    ax.fill(px,pz,color='#2a7fff',alpha=.75,zorder=3)
if len(obs['trunk']):
    t=np.array([o[:4] for o in obs['trunk']]);sel=(t[:,0]>bx0)&(t[:,0]<bx1)&(t[:,1]>bz0)&(t[:,1]<bz1);ax.scatter(t[sel,0],t[sel,1],s=3,c='#0b3d0b',zorder=4,label='tree trunk (collider)')
for o in obs['building']:
    x,z,sx,sz=o[:4]
    if bx0<x<bx1 and bz0<z<bz1:ax.add_patch(plt.Rectangle((x-sx/2,z-sz/2),sx,sz,fc='#222',ec='w',lw=.5,zorder=5))
m=np.array(main);ax.plot(m[:,0],m[:,2],color='w',lw=3.2,zorder=6,label='main');ax.plot(m[:,0],m[:,2],color='#555',lw=.8,zorder=6)
for s in range(0,int(LEN),50):
    p=at(s)
    if bx0<p[0]<bx1 and bz0<p[2]<bz1:
        ax.plot(p[0],p[2],'o',ms=2.5,color='k',zorder=7)
        if s%100==0:ax.annotate(f'{s}',(p[0],p[2]),xytext=(4,4),textcoords='offset points',fontsize=7,color='k',zorder=8,bbox=dict(boxstyle='round,pad=.1',fc='w',alpha=.7,lw=0))
e=np.array(echo);ax.plot(e[:,0],e[:,2],color='#ffbf00',lw=3,zorder=6,label='Echo Cave shortcut')
gates=[(65,'START'),(260,'CP1'),(850,'CP2'),(1490,'CP3'),(1900,'CP4')]
for s,n in gates:
    p=at(s)
    if bx0<p[0]<bx1 and bz0<p[2]<bz1:ax.plot(p[0],p[2],'s',ms=7,color='red',zorder=9);ax.annotate(n,(p[0],p[2]),xytext=(-14,-13),textcoords='offset points',fontsize=8,color='red',weight='bold',zorder=9)
for spec in sys.argv[5:]:
    name,rest=spec.split('=');parts=rest.split(';');col=parts[-1];lips=[i for i,p in enumerate(parts[:-1]) if p.endswith('!')]
    P=np.array([list(map(float,p.rstrip('!').split(','))) for p in parts[:-1]])
    ax.plot(P[:,0],P[:,1],color=col,lw=3,ls='--',zorder=10,label=name);ax.plot(P[0,0],P[0,1],'o',color=col,ms=8,mec='k',zorder=10);ax.plot(P[-1,0],P[-1,1],'s',color=col,ms=7,mec='k',zorder=10)
    for i in lips:
        q=P[i];ax.plot(q[0],q[1],'^',color='yellow',ms=12,mec='k',zorder=11)
        if i+1<len(P):ax.plot(P[i:i+2,0],P[i:i+2,1],color='yellow',lw=2,ls=':',zorder=11)
ax.set_xlim(bx0,bx1);ax.set_ylim(bz0,bz1);ax.set_xlabel('x (m, east)');ax.set_ylabel('z (m, north)');ax.set_aspect('equal')
ax.plot([],[],'^',color='yellow',mec='k',ms=10,label='jump lip (dotted: flight)');ax.legend(loc='lower left',fontsize=7,framealpha=.85);ax.set_title(os.path.basename(png).replace('.png',''),fontsize=9)
fig.tight_layout();fig.savefig(png);print('wrote',png)
