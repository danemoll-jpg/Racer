# Top-down shot of the stretch (stitched from the player's tiles, 2 px per m) with the stunt plot outlined, above the NAIP reference of the
# same real stretch (mapped through the junction fit, so the two line up road for road). Usage: topdown.py <tiles dir> <out.jpg> [label]
import sys,json,numpy as np
from PIL import Image,ImageDraw,ImageFont
from hwymap import *
from naip import IMG,xy2px
src,out=sys.argv[1],sys.argv[2];label=sys.argv[3] if len(sys.argv)>3 else ''
W=3200;H=940;game=Image.new('RGB',(W,H))
for i,x in enumerate(range(-800,800,200)):game.paste(Image.open(f'{src}/top-{x+800:04d}.png').convert('RGB'),(i*400,0))
d=ImageDraw.Draw(game)
plot=json.load(open(ROOT+'/Assets/Resources/Hwy92/Hwy92Roadside.json'))['plot']
pts=[((plot[i]+800)*2,(850-plot[i+1])*2) for i in range(0,len(plot),2)]
if 'noplot' not in sys.argv:d.line(pts+[pts[0]],fill=(255,230,0),width=5);d.text((pts[0][0]+60,pts[0][1]+60),'stunt-course plot (left empty)',fill=(255,230,0))
for x in range(-800,801,100):d.text(((x+800)*2+3,H-14),str(x),fill=(255,255,255))
d.text((8,8),label,fill=(255,255,255))
# the reference: every game pixel's real place sampled from the NAIP image
ref=Image.new('RGB',(W//2,H//2));a=np.asarray(IMG);o=np.zeros((H//2,W//2,3),np.uint8)
for j in range(H//2):
    zz=850-(j+.5)
    for i in range(W//2):
        xx=-800+(i+.5);r=g2r((xx,zz));px,py=xy2px(*r)
        if 0<=int(py)<a.shape[0] and 0<=int(px)<a.shape[1]:o[j,i]=a[int(py),int(px)]
ref=Image.fromarray(o).resize((W,H));dr=ImageDraw.Draw(ref)
if 'noplot' not in sys.argv:dr.line(pts+[pts[0]],fill=(255,230,0),width=5)
dr.text((8,8),'USGS NAIP aerial of the real stretch, mapped to the game (placement reference only, never shown in the game)',fill=(255,255,255))
c=Image.new('RGB',(W,2*H+10));c.paste(game,(0,0));c.paste(ref,(0,H+10));c.save(out,quality=88);print(out,c.size)
