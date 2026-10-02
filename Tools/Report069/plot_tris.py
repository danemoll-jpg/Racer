# Top-down plot of exported triangles (Report069Mesh): pavement brown, other meshes by name, pavement boundary edges red.
import sys,collections
from PIL import Image,ImageDraw
src,dst=sys.argv[1],sys.argv[2]; S=int(sys.argv[3]) if len(sys.argv)>3 else 30
rows=[l.strip().split(';') for l in open(src) if l.strip()]
tris=[(r[0],[tuple(map(float,p.split(','))) for p in r[1:]]) for r in rows]
xs=[p[0] for _,t in tris for p in t]; zs=[p[2] for _,t in tris for p in t]
cx=(min(xs)+max(xs))/2; cz=(min(zs)+max(zs))/2; R=max(max(xs)-min(xs),max(zs)-min(zs))/2
W=int(2*R*S)+20; img=Image.new('RGB',(W,W),(255,255,255)); d=ImageDraw.Draw(img)
def P(p): return (10+(p[0]-cx+R)*S, 10+(cz+R-p[2])*S)
cols={'driving surface':(170,120,80),'edge shoulders':(90,160,90),'edge seams':(160,200,120),'continuous solid':(60,110,60),'earth banks':(120,140,70)}
order=sorted(tris,key=lambda t:('driving surface' in t[0]))
for n,t in order:
    if 'driving surface' in n: continue
    c=next((v for k,v in cols.items() if k in n),(150,150,150)); d.polygon([P(p) for p in t],fill=c,outline=None)
for n,t in tris:
    if 'driving surface' in n: d.polygon([P(p) for p in t],fill=(170,120,80),outline=(150,100,60))
ec=collections.Counter()
key=lambda p:(round(p[0],2),round(p[2],2))
for n,t in tris:
    if 'driving surface' not in n: continue
    for i in range(3):
        a,b=key(t[i]),key(t[(i+1)%3]); ec[tuple(sorted((a,b)))]+=1
for (a,b),k in ec.items():
    if k==1: d.line([P((a[0],0,a[1])),P((b[0],0,b[1]))],fill=(255,0,0),width=2)
for i in range(-int(R),int(R)+1):
    if i%5==0:
        d.text(P((cx+i,0,cz+R-0.5)),f"{cx+i:.0f}",fill=(0,0,0)); d.text(P((cx-R,0,cz+i)),f"{cz+i:.0f}",fill=(0,0,0))
img.save(dst)
