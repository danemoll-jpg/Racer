"""0.73 Part A: the frozen-water dusting texture (tileable, generated; no external asset).
Pale blue-white ice with soft drifts of snow and a few faint lighter streaks. Written to Assets/Resources/WaterIceFrost.png."""
import numpy as np, pathlib
from PIL import Image
N=256; rng=np.random.default_rng(7301)
def noise(cells):
    g=rng.random((cells,cells)); x=np.linspace(0,cells,N,endpoint=False); i=np.floor(x).astype(int); f=x-i; f=f*f*(3-2*f)
    a=g[np.ix_(i,i)]; b=g[np.ix_(i,(i+1)%cells)]; c=g[np.ix_((i+1)%cells,i)]; d=g[np.ix_((i+1)%cells,(i+1)%cells)]
    fy=f[:,None]; fx=f[None,:]; return (a*(1-fx)+b*fx)*(1-fy)+(c*(1-fx)+d*fx)*fy
n=noise(4)*.5+noise(8)*.27+noise(16)*.15+noise(32)*.08
dust=np.clip((n-.48)/.22,0,1); dust=dust*dust*(3-2*dust)
speck=(noise(64)>.82)*.25
ice=np.array([.80,.89,.96]); snow=np.array([.97,.98,1.0])
img=ice[None,None,:]*(1-dust[...,None])+snow[None,None,:]*dust[...,None]
img=np.clip(img+speck[...,None]*.08,0,1)
out=pathlib.Path(__file__).resolve().parents[2]/'Assets/Resources/WaterIceFrost.png'
Image.fromarray((img*255).astype(np.uint8)).save(out); print(out)
